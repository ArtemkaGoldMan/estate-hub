using EstateHub.ListingService.Core.Mappers;
using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Enums;
using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Domain.Models;
using EstateHub.SharedKernel.Execution;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EstateHub.ListingService.Core.Tests;

public class ListingServiceTests
{
    private readonly Mock<ILogger<EstateHub.ListingService.Core.UseCases.ListingService>> _loggerMock;
    private readonly Mock<IListingRepository> _listingRepositoryMock;
    private readonly Mock<ILikedListingRepository> _likedListingRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IValidator<CreateListingInput>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateListingInput>> _updateValidatorMock;
    private readonly Mock<IValidator<ChangeStatusInput>> _statusValidatorMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ListingDtoMapper _dtoMapper;
    private readonly EstateHub.ListingService.Core.UseCases.ListingService _listingService;

    public ListingServiceTests()
    {
        _loggerMock = new Mock<ILogger<EstateHub.ListingService.Core.UseCases.ListingService>>();
        _listingRepositoryMock = new Mock<IListingRepository>();
        _likedListingRepositoryMock = new Mock<ILikedListingRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _createValidatorMock = new Mock<IValidator<CreateListingInput>>();
        _updateValidatorMock = new Mock<IValidator<UpdateListingInput>>();
        _statusValidatorMock = new Mock<IValidator<ChangeStatusInput>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        // Setup unit of work to return success
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(CSharpFunctionalExtensions.Result.Success(true));
        _unitOfWorkMock.Setup(u => u.CommitAsync())
            .ReturnsAsync(CSharpFunctionalExtensions.Result.Success(true));

        // Setup DTO mapper
        _dtoMapper = new ListingDtoMapper(_likedListingRepositoryMock.Object);

        // Setup validators to return success by default
        _createValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateListingInput>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _updateValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateListingInput>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _statusValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ChangeStatusInput>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _listingService = new EstateHub.ListingService.Core.UseCases.ListingService(
            _listingRepositoryMock.Object,
            _likedListingRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
            _statusValidatorMock.Object,
            _dtoMapper,
            _loggerMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task CreateAsync_WithValidInput_ReturnsListingId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var input = new CreateListingInput(
            ListingCategory.Sale,
            PropertyType.Apartment,
            "Beautiful Apartment",
            "A lovely apartment in the city center",
            "123 Main St",
            "Downtown",
            "Warsaw",
            "00-001",
            52.2297m,
            21.0122m,
            75.5m,
            3,
            null,
            null,
            null,
            Condition.Good,
            true,
            false,
            true,
            false,
            false,
            500000m,
            null
        );

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _listingRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Listing>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _listingService.CreateAsync(input);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        _currentUserServiceMock.Verify(s => s.GetUserId(), Times.Once);
        _createValidatorMock.Verify(v => v.ValidateAsync(input, It.IsAny<System.Threading.CancellationToken>()), Times.Once);
        _listingRepositoryMock.Verify(r => r.AddAsync(It.Is<Listing>(l => 
            l.OwnerId == userId && 
            l.Title == "Beautiful Apartment" &&
            l.Category == ListingCategory.Sale
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_AsOwner_UpdatesListing()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var existingListing = new Listing(
            userId,
            ListingCategory.Sale,
            PropertyType.Apartment,
            "Original Title",
            "Original Description",
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

        var updateInput = new UpdateListingInput(
            Title: "Updated Title",
            Description: null,
            AddressLine: null,
            District: null,
            City: null,
            PostalCode: null,
            Latitude: null,
            Longitude: null,
            SquareMeters: null,
            Rooms: null,
            Floor: null,
            FloorCount: null,
            BuildYear: null,
            Condition: null,
            HasBalcony: null,
            HasElevator: null,
            HasParkingSpace: null,
            HasSecurity: null,
            HasStorageRoom: null,
            PricePln: null,
            MonthlyRentPln: null
        );

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(existingListing);

        _listingRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Listing>()))
            .Returns(Task.CompletedTask);

        // Act
        await _listingService.UpdateAsync(listingId, updateInput);

        // Assert
        _listingRepositoryMock.Verify(r => r.GetByIdAsync(listingId), Times.Once);
        _listingRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Listing>(l => 
            l.OwnerId == userId &&
            l.Title == "Updated Title"
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_AsNonOwner_ThrowsError()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var existingListing = new Listing(
            ownerId,
            ListingCategory.Sale,
            PropertyType.Apartment,
            "Original Title",
            "Original Description",
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

        var updateInput = new UpdateListingInput(
            Title: "Updated Title",
            Description: null,
            AddressLine: null,
            District: null,
            City: null,
            PostalCode: null,
            Latitude: null,
            Longitude: null,
            SquareMeters: null,
            Rooms: null,
            Floor: null,
            FloorCount: null,
            BuildYear: null,
            Condition: null,
            HasBalcony: null,
            HasElevator: null,
            HasParkingSpace: null,
            HasSecurity: null,
            HasStorageRoom: null,
            PricePln: null,
            MonthlyRentPln: null
        );

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(otherUserId);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(existingListing);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _listingService.UpdateAsync(listingId, updateInput));

        _listingRepositoryMock.Verify(r => r.GetByIdAsync(listingId), Times.Once);
        _listingRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Listing>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_AsOwner_DeletesListing()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var existingListing = new Listing(
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

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(existingListing);

        _listingRepositoryMock
            .Setup(r => r.DeleteAsync(listingId))
            .Returns(Task.CompletedTask);

        // Act
        await _listingService.DeleteAsync(listingId);

        // Assert
        _listingRepositoryMock.Verify(r => r.GetByIdAsync(listingId), Times.Once);
        _listingRepositoryMock.Verify(r => r.DeleteAsync(listingId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithPublishedListing_ReturnsListing()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var listing = new Listing(
            ownerId,
            ListingCategory.Sale,
            PropertyType.Apartment,
            "Published Listing",
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

        // Set status to Published using record with expression
        var listingWithStatus = listing with { Status = ListingStatus.Published };

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Throws<UnauthorizedAccessException>(); // Not authenticated

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(listingWithStatus);

        _likedListingRepositoryMock
            .Setup(r => r.GetLikedByUserAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<Listing>());

        // Act
        var result = await _listingService.GetByIdAsync(listingId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(listingWithStatus.Id, result.Id);
        Assert.Equal("Published Listing", result.Title);
        _listingRepositoryMock.Verify(r => r.GetByIdAsync(listingId), Times.Once);
    }
}

