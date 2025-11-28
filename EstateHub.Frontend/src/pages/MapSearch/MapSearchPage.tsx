import { useDeferredValue, useMemo, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  type Listing,
  type ListingFilter,
  type ListingsRequest,
  type MapBounds,
  useListingsOnMapQuery,
} from '../../entities/listing';
import {
  ListingFilters,
  type ListingsFiltersState,
  ListingsMap,
} from '../../features';
import { Button, LoadingSpinner } from '../../shared';
import { formatCurrency } from '../../shared/lib/formatCurrency';
import './MapSearchPage.css';

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
  filters: ListingsFiltersState,
  search: string
): Omit<ListingsRequest, 'page' | 'pageSize'> => ({
  filter: sanitizeFilter(filters),
  search: search.length > 0 ? search : undefined,
});

export const MapSearchPage = () => {
  const navigate = useNavigate();
  const [filters, setFilters] = useState<ListingsFiltersState>({});
  const [mapBounds, setMapBounds] = useState<MapBounds | undefined>();
  const [selectedListing, setSelectedListing] = useState<Listing | null>(null);
  const [showFilters, setShowFilters] = useState(false);

  const deferredSearch = useDeferredValue(filters.search ?? '');

  const listingsRequest = useMemo(
    () => buildRequest(filters, deferredSearch.trim()),
    [filters, deferredSearch]
  );

  // Query map listings when bounds are available
  const mapQuery = useListingsOnMapQuery({
    bounds: mapBounds,
    filter: listingsRequest.filter,
    page: 1,
    pageSize: 200,
    enabled: mapBounds !== undefined,
  });

  const listings = mapQuery.data?.items ?? [];
  const loading = mapQuery.loading;
  const error = mapQuery.error;

  const handleFiltersChange = useCallback((next: ListingsFiltersState) => {
    setFilters(next);
    setSelectedListing(null); // Clear selection when filters change
  }, []);

  const handleResetFilters = useCallback(() => {
    setFilters({});
    setSelectedListing(null);
  }, []);

  // Handle navigation for listing clicks
  const handleListingSelect = useCallback((listing: Listing) => {
    setSelectedListing(listing);
  }, []);

  const handleViewListing = useCallback(() => {
    if (selectedListing) {
      navigate(`/listings/${selectedListing.id}`, {
        state: { from: '/map' },
      });
    }
  }, [selectedListing, navigate]);

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

  const getPriceLabel = (listing: Listing) => {
    if (listing.category === 'RENT') {
      return `${formatCurrency(listing.monthlyRentPln)} / month`;
    }
    return formatCurrency(listing.pricePln);
  };

  return (
    <div className="map-search-page">
      {/* Filters Toggle Button */}
      <div className="map-search-page__controls">
        <Button
          variant="primary"
          onClick={() => setShowFilters(!showFilters)}
          className="map-search-page__filters-toggle"
        >
          {showFilters ? '✕ Hide Filters' : '☰ Show Filters'}
        </Button>
        {selectedListing && (
          <Button
            variant="outline"
            onClick={() => setSelectedListing(null)}
            className="map-search-page__close-details"
          >
            ✕ Close Details
          </Button>
        )}
      </div>

      {/* Filters Sidebar */}
      {showFilters && (
        <div className="map-search-page__filters-panel">
          <div className="map-search-page__filters-header">
            <h2>Filters</h2>
            <Button
              variant="ghost"
              onClick={() => setShowFilters(false)}
              className="map-search-page__close-filters"
            >
              ✕
            </Button>
          </div>
          <div className="map-search-page__filters-content">
            <ListingFilters
              filters={filters}
              onFiltersChange={handleFiltersChange}
              onReset={handleResetFilters}
            />
          </div>
        </div>
      )}

      {/* Map Container */}
      <div className="map-search-page__map-container">
        {error && (
          <div className="map-search-page__error">
            <h3>We couldn&apos;t load listings right now</h3>
            <p>Please try again in a moment.</p>
          </div>
        )}

        {loading && (
          <div className="map-search-page__loading">
            <LoadingSpinner />
            <p>Loading listings...</p>
          </div>
        )}

        <ListingsMap
          listings={listings}
          onBoundsChange={handleBoundsChange}
          onSelectListing={handleListingSelect}
        />

        {/* Listing Count Badge */}
        {!loading && listings.length > 0 && (
          <div className="map-search-page__count-badge">
            {listings.length} {listings.length === 1 ? 'listing' : 'listings'} found
          </div>
        )}
      </div>

      {/* Selected Listing Details Sidebar */}
      {selectedListing && (
        <div className="map-search-page__details-panel">
          <div className="map-search-page__details-header">
            <h2>Property Details</h2>
            <Button
              variant="ghost"
              onClick={() => setSelectedListing(null)}
              className="map-search-page__close-details-btn"
            >
              ✕
            </Button>
          </div>
          <div className="map-search-page__details-content">
            {selectedListing.firstPhotoUrl && (
              <div
                className="map-search-page__details-photo"
                style={{
                  backgroundImage: `url(${selectedListing.firstPhotoUrl})`,
                }}
              />
            )}
            <div className="map-search-page__details-body">
              <h3>{selectedListing.title}</h3>
              <p className="map-search-page__details-location">
                {selectedListing.city}
                {selectedListing.district && `, ${selectedListing.district}`}
              </p>
              <p className="map-search-page__details-price">{getPriceLabel(selectedListing)}</p>
              <div className="map-search-page__details-specs">
                <div className="map-search-page__spec">
                  <span className="map-search-page__spec-label">Area</span>
                  <span className="map-search-page__spec-value">
                    {selectedListing.squareMeters} m²
                  </span>
                </div>
                <div className="map-search-page__spec">
                  <span className="map-search-page__spec-label">Rooms</span>
                  <span className="map-search-page__spec-value">{selectedListing.rooms}</span>
                </div>
                {selectedListing.floor !== null &&
                  selectedListing.floor !== undefined && (
                    <div className="map-search-page__spec">
                      <span className="map-search-page__spec-label">Floor</span>
                      <span className="map-search-page__spec-value">
                        {selectedListing.floor}
                        {selectedListing.floorCount
                          ? ` / ${selectedListing.floorCount}`
                          : ''}
                      </span>
                    </div>
                  )}
              </div>
              {selectedListing.description && (
                <p className="map-search-page__details-description">
                  {selectedListing.description.length > 200
                    ? `${selectedListing.description.substring(0, 200)}...`
                    : selectedListing.description}
                </p>
              )}
              <div className="map-search-page__details-actions">
                <Button variant="primary" onClick={handleViewListing}>
                  View Full Details
                </Button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

