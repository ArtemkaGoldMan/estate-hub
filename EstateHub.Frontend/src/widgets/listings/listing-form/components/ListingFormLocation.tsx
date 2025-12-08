import { Input } from '../../../../shared';
import { MapContainer, TileLayer } from 'react-leaflet';
import { LocationPicker } from './LocationPicker';
import 'leaflet/dist/leaflet.css';

interface ListingFormLocationProps {
  addressLine?: string;
  district: string;
  city: string;
  postalCode?: string;
  mapPosition: [number, number] | null;
  errors: Record<string, string>;
  onAddressLineChange?: (value: string) => void;
  onDistrictChange: (value: string) => void;
  onCityChange: (value: string) => void;
  onPostalCodeChange?: (value: string) => void;
  onMapPositionChange: (lat: number, lng: number) => void;
}

const DEFAULT_CENTER: [number, number] = [52.2297, 21.0122];

export const ListingFormLocation = ({
  addressLine,
  district,
  city,
  postalCode,
  mapPosition,
  errors,
  onAddressLineChange,
  onDistrictChange,
  onCityChange,
  onPostalCodeChange,
  onMapPositionChange,
}: ListingFormLocationProps) => {
  const center = mapPosition || DEFAULT_CENTER;

  return (
    <div className="listing-form__section">
      <h2>Location</h2>

      {onAddressLineChange && (
        <div className="listing-form__field">
          <label htmlFor="addressLine">Address *</label>
          <Input
            id="addressLine"
            type="text"
            value={addressLine || ''}
            onChange={(e) => onAddressLineChange(e.target.value)}
            placeholder="Street address"
            error={errors.addressLine}
          />
        </div>
      )}

      <div className="listing-form__field-row">
        <div className="listing-form__field">
          <label htmlFor="district">District *</label>
          <Input
            id="district"
            type="text"
            value={district}
            onChange={(e) => onDistrictChange(e.target.value)}
            placeholder="District"
            error={errors.district}
          />
        </div>

        <div className="listing-form__field">
          <label htmlFor="city">City *</label>
          <Input
            id="city"
            type="text"
            value={city}
            onChange={(e) => onCityChange(e.target.value)}
            placeholder="City"
            error={errors.city}
          />
        </div>

        {onPostalCodeChange && (
          <div className="listing-form__field">
            <label htmlFor="postalCode">Postal Code *</label>
            <Input
              id="postalCode"
              type="text"
              value={postalCode || ''}
              onChange={(e) => onPostalCodeChange(e.target.value)}
              placeholder="00-000"
              error={errors.postalCode}
            />
          </div>
        )}
      </div>

      <div className="listing-form__field">
        <label>Location on Map *</label>
        <p className="listing-form__hint">Click on the map to set the location</p>
        {errors.location && <span className="error-message">{errors.location}</span>}
        <div className="listing-form__map">
          <MapContainer
            center={center}
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
              onPositionChange={onMapPositionChange}
            />
          </MapContainer>
        </div>
      </div>
    </div>
  );
};



