import { gql, useQuery } from '@apollo/client';
import { useMemo } from 'react';

export interface Photo {
  id: string;
  listingId: string;
  url: string;
  order: number;
}

const GET_PHOTOS = gql`
  query GetPhotos($listingId: UUID!) {
    photos(listingId: $listingId) {
      id
      listingId
      url
      order
    }
  }
`;

type GetPhotosData = {
  photos: Array<{
    id: string;
    listingId: string;
    url: string;
    order: number;
  }>;
};

type GetPhotosVariables = {
  listingId: string;
};

export const usePhotosQuery = (listingId: string, enabled: boolean = true) => {
  const query = useQuery<GetPhotosData, GetPhotosVariables>(GET_PHOTOS, {
    variables: { listingId },
    fetchPolicy: 'cache-and-network',
    nextFetchPolicy: 'cache-first',
    skip: !enabled || !listingId,
    errorPolicy: 'all', // Continue even if auth fails
  });

  const photos = useMemo<Photo[]>(() => {
    if (!query.data?.photos) {
      return [];
    }
    return query.data.photos
      .map((photo) => ({
        id: photo.id,
        listingId: photo.listingId,
        url: photo.url,
        order: photo.order,
      }))
      .sort((a, b) => a.order - b.order);
  }, [query.data?.photos]);

  return {
    photos,
    loading: query.loading,
    error: query.error,
    refetch: query.refetch,
  };
};

export { GET_PHOTOS };


