namespace EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses;

public class GetUserWithRolesResponse : GetUserResponse
{
    public List<string> Roles { get; set; }
}
