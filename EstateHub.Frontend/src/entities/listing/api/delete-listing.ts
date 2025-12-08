import { gql, useMutation } from '@apollo/client';
import { GET_MY_LISTINGS } from './get-my-listings';

const DELETE_LISTING = gql`
  mutation DeleteListing($id: UUID!) {
    deleteListing(id: $id)
  }
`;

type DeleteListingData = {
  deleteListing: boolean;
};

type DeleteListingVariables = {
  id: string;
};

export const useDeleteListing = () => {
  const [mutate, { loading, error }] = useMutation<
    DeleteListingData,
    DeleteListingVariables
  >(DELETE_LISTING, {
    // Only refetch GET_MY_LISTINGS - GET_LISTINGS requires variables and causes errors
    // Pages using GET_LISTINGS should handle their own refetching
    refetchQueries: [GET_MY_LISTINGS],
    awaitRefetchQueries: false,
  });

  const deleteListing = async (id: string): Promise<void> => {
    const result = await mutate({
      variables: { id },
    });

    if (!result.data?.deleteListing) {
      throw new Error('Failed to delete listing');
    }
  };

  return {
    deleteListing,
    loading,
    error,
  };
};


