using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Enums;
using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Domain.Models;
using EstateHub.SharedKernel.Execution;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EstateHub.ListingService.Core.Tests;

public class PhotoServiceTests
{
    private readonly Mock<ILogger<EstateHub.ListingService.Core.Services.PhotoService>> _loggerMock;
    private readonly Mock<IPhotoRepository> _photoRepositoryMock;
    private readonly Mock<IListingRepository> _listingRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IPhotoStorageService> _photoStorageServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly EstateHub.ListingService.Core.Services.PhotoService _photoService;

    public PhotoServiceTests()
    {
        _loggerMock = new Mock<ILogger<EstateHub.ListingService.Core.Services.PhotoService>>();
        _photoRepositoryMock = new Mock<IPhotoRepository>();
        _listingRepositoryMock = new Mock<IListingRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _photoStorageServiceMock = new Mock<IPhotoStorageService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        // Setup unit of work to return success
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(CSharpFunctionalExtensions.Result.Success(true));
        _unitOfWorkMock.Setup(u => u.CommitAsync())
            .ReturnsAsync(CSharpFunctionalExtensions.Result.Success(true));

        _photoService = new EstateHub.ListingService.Core.Services.PhotoService(
            _photoRepositoryMock.Object,
            _listingRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _photoStorageServiceMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task AddPhotoAsync_AsOwner_ReturnsPhotoId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var photoId = Guid.NewGuid();
        var photoUrl = "https://example.com/photo.jpg";

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

        var photo = new ListingPhoto(listingId, photoUrl, 0);

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(listing);

        _photoRepositoryMock
            .Setup(r => r.AddPhotoAsync(listingId, photoUrl))
            .ReturnsAsync(photo);

        // Act
        var result = await _photoService.AddPhotoAsync(listingId, photoUrl);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        _listingRepositoryMock.Verify(r => r.GetByIdAsync(listingId), Times.Once);
        _photoRepositoryMock.Verify(r => r.AddPhotoAsync(listingId, photoUrl), Times.Once);
    }

    [Fact]
    public async Task AddPhotoAsync_AsNonOwner_ThrowsError()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var photoUrl = "https://example.com/photo.jpg";

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
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _photoService.AddPhotoAsync(listingId, photoUrl));

        _listingRepositoryMock.Verify(r => r.GetByIdAsync(listingId), Times.Once);
        _photoRepositoryMock.Verify(r => r.AddPhotoAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RemovePhotoAsync_AsOwner_RemovesPhoto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var photoId = Guid.NewGuid();
        var photoUrl = "https://example.com/photo.jpg";

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

        var photo = new ListingPhoto(listingId, photoUrl, 0);

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(listing);

        _photoRepositoryMock
            .Setup(r => r.GetByIdAsync(photoId))
            .ReturnsAsync(photo);

        _photoStorageServiceMock
            .Setup(s => s.DeletePhotoAsync(photoUrl))
            .Returns(Task.CompletedTask);

        _photoRepositoryMock
            .Setup(r => r.RemovePhotoAsync(listingId, photoId))
            .Returns(Task.CompletedTask);

        // Act
        await _photoService.RemovePhotoAsync(listingId, photoId);

        // Assert
        _listingRepositoryMock.Verify(r => r.GetByIdAsync(listingId), Times.Once);
        _photoRepositoryMock.Verify(r => r.GetByIdAsync(photoId), Times.Once);
        _photoStorageServiceMock.Verify(s => s.DeletePhotoAsync(photoUrl), Times.Once);
        _photoRepositoryMock.Verify(r => r.RemovePhotoAsync(listingId, photoId), Times.Once);
    }

    [Fact]
    public async Task UploadPhotoAsync_WithValidFile_ReturnsPhotoId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var photoId = Guid.NewGuid();
        var photoUrl = "https://example.com/uploaded-photo.jpg";
        var fileName = "photo.jpg";
        var contentType = "image/jpeg";
        var fileStream = new MemoryStream(new byte[] { 1, 2, 3, 4 });

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

        var photo = new ListingPhoto(listingId, photoUrl, 0);

        var validationResult = new FileValidationResult(IsValid: true);

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(listing);

        _photoStorageServiceMock
            .Setup(s => s.ValidateFileAsync(fileStream, fileName, contentType))
            .ReturnsAsync(validationResult);

        _photoStorageServiceMock
            .Setup(s => s.UploadPhotoAsync(listingId, fileStream, fileName, contentType))
            .ReturnsAsync(photoUrl);

        _photoRepositoryMock
            .Setup(r => r.AddPhotoAsync(listingId, photoUrl))
            .ReturnsAsync(photo);

        // Act
        var result = await _photoService.UploadPhotoAsync(listingId, fileStream, fileName, contentType);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        _listingRepositoryMock.Verify(r => r.GetByIdAsync(listingId), Times.Once);
        _photoStorageServiceMock.Verify(s => s.ValidateFileAsync(fileStream, fileName, contentType), Times.Once);
        _photoStorageServiceMock.Verify(s => s.UploadPhotoAsync(listingId, fileStream, fileName, contentType), Times.Once);
        _photoRepositoryMock.Verify(r => r.AddPhotoAsync(listingId, photoUrl), Times.Once);
    }
}

