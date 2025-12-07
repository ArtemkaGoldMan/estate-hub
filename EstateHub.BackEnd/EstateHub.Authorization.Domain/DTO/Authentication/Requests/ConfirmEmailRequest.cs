using System.ComponentModel.DataAnnotations;

namespace EstateHub.Authorization.Domain.DTO.Authentication.Requests;

/// <summary>
/// Request DTO for email confirmation.
/// </summary>
public class ConfirmEmailRequest
{
    /// <summary>
    /// The email confirmation token.
    /// </summary>
    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// The user ID associated with the confirmation token.
    /// </summary>
    [Required(ErrorMessage = "User ID is required")]
    public Guid UserId { get; set; }
}