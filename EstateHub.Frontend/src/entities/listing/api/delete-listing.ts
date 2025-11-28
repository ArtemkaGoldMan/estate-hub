import { gql, useMutation } from '@apollo/client';
import { GET_MY_LISTINGS } from './get-my-listings';
import { GET_LISTINGS } from './get-listings';

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
    refetchQueries: [GET_MY_LISTINGS, GET_LISTINGS],
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


