using AutoMapper;
using AutoMapper.QueryableExtensions;
using CSharpFunctionalExtensions;
using EstateHub.Authorization.Domain.Errors;
using EstateHub.Authorization.Domain.Interfaces.DataAccessInterfaces;
using EstateHub.Authorization.Domain.Models;
using EstateHub.Authorization.DataAccess.SqlServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EstateHub.Authorization.DataAccess.SqlServer.Repositories;

public class SessionsRepository : ISessionsRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<SessionsRepository> _logger;

    private IConfigurationProvider _mapperConfig => _mapper.ConfigurationProvider;

    public SessionsRepository(ApplicationDbContext context, IMapper mapper, ILogger<SessionsRepository> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TProjectTo?> GetByIdAsync<TProjectTo>(Guid id)
        where TProjectTo : class =>
        await _context.Sessions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .ProjectTo<TProjectTo>(_mapperConfig)
            .FirstOrDefaultAsync();

    public async Task<TProjectTo?> GetByRefreshTokenAsync<TProjectTo>(string refreshToken)
        where TProjectTo : class =>
        await _context.Sessions
            .AsNoTracking()
            .Where(x => x.RefreshToken == refreshToken)
            .ProjectTo<TProjectTo>(_mapperConfig)
            .FirstOrDefaultAsync();

    public async Task<TProjectTo> CreateAsync<TProjectTo>(Session session)
    {
        var sessionEntity = _mapper.Map<Session, SessionEntity>(session);
        var result = await _context.Sessions.AddAsync(sessionEntity);
        await _context.SaveChangesAsync();

        return await _context.Sessions
            .AsNoTracking()
            .Where(e => result.Entity.Id == e.Id)
            .ProjectTo<TProjectTo>(_mapperConfig)
            .FirstAsync();
    }

    public async Task<bool> UpdateAsync(Session session)
    {
        SessionEntity? existingEntity = await _context.Sessions
            .FirstOrDefaultAsync(s => s.Id == session.Id);

        if (existingEntity != null)
        {
            _mapper.Map(session, existingEntity);
        }

        int result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(string refreshToken)
    {
        SessionEntity? session = await GetByRefreshTokenAsync<SessionEntity>(refreshToken);
        if (session is null)
        {
            _logger.LogError(SessionErrors.NotFoundByRefreshToken(refreshToken).ToString());
            throw new ArgumentNullException(SessionErrors.NotFoundByRefreshToken(refreshToken).ToString());
        }

        _context.Remove(session);
        int result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteByUserIdAsync(Guid userId)
    {
        List<SessionEntity> sessions = await _context.Sessions
            .Where(x => x.UserId == userId)
            .ToListAsync();

        if (!sessions.Any())
        {
            return true;
        }

        _context.Sessions.RemoveRange(sessions);
        int result = await _context.SaveChangesAsync();
        return result > 0;
    }
}
