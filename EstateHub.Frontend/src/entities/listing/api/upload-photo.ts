import { gql, useMutation } from '@apollo/client';
import { GET_PHOTOS } from './get-photos';

const UPLOAD_PHOTO = gql`
  mutation UploadPhoto($listingId: UUID!, $file: Upload!) {
    uploadPhoto(listingId: $listingId, file: $file)
  }
`;

type UploadPhotoData = {
  uploadPhoto: string; // Returns Guid as string
};

type UploadPhotoVariables = {
  listingId: string;
  file: File;
};

export const useUploadPhoto = () => {
  const [mutate, { loading, error }] = useMutation<
    UploadPhotoData,
    UploadPhotoVariables
  >(UPLOAD_PHOTO, {
    refetchQueries: [GET_PHOTOS],
    awaitRefetchQueries: false,
  });

  const uploadPhoto = async (
    listingId: string,
    file: File
  ): Promise<string> => {
    const result = await mutate({
      variables: { listingId, file },
    });

    if (!result.data) {
      throw new Error('Failed to upload photo');
    }

    return result.data.uploadPhoto;
  };

  return {
    uploadPhoto,
    loading,
    error,
  };
};


