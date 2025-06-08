namespace EstateHub.Authorization.Domain.DTO.User;

public class UserUpdateDto
{
    public string DisplayName { get; set; }
    public byte[]? AvatarData { get; set; }
    public string? AvatarContentType { get; set; }
}
