import { gql, useMutation } from '@apollo/client';
import { GET_MY_LISTINGS } from './get-my-listings';
import { GET_ARCHIVED_LISTINGS } from './get-archived-listings';

const CHANGE_STATUS = gql`
  mutation ChangeStatus($id: UUID!, $input: ChangeStatusInputTypeInput!) {
    changeStatus(id: $id, input: $input)
  }
`;

type ChangeStatusData = {
  changeStatus: boolean;
};

type ChangeStatusVariables = {
  id: string;
  input: {
    newStatus: 'DRAFT' | 'PUBLISHED' | 'ARCHIVED';
  };
};

export const useChangeListingStatus = () => {
  const [mutate, { loading, error }] = useMutation<
    ChangeStatusData,
    ChangeStatusVariables
  >(CHANGE_STATUS, {
    // Only refetch queries that don't require variables
    // GET_LISTINGS requires variables and causes errors when refetched
    // Pages using GET_LISTINGS should handle their own refetching
    refetchQueries: [GET_MY_LISTINGS, GET_ARCHIVED_LISTINGS],
    awaitRefetchQueries: false,
  });

  const changeStatus = async (id: string, newStatus: 'DRAFT' | 'PUBLISHED' | 'ARCHIVED'): Promise<void> => {
    const result = await mutate({
      variables: { 
        id, 
        input: { newStatus } 
      },
    });

    if (!result.data?.changeStatus) {
      throw new Error(`Failed to change listing status to ${newStatus}`);
    }
  };

  const publishListing = async (id: string): Promise<void> => {
    await changeStatus(id, 'PUBLISHED');
  };

  const unpublishListing = async (id: string): Promise<void> => {
    await changeStatus(id, 'DRAFT');
  };

  const archiveListing = async (id: string): Promise<void> => {
    await changeStatus(id, 'ARCHIVED');
  };

  const unarchiveListing = async (id: string): Promise<void> => {
    await changeStatus(id, 'DRAFT');
  };

  return {
    changeStatus,
    publishListing,
    unpublishListing,
    archiveListing,
    unarchiveListing,
    loading,
    error,
  };
};

// Keep the old hook for backward compatibility
export const usePublishListing = () => {
  const { publishListing, loading, error } = useChangeListingStatus();
  return { publishListing, loading, error };
};

