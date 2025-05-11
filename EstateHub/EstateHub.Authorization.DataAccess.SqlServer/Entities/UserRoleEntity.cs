using Microsoft.AspNetCore.Identity;

namespace EstateHub.Authorization.DataAccess.SqlServer.Entities
{
    public class UserRoleEntity : IdentityUserRole<Guid>
    {
        public virtual UserEntity User { get; set; } = null!;
        public virtual RoleEntity Role { get; set; } = null!;
    }
} 
 