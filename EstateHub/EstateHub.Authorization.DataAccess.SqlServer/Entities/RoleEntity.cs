using Microsoft.AspNetCore.Identity;

namespace EstateHub.Authorization.DataAccess.SqlServer.Entities
{
    public class RoleEntity : IdentityRole<Guid>
    {
        public virtual ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
    }
} 