import { MapContainer, TileLayer, Marker, Popup } from 'react-leaflet';
import type { Listing } from '../../../../entities/listing';
import 'leaflet/dist/leaflet.css';
import './ListingDetailMap.css';

interface ListingDetailMapProps {
  listing: Listing;
}

export const ListingDetailMap = ({ listing }: ListingDetailMapProps) => {
  return (
    <div className="listing-detail-map">
      <h2>Location</h2>
      <div className="listing-detail-map__container">
        <MapContainer
          center={[listing.latitude, listing.longitude]}
          zoom={15}
          style={{ height: '400px', width: '100%' }}
          scrollWheelZoom={false}
        >
          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          <Marker position={[listing.latitude, listing.longitude]}>
            <Popup>
              <strong>{listing.title}</strong>
              <br />
              {listing.addressLine && `${listing.addressLine}, `}
              {listing.city}
              {listing.district && `, ${listing.district}`}
              {listing.postalCode && `, ${listing.postalCode}`}
            </Popup>
          </Marker>
        </MapContainer>
      </div>
      <div className="listing-detail-map__address">
        <p>
          {listing.addressLine && (
            <>
              <strong>{listing.addressLine}</strong>
              <br />
            </>
          )}
          <strong>{listing.city}</strong>
          {listing.district && `, ${listing.district}`}
          {listing.postalCode && `, ${listing.postalCode}`}
        </p>
      </div>
    </div>
  );
};

