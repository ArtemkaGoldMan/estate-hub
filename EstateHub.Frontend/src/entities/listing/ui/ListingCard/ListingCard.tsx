import { memo } from 'react';
import { useNavigate } from 'react-router-dom';
import clsx from 'clsx';
import type { Listing } from '../../model/types';
import { formatCurrency } from '../../../../shared';
import './ListingCard.css';

interface ListingCardProps {
  listing: Listing;
  onClick?: (listing: Listing) => void;
  className?: string;
}

const FALLBACK_IMAGE =
  'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=1200&q=80';

const getPriceLabel = (listing: Listing) => {
  if (listing.category === 'RENT') {
    return `${formatCurrency(listing.monthlyRentPln)} / month`;
  }

  return formatCurrency(listing.pricePln);
};

export const ListingCard = memo(
  ({ listing, onClick, className }: ListingCardProps) => {
    const navigate = useNavigate();
    const coverUrl = listing.firstPhotoUrl ?? FALLBACK_IMAGE;
    const priceLabel = getPriceLabel(listing);

    const handleClick = (e: React.MouseEvent<HTMLElement>) => {
      e.preventDefault();
      e.stopPropagation();
      
      // Call onClick callback if provided (for map clicks, etc.)
      if (onClick) {
        onClick(listing);
      } else {
        // Use navigate for programmatic navigation
        // Pass current location as state so we can go back to the right place
        const currentPath = window.location.pathname;
        navigate(`/listings/${listing.id}`, { 
          replace: false,
          state: { from: currentPath }
        });
      }
    };

    return (
      <article
        className={clsx('listing-card', className)}
        onClick={handleClick}
        role="button"
        tabIndex={0}
        onKeyDown={(e) => {
          if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            handleClick(e as unknown as React.MouseEvent<HTMLElement>);
          }
        }}
        aria-label={`View listing: ${listing.title}`}
      >
        <div
          className="listing-card__media"
          style={{ backgroundImage: `url(${coverUrl})` }}
        >
          <span className="listing-card__badge">
            {listing.category === 'RENT' ? 'For Rent' : 'For Sale'}
          </span>
          {listing.status === 'Draft' && (
            <span className="listing-card__badge listing-card__badge--draft" title="Draft - Not visible to others">
              Draft
            </span>
          )}
          {listing.isLikedByCurrentUser && (
            <span className="listing-card__favorite" aria-label="Liked listing">
              ♥
            </span>
          )}
        </div>
        <div className="listing-card__body">
          <header className="listing-card__header">
            <h3 className="listing-card__title">{listing.title}</h3>
            <span className="listing-card__price">{priceLabel}</span>
          </header>
          <p className="listing-card__location">
            {listing.city}, {listing.district}
          </p>
          <dl className="listing-card__meta">
            <div>
              <dt>Area</dt>
              <dd>{listing.squareMeters} m²</dd>
            </div>
            <div>
              <dt>Rooms</dt>
              <dd>{listing.rooms}</dd>
            </div>
            {listing.floor !== null && listing.floor !== undefined && (
              <div>
                <dt>Floor</dt>
                <dd>
                  {listing.floor}
                  {listing.floorCount ? ` / ${listing.floorCount}` : ''}
                </dd>
              </div>
            )}
          </dl>
        </div>
      </article>
    );
  }
);

ListingCard.displayName = 'ListingCard';


