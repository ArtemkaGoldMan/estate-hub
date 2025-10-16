namespace EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses;

public class GetUserWithRolesResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? PostalCode { get; set; }
    public string? CompanyName { get; set; }
    public string? Website { get; set; }
    public DateTime? LastActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string Avatar { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}
