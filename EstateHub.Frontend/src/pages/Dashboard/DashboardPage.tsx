import { useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../shared/context/AuthContext';
import { useMyListingsQuery, useLikedListingsQuery } from '../../entities/listing';
import { ListingsGrid } from '../../widgets';
import { DashboardListingCard } from '../../widgets/listings/dashboard-listing-card';
import { Button, LoadingSpinner, Pagination } from '../../shared';
import './DashboardPage.css';

const PAGE_SIZE = 12;

type TabType = 'my-listings' | 'liked-listings';

export const DashboardPage = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState<TabType>('my-listings');
  const [myListingsPage, setMyListingsPage] = useState(1);
  const [likedListingsPage, setLikedListingsPage] = useState(1);

  const { data: myListingsData, loading: myListingsLoading, error: myListingsError, refetch: refetchMyListings } = 
    useMyListingsQuery(myListingsPage, PAGE_SIZE);
  
  const { data: likedListingsData, loading: likedListingsLoading, error: likedListingsError } = 
    useLikedListingsQuery(likedListingsPage, PAGE_SIZE);

  // All hooks must be called before any early returns
  const handleListingSelect = useCallback((listing: { id: string }) => {
    navigate(`/listings/${listing.id}`, { state: { from: '/dashboard' } });
  }, [navigate]);

  const handleCreateListing = useCallback(() => {
    navigate('/listings/new');
  }, [navigate]);

  // Note: Route protection is now handled by ProtectedRoute component

  const activeData = activeTab === 'my-listings' ? myListingsData : likedListingsData;
  const activeLoading = activeTab === 'my-listings' ? myListingsLoading : likedListingsLoading;
  const activeError = activeTab === 'my-listings' ? myListingsError : likedListingsError;
  const activePage = activeTab === 'my-listings' ? myListingsPage : likedListingsPage;
  const setActivePage = activeTab === 'my-listings' ? setMyListingsPage : setLikedListingsPage;

  const listings = activeData ?? { items: [], total: 0 };

  return (
    <div className="dashboard-page">
      <div className="dashboard-page__header">
        <div>
          <h1>Dashboard</h1>
          <p>Welcome back, {user?.email || 'User'}!</p>
        </div>
        <Button variant="primary" onClick={handleCreateListing}>
          + Create Listing
        </Button>
      </div>

      <div className="dashboard-page__tabs">
        <button
          className={`dashboard-page__tab ${activeTab === 'my-listings' ? 'dashboard-page__tab--active' : ''}`}
          onClick={() => setActiveTab('my-listings')}
        >
          My Listings
          {myListingsData && myListingsData.total > 0 && (
            <span className="dashboard-page__tab-badge">{myListingsData.total}</span>
          )}
        </button>
        <button
          className={`dashboard-page__tab ${activeTab === 'liked-listings' ? 'dashboard-page__tab--active' : ''}`}
          onClick={() => setActiveTab('liked-listings')}
        >
          Liked Listings
          {likedListingsData && likedListingsData.total > 0 && (
            <span className="dashboard-page__tab-badge">{likedListingsData.total}</span>
          )}
        </button>
      </div>

      {activeError && (
        <div className="dashboard-page__error">
          <h3>We couldn&apos;t load your {activeTab === 'my-listings' ? 'listings' : 'liked listings'} right now</h3>
          <p>Please try again in a moment.</p>
        </div>
      )}

      {activeLoading && !activeData && (
        <div className="dashboard-page__loading">
          <LoadingSpinner text={`Loading ${activeTab === 'my-listings' ? 'your listings' : 'liked listings'}...`} />
        </div>
      )}

      {!activeLoading && listings.items.length === 0 && (
        <div className="dashboard-page__empty">
          <h3>
            {activeTab === 'my-listings' 
              ? "You haven't created any listings yet" 
              : "You haven't liked any listings yet"}
          </h3>
          <p>
            {activeTab === 'my-listings'
              ? 'Create your first listing to get started!'
              : 'Start browsing listings and like the ones you\'re interested in.'}
          </p>
          {activeTab === 'my-listings' && (
            <Button variant="primary" onClick={handleCreateListing}>
              Create Your First Listing
            </Button>
          )}
          {activeTab === 'liked-listings' && (
            <Button variant="primary" onClick={() => navigate('/listings')}>
              Browse Listings
            </Button>
          )}
        </div>
      )}

      {!activeLoading && listings.items.length > 0 && (
        <>
          {activeTab === 'my-listings' ? (
            <div className="dashboard-page__listings-grid">
              <header className="dashboard-page__listings-header">
                <h2>My Listings</h2>
                <span className="dashboard-page__listings-count">{listings.total} listings</span>
              </header>
              <div className="dashboard-page__listings-content">
                {listings.items.map((listing) => (
                  <DashboardListingCard
                    key={listing.id}
                    listing={listing}
                    onStatusChange={refetchMyListings}
                  />
                ))}
              </div>
            </div>
          ) : (
            <ListingsGrid
              listings={listings.items}
              total={listings.total}
              loading={activeLoading}
              onSelect={handleListingSelect}
            />
          )}
          <div className="dashboard-page__pagination">
            <Pagination
              currentPage={activePage}
              totalItems={listings.total}
              pageSize={PAGE_SIZE}
              onPageChange={setActivePage}
              disabled={activeLoading}
            />
          </div>
        </>
      )}
    </div>
  );
};


