import { LoadingSpinner } from '../../shared';
import { useDashboardPage } from './hooks/useDashboardPage';
import {
  DashboardPageHeader,
  DashboardTabs,
  DashboardListingsGrid,
  DashboardEmptyState,
} from './components';
import './DashboardPage.css';

export const DashboardPage = () => {
  const {
    user,
    activeTab,
    activeLoading,
    activeError,
    activePage,
    listings,
    myListingsData,
    likedListingsData,
    archivedListingsData,
    PAGE_SIZE,
    setActiveTab,
    setActivePage,
    handleStatusChange,
    handleCreateListing,
    navigate,
  } = useDashboardPage();

  const getErrorTabName = () => {
    if (activeTab === 'my-listings') return 'listings';
    if (activeTab === 'liked-listings') return 'liked listings';
    return 'archived listings';
  };

  const getLoadingTabName = () => {
    if (activeTab === 'my-listings') return 'your listings';
    if (activeTab === 'liked-listings') return 'liked listings';
    return 'archived listings';
  };

  return (
    <div className="dashboard-page">
      <DashboardPageHeader user={user} onCreateListing={handleCreateListing} />

      <DashboardTabs
        activeTab={activeTab}
        myListingsData={myListingsData}
        likedListingsData={likedListingsData}
        archivedListingsData={archivedListingsData}
        onTabChange={setActiveTab}
      />

      {activeError && (
        <div className="dashboard-page__error">
          <h3>We couldn&apos;t load your {getErrorTabName()} right now</h3>
          <p>Please try again in a moment.</p>
        </div>
      )}

      {activeLoading && !listings.items.length && (
        <div className="dashboard-page__loading">
          <LoadingSpinner text={`Loading ${getLoadingTabName()}...`} />
        </div>
      )}

      {!activeLoading && listings.items.length === 0 && (
        <DashboardEmptyState
          activeTab={activeTab}
          onCreateListing={handleCreateListing}
          onBrowseListings={() => navigate('/listings')}
        />
      )}

      {!activeLoading && listings.items.length > 0 && (
        <DashboardListingsGrid
          listings={listings.items}
          activeTab={activeTab}
          activePage={activePage}
          activeLoading={activeLoading}
          totalItems={listings.total}
          pageSize={PAGE_SIZE}
          onStatusChange={handleStatusChange}
          onPageChange={setActivePage}
        />
      )}
    </div>
  );
};


