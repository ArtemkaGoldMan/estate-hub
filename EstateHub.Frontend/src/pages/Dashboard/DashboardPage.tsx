import { useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../shared/context/AuthContext';
import { useMyListingsQuery, useLikedListingsQuery, useArchivedListingsQuery } from '../../entities/listing';
import { DashboardListingCard } from '../../widgets/listings/dashboard-listing-card';
import { LikedListingCard } from '../../widgets/listings/liked-listing-card';
import { Button, LoadingSpinner, Pagination } from '../../shared';
import './DashboardPage.css';

const PAGE_SIZE = 12;

type TabType = 'my-listings' | 'liked-listings' | 'archived-listings';

export const DashboardPage = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState<TabType>('my-listings');
  const [myListingsPage, setMyListingsPage] = useState(1);
  const [likedListingsPage, setLikedListingsPage] = useState(1);
  const [archivedListingsPage, setArchivedListingsPage] = useState(1);

  const { data: myListingsData, loading: myListingsLoading, error: myListingsError, refetch: refetchMyListings } = 
    useMyListingsQuery(myListingsPage, PAGE_SIZE, {
      enablePolling: true, // Enable polling to check for moderation status updates
      pollInterval: 10000, // Poll every 10 seconds
    });
  
  const { data: likedListingsData, loading: likedListingsLoading, error: likedListingsError, refetch: refetchLikedListings } = 
    useLikedListingsQuery(likedListingsPage, PAGE_SIZE);
  
  const { data: archivedListingsData, loading: archivedListingsLoading, error: archivedListingsError, refetch: refetchArchivedListings } = 
    useArchivedListingsQuery(archivedListingsPage, PAGE_SIZE);

  // All hooks must be called before any early returns
  const handleStatusChange = useCallback(() => {
    // Refetch the active tab's query when status changes
    if (activeTab === 'my-listings') {
      refetchMyListings();
    } else if (activeTab === 'liked-listings') {
      refetchLikedListings();
    } else if (activeTab === 'archived-listings') {
      refetchArchivedListings();
    }
  }, [activeTab, refetchMyListings, refetchLikedListings, refetchArchivedListings]);

  const handleCreateListing = useCallback(() => {
    navigate('/listings/new');
  }, [navigate]);

  // Note: Route protection is now handled by ProtectedRoute component

  const activeData = activeTab === 'my-listings' 
    ? myListingsData 
    : activeTab === 'liked-listings' 
    ? likedListingsData 
    : archivedListingsData;
  const activeLoading = activeTab === 'my-listings' 
    ? myListingsLoading 
    : activeTab === 'liked-listings' 
    ? likedListingsLoading 
    : archivedListingsLoading;
  const activeError = activeTab === 'my-listings' 
    ? myListingsError 
    : activeTab === 'liked-listings' 
    ? likedListingsError 
    : archivedListingsError;
  const activePage = activeTab === 'my-listings' 
    ? myListingsPage 
    : activeTab === 'liked-listings' 
    ? likedListingsPage 
    : archivedListingsPage;
  const setActivePage = activeTab === 'my-listings' 
    ? setMyListingsPage 
    : activeTab === 'liked-listings' 
    ? setLikedListingsPage 
    : setArchivedListingsPage;

  const listings = activeData ?? { items: [], total: 0 };

  return (
    <div className="dashboard-page">
      <div className="dashboard-page__header">
        <div>
          <h1>Dashboard</h1>
          <p>Welcome back, {user?.displayName || user?.email || 'User'}!</p>
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
        <button
          className={`dashboard-page__tab ${activeTab === 'archived-listings' ? 'dashboard-page__tab--active' : ''}`}
          onClick={() => setActiveTab('archived-listings')}
        >
          Archive
          {archivedListingsData && archivedListingsData.total > 0 && (
            <span className="dashboard-page__tab-badge">{archivedListingsData.total}</span>
          )}
        </button>
      </div>

      {activeError && (
        <div className="dashboard-page__error">
          <h3>We couldn&apos;t load your {activeTab === 'my-listings' ? 'listings' : activeTab === 'liked-listings' ? 'liked listings' : 'archived listings'} right now</h3>
          <p>Please try again in a moment.</p>
        </div>
      )}

      {activeLoading && !activeData && (
        <div className="dashboard-page__loading">
          <LoadingSpinner text={`Loading ${activeTab === 'my-listings' ? 'your listings' : activeTab === 'liked-listings' ? 'liked listings' : 'archived listings'}...`} />
        </div>
      )}

      {!activeLoading && listings.items.length === 0 && (
        <div className="dashboard-page__empty">
          <h3>
            {activeTab === 'my-listings' 
              ? "You haven't created any listings yet" 
              : activeTab === 'liked-listings'
              ? "You haven't liked any listings yet"
              : "You haven't archived any listings yet"}
          </h3>
          <p>
            {activeTab === 'my-listings'
              ? 'Create your first listing to get started!'
              : activeTab === 'liked-listings'
              ? 'Start browsing listings and like the ones you\'re interested in.'
              : 'Archive listings from your "My Listings" page to see them here.'}
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
          <div className="dashboard-page__listings-grid">
            {listings.items.map((listing) => {
              if (activeTab === 'liked-listings') {
                return (
                  <LikedListingCard
                    key={listing.id}
                    listing={listing}
                    onStatusChange={handleStatusChange}
                  />
                );
              } else if (activeTab === 'archived-listings') {
                return (
                  <DashboardListingCard
                    key={listing.id}
                    listing={listing}
                    onStatusChange={handleStatusChange}
                    mode="archive-only"
                  />
                );
              } else {
                // My Listings - full mode
                return (
                  <DashboardListingCard
                    key={listing.id}
                    listing={listing}
                    onStatusChange={handleStatusChange}
                    mode="full"
                  />
                );
              }
            })}
          </div>
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


