import { formatCurrency } from '../../../../shared';
import type { Listing } from '../../../../entities/listing';
import './ListingDetailTitle.css';

interface ListingDetailTitleProps {
  listing: Listing;
}

export const ListingDetailTitle = ({ listing }: ListingDetailTitleProps) => {
  return (
    <div className="listing-detail-title">
      <h1 className="listing-detail-title__title">{listing.title}</h1>
      <div className="listing-detail-title__price">
        {listing.category === 'SALE' && listing.pricePln && (
          <span className="listing-detail-title__price-main">
            {formatCurrency(listing.pricePln)}
          </span>
        )}
        {listing.category === 'RENT' && listing.monthlyRentPln && (
          <span className="listing-detail-title__price-main">
            {formatCurrency(listing.monthlyRentPln)}/month
          </span>
        )}
      </div>
      <div className="listing-detail-title__location">
        {listing.addressLine && (
          <>
            <span className="listing-detail-title__location-address">{listing.addressLine}</span>
            <span className="listing-detail-title__location-separator">,</span>
          </>
        )}
        <span className="listing-detail-title__location-city">{listing.city}</span>
        {listing.district && (
          <>
            <span className="listing-detail-title__location-separator">,</span>
            <span className="listing-detail-title__location-district">{listing.district}</span>
          </>
        )}
        {listing.postalCode && (
          <>
            <span className="listing-detail-title__location-separator">,</span>
            <span className="listing-detail-title__location-postal">{listing.postalCode}</span>
          </>
        )}
      </div>
    </div>
  );
};

