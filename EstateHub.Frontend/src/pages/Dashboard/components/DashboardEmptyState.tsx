import { Button } from '../../../shared';
import type { TabType } from '../hooks/useDashboardPage';
import './DashboardEmptyState.css';

interface DashboardEmptyStateProps {
  activeTab: TabType;
  onCreateListing: () => void;
  onBrowseListings: () => void;
}

export const DashboardEmptyState = ({
  activeTab,
  onCreateListing,
  onBrowseListings,
}: DashboardEmptyStateProps) => {
  const getTitle = () => {
    if (activeTab === 'my-listings') {
      return "You haven't created any listings yet";
    } else if (activeTab === 'liked-listings') {
      return "You haven't liked any listings yet";
    } else {
      return "You haven't archived any listings yet";
    }
  };

  const getMessage = () => {
    if (activeTab === 'my-listings') {
      return 'Create your first listing to get started!';
    } else if (activeTab === 'liked-listings') {
      return "Start browsing listings and like the ones you're interested in.";
    } else {
      return 'Archive listings from your "My Listings" page to see them here.';
    }
  };

  return (
    <div className="dashboard-page__empty">
      <h3>{getTitle()}</h3>
      <p>{getMessage()}</p>
      {activeTab === 'my-listings' && (
        <Button variant="primary" onClick={onCreateListing}>
          Create Your First Listing
        </Button>
      )}
      {activeTab === 'liked-listings' && (
        <Button variant="primary" onClick={onBrowseListings}>
          Browse Listings
        </Button>
      )}
    </div>
  );
};



