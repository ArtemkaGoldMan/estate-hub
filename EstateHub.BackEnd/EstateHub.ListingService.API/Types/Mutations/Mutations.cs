using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Domain.Enums;
using EstateHub.ListingService.API.Types.InputTypes;
using EstateHub.SharedKernel.API.Authorization.Attributes;
using HotChocolate;
using HotChocolate.Authorization;

namespace EstateHub.ListingService.API.Types.Mutations;

/// <summary>
/// GraphQL mutations for listing operations.
/// Provides methods for creating, updating, deleting, and managing listings.
/// </summary>
public class Mutations
{
    /// <summary>
    /// Creates a new listing. Requires authentication.
    /// </summary>
    /// <param name="input">The listing creation input containing all listing details (title, description, location, price, etc.).</param>
    /// <param name="listingService">The listing service injected by HotChocolate.</param>
    /// <returns>The unique identifier (Guid) of the newly created listing.</returns>
    [Authorize]
    public async Task<Guid> CreateListing(
        CreateListingInputType input,
        [Service] IListingService listingService)
    {
        var inputDto = input.ToDto();
        return await listingService.CreateAsync(inputDto);
    }

    /// <summary>
    /// Updates an existing listing. Requires authentication.
    /// Users can only update their own listings.
    /// </summary>
    /// <param name="id">The unique identifier of the listing to update.</param>
    /// <param name="input">The listing update input containing the fields to update.</param>
    /// <param name="listingService">The listing service injected by HotChocolate.</param>
    /// <returns>True if the update was successful.</returns>
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

    /// <summary>
    /// Deletes a listing. Requires authentication.
    /// Users can only delete their own listings.
    /// </summary>
    /// <param name="id">The unique identifier of the listing to delete.</param>
    /// <param name="listingService">The listing service injected by HotChocolate.</param>
    /// <returns>True if the deletion was successful.</returns>
    [Authorize]
    public async Task<bool> DeleteListing(
        Guid id,
        [Service] IListingService listingService)
    {
        await listingService.DeleteAsync(id);
        return true;
    }

    /// <summary>
    /// Changes the status of a listing (e.g., Draft, Published, Archived). Requires authentication.
    /// Users can only change the status of their own listings.
    /// </summary>
    /// <param name="id">The unique identifier of the listing.</param>
    /// <param name="input">The status change input containing the new status.</param>
    /// <param name="listingService">The listing service injected by HotChocolate.</param>
    /// <returns>True if the status change was successful.</returns>
    [Authorize]
    public async Task<bool> ChangeStatus(
        Guid id,
        ChangeStatusInputType input,
        [Service] IListingService listingService)
    {
        await listingService.ChangeStatusAsync(id, input.NewStatus);
        return true;
    }

    /// <summary>
    /// Adds a like to a listing. Requires authentication.
    /// If the listing is already liked by the user, this operation has no effect.
    /// </summary>
    /// <param name="id">The unique identifier of the listing to like.</param>
    /// <param name="listingService">The listing service injected by HotChocolate.</param>
    /// <returns>True if the like was successfully added.</returns>
    [Authorize]
    public async Task<bool> LikeListing(
        Guid id,
        [Service] IListingService listingService)
    {
        await listingService.LikeAsync(id);
        return true;
    }

    /// <summary>
    /// Removes a like from a listing. Requires authentication.
    /// If the listing is not liked by the user, this operation has no effect.
    /// </summary>
    /// <param name="id">The unique identifier of the listing to unlike.</param>
    /// <param name="listingService">The listing service injected by HotChocolate.</param>
    /// <returns>True if the like was successfully removed.</returns>
    [Authorize]
    public async Task<bool> UnlikeListing(
        Guid id,
        [Service] IListingService listingService)
    {
        await listingService.UnlikeAsync(id);
        return true;
    }

    /// <summary>
    /// Unpublishes a listing as an administrator with a required reason. Admin only.
    /// The reason will be visible to the listing owner and prevents the listing from being republished until changes are made.
    /// </summary>
    /// <param name="id">The unique identifier of the listing to unpublish.</param>
    /// <param name="input">The admin unpublish input containing the reason for unpublishing.</param>
    /// <param name="listingService">The listing service injected by HotChocolate.</param>
    /// <returns>True if the unpublish operation was successful.</returns>
    [Authorize]
    [RequirePermission("ManageListings")]
    public async Task<bool> AdminUnpublishListing(
        Guid id,
        AdminUnpublishListingInputType input,
        [Service] IListingService listingService)
    {
        await listingService.AdminUnpublishAsync(id, input.Reason);
        return true;
    }
}