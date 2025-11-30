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
}

