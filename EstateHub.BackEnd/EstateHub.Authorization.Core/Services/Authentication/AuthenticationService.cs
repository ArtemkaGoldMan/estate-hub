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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EstateHub.Authorization.Core.Services.Authentication;

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

    public Task<Result<AuthenticationResult>> LoginAsync(LoginRequest request, bool beginTransaction = true)
    {
        return _resultExecutor.ExecuteWithTransactionAsync(
            async () =>
            {
                var userResult = await _usersRepository.GetByEmailAsync<UserWithRolesDto>(request.Email, true);

                if (userResult is null)
                {
                    _logger.LogError("{error}", UserErrors.NotFoundByEmail(request.Email).ToString());
                    throw new ArgumentNullException(AuthorizationErrors.IncorrectPasswordOrUsername().ToString());
                }

                if (userResult.IsDeleted)
                {
                    _logger.LogWarning("{error}", UserErrors.UserIsDeleted(userResult.Email).ToString());
                    throw new ArgumentException(UserErrors.UserIsDeleted(userResult.Email).ToString());
                }

                await _identityService.CheckPasswordAsync(userResult.Id, request.Password);

                return await CreateUserSessionAsync(userResult);
            }, beginTransaction);
    }

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
                    throw new ArgumentException(UserErrors.UserIsDeleted(request.Email).ToString());
                }

                _logger.LogError("{error}", UserErrors.EmailNotUnique().ToString());
                throw new ArgumentException(UserErrors.EmailNotUnique().ToString());
            }

            var userValidationResult = User.Create(request.Email, string.Empty, string.Empty, request.Password);

            if (userValidationResult.IsFailure)
            {
                _logger.LogError("{error}", userValidationResult.Error);
                throw new ArgumentException(userValidationResult.Error);
            }

            var result = await _identityService.RegisterAsync(userValidationResult.Value);

            if (!result.RequireConfirmedAccount)
            {
                var loginResult = await LoginAsync(
                    new LoginRequest { Email = request.Email, Password = request.Password },
                    beginTransaction: false);

                if (loginResult.IsFailure)
                    throw new ArgumentException(loginResult.Error);

                return loginResult.Value;
            }

            if (string.IsNullOrWhiteSpace(request.CallbackUrl))
            {
                _logger.LogError("{error}", AuthorizationErrors.CallbackIsNull().ToString());
                throw new ArgumentNullException(AuthorizationErrors.CallbackIsNull().ToString());
            }

            var sendResult = await _emailSmtpService
                .SendEmailConfirmationAsync(_smtpOptions, result.Email, result.GeneratedEmailConfirmationToken,
                    request.CallbackUrl, result.Id);

            if (sendResult.IsFailure)
            {
                throw new Exception(sendResult.Error);
            }

            return null;
        });
    }

    public Task<Result<AuthenticationResult>> ConfirmEmailAsync(ConfirmEmailRequest request)
    {
        return _resultExecutor.ExecuteWithTransactionAsync(async () =>
        {
            var userResult =
                await _usersRepository.GetByIdAsync<UserWithRolesDto>(request.UserId);

            if (userResult is null)
            {
                var error = UserErrors.NotFoundById(request.UserId).ToString();
                _logger.LogError("{error}", error);
                throw new ArgumentException(error);
            }

            await _identityService.ConfirmEmailAsync(request.UserId, request.Token);
            return await CreateUserSessionAsync(userResult);
        });
    }

    public Task<Result> ManageAccountState(ManageAccountRequest request)
    {
        return _resultExecutor.ExecuteAsync(async () =>
        {
            var userResult = await _usersRepository.GetByEmailAsync<UserDto>(request.Email, true);

            if (userResult is null || !userResult.IsDeleted)
            {
                var error = UserErrors.NotFoundByEmail(userResult.Email).ToString();
                _logger.LogError("{error}", error);
                throw new ArgumentNullException(error);
            }

            var token = await _identityService.GenerateAccountActionToken(userResult.Id, request.ActionType);

            var emailResult =
                await _emailSmtpService.SendAccountActionToken(_smtpOptions, userResult.Email, token,
                    request.ReturnUrl, request.ActionType, userResult.Id);

            if (emailResult.IsFailure)
            {
                throw new ArgumentException(emailResult.Error);
            }
        });
    }

    public Task<Result> ConfirmAccountAction(ConfirmAccountActionRequest request)
    {
        return _resultExecutor.ExecuteAsync(async () =>
        {
            var userResult =
                await _usersRepository.GetByIdAsync<UserDto>(request.UserId, true);

            if (userResult is null || !userResult.IsDeleted)
            {
                var error = UserErrors.NotFoundById(request.UserId).ToString();
                _logger.LogError("{error}", error);
                throw new ArgumentNullException(error);
            }

            await _identityService.ConfirmAccountAction(request.UserId, request.Token, request.ActionType);
        });
    }

    public Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        return _resultExecutor.ExecuteAsync(async () =>
        {
            var userResult = await _usersRepository.GetByEmailAsync<UserWithRolesDto>(request.Email);

            if (userResult is null)
            {
                _logger.LogError("{error}", UserErrors.NotFoundByEmail(request.Email).ToString());
                throw new ArgumentNullException(AuthorizationErrors.IncorrectPasswordOrUsername().ToString());
            }

            var token = await _identityService.GeneratePasswordResetTokenAsync(userResult.Id);

            var emailResult =
                await _emailSmtpService.SendForgetPasswordToken(_smtpOptions, userResult.Email, token,
                    request.ReturnUrl, userResult.Id);

            if (emailResult.IsFailure)
            {
                throw new ArgumentException(emailResult.Error);
            }
        });
    }

    public Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
    {
        return _resultExecutor.ExecuteWithTransactionAsync(async () =>
        {
            var userResult =
                await _usersRepository.GetByIdAsync<UserDto>(request.UserId);

            if (userResult is null)
            {
                _logger.LogError("{error}", UserErrors.NotFoundById(request.UserId).ToString());
                throw new ArgumentNullException(UserErrors.NotFoundById(request.UserId).ToString());
            }

            await _identityService.ResetPasswordAsync(request.UserId, request.Token, request.Password);
            await _sessionsRepository.DeleteByUserIdAsync(userResult.Id);
        });
    }

    public Task<Result<AuthenticationResponse>> RefreshAccessTokenAsync(string refreshToken)
    {
        return _resultExecutor.ExecuteWithTransactionAsync(async () =>
        {
            var payload = JwtHelper.GetPayloadFromJWTToken(refreshToken, _options);
            var userInformation = JwtHelper.ParseUserInformation(payload);

            if (userInformation.IsFailure)
            {
                _logger.LogError("{error}", userInformation.Error);
                throw new ArgumentException(userInformation.Error);
            }

            var user = await _usersRepository.GetByIdAsync<UserDto>(userInformation.Value.UserId);

            if (user is null)
            {
                _logger.LogError("{error}", UserErrors.NotFoundById(userInformation.Value.UserId).ToString());
                throw new ArgumentNullException(UserErrors.NotFoundById(userInformation.Value.UserId).ToString());
            }

            var resultGet = await _sessionsRepository.GetByRefreshTokenAsync<SessionDto>(refreshToken);
            if (resultGet is null)
            {
                _logger.LogError(
                    "{error}",
                    SessionErrors.NotFoundByRefreshToken(refreshToken).ToString());
                throw new ArgumentException(SessionErrors.NotFoundByRefreshToken(refreshToken).ToString());
            }

            if (resultGet.UserId != userInformation.Value.UserId)
            {
                _logger.LogError(
                    "{error}",
                    AuthorizationErrors.UserIdsNotEquals(resultGet.UserId, userInformation.Value.UserId).ToString());
                throw new ArgumentException(AuthorizationErrors.NotFoundRefreshToken().ToString());
            }

            if (resultGet.Id != userInformation.Value.SessionId)
            {
                _logger.LogError(
                    "{error}",
                    AuthorizationErrors.SessionIdsNotEquals(resultGet.Id, userInformation.Value.SessionId).ToString());
                throw new ArgumentException(AuthorizationErrors.NotFoundRefreshToken().ToString());
            }

            if (resultGet.ExpirationDate < DateTime.UtcNow)
            {
                var error = AuthorizationErrors.RefreshTokenExpired();
                _logger.LogError("{error}", error.ToString());
                throw new ArgumentException(error.ToString());
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
                throw new ArgumentException(session.Error);
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

    public Task<Result> LogoutAsync(string refreshToken)
    {
        return _resultExecutor.ExecuteWithTransactionAsync(async () =>
        {
            var session = await _sessionsRepository.GetByRefreshTokenAsync<SessionDto>(refreshToken);
            if (session is null)
            {
                var error = SessionErrors.NotFoundByRefreshToken(refreshToken);
                _logger.LogError("{error}", error.ToString());
                throw new ArgumentNullException(error.ToString());
            }

            var result = await _sessionsRepository.DeleteAsync(refreshToken);
            if (!result)
            {
                var error = AuthorizationErrors.LogoutError();
                _logger.LogError("{error}", error.ToString());
                throw new ArgumentException(error.ToString());
            }
        });
    }

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
            throw new ArgumentException(tokenResult.Error);
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
