import { gql, useQuery } from '@apollo/client';
import { useMemo } from 'react';
import type { ListingsResponse } from '../model/types';
import type { GraphqlListing } from '../lib/adapters';
import { mapListing } from '../lib/adapters';
import { LISTING_CORE_FIELDS } from './get-listings';

const GET_MY_LISTINGS = gql`
  ${LISTING_CORE_FIELDS}
  query GetMyListings($page: Int!, $pageSize: Int!) {
    myListings(page: $page, pageSize: $pageSize) {
      items {
        ...ListingCoreFields
      }
      total
    }
  }
`;

type GetMyListingsData = {
  myListings: {
    items: GraphqlListing[];
    total: number;
  };
};

type GetMyListingsVariables = {
  page: number;
  pageSize: number;
};

export const useMyListingsQuery = (page: number, pageSize: number) => {
  const query = useQuery<GetMyListingsData, GetMyListingsVariables>(
    GET_MY_LISTINGS,
    {
      variables: { page, pageSize },
      fetchPolicy: 'cache-and-network',
      nextFetchPolicy: 'cache-first',
    }
  );

  const data = useMemo<ListingsResponse | undefined>(() => {
    if (!query.data?.myListings) {
      return undefined;
    }

    return {
      items: query.data.myListings.items.map(mapListing),
      total: query.data.myListings.total,
    };
  }, [query.data?.myListings]);

  return {
    data,
    loading: query.loading,
    error: query.error,
    refetch: query.refetch,
  };
};

export { GET_MY_LISTINGS };


