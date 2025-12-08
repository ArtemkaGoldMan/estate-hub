import { gql, useQuery } from '@apollo/client';
import { useMemo } from 'react';
import type { ListingsResponse } from '../model/types';
import type { GraphqlListing } from '../lib/adapters';
import { mapListing } from '../lib/adapters';
import { LISTING_CORE_FIELDS } from './get-listings';

const GET_ARCHIVED_LISTINGS = gql`
  ${LISTING_CORE_FIELDS}
  query GetArchivedListings($page: Int!, $pageSize: Int!) {
    archivedListings(page: $page, pageSize: $pageSize) {
      items {
        ...ListingCoreFields
      }
      total
    }
  }
`;

type GetArchivedListingsData = {
  archivedListings: {
    items: GraphqlListing[];
    total: number;
  };
};

type GetArchivedListingsVariables = {
  page: number;
  pageSize: number;
};

export const useArchivedListingsQuery = (
  page: number,
  pageSize: number,
  options?: { skip?: boolean }
) => {
  const { skip = false } = options || {};

  const query = useQuery<GetArchivedListingsData, GetArchivedListingsVariables>(
    GET_ARCHIVED_LISTINGS,
    {
      variables: { page, pageSize },
      fetchPolicy: 'cache-and-network',
      nextFetchPolicy: 'cache-first',
      skip,
    }
  );

  const data = useMemo<ListingsResponse | undefined>(() => {
    if (!query.data?.archivedListings) {
      return undefined;
    }

    return {
      items: query.data.archivedListings.items.map(mapListing),
      total: query.data.archivedListings.total,
    };
  }, [query.data?.archivedListings]);

  return {
    data,
    loading: query.loading,
    error: query.error,
    refetch: query.refetch,
  };
};

export { GET_ARCHIVED_LISTINGS };




