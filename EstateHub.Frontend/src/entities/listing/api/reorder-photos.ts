import { gql, useMutation } from '@apollo/client';
import { GET_PHOTOS } from './get-photos';

export interface PhotoOrder {
  photoId: string;
  order: number;
}

export interface ReorderPhotosInput {
  listingId: string;
  photoOrders: PhotoOrder[];
}

const REORDER_PHOTOS = gql`
  mutation ReorderPhotos($input: ReorderPhotosInputTypeInput!) {
    reorderPhotos(input: $input)
  }
`;

type ReorderPhotosData = {
  reorderPhotos: boolean;
};

type ReorderPhotosVariables = {
  input: ReorderPhotosInput;
};

export const useReorderPhotos = () => {
  const [mutate, { loading, error }] = useMutation<
    ReorderPhotosData,
    ReorderPhotosVariables
  >(REORDER_PHOTOS, {
    refetchQueries: [GET_PHOTOS],
    awaitRefetchQueries: false,
  });

  const reorderPhotos = async (
    input: ReorderPhotosInput
  ): Promise<void> => {
    const result = await mutate({
      variables: { input },
    });

    if (!result.data?.reorderPhotos) {
      throw new Error('Failed to reorder photos');
    }
  };

  return {
    reorderPhotos,
    loading,
    error,
  };
};


