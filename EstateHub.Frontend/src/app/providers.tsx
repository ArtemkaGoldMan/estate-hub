import { BrowserRouter } from 'react-router-dom';
import type { ReactNode } from 'react';

interface AppProvidersProps {
  children: ReactNode;
}

export const AppProviders = ({ children }: AppProvidersProps) => {
  return <BrowserRouter>{children}</BrowserRouter>;
};

