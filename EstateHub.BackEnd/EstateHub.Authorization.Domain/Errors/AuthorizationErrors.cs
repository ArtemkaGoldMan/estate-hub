using EstateHub.SharedKernel;

namespace EstateHub.Authorization.Domain.Errors;

public static class AuthorizationErrors
{
    public static Error NotFoundRefreshToken() => new(
        "401", "Authorization.NotFoundRefreshToken", "1001", "Refresh token not found");

    public static Error IncorrectPasswordOrUsername() => new(
        "401", "Authorization.IncorrectPasswordOrUsername", "1002", "Password or username is incorrect");

    public static Error EmailNotConfirmed() => new(
        "403", "Authorization.EmailNotConfirmed", "1003", "Email is not confirmed");

    public static Error InvalidRole() => new(
        "400", "Authorization.InvalidRole", "1004", "Role does not exist");

    public static Error CallbackIsNull() => new(
        "400", "Authorization.CallbackIsNull", "1005", "Callback URL is required");

    public static Error ResetPasswordError() => new(
        "400", "Authorization.ResetPasswordError", "1006", "Password isn't reset.");

    public static Error UserIdsNotEquals(Guid left, Guid right) => new(
        "403", "Authorization.UserIdsNotEquals", "1007", $"User IDs are not equal {left} and {right}");

    public static Error RefreshTokenExpired() => new(
        "401", "Authorization.RefreshTokenExpired", "1008", "Refresh token is expired");

    public static Error LogoutError() => new(
        "400", "Authorization.LogoutError", "1009", "Logout error");

    public static Error SessionIdsNotEquals(Guid resultGetId, Guid valueSessionId) => new(
        "403", "Authorization.SessionIdsNotEquals", "1010", $"Session IDs are not equal {resultGetId} and {valueSessionId}");

    public static Error InvalidToken() => new(
        "401", "Authorization.InvalidToken", "1011", "Token is invalid");

    public static Error NotFoundAccountAction() => new(
        "404", "Authorization.NotFoundAccountAction", "1012", "Account action not found");

    public static Error CanUpdateOnlySelf() => new(
        "403", "Authorization.CanUpdateOnlySelf", "1013", "User can update only their own profile");

    public static Error InvalidAccessToken() => new(
        "401", "Authorization.InvalidAccessToken", "1014", "Access token is invalid");

    public static Error CanDeleteOnlySelf() => new(
        "403", "Authorization.CanDeleteOnlySelf", "1015", "You can only delete your own account");
}
