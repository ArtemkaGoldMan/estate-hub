namespace EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses;

public class GetUserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public string DisplayName { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; } = null;
    public string Avatar { get; set; }
}
