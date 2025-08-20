namespace EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses;

public class GetUserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public string DisplayName { get; set; }
    
    // Contact & Location Information
    public string? PhoneNumber { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? PostalCode { get; set; }
    
    // Professional Information
    public string? CompanyName { get; set; }
    public string? Website { get; set; }
    
    // Activity Tracking
    public DateTime? LastActive { get; set; }
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; } = null;
    public string Avatar { get; set; }
}
