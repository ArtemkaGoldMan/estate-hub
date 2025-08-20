namespace EstateHub.Authorization.Domain;

public enum Roles
{
    Admin,      // Full system control
    Moderator,  // Content moderation + user management
    User        // Regular users - can do everything normal users do
}