import type { Listing } from '../model/types';

export type GraphqlListing = Omit<Listing, 'pricePln' | 'monthlyRentPln'> & {
  pricePln?: number | string | null;
  monthlyRentPln?: number | string | null;
};

export const mapListing = (listing: GraphqlListing): Listing => ({
  ...listing,
  pricePln:
    listing.pricePln !== undefined && listing.pricePln !== null
      ? Number(listing.pricePln)
      : null,
  monthlyRentPln:
    listing.monthlyRentPln !== undefined && listing.monthlyRentPln !== null
      ? Number(listing.monthlyRentPln)
      : null,
});


