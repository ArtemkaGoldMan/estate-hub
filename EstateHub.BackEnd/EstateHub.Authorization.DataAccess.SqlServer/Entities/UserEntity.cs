using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EstateHub.Authorization.DataAccess.SqlServer.Entities;

public class UserEntity : IdentityUser<Guid>
{
    public string DisplayName { get; set; }
    public byte[]? AvatarData { get; set; }
    public string? AvatarContentType { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; } = null;
    public virtual ICollection<UserRoleEntity> UserRoles { get; set; }
}

public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.Property(x => x.DisplayName)
            .IsRequired(true);

        builder.Property(x => x.AvatarData)
            .HasColumnType("varbinary(max)")
            .IsRequired(false);

        builder.Property(x => x.AvatarContentType)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(x => x.IsDeleted)
            .IsRequired(true);

        builder.Property(x => x.DeletedAt)
            .IsRequired(false);
    }
}
