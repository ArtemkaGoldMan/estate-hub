import { memo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import clsx from 'clsx';
import { FaClock, FaTimesCircle, FaCheckCircle, FaExclamationTriangle, FaInfoCircle, FaEdit, FaTrash, FaBullhorn, FaFileAlt, FaArchive } from 'react-icons/fa';
import type { Listing } from '../../../entities/listing/model/types';
import { useChangeListingStatus } from '../../../entities/listing/api/publish-listing';
import { useDeleteListing } from '../../../entities/listing/api/delete-listing';
import { useToast } from '../../../shared/context/ToastContext';
import { formatCurrency } from '../../../shared';
import { Button } from '../../../shared/ui';
import './DashboardListingCard.css';

type DashboardCardMode = 'full' | 'archive-only';

interface DashboardListingCardProps {
  listing: Listing;
  onStatusChange?: () => void;
  className?: string;
  mode?: DashboardCardMode;
}

const FALLBACK_IMAGE =
  'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=1200&q=80';

const getPriceLabel = (listing: Listing) => {
  if (listing.category === 'RENT') {
    return `${formatCurrency(listing.monthlyRentPln)} / month`;
  }
  return formatCurrency(listing.pricePln);
};

export const DashboardListingCard = memo(
  ({ listing, onStatusChange, className, mode = 'full' }: DashboardListingCardProps) => {
    const navigate = useNavigate();
    const { showSuccess, showError } = useToast();
    const { publishListing, unpublishListing, unarchiveListing, archiveListing, loading: statusLoading } = useChangeListingStatus();
    const { deleteListing, loading: deleteLoading } = useDeleteListing();
    const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

    const coverUrl = listing.firstPhotoUrl ?? FALLBACK_IMAGE;
    const priceLabel = getPriceLabel(listing);
    const isPublished = listing.status === 'Published';
    const isDraft = listing.status === 'Draft';
    const isArchived = listing.status === 'Archived';
    
    // Check if listing can be published (must be approved by moderation)
    const canPublish = isDraft && listing.isModerationApproved === true;
    const moderationPending = isDraft && listing.isModerationApproved === null;
    const moderationRejected = isDraft && listing.isModerationApproved === false;

    const handleView = () => {
      navigate(`/dashboard/listings/${listing.id}`);
    };

    const handleUnarchive = async (e: React.MouseEvent) => {
      e.stopPropagation();
      try {
        await unarchiveListing(listing.id);
        onStatusChange?.();
        showSuccess('Listing unarchived successfully. It is now in draft status.');
      } catch (error) {
        showError(error instanceof Error ? error.message : 'Failed to unarchive listing');
      }
    };

    const handleEdit = (e: React.MouseEvent) => {
      e.stopPropagation();
      navigate(`/listings/${listing.id}/edit`);
    };

    const handlePublish = async (e: React.MouseEvent) => {
      e.stopPropagation();
      try {
        await publishListing(listing.id);
        // Manually trigger refetch via callback
        onStatusChange?.();
        showSuccess('Listing published successfully');
      } catch (error) {
        showError(error instanceof Error ? error.message : 'Failed to publish listing');
      }
    };

    const handleUnpublish = async (e: React.MouseEvent) => {
      e.stopPropagation();
      try {
        await unpublishListing(listing.id);
        // Manually trigger refetch via callback
        onStatusChange?.();
        showSuccess('Listing unpublished successfully');
      } catch (error) {
        showError(error instanceof Error ? error.message : 'Failed to unpublish listing');
      }
    };

    const handleDelete = async (e: React.MouseEvent) => {
      e.stopPropagation();
      if (!showDeleteConfirm) {
        setShowDeleteConfirm(true);
        return;
      }

      try {
        await deleteListing(listing.id);
        onStatusChange?.();
        showSuccess('Listing deleted successfully');
      } catch (error) {
        showError(error instanceof Error ? error.message : 'Failed to delete listing');
        setShowDeleteConfirm(false);
      }
    };

    const handleCancelDelete = (e: React.MouseEvent) => {
      e.stopPropagation();
      setShowDeleteConfirm(false);
    };

    return (
      <article
        className={clsx('dashboard-listing-card', className)}
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
          className="dashboard-listing-card__media"
          style={{ backgroundImage: `url(${coverUrl})` }}
        >
          <span className="dashboard-listing-card__badge">
            {listing.category === 'RENT' ? 'For Rent' : 'For Sale'}
          </span>
          {isDraft && (
            <>
              <span className="dashboard-listing-card__badge dashboard-listing-card__badge--draft">
                Draft
              </span>
              {listing.isModerationApproved === null && (
                <span className="dashboard-listing-card__badge dashboard-listing-card__badge--moderation-pending">
                  <FaClock style={{ marginRight: '0.25rem' }} /> Pending
                </span>
              )}
              {listing.isModerationApproved === false && (
                <span className="dashboard-listing-card__badge dashboard-listing-card__badge--moderation-rejected">
                  <FaTimesCircle style={{ marginRight: '0.25rem' }} /> Rejected
                </span>
              )}
              {listing.isModerationApproved === true && (
                <span className="dashboard-listing-card__badge dashboard-listing-card__badge--moderation-approved">
                  <FaCheckCircle style={{ marginRight: '0.25rem' }} /> Approved
                </span>
              )}
            </>
          )}
          {isArchived && (
            <span className="dashboard-listing-card__badge dashboard-listing-card__badge--archived">
              Archived
            </span>
          )}
        </div>
        <div className="dashboard-listing-card__body">
          <header className="dashboard-listing-card__header">
            <h3 className="dashboard-listing-card__title">{listing.title}</h3>
            <span className="dashboard-listing-card__price">{priceLabel}</span>
          </header>
          <p className="dashboard-listing-card__location">
            {listing.city}, {listing.district}
          </p>
          <dl className="dashboard-listing-card__meta">
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
          <div className="dashboard-listing-card__status">
            <strong>Status:</strong> {listing.status}
          </div>
          {moderationRejected && (
            <div className="dashboard-listing-card__moderation-warning">
              <FaExclamationTriangle style={{ marginRight: '0.5rem' }} /> Content rejected: {listing.moderationRejectionReason || 'Please edit and re-check moderation'}
            </div>
          )}
          {moderationPending && (
            <div className="dashboard-listing-card__moderation-info">
              <FaInfoCircle style={{ marginRight: '0.5rem' }} /> Moderation not checked yet
            </div>
          )}
          <div className="dashboard-listing-card__actions" onClick={(e) => e.stopPropagation()}>
            {mode === 'archive-only' ? (
              // Archive mode: only show Unarchive button
              <Button
                variant="outline"
                size="sm"
                onClick={handleUnarchive}
                disabled={statusLoading}
                isLoading={statusLoading}
                style={{ width: '100%' }}
              >
                <FaArchive style={{ marginRight: '0.5rem' }} /> Unarchive
              </Button>
            ) : (
              // Full mode: show all management buttons
              <>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={handleEdit}
                  disabled={statusLoading || deleteLoading}
                >
                  <FaEdit style={{ marginRight: '0.5rem' }} /> Edit
                </Button>
                {isDraft && (
                  <Button
                    variant="primary"
                    size="sm"
                    onClick={handlePublish}
                    disabled={statusLoading || deleteLoading || !canPublish}
                    isLoading={statusLoading}
                    title={
                      !canPublish
                        ? moderationRejected
                          ? 'Content must pass moderation before publishing'
                          : moderationPending
                          ? 'Please check moderation first'
                          : 'Content must pass moderation before publishing'
                        : 'Publish listing'
                    }
                  >
                    <FaBullhorn style={{ marginRight: '0.5rem' }} /> Publish
                  </Button>
                )}
                {isPublished && (
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={handleUnpublish}
                    disabled={statusLoading || deleteLoading}
                    isLoading={statusLoading}
                  >
                    <FaFileAlt style={{ marginRight: '0.5rem' }} /> Unpublish
                  </Button>
                )}
                {!isArchived && (
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={async (e) => {
                      e.stopPropagation();
                      if (!confirm('Are you sure you want to archive this listing? It will be hidden from public view.')) {
                        return;
                      }
                      try {
                        await archiveListing(listing.id);
                        onStatusChange?.();
                        showSuccess('Listing archived successfully');
                      } catch (error) {
                        showError(error instanceof Error ? error.message : 'Failed to archive listing');
                      }
                    }}
                    disabled={statusLoading || deleteLoading}
                    isLoading={statusLoading}
                  >
                    <FaArchive style={{ marginRight: '0.5rem' }} /> Archive
                  </Button>
                )}
                {!showDeleteConfirm ? (
                  <Button
                    variant="danger"
                    size="sm"
                    onClick={handleDelete}
                    disabled={statusLoading || deleteLoading}
                  >
                    <FaTrash style={{ marginRight: '0.5rem' }} /> Delete
                  </Button>
                ) : (
                  <>
                    <Button
                      variant="danger"
                      size="sm"
                      onClick={handleDelete}
                      disabled={statusLoading || deleteLoading}
                      isLoading={deleteLoading}
                    >
                      ✓ Confirm
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={handleCancelDelete}
                      disabled={statusLoading || deleteLoading}
                    >
                      Cancel
                    </Button>
                  </>
                )}
              </>
            )}
          </div>
        </div>
      </article>
    );
  }
);

DashboardListingCard.displayName = 'DashboardListingCard';

