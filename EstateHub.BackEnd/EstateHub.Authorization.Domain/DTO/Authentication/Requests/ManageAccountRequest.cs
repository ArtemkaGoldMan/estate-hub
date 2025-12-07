using System.ComponentModel.DataAnnotations;
using EstateHub.Authorization.Domain.Models;

namespace EstateHub.Authorization.Domain.DTO.Authentication.Requests;

/// <summary>
/// Request DTO for managing account state (e.g., requesting account recovery or hard delete).
/// </summary>
public class ManageAccountRequest
{
    /// <summary>
    /// The email address of the account to manage.
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// The type of account action to perform.
    /// </summary>
    [Required(ErrorMessage = "Action type is required")]
    public AccountActionType ActionType { get; set; }
    
    /// <summary>
    /// The return URL to redirect to after the action is confirmed.
    /// </summary>
    public string ReturnUrl { get; set; } = string.Empty;
}
