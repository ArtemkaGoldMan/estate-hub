namespace EstateHub.ListingService.Domain.Enums;

public enum ReportReason
{
    InappropriateContent,  // Offensive, illegal, or inappropriate content
    Spam,                 // Spam or misleading information
    FakeListing,          // Fake or fraudulent listing
    WrongCategory,        // Listed in wrong category
    DuplicateListing,     // Duplicate of another listing
    OutdatedInformation,  // Information is outdated or incorrect
    Other                 // Other reason (specified in description)
}
