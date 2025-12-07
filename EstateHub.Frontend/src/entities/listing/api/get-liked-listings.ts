import { gql, useQuery } from '@apollo/client';
import { useMemo } from 'react';
import type { ListingsResponse } from '../model/types';
import type { GraphqlListing } from '../lib/adapters';
import { mapListing } from '../lib/adapters';
import { LISTING_CORE_FIELDS } from './get-listings';

const GET_LIKED_LISTINGS = gql`
  ${LISTING_CORE_FIELDS}
  query GetLikedListings($page: Int!, $pageSize: Int!) {
    likedListings(page: $page, pageSize: $pageSize) {
      items {
        ...ListingCoreFields
      }
      total
    }
  }
`;

type GetLikedListingsData = {
  likedListings: {
    items: GraphqlListing[];
    total: number;
  };
};

type GetLikedListingsVariables = {
  page: number;
  pageSize: number;
};

export const useLikedListingsQuery = (page: number, pageSize: number) => {
  const query = useQuery<GetLikedListingsData, GetLikedListingsVariables>(
    GET_LIKED_LISTINGS,
    {
      variables: { page, pageSize },
      fetchPolicy: 'cache-and-network',
      nextFetchPolicy: 'cache-first',
    }
  );

  const data = useMemo<ListingsResponse | undefined>(() => {
    if (!query.data?.likedListings) {
      return undefined;
    }

    return {
      items: query.data.likedListings.items.map(mapListing),
      total: query.data.likedListings.total,
    };
  }, [query.data?.likedListings]);

  return {
    data,
    loading: query.loading,
    error: query.error,
    refetch: query.refetch,
  };
};

export { GET_LIKED_LISTINGS };

