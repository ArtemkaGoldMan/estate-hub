import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { LoadingSpinner } from '../ui';
import { hasRole, isTokenExpired } from '../lib/jwt';

const ACCESS_TOKEN_KEY = 'estatehub_access_token';

interface ProtectedRouteProps {
  children: React.ReactNode;
  requireAuth?: boolean;
  requireAdmin?: boolean;
}

export const ProtectedRoute = ({
  children,
  requireAuth = true,
  requireAdmin = false,
}: ProtectedRouteProps) => {
  const { isAuthenticated, isLoading } = useAuth();

  // Show loading while auth context is initializing
  if (isLoading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh' }}>
        <LoadingSpinner />
      </div>
    );
  }

  // Check admin permissions if required
  if (requireAdmin) {
    const token = localStorage.getItem(ACCESS_TOKEN_KEY);
    
    // No token or token is expired
    if (!token || isTokenExpired(token)) {
      return <Navigate to="/" replace />;
    }

    // Check if user has Admin role using secure token parsing
    if (!hasRole(token, 'Admin')) {
      return <Navigate to="/" replace />;
    }
  }

  // Check authentication if required (only after loading is complete)
  if (requireAuth && !isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
};

