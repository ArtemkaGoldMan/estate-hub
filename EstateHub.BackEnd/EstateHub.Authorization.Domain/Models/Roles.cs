namespace EstateHub.Authorization.Domain;

public enum Roles
{
    Admin,      // Full system control (users, roles, content moderation, reports)
    User        // Regular users - can create listings and reports
}