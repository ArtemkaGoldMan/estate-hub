using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Core.Abstractions;
using EstateHub.ListingService.Domain.Enums;
using HotChocolate;
using HotChocolate.Authorization;

namespace EstateHub.ListingService.API.Types;

public class Mutations
{
    [Authorize]
    public async Task<Guid> CreateListing(
        CreateListingInputType input,
        [Service] IListingService listingService)
    {
        var inputDto = input.ToDto();
        return await listingService.CreateAsync(inputDto);
    }

    [Authorize]
    public async Task<bool> UpdateListing(
        Guid id,
        UpdateListingInputType input,
        [Service] IListingService listingService)
    {
        var inputDto = input.ToDto();
        await listingService.UpdateAsync(id, inputDto);
        return true;
    }

    [Authorize]
    public async Task<bool> DeleteListing(
        Guid id,
        [Service] IListingService listingService)
    {
        await listingService.DeleteAsync(id);
        return true;
    }

    [Authorize]
    public async Task<bool> ChangeStatus(
        Guid id,
        ChangeStatusInputType input,
        [Service] IListingService listingService)
    {
        await listingService.ChangeStatusAsync(id, input.NewStatus);
        return true;
    }

    [Authorize]
    public async Task<bool> LikeListing(
        Guid id,
        [Service] IListingService listingService)
    {
        await listingService.LikeAsync(id);
        return true;
    }

    [Authorize]
    public async Task<bool> UnlikeListing(
        Guid id,
        [Service] IListingService listingService)
    {
        await listingService.UnlikeAsync(id);
        return true;
    }
}