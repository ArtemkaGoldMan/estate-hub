using EstateHub.Authorization.Domain.Interfaces.CoreInterfaces;
using EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses;
using EstateHub.SharedKernel.Contracts.Grpc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace EstateHub.Authorization.Infrastructure.Services;

public class UserGrpcService : EstateHub.SharedKernel.Contracts.Grpc.UserService.UserServiceBase
{
    private readonly IUsersService _usersService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserGrpcService> _logger;

    public UserGrpcService(
        IUsersService usersService, 
        IHttpContextAccessor httpContextAccessor,
        ILogger<UserGrpcService> logger)
    {
        _usersService = usersService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public override async Task<GetUserIdFromTokenResponse> GetUserIdFromToken(
        Google.Protobuf.WellKnownTypes.Empty request, 
        Grpc.Core.ServerCallContext context)
    {
        try
        {
            var userId = GetUserIdFromContext();
            _logger.LogDebug("GetUserIdFromToken called for user {UserId}", userId);
            
            return new GetUserIdFromTokenResponse 
            { 
                UserId = userId.ToString() 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUserIdFromToken");
            throw new Grpc.Core.RpcException(new Grpc.Core.Status(Grpc.Core.StatusCode.Unauthenticated, "Invalid or missing token"));
        }
    }

    public override async Task<EstateHub.SharedKernel.Contracts.Grpc.GetUserResponse> GetUserById(
        EstateHub.SharedKernel.Contracts.Grpc.GetUserByIdRequest request, 
        Grpc.Core.ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.Id, out var userId))
            {
                _logger.LogWarning("Invalid user ID format: {UserId}", request.Id);
                throw new Grpc.Core.RpcException(new Grpc.Core.Status(Grpc.Core.StatusCode.InvalidArgument, "Invalid user ID format"));
            }

            _logger.LogDebug("GetUserById called for user {UserId}, includeDeleted: {IncludeDeleted}", 
                userId, request.IncludeDeleted);

            var result = await _usersService.GetByIdAsync<EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses.GetUserResponse>(
                userId, request.IncludeDeleted);
            
            if (!result.IsSuccess)
            {
                _logger.LogWarning("User not found for ID: {UserId}", userId);
                throw new Grpc.Core.RpcException(new Grpc.Core.Status(Grpc.Core.StatusCode.NotFound, "User not found"));
            }

            return MapToGrpcResponse(result.Value);
        }
        catch (Grpc.Core.RpcException)
        {
            throw; // Re-throw gRPC exceptions as-is
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUserById for user {UserId}", request.Id);
            throw new Grpc.Core.RpcException(new Grpc.Core.Status(Grpc.Core.StatusCode.Internal, "Internal server error"));
        }
    }

    public override async Task<EstateHub.SharedKernel.Contracts.Grpc.GetUsersByIdsResponse> GetUsersByIds(
        EstateHub.SharedKernel.Contracts.Grpc.GetUsersByIdsRequest request, 
        Grpc.Core.ServerCallContext context)
    {
        try
        {
            if (request.Ids.Count == 0)
            {
                return new EstateHub.SharedKernel.Contracts.Grpc.GetUsersByIdsResponse();
            }

            var userIds = new List<Guid>();
            foreach (var id in request.Ids)
            {
                if (!Guid.TryParse(id, out var userId))
                {
                    throw new Grpc.Core.RpcException(new Grpc.Core.Status(Grpc.Core.StatusCode.InvalidArgument, $"Invalid user ID format: {id}"));
                }
                userIds.Add(userId);
            }

            _logger.LogDebug("GetUsersByIds called for {Count} users, includeDeleted: {IncludeDeleted}", 
                userIds.Count, request.IncludeDeleted);

            var result = await _usersService.GetByIdsAsync<EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses.GetUserResponse>(
                userIds, request.IncludeDeleted);
            
            if (!result.IsSuccess)
            {
                _logger.LogError("Failed to retrieve users by IDs: {Error}", result.Error);
                throw new Grpc.Core.RpcException(new Grpc.Core.Status(Grpc.Core.StatusCode.Internal, "Failed to retrieve users"));
            }

            var grpcUsers = result.Value.Select(MapToGrpcResponse).ToList();
            return new EstateHub.SharedKernel.Contracts.Grpc.GetUsersByIdsResponse { Users = { grpcUsers } };
        }
        catch (Grpc.Core.RpcException)
        {
            throw; // Re-throw gRPC exceptions as-is
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUsersByIds for {Count} users", request.Ids.Count);
            throw new Grpc.Core.RpcException(new Grpc.Core.Status(Grpc.Core.StatusCode.Internal, "Internal server error"));
        }
    }

    private Guid GetUserIdFromContext()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            throw new Grpc.Core.RpcException(new Grpc.Core.Status(Grpc.Core.StatusCode.Unauthenticated, "User is not authenticated"));
        }

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                         user.FindFirst("sub")?.Value ??
                         user.FindFirst("user_id")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new Grpc.Core.RpcException(new Grpc.Core.Status(Grpc.Core.StatusCode.Unauthenticated, "User ID not found in claims"));
        }

        return userId;
    }

    private static EstateHub.SharedKernel.Contracts.Grpc.GetUserResponse MapToGrpcResponse(EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses.GetUserResponse httpResponse)
    {
        return new EstateHub.SharedKernel.Contracts.Grpc.GetUserResponse
        {
            Id = httpResponse.Id.ToString(),
            Email = httpResponse.Email,
            UserName = httpResponse.UserName,
            DisplayName = httpResponse.DisplayName,
            PhoneNumber = httpResponse.PhoneNumber ?? string.Empty,
            Country = httpResponse.Country ?? string.Empty,
            City = httpResponse.City ?? string.Empty,
            Address = httpResponse.Address ?? string.Empty,
            PostalCode = httpResponse.PostalCode ?? string.Empty,
            CompanyName = httpResponse.CompanyName ?? string.Empty,
            Website = httpResponse.Website ?? string.Empty,
            LastActive = httpResponse.LastActive?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? string.Empty,
            IsDeleted = httpResponse.IsDeleted,
            DeletedAt = httpResponse.DeletedAt?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? string.Empty,
            Avatar = httpResponse.Avatar
        };
    }
}
