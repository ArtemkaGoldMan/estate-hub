using AutoMapper;
using AutoMapper.QueryableExtensions;
using EstateHub.Authorization.Domain.DTO.Authentication.Requests;
using EstateHub.Authorization.Domain.DTO.User;
using EstateHub.Authorization.Domain.Errors;
using EstateHub.Authorization.Domain.Interfaces.DataAccessInterfaces;
using EstateHub.Authorization.DataAccess.SqlServer.Entities;
using EstateHub.Authorization.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EstateHub.Authorization.DataAccess.SqlServer.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    private IConfigurationProvider _mapperConfig => _mapper.ConfigurationProvider;

    public UsersRepository(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public Task<TProjectTo?> GetByIdAsync<TProjectTo>(Guid id, bool includeDeleted = false)
        where TProjectTo : class =>
        _context.Users
            .AsNoTracking()
            .Where(x => x.Id == id && (includeDeleted || !x.IsDeleted))
            .ProjectTo<TProjectTo>(_mapperConfig)
            .FirstOrDefaultAsync();

    public Task<TProjectTo?> GetByEmailAsync<TProjectTo>(string email, bool includeDeleted = false)
        where TProjectTo : class =>
        _context.Users
            .AsNoTracking()
            .Where(x => x.Email == email && (includeDeleted || !x.IsDeleted))
            .ProjectTo<TProjectTo>(_mapperConfig)
            .FirstOrDefaultAsync();

    public Task<List<TProjectTo>> GetByIdsAsync<TProjectTo>(List<Guid> ids, bool includeDeleted = false)
        where TProjectTo : class =>
        _context.Users
            .Where(x => ids.Contains(x.Id) && (includeDeleted || !x.IsDeleted))
            .ProjectTo<TProjectTo>(_mapperConfig)
            .ToListAsync();

    public async Task<bool> UpdateByIdAsync(Guid id, UserUpdateDto user)
    {
        var userEntity = await _context.Users.FindAsync(id);
        if (userEntity == null || userEntity.IsDeleted)
        {
            throw new ArgumentNullException(UserErrors.NotFoundById(id).ToString());
        }

        userEntity.DisplayName = user.DisplayName;
        if (user.AvatarData != null && user.AvatarContentType != null)
        {
            userEntity.AvatarData = user.AvatarData;
            userEntity.AvatarContentType = user.AvatarContentType;
        }

        _context.Users.Update(userEntity);

        int result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null || user.IsDeleted)
        {
            throw new ArgumentNullException(UserErrors.NotFoundById(id).ToString());
        }

        user.AvatarData = null;
        user.AvatarContentType = null;
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;

        int result = await _context.SaveChangesAsync();
        return result > 0;
    }
}
