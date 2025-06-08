namespace EstateHub.Authorization.Domain.DTO.User;

public class UserRegistrationDto : UserWithRolesDto
{
    public bool RequireConfirmedAccount { get; set; } = false;
    public string GeneratedEmailConfirmationToken { get; set; } = string.Empty;
}