using System.ComponentModel.DataAnnotations;
using EstateHub.SharedKernel.CustomAttributes;
using Microsoft.AspNetCore.Http;

namespace EstateHub.Authorization.Domain.DTO.Authentication.Requests;

public class UserUpdateRequest
{
    public string? DisplayName { get; set; }

    [MaxFileSize(Models.User.MaxAvatarSizeBytes)]
    public IFormFile? Avatar { get; set; }
}
