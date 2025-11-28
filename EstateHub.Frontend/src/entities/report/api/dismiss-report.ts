import { gql, useMutation } from '@apollo/client';
import type { DismissReportInput } from '../model/types';
import { GET_REPORTS } from './get-reports';
import { GET_REPORTS_FOR_MODERATION } from './get-reports-for-moderation';

const DISMISS_REPORT = gql`
  mutation DismissReport($input: DismissReportInputTypeInput!) {
    dismissReport(input: $input)
  }
`;

type DismissReportData = {
  dismissReport: boolean;
};

type DismissReportVariables = {
  input: DismissReportInput;
};

export const useDismissReport = () => {
  const [mutate, { loading, error }] = useMutation<DismissReportData, DismissReportVariables>(
    DISMISS_REPORT,
    {
      refetchQueries: [GET_REPORTS, GET_REPORTS_FOR_MODERATION],
      awaitRefetchQueries: false,
    }
  );

  const dismissReport = async (input: DismissReportInput): Promise<void> => {
    const result = await mutate({
      variables: { input },
    });

    if (!result.data?.dismissReport) {
      throw new Error('Failed to dismiss report');
    }
  };

  return {
    dismissReport,
    loading,
    error,
  };
};

