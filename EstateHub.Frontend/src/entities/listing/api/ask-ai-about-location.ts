import { gql, useLazyQuery, useQuery, ApolloError } from '@apollo/client';

/**
 * Extracts user-friendly error messages from GraphQL or regular errors
 */
function extractUserFriendlyError(error: Error | ApolloError): string {
  // Handle Apollo GraphQL errors
  if ('graphQLErrors' in error && error.graphQLErrors) {
    const graphQLError = error.graphQLErrors[0];
    // Check for user-friendly message in extensions
    if (graphQLError.extensions?.userMessage) {
      return graphQLError.extensions.userMessage as string;
    }
    // Check for message in extensions
    if (graphQLError.extensions?.message) {
      return graphQLError.extensions.message as string;
    }
    // Use the error message if it's user-friendly
    if (graphQLError.message && !graphQLError.message.includes('GraphQL') && !graphQLError.message.includes('field')) {
      return graphQLError.message;
    }
  }

  // Handle network errors
  if ('networkError' in error) {
    return 'Unable to connect to the server. Please check your internet connection and try again.';
  }

  // Handle regular errors
  const message = error.message || 'An unexpected error occurred';
  
  // Filter out technical error messages
  if (message.includes('GraphQL') || message.includes('field') || message.includes('type')) {
    return 'Unable to process your request. Please try again later.';
  }

  // Check for specific user-friendly messages
  if (message.includes('Daily limit reached') || message.includes('limit')) {
    return message; // Keep limit messages as-is
  }

  if (message.includes('not authenticated') || message.includes('unauthorized')) {
    return 'Please log in to use this feature.';
  }

  if (message.includes('not found')) {
    return 'The requested information could not be found.';
  }

  // Default user-friendly message
  return message.length > 100 || message.includes('Error:') 
    ? 'Something went wrong. Please try again later.'
    : message;
}

const ASK_ABOUT_LOCATION = gql`
  query AskAboutLocation($listingId: UUID!, $questionId: String!) {
    askAboutLocation(listingId: $listingId, questionId: $questionId) {
      answer
      remainingQuestions
    }
  }
`;

const GET_REMAINING_AI_QUESTIONS = gql`
  query GetRemainingAIQuestions {
    getRemainingAIQuestions
  }
`;

type AskAboutLocationVariables = {
  listingId: string;
  questionId: string;
};

type AskAboutLocationResult = {
  answer: string;
  remainingQuestions: number;
};

type AskAboutLocationData = {
  askAboutLocation: AskAboutLocationResult;
};

type RemainingQuestionsData = {
  getRemainingAIQuestions: number;
};

export const useAskAboutLocation = () => {
  const [query, { loading, error }] = useLazyQuery<
    AskAboutLocationData,
    AskAboutLocationVariables
  >(ASK_ABOUT_LOCATION, {
    fetchPolicy: 'network-only', // Always fetch fresh data
  });

  const askAboutLocation = async (
    listingId: string,
    questionId: string
  ): Promise<{ answer: string; remainingQuestions: number }> => {
    try {
      const result = await query({
        variables: { listingId, questionId },
      });

      if (result.error) {
        // Extract user-friendly message from GraphQL error
        const errorMessage = extractUserFriendlyError(result.error);
        throw new Error(errorMessage);
      }

      if (!result.data) {
        throw new Error('Unable to get AI response. Please try again.');
      }

      return {
        answer: result.data.askAboutLocation.answer,
        remainingQuestions: result.data.askAboutLocation.remainingQuestions,
      };
    } catch (err) {
      // Convert any error to user-friendly message
      if (err instanceof Error) {
        throw new Error(extractUserFriendlyError(err));
      }
      throw new Error('An unexpected error occurred. Please try again.');
    }
  };

  return {
    askAboutLocation,
    loading,
    error,
  };
};

export const useRemainingAIQuestions = (enabled: boolean = true) => {
  const { data, loading, error, refetch } = useQuery<RemainingQuestionsData>(
    GET_REMAINING_AI_QUESTIONS,
    {
      fetchPolicy: 'network-only',
      pollInterval: 0, // Don't poll by default
      errorPolicy: 'ignore', // Ignore errors and just return default value
      skip: !enabled, // Only fetch if enabled (e.g., when authenticated)
    }
  );

  // If query fails or doesn't exist, default to 5 (full limit)
  // This prevents errors from breaking the UI
  const remainingQuestions = error ? 5 : (data?.getRemainingAIQuestions ?? 5);

  return {
    remainingQuestions,
    loading: loading && !error, // Don't show loading if there's an error
    error: null, // Don't expose technical errors to consumers
    refetch: async () => {
      try {
        await refetch();
      } catch {
        // Silently handle refetch errors
      }
    },
  };
};

export { ASK_ABOUT_LOCATION, GET_REMAINING_AI_QUESTIONS };

