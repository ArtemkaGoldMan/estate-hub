using System.ComponentModel.DataAnnotations;

namespace EstateHub.Authorization.Domain.DTO.Authentication.Requests;

/// <summary>
/// Request DTO for resetting user password.
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// The password reset token received via email.
    /// </summary>
    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// The user ID associated with the password reset token.
    /// </summary>
    [Required(ErrorMessage = "User ID is required")]
    public Guid UserId { get; set; }
    
    /// <summary>
    /// The new password. Must meet security requirements (minimum 12 characters, uppercase, lowercase, digit, special character).
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Password confirmation. Must match the Password field.
    /// </summary>
    [Required(ErrorMessage = "Password confirmation is required")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}