using EstateHub.ListingService.Domain.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace EstateHub.ListingService.Infrastructure.Services;

/// <summary>
/// MongoDB GridFS file storage service for photos.
/// Used internally by PhotoService - not exposed via interface.
/// </summary>
public class MongoGridFSStorageService
{
    private readonly ILogger<MongoGridFSStorageService> _logger;
    private readonly GridFSBucket _gridFSBucket;
    private readonly IMongoDatabase _database;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private readonly string[] _allowedMimeTypes = { "image/jpeg", "image/png", "image/webp" };
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

    public MongoGridFSStorageService(IConfiguration configuration, ILogger<MongoGridFSStorageService> logger)
    {
        _logger = logger;

        var connectionString = configuration["MongoDB:ConnectionString"] 
            ?? throw new InvalidOperationException("MongoDB:ConnectionString is not configured");
        var databaseName = configuration["MongoDB:DatabaseName"] 
            ?? throw new InvalidOperationException("MongoDB:DatabaseName is not configured");
        var bucketName = configuration["MongoDB:GridFSBucket"] ?? "photos";

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
        _gridFSBucket = new GridFSBucket(_database, new GridFSBucketOptions
        {
            BucketName = bucketName
        });

        _logger.LogInformation("MongoDB GridFS initialized with bucket: {BucketName}, database: {DatabaseName}", 
            bucketName, databaseName);
    }

    public async Task<string> UploadPhotoAsync(Guid listingId, Stream fileStream, string fileName, string contentType)
    {
        // Validate file first
        var validation = await ValidateFileAsync(fileStream, fileName, contentType);
        if (!validation.IsValid)
        {
            throw new ArgumentException(validation.ErrorMessage);
        }

        // Reset stream position after validation
        fileStream.Position = 0;

        // Generate unique filename with listing ID prefix
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueFileName = $"{listingId}/{Guid.NewGuid()}{fileExtension}";

        // Upload metadata
        var metadata = new BsonDocument
        {
            { "listingId", listingId.ToString() },
            { "originalFileName", fileName },
            { "contentType", contentType },
            { "uploadedAt", DateTime.UtcNow }
        };

        var options = new GridFSUploadOptions
        {
            Metadata = metadata
        };

        // Upload to GridFS
        ObjectId fileId;
        try
        {
            fileId = await _gridFSBucket.UploadFromStreamAsync(uniqueFileName, fileStream, options);
            _logger.LogInformation("Photo uploaded to GridFS: {FileName}, FileId: {FileId}, ListingId: {ListingId}", 
                uniqueFileName, fileId, listingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading photo to GridFS for listing {ListingId}", listingId);
            throw;
        }

        // Return URL that references the file ID
        // Format: /api/photo/gridfs/{fileId}
        return $"/api/photo/gridfs/{fileId}";
    }

    public async Task DeletePhotoAsync(string photoUrl)
    {
        try
        {
            var fileId = ExtractFileIdFromUrl(photoUrl);
            if (!fileId.HasValue)
            {
                _logger.LogWarning("Invalid photo URL format for deletion: {PhotoUrl}", photoUrl);
                return;
            }

            await _gridFSBucket.DeleteAsync(fileId.Value);
            _logger.LogInformation("Photo deleted from GridFS: FileId: {FileId}", fileId.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting photo from GridFS: {PhotoUrl}", photoUrl);
            throw;
        }
    }

    public string GetPhotoUrl(string relativePath)
    {
        // This method is kept for compatibility but not used with GridFS
        // GridFS URLs are generated during upload
        return $"/api/photo/gridfs/{relativePath}";
    }

    public async Task<FileValidationResult> ValidateFileAsync(Stream fileStream, string fileName, string contentType)
    {
        // Check file extension
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(fileExtension))
        {
            return new FileValidationResult(false, 
                $"File type {fileExtension} is not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");
        }

        // Check MIME type
        if (!_allowedMimeTypes.Contains(contentType.ToLowerInvariant()))
        {
            return new FileValidationResult(false, 
                $"Content type {contentType} is not allowed. Allowed types: {string.Join(", ", _allowedMimeTypes)}");
        }

        // Check file size
        var fileSize = fileStream.Length;
        if (fileSize > _maxFileSize)
        {
            return new FileValidationResult(false, 
                $"File size {fileSize} bytes exceeds maximum allowed size of {_maxFileSize} bytes");
        }

        if (fileSize == 0)
        {
            return new FileValidationResult(false, "File is empty");
        }

        // Basic image validation (check file header)
        var isValidImage = await ValidateImageHeaderAsync(fileStream);
        if (!isValidImage)
        {
            return new FileValidationResult(false, "File does not appear to be a valid image");
        }

        return new FileValidationResult(true, FileSize: fileSize, DetectedContentType: contentType);
    }

    /// <summary>
    /// Get file stream from GridFS by ObjectId
    /// </summary>
    public async Task<(Stream Stream, string ContentType, string FileName)?> GetFileStreamAsync(ObjectId fileId)
    {
        try
        {
            // Use BSON document filter instead of LINQ expression for GridFS
            // MongoDB driver doesn't support LINQ expressions like x.Id for GridFSFileInfo
            var filter = Builders<GridFSFileInfo>.Filter.Eq("_id", fileId);
            var fileInfo = await _gridFSBucket.Find(filter).FirstOrDefaultAsync();

            if (fileInfo == null)
            {
                _logger.LogWarning("File not found in GridFS: FileId: {FileId}", fileId);
                return null;
            }

            // Open download stream directly from GridFS
            // This is more efficient than downloading to MemoryStream for large files
            var downloadStream = await _gridFSBucket.OpenDownloadStreamAsync(fileId);
            
            // Read the stream into a MemoryStream so we can close the GridFS stream
            // and return a stream that ASP.NET Core can properly dispose
            var memoryStream = new MemoryStream();
            try
            {
                await downloadStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
            }
            finally
            {
                await downloadStream.DisposeAsync();
            }

            var contentType = fileInfo.Metadata?.GetValue("contentType")?.AsString ?? "application/octet-stream";
            var fileName = fileInfo.Filename;

            return (memoryStream, contentType, fileName);
        }
        catch (MongoDB.Driver.MongoException mongoEx)
        {
            _logger.LogError(mongoEx, "MongoDB error retrieving file from GridFS: FileId: {FileId}, Error: {Error}", 
                fileId, mongoEx.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file from GridFS: FileId: {FileId}, Error: {Error}", 
                fileId, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Extract ObjectId from GridFS URL
    /// </summary>
    private static ObjectId? ExtractFileIdFromUrl(string photoUrl)
    {
        try
        {
            // URL format: /api/photo/gridfs/{fileId}
            var parts = photoUrl.Split('/');
            if (parts.Length > 0)
            {
                var fileIdString = parts[^1]; // Last part
                if (ObjectId.TryParse(fileIdString, out var fileId))
                {
                    return fileId;
                }
            }
        }
        catch
        {
            // Ignore parsing errors
        }

        return null;
    }

    private async Task<bool> ValidateImageHeaderAsync(Stream fileStream)
    {
        try
        {
            var buffer = new byte[12];
            fileStream.Position = 0;
            await fileStream.ReadAsync(buffer, 0, 12);
            fileStream.Position = 0;

            // Check for common image file signatures
            return IsJpeg(buffer) || IsPng(buffer) || IsWebP(buffer);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsJpeg(byte[] buffer)
    {
        return buffer.Length >= 2 && buffer[0] == 0xFF && buffer[1] == 0xD8;
    }

    private static bool IsPng(byte[] buffer)
    {
        return buffer.Length >= 8 &&
               buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47 &&
               buffer[4] == 0x0D && buffer[5] == 0x0A && buffer[6] == 0x1A && buffer[7] == 0x0A;
    }

    private static bool IsWebP(byte[] buffer)
    {
        return buffer.Length >= 12 &&
               buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46 &&
               buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50;
    }
}

