using System.Threading;
using EstateHub.ListingService.Core.Services;
using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Enums;
using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Domain.Models;
using EstateHub.ListingService.Domain.Errors;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EstateHub.ListingService.Core.Tests;

public class ModerationServiceTests
{
    private readonly Mock<ILogger<ModerationService>> _loggerMock;
    private readonly Mock<IListingRepository> _listingRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IContentModerationService> _contentModerationServiceMock;
    private readonly ModerationService _moderationService;

    public ModerationServiceTests()
    {
        _loggerMock = new Mock<ILogger<ModerationService>>();
        _listingRepositoryMock = new Mock<IListingRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _contentModerationServiceMock = new Mock<IContentModerationService>();

        _moderationService = new ModerationService(
            _listingRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _contentModerationServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task CheckModerationAsync_WithApprovedContent_UpdatesListing()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var listing = new Listing(
            userId,
            ListingCategory.Sale,
            PropertyType.Apartment,
            "Test Listing",
            "Test Description",
            "123 Main St",
            "Downtown",
            "Warsaw",
            "00-001",
            52.2297m,
            21.0122m,
            75.5m,
            3,
            Condition.Good,
            false,
            false,
            false,
            false,
            false,
            null,
            null,
            null,
            500000m,
            null
        );

        var moderationResult = new ModerationResult(IsApproved: true, RejectionReason: null, Suggestions: null);

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(listing);

        _contentModerationServiceMock
            .Setup(s => s.ModerateAsync(listing.Title, listing.Description, It.IsAny<CancellationToken>()))
            .ReturnsAsync(moderationResult);

        _listingRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Listing>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _moderationService.CheckModerationAsync(listingId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsApproved);
        Assert.Null(result.RejectionReason);
        _listingRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Listing>(l => 
            l.IsModerationApproved == true &&
            l.ModerationCheckedAt != null
        )), Times.Once);
    }

    [Fact]
    public async Task CheckModerationAsync_WithRejectedContent_UpdatesListing()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var listing = new Listing(
            userId,
            ListingCategory.Sale,
            PropertyType.Apartment,
            "Spam Listing",
            "Buy now!",
            "123 Main St",
            "Downtown",
            "Warsaw",
            "00-001",
            52.2297m,
            21.0122m,
            75.5m,
            3,
            Condition.Good,
            false,
            false,
            false,
            false,
            false,
            null,
            null,
            null,
            500000m,
            null
        );

        var moderationResult = new ModerationResult(
            IsApproved: false, 
            RejectionReason: "Content violates guidelines",
            Suggestions: null
        );

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(listing);

        _contentModerationServiceMock
            .Setup(s => s.ModerateAsync(listing.Title, listing.Description, It.IsAny<CancellationToken>()))
            .ReturnsAsync(moderationResult);

        _listingRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Listing>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _moderationService.CheckModerationAsync(listingId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsApproved);
        Assert.Equal("Content violates guidelines", result.RejectionReason);
        _listingRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Listing>(l => 
            l.IsModerationApproved == false &&
            l.ModerationRejectionReason == "Content violates guidelines" &&
            l.ModerationCheckedAt != null
        )), Times.Once);
    }

    [Fact]
    public async Task CheckModerationAsync_WithNonExistentListing_ThrowsError()
    {
        // Arrange
        var listingId = Guid.NewGuid();

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync((Listing?)null);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _moderationService.CheckModerationAsync(listingId));

        _contentModerationServiceMock.Verify(s => s.ModerateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _listingRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Listing>()), Times.Never);
    }

    [Fact]
    public async Task CheckModerationAsync_AsNonOwner_ThrowsError()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var listing = new Listing(
            ownerId,
            ListingCategory.Sale,
            PropertyType.Apartment,
            "Test Listing",
            "Description",
            "123 Main St",
            "Downtown",
            "Warsaw",
            "00-001",
            52.2297m,
            21.0122m,
            75.5m,
            3,
            Condition.Good,
            false,
            false,
            false,
            false,
            false,
            null,
            null,
            null,
            500000m,
            null
        );

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(otherUserId);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(listing);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _moderationService.CheckModerationAsync(listingId));

        _contentModerationServiceMock.Verify(s => s.ModerateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _listingRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Listing>()), Times.Never);
    }

    [Fact]
    public async Task CheckModerationAsync_WithoutUserContext_AllowsBackgroundTask()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var listing = new Listing(
            ownerId,
            ListingCategory.Sale,
            PropertyType.Apartment,
            "Test Listing",
            "Description",
            "123 Main St",
            "Downtown",
            "Warsaw",
            "00-001",
            52.2297m,
            21.0122m,
            75.5m,
            3,
            Condition.Good,
            false,
            false,
            false,
            false,
            false,
            null,
            null,
            null,
            500000m,
            null
        );

        var moderationResult = new ModerationResult(IsApproved: true, RejectionReason: null, Suggestions: null);

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Throws<UnauthorizedAccessException>(); // No user context (background task)

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(listing);

        _contentModerationServiceMock
            .Setup(s => s.ModerateAsync(listing.Title, listing.Description, It.IsAny<CancellationToken>()))
            .ReturnsAsync(moderationResult);

        _listingRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Listing>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _moderationService.CheckModerationAsync(listingId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsApproved);
        _listingRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Listing>()), Times.Once);
    }

    [Fact]
    public async Task CheckModerationAsync_WithEmptyTitle_StillProcesses()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var listing = new Listing(
            userId,
            ListingCategory.Sale,
            PropertyType.Apartment,
            "Test Listing",
            "Description",
            "123 Main St",
            "Downtown",
            "Warsaw",
            "00-001",
            52.2297m,
            21.0122m,
            75.5m,
            3,
            Condition.Good,
            false,
            false,
            false,
            false,
            false,
            null,
            null,
            null,
            500000m,
            null
        );

        var moderationResult = new ModerationResult(IsApproved: true, RejectionReason: null, Suggestions: null);

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(listing);

        _contentModerationServiceMock
            .Setup(s => s.ModerateAsync(listing.Title, listing.Description, It.IsAny<CancellationToken>()))
            .ReturnsAsync(moderationResult);

        _listingRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Listing>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _moderationService.CheckModerationAsync(listingId);

        // Assert
        Assert.NotNull(result);
        _contentModerationServiceMock.Verify(s => s.ModerateAsync(listing.Title, listing.Description, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckModerationAsync_WithContentModerationError_PropagatesError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var listing = new Listing(
            userId,
            ListingCategory.Sale,
            PropertyType.Apartment,
            "Test Listing",
            "Description",
            "123 Main St",
            "Downtown",
            "Warsaw",
            "00-001",
            52.2297m,
            21.0122m,
            75.5m,
            3,
            Condition.Good,
            false,
            false,
            false,
            false,
            false,
            null,
            null,
            null,
            500000m,
            null
        );

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(listing);

        _contentModerationServiceMock
            .Setup(s => s.ModerateAsync(listing.Title, listing.Description, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Moderation service unavailable"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _moderationService.CheckModerationAsync(listingId));

        _listingRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Listing>()), Times.Never);
    }
}

