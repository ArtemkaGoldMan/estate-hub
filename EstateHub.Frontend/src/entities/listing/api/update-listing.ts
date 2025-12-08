import { gql, useMutation } from '@apollo/client';
import type {
  ListingCondition,
} from '../model/types';
import { GET_LISTING } from './get-listing';
import { GET_MY_LISTINGS } from './get-my-listings';

export interface UpdateListingInput {
  title?: string | null;
  description?: string | null;
  addressLine?: string | null;
  district?: string | null;
  city?: string | null;
  postalCode?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  squareMeters?: number | null;
  rooms?: number | null;
  floor?: number | null;
  floorCount?: number | null;
  buildYear?: number | null;
  condition?: ListingCondition | null;
  hasBalcony?: boolean | null;
  hasElevator?: boolean | null;
  hasParkingSpace?: boolean | null;
  hasSecurity?: boolean | null;
  hasStorageRoom?: boolean | null;
  pricePln?: number | null;
  monthlyRentPln?: number | null;
}

const UPDATE_LISTING = gql`
  mutation UpdateListing($id: UUID!, $input: UpdateListingInputTypeInput!) {
    updateListing(id: $id, input: $input)
  }
`;

type UpdateListingData = {
  updateListing: boolean;
};

type UpdateListingVariables = {
  id: string;
  input: UpdateListingInput;
};

export const useUpdateListing = () => {
  const [mutate, { loading, error }] = useMutation<
    UpdateListingData,
    UpdateListingVariables
  >(UPDATE_LISTING, {
    // Only refetch queries that don't require variables
    // GET_LISTINGS requires variables and causes errors when refetched
    // Pages using GET_LISTINGS should handle their own refetching
    refetchQueries: [GET_LISTING, GET_MY_LISTINGS],
    awaitRefetchQueries: false,
  });

  const updateListing = async (
    id: string,
    input: UpdateListingInput
  ): Promise<void> => {
    const result = await mutate({
      variables: { id, input },
    });

    if (!result.data?.updateListing) {
      throw new Error('Failed to update listing');
    }
  };

  return {
    updateListing,
    loading,
    error,
  };
};

