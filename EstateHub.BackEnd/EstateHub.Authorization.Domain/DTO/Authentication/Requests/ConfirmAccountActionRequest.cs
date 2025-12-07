using System.ComponentModel.DataAnnotations;
using EstateHub.Authorization.Domain.Models;

namespace EstateHub.Authorization.Domain.DTO.Authentication.Requests;

/// <summary>
/// Request DTO for confirming account actions (e.g., account recovery, hard delete).
/// </summary>
public class ConfirmAccountActionRequest
{
    /// <summary>
    /// The user ID associated with the account action.
    /// </summary>
    [Required(ErrorMessage = "User ID is required")]
    public Guid UserId { get; set; }
    
    /// <summary>
    /// The token for confirming the account action.
    /// </summary>
    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// The type of account action to confirm.
    /// </summary>
    [Required(ErrorMessage = "Action type is required")]
    public AccountActionType ActionType { get; set; }
}
