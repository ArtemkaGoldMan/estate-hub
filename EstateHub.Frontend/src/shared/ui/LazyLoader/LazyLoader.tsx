import { Suspense, type ReactNode } from 'react';
import { LoadingSpinner } from '../LoadingSpinner';

interface LazyLoaderProps {
  children: ReactNode;
}

export const LazyLoader = ({ children }: LazyLoaderProps) => {
  return (
    <Suspense
      fallback={
        <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh' }}>
          <LoadingSpinner />
        </div>
      }
    >
      {children}
    </Suspense>
  );
};

