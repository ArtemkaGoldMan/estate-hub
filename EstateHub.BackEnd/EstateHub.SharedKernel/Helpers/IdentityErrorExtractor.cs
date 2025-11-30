using Microsoft.AspNetCore.Identity;

namespace EstateHub.SharedKernel.Helpers;

/// <summary>
/// Simple helper to convert Identity errors to user-friendly messages
/// </summary>
public static class IdentityErrorExtractor
{
    /// <summary>
    /// Converts IdentityResult errors to a single user-friendly message
    /// </summary>
    public static string ToUserMessage(IdentityResult result)
    {
        if (result.Succeeded) return string.Empty;

        var messages = result.Errors.Select(MapError).ToList();
        return messages.Count > 0 ? string.Join(" ", messages) : "Validation failed";
    }

    private static string MapError(IdentityError error)
    {
        return error.Code switch
        {
            "PasswordRequiresDigit" => "Password must contain at least one number (0-9).",
            "PasswordRequiresLower" => "Password must contain at least one lowercase letter (a-z).",
            "PasswordRequiresUpper" => "Password must contain at least one uppercase letter (A-Z).",
            "PasswordRequiresNonAlphanumeric" => "Password must contain at least one special character (!@#$%^&*).",
            "PasswordTooShort" => $"Password must be at least {GetMinLength(error)} characters long.",
            "DuplicateEmail" => "This email is already registered. Please use a different email or try logging in.",
            "InvalidEmail" => "Please enter a valid email address.",
            "DuplicateUserName" => "This username is already taken. Please choose a different one.",
            "InvalidUserName" => "Username contains invalid characters.",
            "UserLockedOut" => "Your account has been temporarily locked. Please try again later.",
            _ => error.Description
        };
    }

    private static int GetMinLength(IdentityError error)
    {
        // Try to extract min length from error description
        // Format is usually "Password must be at least X characters long"
        var match = System.Text.RegularExpressions.Regex.Match(error.Description, @"(\d+)");
        return match.Success && int.TryParse(match.Value, out var length) ? length : 12;
    }
}

