import { gql, useQuery } from '@apollo/client';
import { useMemo, useEffect } from 'react';
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

type UseMyListingsQueryOptions = {
  /**
   * Enable automatic polling to check for moderation status updates
   * Polls every 10 seconds when any listing has pending moderation
   */
  enablePolling?: boolean;
  /**
   * Custom polling interval in milliseconds (default: 10000)
   */
  pollInterval?: number;
};

export const useMyListingsQuery = (
  page: number,
  pageSize: number,
  options?: UseMyListingsQueryOptions
) => {
  const { enablePolling = false, pollInterval = 10000 } = options || {};

  const query = useQuery<GetMyListingsData, GetMyListingsVariables>(
    GET_MY_LISTINGS,
    {
      variables: { page, pageSize },
      fetchPolicy: 'cache-and-network',
      nextFetchPolicy: 'cache-first',
      notifyOnNetworkStatusChange: true,
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

  // Check if any listing has pending moderation
  const hasPendingModeration = useMemo(() => {
    if (!data?.items) return false;
    return data.items.some(
      (listing) =>
        listing.status === 'Draft' && listing.isModerationApproved === null
    );
  }, [data?.items]);

  // Determine if we should poll based on moderation status
  const shouldPoll = enablePolling && hasPendingModeration;

  // Dynamically start/stop polling based on moderation status
  useEffect(() => {
    if (shouldPoll) {
      query.startPolling(pollInterval);
    } else {
      query.stopPolling();
    }
    // Cleanup: stop polling when component unmounts or conditions change
    return () => {
      query.stopPolling();
    };
  }, [shouldPoll, pollInterval, query]);

  return {
    data,
    loading: query.loading,
    error: query.error,
    refetch: query.refetch,
  };
};

export { GET_MY_LISTINGS };


