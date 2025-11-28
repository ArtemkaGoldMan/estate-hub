import { gql, useQuery } from '@apollo/client';
import { useMemo } from 'react';
import type { Listing } from '../model/types';
import type { GraphqlListing } from '../lib/adapters';
import { mapListing } from '../lib/adapters';
import { LISTING_CORE_FIELDS } from './get-listings';

const GET_LISTING = gql`
  ${LISTING_CORE_FIELDS}
  query GetListing($id: UUID!) {
    listing(id: $id) {
      ...ListingCoreFields
    }
  }
`;

type GetListingData = {
  listing: GraphqlListing | null;
};

type GetListingVariables = {
  id: string;
};

export const useListingQuery = (id: string) => {
  const query = useQuery<GetListingData, GetListingVariables>(GET_LISTING, {
    variables: { id },
    fetchPolicy: 'cache-and-network',
    nextFetchPolicy: 'cache-first',
    skip: !id,
  });

  const listing = useMemo<Listing | null>(() => {
    if (!query.data?.listing) {
      return null;
    }
    return mapListing(query.data.listing);
  }, [query.data?.listing]);

  return {
    listing,
    loading: query.loading,
    error: query.error,
    refetch: query.refetch,
  };
};

export { GET_LISTING };

