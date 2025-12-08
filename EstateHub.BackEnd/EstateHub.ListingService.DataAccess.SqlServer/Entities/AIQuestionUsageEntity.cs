namespace EstateHub.ListingService.DataAccess.SqlServer.Entities;

public class AIQuestionUsageEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateOnly Date { get; set; }
    public int QuestionCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}


