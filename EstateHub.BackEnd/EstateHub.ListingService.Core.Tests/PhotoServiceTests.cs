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

    [Fact]
    public async Task AddPhotoAsync_WithEmptyUrl_ThrowsError()
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

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _photoService.AddPhotoAsync(listingId, string.Empty));
        
        _photoRepositoryMock.Verify(r => r.AddPhotoAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AddPhotoAsync_WithInvalidUrl_ThrowsError()
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

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _photoService.AddPhotoAsync(listingId, "not-a-valid-url"));
        
        _photoRepositoryMock.Verify(r => r.AddPhotoAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AddPhotoAsync_WithNonExistentListing_ThrowsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var photoUrl = "https://example.com/photo.jpg";

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync((Listing?)null);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _photoService.AddPhotoAsync(listingId, photoUrl));
        
        _photoRepositoryMock.Verify(r => r.AddPhotoAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UploadPhotoAsync_WithInvalidFile_ThrowsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
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

        var validationResult = new FileValidationResult(IsValid: false, ErrorMessage: "File too large");

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(listing);

        _photoStorageServiceMock
            .Setup(s => s.ValidateFileAsync(fileStream, fileName, contentType))
            .ReturnsAsync(validationResult);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _photoService.UploadPhotoAsync(listingId, fileStream, fileName, contentType));
        
        _photoStorageServiceMock.Verify(s => s.UploadPhotoAsync(It.IsAny<Guid>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _photoRepositoryMock.Verify(r => r.AddPhotoAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RemovePhotoAsync_WithNonExistentPhoto_ThrowsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var photoId = Guid.NewGuid();

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

        _photoRepositoryMock
            .Setup(r => r.GetByIdAsync(photoId))
            .ReturnsAsync((ListingPhoto?)null);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _photoService.RemovePhotoAsync(listingId, photoId));
        
        _photoStorageServiceMock.Verify(s => s.DeletePhotoAsync(It.IsAny<string>()), Times.Never);
        _photoRepositoryMock.Verify(r => r.RemovePhotoAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task RemovePhotoAsync_WithPhotoFromDifferentListing_ThrowsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var otherListingId = Guid.NewGuid();
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

        var photo = new ListingPhoto(otherListingId, photoUrl, 0);

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(listing);

        _photoRepositoryMock
            .Setup(r => r.GetByIdAsync(photoId))
            .ReturnsAsync(photo);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _photoService.RemovePhotoAsync(listingId, photoId));
        
        _photoStorageServiceMock.Verify(s => s.DeletePhotoAsync(It.IsAny<string>()), Times.Never);
        _photoRepositoryMock.Verify(r => r.RemovePhotoAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ReorderPhotosAsync_WithValidOrder_Succeeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var photo1Id = Guid.NewGuid();
        var photo2Id = Guid.NewGuid();
        var photo3Id = Guid.NewGuid();

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

        var photos = new List<ListingPhoto>
        {
            new ListingPhoto(listingId, "https://example.com/photo1.jpg", 0) { Id = photo1Id },
            new ListingPhoto(listingId, "https://example.com/photo2.jpg", 1) { Id = photo2Id },
            new ListingPhoto(listingId, "https://example.com/photo3.jpg", 2) { Id = photo3Id }
        };

        var orderedPhotoIds = new List<Guid> { photo3Id, photo1Id, photo2Id };

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(listing);

        _photoRepositoryMock
            .Setup(r => r.GetPhotosByListingIdAsync(listingId))
            .ReturnsAsync(photos);

        _photoRepositoryMock
            .Setup(r => r.ReorderPhotosAsync(listingId, orderedPhotoIds))
            .Returns(Task.CompletedTask);

        // Act
        await _photoService.ReorderPhotosAsync(listingId, orderedPhotoIds);

        // Assert
        _photoRepositoryMock.Verify(r => r.ReorderPhotosAsync(listingId, orderedPhotoIds), Times.Once);
    }

    [Fact]
    public async Task ReorderPhotosAsync_WithEmptyOrder_ThrowsError()
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

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _photoService.ReorderPhotosAsync(listingId, new List<Guid>()));
        
        _photoRepositoryMock.Verify(r => r.ReorderPhotosAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>()), Times.Never);
    }

    [Fact]
    public async Task ReorderPhotosAsync_WithInvalidPhotoId_ThrowsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var photo1Id = Guid.NewGuid();
        var invalidPhotoId = Guid.NewGuid();

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

        var photos = new List<ListingPhoto>
        {
            new ListingPhoto(listingId, "https://example.com/photo1.jpg", 0) { Id = photo1Id }
        };

        var orderedPhotoIds = new List<Guid> { photo1Id, invalidPhotoId };

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(listing);

        _photoRepositoryMock
            .Setup(r => r.GetPhotosByListingIdAsync(listingId))
            .ReturnsAsync(photos);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _photoService.ReorderPhotosAsync(listingId, orderedPhotoIds));
        
        _photoRepositoryMock.Verify(r => r.ReorderPhotosAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>()), Times.Never);
    }

    [Fact]
    public async Task GetPhotosAsync_ReturnsAllPhotos()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        var photos = new List<ListingPhoto>
        {
            new ListingPhoto(listingId, "https://example.com/photo1.jpg", 0),
            new ListingPhoto(listingId, "https://example.com/photo2.jpg", 1)
        };

        _photoRepositoryMock
            .Setup(r => r.GetPhotosByListingIdAsync(listingId))
            .ReturnsAsync(photos);

        // Act
        var result = await _photoService.GetPhotosAsync(listingId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        _photoRepositoryMock.Verify(r => r.GetPhotosByListingIdAsync(listingId), Times.Once);
    }

    [Fact]
    public async Task GetPhotoAsync_WithExistingPhoto_ReturnsPhoto()
    {
        // Arrange
        var photoId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var photo = new ListingPhoto(listingId, "https://example.com/photo.jpg", 0) { Id = photoId };

        _photoRepositoryMock
            .Setup(r => r.GetByIdAsync(photoId))
            .ReturnsAsync(photo);

        // Act
        var result = await _photoService.GetPhotoAsync(photoId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(photoId, result.Id);
        Assert.Equal(listingId, result.ListingId);
    }

    [Fact]
    public async Task GetPhotoAsync_WithNonExistentPhoto_ReturnsNull()
    {
        // Arrange
        var photoId = Guid.NewGuid();

        _photoRepositoryMock
            .Setup(r => r.GetByIdAsync(photoId))
            .ReturnsAsync((ListingPhoto?)null);

        // Act
        var result = await _photoService.GetPhotoAsync(photoId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPhotoStreamAsync_ReturnsStream()
    {
        // Arrange
        var photoUrl = "https://example.com/photo.jpg";
        var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
        var contentType = "image/jpeg";
        var fileName = "photo.jpg";

        _photoStorageServiceMock
            .Setup(s => s.GetPhotoStreamAsync(photoUrl))
            .ReturnsAsync((stream, contentType, fileName));

        // Act
        var result = await _photoService.GetPhotoStreamAsync(photoUrl);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(stream, result.Value.Stream);
        Assert.Equal(contentType, result.Value.ContentType);
        Assert.Equal(fileName, result.Value.FileName);
    }
}

