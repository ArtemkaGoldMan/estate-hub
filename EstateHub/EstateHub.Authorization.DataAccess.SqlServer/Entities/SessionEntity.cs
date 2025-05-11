using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EstateHub.Authorization.DataAccess.SqlServer.Entities
{
    public class SessionEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTimeOffset ExpirationDate { get; set; }
    }

    public class SessionEntityConfiguration : IEntityTypeConfiguration<SessionEntity>
    {
        public void Configure(EntityTypeBuilder<SessionEntity> builder)
        {
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.UserId)
                .IsRequired(true);

            builder.Property(x => x.AccessToken)
                .HasMaxLength(2000) // Adjust length as needed
                .IsRequired(true);

            builder.Property(x => x.RefreshToken)
                .HasMaxLength(2000) // Adjust length as needed
                .IsRequired(true);
            
            builder.Property(x => x.ExpirationDate)
                .IsRequired(true);

            // Creating index on UserId for faster lookups
            builder.HasIndex(x => x.UserId);
        }
    }
} 