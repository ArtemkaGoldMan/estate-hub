using EstateHub.SharedKernel;

namespace EstateHub.ListingService.Domain.Errors;

/// <summary>
/// Error definitions for Listing Service with error codes
/// Error codes start from 2000 to avoid conflicts with Authorization Service (1000-1999)
/// </summary>
public static class ListingServiceErrors
{
    // 400 - Bad Request errors (2000-2099)
    public static Error InvalidInput(string? details = null) => new(
        "400", "Listing.InvalidInput", "2000", 
        $"Invalid input provided.{(details != null ? $" {details}" : "")}");

    public static Error ValidationFailed(string details) => new(
        "400", "Listing.ValidationFailed", "2001", 
        $"Validation failed: {details}");

    public static Error InvalidStatusTransition() => new(
        "400", "Listing.InvalidStatusTransition", "2002", 
        "Invalid status transition");

    // 401 - Unauthorized errors (2100-2199)
    // (Not typically used in Listing Service, handled by auth middleware)

    // 403 - Forbidden errors (2200-2299)
    public static Error NotOwner() => new(
        "403", "Listing.NotOwner", "2200", 
        "You can only perform this action on your own listings");

    public static Error UnauthorizedAccess() => new(
        "403", "Listing.UnauthorizedAccess", "2201", 
        "You do not have permission to perform this action");

    // 404 - Not Found errors (2300-2399)
    public static Error ListingNotFound(Guid id) => new(
        "404", "Listing.NotFound", "2300", 
        $"Listing with ID {id} not found");

    public static Error PhotoNotFound(Guid id) => new(
        "404", "Listing.PhotoNotFound", "2301", 
        $"Photo with ID {id} not found");

    public static Error ReportNotFound(Guid id) => new(
        "404", "Listing.ReportNotFound", "2302", 
        $"Report with ID {id} not found");

    // 409 - Conflict errors (2400-2499)
    public static Error AlreadyReported() => new(
        "409", "Listing.AlreadyReported", "2400", 
        "You have already reported this listing");

    public static Error ListingAlreadyLiked() => new(
        "409", "Listing.AlreadyLiked", "2401", 
        "Listing is already liked");

    public static Error ListingNotLiked() => new(
        "409", "Listing.NotLiked", "2402", 
        "Listing is not liked");

    // 422 - Unprocessable Entity errors (2500-2599)
    public static Error CannotDeleteWithPhotos() => new(
        "422", "Listing.CannotDeleteWithPhotos", "2500", 
        "Cannot delete listing that has photos. Remove photos first");

    // 500 - Internal Server errors (2900-2999)
    public static Error DatabaseError(string? details = null) => new(
        "500", "Listing.DatabaseError", "2900", 
        $"Database operation failed.{(details != null ? $" {details}" : "")}");

    public static Error ExternalServiceError(string service, string? details = null) => new(
        "500", "Listing.ExternalServiceError", "2901", 
        $"Error communicating with {service}.{(details != null ? $" {details}" : "")}");
}

