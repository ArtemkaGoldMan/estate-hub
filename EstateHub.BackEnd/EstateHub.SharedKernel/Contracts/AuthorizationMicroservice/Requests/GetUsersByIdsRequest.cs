namespace EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Requests;

public class GetUsersByIdsRequest
{
    public List<Guid> Ids { get; set; }
}
