using AutoMapper;
using EstateHub.Authorization.Domain;
using EstateHub.Authorization.Domain.DTO.User;
using EstateHub.Authorization.Domain.Errors;
using EstateHub.Authorization.Domain.Interfaces.DataAccessInterfaces;
using EstateHub.Authorization.Domain.Models;
using EstateHub.Authorization.DataAccess.SqlServer.Entities;
using EstateHub.Authorization.DataAccess.SqlServer.Repositories;
using EstateHub.SharedKernel;
using EstateHub.SharedKernel.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EstateHub.Authorization.DataAccess.SqlServer.Services;

public class IdentityService : IIdentityService
{
    private readonly IMapper _mapper;
    private readonly UserManager<UserEntity> _userManager;
    private readonly RoleManager<RoleEntity> _roleManager;
    private readonly ILogger<UsersRepository> _logger;
    private IConfigurationProvider _mapperConfig => _mapper.ConfigurationProvider;

    public IdentityService(
        IMapper mapper,
        UserManager<UserEntity> userManager,
        RoleManager<RoleEntity> roleManager,
        ILogger<UsersRepository> logger)
    {
        _mapper = mapper;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task CheckPasswordAsync(Guid userId, string password)
    {
        var userResult = await _userManager.FindByIdAsync(userId.ToString());

        if (userResult is null)
        {
            _logger.LogError("{error}", AuthorizationErrors.IncorrectPasswordOrUsername().ToString());
            throw new ArgumentNullException(AuthorizationErrors.IncorrectPasswordOrUsername().ToString());
        }

        if (_userManager.Options.SignIn.RequireConfirmedAccount && !userResult.EmailConfirmed)
        {
            _logger.LogError("{error}", AuthorizationErrors.EmailNotConfirmed().ToString());
            throw new ArgumentException(AuthorizationErrors.EmailNotConfirmed().ToString());
        }

        var isSuccess = await _userManager
            .CheckPasswordAsync(userResult, password);

        if (!isSuccess)
        {
            _logger.LogError("{error}", AuthorizationErrors.IncorrectPasswordOrUsername().ToString());
            throw new ArgumentException(AuthorizationErrors.IncorrectPasswordOrUsername().ToString());
        }

        var roles = await _userManager.GetRolesAsync(userResult);
        var role = roles.FirstOrDefault();
        if (role is null)
        {
            _logger.LogError("{error}", AuthorizationErrors.InvalidRole().ToString());
            throw new ArgumentNullException(AuthorizationErrors.InvalidRole().ToString());
        }
    }

    public async Task<UserRegistrationDto> RegisterAsync(User request)
    {
        var newUser = new UserEntity
        {
            Id = request.Id,
            UserName = request.UserName,
            Email = request.Email,
            DisplayName = request.DisplayName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(
            newUser,
            request.Password);

        if (!result.Succeeded)
        {
            _logger.LogError("{errors}", result.Errors);
            var userMessage = EstateHub.SharedKernel.Helpers.IdentityErrorExtractor.ToUserMessage(result);
            var error = UserErrors.UserNotCreated().WithUserMessage(userMessage);
            throw new ArgumentException(error.ToString()) { Data = { ["Error"] = error } };
        }

        var roleExists = await _roleManager.RoleExistsAsync(nameof(Roles.User));
        if (!roleExists)
        {
            var role = new RoleEntity
            {
                Name = nameof(Roles.User)
            };

            await _roleManager.CreateAsync(role);
        }

        var isSuccess = await _userManager.AddToRoleAsync(newUser, nameof(Roles.User));
        if (!isSuccess.Succeeded)
        {
            _logger.LogError("{errors}", isSuccess.Errors);
            var userMessage = EstateHub.SharedKernel.Helpers.IdentityErrorExtractor.ToUserMessage(isSuccess);
            var error = UserErrors.UserNotAddedToRole().WithUserMessage(userMessage);
            throw new ArgumentException(error.ToString()) { Data = { ["Error"] = error } };
        }

        var generatedEmailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);

        if (!_userManager.Options.SignIn.RequireConfirmedAccount)
        {
            var confirmEmailResult = await _userManager.ConfirmEmailAsync(newUser, generatedEmailConfirmationToken);
            if (!confirmEmailResult.Succeeded)
            {
                _logger.LogError("{errors}", confirmEmailResult.Errors);
                var userMessage = EstateHub.SharedKernel.Helpers.IdentityErrorExtractor.ToUserMessage(confirmEmailResult);
                var error = AuthorizationErrors.EmailNotConfirmed().WithUserMessage(userMessage);
                throw new ArgumentException(error.ToString()) { Data = { ["Error"] = error } };
            }

            generatedEmailConfirmationToken = string.Empty;
        }

        return new UserRegistrationDto
        {
            Id = newUser.Id,
            Email = newUser.Email,
            UserName = newUser.UserName,
            Roles = new List<string> { nameof(Roles.User) },
            RequireConfirmedAccount = _userManager.Options.SignIn.RequireConfirmedAccount,
            GeneratedEmailConfirmationToken = generatedEmailConfirmationToken,
        };
    }

    public async Task<string> GenerateAccountActionToken(Guid userId, AccountActionType actionType)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            _logger.LogError("{error}", UserErrors.NotFoundById(userId).ToString());
            throw new ArgumentException(UserErrors.NotFoundById(userId).ToString());
        }

        if (!user.EmailConfirmed)
        {
            _logger.LogError("{error}", AuthorizationErrors.EmailNotConfirmed().ToString());
            throw new ArgumentException(AuthorizationErrors.EmailNotConfirmed().ToString());
        }

        string purpose = actionType switch
        {
            AccountActionType.Recover => TokenPurpose.RecoverAccount,
            //AccountActionType.HardDelete => TokenPurpose.HardDelete,
            _ => throw new ArgumentException(AuthorizationErrors.NotFoundAccountAction().ToString())
        };

        return await _userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, purpose);
    }

    public async Task ConfirmAccountAction(Guid requestUserId, string requestToken, AccountActionType requestActionType)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == requestUserId);

        if (user is null)
        {
            _logger.LogError("{error}", UserErrors.NotFoundById(requestUserId).ToString());
            throw new ArgumentException(UserErrors.NotFoundById(requestUserId).ToString());
        }

        var result = requestActionType switch
        {
            AccountActionType.Recover => await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, TokenPurpose.RecoverAccount, requestToken),
            //AccountActionType.HardDelete => await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, TokenPurpose.HardDelete, requestToken),
            _ => throw new ArgumentException(AuthorizationErrors.NotFoundAccountAction().ToString())
        };

        if (!result)
        {
            _logger.LogError("{error}", AuthorizationErrors.InvalidToken().ToString());
            throw new ArgumentException(AuthorizationErrors.InvalidToken().ToString());
        }

        await _userManager.UpdateSecurityStampAsync(user);

        if (requestActionType == AccountActionType.HardDelete)
        {
            var resultDelete = await _userManager.DeleteAsync(user);
            if (!resultDelete.Succeeded)
            {
                _logger.LogError("{errors}", resultDelete.Errors);
                throw new ArgumentException(UserErrors.UserNotHardDeleted().ToString());
            }
        }
        else if (requestActionType == AccountActionType.Recover)
        {
            user.IsDeleted = false;
            user.DeletedAt = null;
            var resultRecover = await _userManager.UpdateAsync(user);
        if (!resultRecover.Succeeded)
        {
            _logger.LogError("{errors}", resultRecover.Errors);
            var userMessage = IdentityErrorExtractor.ToUserMessage(resultRecover);
            var error = UserErrors.UserNotRecovered().WithUserMessage(userMessage);
            ErrorHelper.ThrowError(error);
        }
        }
    }

    public async Task ConfirmEmailAsync(Guid userId, string token)
    {
        var userResult =
            await _userManager.FindByIdAsync(userId.ToString());

        if (userResult is null)
        {
            _logger.LogError("{error}", UserErrors.NotFoundById(userId).ToString());
            ErrorHelper.ThrowError(UserErrors.NotFoundById(userId));
        }

        var result = await _userManager.ConfirmEmailAsync(userResult, token);
        if (!result.Succeeded)
        {
            _logger.LogError("{errors}", result.Errors);
            var userMessage = IdentityErrorExtractor.ToUserMessage(result);
            var error = AuthorizationErrors.EmailNotConfirmed().WithUserMessage(userMessage);
            ErrorHelper.ThrowError(error);
        }
    }

    public async Task<string> GeneratePasswordResetTokenAsync(Guid userId)
    {
        var userResult =
            await _userManager.FindByIdAsync(userId.ToString());

        if (userResult is null)
        {
            _logger.LogError("{error}", UserErrors.NotFoundById(userId).ToString());
            ErrorHelper.ThrowError(UserErrors.NotFoundById(userId));
        }

        if (!userResult.EmailConfirmed)
        {
            _logger.LogError("{error}", AuthorizationErrors.EmailNotConfirmed().ToString());
            ErrorHelper.ThrowError(AuthorizationErrors.EmailNotConfirmed());
        }

        return await _userManager.GeneratePasswordResetTokenAsync(userResult);
    }

    public async Task ResetPasswordAsync(Guid userId, string token, string password)
    {
        var userResult =
            await _userManager.FindByIdAsync(userId.ToString());

        if (userResult is null)
        {
            _logger.LogError("{error}", UserErrors.NotFoundById(userId).ToString());
            ErrorHelper.ThrowError(UserErrors.NotFoundById(userId));
        }

        var userValidationResult = User.Create(userResult.Email, userResult.DisplayName, userResult.UserName, password);

        if (userValidationResult.IsFailure)
        {
            // Parse error from Result string format
            var errorParts = userValidationResult.Error.Split(TextDelimiters.Separator);
            if (errorParts.Length >= 4)
            {
                var error = new Error(errorParts[0], errorParts[1], errorParts[2], errorParts[3]);
                ErrorHelper.ThrowError(error);
            }
            ErrorHelper.ThrowError(UserErrors.InvalidPassword());
        }

        var result = await _userManager.ResetPasswordAsync(userResult, token, password);
        if (!result.Succeeded)
        {
            _logger.LogError("{errors}", result.Errors);
            var userMessage = IdentityErrorExtractor.ToUserMessage(result);
            var error = AuthorizationErrors.IncorrectPasswordOrUsername().WithUserMessage(userMessage);
            ErrorHelper.ThrowError(error);
        }
    }
}
