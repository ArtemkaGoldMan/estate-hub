using EstateHub.ListingService.DataAccess.SqlServer.Db;
using EstateHub.ListingService.DataAccess.SqlServer.Entities;
using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Enums;
using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EstateHub.ListingService.DataAccess.SqlServer.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly ApplicationDbContext _context;

    public ReportRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Report?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Reports
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        return entity != null ? MapToDomain(entity) : null;
    }

    public async Task<IEnumerable<Report>> GetAllAsync(int page, int pageSize, ReportFilter? filter = null)
    {
        var query = _context.Reports
            .AsNoTracking()
            .AsQueryable();

        if (filter != null)
        {
            query = ApplyFilter(query, filter);
        }

        var entities = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return entities.Select(MapToDomain);
    }

    public async Task<IEnumerable<Report>> GetByReporterIdAsync(Guid reporterId, int page, int pageSize)
    {
        var entities = await _context.Reports
            .AsNoTracking()
            .Where(r => r.ReporterId == reporterId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return entities.Select(MapToDomain);
    }

    public async Task<IEnumerable<Report>> GetByModeratorIdAsync(Guid moderatorId, int page, int pageSize)
    {
        var entities = await _context.Reports
            .AsNoTracking()
            .Where(r => r.ModeratorId == moderatorId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return entities.Select(MapToDomain);
    }

    public async Task<IEnumerable<Report>> GetByListingIdAsync(Guid listingId)
    {
        var entities = await _context.Reports
            .AsNoTracking()
            .Where(r => r.ListingId == listingId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return entities.Select(MapToDomain);
    }

    public async Task<int> GetTotalCountAsync(ReportFilter? filter = null)
    {
        var query = _context.Reports.AsQueryable();

        if (filter != null)
        {
            query = ApplyFilter(query, filter);
        }

        return await query.CountAsync();
    }

    public async Task AddAsync(Report report)
    {
        var entity = MapToEntity(report);
        _context.Reports.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Report report)
    {
        var entity = MapToEntity(report);
        _context.Reports.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Reports.FindAsync(id);
        if (entity != null)
        {
            _context.Reports.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    private IQueryable<ReportEntity> ApplyFilter(IQueryable<ReportEntity> query, ReportFilter filter)
    {
        if (filter.Status.HasValue)
        {
            query = query.Where(r => r.Status == (int)filter.Status.Value);
        }

        if (filter.Reason.HasValue)
        {
            query = query.Where(r => r.Reason == (int)filter.Reason.Value);
        }

        if (filter.ReporterId.HasValue)
        {
            query = query.Where(r => r.ReporterId == filter.ReporterId.Value);
        }

        if (filter.ModeratorId.HasValue)
        {
            query = query.Where(r => r.ModeratorId == filter.ModeratorId.Value);
        }

        if (filter.CreatedFrom.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= filter.CreatedFrom.Value);
        }

        if (filter.CreatedTo.HasValue)
        {
            query = query.Where(r => r.CreatedAt <= filter.CreatedTo.Value);
        }

        return query;
    }

    private Report MapToDomain(ReportEntity entity)
    {
        // Use object initializer to set all properties including computed ones
        return new Report(
            entity.ReporterId,
            entity.ListingId,
            (ReportReason)entity.Reason,
            entity.Description) with
        {
            Id = entity.Id,
            Status = (ReportStatus)entity.Status,
            ModeratorId = entity.ModeratorId,
            ModeratorNotes = entity.ModeratorNotes,
            Resolution = entity.Resolution,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            ResolvedAt = entity.ResolvedAt
        };
    }

    private ReportEntity MapToEntity(Report report)
    {
        return new ReportEntity
        {
            Id = report.Id,
            ReporterId = report.ReporterId,
            ListingId = report.ListingId,
            Reason = (int)report.Reason,
            Description = report.Description,
            Status = (int)report.Status,
            ModeratorId = report.ModeratorId,
            ModeratorNotes = report.ModeratorNotes,
            Resolution = report.Resolution,
            CreatedAt = report.CreatedAt,
            UpdatedAt = report.UpdatedAt,
            ResolvedAt = report.ResolvedAt
        };
    }
}
