import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { AppProviders } from './app';
import { AppRouter } from './app/providers/router/RouterProvider';
import { setupLeafletIcons } from './shared/lib/leafletSetup';
import './index.css';

// Initialize Leaflet icons once at app startup
setupLeafletIcons();

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <AppProviders>
      <AppRouter />
    </AppProviders>
  </StrictMode>
);
