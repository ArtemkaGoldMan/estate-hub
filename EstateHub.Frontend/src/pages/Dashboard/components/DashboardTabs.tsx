import type { TabType } from '../hooks/useDashboardPage';
import type { ListingsResponse } from '../../../entities/listing';
import './DashboardTabs.css';

interface DashboardTabsProps {
  activeTab: TabType;
  myListingsData: ListingsResponse | undefined;
  likedListingsData: ListingsResponse | undefined;
  archivedListingsData: ListingsResponse | undefined;
  onTabChange: (tab: TabType) => void;
}

export const DashboardTabs = ({
  activeTab,
  myListingsData,
  likedListingsData,
  archivedListingsData,
  onTabChange,
}: DashboardTabsProps) => {
  return (
    <div className="dashboard-page__tabs">
      <button
        className={`dashboard-page__tab ${activeTab === 'my-listings' ? 'dashboard-page__tab--active' : ''}`}
        onClick={() => onTabChange('my-listings')}
      >
        My Listings
        {myListingsData && myListingsData.total > 0 && (
          <span className="dashboard-page__tab-badge">{myListingsData.total}</span>
        )}
      </button>
      <button
        className={`dashboard-page__tab ${activeTab === 'liked-listings' ? 'dashboard-page__tab--active' : ''}`}
        onClick={() => onTabChange('liked-listings')}
      >
        Liked Listings
        {likedListingsData && likedListingsData.total > 0 && (
          <span className="dashboard-page__tab-badge">{likedListingsData.total}</span>
        )}
      </button>
      <button
        className={`dashboard-page__tab ${activeTab === 'archived-listings' ? 'dashboard-page__tab--active' : ''}`}
        onClick={() => onTabChange('archived-listings')}
      >
        Archive
        {archivedListingsData && archivedListingsData.total > 0 && (
          <span className="dashboard-page__tab-badge">{archivedListingsData.total}</span>
        )}
      </button>
    </div>
  );
};

