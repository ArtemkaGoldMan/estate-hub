import { gql, useMutation } from '@apollo/client';
import { GET_LISTING } from './get-listing';

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
  const [likeListing, { loading: likeLoading, error: likeError }] = useMutation(LIKE_LISTING, {
    // Only refetch the specific listing query, not all queries
    refetchQueries: [GET_LISTING],
    awaitRefetchQueries: false,
  });
  const [unlikeListing, { loading: unlikeLoading, error: unlikeError }] = useMutation(UNLIKE_LISTING, {
    // Only refetch the specific listing query, not all queries
    refetchQueries: [GET_LISTING],
    awaitRefetchQueries: false,
  });

  const toggleLike = async (listingId: string, isCurrentlyLiked: boolean) => {
    if (isCurrentlyLiked) {
      await unlikeListing({
        variables: { id: listingId },
      });
    } else {
      await likeListing({
        variables: { id: listingId },
      });
    }
  };

  return {
    toggleLike,
    loading: likeLoading || unlikeLoading,
    error: likeError || unlikeError,
  };
};


