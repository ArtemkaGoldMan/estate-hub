import { useEffect, useMemo, useState } from 'react';
import {
  MapContainer,
  TileLayer,
  CircleMarker,
  Popup,
  useMapEvents,
} from 'react-leaflet';
import type { LatLngBounds } from 'leaflet';
import 'leaflet/dist/leaflet.css';
import type { Listing, MapBounds } from '../../../../../entities/listing';
import './ListingsMap.css';

interface ListingsMapProps {
  listings: Listing[];
  onBoundsChange?: (bounds: MapBounds) => void;
  onSelectListing?: (listing: Listing) => void;
}

const DEFAULT_CENTER: [number, number] = [52.2297, 21.0122]; // Warsaw
const DEFAULT_ZOOM = 12;

const boundsToDto = (bounds: LatLngBounds): MapBounds => ({
  latMin: bounds.getSouth(),
  latMax: bounds.getNorth(),
  lonMin: bounds.getWest(),
  lonMax: bounds.getEast(),
});

const BoundsTracker = ({
  onBoundsChange,
}: {
  onBoundsChange?: (bounds: MapBounds) => void;
}) => {
  const map = useMapEvents({
    moveend: () => {
      onBoundsChange?.(boundsToDto(map.getBounds()));
    },
    zoomend: () => {
      onBoundsChange?.(boundsToDto(map.getBounds()));
    },
  });

  // Only call onBoundsChange once on mount, not on every render
  useEffect(() => {
    onBoundsChange?.(boundsToDto(map.getBounds()));
    // map is stable from useMapEvents, onBoundsChange is optional callback
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [map]);

  return null;
};

const computeCenter = (listings: Listing[]): [number, number] => {
  if (!listings.length) {
    return DEFAULT_CENTER;
  }

  const latSum = listings.reduce((sum, listing) => sum + listing.latitude, 0);
  const lonSum = listings.reduce((sum, listing) => sum + listing.longitude, 0);

  return [latSum / listings.length, lonSum / listings.length];
};

export const ListingsMap = ({
  listings,
  onBoundsChange,
  onSelectListing,
}: ListingsMapProps) => {
  const [isClient, setIsClient] = useState(false);

  useEffect(() => {
    setIsClient(true);
  }, []);

  const center = useMemo(() => computeCenter(listings), [listings]);

  if (!isClient) {
    return null;
  }

  return (
    <div className="listings-map">
      <MapContainer
        center={center}
        zoom={DEFAULT_ZOOM}
        scrollWheelZoom={true}
        className="listings-map__container"
      >
        <TileLayer
          attribution='&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />
        <BoundsTracker onBoundsChange={onBoundsChange} />
        {listings.map((listing) => (
          <CircleMarker
            key={listing.id}
            center={[listing.latitude, listing.longitude]}
            radius={8}
            color="#2563eb"
            fillColor="#2563eb"
            fillOpacity={0.7}
            eventHandlers={{
              click: () => onSelectListing?.(listing),
            }}
          >
            <Popup>
              <div className="listings-map__popup">
                <strong>{listing.title}</strong>
                <p>
                  {listing.city}, {listing.district}
                </p>
              </div>
            </Popup>
          </CircleMarker>
        ))}
      </MapContainer>
    </div>
  );
};


