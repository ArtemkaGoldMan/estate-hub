namespace EstateHub.SharedKernel.API.MicroserviceEndpoints;

public static class AuthorizationMicroservice
{
    public const string GetUserIdFromToken = "/user-id-from-token";
    public const string GetUserById = "/user/{id:guid}";
    public const string GetUsersByIds = "/users/by-ids";

    public const string IncludeDeletedQuery = "includeDeleted=true";

    public static string GetUserByIdWithDeleted => $"{GetUserById}?{IncludeDeletedQuery}";
    public static string GetUsersByIdsWithDeleted => $"{GetUsersByIds}?{IncludeDeletedQuery}";

    public static string WithIncludeDeleted(this string endpoint, bool includeDeleted)
    {
        return includeDeleted ? $"{endpoint}?{IncludeDeletedQuery}" : endpoint;
    }
}
