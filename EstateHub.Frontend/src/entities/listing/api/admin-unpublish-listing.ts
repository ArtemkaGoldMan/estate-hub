import { gql, useMutation } from '@apollo/client';
import { GET_LISTING } from './get-listing';
import { GET_MY_LISTINGS } from './get-my-listings';
import { GET_ARCHIVED_LISTINGS } from './get-archived-listings';

const ADMIN_UNPUBLISH_LISTING = gql`
  mutation AdminUnpublishListing($id: UUID!, $input: AdminUnpublishListingInputTypeInput!) {
    adminUnpublishListing(id: $id, input: $input)
  }
`;

type AdminUnpublishListingData = {
  adminUnpublishListing: boolean;
};

type AdminUnpublishListingVariables = {
  id: string;
  input: {
    reason: string;
  };
};

export const useAdminUnpublishListing = () => {
  const [mutate, { loading, error }] = useMutation<
    AdminUnpublishListingData,
    AdminUnpublishListingVariables
  >(ADMIN_UNPUBLISH_LISTING, {
    refetchQueries: [GET_MY_LISTINGS, GET_ARCHIVED_LISTINGS],
    awaitRefetchQueries: false,
  });

  const adminUnpublishListing = async (id: string, reason: string): Promise<void> => {
    const result = await mutate({
      variables: { 
        id, 
        input: { reason } 
      },
      // Also refetch the specific listing if it's in cache
      refetchQueries: [
        {
          query: GET_LISTING,
          variables: { id },
        },
      ],
    });

    if (!result.data?.adminUnpublishListing) {
      throw new Error('Failed to unpublish listing');
    }
  };

  return {
    adminUnpublishListing,
    loading,
    error,
  };
};

