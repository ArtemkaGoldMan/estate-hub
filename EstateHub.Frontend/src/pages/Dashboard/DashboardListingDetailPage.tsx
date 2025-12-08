import { Button, LoadingSpinner } from '../../shared';
import { PhotoGallery } from '../../entities/listing/ui';
import {
  ListingDetailTitle,
  ListingDetailDescription,
  ListingDetailSpecs,
  ListingDetailFeatures,
  ListingDetailMap,
  ListingDetailContact,
  ListingDetailOwnerActions,
  ListingDetailModerationStatus,
  ListingDetailDraftNotice,
} from '../../widgets/listings/listing-detail';
import { useDashboardListingDetail } from './hooks/useDashboardListingDetail';
import './DashboardListingDetailPage.css';

export const DashboardListingDetailPage = () => {
  const {
    listing,
    photos,
    ownerInfo,
    loading,
    loadingOwner,
    error,
    statusLoading,
    deleteLoading,
    showDeleteConfirm,
    isOwner,
    isDraft,
    canPublish,
    handlePublish,
    handleUnpublish,
    handleArchive,
    handleUnarchive,
    handleDelete,
    handleBack,
    handleEdit,
  } = useDashboardListingDetail();

  if (loading) {
    return (
      <div className="dashboard-listing-detail-page">
        <div className="dashboard-listing-detail-page__loading">
          <LoadingSpinner text="Loading listing..." />
        </div>
      </div>
    );
  }

  if (error || !listing) {
    return (
      <div className="dashboard-listing-detail-page">
        <div className="dashboard-listing-detail-page__error">
          <h2>Listing not found</h2>
          <p>Sorry, we couldn't find the listing you're looking for.</p>
          <Button onClick={handleBack}>Back to Dashboard</Button>
        </div>
      </div>
    );
  }

  if (!isOwner) {
    return (
      <div className="dashboard-listing-detail-page">
        <div className="dashboard-listing-detail-page__error">
          <h2>Access Denied</h2>
          <p>You don't have permission to view this listing.</p>
          <Button onClick={handleBack}>Back to Dashboard</Button>
        </div>
      </div>
    );
  }

  return (
    <div className="dashboard-listing-detail-page">
      <div className="dashboard-listing-detail-page__header">
        <Button variant="ghost" onClick={handleBack}>
          ← Back to Dashboard
        </Button>
      </div>

      {isDraft && <ListingDetailDraftNotice />}

      <div className="dashboard-listing-detail-page__content">
        <div className="dashboard-listing-detail-page__main-content">
          <div className="dashboard-listing-detail-page__gallery">
            <PhotoGallery photos={photos} fallbackUrl={listing.firstPhotoUrl} />
          </div>

          <ListingDetailTitle listing={listing} />
          <ListingDetailDescription listing={listing} />
          <ListingDetailSpecs listing={listing} />
          <ListingDetailFeatures listing={listing} />
          <ListingDetailMap listing={listing} />
          <ListingDetailContact
            isAuthenticated={true}
            loadingOwner={loadingOwner}
            ownerInfo={ownerInfo}
          />
        </div>

        <div className="dashboard-listing-detail-page__management-panel">
          {listing.adminUnpublishReason && (
            <div className="dashboard-listing-detail-page__management-section">
              <h2>Admin Action Required</h2>
              <div className="dashboard-listing-detail-page__moderation-review--rejected">
                  <strong>Listing Unpublished by Administrator</strong>
                <p><strong>Reason:</strong> {listing.adminUnpublishReason}</p>
                <p>Your listing has been unpublished due to a report. Please review the reason above, make necessary changes, and the listing will need to be re-moderated before it can be republished.</p>
              </div>
            </div>
          )}

          {isDraft && !listing.adminUnpublishReason && (
            <div className="dashboard-listing-detail-page__management-section">
              <h2>Moderator Review</h2>
              <ListingDetailModerationStatus listing={listing} />
            </div>
          )}

          <div className="dashboard-listing-detail-page__management-section">
            <ListingDetailOwnerActions
              listing={listing}
              statusLoading={statusLoading}
              deleteLoading={deleteLoading}
              showDeleteConfirm={showDeleteConfirm}
              canPublish={canPublish}
              onEdit={handleEdit}
              onPublish={handlePublish}
              onUnpublish={handleUnpublish}
              onArchive={handleArchive}
              onUnarchive={handleUnarchive}
              onDelete={handleDelete}
            />
          </div>
        </div>
      </div>
    </div>
  );
};
