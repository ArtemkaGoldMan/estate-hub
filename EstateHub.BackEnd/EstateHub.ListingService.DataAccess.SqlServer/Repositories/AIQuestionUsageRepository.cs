using EstateHub.ListingService.DataAccess.SqlServer.Db;
using EstateHub.ListingService.DataAccess.SqlServer.Entities;
using EstateHub.ListingService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EstateHub.ListingService.DataAccess.SqlServer.Repositories;

public class AIQuestionUsageRepository : IAIQuestionUsageRepository
{
    private readonly ApplicationDbContext _context;

    public AIQuestionUsageRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetTodayQuestionCountAsync(Guid userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var usage = await _context.AIQuestionUsage
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId && u.Date == today);

        return usage?.QuestionCount ?? 0;
    }

    public async Task IncrementQuestionCountAsync(Guid userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var usage = await _context.AIQuestionUsage
            .FirstOrDefaultAsync(u => u.UserId == userId && u.Date == today);

        if (usage == null)
        {
            usage = new AIQuestionUsageEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Date = today,
                QuestionCount = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.AIQuestionUsage.Add(usage);
        }
        else
        {
            usage.QuestionCount++;
            usage.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}


