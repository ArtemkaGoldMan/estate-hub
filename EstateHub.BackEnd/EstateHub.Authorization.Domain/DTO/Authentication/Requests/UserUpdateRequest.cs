using System.ComponentModel.DataAnnotations;
using EstateHub.SharedKernel.CustomAttributes;
using Microsoft.AspNetCore.Http;

namespace EstateHub.Authorization.Domain.DTO.Authentication.Requests;

public class UserUpdateRequest
{
    public string? DisplayName { get; set; }

    [MaxFileSize(Models.User.MaxAvatarSizeBytes)]
    public IFormFile? Avatar { get; set; }
    
    // Contact & Location Information
    public string? PhoneNumber { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? PostalCode { get; set; }
    
    // Professional Information
    public string? CompanyName { get; set; }
    public string? Website { get; set; }
}
