using EstateHub.Authorization.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EstateHub.Authorization.DataAccess.SqlServer.Entities;

public class SessionEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string AccessToken { get; set; }

    public string RefreshToken { get; set; }
    
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
            .HasMaxLength(Session.MaxLengthToken)
            .IsRequired(true);

        builder.Property(x => x.RefreshToken)
            .HasMaxLength(Session.MaxLengthToken)
            .IsRequired(true);
        
        builder.Property(x => x.ExpirationDate)
            .IsRequired(true);
    }
}