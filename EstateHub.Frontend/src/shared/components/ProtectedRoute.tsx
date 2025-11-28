import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { LoadingSpinner } from '../ui';

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
  const { isAuthenticated, user } = useAuth();
  const location = useLocation();

  // Check admin permissions if required
  if (requireAdmin) {
    const token = localStorage.getItem('estatehub_access_token');
    if (!token) {
      return <Navigate to="/login" state={{ from: location }} replace />;
    }

    // Parse roles from token
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );
      const decoded = JSON.parse(jsonPayload);
      const roleClaim =
        decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
        decoded.role ||
        decoded.roles ||
        [];
      const roles = Array.isArray(roleClaim) ? roleClaim : [roleClaim].filter(Boolean);

      if (!roles.includes('Admin')) {
        return <Navigate to="/dashboard" state={{ from: location }} replace />;
      }
    } catch {
      return <Navigate to="/login" state={{ from: location }} replace />;
    }
  }

  // Check authentication if required
  if (requireAuth && !isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // Show loading while checking auth
  if (requireAuth && !user) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh' }}>
        <LoadingSpinner />
      </div>
    );
  }

  return <>{children}</>;
};

