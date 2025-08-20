namespace EstateHub.Authorization.Domain.DTO.User;

public class UserUpdateDto
{
    public string DisplayName { get; set; }
    public byte[]? AvatarData { get; set; }
    public string? AvatarContentType { get; set; }
    
    // Contact & Location Information
    public string? PhoneNumber { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? PostalCode { get; set; }
    
    // Professional Information
    public string? CompanyName { get; set; }
    public string? Website { get; set; }
}
