using EstateHub.SharedKernel;

namespace EstateHub.Authorization.Domain.Errors;

public static class SessionErrors
{
    public static Error NotFound(Guid sessionId) => new(
        "404", "Sessions.NotFound", "2001", $"The session with the Id = '{sessionId}' was not found");

    public static Error NotFoundByRefreshToken(string refreshToken) => new(
        "404", "Sessions.NotFoundByRefreshToken", "2002", $"The session with the specified refresh token was not found");

    public static Error NotFoundByUserId(Guid userId) => new(
        "404", "Sessions.NotFoundByUserId", "2003", $"The session with the UserId = '{userId}' was not found");

    public static Error EmptyUserId() => new(
        "400", "Sessions.EmptyUserId", "2004", "The userId must not be empty");

    public static Error InvalidAccessToken() => new(
        "400", "Sessions.InvalidAccessToken", "2005", "The access token must not be null or whitespace");

    public static Error AccessTokenTooLong(int maxLength) => new(
        "400", "Sessions.AccessTokenTooLong", "2006", $"The access token exceeds the maximum length of {maxLength} characters");

    public static Error InvalidRefreshToken() => new(
        "400", "Sessions.InvalidRefreshToken", "2007", "The refresh token must not be null or whitespace");

    public static Error RefreshTokenTooLong(int maxLength) => new(
        "400", "Sessions.RefreshTokenTooLong", "2008", $"The refresh token exceeds the maximum length of {maxLength} characters");

    public static Error InvalidExpirationTime() => new(
        "400", "Sessions.InvalidExpirationTime", "2009", "The refresh token expiration time must be in the future");

    public static Error DeletionFailed(Guid id) => new(
        "500", "Sessions.DeletionFailed", "2010", $"The session with the Id = '{id}' was not deleted");

    public static Error DeletionFailedByUserId(Guid userId) => new(
        "500", "Sessions.DeletionFailedByUserId", "2011", $"The session with the UserId = '{userId}' was not deleted");
}
