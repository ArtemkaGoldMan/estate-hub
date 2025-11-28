import { gql, useMutation } from '@apollo/client';
import { GET_MY_REPORTS } from './get-my-reports';

const DELETE_REPORT = gql`
  mutation DeleteReport($id: UUID!) {
    deleteReport(id: $id)
  }
`;

type DeleteReportData = {
  deleteReport: boolean;
};

type DeleteReportVariables = {
  id: string;
};

export const useDeleteReport = () => {
  const [mutate, { loading, error }] = useMutation<DeleteReportData, DeleteReportVariables>(
    DELETE_REPORT,
    {
      refetchQueries: [GET_MY_REPORTS],
      awaitRefetchQueries: false,
    }
  );

  const deleteReport = async (id: string): Promise<void> => {
    const result = await mutate({
      variables: { id },
    });

    if (!result.data?.deleteReport) {
      throw new Error('Failed to delete report');
    }
  };

  return {
    deleteReport,
    loading,
    error,
  };
};


