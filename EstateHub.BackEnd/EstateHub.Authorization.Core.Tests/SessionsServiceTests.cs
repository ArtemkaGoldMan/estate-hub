using CSharpFunctionalExtensions;
using EstateHub.Authorization.Core.Services;
using EstateHub.Authorization.Domain.DTO.Session;
using EstateHub.Authorization.Domain.Errors;
using EstateHub.Authorization.Domain.Interfaces.ApplicationInterfaces;
using EstateHub.Authorization.Domain.Interfaces.DataAccessInterfaces;
using EstateHub.SharedKernel.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EstateHub.Authorization.Core.Tests;

public class SessionsServiceTests
{
    private readonly Mock<ISessionsRepository> _sessionsRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<SessionsService>> _loggerMock;
    private readonly SessionsService _sessionsService;

    public SessionsServiceTests()
    {
        _sessionsRepositoryMock = new Mock<ISessionsRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<SessionsService>>();

        // Setup unit of work to return success
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(Result.Success(true));
        _unitOfWorkMock.Setup(u => u.CommitAsync())
            .ReturnsAsync(Result.Success(true));

        _sessionsService = new SessionsService(
            _sessionsRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object
        );
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = new SessionDto
        {
            Id = sessionId,
            UserId = userId,
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            ExpirationDate = DateTimeOffset.UtcNow.AddMonths(1)
        };

        _sessionsRepositoryMock
            .Setup(r => r.GetByIdAsync<SessionDto>(sessionId))
            .ReturnsAsync(session);

        // Act
        var result = await _sessionsService.GetByIdAsync<SessionDto>(sessionId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(sessionId, result.Value.Id);
        Assert.Equal(userId, result.Value.UserId);
        _sessionsRepositoryMock.Verify(r => r.GetByIdAsync<SessionDto>(sessionId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyGuid_ReturnsFailure()
    {
        // Arrange
        var emptyId = Guid.Empty;

        // Act
        var result = await _sessionsService.GetByIdAsync<SessionDto>(emptyId);

        // Assert
        Assert.True(result.IsFailure);
        _sessionsRepositoryMock.Verify(r => r.GetByIdAsync<SessionDto>(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsFailure()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        _sessionsRepositoryMock
            .Setup(r => r.GetByIdAsync<SessionDto>(sessionId))
            .ReturnsAsync((SessionDto?)null);

        // Act
        var result = await _sessionsService.GetByIdAsync<SessionDto>(sessionId);

        // Assert
        Assert.True(result.IsFailure);
        _sessionsRepositoryMock.Verify(r => r.GetByIdAsync<SessionDto>(sessionId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithExpiredSession_StillReturnsSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var expiredSession = new SessionDto
        {
            Id = sessionId,
            UserId = userId,
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            ExpirationDate = DateTimeOffset.UtcNow.AddDays(-1) // Expired
        };

        _sessionsRepositoryMock
            .Setup(r => r.GetByIdAsync<SessionDto>(sessionId))
            .ReturnsAsync(expiredSession);

        // Act
        var result = await _sessionsService.GetByIdAsync<SessionDto>(sessionId);

        // Assert
        // Note: Service doesn't check expiration, it just returns what repository gives
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.True(result.Value.ExpirationDate < DateTimeOffset.UtcNow);
        _sessionsRepositoryMock.Verify(r => r.GetByIdAsync<SessionDto>(sessionId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidSession_ReturnsCorrectData()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var accessToken = "test-access-token";
        var refreshToken = "test-refresh-token";
        var expirationDate = DateTimeOffset.UtcNow.AddMonths(1);

        var session = new SessionDto
        {
            Id = sessionId,
            UserId = userId,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpirationDate = expirationDate
        };

        _sessionsRepositoryMock
            .Setup(r => r.GetByIdAsync<SessionDto>(sessionId))
            .ReturnsAsync(session);

        // Act
        var result = await _sessionsService.GetByIdAsync<SessionDto>(sessionId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(sessionId, result.Value.Id);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(accessToken, result.Value.AccessToken);
        Assert.Equal(refreshToken, result.Value.RefreshToken);
        Assert.Equal(expirationDate, result.Value.ExpirationDate);
    }

    #endregion
}


