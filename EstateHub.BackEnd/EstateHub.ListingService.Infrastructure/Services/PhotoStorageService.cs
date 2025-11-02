using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace EstateHub.ListingService.Infrastructure.Services;

/// <summary>
/// Photo storage service that wraps MongoGridFSStorageService.
/// Implements IPhotoStorageService interface to provide abstraction.
/// </summary>
public class PhotoStorageService : IPhotoStorageService
{
    private readonly MongoGridFSStorageService _gridFSService;
    private readonly ILogger<PhotoStorageService> _logger;

    public PhotoStorageService(
        MongoGridFSStorageService gridFSService,
        ILogger<PhotoStorageService> logger)
    {
        _gridFSService = gridFSService;
        _logger = logger;
    }

    public async Task<string> UploadPhotoAsync(Guid listingId, Stream fileStream, string fileName, string contentType)
    {
        return await _gridFSService.UploadPhotoAsync(listingId, fileStream, fileName, contentType);
    }

    public async Task DeletePhotoAsync(string photoUrl)
    {
        await _gridFSService.DeletePhotoAsync(photoUrl);
    }

    public async Task<(Stream Stream, string ContentType, string FileName)?> GetPhotoStreamAsync(string photoUrl)
    {
        // Extract file ID from GridFS URL
        if (!photoUrl.StartsWith("/api/photo/gridfs/"))
        {
            // Not a GridFS URL, return null (could be external URL or legacy file)
            return null;
        }

        var fileIdString = photoUrl.Split('/').Last();
        if (!ObjectId.TryParse(fileIdString, out var fileId))
        {
            _logger.LogWarning("Invalid GridFS file ID in URL: {PhotoUrl}", photoUrl);
            return null;
        }

        return await _gridFSService.GetFileStreamAsync(fileId);
    }

    public async Task<FileValidationResult> ValidateFileAsync(Stream fileStream, string fileName, string contentType)
    {
        return await _gridFSService.ValidateFileAsync(fileStream, fileName, contentType);
    }
}

