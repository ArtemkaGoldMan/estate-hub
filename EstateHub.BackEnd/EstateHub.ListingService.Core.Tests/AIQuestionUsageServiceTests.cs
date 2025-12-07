using EstateHub.ListingService.Core.Services;
using EstateHub.ListingService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EstateHub.ListingService.Core.Tests;

public class AIQuestionUsageServiceTests
{
    private readonly Mock<ILogger<AIQuestionUsageService>> _loggerMock;
    private readonly Mock<IAIQuestionUsageRepository> _repositoryMock;
    private readonly AIQuestionUsageService _service;

    public AIQuestionUsageServiceTests()
    {
        _loggerMock = new Mock<ILogger<AIQuestionUsageService>>();
        _repositoryMock = new Mock<IAIQuestionUsageRepository>();

        _service = new AIQuestionUsageService(
            _repositoryMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task CheckAndIncrementUsageAsync_WithAvailableQuota_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentCount = 2; // Below limit of 5

        _repositoryMock
            .Setup(r => r.GetTodayQuestionCountAsync(userId))
            .ReturnsAsync(currentCount);

        _repositoryMock
            .Setup(r => r.IncrementQuestionCountAsync(userId))
            .Returns(Task.CompletedTask);

        // Act
        var (canAsk, remainingCount) = await _service.CheckAndIncrementUsageAsync(userId);

        // Assert
        Assert.True(canAsk);
        Assert.Equal(2, remainingCount); // 5 - 2 - 1 = 2
        _repositoryMock.Verify(r => r.IncrementQuestionCountAsync(userId), Times.Once);
    }

    [Fact]
    public async Task CheckAndIncrementUsageAsync_AtLimit_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentCount = 5; // At limit

        _repositoryMock
            .Setup(r => r.GetTodayQuestionCountAsync(userId))
            .ReturnsAsync(currentCount);

        // Act
        var (canAsk, remainingCount) = await _service.CheckAndIncrementUsageAsync(userId);

        // Assert
        Assert.False(canAsk);
        Assert.Equal(0, remainingCount);
        _repositoryMock.Verify(r => r.IncrementQuestionCountAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task CheckAndIncrementUsageAsync_AboveLimit_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentCount = 10; // Above limit

        _repositoryMock
            .Setup(r => r.GetTodayQuestionCountAsync(userId))
            .ReturnsAsync(currentCount);

        // Act
        var (canAsk, remainingCount) = await _service.CheckAndIncrementUsageAsync(userId);

        // Assert
        Assert.False(canAsk);
        Assert.Equal(0, remainingCount);
        _repositoryMock.Verify(r => r.IncrementQuestionCountAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task CheckAndIncrementUsageAsync_WithZeroCount_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentCount = 0;

        _repositoryMock
            .Setup(r => r.GetTodayQuestionCountAsync(userId))
            .ReturnsAsync(currentCount);

        _repositoryMock
            .Setup(r => r.IncrementQuestionCountAsync(userId))
            .Returns(Task.CompletedTask);

        // Act
        var (canAsk, remainingCount) = await _service.CheckAndIncrementUsageAsync(userId);

        // Assert
        Assert.True(canAsk);
        Assert.Equal(4, remainingCount); // 5 - 0 - 1 = 4
        _repositoryMock.Verify(r => r.IncrementQuestionCountAsync(userId), Times.Once);
    }

    [Fact]
    public async Task CheckAndIncrementUsageAsync_WithOneRemaining_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentCount = 4; // One remaining

        _repositoryMock
            .Setup(r => r.GetTodayQuestionCountAsync(userId))
            .ReturnsAsync(currentCount);

        _repositoryMock
            .Setup(r => r.IncrementQuestionCountAsync(userId))
            .Returns(Task.CompletedTask);

        // Act
        var (canAsk, remainingCount) = await _service.CheckAndIncrementUsageAsync(userId);

        // Assert
        Assert.True(canAsk);
        Assert.Equal(0, remainingCount); // 5 - 4 - 1 = 0
        _repositoryMock.Verify(r => r.IncrementQuestionCountAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetRemainingCountAsync_WithZeroCount_ReturnsFive()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentCount = 0;

        _repositoryMock
            .Setup(r => r.GetTodayQuestionCountAsync(userId))
            .ReturnsAsync(currentCount);

        // Act
        var remaining = await _service.GetRemainingCountAsync(userId);

        // Assert
        Assert.Equal(5, remaining);
    }

    [Fact]
    public async Task GetRemainingCountAsync_WithThreeCount_ReturnsTwo()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentCount = 3;

        _repositoryMock
            .Setup(r => r.GetTodayQuestionCountAsync(userId))
            .ReturnsAsync(currentCount);

        // Act
        var remaining = await _service.GetRemainingCountAsync(userId);

        // Assert
        Assert.Equal(2, remaining);
    }

    [Fact]
    public async Task GetRemainingCountAsync_AtLimit_ReturnsZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentCount = 5;

        _repositoryMock
            .Setup(r => r.GetTodayQuestionCountAsync(userId))
            .ReturnsAsync(currentCount);

        // Act
        var remaining = await _service.GetRemainingCountAsync(userId);

        // Assert
        Assert.Equal(0, remaining);
    }

    [Fact]
    public async Task GetRemainingCountAsync_AboveLimit_ReturnsZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentCount = 10;

        _repositoryMock
            .Setup(r => r.GetTodayQuestionCountAsync(userId))
            .ReturnsAsync(currentCount);

        // Act
        var remaining = await _service.GetRemainingCountAsync(userId);

        // Assert
        Assert.Equal(0, remaining);
    }

    [Fact]
    public async Task CheckAndIncrementUsageAsync_WithDifferentUsers_IndependentQuotas()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetTodayQuestionCountAsync(userId1))
            .ReturnsAsync(4);

        _repositoryMock
            .Setup(r => r.GetTodayQuestionCountAsync(userId2))
            .ReturnsAsync(1);

        _repositoryMock
            .Setup(r => r.IncrementQuestionCountAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        // Act
        var (canAsk1, remaining1) = await _service.CheckAndIncrementUsageAsync(userId1);
        var (canAsk2, remaining2) = await _service.CheckAndIncrementUsageAsync(userId2);

        // Assert
        Assert.True(canAsk1);
        Assert.Equal(0, remaining1);
        Assert.True(canAsk2);
        Assert.Equal(3, remaining2);
    }
}

