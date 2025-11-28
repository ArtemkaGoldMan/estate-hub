import { useDeferredValue, useEffect, useMemo, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  type Listing,
  type ListingFilter,
  type ListingsRequest,
  type ListingsResponse,
  type MapBounds,
  useListingsOnMapQuery,
  useListingsQuery,
} from '../../entities/listing';
import {
  ListingFilters,
  type ListingsFiltersState,
  ListingsMap,
} from '../../features';
import { ListingsGrid } from '../../widgets';
import { Button, Pagination } from '../../shared';
import './ListingsPage.css';

const PAGE_SIZE = 12;

type ViewMode = 'split' | 'list' | 'map';

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
  const [viewMode, setViewMode] = useState<ViewMode>('split');
  const [page, setPage] = useState(1);
  const [filters, setFilters] = useState<ListingsFiltersState>({});
  const [mapBounds, setMapBounds] = useState<MapBounds | undefined>();

  const deferredSearch = useDeferredValue(filters.search ?? '');
  
  // Memoize filters object to prevent unnecessary re-renders
  const filtersString = useMemo(() => JSON.stringify(filters), [filters]);
  
  const listingsRequest = useMemo(
    () => buildRequest(page, PAGE_SIZE, filters, deferredSearch.trim()),
    [page, filters, deferredSearch]
  );

  const { data, loading, error } = useListingsQuery(listingsRequest);

  // Only query map listings when not in list-only mode and bounds are available
  const shouldQueryMap = useMemo(
    () => viewMode !== 'list' && mapBounds !== undefined,
    [viewMode, mapBounds]
  );
  
  const mapQuery = useListingsOnMapQuery({
    bounds: mapBounds,
    filter: listingsRequest.filter,
    page: 1,
    pageSize: 200,
    enabled: shouldQueryMap,
  });

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

  const handleViewModeChange = useCallback((mode: ViewMode) => {
    setViewMode(mode);
  }, []);

  // Memoize bounds change handler to prevent infinite loops
  const handleBoundsChange = useCallback((bounds: MapBounds) => {
    setMapBounds((prevBounds) => {
      // Only update if bounds actually changed
      if (
        !prevBounds ||
        prevBounds.latMin !== bounds.latMin ||
        prevBounds.latMax !== bounds.latMax ||
        prevBounds.lonMin !== bounds.lonMin ||
        prevBounds.lonMax !== bounds.lonMax
      ) {
        return bounds;
      }
      return prevBounds;
    });
  }, []);

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
        <div className="listings-page__view-toggle">
          <span>View</span>
          <div className="listings-page__view-buttons">
            <Button
              type="button"
              variant={viewMode === 'list' ? 'primary' : 'ghost'}
              onClick={() => handleViewModeChange('list')}
            >
              List
            </Button>
            <Button
              type="button"
              variant={viewMode === 'split' ? 'primary' : 'ghost'}
              onClick={() => handleViewModeChange('split')}
            >
              Split
            </Button>
            <Button
              type="button"
              variant={viewMode === 'map' ? 'primary' : 'ghost'}
              onClick={() => handleViewModeChange('map')}
            >
              Map
            </Button>
          </div>
        </div>
      </section>

      <ListingFilters
        filters={filters}
        onFiltersChange={handleFiltersChange}
        onReset={handleResetFilters}
      />

      {error && (
        <div className="listings-page__error">
          <h3>We couldn&apos;t load listings right now</h3>
          <p>Please try again in a moment.</p>
        </div>
      )}

      <div
        className={`listings-page__content listings-page__content--${viewMode}`}
      >
        {viewMode !== 'map' && (
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
        )}

        {viewMode !== 'list' && (
          <div className="listings-page__map">
            <ListingsMap
              listings={mapQuery.data?.items ?? listings.items}
              onBoundsChange={handleBoundsChange}
              onSelectListing={handleListingSelect}
            />
          </div>
        )}
      </div>
    </div>
  );
};


