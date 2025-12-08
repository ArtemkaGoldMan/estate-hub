import { Pagination } from '../../../shared';
import { DashboardListingCard } from '../../../widgets/listings/dashboard-listing-card';
import { LikedListingCard } from '../../../widgets/listings/liked-listing-card';
import type { TabType } from '../hooks/useDashboardPage';
import type { Listing } from '../../../entities/listing';
import './DashboardListingsGrid.css';

interface DashboardListingsGridProps {
  listings: Listing[];
  activeTab: TabType;
  activePage: number;
  activeLoading: boolean;
  totalItems: number;
  pageSize: number;
  onStatusChange: () => void;
  onPageChange: (page: number) => void;
}

export const DashboardListingsGrid = ({
  listings,
  activeTab,
  activePage,
  activeLoading,
  totalItems,
  pageSize,
  onStatusChange,
  onPageChange,
}: DashboardListingsGridProps) => {
  return (
    <>
      <div className="dashboard-page__listings-grid">
        {listings.map((listing) => {
          if (activeTab === 'liked-listings') {
            return (
              <LikedListingCard
                key={listing.id}
                listing={listing}
                onStatusChange={onStatusChange}
              />
            );
          } else if (activeTab === 'archived-listings') {
            return (
              <DashboardListingCard
                key={listing.id}
                listing={listing}
                onStatusChange={onStatusChange}
                mode="archive-only"
              />
            );
          } else {
            return (
              <DashboardListingCard
                key={listing.id}
                listing={listing}
                onStatusChange={onStatusChange}
                mode="full"
              />
            );
          }
        })}
      </div>
      <div className="dashboard-page__pagination">
        <Pagination
          currentPage={activePage}
          totalItems={totalItems}
          pageSize={pageSize}
          onPageChange={onPageChange}
          disabled={activeLoading}
        />
      </div>
    </>
  );
};

