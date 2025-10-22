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

    // Admin methods implementation
    public Task<List<TProjectTo>> GetUsersAsync<TProjectTo>(int page, int pageSize, bool includeDeleted = false)
        where TProjectTo : class =>
        _context.Users
            .AsNoTracking()
            .Where(x => includeDeleted || !x.IsDeleted)
            .OrderBy(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<TProjectTo>(_mapperConfig)
            .ToListAsync();

    public Task<int> GetUsersCountAsync(bool includeDeleted = false) =>
        _context.Users
            .Where(x => includeDeleted || !x.IsDeleted)
            .CountAsync();

    public Task<int> GetActiveUsersCountAsync() =>
        _context.Users
            .Where(x => !x.IsDeleted && x.LockoutEnd == null)
            .CountAsync();

    public Task<int> GetSuspendedUsersCountAsync() =>
        _context.Users
            .Where(x => !x.IsDeleted && x.LockoutEnd != null && x.LockoutEnd > DateTimeOffset.UtcNow)
            .CountAsync();

    public Task<int> GetNewUsersThisMonthCountAsync()
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        return _context.Users
            .Where(x => !x.IsDeleted && x.CreatedAt >= startOfMonth)
            .CountAsync();
    }

    public async Task<bool> AssignUserRoleAsync(Guid userId, string role)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.IsDeleted)
        {
            return false;
        }

        var roleEntity = await _context.Roles.FirstOrDefaultAsync(r => r.Name == role);
        if (roleEntity == null)
        {
            return false;
        }

        var userRole = new UserRoleEntity
        {
            UserId = userId,
            RoleId = roleEntity.Id
        };

        _context.UserRoles.Add(userRole);
        int result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> RemoveUserRoleAsync(Guid userId, string role)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.IsDeleted)
        {
            return false;
        }

        var roleEntity = await _context.Roles.FirstOrDefaultAsync(r => r.Name == role);
        if (roleEntity == null)
        {
            return false;
        }

        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleEntity.Id);

        if (userRole == null)
        {
            return false;
        }

        _context.UserRoles.Remove(userRole);
        int result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> SuspendUserAsync(Guid userId, string reason)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.IsDeleted)
        {
            return false;
        }

        // Suspend user for 1 year (can be adjusted)
        user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(1);
        user.LockoutEnabled = true;

        _context.Users.Update(user);
        int result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> ActivateUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.IsDeleted)
        {
            return false;
        }

        user.LockoutEnd = null;
        user.LockoutEnabled = false;

        _context.Users.Update(user);
        int result = await _context.SaveChangesAsync();
        return result > 0;
    }
}
