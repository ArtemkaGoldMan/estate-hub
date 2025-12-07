using System.ComponentModel.DataAnnotations;

namespace EstateHub.Authorization.Domain.DTO.Authentication.Requests;

/// <summary>
/// Request DTO for user registration.
/// </summary>
public class UserRegistrationRequest
{
    /// <summary>
    /// The user's email address (used as username).
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The user's password. Must meet security requirements (minimum 12 characters, uppercase, lowercase, digit, special character).
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

    /// <summary>
    /// The callback URL for email confirmation. Required if email confirmation is enabled.
    /// </summary>
    public string CallbackUrl { get; set; } = string.Empty;
}
