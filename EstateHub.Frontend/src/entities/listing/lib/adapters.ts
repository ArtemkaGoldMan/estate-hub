import type { Listing, ListingStatus } from '../model/types';

export type GraphqlListing = Omit<Listing, 'pricePln' | 'monthlyRentPln' | 'status'> & {
  pricePln?: number | string | null;
  monthlyRentPln?: number | string | null;
  status?: string; // GraphQL may return uppercase enum values
};

// Normalize status from GraphQL (may be uppercase) to TypeScript type
const normalizeStatus = (status?: string): ListingStatus => {
  if (!status) return 'Draft';
  const upper = status.toUpperCase();
  if (upper === 'DRAFT') return 'Draft';
  if (upper === 'PUBLISHED') return 'Published';
  if (upper === 'ARCHIVED') return 'Archived';
  return status as ListingStatus; // Fallback to original if it matches
};

export const mapListing = (listing: GraphqlListing): Listing => ({
  ...listing,
  status: normalizeStatus(listing.status),
  pricePln:
    listing.pricePln !== undefined && listing.pricePln !== null
      ? Number(listing.pricePln)
      : null,
  monthlyRentPln:
    listing.monthlyRentPln !== undefined && listing.monthlyRentPln !== null
      ? Number(listing.monthlyRentPln)
      : null,
});


