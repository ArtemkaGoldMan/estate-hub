import { useState, useEffect, useCallback } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '../../shared/context/AuthContext';
import { useToast } from '../../shared/context/ToastContext';
import { UserFriendlyError } from '../../shared/lib/errorParser';
import {
  useListingQuery,
  useUpdateListing,
  useDeleteListing,
  type UpdateListingInput,
  type ListingCondition,
} from '../../entities/listing';
import { Button, Input, LoadingSpinner, Modal, RichTextEditor } from '../../shared';
import { PhotoManager } from '../../features/listings/photos/ui/PhotoManager';
import { MapContainer, TileLayer, Marker, useMapEvents } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import { Icon } from 'leaflet';
import './EditListingPage.css';

// Fix for default marker icon
// eslint-disable-next-line @typescript-eslint/no-explicit-any
delete (Icon.Default.prototype as any)._getIconUrl;
Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png',
  iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
});

function LocationPicker({
  position,
  onPositionChange,
}: {
  position: [number, number];
  onPositionChange: (lat: number, lng: number) => void;
}) {
  useMapEvents({
    click(e) {
      onPositionChange(e.latlng.lat, e.latlng.lng);
    },
  });

  return position ? <Marker position={position} /> : null;
}

export const EditListingPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { isAuthenticated, user } = useAuth();
  const { showError } = useToast();
  const { listing, loading: listingLoading, error: listingError } = useListingQuery(id || '');
  const { updateListing, loading: updating, error: updateError } = useUpdateListing();
  const { deleteListing, loading: deleting, error: deleteError } = useDeleteListing();

  const [formData, setFormData] = useState<Partial<UpdateListingInput>>({});
  const [mapPosition, setMapPosition] = useState<[number, number] | null>(null);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [showDeleteModal, setShowDeleteModal] = useState(false);

  // Redirect if not authenticated
  useEffect(() => {
    if (!isAuthenticated || !user) {
      navigate('/login', { replace: true });
    }
  }, [isAuthenticated, user, navigate]);

  // Redirect if no ID
  useEffect(() => {
    if (!id) {
      navigate('/dashboard', { replace: true });
    }
  }, [id, navigate]);

  // Load listing data into form
  useEffect(() => {
    if (listing && user) {
      // Check if user owns this listing
      if (listing.ownerId !== user.id) {
        navigate('/dashboard', { replace: true });
        return;
      }

      setFormData({
        title: listing.title,
        description: listing.description,
        city: listing.city,
        district: listing.district,
        squareMeters: listing.squareMeters,
        rooms: listing.rooms,
        floor: listing.floor ?? null,
        floorCount: listing.floorCount ?? null,
        buildYear: listing.buildYear ?? null,
        condition: listing.condition,
        hasBalcony: listing.hasBalcony,
        hasElevator: listing.hasElevator,
        hasParkingSpace: listing.hasParkingSpace,
        hasSecurity: listing.hasSecurity,
        hasStorageRoom: listing.hasStorageRoom,
        pricePln: listing.pricePln ?? null,
        monthlyRentPln: listing.monthlyRentPln ?? null,
      });

      setMapPosition([listing.latitude, listing.longitude]);
    }
  }, [listing, user, navigate]);

  // All hooks must be called before any early returns
  const handleSubmit = useCallback(
    async (e: React.FormEvent) => {
      e.preventDefault();

      if (!id || !mapPosition || !listing) {
        return;
      }

      // Validate form inline to avoid dependency issues
      const newErrors: Record<string, string> = {};

      if (!formData.title?.trim()) newErrors.title = 'Title is required';
      if (!formData.description?.trim()) newErrors.description = 'Description is required';
      if (!formData.district?.trim()) newErrors.district = 'District is required';
      if (!formData.city?.trim()) newErrors.city = 'City is required';
      if (!formData.squareMeters || formData.squareMeters <= 0) {
        newErrors.squareMeters = 'Square meters must be greater than 0';
      }
      if (!formData.rooms || formData.rooms <= 0) {
        newErrors.rooms = 'Number of rooms must be greater than 0';
      }
      if (listing.category === 'SALE' && formData.pricePln !== null && formData.pricePln !== undefined && formData.pricePln <= 0) {
        newErrors.pricePln = 'Price must be greater than 0';
      }
      if (listing.category === 'RENT' && formData.monthlyRentPln !== null && formData.monthlyRentPln !== undefined && formData.monthlyRentPln <= 0) {
        newErrors.monthlyRentPln = 'Monthly rent must be greater than 0';
      }
      if (!mapPosition) {
        newErrors.location = 'Please select a location on the map';
      }

      setErrors(newErrors);
      if (Object.keys(newErrors).length > 0) {
        return;
      }

      try {
        const input: UpdateListingInput = {
          title: formData.title,
          description: formData.description,
          district: formData.district,
          city: formData.city,
          latitude: mapPosition[0],
          longitude: mapPosition[1],
          squareMeters: formData.squareMeters,
          rooms: formData.rooms,
          floor: formData.floor ?? null,
          floorCount: formData.floorCount ?? null,
          buildYear: formData.buildYear ?? null,
          condition: formData.condition,
          hasBalcony: formData.hasBalcony,
          hasElevator: formData.hasElevator,
          hasParkingSpace: formData.hasParkingSpace,
          hasSecurity: formData.hasSecurity,
          hasStorageRoom: formData.hasStorageRoom,
          pricePln: listing.category === 'SALE' ? formData.pricePln ?? null : null,
          monthlyRentPln: listing.category === 'RENT' ? formData.monthlyRentPln ?? null : null,
        };

        await updateListing(id, input);
        navigate(`/dashboard/listings/${id}`);
      } catch (err) {
        // Error is handled by the mutation hook and displayed in the form
      }
    },
    [formData, mapPosition, updateListing, navigate, id, listing, setErrors]
  );

  const handleDelete = useCallback(async () => {
    if (!id) return;

    try {
      await deleteListing(id);
      navigate('/dashboard', { replace: true });
    } catch (err) {
      if (err instanceof UserFriendlyError) {
        showError(err.userMessage);
      } else {
        showError(err instanceof Error ? err.message : 'Failed to delete listing');
      }
    }
  }, [id, deleteListing, navigate]);

  const handleInputChange = useCallback(
    (field: keyof UpdateListingInput, value: string | number | boolean | null) => {
      setFormData((prev) => ({ ...prev, [field]: value }));
      // Clear error for this field using functional update to avoid dependency
      setErrors((prev) => {
        if (prev[field]) {
          const newErrors = { ...prev };
          delete newErrors[field];
          return newErrors;
        }
        return prev;
      });
    },
    [] // No dependencies needed - using functional updates
  );

  const handleMapClick = useCallback((lat: number, lng: number) => {
    setMapPosition([lat, lng]);
    // Clear location error using functional update to avoid dependency
    setErrors((prev) => {
      if (prev.location) {
        const newErrors = { ...prev };
        delete newErrors.location;
        return newErrors;
      }
      return prev;
    });
  }, []); // No dependencies needed - using functional updates

  // Note: Route protection is now handled by ProtectedRoute component
  if (!id || !user) {
    return null;
  }

  if (listingLoading) {
    return (
      <div className="edit-listing-page">
        <div className="edit-listing-page__loading">
          <LoadingSpinner text="Loading listing..." />
        </div>
      </div>
    );
  }

  if (listingError || !listing) {
    return (
      <div className="edit-listing-page">
        <div className="edit-listing-page__error">
          <h2>Listing not found</h2>
          <p>Sorry, we couldn't find the listing you're looking for.</p>
          <Button onClick={() => navigate('/dashboard')}>Back to Dashboard</Button>
        </div>
      </div>
    );
  }

  const error = updateError || deleteError;

  return (
    <div className="edit-listing-page">
      <div className="edit-listing-page__header">
        <h1>Edit Listing</h1>
        <Button variant="ghost" onClick={() => navigate(`/dashboard/listings/${id}`)}>
          ← Back to Dashboard Listing
        </Button>
      </div>

      {error && (
        <div className="edit-listing-page__error-banner">
          <p>{error.message || 'An error occurred. Please try again.'}</p>
        </div>
      )}

      <form onSubmit={handleSubmit} className="edit-listing-page__form">
        <div className="edit-listing-page__section">
          <h2>Basic Information</h2>

          <div className="edit-listing-page__field">
            <label>Category</label>
            <Input
              type="text"
              value={listing.category === 'SALE' ? 'For Sale' : 'For Rent'}
              disabled
              readOnly
            />
          </div>

          <div className="edit-listing-page__field">
            <label>Property Type</label>
            <Input
              type="text"
              value={listing.propertyType}
              disabled
              readOnly
            />
          </div>

          <div className="edit-listing-page__field">
            <label htmlFor="title">Title *</label>
            <Input
              id="title"
              type="text"
              value={formData.title || ''}
              onChange={(e) => handleInputChange('title', e.target.value)}
              maxLength={200}
              className={errors.title ? 'error' : ''}
            />
            {errors.title && <span className="error-message">{errors.title}</span>}
          </div>

          <div className="edit-listing-page__field">
            <label htmlFor="description">Description *</label>
            <RichTextEditor
              id="description"
              value={formData.description || ''}
              onChange={(value) => handleInputChange('description', value)}
              placeholder="Describe your property... You can use formatting like bold, italic, paragraphs, and lists."
              rows={6}
              error={errors.description}
            />
          </div>

          <div className="edit-listing-page__field">
            <label htmlFor="condition">Condition *</label>
            <select
              id="condition"
              value={formData.condition || 'GOOD'}
              onChange={(e) => handleInputChange('condition', e.target.value as ListingCondition)}
            >
              <option value="NEW">New</option>
              <option value="GOOD">Good</option>
              <option value="NEEDS_RENOVATION">Needs Renovation</option>
            </select>
          </div>
        </div>

        <div className="edit-listing-page__section">
          <h2>Location</h2>

          <div className="edit-listing-page__field-row">
            <div className="edit-listing-page__field">
              <label htmlFor="district">District *</label>
              <Input
                id="district"
                type="text"
                value={formData.district || ''}
                onChange={(e) => handleInputChange('district', e.target.value)}
                className={errors.district ? 'error' : ''}
              />
              {errors.district && <span className="error-message">{errors.district}</span>}
            </div>

            <div className="edit-listing-page__field">
              <label htmlFor="city">City *</label>
              <Input
                id="city"
                type="text"
                value={formData.city || ''}
                onChange={(e) => handleInputChange('city', e.target.value)}
                className={errors.city ? 'error' : ''}
              />
              {errors.city && <span className="error-message">{errors.city}</span>}
            </div>
          </div>

          <div className="edit-listing-page__field">
            <label>Location on Map *</label>
            <p className="edit-listing-page__hint">Click on the map to update the location</p>
            {errors.location && <span className="error-message">{errors.location}</span>}
            {mapPosition && (
              <div className="edit-listing-page__map">
                <MapContainer
                  center={mapPosition}
                  zoom={13}
                  style={{ height: '400px', width: '100%' }}
                  scrollWheelZoom={true}
                >
                  <TileLayer
                    attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                    url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                  />
                  <LocationPicker
                    position={mapPosition}
                    onPositionChange={handleMapClick}
                  />
                </MapContainer>
              </div>
            )}
          </div>
        </div>

        <div className="edit-listing-page__section">
          <h2>Property Details</h2>

          <div className="edit-listing-page__field-row">
            <div className="edit-listing-page__field">
              <label htmlFor="squareMeters">Square Meters *</label>
              <Input
                id="squareMeters"
                type="number"
                min="1"
                value={formData.squareMeters || ''}
                onChange={(e) => handleInputChange('squareMeters', parseFloat(e.target.value) || 0)}
                className={errors.squareMeters ? 'error' : ''}
              />
              {errors.squareMeters && <span className="error-message">{errors.squareMeters}</span>}
            </div>

            <div className="edit-listing-page__field">
              <label htmlFor="rooms">Rooms *</label>
              <Input
                id="rooms"
                type="number"
                min="1"
                value={formData.rooms || ''}
                onChange={(e) => handleInputChange('rooms', parseInt(e.target.value) || 0)}
                className={errors.rooms ? 'error' : ''}
              />
              {errors.rooms && <span className="error-message">{errors.rooms}</span>}
            </div>
          </div>

          <div className="edit-listing-page__field-row">
            <div className="edit-listing-page__field">
              <label htmlFor="floor">Floor</label>
              <Input
                id="floor"
                type="number"
                value={formData.floor || ''}
                onChange={(e) => handleInputChange('floor', e.target.value ? parseInt(e.target.value) : null)}
              />
            </div>

            <div className="edit-listing-page__field">
              <label htmlFor="floorCount">Total Floors</label>
              <Input
                id="floorCount"
                type="number"
                value={formData.floorCount || ''}
                onChange={(e) => handleInputChange('floorCount', e.target.value ? parseInt(e.target.value) : null)}
              />
            </div>

            <div className="edit-listing-page__field">
              <label htmlFor="buildYear">Build Year</label>
              <Input
                id="buildYear"
                type="number"
                min="1800"
                max={new Date().getFullYear()}
                value={formData.buildYear || ''}
                onChange={(e) => handleInputChange('buildYear', e.target.value ? parseInt(e.target.value) : null)}
              />
            </div>
          </div>
        </div>

        <div className="edit-listing-page__section">
          <h2>Pricing</h2>

          {listing.category === 'SALE' ? (
            <div className="edit-listing-page__field">
              <label htmlFor="pricePln">Price (PLN) *</label>
              <Input
                id="pricePln"
                type="number"
                min="0"
                step="0.01"
                value={formData.pricePln || ''}
                onChange={(e) => handleInputChange('pricePln', e.target.value ? parseFloat(e.target.value) : null)}
                className={errors.pricePln ? 'error' : ''}
              />
              {errors.pricePln && <span className="error-message">{errors.pricePln}</span>}
            </div>
          ) : (
            <div className="edit-listing-page__field">
              <label htmlFor="monthlyRentPln">Monthly Rent (PLN) *</label>
              <Input
                id="monthlyRentPln"
                type="number"
                min="0"
                step="0.01"
                value={formData.monthlyRentPln || ''}
                onChange={(e) => handleInputChange('monthlyRentPln', e.target.value ? parseFloat(e.target.value) : null)}
                className={errors.monthlyRentPln ? 'error' : ''}
              />
              {errors.monthlyRentPln && <span className="error-message">{errors.monthlyRentPln}</span>}
            </div>
          )}
        </div>

        <div className="edit-listing-page__section">
          <h2>Features</h2>

          <div className="edit-listing-page__features">
            {[
              { key: 'hasBalcony', label: 'Balcony' },
              { key: 'hasElevator', label: 'Elevator' },
              { key: 'hasParkingSpace', label: 'Parking Space' },
              { key: 'hasSecurity', label: 'Security' },
              { key: 'hasStorageRoom', label: 'Storage Room' },
            ].map((feature) => (
              <label key={feature.key} className="edit-listing-page__checkbox">
                <input
                  type="checkbox"
                  checked={formData[feature.key as keyof UpdateListingInput] as boolean || false}
                  onChange={(e) => handleInputChange(feature.key as keyof UpdateListingInput, e.target.checked)}
                />
                <span>{feature.label}</span>
              </label>
            ))}
          </div>
        </div>

        <div className="edit-listing-page__section" onClick={(e) => e.stopPropagation()}>
          <h2>Photos</h2>
          <PhotoManager listingId={id} />
        </div>

        <div className="edit-listing-page__actions">
          <Button
            type="submit"
            variant="primary"
            disabled={updating}
            isLoading={updating}
          >
            {updating ? 'Saving...' : 'Save Changes'}
          </Button>
          <Button
            type="button"
            variant="outline"
            onClick={() => navigate(`/dashboard/listings/${id}`)}
            disabled={updating}
          >
            Cancel
          </Button>
          <Button
            type="button"
            variant="danger"
            onClick={() => setShowDeleteModal(true)}
            disabled={updating || deleting}
            className="edit-listing-page__delete-button"
          >
            Delete Listing
          </Button>
        </div>
      </form>

      <Modal
        isOpen={showDeleteModal}
        onClose={() => setShowDeleteModal(false)}
        title="Delete Listing"
      >
        <div className="edit-listing-page__delete-modal">
          <p>Are you sure you want to delete this listing? This action cannot be undone.</p>
          <div className="edit-listing-page__delete-modal-actions">
            <Button
              variant="outline"
              onClick={() => setShowDeleteModal(false)}
              disabled={deleting}
            >
              Cancel
            </Button>
            <Button
              variant="danger"
              onClick={handleDelete}
              disabled={deleting}
              isLoading={deleting}
            >
              {deleting ? 'Deleting...' : 'Delete Listing'}
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
};

