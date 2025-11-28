using System.Security.Claims;
using System.Text;
using CSharpFunctionalExtensions;
using EstateHub.Authorization.Domain.Options;
using JWT.Algorithms;
using JWT.Builder;
using EstateHub.Authorization.Core.Services.Authentication;

namespace EstateHub.Authorization.Core.Helpers;

public class JwtHelper
{
    public const string SessionIdClaimName = "sessionId";
    
    public static TokenResult CreateAccessToken(UserInformation information, JWTOptions options)
    {
        var expirationMinutes = options.ExpirationMinutes > 0 ? options.ExpirationMinutes : 10;
        var builder = JwtBuilder.Create()
            .WithAlgorithm(new HMACSHA256Algorithm())
            .WithSecret(Encoding.UTF8.GetBytes(options.Secret))
            .ExpirationTime(DateTimeOffset.UtcNow.AddMinutes(expirationMinutes).ToUnixTimeSeconds())
            .AddClaim(ClaimTypes.Name, information.UserName)
            .AddClaim(ClaimTypes.NameIdentifier, information.UserId)
            .AddClaim(ClaimTypes.Role, information.Role)
            .AddClaim(SessionIdClaimName, information.SessionId)
            .WithVerifySignature(true);

        // Add issuer and audience if configured
        if (!string.IsNullOrEmpty(options.Issuer))
        {
            builder = builder.AddClaim("iss", options.Issuer);
        }

        if (!string.IsNullOrEmpty(options.Audience))
        {
            builder = builder.AddClaim("aud", options.Audience);
        }

        var accessToken = builder.Encode();

        return new TokenResult
        {
            Token = accessToken,
            ExpirationDate = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes)
        };
    }

    public static TokenResult CreateRefreshToken(UserInformation information, JWTOptions options)
    {
        var builder = JwtBuilder.Create()
            .WithAlgorithm(new HMACSHA256Algorithm())
            .WithSecret(options.Secret)
            .ExpirationTime(DateTimeOffset.UtcNow.AddMonths(1).ToUnixTimeSeconds())
            .AddClaim(ClaimTypes.Name, information.UserName)
            .AddClaim(ClaimTypes.NameIdentifier, information.UserId)
            .AddClaim(ClaimTypes.Role, information.Role)
            .AddClaim(SessionIdClaimName, information.SessionId)
            .WithVerifySignature(true);

        // Add issuer and audience if configured
        if (!string.IsNullOrEmpty(options.Issuer))
        {
            builder = builder.AddClaim("iss", options.Issuer);
        }

        if (!string.IsNullOrEmpty(options.Audience))
        {
            builder = builder.AddClaim("aud", options.Audience);
        }

        var refreshToken = builder.Encode();

        return new TokenResult
        {
            Token = refreshToken,
            ExpirationDate = DateTimeOffset.UtcNow.AddMonths(1)
        };
    }
    
    public class TokenResult
    {
        public string Token { get; set; } = string.Empty;
        public DateTimeOffset ExpirationDate { get; set; }
    }

    public static Result<UserInformation> GetPayloadFromJWTTokenV2(string token, JWTOptions options)
    {
        var payload = JwtBuilder.Create()
            .WithAlgorithm(new HMACSHA256Algorithm())
            .WithSecret(options.Secret)
            .MustVerifySignature()
            .Decode<UserInformation>(token);

        return payload;
    }

    public static IDictionary<string, object> GetPayloadFromJWTToken(string token, JWTOptions options)
    {
        var payload = JwtBuilder.Create()
            .WithAlgorithm(new HMACSHA256Algorithm())
            .WithSecret(options.Secret)
            .MustVerifySignature()
            .Decode<IDictionary<string, object>>(token);

        return payload;
    }

    public static Result<UserInformation> ParseUserInformation(IDictionary<string, object> payload)
    {
        Result failure = Result.Success();

        if (!payload.TryGetValue(ClaimTypes.NameIdentifier, out var nameIdentifierValue))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<UserInformation>("User id is not found."));
        }

        var nameIdentifierValueStr = nameIdentifierValue?.ToString();

        if (string.IsNullOrWhiteSpace(nameIdentifierValueStr))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<UserInformation>("User id can't be null"));
        }

        if (!Guid.TryParse(nameIdentifierValueStr, out var userId))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<UserInformation>(
                    $"{nameof(userId)} is can't parsing."));
        }

        if (!payload.TryGetValue(ClaimTypes.Role, out var roleValue))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<UserInformation>("Role is not found."));
        }

        var role = roleValue?.ToString();

        if (string.IsNullOrWhiteSpace(role))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<UserInformation>(
                    $"{nameof(role)} is can't parsing."));
        }

        if (!payload.TryGetValue(ClaimTypes.Name, out var nicknameValue))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<UserInformation>("Nickname is not found."));
        }

        var nickname = nicknameValue?.ToString();
        if (string.IsNullOrWhiteSpace(nickname))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<UserInformation>(
                    $"{nameof(nickname)} is can't parsing."));
        }
        
        if (!payload.TryGetValue(SessionIdClaimName, out var sessionIdValue))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<UserInformation>("Session id is not found."));
        }
        
        var sessionIdStr = sessionIdValue?.ToString();
        
        if (string.IsNullOrWhiteSpace(sessionIdStr))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<UserInformation>(
                    $"{nameof(sessionIdStr)} is can't parsing."));
        }
        
        if (!Guid.TryParse(sessionIdStr, out var sessionId))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<UserInformation>(
                    $"{nameof(sessionIdStr)} is can't parsing."));
        }

        if (failure.IsFailure)
        {
            return Result.Failure<UserInformation>(failure.Error);
        }

        return new UserInformation(nickname, userId, role, sessionId);
    }
}