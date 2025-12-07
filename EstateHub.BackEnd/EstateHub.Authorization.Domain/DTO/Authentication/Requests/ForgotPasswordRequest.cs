using System.ComponentModel.DataAnnotations;

namespace EstateHub.Authorization.Domain.DTO.Authentication.Requests;

/// <summary>
/// Request DTO for initiating password reset process.
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// The email address of the user requesting password reset.
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The return URL to redirect to after password reset is completed.
    /// </summary>
    public string ReturnUrl { get; set; } = string.Empty;
}