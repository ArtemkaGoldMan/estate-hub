using EstateHub.ListingService.Core.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace EstateHub.ListingService.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly string _uploadPath;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private readonly string[] _allowedMimeTypes = { "image/jpeg", "image/png", "image/webp" };
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

    public LocalFileStorageService(IWebHostEnvironment environment, ILogger<LocalFileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
        _uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "listings");
        
        // Ensure upload directory exists
        Directory.CreateDirectory(_uploadPath);
    }

    public async Task<string> UploadPhotoAsync(Guid listingId, Stream fileStream, string fileName, string contentType)
    {
        // Validate file first
        var validation = await ValidateFileAsync(fileStream, fileName, contentType);
        if (!validation.IsValid)
        {
            throw new ArgumentException(validation.ErrorMessage);
        }

        // Create listing-specific directory
        var listingDir = Path.Combine(_uploadPath, listingId.ToString());
        Directory.CreateDirectory(listingDir);

        // Generate unique filename
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(listingDir, uniqueFileName);

        // Reset stream position
        fileStream.Position = 0;

        // Save file
        using var fileStreamOut = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(fileStreamOut);

        _logger.LogInformation("Photo uploaded: {FilePath}", filePath);

        // Return relative URL
        var relativePath = $"uploads/listings/{listingId}/{uniqueFileName}";
        return GetPhotoUrl(relativePath);
    }

    public async Task DeletePhotoAsync(string photoUrl)
    {
        try
        {
            var relativePath = ExtractRelativePath(photoUrl);
            var fullPath = Path.Combine(_environment.WebRootPath, relativePath);
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Photo deleted: {FilePath}", fullPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting photo: {PhotoUrl}", photoUrl);
            throw;
        }
    }

    public string GetPhotoUrl(string relativePath)
    {
        // For local development, return relative URL
        // In production, this would be the full CDN URL
        return $"/{relativePath.Replace("\\", "/")}";
    }

    public async Task<FileValidationResult> ValidateFileAsync(Stream fileStream, string fileName, string contentType)
    {
        // Check file extension
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(fileExtension))
        {
            return new FileValidationResult(false, $"File type {fileExtension} is not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");
        }

        // Check MIME type
        if (!_allowedMimeTypes.Contains(contentType.ToLowerInvariant()))
        {
            return new FileValidationResult(false, $"Content type {contentType} is not allowed. Allowed types: {string.Join(", ", _allowedMimeTypes)}");
        }

        // Check file size
        var fileSize = fileStream.Length;
        if (fileSize > _maxFileSize)
        {
            return new FileValidationResult(false, $"File size {fileSize} bytes exceeds maximum allowed size of {_maxFileSize} bytes");
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

    private async Task<bool> ValidateImageHeaderAsync(Stream fileStream)
    {
        try
        {
            var buffer = new byte[8];
            fileStream.Position = 0;
            await fileStream.ReadAsync(buffer, 0, 8);
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

    private static string ExtractRelativePath(string photoUrl)
    {
        // Extract relative path from URL like "/uploads/listings/guid/filename.jpg"
        var uri = new Uri(photoUrl, UriKind.RelativeOrAbsolute);
        return uri.IsAbsoluteUri ? uri.LocalPath.TrimStart('/') : photoUrl.TrimStart('/');
    }
}
