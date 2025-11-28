import { gql, useMutation } from '@apollo/client';
import type { ResolveReportInput } from '../model/types';
import { GET_REPORTS } from './get-reports';
import { GET_REPORTS_FOR_MODERATION } from './get-reports-for-moderation';

const RESOLVE_REPORT = gql`
  mutation ResolveReport($input: ResolveReportInputTypeInput!) {
    resolveReport(input: $input)
  }
`;

type ResolveReportData = {
  resolveReport: boolean;
};

type ResolveReportVariables = {
  input: ResolveReportInput;
};

export const useResolveReport = () => {
  const [mutate, { loading, error }] = useMutation<ResolveReportData, ResolveReportVariables>(
    RESOLVE_REPORT,
    {
      refetchQueries: [GET_REPORTS, GET_REPORTS_FOR_MODERATION],
      awaitRefetchQueries: false,
    }
  );

  const resolveReport = async (input: ResolveReportInput): Promise<void> => {
    const result = await mutate({
      variables: { input },
    });

    if (!result.data?.resolveReport) {
      throw new Error('Failed to resolve report');
    }
  };

  return {
    resolveReport,
    loading,
    error,
  };
};

