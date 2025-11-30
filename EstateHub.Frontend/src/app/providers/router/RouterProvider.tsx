import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import { MainLayout } from '../../layouts/MainLayout';
import '../../index.css';

// Router configuration - created at module level (not inside component)
// This ensures the router instance persists across re-renders
const routerConfig = [
  {
    path: '/',
    element: <MainLayout />,
    children: [],
  },
];

// Create router at module level - this ensures it persists across renders
// and properly manages navigation state
const router = createBrowserRouter(routerConfig);

export const AppRouter = () => {
  return <RouterProvider router={router} />;
};

