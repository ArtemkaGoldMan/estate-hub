import { gql, useMutation } from '@apollo/client';
import type {
  ListingCategory,
  PropertyType,
  ListingCondition,
} from '../model/types';
import { GET_MY_LISTINGS } from './get-my-listings';
import { GET_LISTINGS } from './get-listings';

export interface CreateListingInput {
  category: ListingCategory;
  propertyType: PropertyType;
  title: string;
  description: string;
  addressLine: string;
  district: string;
  city: string;
  postalCode: string;
  latitude: number;
  longitude: number;
  squareMeters: number;
  rooms: number;
  floor?: number | null;
  floorCount?: number | null;
  buildYear?: number | null;
  condition: ListingCondition;
  hasBalcony: boolean;
  hasElevator: boolean;
  hasParkingSpace: boolean;
  hasSecurity: boolean;
  hasStorageRoom: boolean;
  pricePln?: number | null;
  monthlyRentPln?: number | null;
}

const CREATE_LISTING = gql`
  mutation CreateListing($input: CreateListingInputTypeInput!) {
    createListing(input: $input)
  }
`;

type CreateListingData = {
  createListing: string; // Returns Guid as string
};

type CreateListingVariables = {
  input: CreateListingInput;
};

export const useCreateListing = () => {
  const [mutate, { loading, error }] = useMutation<
    CreateListingData,
    CreateListingVariables
  >(CREATE_LISTING, {
    refetchQueries: [GET_MY_LISTINGS, GET_LISTINGS],
    awaitRefetchQueries: false,
  });

  const createListing = async (input: CreateListingInput): Promise<string> => {
    try {
      const result = await mutate({
        variables: { input },
      });

      if (result.errors && result.errors.length > 0) {
        const errorMessage = result.errors
          .map((err) => err.message || JSON.stringify(err))
          .join(', ');
        throw new Error(errorMessage);
      }

      if (!result.data) {
        throw new Error('Failed to create listing: No data returned');
      }

      return result.data.createListing;
    } catch (err) {
      if (err instanceof Error) {
        throw err;
      }
      throw new Error('Failed to create listing: Unknown error');
    }
  };

  return {
    createListing,
    loading,
    error,
  };
};


