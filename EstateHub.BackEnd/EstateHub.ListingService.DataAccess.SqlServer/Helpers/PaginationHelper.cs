using Microsoft.EntityFrameworkCore;

namespace EstateHub.ListingService.DataAccess.SqlServer.Helpers;

/// <summary>
/// Shared pagination helper for database queries
/// </summary>
public static class PaginationHelper
{
    /// <summary>
    /// Applies pagination to an IQueryable and returns the paged results
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="query">The queryable to paginate</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paged queryable</returns>
    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100; // Max page size limit

        return query
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    /// <summary>
    /// Gets the total count of items in a query without loading all records
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="query">The queryable to count</param>
    /// <returns>Total count</returns>
    public static async Task<int> GetCountAsync<T>(this IQueryable<T> query)
    {
        return await query.CountAsync();
    }
}

