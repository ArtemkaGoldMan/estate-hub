using CSharpFunctionalExtensions;
using EstateHub.Authorization.Core.Services.Authentication;
using EstateHub.Authorization.Domain.DTO.Authentication.Requests;
using EstateHub.Authorization.Domain.DTO.Authentication.Responses;
using EstateHub.Authorization.Domain.DTO.Session;
using EstateHub.Authorization.Domain.DTO.User;
using EstateHub.Authorization.Domain.Interfaces.CoreInterfaces;
using EstateHub.Authorization.Domain.Interfaces.DataAccessInterfaces;
using EstateHub.Authorization.Domain.Interfaces.InfrastructureInterfaces;
using EstateHub.Authorization.Domain.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace EstateHub.Authorization.Core.Tests;

public class AuthenticationServiceTests
{
    private readonly Mock<ILogger<AuthenticationService>> _loggerMock;
    private readonly Mock<IUsersRepository> _usersRepositoryMock;
    private readonly Mock<ISessionsRepository> _sessionsRepositoryMock;
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IEmailSmtpService> _emailSmtpServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IOptions<JWTOptions> _jwtOptions;
    private readonly IOptions<SmtpOptions> _smtpOptions;
    private readonly AuthenticationService _authenticationService;

    public AuthenticationServiceTests()
    {
        _loggerMock = new Mock<ILogger<AuthenticationService>>();
        _usersRepositoryMock = new Mock<IUsersRepository>();
        _sessionsRepositoryMock = new Mock<ISessionsRepository>();
        _identityServiceMock = new Mock<IIdentityService>();
        _emailSmtpServiceMock = new Mock<IEmailSmtpService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        // Setup JWT options
        var jwtOptionsValue = new JWTOptions
        {
            Secret = "test-secret-key-that-is-at-least-32-characters-long",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpirationMinutes = 60
        };
        _jwtOptions = Options.Create(jwtOptionsValue);

        // Setup SMTP options
        var smtpOptionsValue = new SmtpOptions();
        _smtpOptions = Options.Create(smtpOptionsValue);

        // Setup unit of work to return success
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(CSharpFunctionalExtensions.Result.Success(true));
        _unitOfWorkMock.Setup(u => u.CommitAsync())
            .ReturnsAsync(CSharpFunctionalExtensions.Result.Success(true));

        _authenticationService = new AuthenticationService(
            _loggerMock.Object,
            _jwtOptions,
            _smtpOptions,
            _unitOfWorkMock.Object,
            _usersRepositoryMock.Object,
            _sessionsRepositoryMock.Object,
            _emailSmtpServiceMock.Object,
            _identityServiceMock.Object
        );
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthenticationResult()
    {
        // Arrange
        var email = "test@example.com";
        var password = "ValidPassword123!@#";
        var userId = Guid.NewGuid();
        var request = new LoginRequest { Email = email, Password = password };

        var userWithRoles = new UserWithRolesDto
        {
            Id = userId,
            Email = email,
            UserName = email,
            DisplayName = "Test User",
            IsDeleted = false,
            Roles = new List<string> { "User" }
        };

        _usersRepositoryMock
            .Setup(r => r.GetByEmailAsync<UserWithRolesDto>(email, true))
            .ReturnsAsync(userWithRoles);

        _identityServiceMock
            .Setup(s => s.CheckPasswordAsync(userId, password))
            .Returns(Task.CompletedTask);

        _sessionsRepositoryMock
            .Setup(r => r.CreateAsync<SessionDto>(It.IsAny<EstateHub.Authorization.Domain.Models.Session>()))
            .ReturnsAsync((SessionDto)null!);

        // Act
        var result = await _authenticationService.LoginAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value.AccessToken);
        Assert.NotNull(result.Value.RefreshToken);
        Assert.Equal(userId, result.Value.Id);

        _usersRepositoryMock.Verify(r => r.GetByEmailAsync<UserWithRolesDto>(email, true), Times.Once);
        _identityServiceMock.Verify(s => s.CheckPasswordAsync(userId, password), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentUser_ReturnsFailure()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var password = "SomePassword123!@#";
        var request = new LoginRequest { Email = email, Password = password };

        _usersRepositoryMock
            .Setup(r => r.GetByEmailAsync<UserWithRolesDto>(email, true))
            .ReturnsAsync((UserWithRolesDto?)null);

        // Act
        var result = await _authenticationService.LoginAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.GetByEmailAsync<UserWithRolesDto>(email, true), Times.Once);
        _identityServiceMock.Verify(s => s.CheckPasswordAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithDeletedUser_ReturnsFailure()
    {
        // Arrange
        var email = "deleted@example.com";
        var password = "SomePassword123!@#";
        var userId = Guid.NewGuid();
        var request = new LoginRequest { Email = email, Password = password };

        var deletedUser = new UserWithRolesDto
        {
            Id = userId,
            Email = email,
            UserName = email,
            DisplayName = "Deleted User",
            IsDeleted = true,
            Roles = new List<string> { "User" }
        };

        _usersRepositoryMock
            .Setup(r => r.GetByEmailAsync<UserWithRolesDto>(email, true))
            .ReturnsAsync(deletedUser);

        // Act
        var result = await _authenticationService.LoginAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.GetByEmailAsync<UserWithRolesDto>(email, true), Times.Once);
        _identityServiceMock.Verify(s => s.CheckPasswordAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithNewEmail_ReturnsSuccess()
    {
        // Arrange
        var email = "newuser@example.com";
        var password = "ValidPassword123!@#";
        var userId = Guid.NewGuid();
        var request = new UserRegistrationRequest
        {
            Email = email,
            Password = password,
            ConfirmPassword = password,
            CallbackUrl = "https://example.com/confirm"
        };

        var userRegistrationResult = new EstateHub.Authorization.Domain.DTO.User.UserRegistrationDto
        {
            Id = userId,
            Email = email,
            UserName = email,
            Roles = new List<string> { "User" },
            RequireConfirmedAccount = true,
            GeneratedEmailConfirmationToken = "test-token"
        };

        _usersRepositoryMock
            .Setup(r => r.GetByEmailAsync<UserDto>(email, true))
            .ReturnsAsync((UserDto?)null);

        _identityServiceMock
            .Setup(s => s.RegisterAsync(It.IsAny<EstateHub.Authorization.Domain.Models.User>()))
            .ReturnsAsync(userRegistrationResult);

        _emailSmtpServiceMock
            .Setup(s => s.SendEmailConfirmationAsync(
                It.IsAny<SmtpOptions>(),
                email,
                "test-token",
                request.CallbackUrl,
                userId))
            .ReturnsAsync(CSharpFunctionalExtensions.Result.Success(true));

        // Act
        var result = await _authenticationService.RegisterAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value); // Returns null when email confirmation is required
        _usersRepositoryMock.Verify(r => r.GetByEmailAsync<UserDto>(email, true), Times.Once);
        _identityServiceMock.Verify(s => s.RegisterAsync(It.IsAny<EstateHub.Authorization.Domain.Models.User>()), Times.Once);
        _emailSmtpServiceMock.Verify(s => s.SendEmailConfirmationAsync(
            It.IsAny<SmtpOptions>(), email, "test-token", request.CallbackUrl, userId), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ReturnsFailure()
    {
        // Arrange
        var email = "existing@example.com";
        var password = "ValidPassword123!@#";
        var request = new UserRegistrationRequest
        {
            Email = email,
            Password = password,
            ConfirmPassword = password,
            CallbackUrl = "https://example.com/confirm"
        };

        var existingUser = new UserDto
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            DisplayName = "Existing User",
            IsDeleted = false
        };

        _usersRepositoryMock
            .Setup(r => r.GetByEmailAsync<UserDto>(email, true))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _authenticationService.RegisterAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.GetByEmailAsync<UserDto>(email, true), Times.Once);
        _identityServiceMock.Verify(s => s.RegisterAsync(It.IsAny<EstateHub.Authorization.Domain.Models.User>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmEmailAsync_WithValidToken_ReturnsAuthenticationResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "valid-confirmation-token";
        var request = new ConfirmEmailRequest { UserId = userId, Token = token };

        var userWithRoles = new UserWithRolesDto
        {
            Id = userId,
            Email = "test@example.com",
            UserName = "test@example.com",
            DisplayName = "Test User",
            IsDeleted = false,
            Roles = new List<string> { "User" }
        };

        _usersRepositoryMock
            .Setup(r => r.GetByIdAsync<UserWithRolesDto>(userId, false))
            .ReturnsAsync(userWithRoles);

        _identityServiceMock
            .Setup(s => s.ConfirmEmailAsync(userId, token))
            .Returns(Task.CompletedTask);

        _sessionsRepositoryMock
            .Setup(r => r.CreateAsync<SessionDto>(It.IsAny<EstateHub.Authorization.Domain.Models.Session>()))
            .ReturnsAsync((SessionDto)null!);

        // Act
        var result = await _authenticationService.ConfirmEmailAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value.AccessToken);
        Assert.NotNull(result.Value.RefreshToken);
        Assert.Equal(userId, result.Value.Id);

        _usersRepositoryMock.Verify(r => r.GetByIdAsync<UserWithRolesDto>(userId, false), Times.Once);
        _identityServiceMock.Verify(s => s.ConfirmEmailAsync(userId, token), Times.Once);
    }

    [Fact]
    public async Task ForgotPasswordAsync_WithValidEmail_SendsPasswordResetEmail()
    {
        // Arrange
        var email = "test@example.com";
        var userId = Guid.NewGuid();
        var returnUrl = "https://example.com/reset";
        var request = new ForgotPasswordRequest { Email = email, ReturnUrl = returnUrl };

        var userWithRoles = new UserWithRolesDto
        {
            Id = userId,
            Email = email,
            UserName = email,
            DisplayName = "Test User",
            IsDeleted = false,
            Roles = new List<string> { "User" }
        };

        var resetToken = "password-reset-token";

        _usersRepositoryMock
            .Setup(r => r.GetByEmailAsync<UserWithRolesDto>(email, false))
            .ReturnsAsync(userWithRoles);

        _identityServiceMock
            .Setup(s => s.GeneratePasswordResetTokenAsync(userId))
            .ReturnsAsync(resetToken);

        _emailSmtpServiceMock
            .Setup(s => s.SendForgetPasswordToken(
                It.IsAny<SmtpOptions>(),
                email,
                resetToken,
                returnUrl,
                userId))
            .ReturnsAsync(CSharpFunctionalExtensions.Result.Success(true));

        // Act
        var result = await _authenticationService.ForgotPasswordAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        _usersRepositoryMock.Verify(r => r.GetByEmailAsync<UserWithRolesDto>(email, false), Times.Once);
        _identityServiceMock.Verify(s => s.GeneratePasswordResetTokenAsync(userId), Times.Once);
        _emailSmtpServiceMock.Verify(s => s.SendForgetPasswordToken(
            It.IsAny<SmtpOptions>(), email, resetToken, returnUrl, userId), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsFailure()
    {
        // Arrange
        var email = "test@example.com";
        var password = "WrongPassword123!@#";
        var userId = Guid.NewGuid();
        var request = new LoginRequest { Email = email, Password = password };

        var userWithRoles = new UserWithRolesDto
        {
            Id = userId,
            Email = email,
            UserName = email,
            DisplayName = "Test User",
            IsDeleted = false,
            Roles = new List<string> { "User" }
        };

        _usersRepositoryMock
            .Setup(r => r.GetByEmailAsync<UserWithRolesDto>(email, true))
            .ReturnsAsync(userWithRoles);

        _identityServiceMock
            .Setup(s => s.CheckPasswordAsync(userId, password))
            .ThrowsAsync(new InvalidOperationException("Invalid password"));

        // Act
        var result = await _authenticationService.LoginAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.GetByEmailAsync<UserWithRolesDto>(email, true), Times.Once);
        _identityServiceMock.Verify(s => s.CheckPasswordAsync(userId, password), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithoutEmailConfirmation_AutoLogsIn()
    {
        // Arrange
        var email = "newuser@example.com";
        var password = "ValidPassword123!@#";
        var userId = Guid.NewGuid();
        var request = new UserRegistrationRequest
        {
            Email = email,
            Password = password,
            ConfirmPassword = password,
            CallbackUrl = "https://example.com/confirm"
        };

        var userRegistrationResult = new EstateHub.Authorization.Domain.DTO.User.UserRegistrationDto
        {
            Id = userId,
            Email = email,
            UserName = email,
            Roles = new List<string> { "User" },
            RequireConfirmedAccount = false, // Auto-login enabled
            GeneratedEmailConfirmationToken = string.Empty
        };

        var userWithRoles = new UserWithRolesDto
        {
            Id = userId,
            Email = email,
            UserName = email,
            DisplayName = "New User",
            IsDeleted = false,
            Roles = new List<string> { "User" }
        };

        _usersRepositoryMock
            .Setup(r => r.GetByEmailAsync<UserDto>(email, true))
            .ReturnsAsync((UserDto?)null);

        _identityServiceMock
            .Setup(s => s.RegisterAsync(It.IsAny<EstateHub.Authorization.Domain.Models.User>()))
            .ReturnsAsync(userRegistrationResult);

        _usersRepositoryMock
            .Setup(r => r.GetByEmailAsync<UserWithRolesDto>(email, true))
            .ReturnsAsync(userWithRoles);

        _identityServiceMock
            .Setup(s => s.CheckPasswordAsync(userId, password))
            .Returns(Task.CompletedTask);

        _sessionsRepositoryMock
            .Setup(r => r.CreateAsync<SessionDto>(It.IsAny<EstateHub.Authorization.Domain.Models.Session>()))
            .ReturnsAsync((SessionDto)null!);

        // Act
        var result = await _authenticationService.RegisterAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value); // Should return authentication result
        Assert.NotNull(result.Value.AccessToken);
        Assert.NotNull(result.Value.RefreshToken);
        Assert.Equal(userId, result.Value.Id);
        _emailSmtpServiceMock.Verify(s => s.SendEmailConfirmationAsync(
            It.IsAny<SmtpOptions>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithValidToken_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "valid-reset-token";
        var newPassword = "NewPassword123!@#";
        var request = new ResetPasswordRequest
        {
            UserId = userId,
            Token = token,
            Password = newPassword,
            ConfirmPassword = newPassword
        };

        var user = new UserDto
        {
            Id = userId,
            Email = "test@example.com",
            UserName = "test@example.com",
            DisplayName = "Test User",
            IsDeleted = false
        };

        _usersRepositoryMock
            .Setup(r => r.GetByIdAsync<UserDto>(userId, false))
            .ReturnsAsync(user);

        _identityServiceMock
            .Setup(s => s.ResetPasswordAsync(userId, token, newPassword))
            .Returns(Task.CompletedTask);

        _sessionsRepositoryMock
            .Setup(r => r.DeleteByUserIdAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _authenticationService.ResetPasswordAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        _usersRepositoryMock.Verify(r => r.GetByIdAsync<UserDto>(userId, false), Times.Once);
        _identityServiceMock.Verify(s => s.ResetPasswordAsync(userId, token, newPassword), Times.Once);
        _sessionsRepositoryMock.Verify(r => r.DeleteByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithInvalidToken_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "invalid-token";
        var newPassword = "NewPassword123!@#";
        var request = new ResetPasswordRequest
        {
            UserId = userId,
            Token = token,
            Password = newPassword,
            ConfirmPassword = newPassword
        };

        var user = new UserDto
        {
            Id = userId,
            Email = "test@example.com",
            UserName = "test@example.com",
            DisplayName = "Test User",
            IsDeleted = false
        };

        _usersRepositoryMock
            .Setup(r => r.GetByIdAsync<UserDto>(userId, false))
            .ReturnsAsync(user);

        _identityServiceMock
            .Setup(s => s.ResetPasswordAsync(userId, token, newPassword))
            .ThrowsAsync(new InvalidOperationException("Invalid token"));

        // Act
        var result = await _authenticationService.ResetPasswordAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.GetByIdAsync<UserDto>(userId, false), Times.Once);
        _identityServiceMock.Verify(s => s.ResetPasswordAsync(userId, token, newPassword), Times.Once);
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_WithValidToken_ReturnsNewTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var expirationDate = DateTimeOffset.UtcNow.AddMonths(1);

        // Create a valid JWT refresh token for testing
        var userInformation = new UserInformation("test@example.com", userId, "User", sessionId);
        var validRefreshToken = EstateHub.Authorization.Core.Helpers.JwtHelper.CreateRefreshToken(userInformation, _jwtOptions.Value);

        var user = new UserDto
        {
            Id = userId,
            Email = "test@example.com",
            UserName = "test@example.com",
            DisplayName = "Test User",
            IsDeleted = false
        };

        var session = new SessionDto
        {
            Id = sessionId,
            UserId = userId,
            AccessToken = "old-access-token",
            RefreshToken = validRefreshToken.Token, // Use the actual token from JWT helper
            ExpirationDate = expirationDate
        };

        _usersRepositoryMock
            .Setup(r => r.GetByIdAsync<UserDto>(userId, false))
            .ReturnsAsync(user);

        _sessionsRepositoryMock
            .Setup(r => r.GetByRefreshTokenAsync<SessionDto>(validRefreshToken.Token))
            .ReturnsAsync(session);

        _sessionsRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<EstateHub.Authorization.Domain.Models.Session>()))
            .ReturnsAsync(true);

        // Act
        var result = await _authenticationService.RefreshAccessTokenAsync(validRefreshToken.Token);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value.AccessToken);
        Assert.Equal(userId, result.Value.Id);
        _sessionsRepositoryMock.Verify(r => r.GetByRefreshTokenAsync<SessionDto>(validRefreshToken.Token), Times.Once);
        _sessionsRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<EstateHub.Authorization.Domain.Models.Session>()), Times.Once);
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_WithExpiredSession_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var expiredDate = DateTimeOffset.UtcNow.AddDays(-1); // Expired

        // Create a valid JWT refresh token for testing
        var userInformation = new UserInformation("test@example.com", userId, "User", sessionId);
        var validRefreshToken = EstateHub.Authorization.Core.Helpers.JwtHelper.CreateRefreshToken(userInformation, _jwtOptions.Value);

        var user = new UserDto
        {
            Id = userId,
            Email = "test@example.com",
            UserName = "test@example.com",
            DisplayName = "Test User",
            IsDeleted = false
        };

        var expiredSession = new SessionDto
        {
            Id = sessionId,
            UserId = userId,
            AccessToken = "old-access-token",
            RefreshToken = validRefreshToken.Token,
            ExpirationDate = expiredDate // Expired session
        };

        _usersRepositoryMock
            .Setup(r => r.GetByIdAsync<UserDto>(userId, false))
            .ReturnsAsync(user);

        _sessionsRepositoryMock
            .Setup(r => r.GetByRefreshTokenAsync<SessionDto>(validRefreshToken.Token))
            .ReturnsAsync(expiredSession);

        // Act
        var result = await _authenticationService.RefreshAccessTokenAsync(validRefreshToken.Token);

        // Assert
        Assert.True(result.IsFailure);
        _sessionsRepositoryMock.Verify(r => r.GetByRefreshTokenAsync<SessionDto>(validRefreshToken.Token), Times.Once);
        _sessionsRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<EstateHub.Authorization.Domain.Models.Session>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_WithInvalidToken_ReturnsFailure()
    {
        // Arrange
        var invalidToken = "invalid-token-string";

        // Act
        var result = await _authenticationService.RefreshAccessTokenAsync(invalidToken);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.GetByIdAsync<UserDto>(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
        _sessionsRepositoryMock.Verify(r => r.GetByRefreshTokenAsync<SessionDto>(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_WithValidToken_ReturnsSuccess()
    {
        // Arrange
        var refreshToken = "valid-refresh-token";
        var session = new SessionDto
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            AccessToken = "access-token",
            RefreshToken = refreshToken,
            ExpirationDate = DateTimeOffset.UtcNow.AddMonths(1)
        };

        _sessionsRepositoryMock
            .Setup(r => r.GetByRefreshTokenAsync<SessionDto>(refreshToken))
            .ReturnsAsync(session);

        _sessionsRepositoryMock
            .Setup(r => r.DeleteAsync(refreshToken))
            .ReturnsAsync(true);

        // Act
        var result = await _authenticationService.LogoutAsync(refreshToken);

        // Assert
        Assert.True(result.IsSuccess);
        _sessionsRepositoryMock.Verify(r => r.GetByRefreshTokenAsync<SessionDto>(refreshToken), Times.Once);
        _sessionsRepositoryMock.Verify(r => r.DeleteAsync(refreshToken), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_WithInvalidToken_ReturnsFailure()
    {
        // Arrange
        var invalidToken = "invalid-refresh-token";

        _sessionsRepositoryMock
            .Setup(r => r.GetByRefreshTokenAsync<SessionDto>(invalidToken))
            .ReturnsAsync((SessionDto?)null);

        // Act
        var result = await _authenticationService.LogoutAsync(invalidToken);

        // Assert
        Assert.True(result.IsFailure);
        _sessionsRepositoryMock.Verify(r => r.GetByRefreshTokenAsync<SessionDto>(invalidToken), Times.Once);
        _sessionsRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmEmailAsync_WithInvalidToken_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "invalid-token";
        var request = new ConfirmEmailRequest { UserId = userId, Token = token };

        var userWithRoles = new UserWithRolesDto
        {
            Id = userId,
            Email = "test@example.com",
            UserName = "test@example.com",
            DisplayName = "Test User",
            IsDeleted = false,
            Roles = new List<string> { "User" }
        };

        _usersRepositoryMock
            .Setup(r => r.GetByIdAsync<UserWithRolesDto>(userId, false))
            .ReturnsAsync(userWithRoles);

        _identityServiceMock
            .Setup(s => s.ConfirmEmailAsync(userId, token))
            .ThrowsAsync(new InvalidOperationException("Invalid token"));

        // Act
        var result = await _authenticationService.ConfirmEmailAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.GetByIdAsync<UserWithRolesDto>(userId, false), Times.Once);
        _identityServiceMock.Verify(s => s.ConfirmEmailAsync(userId, token), Times.Once);
    }

    [Fact]
    public async Task ForgotPasswordAsync_WithNonExistentEmail_ReturnsFailure()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var returnUrl = "https://example.com/reset";
        var request = new ForgotPasswordRequest { Email = email, ReturnUrl = returnUrl };

        _usersRepositoryMock
            .Setup(r => r.GetByEmailAsync<UserWithRolesDto>(email, false))
            .ReturnsAsync((UserWithRolesDto?)null);

        // Act
        var result = await _authenticationService.ForgotPasswordAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.GetByEmailAsync<UserWithRolesDto>(email, false), Times.Once);
        _identityServiceMock.Verify(s => s.GeneratePasswordResetTokenAsync(It.IsAny<Guid>()), Times.Never);
        _emailSmtpServiceMock.Verify(s => s.SendForgetPasswordToken(
            It.IsAny<SmtpOptions>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ManageAccountState_WithDeletedUser_ReturnsSuccess()
    {
        // Arrange
        var email = "deleted@example.com";
        var userId = Guid.NewGuid();
        var returnUrl = "https://example.com/confirm";
        var actionType = EstateHub.Authorization.Domain.Models.AccountActionType.Recover;
        var request = new ManageAccountRequest
        {
            Email = email,
            ActionType = actionType,
            ReturnUrl = returnUrl
        };

        var deletedUser = new UserDto
        {
            Id = userId,
            Email = email,
            UserName = email,
            DisplayName = "Deleted User",
            IsDeleted = true
        };

        var accountActionToken = "account-action-token";

        _usersRepositoryMock
            .Setup(r => r.GetByEmailAsync<UserDto>(email, true))
            .ReturnsAsync(deletedUser);

        _identityServiceMock
            .Setup(s => s.GenerateAccountActionToken(userId, actionType))
            .ReturnsAsync(accountActionToken);

        _emailSmtpServiceMock
            .Setup(s => s.SendAccountActionToken(
                It.IsAny<SmtpOptions>(),
                email,
                accountActionToken,
                returnUrl,
                actionType,
                userId))
            .ReturnsAsync(CSharpFunctionalExtensions.Result.Success(true));

        // Act
        var result = await _authenticationService.ManageAccountState(request);

        // Assert
        Assert.True(result.IsSuccess);
        _usersRepositoryMock.Verify(r => r.GetByEmailAsync<UserDto>(email, true), Times.Once);
        _identityServiceMock.Verify(s => s.GenerateAccountActionToken(userId, actionType), Times.Once);
        _emailSmtpServiceMock.Verify(s => s.SendAccountActionToken(
            It.IsAny<SmtpOptions>(), email, accountActionToken, returnUrl, actionType, userId), Times.Once);
    }

    [Fact]
    public async Task ManageAccountState_WithNonExistentUser_ReturnsFailure()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var returnUrl = "https://example.com/confirm";
        var actionType = EstateHub.Authorization.Domain.Models.AccountActionType.Recover;
        var request = new ManageAccountRequest
        {
            Email = email,
            ActionType = actionType,
            ReturnUrl = returnUrl
        };

        _usersRepositoryMock
            .Setup(r => r.GetByEmailAsync<UserDto>(email, true))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _authenticationService.ManageAccountState(request);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.GetByEmailAsync<UserDto>(email, true), Times.Once);
        _identityServiceMock.Verify(s => s.GenerateAccountActionToken(It.IsAny<Guid>(), It.IsAny<EstateHub.Authorization.Domain.Models.AccountActionType>()), Times.Never);
    }

    [Fact]
    public async Task ManageAccountState_WithActiveUser_ReturnsFailure()
    {
        // Arrange
        var email = "active@example.com";
        var userId = Guid.NewGuid();
        var returnUrl = "https://example.com/confirm";
        var actionType = EstateHub.Authorization.Domain.Models.AccountActionType.Recover;
        var request = new ManageAccountRequest
        {
            Email = email,
            ActionType = actionType,
            ReturnUrl = returnUrl
        };

        var activeUser = new UserDto
        {
            Id = userId,
            Email = email,
            UserName = email,
            DisplayName = "Active User",
            IsDeleted = false // Not deleted
        };

        _usersRepositoryMock
            .Setup(r => r.GetByEmailAsync<UserDto>(email, true))
            .ReturnsAsync(activeUser);

        // Act
        var result = await _authenticationService.ManageAccountState(request);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.GetByEmailAsync<UserDto>(email, true), Times.Once);
        _identityServiceMock.Verify(s => s.GenerateAccountActionToken(It.IsAny<Guid>(), It.IsAny<EstateHub.Authorization.Domain.Models.AccountActionType>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmAccountAction_WithValidToken_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "valid-account-action-token";
        var actionType = EstateHub.Authorization.Domain.Models.AccountActionType.Recover;
        var request = new ConfirmAccountActionRequest
        {
            UserId = userId,
            Token = token,
            ActionType = actionType
        };

        var deletedUser = new UserDto
        {
            Id = userId,
            Email = "deleted@example.com",
            UserName = "deleted@example.com",
            DisplayName = "Deleted User",
            IsDeleted = true
        };

        _usersRepositoryMock
            .Setup(r => r.GetByIdAsync<UserDto>(userId, true))
            .ReturnsAsync(deletedUser);

        _identityServiceMock
            .Setup(s => s.ConfirmAccountAction(userId, token, actionType))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authenticationService.ConfirmAccountAction(request);

        // Assert
        Assert.True(result.IsSuccess);
        _usersRepositoryMock.Verify(r => r.GetByIdAsync<UserDto>(userId, true), Times.Once);
        _identityServiceMock.Verify(s => s.ConfirmAccountAction(userId, token, actionType), Times.Once);
    }

    [Fact]
    public async Task ConfirmAccountAction_WithNonExistentUser_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "valid-token";
        var actionType = EstateHub.Authorization.Domain.Models.AccountActionType.Recover;
        var request = new ConfirmAccountActionRequest
        {
            UserId = userId,
            Token = token,
            ActionType = actionType
        };

        _usersRepositoryMock
            .Setup(r => r.GetByIdAsync<UserDto>(userId, true))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _authenticationService.ConfirmAccountAction(request);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.GetByIdAsync<UserDto>(userId, true), Times.Once);
        _identityServiceMock.Verify(s => s.ConfirmAccountAction(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<EstateHub.Authorization.Domain.Models.AccountActionType>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmAccountAction_WithActiveUser_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "valid-token";
        var actionType = EstateHub.Authorization.Domain.Models.AccountActionType.Recover;
        var request = new ConfirmAccountActionRequest
        {
            UserId = userId,
            Token = token,
            ActionType = actionType
        };

        var activeUser = new UserDto
        {
            Id = userId,
            Email = "active@example.com",
            UserName = "active@example.com",
            DisplayName = "Active User",
            IsDeleted = false // Not deleted
        };

        _usersRepositoryMock
            .Setup(r => r.GetByIdAsync<UserDto>(userId, true))
            .ReturnsAsync(activeUser);

        // Act
        var result = await _authenticationService.ConfirmAccountAction(request);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.GetByIdAsync<UserDto>(userId, true), Times.Once);
        _identityServiceMock.Verify(s => s.ConfirmAccountAction(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<EstateHub.Authorization.Domain.Models.AccountActionType>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmAccountAction_WithInvalidToken_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "invalid-token";
        var actionType = EstateHub.Authorization.Domain.Models.AccountActionType.Recover;
        var request = new ConfirmAccountActionRequest
        {
            UserId = userId,
            Token = token,
            ActionType = actionType
        };

        var deletedUser = new UserDto
        {
            Id = userId,
            Email = "deleted@example.com",
            UserName = "deleted@example.com",
            DisplayName = "Deleted User",
            IsDeleted = true
        };

        _usersRepositoryMock
            .Setup(r => r.GetByIdAsync<UserDto>(userId, true))
            .ReturnsAsync(deletedUser);

        _identityServiceMock
            .Setup(s => s.ConfirmAccountAction(userId, token, actionType))
            .ThrowsAsync(new InvalidOperationException("Invalid token"));

        // Act
        var result = await _authenticationService.ConfirmAccountAction(request);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.GetByIdAsync<UserDto>(userId, true), Times.Once);
        _identityServiceMock.Verify(s => s.ConfirmAccountAction(userId, token, actionType), Times.Once);
    }
}

