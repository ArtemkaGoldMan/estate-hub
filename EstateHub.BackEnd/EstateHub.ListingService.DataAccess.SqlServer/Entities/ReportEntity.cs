using EstateHub.ListingService.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstateHub.ListingService.DataAccess.SqlServer.Entities;

[Table("Reports")]
public class ReportEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid ReporterId { get; set; }

    [Required]
    public Guid ListingId { get; set; }

    [Required]
    public int Reason { get; set; } // ReportReason enum as int

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int Status { get; set; } // ReportStatus enum as int

    public Guid? ModeratorId { get; set; }

    [MaxLength(2000)]
    public string? ModeratorNotes { get; set; }

    [MaxLength(2000)]
    public string? Resolution { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    // Navigation properties
    [ForeignKey("ListingId")]
    public virtual ListingEntity Listing { get; set; } = null!;
}
