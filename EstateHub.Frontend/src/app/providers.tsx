import { ApolloProvider } from '@apollo/client';
import type { ReactNode } from 'react';
import { graphqlClient } from '../shared';
import { AuthProvider } from '../shared/context/AuthContext';
import { ToastProvider } from '../shared/context/ToastContext';
import { ErrorBoundary } from '../shared/ui/ErrorBoundary';

interface AppProvidersProps {
  children: ReactNode;
}

export const AppProviders = ({ children }: AppProvidersProps) => {
  return (
    <ErrorBoundary>
      <ApolloProvider client={graphqlClient}>
        <AuthProvider>
          <ToastProvider>{children}</ToastProvider>
        </AuthProvider>
      </ApolloProvider>
    </ErrorBoundary>
  );
};

