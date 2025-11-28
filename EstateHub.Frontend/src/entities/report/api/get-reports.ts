import { gql, useQuery } from '@apollo/client';
import type { PagedReports, ReportFilter } from '../model/types';

const GET_REPORTS = gql`
  query GetReports($filter: ReportFilterTypeInput, $page: Int!, $pageSize: Int!) {
    reports(filter: $filter, page: $page, pageSize: $pageSize) {
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

type GetReportsData = {
  reports: PagedReports;
};

type GetReportsVariables = {
  filter?: ReportFilter | null;
  page: number;
  pageSize: number;
};

export const useReportsQuery = (filter: ReportFilter | null, page: number, pageSize: number, enabled: boolean = true) => {
  const query = useQuery<GetReportsData, GetReportsVariables>(GET_REPORTS, {
    variables: { filter: filter || null, page, pageSize },
    fetchPolicy: 'cache-and-network',
    nextFetchPolicy: 'cache-first',
    skip: !enabled, // Only query if enabled (user has ViewReports permission)
  });

  return {
    data: query.data?.reports,
    loading: query.loading,
    error: query.error,
    refetch: query.refetch,
  };
};

export { GET_REPORTS };

