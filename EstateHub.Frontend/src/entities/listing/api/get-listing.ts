import { gql, useQuery } from '@apollo/client';
import { useMemo, useEffect, useRef } from 'react';
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

type UseListingQueryOptions = {
  /**
   * Enable automatic polling to check for moderation status updates
   * Polls every 10 seconds when moderation is pending
   */
  enablePolling?: boolean;
  /**
   * Custom polling interval in milliseconds (default: 10000)
   */
  pollInterval?: number;
};

export const useListingQuery = (
  id: string,
  options?: UseListingQueryOptions
) => {
  const { enablePolling = false, pollInterval = 10000 } = options || {};

  const query = useQuery<GetListingData, GetListingVariables>(GET_LISTING, {
    variables: { id },
    fetchPolicy: 'cache-and-network',
    nextFetchPolicy: 'cache-first',
    skip: !id,
    notifyOnNetworkStatusChange: true,
  });

  const listing = useMemo<Listing | null>(() => {
    if (!query.data?.listing) {
      return null;
    }
    return mapListing(query.data.listing);
  }, [query.data?.listing]);

  // Determine if we should poll based on moderation status
  // Only poll if listing is loaded and moderation is pending
  const shouldPoll =
    enablePolling &&
    !query.loading &&
    listing?.status === 'Draft' &&
    listing?.isModerationApproved === null;

  // Use ref to track previous polling state to avoid unnecessary start/stop calls
  const wasPollingRef = useRef(false);
  const pollIntervalRef = useRef(pollInterval);
  const queryRef = useRef(query);

  // Update refs when values change
  useEffect(() => {
    pollIntervalRef.current = pollInterval;
    queryRef.current = query;
  }, [pollInterval, query]);

  // Dynamically start/stop polling based on moderation status
  useEffect(() => {
    // Wait for query to finish loading before starting polling
    if (query.loading) {
      return;
    }

    // Only start/stop if the state actually changed
    if (shouldPoll && !wasPollingRef.current) {
      wasPollingRef.current = true;
      // Use the current poll interval from ref
      queryRef.current.startPolling(pollIntervalRef.current);
    } else if (!shouldPoll && wasPollingRef.current) {
      wasPollingRef.current = false;
      queryRef.current.stopPolling();
    }

    // Cleanup: stop polling when component unmounts
    return () => {
      if (wasPollingRef.current) {
        queryRef.current.stopPolling();
        wasPollingRef.current = false;
      }
    };
  }, [shouldPoll, query.loading, query]); // Include query to ensure we have the latest reference

  return {
    listing,
    loading: query.loading,
    error: query.error,
    refetch: query.refetch,
  };
};

export { GET_LISTING };

