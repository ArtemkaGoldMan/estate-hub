import { Button } from '../../../../shared/ui';
import { FaHeart, FaRegHeart, FaArchive, FaExclamationTriangle } from 'react-icons/fa';
import type { Listing } from '../../../../entities/listing';
import './ListingDetailHeader.css';

interface ListingDetailHeaderProps {
  listing: Listing;
  isAuthenticated: boolean;
  isOwner: boolean;
  likeLoading: boolean;
  statusLoading: boolean;
  onBack: () => void;
  onLike: () => void;
  onReport: () => void;
  onArchive: () => void;
  onUnarchive: () => void;
}

export const ListingDetailHeader = ({
  listing,
  isAuthenticated,
  isOwner,
  likeLoading,
  statusLoading,
  onBack,
  onLike,
  onReport,
  onArchive,
  onUnarchive,
}: ListingDetailHeaderProps) => {
  return (
    <div className="listing-detail-header">
      <Button variant="ghost" onClick={onBack}>
        ← Back
      </Button>
      
      {isAuthenticated && (
        <div className="listing-detail-header__actions">
          <Button
            variant={listing.isLikedByCurrentUser ? 'primary' : 'outline'}
            onClick={onLike}
            disabled={likeLoading}
            isLoading={likeLoading}
          >
            {listing.isLikedByCurrentUser ? (
              <>
                <FaHeart style={{ marginRight: '0.5rem' }} /> Liked
              </>
            ) : (
              <>
                <FaRegHeart style={{ marginRight: '0.5rem' }} /> Like
              </>
            )}
          </Button>
          
          <Button variant="outline" onClick={onReport}>
            <FaExclamationTriangle style={{ marginRight: '0.5rem' }} /> Report
          </Button>
          
          {isOwner && (
            <>
              {listing.status !== 'Archived' && (
                <Button
                  variant="outline"
                  onClick={onArchive}
                  disabled={statusLoading}
                  isLoading={statusLoading}
                >
                  <FaArchive style={{ marginRight: '0.5rem' }} /> Archive
                </Button>
              )}
              {listing.status === 'Archived' && (
                <Button
                  variant="outline"
                  onClick={onUnarchive}
                  disabled={statusLoading}
                  isLoading={statusLoading}
                >
                  <FaArchive style={{ marginRight: '0.5rem' }} /> Unarchive
                </Button>
              )}
            </>
          )}
        </div>
      )}
    </div>
  );
};

