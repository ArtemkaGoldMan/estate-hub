using CSharpFunctionalExtensions;
using EstateHub.Authorization.Domain.DTO.Authentication.Requests;
using EstateHub.Authorization.Domain.DTO.Authentication.Requests;
using EstateHub.Authorization.Domain.DTO.Authentication.Responses;
using EstateHub.Authorization.Domain.DTO.Session;
using EstateHub.Authorization.Domain.DTO.User;
using EstateHub.Authorization.Domain.Errors;
using EstateHub.Authorization.Domain.Interfaces.CoreInterfaces;
using EstateHub.Authorization.Domain.Interfaces.DataAccessInterfaces;
using EstateHub.Authorization.Domain.Interfaces.InfrastructureInterfaces;
using EstateHub.Authorization.Domain.Models;
using EstateHub.Authorization.Domain.Options;
using EstateHub.Authorization.Core.Helpers;
using EstateHub.SharedKernel;
using EstateHub.SharedKernel.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EstateHub.Authorization.Core.Services.Authentication;

/// <summary>
/// Service responsible for user authentication operations including login, registration,
/// password management, email confirmation, and session management.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly JWTOptions _options;
    private readonly IUsersRepository _usersRepository;
    private readonly IEmailSmtpService _emailSmtpService;
    private readonly SmtpOptions _smtpOptions;
    private readonly ISessionsRepository _sessionsRepository;
    private readonly IIdentityService _identityService;
    private readonly ResultExecutor<AuthenticationService> _resultExecutor;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging operations.</param>
    /// <param name="options">JWT configuration options.</param>
    /// <param name="smtpOptions">SMTP configuration options for email services.</param>
    /// <param name="unitOfWork">Unit of work for transaction management.</param>
    /// <param name="usersRepository">Repository for user data access.</param>
    /// <param name="sessionsRepository">Repository for session data access.</param>
    /// <param name="emailSmtpService">Service for sending emails.</param>
    /// <param name="identityService">Service for ASP.NET Identity operations.</param>
    public AuthenticationService(
        ILogger<AuthenticationService> logger,
        IOptions<JWTOptions> options,
        IOptions<SmtpOptions> smtpOptions,
        IUnitOfWork unitOfWork,
        IUsersRepository usersRepository,
        ISessionsRepository sessionsRepository,
        IEmailSmtpService emailSmtpService,
        IIdentityService identityService)
    {
        _logger = logger;
        _options = options.Value;
        _smtpOptions = smtpOptions.Value;
        _sessionsRepository = sessionsRepository;
        _emailSmtpService = emailSmtpService;
        _usersRepository = usersRepository;
        _identityService = identityService;
        _resultExecutor = new ResultExecutor<AuthenticationService>(_logger, unitOfWork);
    }

    /// <summary>
    /// Authenticates a user with email and password credentials.
    /// </summary>
    /// <param name="request">Login request containing email and password.</param>
    /// <param name="beginTransaction">Whether to begin a database transaction. Default is true.</param>
    /// <returns>
    /// A result containing <see cref="AuthenticationResult"/> with access token, refresh token, and user information if successful.
    /// Returns failure if user not found, account is deleted, or password is incorrect.
    /// </returns>
    public Task<Result<AuthenticationResult>> LoginAsync(LoginRequest request, bool beginTransaction = true)
    {
        return _resultExecutor.ExecuteWithTransactionAsync(
            async () =>
            {
                var userResult = await _usersRepository.GetByEmailAsync<UserWithRolesDto>(request.Email, true);

                if (userResult is null)
                {
                    _logger.LogError("{error}", UserErrors.NotFoundByEmail(request.Email).ToString());
                    ErrorHelper.ThrowErrorNull(AuthorizationErrors.IncorrectPasswordOrUsername());
                }

                if (userResult.IsDeleted)
                {
                    _logger.LogWarning("{error}", UserErrors.UserIsDeleted(userResult.Email).ToString());
                    ErrorHelper.ThrowError(UserErrors.UserIsDeleted(userResult.Email));
                }

                await _identityService.CheckPasswordAsync(userResult.Id, request.Password);

                return await CreateUserSessionAsync(userResult);
            }, beginTransaction);
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">Registration request containing email, password, and callback URL for email confirmation.</param>
    /// <returns>
    /// If email confirmation is required: Returns null and sends confirmation email.
    /// If email confirmation is disabled: Returns <see cref="AuthenticationResult"/> with tokens (auto-login).
    /// Returns failure if email already exists or validation fails.
    /// </returns>
    public Task<Result<AuthenticationResult?>> RegisterAsync(UserRegistrationRequest request)
    {
        return _resultExecutor.ExecuteWithTransactionAsync<AuthenticationResult?>(async () =>
        {
            var user = await _usersRepository.GetByEmailAsync<UserDto>(request.Email, true);
            if (user is not null)
            {
                if (user.IsDeleted)
                {
                    _logger.LogError("{error}", UserErrors.UserIsDeleted(request.Email).ToString());
                    ErrorHelper.ThrowError(UserErrors.UserIsDeleted(request.Email));
                }

                _logger.LogError("{error}", UserErrors.EmailNotUnique().ToString());
                ErrorHelper.ThrowError(UserErrors.EmailNotUnique());
            }

            var userValidationResult = User.Create(request.Email, string.Empty, string.Empty, request.Password);

            if (userValidationResult.IsFailure)
            {
                _logger.LogError("{error}", userValidationResult.Error);
                // Parse error from Result string format
                var errorParts = userValidationResult.Error.Split(TextDelimiters.Separator);
                if (errorParts.Length >= 4)
                {
                    var error = new Error(errorParts[0], errorParts[1], errorParts[2], errorParts[3],
                        errorParts.Length > 4 ? errorParts[4] : null);
                    ErrorHelper.ThrowError(error);
                }
                ErrorHelper.ThrowError(UserErrors.InvalidPassword());
            }

            var result = await _identityService.RegisterAsync(userValidationResult.Value);

            if (!result.RequireConfirmedAccount)
            {
                var loginResult = await LoginAsync(
                    new LoginRequest { Email = request.Email, Password = request.Password },
                    beginTransaction: false);

                if (loginResult.IsFailure)
                {
                    var error = loginResult.GetErrorObject();
                    ErrorHelper.ThrowError(error);
                }

                return loginResult.Value;
            }

            if (string.IsNullOrWhiteSpace(request.CallbackUrl))
            {
                _logger.LogError("{error}", AuthorizationErrors.CallbackIsNull().ToString());
                ErrorHelper.ThrowErrorNull(AuthorizationErrors.CallbackIsNull());
            }

            var sendResult = await _emailSmtpService
                .SendEmailConfirmationAsync(_smtpOptions, result.Email, result.GeneratedEmailConfirmationToken,
                    request.CallbackUrl, result.Id);

            if (sendResult.IsFailure)
            {
                var error = sendResult.GetErrorObject();
                ErrorHelper.ThrowErrorOperation(error);
            }

            return null;
        });
    }

    /// <summary>
    /// Confirms a user's email address using the token received via email.
    /// </summary>
    /// <param name="request">Email confirmation request containing user ID and confirmation token.</param>
    /// <returns>
    /// A result containing <see cref="AuthenticationResult"/> with access token, refresh token, and user information if successful.
    /// Returns failure if user not found or token is invalid.
    /// </returns>
    public Task<Result<AuthenticationResult>> ConfirmEmailAsync(ConfirmEmailRequest request)
    {
        return _resultExecutor.ExecuteWithTransactionAsync(async () =>
        {
            var userResult =
                await _usersRepository.GetByIdAsync<UserWithRolesDto>(request.UserId);

            if (userResult is null)
            {
                _logger.LogError("{error}", UserErrors.NotFoundById(request.UserId).ToString());
                ErrorHelper.ThrowError(UserErrors.NotFoundById(request.UserId));
            }

            await _identityService.ConfirmEmailAsync(request.UserId, request.Token);
            return await CreateUserSessionAsync(userResult);
        });
    }

    /// <summary>
    /// Initiates account state management (recovery or hard delete) for deleted accounts.
    /// Sends an email with a confirmation token to the user's email address.
    /// </summary>
    /// <param name="request">Account management request containing email, action type, and return URL.</param>
    /// <returns>
    /// A result indicating success if the email was sent successfully.
    /// Returns failure if user not found, user is not deleted, or email sending fails.
    /// </returns>
    public Task<Result> ManageAccountState(ManageAccountRequest request)
    {
        return _resultExecutor.ExecuteAsync(async () =>
        {
            var userResult = await _usersRepository.GetByEmailAsync<UserDto>(request.Email, true);

            if (userResult is null || !userResult.IsDeleted)
            {
                var error = userResult != null 
                    ? UserErrors.NotFoundByEmail(userResult.Email) 
                    : UserErrors.NotFoundByEmail(request.Email);
                _logger.LogError("{error}", error.ToString());
                ErrorHelper.ThrowErrorNull(error);
            }

            var token = await _identityService.GenerateAccountActionToken(userResult.Id, request.ActionType);

            var emailResult =
                await _emailSmtpService.SendAccountActionToken(_smtpOptions, userResult.Email, token,
                    request.ReturnUrl, request.ActionType, userResult.Id);

            if (emailResult.IsFailure)
            {
                var error = emailResult.GetErrorObject();
                ErrorHelper.ThrowError(error);
            }
        });
    }

    /// <summary>
    /// Confirms an account action (recovery or hard delete) using the token received via email.
    /// </summary>
    /// <param name="request">Account action confirmation request containing user ID, token, and action type.</param>
    /// <returns>
    /// A result indicating success if the action was confirmed successfully.
    /// Returns failure if user not found, user is not deleted, or token is invalid.
    /// </returns>
    public Task<Result> ConfirmAccountAction(ConfirmAccountActionRequest request)
    {
        return _resultExecutor.ExecuteAsync(async () =>
        {
            var userResult =
                await _usersRepository.GetByIdAsync<UserDto>(request.UserId, true);

            if (userResult is null || !userResult.IsDeleted)
            {
                _logger.LogError("{error}", UserErrors.NotFoundById(request.UserId).ToString());
                ErrorHelper.ThrowErrorNull(UserErrors.NotFoundById(request.UserId));
            }

            await _identityService.ConfirmAccountAction(request.UserId, request.Token, request.ActionType);
        });
    }

    /// <summary>
    /// Initiates the password reset process by sending a password reset token to the user's email.
    /// </summary>
    /// <param name="request">Password reset request containing email and return URL.</param>
    /// <returns>
    /// A result indicating success if the email was sent (even if user doesn't exist, for security).
    /// Returns failure if email format is invalid.
    /// </returns>
    public Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        return _resultExecutor.ExecuteAsync(async () =>
        {
            var userResult = await _usersRepository.GetByEmailAsync<UserWithRolesDto>(request.Email);

            if (userResult is null)
            {
                _logger.LogError("{error}", UserErrors.NotFoundByEmail(request.Email).ToString());
                ErrorHelper.ThrowErrorNull(AuthorizationErrors.IncorrectPasswordOrUsername());
            }

            var token = await _identityService.GeneratePasswordResetTokenAsync(userResult.Id);

            var emailResult =
                await _emailSmtpService.SendForgetPasswordToken(_smtpOptions, userResult.Email, token,
                    request.ReturnUrl, userResult.Id);

            if (emailResult.IsFailure)
            {
                var error = emailResult.GetErrorObject();
                ErrorHelper.ThrowError(error);
            }
        });
    }

    /// <summary>
    /// Resets a user's password using the token received via email.
    /// Invalidates all existing sessions for the user after successful password reset.
    /// </summary>
    /// <param name="request">Password reset request containing user ID, token, and new password.</param>
    /// <returns>
    /// A result indicating success if the password was reset successfully.
    /// Returns failure if user not found, token is invalid/expired, or password validation fails.
    /// </returns>
    public Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
    {
        return _resultExecutor.ExecuteWithTransactionAsync(async () =>
        {
            var userResult =
                await _usersRepository.GetByIdAsync<UserDto>(request.UserId);

            if (userResult is null)
            {
                _logger.LogError("{error}", UserErrors.NotFoundById(request.UserId).ToString());
                ErrorHelper.ThrowErrorNull(UserErrors.NotFoundById(request.UserId));
            }

            await _identityService.ResetPasswordAsync(request.UserId, request.Token, request.Password);
            await _sessionsRepository.DeleteByUserIdAsync(userResult.Id);
        });
    }

    /// <summary>
    /// Refreshes an access token using a valid refresh token.
    /// Validates the refresh token, checks session expiration, and generates a new access token.
    /// </summary>
    /// <param name="refreshToken">The refresh token JWT string.</param>
    /// <returns>
    /// A result containing <see cref="AuthenticationResponse"/> with new access token and user information if successful.
    /// Returns failure if token is invalid, session not found, session expired, or user IDs don't match.
    /// </returns>
    public Task<Result<AuthenticationResponse>> RefreshAccessTokenAsync(string refreshToken)
    {
        return _resultExecutor.ExecuteWithTransactionAsync(async () =>
        {
            var payload = JwtHelper.GetPayloadFromJWTToken(refreshToken, _options);
            var userInformation = JwtHelper.ParseUserInformation(payload);

            if (userInformation.IsFailure)
            {
                _logger.LogError("{error}", userInformation.Error);
                var error = userInformation.GetErrorObject();
                ErrorHelper.ThrowError(error);
            }

            var user = await _usersRepository.GetByIdAsync<UserDto>(userInformation.Value.UserId);

            if (user is null)
            {
                _logger.LogError("{error}", UserErrors.NotFoundById(userInformation.Value.UserId).ToString());
                ErrorHelper.ThrowErrorNull(UserErrors.NotFoundById(userInformation.Value.UserId));
            }

            var resultGet = await _sessionsRepository.GetByRefreshTokenAsync<SessionDto>(refreshToken);
            if (resultGet is null)
            {
                _logger.LogError(
                    "{error}",
                    SessionErrors.NotFoundByRefreshToken(refreshToken).ToString());
                ErrorHelper.ThrowError(SessionErrors.NotFoundByRefreshToken(refreshToken));
            }

            if (resultGet.UserId != userInformation.Value.UserId)
            {
                _logger.LogError(
                    "{error}",
                    AuthorizationErrors.UserIdsNotEquals(resultGet.UserId, userInformation.Value.UserId).ToString());
                ErrorHelper.ThrowError(AuthorizationErrors.NotFoundRefreshToken());
            }

            if (resultGet.Id != userInformation.Value.SessionId)
            {
                _logger.LogError(
                    "{error}",
                    AuthorizationErrors.SessionIdsNotEquals(resultGet.Id, userInformation.Value.SessionId).ToString());
                ErrorHelper.ThrowError(AuthorizationErrors.NotFoundRefreshToken());
            }

            if (resultGet.ExpirationDate < DateTime.UtcNow)
            {
                var error = AuthorizationErrors.RefreshTokenExpired();
                _logger.LogError("{error}", error.ToString());
                ErrorHelper.ThrowError(error);
            }

            var accessToken = JwtHelper.CreateAccessToken(userInformation.Value, _options);

            var session = Session.Create(
                userInformation.Value.UserId,
                accessToken.Token,
                resultGet.RefreshToken,
                resultGet.ExpirationDate);

            if (session.IsFailure)
            {
                _logger.LogError("{error}", session.Error);
                var error = session.GetErrorObject();
                ErrorHelper.ThrowError(error);
            }

            await _sessionsRepository.UpdateAsync(session.Value with { Id = resultGet.Id });

            return new AuthenticationResponse
            {
                Id = userInformation.Value.UserId,
                Role = userInformation.Value.Role,
                AccessToken = accessToken.Token,
                DisplayName = user.DisplayName,
                Avatar = User.ConvertAvatarToDataUri(user.AvatarData, user.AvatarContentType),
                Email = user.Email,
            };
        });
    }

    /// <summary>
    /// Logs out a user by invalidating their session.
    /// Deletes the session associated with the provided refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token identifying the session to invalidate.</param>
    /// <returns>
    /// A result indicating success if the session was deleted successfully.
    /// Returns failure if session not found or deletion fails.
    /// </returns>
    public Task<Result> LogoutAsync(string refreshToken)
    {
        return _resultExecutor.ExecuteWithTransactionAsync(async () =>
        {
            var session = await _sessionsRepository.GetByRefreshTokenAsync<SessionDto>(refreshToken);
            if (session is null)
            {
                var error = SessionErrors.NotFoundByRefreshToken(refreshToken);
                _logger.LogError("{error}", error.ToString());
                ErrorHelper.ThrowErrorNull(error);
            }

            var result = await _sessionsRepository.DeleteAsync(refreshToken);
            if (!result)
            {
                var error = AuthorizationErrors.LogoutError();
                _logger.LogError("{error}", error.ToString());
                ErrorHelper.ThrowError(error);
            }
        });
    }

    /// <summary>
    /// Creates a new user session with access and refresh tokens.
    /// </summary>
    /// <param name="user">The user with roles for which to create the session.</param>
    /// <returns>Authentication result containing access token, refresh token, and user information.</returns>
    private async Task<AuthenticationResult> CreateUserSessionAsync(UserWithRolesDto user)
    {
        var sessionId = Guid.NewGuid();
        var userRole = user.Roles.First();

        var userInformation = new UserInformation(
            user.UserName,
            user.Id,
            userRole,
            sessionId);

        var accessToken = JwtHelper.CreateAccessToken(userInformation, _options);
        var refreshToken = JwtHelper.CreateRefreshToken(userInformation, _options);

        var tokenResult = Session.Create(
            user.Id,
            accessToken.Token,
            refreshToken.Token,
            refreshToken.ExpirationDate);

        if (tokenResult.IsFailure)
        {
            _logger.LogError("{error}", tokenResult.Error);
            var error = tokenResult.GetErrorObject();
            ErrorHelper.ThrowError(error);
        }

        await _sessionsRepository.CreateAsync<SessionDto>(tokenResult.Value with { Id = sessionId });

        return new AuthenticationResult
        {
            Id = user.Id,
            Role = userRole,
            AccessToken = accessToken.Token,
            RefreshToken = refreshToken.Token,
            DisplayName = user.DisplayName,
            Avatar = User.ConvertAvatarToDataUri(user.AvatarData, user.AvatarContentType),
            Email = user.Email,
        };
    }
}
