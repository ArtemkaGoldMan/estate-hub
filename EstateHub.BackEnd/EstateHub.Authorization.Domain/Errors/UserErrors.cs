using EstateHub.Authorization.Domain.Models;
using EstateHub.SharedKernel;

namespace EstateHub.Authorization.Domain.Errors;

public static class UserErrors
{
    public static Error NotFoundById(Guid userId) => new(
        "404", "Users.NotFoundById", "3002", $"The user with the Id = '{userId}' was not found");

    public static Error NotFoundByIds(List<Guid> userIds) => new(
        "404", "Users.NotFoundByIds", "3003",
        $"The users with the Ids = '{string.Join("', '", userIds)}' were not found");

    public static Error NotFoundByEmail(string email) => new(
        "404", "Users.NotFoundByEmail", "3004", $"The user with the Email = '{email}' was not found");

    public static Error EmailNotUnique() => new(
        "400", "Users.EmailNotUnique", "3005", "The provided email is not unique");

    public static Error UserNotCreated() => new(
        "500", "Users.UserNotCreated", "3006", "The user was not created");

    public static Error UserNotAddedToRole() => new(
        "500", "Users.UserNotAddedToRole", "3007", "The user was not added to the role");

    public static Error EmptyUserName() => new(
        "400", "Users.EmptyUserName", "3008", "Username cannot be empty");

    public static Error InvalidEmail() => new(
        "400", "Users.InvalidEmail", "3009", "Email is incorrect");

    public static Error InvalidDisplayName() => new(
        "400", "Users.InvalidDisplayName", "3010", "DisplayName cannot be empty");

    public static Error InvalidDisplayNameLength(string displayName) => new(
        "400", "Users.InvalidDisplayNameLength", "3011",
        $"DisplayName length should be less than {User.MaxLengthNickname} characters but was {displayName.Length}");

    public static Error InvalidPassword() => new(
        "400", "Users.InvalidPassword", "3012", "Password does not meet the requirements");

    public static Error UpdateFailed(Guid id) => new(
        "500", "Users.UpdateFailed", "3013", $"The user with the Id = '{id}' was not updated");

    public static Error DeletionFailed(Guid userId) => new(
        "500", "Users.DeletionFailed", "3014", $"The user with the Id = '{userId}' was not deleted");

    public static Error UserIsDeleted(string email) => new(
        "410", "Users.UserIsDeleted", "3015", $"The user with the Email = '{email}' was deleted");

    public static Error UserNotHardDeleted() => new(
        "500", "Users.UserNotHardDeleted", "3016", "The user was not hard deleted");

    public static Error UserNotRecovered() => new(
        "500", "Users.UserNotRecovered", "3017", "The user account could not be recovered");

    public static Error AvatarTooLarge() => new(
        "400", "Users.AvatarTooLarge", "3018",
        $"The avatar size exceeds the maximum allowed size of {User.MaxAvatarSizeBytes / 1024 / 1024} MB");

    public static Error InvalidAvatarType() => new(
        "400", "Users.InvalidAvatarType", "3019",
        $"The avatar type is not allowed. Allowed types are: {string.Join(", ", User.AllowedAvatarTypes)}");

    public static Error AvatarMustBeSquare(int imageWidth, int imageHeight) => new(
            "400", "Users.AvatarMustBeSquare", "3020",
            $"The avatar image must be square. Provided dimensions: {imageWidth}x{imageHeight} pixels");

    public static Error InvalidImageFormat() => new (
            "400", "Users.InvalidImageFormat", "3021",
            "The provided image format is invalid or unsupported. Please upload a valid image file.");

    public static Error ImageProcessingError() => new (
            "500", "Users.ImageProcessingError", "3022",
            "An error occurred while processing the image. Please try again or contact support.");

    public static object AvatarTooSmall(int minLengthOrWidthOfAvatar) => new Error(
        "400", "Users.AvatarTooSmall", "3023",
        $"The avatar dimensions must be at least {minLengthOrWidthOfAvatar} pixels in length or width. Provided dimensions are smaller than the minimum required size.");

    public static Error CannotRemoveOwnAdminRole() => new(
        "400", "Users.CannotRemoveOwnAdminRole", "3024", 
        "You cannot remove your own Admin role");
}
