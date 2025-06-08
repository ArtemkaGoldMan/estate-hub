namespace EstateHub.Authorization.Domain.DTO.User;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public string DisplayName { get; set; }
    public byte[]? AvatarData { get; set; }
    public string? AvatarContentType { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; } = null;
}
