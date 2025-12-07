import { gql, useMutation } from '@apollo/client';
import { GET_LISTING } from './get-listing';
import { GET_LISTINGS, SEARCH_LISTINGS } from './get-listings';
import { GET_LISTINGS_ON_MAP } from './get-listings-on-map';
import { GET_LIKED_LISTINGS } from './get-liked-listings';

const LIKE_LISTING = gql`
  mutation LikeListing($id: UUID!) {
    likeListing(id: $id)
  }
`;

const UNLIKE_LISTING = gql`
  mutation UnlikeListing($id: UUID!) {
    unlikeListing(id: $id)
  }
`;

export const useLikeListing = () => {
  const [likeListing, { loading: likeLoading, error: likeError }] = useMutation(LIKE_LISTING);
  const [unlikeListing, { loading: unlikeLoading, error: unlikeError }] = useMutation(UNLIKE_LISTING);

  const toggleLike = async (listingId: string, isCurrentlyLiked: boolean) => {
    try {
      const refetchQueries = [GET_LISTING, GET_LISTINGS, SEARCH_LISTINGS, GET_LISTINGS_ON_MAP, GET_LIKED_LISTINGS];
      
      if (isCurrentlyLiked) {
        await unlikeListing({
          variables: { id: listingId },
          refetchQueries,
          awaitRefetchQueries: false,
        });
      } else {
        await likeListing({
          variables: { id: listingId },
          refetchQueries,
          awaitRefetchQueries: false,
        });
      }
    } catch (error) {
      // Error is thrown to be handled by the calling component
      throw error;
    }
  };

  return {
    toggleLike,
    loading: likeLoading || unlikeLoading,
    error: likeError || unlikeError,
  };
};


