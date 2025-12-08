import type { Listing } from '../../../../entities/listing';
import './ListingDetailFeatures.css';

interface ListingDetailFeaturesProps {
  listing: Listing;
}

export const ListingDetailFeatures = ({ listing }: ListingDetailFeaturesProps) => {
  const hasAnyFeature =
    listing.hasBalcony ||
    listing.hasElevator ||
    listing.hasParkingSpace ||
    listing.hasSecurity ||
    listing.hasStorageRoom;
  
  return (
    <div className="listing-detail-features">
      <h2>Features</h2>
      <div className="listing-detail-features__list">
        {listing.hasBalcony && (
          <span className="listing-detail-features__item">Balcony</span>
        )}
        {listing.hasElevator && (
          <span className="listing-detail-features__item">Elevator</span>
        )}
        {listing.hasParkingSpace && (
          <span className="listing-detail-features__item">Parking</span>
        )}
        {listing.hasSecurity && (
          <span className="listing-detail-features__item">Security</span>
        )}
        {listing.hasStorageRoom && (
          <span className="listing-detail-features__item">Storage Room</span>
        )}
        {!hasAnyFeature && (
          <span className="listing-detail-features__item--none">No features listed</span>
        )}
      </div>
    </div>
  );
};

