import { useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  useCreateListing,
  type CreateListingInput,
  type ListingCategory,
  type PropertyType,
  type ListingCondition,
} from '../../entities/listing';
import { Button, Input, RichTextEditor } from '../../shared';
import { PhotoManager } from '../../features/listings/photos/ui/PhotoManager';
import { MapContainer, TileLayer, Marker, useMapEvents } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import { Icon } from 'leaflet';
import './CreateListingPage.css';

// Fix for default marker icon
// eslint-disable-next-line @typescript-eslint/no-explicit-any
delete (Icon.Default.prototype as any)._getIconUrl;
Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png',
  iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
});

// Default center (Warsaw, Poland)
const DEFAULT_CENTER: [number, number] = [52.2297, 21.0122];

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

export const CreateListingPage = () => {
  const navigate = useNavigate();
  const { createListing, loading, error } = useCreateListing();

  const [formData, setFormData] = useState<Partial<CreateListingInput>>({
    category: 'SALE',
    propertyType: 'APARTMENT',
    condition: 'GOOD',
    hasBalcony: false,
    hasElevator: false,
    hasParkingSpace: false,
    hasSecurity: false,
    hasStorageRoom: false,
  });

  const [mapPosition, setMapPosition] = useState<[number, number]>(DEFAULT_CENTER);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [createdListingId, setCreatedListingId] = useState<string | null>(null);

  // All hooks must be called before any early returns
  const handleSubmit = useCallback(
    async (e: React.FormEvent) => {
      e.preventDefault();

      // Validate form inline to avoid dependency issues
      const newErrors: Record<string, string> = {};

      if (!formData.title?.trim()) newErrors.title = 'Title is required';
      if (!formData.description?.trim()) newErrors.description = 'Description is required';
      if (!formData.addressLine?.trim()) newErrors.addressLine = 'Address is required';
      if (!formData.district?.trim()) newErrors.district = 'District is required';
      if (!formData.city?.trim()) newErrors.city = 'City is required';
      if (!formData.postalCode?.trim()) newErrors.postalCode = 'Postal code is required';
      if (!formData.squareMeters || formData.squareMeters <= 0) {
        newErrors.squareMeters = 'Square meters must be greater than 0';
      }
      if (!formData.rooms || formData.rooms <= 0) {
        newErrors.rooms = 'Number of rooms must be greater than 0';
      }
      if (formData.category === 'SALE' && (!formData.pricePln || formData.pricePln <= 0)) {
        newErrors.pricePln = 'Price is required for sale listings';
      }
      if (formData.category === 'RENT' && (!formData.monthlyRentPln || formData.monthlyRentPln <= 0)) {
        newErrors.monthlyRentPln = 'Monthly rent is required for rental listings';
      }
      if (!mapPosition || mapPosition.length !== 2) {
        newErrors.location = 'Please select a location on the map';
      }

      setErrors(newErrors);
      if (Object.keys(newErrors).length > 0) {
        return;
      }

      try {
        // Ensure category is valid before sending
        if (!formData.category || (formData.category !== 'SALE' && formData.category !== 'RENT')) {
          setErrors({ submit: 'Invalid category selected' });
          return;
        }

        const input: CreateListingInput = {
          category: formData.category as ListingCategory,
          propertyType: formData.propertyType!,
          title: formData.title!,
          description: formData.description!,
          addressLine: formData.addressLine!,
          district: formData.district!,
          city: formData.city!,
          postalCode: formData.postalCode!,
          latitude: mapPosition[0],
          longitude: mapPosition[1],
          squareMeters: formData.squareMeters!,
          rooms: formData.rooms!,
          floor: formData.floor ?? null,
          floorCount: formData.floorCount ?? null,
          buildYear: formData.buildYear ?? null,
          condition: formData.condition!,
          hasBalcony: formData.hasBalcony ?? false,
          hasElevator: formData.hasElevator ?? false,
          hasParkingSpace: formData.hasParkingSpace ?? false,
          hasSecurity: formData.hasSecurity ?? false,
          hasStorageRoom: formData.hasStorageRoom ?? false,
          pricePln: formData.category === 'SALE' ? formData.pricePln ?? null : null,
          monthlyRentPln: formData.category === 'RENT' ? formData.monthlyRentPln ?? null : null,
        };

        const listingId = await createListing(input);
        // Store the listing ID to show photo upload section
        setCreatedListingId(listingId);
        // Don't redirect yet - allow user to upload photos first
      } catch (err) {
        // Error is handled by the mutation hook
        // Set a user-friendly error message
        if (err instanceof Error) {
          setErrors({ submit: err.message });
        } else {
          setErrors({ submit: 'Failed to create listing. Please try again.' });
        }
      }
    },
    [formData, mapPosition, createListing, navigate, setErrors]
  );

  const handleInputChange = useCallback(
    (field: keyof CreateListingInput, value: string | number | boolean | null) => {
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
    setFormData((prev) => ({
      ...prev,
      latitude: lat,
      longitude: lng,
    }));
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

  // Redirect if not authenticated (use useEffect to avoid conditional hook calls)
  // Note: Route protection is now handled by ProtectedRoute component

  return (
    <div className="create-listing-page">
      <div className="create-listing-page__header">
        <h1>Create New Listing</h1>
        <Button variant="ghost" onClick={() => navigate('/dashboard')}>
          ← Back to Dashboard
        </Button>
      </div>

      {(error || errors.submit) && (
        <div className="create-listing-page__error">
          <p>
            {error?.message?.includes('expired') || errors.submit?.includes('expired')
              ? 'Your session has expired. You will be redirected to login shortly.'
              : error?.message || 
                errors.submit || 
                'Failed to create listing. Please try again.'}
          </p>
        </div>
      )}

      <form onSubmit={handleSubmit} className="create-listing-page__form">
        <div className="create-listing-page__section">
          <h2>Basic Information</h2>

          <div className="create-listing-page__field">
            <label htmlFor="category">Category *</label>
            <select
              id="category"
              value={formData.category}
              onChange={(e) => handleInputChange('category', e.target.value as ListingCategory)}
              className={errors.category ? 'error' : ''}
            >
              <option value="SALE">For Sale</option>
              <option value="RENT">For Rent</option>
            </select>
            {errors.category && <span className="error-message">{errors.category}</span>}
          </div>

          <div className="create-listing-page__field">
            <label htmlFor="propertyType">Property Type *</label>
            <select
              id="propertyType"
              value={formData.propertyType}
              onChange={(e) => handleInputChange('propertyType', e.target.value as PropertyType)}
              className={errors.propertyType ? 'error' : ''}
            >
              <option value="APARTMENT">Apartment</option>
              <option value="HOUSE">House</option>
              <option value="STUDIO">Studio</option>
              <option value="ROOM">Room</option>
              <option value="OTHER">Other</option>
            </select>
            {errors.propertyType && <span className="error-message">{errors.propertyType}</span>}
          </div>

          <div className="create-listing-page__field">
            <label htmlFor="title">Title *</label>
            <Input
              id="title"
              type="text"
              value={formData.title || ''}
              onChange={(e) => handleInputChange('title', e.target.value)}
              placeholder="e.g., Beautiful 2-bedroom apartment in city center"
              maxLength={200}
              className={errors.title ? 'error' : ''}
            />
            {errors.title && <span className="error-message">{errors.title}</span>}
          </div>

          <div className="create-listing-page__field">
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

          <div className="create-listing-page__field">
            <label htmlFor="condition">Condition *</label>
            <select
              id="condition"
              value={formData.condition}
              onChange={(e) => handleInputChange('condition', e.target.value as ListingCondition)}
            >
              <option value="NEW">New</option>
              <option value="GOOD">Good</option>
              <option value="NEEDS_RENOVATION">Needs Renovation</option>
            </select>
          </div>
        </div>

        <div className="create-listing-page__section">
          <h2>Location</h2>

          <div className="create-listing-page__field">
            <label htmlFor="addressLine">Address *</label>
            <Input
              id="addressLine"
              type="text"
              value={formData.addressLine || ''}
              onChange={(e) => handleInputChange('addressLine', e.target.value)}
              placeholder="Street address"
              className={errors.addressLine ? 'error' : ''}
            />
            {errors.addressLine && <span className="error-message">{errors.addressLine}</span>}
          </div>

          <div className="create-listing-page__field-row">
            <div className="create-listing-page__field">
              <label htmlFor="district">District *</label>
              <Input
                id="district"
                type="text"
                value={formData.district || ''}
                onChange={(e) => handleInputChange('district', e.target.value)}
                placeholder="District"
                className={errors.district ? 'error' : ''}
              />
              {errors.district && <span className="error-message">{errors.district}</span>}
            </div>

            <div className="create-listing-page__field">
              <label htmlFor="city">City *</label>
              <Input
                id="city"
                type="text"
                value={formData.city || ''}
                onChange={(e) => handleInputChange('city', e.target.value)}
                placeholder="City"
                className={errors.city ? 'error' : ''}
              />
              {errors.city && <span className="error-message">{errors.city}</span>}
            </div>

            <div className="create-listing-page__field">
              <label htmlFor="postalCode">Postal Code *</label>
              <Input
                id="postalCode"
                type="text"
                value={formData.postalCode || ''}
                onChange={(e) => handleInputChange('postalCode', e.target.value)}
                placeholder="00-000"
                className={errors.postalCode ? 'error' : ''}
              />
              {errors.postalCode && <span className="error-message">{errors.postalCode}</span>}
            </div>
          </div>

          <div className="create-listing-page__field">
            <label>Location on Map *</label>
            <p className="create-listing-page__hint">Click on the map to set the location</p>
            {errors.location && <span className="error-message">{errors.location}</span>}
            <div className="create-listing-page__map">
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
          </div>
        </div>

        <div className="create-listing-page__section">
          <h2>Property Details</h2>

          <div className="create-listing-page__field-row">
            <div className="create-listing-page__field">
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

            <div className="create-listing-page__field">
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

          <div className="create-listing-page__field-row">
            <div className="create-listing-page__field">
              <label htmlFor="floor">Floor</label>
              <Input
                id="floor"
                type="number"
                value={formData.floor || ''}
                onChange={(e) => handleInputChange('floor', e.target.value ? parseInt(e.target.value) : null)}
                placeholder="Optional"
              />
            </div>

            <div className="create-listing-page__field">
              <label htmlFor="floorCount">Total Floors</label>
              <Input
                id="floorCount"
                type="number"
                value={formData.floorCount || ''}
                onChange={(e) => handleInputChange('floorCount', e.target.value ? parseInt(e.target.value) : null)}
                placeholder="Optional"
              />
            </div>

            <div className="create-listing-page__field">
              <label htmlFor="buildYear">Build Year</label>
              <Input
                id="buildYear"
                type="number"
                min="1800"
                max={new Date().getFullYear()}
                value={formData.buildYear || ''}
                onChange={(e) => handleInputChange('buildYear', e.target.value ? parseInt(e.target.value) : null)}
                placeholder="Optional"
              />
            </div>
          </div>
        </div>

        <div className="create-listing-page__section">
          <h2>Pricing</h2>

          {formData.category === 'SALE' ? (
            <div className="create-listing-page__field">
              <label htmlFor="pricePln">Price (PLN) *</label>
              <Input
                id="pricePln"
                type="number"
                min="0"
                step="0.01"
                value={formData.pricePln || ''}
                onChange={(e) => handleInputChange('pricePln', e.target.value ? parseFloat(e.target.value) : null)}
                placeholder="0.00"
                className={errors.pricePln ? 'error' : ''}
              />
              {errors.pricePln && <span className="error-message">{errors.pricePln}</span>}
            </div>
          ) : (
            <div className="create-listing-page__field">
              <label htmlFor="monthlyRentPln">Monthly Rent (PLN) *</label>
              <Input
                id="monthlyRentPln"
                type="number"
                min="0"
                step="0.01"
                value={formData.monthlyRentPln || ''}
                onChange={(e) => handleInputChange('monthlyRentPln', e.target.value ? parseFloat(e.target.value) : null)}
                placeholder="0.00"
                className={errors.monthlyRentPln ? 'error' : ''}
              />
              {errors.monthlyRentPln && <span className="error-message">{errors.monthlyRentPln}</span>}
            </div>
          )}
        </div>

        <div className="create-listing-page__section">
          <h2>Features</h2>

          <div className="create-listing-page__features">
            {[
              { key: 'hasBalcony', label: 'Balcony' },
              { key: 'hasElevator', label: 'Elevator' },
              { key: 'hasParkingSpace', label: 'Parking Space' },
              { key: 'hasSecurity', label: 'Security' },
              { key: 'hasStorageRoom', label: 'Storage Room' },
            ].map((feature) => (
              <label key={feature.key} className="create-listing-page__checkbox">
                <input
                  type="checkbox"
                  checked={formData[feature.key as keyof CreateListingInput] as boolean || false}
                  onChange={(e) => handleInputChange(feature.key as keyof CreateListingInput, e.target.checked)}
                />
                <span>{feature.label}</span>
              </label>
            ))}
          </div>
        </div>

        {createdListingId && (
          <div className="create-listing-page__section" onClick={(e) => e.stopPropagation()}>
            <h2>Photos</h2>
            <p className="create-listing-page__hint">Listing created! You can now upload photos.</p>
            <PhotoManager listingId={createdListingId} />
          </div>
        )}

        <div className="create-listing-page__actions">
          {!createdListingId ? (
            <>
              <Button
                type="submit"
                variant="primary"
                disabled={loading}
                isLoading={loading}
              >
                {loading ? 'Saving...' : 'Save Listing'}
              </Button>
              <Button
                type="button"
                variant="outline"
                onClick={() => navigate('/dashboard')}
                disabled={loading}
              >
                Cancel
              </Button>
            </>
          ) : (
            <>
              <Button
                type="button"
                variant="primary"
                onClick={() => navigate(`/dashboard/listings/${createdListingId}`)}
              >
                Go to Listing
              </Button>
              <Button
                type="button"
                variant="outline"
                onClick={() => navigate('/dashboard')}
              >
                Back to Dashboard
              </Button>
            </>
          )}
        </div>
      </form>
    </div>
  );
};

