import { lazy, Suspense } from 'react';
import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import { MainLayout } from '../../layouts/MainLayout';
import { ProtectedRoute, AdminRoute } from '../../../shared/components';
import { LoadingSpinner } from '../../../shared/ui/LoadingSpinner';
import '../../index.css';

// Lazy load pages for code splitting
const ListingsPage = lazy(() => import('../../../pages/Listings/ListingsPage').then(m => ({ default: m.ListingsPage })));
const HomePage = lazy(() => import('../../../pages/Home/HomePage').then(m => ({ default: m.HomePage })));
const ListingDetailPage = lazy(() => import('../../../pages/ListingDetail/ListingDetailPage').then(m => ({ default: m.ListingDetailPage })));
const LoginPage = lazy(() => import('../../../pages/Auth/LoginPage').then(m => ({ default: m.LoginPage })));
const RegisterPage = lazy(() => import('../../../pages/Auth/RegisterPage').then(m => ({ default: m.RegisterPage })));
const EmailConfirmationPage = lazy(() => import('../../../pages/Auth/EmailConfirmationPage').then(m => ({ default: m.EmailConfirmationPage })));
const CheckEmailPage = lazy(() => import('../../../pages/Auth/CheckEmailPage').then(m => ({ default: m.CheckEmailPage })));
const ForgotPasswordPage = lazy(() => import('../../../pages/Auth/ForgotPasswordPage').then(m => ({ default: m.ForgotPasswordPage })));
const ResetPasswordPage = lazy(() => import('../../../pages/Auth/ResetPasswordPage').then(m => ({ default: m.ResetPasswordPage })));
const AccountActionPage = lazy(() => import('../../../pages/Auth/AccountActionPage').then(m => ({ default: m.AccountActionPage })));
const DashboardPage = lazy(() => import('../../../pages/Dashboard/DashboardPage').then(m => ({ default: m.DashboardPage })));
const DashboardListingDetailPage = lazy(() => import('../../../pages/Dashboard/DashboardListingDetailPage').then(m => ({ default: m.DashboardListingDetailPage })));
const ProfilePage = lazy(() => import('../../../pages/Profile/ProfilePage').then(m => ({ default: m.ProfilePage })));
const CreateListingPage = lazy(() => import('../../../pages/CreateListing/CreateListingPage').then(m => ({ default: m.CreateListingPage })));
const EditListingPage = lazy(() => import('../../../pages/EditListing/EditListingPage').then(m => ({ default: m.EditListingPage })));
const ReportsPage = lazy(() => import('../../../pages/Reports/ReportsPage').then(m => ({ default: m.ReportsPage })));
const MapSearchPage = lazy(() => import('../../../pages/MapSearch/MapSearchPage').then(m => ({ default: m.MapSearchPage })));
const AdminUsersPage = lazy(() => import('../../../pages/AdminUsers/AdminUsersPage').then(m => ({ default: m.AdminUsersPage })));
const AboutPage = lazy(() => import('../../../pages/Static/AboutPage').then(m => ({ default: m.AboutPage })));
const TermsPage = lazy(() => import('../../../pages/Static/TermsPage').then(m => ({ default: m.TermsPage })));
const PrivacyPage = lazy(() => import('../../../pages/Static/PrivacyPage').then(m => ({ default: m.PrivacyPage })));

const SuspenseFallback = () => (
  <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh' }}>
    <LoadingSpinner />
  </div>
);

// Router configuration - created at module level (not inside component)
// This ensures the router instance persists across re-renders
const routerConfig = [
  {
    path: '/',
    element: <MainLayout />,
    children: [
      {
        index: true,
        element: (
          <Suspense fallback={<SuspenseFallback />}>
            <HomePage />
          </Suspense>
        ),
      },
      {
        path: 'listings',
        element: (
          <Suspense fallback={<SuspenseFallback />}>
            <ListingsPage />
          </Suspense>
        ),
      },
      {
        path: 'listings/:id',
        element: (
          <Suspense fallback={<SuspenseFallback />}>
            <ListingDetailPage />
          </Suspense>
        ),
      },
      {
        path: 'listings/new',
        element: (
          <ProtectedRoute>
            <Suspense fallback={<SuspenseFallback />}>
              <CreateListingPage />
            </Suspense>
          </ProtectedRoute>
        ),
      },
      {
        path: 'listings/:id/edit',
        element: (
          <ProtectedRoute>
            <Suspense fallback={<SuspenseFallback />}>
              <EditListingPage />
            </Suspense>
          </ProtectedRoute>
        ),
      },
      {
        path: 'login',
        element: (
          <Suspense fallback={<SuspenseFallback />}>
            <LoginPage />
          </Suspense>
        ),
      },
      {
        path: 'register',
        element: (
          <Suspense fallback={<SuspenseFallback />}>
            <RegisterPage />
          </Suspense>
        ),
      },
      {
        path: 'check-email',
        element: (
          <Suspense fallback={<SuspenseFallback />}>
            <CheckEmailPage />
          </Suspense>
        ),
      },
      {
        path: 'confirm-email',
        element: (
          <Suspense fallback={<SuspenseFallback />}>
            <EmailConfirmationPage />
          </Suspense>
        ),
      },
      {
        path: 'auth/forgot',
        element: (
          <Suspense fallback={<SuspenseFallback />}>
            <ForgotPasswordPage />
          </Suspense>
        ),
      },
      {
        path: 'auth/reset',
        element: (
          <Suspense fallback={<SuspenseFallback />}>
            <ResetPasswordPage />
          </Suspense>
        ),
      },
      {
        path: 'auth/account-action',
        element: (
          <Suspense fallback={<SuspenseFallback />}>
            <AccountActionPage />
          </Suspense>
        ),
      },
      {
        path: 'dashboard',
        element: (
          <ProtectedRoute>
            <Suspense fallback={<SuspenseFallback />}>
              <DashboardPage />
            </Suspense>
          </ProtectedRoute>
        ),
      },
      {
        path: 'dashboard/listings/:id',
        element: (
          <ProtectedRoute>
            <Suspense fallback={<SuspenseFallback />}>
              <DashboardListingDetailPage />
            </Suspense>
          </ProtectedRoute>
        ),
      },
      {
        path: 'profile',
        element: (
          <ProtectedRoute>
            <Suspense fallback={<SuspenseFallback />}>
              <ProfilePage />
            </Suspense>
          </ProtectedRoute>
        ),
      },
      {
        path: 'reports',
        element: (
          <ProtectedRoute>
            <Suspense fallback={<SuspenseFallback />}>
              <ReportsPage />
            </Suspense>
          </ProtectedRoute>
        ),
      },
      {
        path: 'map',
        element: (
          <Suspense fallback={<SuspenseFallback />}>
            <MapSearchPage />
          </Suspense>
        ),
      },
      {
        path: 'admin/users',
        element: (
          <AdminRoute>
            <Suspense fallback={<SuspenseFallback />}>
              <AdminUsersPage />
            </Suspense>
          </AdminRoute>
        ),
      },
      {
        path: 'about',
        element: (
          <Suspense fallback={<SuspenseFallback />}>
            <AboutPage />
          </Suspense>
        ),
      },
      {
        path: 'terms',
        element: (
          <Suspense fallback={<SuspenseFallback />}>
            <TermsPage />
          </Suspense>
        ),
      },
      {
        path: 'privacy',
        element: (
          <Suspense fallback={<SuspenseFallback />}>
            <PrivacyPage />
          </Suspense>
        ),
      },
    ],
  },
];

// Create router at module level - this ensures it persists across renders
// and properly manages navigation state
const router = createBrowserRouter(routerConfig, {
  // Preserve scroll position on navigation
  future: {
    v7_startTransition: true,
    v7_relativeSplatPath: true,
  },
});

export const AppRouter = () => {
  return <RouterProvider router={router} />;
};

