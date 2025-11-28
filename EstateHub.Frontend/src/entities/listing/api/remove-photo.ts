import { gql, useMutation } from '@apollo/client';
import { GET_PHOTOS } from './get-photos';

const REMOVE_PHOTO = gql`
  mutation RemovePhoto($listingId: UUID!, $photoId: UUID!) {
    removePhoto(listingId: $listingId, photoId: $photoId)
  }
`;

type RemovePhotoData = {
  removePhoto: boolean;
};

type RemovePhotoVariables = {
  listingId: string;
  photoId: string;
};

export const useRemovePhoto = () => {
  const [mutate, { loading, error }] = useMutation<
    RemovePhotoData,
    RemovePhotoVariables
  >(REMOVE_PHOTO, {
    refetchQueries: [GET_PHOTOS],
    awaitRefetchQueries: false,
  });

  const removePhoto = async (
    listingId: string,
    photoId: string
  ): Promise<void> => {
    const result = await mutate({
      variables: { listingId, photoId },
    });

    if (!result.data?.removePhoto) {
      throw new Error('Failed to remove photo');
    }
  };

  return {
    removePhoto,
    loading,
    error,
  };
};


