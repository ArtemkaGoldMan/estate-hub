namespace EstateHub.Authorization.DataAccess.SqlServer.Entities
{
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string PropertySeeker = "PropertySeeker"; // User looking for offers
        public const string PropertyOwner = "PropertyOwner"; // User creating offers with their estate
    }
} 