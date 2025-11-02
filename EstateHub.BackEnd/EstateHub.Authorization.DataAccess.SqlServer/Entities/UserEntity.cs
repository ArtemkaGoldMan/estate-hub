using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EstateHub.Authorization.DataAccess.SqlServer.Entities;

public class UserEntity : IdentityUser<Guid>
{
    public string DisplayName { get; set; }
    public byte[]? AvatarData { get; set; }
    public string? AvatarContentType { get; set; }
    
    // Contact & Location Information
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? PostalCode { get; set; }
    
    // Professional Information
    public string? CompanyName { get; set; }
    public string? Website { get; set; }
    
    // Activity Tracking
    public DateTime? LastActive { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
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

        // Contact & Location Information
        builder.Property(x => x.Country)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(x => x.City)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(x => x.Address)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.PostalCode)
            .HasMaxLength(20)
            .IsRequired(false);

        // Professional Information
        builder.Property(x => x.CompanyName)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(x => x.Website)
            .HasMaxLength(500)
            .IsRequired(false);

        // Activity Tracking
        builder.Property(x => x.LastActive)
            .IsRequired(false);

        // Timestamps
        builder.Property(x => x.CreatedAt)
            .IsRequired(true)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.IsDeleted)
            .IsRequired(true);

        builder.Property(x => x.DeletedAt)
            .IsRequired(false);
    }
}
