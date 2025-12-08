import { Button } from '../../../shared';
import type { AuthenticationResponse } from '../../../shared/api/auth';
import './DashboardPageHeader.css';

interface DashboardPageHeaderProps {
  user: AuthenticationResponse | null;
  onCreateListing: () => void;
}

export const DashboardPageHeader = ({ user, onCreateListing }: DashboardPageHeaderProps) => {
  return (
    <div className="dashboard-page__header">
      <div>
        <h1>Dashboard</h1>
        <p>Welcome back, {user?.displayName || user?.email || 'User'}!</p>
      </div>
      <Button variant="primary" onClick={onCreateListing}>
        + Create Listing
      </Button>
    </div>
  );
};



