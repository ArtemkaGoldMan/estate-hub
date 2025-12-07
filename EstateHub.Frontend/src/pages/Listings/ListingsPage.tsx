import { useDeferredValue, useEffect, useMemo, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  type Listing,
  type ListingFilter,
  type ListingsRequest,
  type ListingsResponse,
  useListingsQuery,
} from '../../entities/listing';
import {
  ListingFilters,
  type ListingsFiltersState,
} from '../../features';
import { FaFilter, FaChevronDown, FaChevronUp } from 'react-icons/fa';
import { ListingsGrid } from '../../widgets';
import { Button, Pagination } from '../../shared';
import './ListingsPage.css';

const PAGE_SIZE = 12;

const sanitizeFilter = (filters: ListingsFiltersState): ListingFilter => {
  const entries = Object.entries(filters).filter(([key, value]) => {
    if (key === 'search') {
      return false;
    }

    if (typeof value === 'string') {
      return value.trim().length > 0;
    }

    if (typeof value === 'number') {
      return !Number.isNaN(value);
    }

    if (typeof value === 'boolean') {
      return value;
    }

    return value !== null && value !== undefined;
  });

  return Object.fromEntries(entries) as ListingFilter;
};

const buildRequest = (
  page: number,
  pageSize: number,
  filters: ListingsFiltersState,
  search: string
): ListingsRequest => ({
  page,
  pageSize,
  filter: sanitizeFilter(filters),
  search: search.length > 0 ? search : undefined,
});

export const ListingsPage = () => {
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [filters, setFilters] = useState<ListingsFiltersState>({});
  const [showFilters, setShowFilters] = useState(false);

  const deferredSearch = useDeferredValue(filters.search ?? '');
  
  // Memoize filters object to prevent unnecessary re-renders
  const filtersString = useMemo(() => JSON.stringify(filters), [filters]);
  
  const listingsRequest = useMemo(
    () => buildRequest(page, PAGE_SIZE, filters, deferredSearch.trim()),
    [page, filters, deferredSearch]
  );

  const { data, loading, error } = useListingsQuery(listingsRequest);

  // Reset page when filters or search actually change (not on every render)
  useEffect(() => {
    setPage(1);
  }, [filtersString, deferredSearch]);

  const handleFiltersChange = useCallback((next: ListingsFiltersState) => {
    setFilters(next);
  }, []);

  const handleResetFilters = useCallback(() => {
    setFilters({});
  }, []);

  // Handle navigation for map clicks and card clicks
  const handleListingSelect = useCallback((listing: Listing) => {
    navigate(`/listings/${listing.id}`);
  }, [navigate]);


  const listings: ListingsResponse = data ?? { items: [], total: 0 };

  return (
    <div className="listings-page">
      <section className="listings-page__hero">
        <div>
          <h1>Find your next home</h1>
          <p>
            Browse curated properties across Poland. Filter by price, area, and amenities
            or explore the interactive map to discover opportunities.
          </p>
        </div>
      </section>

      <div className="listings-page__filters-section">
        <Button
          variant="outline"
          onClick={() => setShowFilters(!showFilters)}
          className="listings-page__filters-toggle"
        >
          <FaFilter style={{ marginRight: '0.5rem' }} />
          {showFilters ? 'Hide Filters' : 'Show Filters'}
          {showFilters ? <FaChevronUp style={{ marginLeft: '0.5rem' }} /> : <FaChevronDown style={{ marginLeft: '0.5rem' }} />}
        </Button>
        
        {showFilters && (
          <div className="listings-page__filters-content">
            <ListingFilters
              filters={filters}
              onFiltersChange={handleFiltersChange}
              onReset={handleResetFilters}
            />
          </div>
        )}
      </div>

      {error && (
        <div className="listings-page__error">
          <h3>We couldn&apos;t load listings right now</h3>
          <p>Please try again in a moment.</p>
        </div>
      )}

      <div className="listings-page__content">
        <div className="listings-page__list">
          <ListingsGrid
            listings={listings.items}
            total={listings.total}
            loading={loading}
            onSelect={handleListingSelect}
          />

          <div className="listings-page__pagination">
            <Pagination
              currentPage={page}
              totalItems={listings.total}
              pageSize={PAGE_SIZE}
              onPageChange={setPage}
              disabled={loading}
            />
          </div>
        </div>
      </div>
    </div>
  );
};


