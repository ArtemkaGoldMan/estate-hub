import { gql, useMutation } from '@apollo/client';
import type { CreateReportInput } from '../model/types';
import { GET_MY_REPORTS } from './get-my-reports';

const CREATE_REPORT = gql`
  mutation CreateReport($input: CreateReportInputTypeInput!) {
    createReport(input: $input)
  }
`;

type CreateReportData = {
  createReport: string; // Returns Guid as string
};

type CreateReportVariables = {
  input: CreateReportInput;
};

export const useCreateReport = () => {
  const [mutate, { loading, error }] = useMutation<CreateReportData, CreateReportVariables>(
    CREATE_REPORT,
    {
      refetchQueries: [GET_MY_REPORTS],
      awaitRefetchQueries: false,
    }
  );

  const createReport = async (input: CreateReportInput): Promise<string> => {
    const result = await mutate({
      variables: { input },
    });

    if (!result.data?.createReport) {
      throw new Error('Failed to create report');
    }

    return result.data.createReport;
  };

  return {
    createReport,
    loading,
    error,
  };
};


