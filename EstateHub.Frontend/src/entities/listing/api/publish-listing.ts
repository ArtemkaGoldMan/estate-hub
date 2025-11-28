import { gql, useMutation } from '@apollo/client';
import { GET_LISTING } from './get-listing';
import { GET_MY_LISTINGS } from './get-my-listings';
import { GET_LISTINGS } from './get-listings';

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
    newStatus: 'Draft' | 'Published' | 'Archived';
  };
};

export const useChangeListingStatus = () => {
  const [mutate, { loading, error }] = useMutation<
    ChangeStatusData,
    ChangeStatusVariables
  >(CHANGE_STATUS, {
    refetchQueries: [GET_LISTING, GET_MY_LISTINGS, GET_LISTINGS],
    awaitRefetchQueries: false,
  });

  const changeStatus = async (id: string, newStatus: 'Draft' | 'Published' | 'Archived'): Promise<void> => {
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
    await changeStatus(id, 'Published');
  };

  const unpublishListing = async (id: string): Promise<void> => {
    await changeStatus(id, 'Draft');
  };

  const archiveListing = async (id: string): Promise<void> => {
    await changeStatus(id, 'Archived');
  };

  return {
    changeStatus,
    publishListing,
    unpublishListing,
    archiveListing,
    loading,
    error,
  };
};

// Keep the old hook for backward compatibility
export const usePublishListing = () => {
  const { publishListing, loading, error } = useChangeListingStatus();
  return { publishListing, loading, error };
};

