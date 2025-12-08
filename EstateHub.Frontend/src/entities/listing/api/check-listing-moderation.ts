import { gql, useQuery } from '@apollo/client';

export interface ModerationResult {
  isApproved: boolean;
  rejectionReason?: string | null;
  suggestions?: string[] | null;
}

const CHECK_LISTING_MODERATION = gql`
  query CheckListingModeration($listingId: UUID!) {
    checkListingModeration(listingId: $listingId) {
      isApproved
      rejectionReason
      suggestions
    }
  }
`;

type CheckListingModerationData = {
  checkListingModeration: ModerationResult;
};

type CheckListingModerationVariables = {
  listingId: string;
};

export const useCheckListingModeration = (listingId: string | null) => {
  const { data, loading, error, refetch } = useQuery<
    CheckListingModerationData,
    CheckListingModerationVariables
  >(CHECK_LISTING_MODERATION, {
    variables: { listingId: listingId! },
    skip: !listingId,
    fetchPolicy: 'network-only', // Always check fresh, don't use cache
    errorPolicy: 'all', // Continue even if there's an error
  });

  return {
    moderationResult: data?.checkListingModeration ?? null,
    loading,
    error,
    refetch,
  };
};












