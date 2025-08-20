using System.Net.Mail;
using CSharpFunctionalExtensions;
using EstateHub.Authorization.Domain.Errors;
using SixLabors.ImageSharp;

namespace EstateHub.Authorization.Domain.Models;

public record User
{
    public const int MaxLengthNickname = 50;
    public const int MaxEmailLength = 320;
    public const int MaxAvatarSizeBytes = 2 * 1024 * 1024; // 2MB
    public const int MinLengthOrWidthOfAvatar = 32; // 32px
    public static readonly string[] AllowedAvatarTypes = { "image/jpeg", "image/jpg", "image/png" };

    private User(Guid id, string email, string userName, string displayName, string password, byte[]? avatarData, string? avatarContentType, 
                 string? phoneNumber, string? country, string? city, string? address, string? postalCode, 
                 string? companyName, string? website, DateTime? lastActive)
    {
        Id = id;
        Email = email;
        UserName = userName;
        DisplayName = displayName;
        Password = password;
        AvatarData = avatarData;
        AvatarContentType = avatarContentType;
        PhoneNumber = phoneNumber;
        Country = country;
        City = city;
        Address = address;
        PostalCode = postalCode;
        CompanyName = companyName;
        Website = website;
        LastActive = lastActive;
    }

    public Guid Id { get; }
    public string Email { get; }
    public string UserName { get; }
    public string Password { get; }
    public string DisplayName { get; }
    public byte[]? AvatarData { get; }
    public string? AvatarContentType { get; }
    
    // Contact & Location Information
    public string? PhoneNumber { get; }
    public string? Country { get; }
    public string? City { get; }
    public string? Address { get; }
    public string? PostalCode { get; }
    
    // Professional Information
    public string? CompanyName { get; }
    public string? Website { get; }
    
    // Activity Tracking
    public DateTime? LastActive { get; }

    public static Result<User> Create(string email, string displayName, string userName, string password)
    {
        Guid id = Guid.Empty;

        if (IsValidEmail(email) == false)
        {
            return Result.Failure<User>(UserErrors.InvalidEmail().ToString());
        }

        if (email.Length > MaxEmailLength)
        {
            return Result.Failure<User>(UserErrors.InvalidEmail().ToString());
        }

        if (string.IsNullOrWhiteSpace(userName))
        {
            id = Guid.NewGuid();
            userName = GenerateUniqueUsername(email, id);
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = email;
        }
        else if (displayName.Length > MaxLengthNickname)
        {
            return Result.Failure<User>(UserErrors.InvalidDisplayNameLength(displayName).ToString());
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return Result.Failure<User>(UserErrors.InvalidPassword().ToString());
        }
        //here can be added password validation
        //if (IsValidPassword(password) == false)
        //{
        //    return Result.Failure<User>(UserErrors.InvalidPassword().ToString());
        //}

        var user = new User(id, email, userName, displayName, password, null, null, 
                           null, null, null, null, null, null, null, DateTime.UtcNow);

        return user;
    }

    public static Result<User> Update(Guid id, string displayName, byte[]? avatarData = null, string? avatarContentType = null,
                                     string? phoneNumber = null, string? country = null, string? city = null, 
                                     string? address = null, string? postalCode = null, string? companyName = null, 
                                     string? website = null)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return Result.Failure<User>(UserErrors.InvalidDisplayName().ToString());
        }

        if (displayName.Length > MaxLengthNickname)
        {
            return Result.Failure<User>(UserErrors.InvalidDisplayNameLength(displayName).ToString());
        }

        if (avatarData != null && avatarContentType != null)
        {
            var avatarValidation = ValidateAvatar(avatarData, avatarContentType);
            if (avatarValidation.IsFailure)
            {
                return Result.Failure<User>(avatarValidation.Error);
            }
        }

        return Result.Success(new User(id, string.Empty, string.Empty, displayName, string.Empty, avatarData,
            avatarContentType, phoneNumber, country, city, address, postalCode, companyName, website, DateTime.UtcNow));
    }

    public static string ConvertAvatarToDataUri(byte[]? avatarData, string? contentType)
    {
        if (avatarData == null || avatarData.Length == 0)
        {
            return string.Empty;
        }

        var mimeType = contentType ?? "image/jpeg";
        var base64String = Convert.ToBase64String(avatarData);
        return $"data:{mimeType};base64,{base64String}";
    }

    private static string GenerateUniqueUsername(string email, Guid id) =>
        $"{email}_{id.ToString("N").Substring(0, 8)}";

    private static Result ValidateAvatar(byte[] avatarData, string contentType)
    {
        if (avatarData.Length > MaxAvatarSizeBytes)
        {
            return Result.Failure(UserErrors.AvatarTooLarge().ToString());
        }

        if (!AllowedAvatarTypes.Contains(contentType.ToLower()))
        {
            return Result.Failure(UserErrors.InvalidAvatarType().ToString());
        }

        var dimensionValidation = ValidateImageDimensions(avatarData);
        if (dimensionValidation.IsFailure)
        {
            return dimensionValidation;
        }

        return Result.Success();
    }

    private static Result ValidateImageDimensions(byte[] imageData)
    {
        try
        {
            using var image = Image.Load(imageData);

            if (image.Width < MinLengthOrWidthOfAvatar || image.Height < MinLengthOrWidthOfAvatar)
            {
                return Result.Failure(UserErrors.AvatarTooSmall(MinLengthOrWidthOfAvatar).ToString());
            }

            if (image.Width != image.Height)
            {
                return Result.Failure(UserErrors.AvatarMustBeSquare(image.Width, image.Height).ToString());
            }

            return Result.Success();
        }
        catch (ArgumentException)
        {
            return Result.Failure(UserErrors.InvalidImageFormat().ToString());
        }
        catch (Exception)
        {
            return Result.Failure(UserErrors.ImageProcessingError().ToString());
        }
    }

    private static bool IsValidEmail(string email)
    {
        var trimmedEmail = email.Trim();

        if (trimmedEmail.EndsWith("."))
        {
            return false;
        }

        try
        {
            var addr = new MailAddress(email);
            return addr.Address == trimmedEmail;
        }
        catch
        {
            return false;
        }
    }
}
