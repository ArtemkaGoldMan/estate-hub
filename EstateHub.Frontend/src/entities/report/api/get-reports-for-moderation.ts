import { gql, useQuery } from '@apollo/client';
import type { PagedReports } from '../model/types';

const GET_REPORTS_FOR_MODERATION = gql`
  query GetReportsForModeration($page: Int!, $pageSize: Int!) {
    reportsForModeration(page: $page, pageSize: $pageSize) {
      items {
        id
        reporterId
        listingId
        reason
        description
        status
        moderatorId
        moderatorNotes
        resolution
        createdAt
        updatedAt
        resolvedAt
        reporterEmail
        moderatorEmail
        listingTitle
      }
      total
    }
  }
`;

type GetReportsForModerationData = {
  reportsForModeration: PagedReports;
};

type GetReportsForModerationVariables = {
  page: number;
  pageSize: number;
};

export const useReportsForModerationQuery = (
  page: number,
  pageSize: number,
  options?: { skip?: boolean }
) => {
  const { skip = false } = options || {};

  const query = useQuery<GetReportsForModerationData, GetReportsForModerationVariables>(
    GET_REPORTS_FOR_MODERATION,
    {
      variables: { page, pageSize },
      fetchPolicy: 'cache-and-network',
      nextFetchPolicy: 'cache-first',
      skip,
    }
  );

  return {
    data: query.data?.reportsForModeration,
    loading: query.loading,
    error: query.error,
    refetch: query.refetch,
  };
};

export { GET_REPORTS_FOR_MODERATION };

