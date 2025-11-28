import type { Listing } from '../../../entities/listing';
import { ListingCard } from '../../../entities/listing';
import { LoadingSpinner } from '../../../shared';
import './ListingsGrid.css';

interface ListingsGridProps {
  listings: Listing[];
  total?: number;
  loading?: boolean;
  onSelect?: (listing: Listing) => void;
  emptyState?: React.ReactNode;
}

export const ListingsGrid = ({
  listings,
  total,
  loading = false,
  onSelect,
  emptyState,
}: ListingsGridProps) => {
  return (
    <section className="listings-grid">
      <header className="listings-grid__header">
        <h2>Available properties</h2>
        {typeof total === 'number' && (
          <span className="listings-grid__count">{total} results</span>
        )}
      </header>

      {loading && (
        <div className="listings-grid__loading">
          <LoadingSpinner />
          <span>Fetching listings...</span>
        </div>
      )}

      {!loading && listings.length === 0 && (
        <div className="listings-grid__empty">
          {emptyState ?? (
            <>
              <h3>No listings found</h3>
              <p>
                Try adjusting your filters or search terms to discover more properties.
              </p>
            </>
          )}
        </div>
      )}

      {!loading && listings.length > 0 && (
        <div className="listings-grid__content">
          {listings.map((listing) => (
            <ListingCard
              key={listing.id}
              listing={listing}
              onClick={onSelect}
            />
          ))}
        </div>
      )}
    </section>
  );
};


