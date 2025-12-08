import { memo } from 'react';
import { useNavigate } from 'react-router-dom';
import clsx from 'clsx';
import { FaHeart } from 'react-icons/fa';
import type { Listing } from '../../../entities/listing/model/types';
import { useLikeListing } from '../../../entities/listing/api/like-listing';
import { useToast } from '../../../shared/context/ToastContext';
import { formatCurrency } from '../../../shared';
import { Button } from '../../../shared/ui';
import './LikedListingCard.css';

interface LikedListingCardProps {
  listing: Listing;
  onStatusChange?: () => void;
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

export const LikedListingCard = memo(
  ({ listing, onStatusChange, className }: LikedListingCardProps) => {
    const navigate = useNavigate();
    const { showSuccess, showError } = useToast();
    const { toggleLike, loading: likeLoading } = useLikeListing();

    const coverUrl = listing.firstPhotoUrl ?? FALLBACK_IMAGE;
    const priceLabel = getPriceLabel(listing);

    const handleView = () => {
      navigate(`/listings/${listing.id}`, { state: { from: '/dashboard' } });
    };

    const handleUnlike = async (e: React.MouseEvent) => {
      e.stopPropagation();
      try {
        await toggleLike(listing.id, true); // true = currently liked, so this will unlike
        onStatusChange?.();
        showSuccess('Removed from liked listings');
      } catch (error) {
        showError(error instanceof Error ? error.message : 'Failed to unlike listing');
      }
    };

    return (
      <article
        className={clsx('liked-listing-card', className)}
        onClick={handleView}
        role="button"
        tabIndex={0}
        onKeyDown={(e) => {
          if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            handleView();
          }
        }}
        aria-label={`View listing: ${listing.title}`}
      >
        <div
          className="liked-listing-card__media"
          style={{ backgroundImage: `url(${coverUrl})` }}
        >
          <span className="liked-listing-card__badge">
            {listing.category === 'RENT' ? 'For Rent' : 'For Sale'}
          </span>
        </div>
        <div className="liked-listing-card__body">
          <header className="liked-listing-card__header">
            <h3 className="liked-listing-card__title">{listing.title}</h3>
            <span className="liked-listing-card__price">{priceLabel}</span>
          </header>
          <p className="liked-listing-card__location">
            {listing.addressLine && `${listing.addressLine}, `}
            {listing.city}
            {listing.district && `, ${listing.district}`}
            {listing.postalCode && `, ${listing.postalCode}`}
          </p>
          <dl className="liked-listing-card__meta">
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
          <div className="liked-listing-card__actions" onClick={(e) => e.stopPropagation()}>
            <Button
              variant="outline"
              size="sm"
              onClick={handleUnlike}
              disabled={likeLoading}
              isLoading={likeLoading}
              style={{ width: '100%' }}
            >
              <FaHeart style={{ marginRight: '0.5rem' }} /> Unlike
            </Button>
          </div>
        </div>
      </article>
    );
  }
);

LikedListingCard.displayName = 'LikedListingCard';




