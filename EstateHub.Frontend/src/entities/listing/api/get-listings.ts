import { gql, useQuery } from '@apollo/client';
import { useMemo } from 'react';
import type { ListingFilter, ListingsRequest, ListingsResponse } from '../model/types';
import type { GraphqlListing } from '../lib/adapters';
import { mapListing } from '../lib/adapters';

const LISTING_CORE_FIELDS = gql`
  fragment ListingCoreFields on ListingType {
    id
    ownerId
    title
    description
    pricePln
    monthlyRentPln
    status
    category
    propertyType
    city
    district
    latitude
    longitude
    squareMeters
    rooms
    floor
    floorCount
    buildYear
    condition
    hasBalcony
    hasElevator
    hasParkingSpace
    hasSecurity
    hasStorageRoom
    createdAt
    updatedAt
    publishedAt
    archivedAt
    firstPhotoUrl
    isLikedByCurrentUser
    isModerationApproved
    moderationCheckedAt
    moderationRejectionReason
  }
`;

const GET_LISTINGS = gql`
  ${LISTING_CORE_FIELDS}
  query GetListings($filter: ListingFilterTypeInput, $page: Int!, $pageSize: Int!) {
    listings(filter: $filter, page: $page, pageSize: $pageSize) {
      items {
        ...ListingCoreFields
      }
      total
    }
  }
`;

const SEARCH_LISTINGS = gql`
  ${LISTING_CORE_FIELDS}
  query SearchListings(
    $text: String!
    $filter: ListingFilterTypeInput
    $page: Int!
    $pageSize: Int!
  ) {
    searchListings(
      text: $text
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

export { LISTING_CORE_FIELDS, GET_LISTINGS, SEARCH_LISTINGS };

type GetListingsData = {
  listings: {
    items: GraphqlListing[];
    total: number;
  };
};

type SearchListingsData = {
  searchListings: {
    items: GraphqlListing[];
    total: number;
  };
};

type GetListingsVariables = {
  filter?: ListingFilter;
  page: number;
  pageSize: number;
};

type SearchListingsVariables = GetListingsVariables & {
  text: string;
};

export const useListingsQuery = ({
  filter,
  page,
  pageSize,
  search,
}: ListingsRequest) => {
  const trimmedSearch = search?.trim();
  const shouldSearch = Boolean(trimmedSearch);

  const listingsQuery = useQuery<GetListingsData, GetListingsVariables>(
    GET_LISTINGS,
    {
      variables: { filter, page, pageSize },
      skip: shouldSearch,
      fetchPolicy: 'cache-and-network',
      nextFetchPolicy: 'cache-first',
    }
  );

  const searchQuery = useQuery<SearchListingsData, SearchListingsVariables>(
    SEARCH_LISTINGS,
    {
      variables: {
        filter,
        page,
        pageSize,
        text: trimmedSearch ?? '',
      },
      skip: !shouldSearch,
      fetchPolicy: 'cache-and-network',
      nextFetchPolicy: 'cache-first',
    }
  );

  const activeQuery = shouldSearch ? searchQuery : listingsQuery;

  const data = useMemo<ListingsResponse | undefined>(() => {
    const payload = shouldSearch
      ? searchQuery.data?.searchListings
      : listingsQuery.data?.listings;

    if (!payload) {
      return undefined;
    }

    return {
      items: payload.items.map(mapListing),
      total: payload.total,
    };
  }, [
    listingsQuery.data?.listings,
    searchQuery.data?.searchListings,
    shouldSearch,
  ]);

  return {
    data,
    loading: listingsQuery.loading || searchQuery.loading,
    error: listingsQuery.error ?? searchQuery.error,
    refetch: activeQuery.refetch,
    fetchMore: activeQuery.fetchMore,
    networkStatus: activeQuery.networkStatus,
  };
};


