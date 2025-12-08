import type { Listing } from '../../../../entities/listing';
import './ListingDetailSpecs.css';

interface ListingDetailSpecsProps {
  listing: Listing;
}

export const ListingDetailSpecs = ({ listing }: ListingDetailSpecsProps) => {
  return (
    <div className="listing-detail-specs">
      <h2>Property Details</h2>
      <div className="listing-detail-specs__grid">
        <div className="listing-detail-specs__item">
          <span className="listing-detail-specs__label">Property Type</span>
          <span className="listing-detail-specs__value">{listing.propertyType}</span>
        </div>
        <div className="listing-detail-specs__item">
          <span className="listing-detail-specs__label">Category</span>
          <span className="listing-detail-specs__value">{listing.category}</span>
        </div>
        <div className="listing-detail-specs__item">
          <span className="listing-detail-specs__label">Square Meters</span>
          <span className="listing-detail-specs__value">{listing.squareMeters} m²</span>
        </div>
        <div className="listing-detail-specs__item">
          <span className="listing-detail-specs__label">Rooms</span>
          <span className="listing-detail-specs__value">{listing.rooms}</span>
        </div>
        {listing.floor !== null && listing.floor !== undefined && (
          <div className="listing-detail-specs__item">
            <span className="listing-detail-specs__label">Floor</span>
            <span className="listing-detail-specs__value">
              {listing.floor}
              {listing.floorCount && ` / ${listing.floorCount}`}
            </span>
          </div>
        )}
        {listing.buildYear && (
          <div className="listing-detail-specs__item">
            <span className="listing-detail-specs__label">Build Year</span>
            <span className="listing-detail-specs__value">{listing.buildYear}</span>
          </div>
        )}
        <div className="listing-detail-specs__item">
          <span className="listing-detail-specs__label">Condition</span>
          <span className="listing-detail-specs__value">{listing.condition}</span>
        </div>
      </div>
    </div>
  );
};

