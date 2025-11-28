import { gql, useQuery } from '@apollo/client';
import { useMemo } from 'react';
import type {
  ListingFilter,
  ListingsResponse,
  MapBounds,
} from '../model/types';
import type { GraphqlListing } from '../lib/adapters';
import { mapListing } from '../lib/adapters';
import { LISTING_CORE_FIELDS } from './get-listings';

const GET_LISTINGS_ON_MAP = gql`
  ${LISTING_CORE_FIELDS}
  query GetListingsOnMap(
    $bounds: BoundsInputTypeInput!
    $filter: ListingFilterTypeInput
    $page: Int!
    $pageSize: Int!
  ) {
    listingsOnMap(
      bounds: $bounds
      filter: $filter
      page: $page
      pageSize: $pageSize
    ) {
      items {
        ...ListingCoreFields
      }
      total
    }
  }
`;

type GetListingsOnMapData = {
  listingsOnMap: {
    items: GraphqlListing[];
    total: number;
  };
};

type GetListingsOnMapVariables = {
  bounds: MapBounds;
  filter?: ListingFilter;
  page: number;
  pageSize: number;
};

export const useListingsOnMapQuery = ({
  bounds,
  filter,
  page,
  pageSize,
  enabled = true,
}: {
  bounds?: MapBounds;
  filter?: ListingFilter;
  page: number;
  pageSize: number;
  enabled?: boolean;
}) => {
  // Always skip if not enabled or no bounds
  const skip = !enabled || !bounds;

  // Provide default bounds when skipped to satisfy GraphQL schema
  const defaultBounds: MapBounds = {
    latMin: 0,
    latMax: 0,
    lonMin: 0,
    lonMax: 0,
  };

  const variables: GetListingsOnMapVariables = {
    bounds: bounds || defaultBounds,
    filter,
    page,
    pageSize,
  };

  const query = useQuery<GetListingsOnMapData, GetListingsOnMapVariables>(
    GET_LISTINGS_ON_MAP,
    {
      variables,
      skip,
      fetchPolicy: 'cache-and-network',
      nextFetchPolicy: 'cache-first',
    }
  );

  const data = useMemo<ListingsResponse | undefined>(() => {
    if (skip || !query.data?.listingsOnMap) {
      return undefined;
    }

    return {
      items: query.data.listingsOnMap.items.map(mapListing),
      total: query.data.listingsOnMap.total,
    };
  }, [query.data?.listingsOnMap, skip]);

  return {
    data,
    loading: query.loading,
    error: query.error,
    refetch: query.refetch,
  };
};

export { GET_LISTINGS_ON_MAP };


