using CSharpFunctionalExtensions;
using EstateHub.Authorization.Domain.Errors;

namespace EstateHub.Authorization.Domain.Models;

public record Session
{
    public const int MaxLengthToken = 2048;

    private Session(Guid id, Guid userId, string accessToken, string refreshToken, DateTimeOffset expirationDate)
    {
        Id = id;
        UserId = userId;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpirationDate = expirationDate;
    }

    public Guid Id { get; init; }
    
    public Guid UserId { get; }

    public string AccessToken { get; }

    public string RefreshToken { get; }
    
    public DateTimeOffset ExpirationDate { get; }
    
    public static Result<Session> Create(
        Guid userId,
        string accessToken,
        string refreshToken,
        DateTimeOffset expirationDate)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<Session>(SessionErrors.EmptyUserId().ToString());
        }

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Result.Failure<Session>(SessionErrors.InvalidAccessToken().ToString());
        }

        if (accessToken.Length > MaxLengthToken)
        {
            return Result.Failure<Session>(SessionErrors.AccessTokenTooLong(MaxLengthToken).ToString());
        }

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result.Failure<Session>(SessionErrors.InvalidRefreshToken().ToString());
        }

        if (refreshToken.Length > MaxLengthToken)
        {
            return Result.Failure<Session>(SessionErrors.RefreshTokenTooLong(MaxLengthToken).ToString());
        }

        if (expirationDate < DateTimeOffset.UtcNow)
        {
            return Result.Failure<Session>(SessionErrors.InvalidExpirationTime().ToString());
        }

        var session = new Session(Guid.Empty, userId, accessToken, refreshToken, expirationDate);

        return session;
    }
}