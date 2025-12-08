import { Button, LoadingSpinner } from '../../shared/ui';
import { PhotoGallery } from '../../entities/listing/ui';
import { CreateReportModal } from '../../features/reports/ui/CreateReportModal';
import { AskAI } from '../../features/listings/ask-ai/ui/AskAI';
import {
  useListingDetail,
  ListingDetailHeader,
  ListingDetailTitle,
  ListingDetailDescription,
  ListingDetailSpecs,
  ListingDetailFeatures,
  ListingDetailMap,
  ListingDetailContact,
  ListingDetailAdminPanel,
  AdminUnpublishModal,
} from '../../widgets/listings/listing-detail';
import './ListingDetailPage.css';

export const ListingDetailPage = () => {
  const {
    listing,
    photos,
    ownerInfo,
    loading,
    loadingOwner,
    likeLoading,
    statusLoading,
    error,
    isAuthenticated,
    isAdmin,
    isOwner,
    showReportModal,
    setShowReportModal,
    showAdminUnpublishModal,
    adminUnpublishReason,
    setAdminUnpublishReason,
    adminUnpublishLoading,
    handleLike,
    handleReport,
    handleAdminUnpublishClick,
    handleAdminUnpublishConfirm,
    handleAdminUnpublishClose,
    handleArchive,
    handleUnarchive,
    handleBack,
  } = useListingDetail();

  if (loading) {
    return (
      <div className="listing-detail-page">
        <div className="listing-detail-page__loading">
          <LoadingSpinner text="Loading listing..." />
        </div>
      </div>
    );
  }

  if (error || !listing) {
    return (
      <div className="listing-detail-page">
        <div className="listing-detail-page__error">
          <h2>Listing not found</h2>
          <p>Sorry, we couldn't find the listing you're looking for.</p>
          <Button onClick={handleBack}>Back</Button>
        </div>
      </div>
    );
  }

  return (
    <div className="listing-detail-page">
      <ListingDetailHeader
        listing={listing}
        isAuthenticated={isAuthenticated}
        isOwner={isOwner}
        likeLoading={likeLoading}
        statusLoading={statusLoading}
        onBack={handleBack}
        onLike={handleLike}
        onReport={handleReport}
        onArchive={handleArchive}
        onUnarchive={handleUnarchive}
      />

      <div className="listing-detail-page__content">
        <div className="listing-detail-page__main">
          <ListingDetailTitle listing={listing} />
          
          <div className="listing-detail-page__gallery">
            <PhotoGallery photos={photos} fallbackUrl={listing.firstPhotoUrl} />
          </div>

          <ListingDetailDescription listing={listing} />
          <ListingDetailSpecs listing={listing} />
          <ListingDetailFeatures listing={listing} />
        </div>

        <div className="listing-detail-page__sidebar">
          <ListingDetailMap listing={listing} />
          <ListingDetailContact
            isAuthenticated={isAuthenticated}
            loadingOwner={loadingOwner}
            ownerInfo={ownerInfo}
          />
          {!isAdmin && <AskAI listingId={listing.id} />}
          {isAdmin && (
            <ListingDetailAdminPanel
              listing={listing}
              statusLoading={statusLoading}
              onReport={handleReport}
              onUnpublishClick={handleAdminUnpublishClick}
            />
          )}
        </div>
      </div>

      {listing && (
        <>
          <CreateReportModal
            listingId={listing.id}
            listingTitle={listing.title}
            isOpen={showReportModal}
            onClose={() => setShowReportModal(false)}
            onSuccess={() => {}}
          />
          {isAdmin && (
            <AdminUnpublishModal
              isOpen={showAdminUnpublishModal}
              listing={listing}
              reason={adminUnpublishReason}
              onReasonChange={setAdminUnpublishReason}
              onClose={handleAdminUnpublishClose}
              onConfirm={handleAdminUnpublishConfirm}
              loading={adminUnpublishLoading}
            />
          )}
        </>
      )}
    </div>
  );
};

