using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.API.Types.OutputTypes;
using EstateHub.ListingService.API.Types.InputTypes;
using HotChocolate;
using HotChocolate.Authorization;

namespace EstateHub.ListingService.API.Types.Queries;

public class Queries
{
    public async Task<ListingType?> GetListing(
        Guid id,
        [Service] IListingService listingService)
    {
        var result = await listingService.GetByIdAsync(id);
        return result != null ? ListingType.FromDto(result) : null;
    }

    public async Task<PagedListingsType> GetListings(
        ListingFilterType? filter,
        int page,
        int pageSize,
        [Service] IListingService listingService)
    {
        pageSize = Math.Min(pageSize, 50);
        
        var filterDto = filter?.ToDto();
        var result = await listingService.GetAllAsync(filterDto, page, pageSize);
        return PagedListingsType.FromDto(result);
    }

    [Authorize]
    public async Task<PagedListingsType> GetMyListings(
        int page,
        int pageSize,
        [Service] IListingService listingService)
    {
        pageSize = Math.Min(pageSize, 50);
        var result = await listingService.GetMyAsync(page, pageSize);
        return PagedListingsType.FromDto(result);
    }

    [Authorize]
    public async Task<PagedListingsType> GetLikedListings(
        int page,
        int pageSize,
        [Service] IListingService listingService)
    {
        pageSize = Math.Min(pageSize, 50);
        var result = await listingService.GetLikedAsync(page, pageSize);
        return PagedListingsType.FromDto(result);
    }

    [Authorize]
    public async Task<PagedListingsType> GetArchivedListings(
        int page,
        int pageSize,
        [Service] IListingService listingService)
    {
        pageSize = Math.Min(pageSize, 50);
        var result = await listingService.GetArchivedAsync(page, pageSize);
        return PagedListingsType.FromDto(result);
    }

    public async Task<PagedListingsType> GetListingsOnMap(
        BoundsInputType? bounds,
        int page,
        int pageSize,
        ListingFilterType? filter,
        [Service] IListingService listingService)
    {
        pageSize = Math.Min(pageSize, 50);
        
        if (bounds == null)
        {
            return PagedListingsType.FromDto(new PagedResult<ListingDto>(new List<ListingDto>(), 0, page, pageSize));
        }
        
        var boundsDto = bounds.ToDto();
        var filterDto = filter?.ToDto();
        var result = await listingService.GetWithinBoundsAsync(boundsDto, page, pageSize, filterDto);
        return PagedListingsType.FromDto(result);
    }

    public async Task<PagedListingsType> SearchListings(
        string text,
        ListingFilterType? filter,
        int page,
        int pageSize,
        [Service] IListingService listingService)
    {
        pageSize = Math.Min(pageSize, 50);
        
        var filterDto = filter?.ToDto();
        var result = await listingService.SearchAsync(text, filterDto, page, pageSize);
        return PagedListingsType.FromDto(result);
    }
}
