import { gql, useQuery } from '@apollo/client';
import type { PagedReports } from '../model/types';

const GET_MY_REPORTS = gql`
  query GetMyReports($page: Int!, $pageSize: Int!) {
    myReports(page: $page, pageSize: $pageSize) {
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

type GetMyReportsData = {
  myReports: PagedReports;
};

type GetMyReportsVariables = {
  page: number;
  pageSize: number;
};

export const useMyReportsQuery = (page: number, pageSize: number) => {
  const query = useQuery<GetMyReportsData, GetMyReportsVariables>(GET_MY_REPORTS, {
    variables: { page, pageSize },
    fetchPolicy: 'cache-and-network',
    nextFetchPolicy: 'cache-first',
  });

  return {
    data: query.data?.myReports,
    loading: query.loading,
    error: query.error,
    refetch: query.refetch,
  };
};

export { GET_MY_REPORTS };

