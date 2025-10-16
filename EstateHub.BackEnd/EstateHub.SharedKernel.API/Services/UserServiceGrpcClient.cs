using EstateHub.SharedKernel.API.Interfaces;
using EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Requests;
using EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses;
using EstateHub.SharedKernel.Contracts.Grpc;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;

namespace EstateHub.SharedKernel.API.Services;

public class UserServiceGrpcClient : IUserServiceClient
{
    private readonly UserService.UserServiceClient _grpcClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserServiceGrpcClient> _logger;

    public UserServiceGrpcClient(
        UserService.UserServiceClient grpcClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<UserServiceGrpcClient> logger)
    {
        _grpcClient = grpcClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<UserIdFromTokenResponse?> GetUserIdFromTokenAsync()
    {
        try
        {
            var metadata = CreateAuthMetadata();
            var response = await _grpcClient.GetUserIdFromTokenAsync(
                new Empty(), 
                headers: metadata);
            
            _logger.LogDebug("GetUserIdFromTokenAsync completed successfully via gRPC");
            return new UserIdFromTokenResponse { UserId = Guid.Parse(response.UserId) };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC GetUserIdFromTokenAsync failed with status: {Status}", ex.Status);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetUserIdFromTokenAsync via gRPC");
            return null;
        }
    }

    public async Task<EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses.GetUserResponse?> GetUserByIdAsync(Guid id, bool includeDeleted = false)
    {
        try
        {
            var metadata = CreateAuthMetadata();
            var request = new EstateHub.SharedKernel.Contracts.Grpc.GetUserByIdRequest
            {
                Id = id.ToString(),
                IncludeDeleted = includeDeleted
            };

            var response = await _grpcClient.GetUserByIdAsync(request, headers: metadata);
            
            _logger.LogDebug("GetUserByIdAsync completed successfully via gRPC for user {UserId}", id);
            return MapFromGrpcResponse(response);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC GetUserByIdAsync failed with status: {Status} for user {UserId}", ex.Status, id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetUserByIdAsync via gRPC for user {UserId}", id);
            return null;
        }
    }

    public async Task<EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses.GetUsersByIdsResponse?> GetUsersByIdsAsync(
        EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Requests.GetUsersByIdsRequest getUsersByIdsRequest, 
        bool includeDeleted = false)
    {
        try
        {
            var metadata = CreateAuthMetadata();
            var grpcRequest = new EstateHub.SharedKernel.Contracts.Grpc.GetUsersByIdsRequest
            {
                IncludeDeleted = includeDeleted
            };
            
            grpcRequest.Ids.AddRange(getUsersByIdsRequest.Ids.Select(id => id.ToString()));

            var response = await _grpcClient.GetUsersByIdsAsync(grpcRequest, headers: metadata);
            
            _logger.LogDebug("GetUsersByIdsAsync completed successfully via gRPC for {Count} users", getUsersByIdsRequest.Ids.Count);
            
            var users = response.Users.Select(MapFromGrpcResponse).ToList();
            return new EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses.GetUsersByIdsResponse { Users = users };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC GetUsersByIdsAsync failed with status: {Status} for {Count} users", 
                ex.Status, getUsersByIdsRequest.Ids.Count);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetUsersByIdsAsync via gRPC for {Count} users", 
                getUsersByIdsRequest.Ids.Count);
            return null;
        }
    }

    private Metadata CreateAuthMetadata()
    {
        var httpRequest = _httpContextAccessor.HttpContext?.Request;
        var authHeader = httpRequest?.Headers.Authorization.FirstOrDefault();
        
        var metadata = new Metadata();
        if (!string.IsNullOrEmpty(authHeader))
        {
            metadata.Add("authorization", authHeader);
            _logger.LogDebug("Added authorization header to gRPC request");
        }
        else
        {
            _logger.LogWarning("No authorization header found in HTTP context for gRPC request");
        }
        
        return metadata;
    }

    private static EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses.GetUserResponse MapFromGrpcResponse(EstateHub.SharedKernel.Contracts.Grpc.GetUserResponse grpcResponse)
    {
        return new EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses.GetUserResponse
        {
            Id = Guid.Parse(grpcResponse.Id),
            Email = grpcResponse.Email,
            UserName = grpcResponse.UserName,
            DisplayName = grpcResponse.DisplayName,
            PhoneNumber = string.IsNullOrEmpty(grpcResponse.PhoneNumber) ? null : grpcResponse.PhoneNumber,
            Country = string.IsNullOrEmpty(grpcResponse.Country) ? null : grpcResponse.Country,
            City = string.IsNullOrEmpty(grpcResponse.City) ? null : grpcResponse.City,
            Address = string.IsNullOrEmpty(grpcResponse.Address) ? null : grpcResponse.Address,
            PostalCode = string.IsNullOrEmpty(grpcResponse.PostalCode) ? null : grpcResponse.PostalCode,
            CompanyName = string.IsNullOrEmpty(grpcResponse.CompanyName) ? null : grpcResponse.CompanyName,
            Website = string.IsNullOrEmpty(grpcResponse.Website) ? null : grpcResponse.Website,
            LastActive = string.IsNullOrEmpty(grpcResponse.LastActive) ? null : DateTime.Parse(grpcResponse.LastActive),
            IsDeleted = grpcResponse.IsDeleted,
            DeletedAt = string.IsNullOrEmpty(grpcResponse.DeletedAt) ? null : DateTime.Parse(grpcResponse.DeletedAt),
            Avatar = grpcResponse.Avatar
        };
    }
}
