import { useParams, useNavigate, useLocation } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { useAuth } from '../../shared/context/AuthContext';
import { useToast } from '../../shared/context/ToastContext';
import { useListingQuery, usePhotosQuery, useLikeListing } from '../../entities/listing';
import { useChangeListingStatus } from '../../entities/listing/api/publish-listing';
import { useDeleteListing } from '../../entities/listing/api/delete-listing';
import { PhotoGallery } from '../../entities/listing/ui';
import { CreateReportModal } from '../../features/reports/ui/CreateReportModal';
import { Button, LoadingSpinner } from '../../shared/ui';
import { formatCurrency } from '../../shared/lib/formatCurrency';
import { sanitizeHtml } from '../../shared/lib/sanitizeHtml';
import { userApi, type GetUserResponse } from '../../shared/api/auth/userApi';
import { MapContainer, TileLayer, Marker, Popup } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import { Icon } from 'leaflet';
import './ListingDetailPage.css';

// Fix for default marker icon in react-leaflet
// eslint-disable-next-line @typescript-eslint/no-explicit-any
delete (Icon.Default.prototype as any)._getIconUrl;
Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png',
  iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
});

export const ListingDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const { user } = useAuth();
  const { showSuccess, showError } = useToast();
  
  const { listing, loading, error, refetch } = useListingQuery(id || '');
  const { photos } = usePhotosQuery(id || '', true);
  const { toggleLike, loading: likeLoading } = useLikeListing();
  const { publishListing, unpublishListing, loading: statusLoading } = useChangeListingStatus();
  const { deleteListing, loading: deleteLoading } = useDeleteListing();
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [ownerInfo, setOwnerInfo] = useState<GetUserResponse | null>(null);
  const [loadingOwner, setLoadingOwner] = useState(false);
  
  const isOwner = listing && user && listing.ownerId === user.id;
  const isDraft = listing?.status === 'Draft';
  const isPublished = listing?.status === 'Published';
  const isAuthenticated = !!user;
  
  useEffect(() => {
    if (!id) {
      navigate('/listings', { replace: true });
    }
  }, [id, navigate]);

  // Fetch owner information when listing is loaded (only if authenticated)
  useEffect(() => {
    if (!listing?.ownerId || !isAuthenticated) return;

    const fetchOwnerInfo = async () => {
      setLoadingOwner(true);
      try {
        const owner = await userApi.getUser(listing.ownerId);
        setOwnerInfo(owner);
      } catch (error) {
        // Silently fail - owner info is optional
        // For non-authenticated users, we just won't show contact info
        console.error('Failed to fetch owner information:', error);
      } finally {
        setLoadingOwner(false);
      }
    };

    fetchOwnerInfo();
  }, [listing?.ownerId, isAuthenticated]);

  const handleLike = async () => {
    if (!listing) return;
    try {
      await toggleLike(listing.id, listing.isLikedByCurrentUser || false);
    } catch (error) {
      showError(error instanceof Error ? error.message : 'Failed to toggle like');
    }
  };

  const [showReportModal, setShowReportModal] = useState(false);

  const handleReport = () => {
    setShowReportModal(true);
  };

  const handlePublish = async () => {
    if (!listing) return;
    try {
      await publishListing(listing.id);
      await refetch();
      showSuccess('Listing published successfully');
    } catch (error) {
      showError(error instanceof Error ? error.message : 'Failed to publish listing');
    }
  };

  const handleUnpublish = async () => {
    if (!listing) return;
    try {
      await unpublishListing(listing.id);
      await refetch();
      showSuccess('Listing unpublished successfully');
    } catch (error) {
      showError(error instanceof Error ? error.message : 'Failed to unpublish listing');
    }
  };

  const handleDelete = async () => {
    if (!listing || !showDeleteConfirm) {
      setShowDeleteConfirm(true);
      return;
    }

    try {
      await deleteListing(listing.id);
      // Go back to where user came from, or dashboard if from listing detail
      const returnTo = (location.state as { from?: string })?.from || '/dashboard';
      navigate(returnTo, { replace: true });
    } catch (error) {
      showError(error instanceof Error ? error.message : 'Failed to delete listing');
      setShowDeleteConfirm(false);
    }
  };

  const handleBack = () => {
    const state = location.state as { from?: string } | null;
    if (state?.from) {
      navigate(state.from);
    } else {
      // Fallback: try to go back in history, or default to listings
      if (window.history.length > 1) {
        navigate(-1);
      } else {
        navigate('/listings');
      }
    }
  };

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
      <div className="listing-detail-page__header">
        <Button variant="ghost" onClick={handleBack}>
          ← Back
        </Button>
        <div className="listing-detail-page__actions">
          {isOwner && (
            <>
              <Button
                variant="outline"
                onClick={() => navigate(`/listings/${listing.id}/edit`)}
                disabled={statusLoading || deleteLoading}
              >
                ✏️ Edit
              </Button>
              {isDraft && (
                <Button
                  variant="primary"
                  onClick={handlePublish}
                  disabled={statusLoading || deleteLoading}
                  isLoading={statusLoading}
                >
                  📢 Publish
                </Button>
              )}
              {isPublished && (
                <Button
                  variant="outline"
                  onClick={handleUnpublish}
                  disabled={statusLoading || deleteLoading}
                  isLoading={statusLoading}
                >
                  📝 Unpublish
                </Button>
              )}
              {!showDeleteConfirm ? (
                <Button
                  variant="danger"
                  onClick={handleDelete}
                  disabled={statusLoading || deleteLoading}
                >
                  🗑️ Delete
                </Button>
              ) : (
                <>
                  <Button
                    variant="danger"
                    onClick={handleDelete}
                    disabled={statusLoading || deleteLoading}
                    isLoading={deleteLoading}
                  >
                    ✓ Confirm Delete
                  </Button>
                  <Button
                    variant="outline"
                    onClick={() => setShowDeleteConfirm(false)}
                    disabled={statusLoading || deleteLoading}
                  >
                    Cancel
                  </Button>
                </>
              )}
            </>
          )}
          {!isOwner && (
            <>
              <Button
                variant={listing.isLikedByCurrentUser ? 'primary' : 'outline'}
                onClick={handleLike}
                disabled={likeLoading}
                isLoading={likeLoading}
              >
                {listing.isLikedByCurrentUser ? '❤️ Liked' : '🤍 Like'}
              </Button>
              <Button variant="outline" onClick={handleReport}>
                Report
              </Button>
            </>
          )}
        </div>
      </div>

      {isDraft && isOwner && (
        <div className="listing-detail-page__draft-notice">
          <div className="listing-detail-page__draft-notice-content">
            <strong>⚠️ Draft Listing</strong>
            <p>This listing exists but is not visible to other users. Click "Publish" to make it visible to everyone.</p>
          </div>
        </div>
      )}

      <div className="listing-detail-page__content">
        <div className="listing-detail-page__main">
          <div className="listing-detail-page__gallery">
            <PhotoGallery
              photos={photos}
              fallbackUrl={listing.firstPhotoUrl}
              title={listing.title}
            />
          </div>

          <div className="listing-detail-page__info">
            <div className="listing-detail-page__title-section">
              <h1 className="listing-detail-page__title">{listing.title}</h1>
              <div className="listing-detail-page__price">
                {listing.category === 'SALE' && listing.pricePln && (
                  <span className="listing-detail-page__price-main">
                    {formatCurrency(listing.pricePln)}
                  </span>
                )}
                {listing.category === 'RENT' && listing.monthlyRentPln && (
                  <span className="listing-detail-page__price-main">
                    {formatCurrency(listing.monthlyRentPln)}/month
                  </span>
                )}
              </div>
            </div>

            <div className="listing-detail-page__location">
              <span className="listing-detail-page__location-city">{listing.city}</span>
              {listing.district && (
                <>
                  <span className="listing-detail-page__location-separator">,</span>
                  <span className="listing-detail-page__location-district">{listing.district}</span>
                </>
              )}
            </div>

            <div className="listing-detail-page__description">
              <h2>Description</h2>
              <div
                className="listing-detail-page__description-content"
                dangerouslySetInnerHTML={{
                  __html: sanitizeHtml(listing.description || '<p>No description provided.</p>'),
                }}
              />
            </div>

            <div className="listing-detail-page__specs">
              <h2>Property Details</h2>
              <div className="listing-detail-page__specs-grid">
                <div className="listing-detail-page__spec">
                  <span className="listing-detail-page__spec-label">Property Type</span>
                  <span className="listing-detail-page__spec-value">{listing.propertyType}</span>
                </div>
                <div className="listing-detail-page__spec">
                  <span className="listing-detail-page__spec-label">Category</span>
                  <span className="listing-detail-page__spec-value">{listing.category}</span>
                </div>
                <div className="listing-detail-page__spec">
                  <span className="listing-detail-page__spec-label">Square Meters</span>
                  <span className="listing-detail-page__spec-value">{listing.squareMeters} m²</span>
                </div>
                <div className="listing-detail-page__spec">
                  <span className="listing-detail-page__spec-label">Rooms</span>
                  <span className="listing-detail-page__spec-value">{listing.rooms}</span>
                </div>
                {listing.floor !== null && listing.floor !== undefined && (
                  <div className="listing-detail-page__spec">
                    <span className="listing-detail-page__spec-label">Floor</span>
                    <span className="listing-detail-page__spec-value">
                      {listing.floor}
                      {listing.floorCount && ` / ${listing.floorCount}`}
                    </span>
                  </div>
                )}
                {listing.buildYear && (
                  <div className="listing-detail-page__spec">
                    <span className="listing-detail-page__spec-label">Build Year</span>
                    <span className="listing-detail-page__spec-value">{listing.buildYear}</span>
                  </div>
                )}
                <div className="listing-detail-page__spec">
                  <span className="listing-detail-page__spec-label">Condition</span>
                  <span className="listing-detail-page__spec-value">{listing.condition}</span>
                </div>
              </div>
            </div>

            <div className="listing-detail-page__features">
              <h2>Features</h2>
              <div className="listing-detail-page__features-list">
                {listing.hasBalcony && (
                  <span className="listing-detail-page__feature">Balcony</span>
                )}
                {listing.hasElevator && (
                  <span className="listing-detail-page__feature">Elevator</span>
                )}
                {listing.hasParkingSpace && (
                  <span className="listing-detail-page__feature">Parking</span>
                )}
                {listing.hasSecurity && (
                  <span className="listing-detail-page__feature">Security</span>
                )}
                {listing.hasStorageRoom && (
                  <span className="listing-detail-page__feature">Storage Room</span>
                )}
                {!listing.hasBalcony &&
                  !listing.hasElevator &&
                  !listing.hasParkingSpace &&
                  !listing.hasSecurity &&
                  !listing.hasStorageRoom && (
                    <span className="listing-detail-page__feature--none">No features listed</span>
                  )}
              </div>
            </div>
          </div>
        </div>

        <div className="listing-detail-page__sidebar">
          <div className="listing-detail-page__map-section">
            <h2>Location</h2>
            <div className="listing-detail-page__map">
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
                    {listing.city}, {listing.district}
                  </Popup>
                </Marker>
              </MapContainer>
            </div>
            <div className="listing-detail-page__map-address">
              <p>
                <strong>{listing.city}</strong>
                {listing.district && `, ${listing.district}`}
              </p>
            </div>
          </div>

          {/* Contact Information Section */}
          <div className="listing-detail-page__contact-section">
            <h2>Contact Information</h2>
            {!isAuthenticated ? (
              <div className="listing-detail-page__contact-login-prompt">
                <p>Please <a href="/login">log in</a> to view contact information</p>
              </div>
            ) : loadingOwner ? (
              <div className="listing-detail-page__contact-loading">
                <LoadingSpinner text="Loading contact info..." />
              </div>
            ) : ownerInfo ? (
              <div className="listing-detail-page__contact-info">
                <div className="listing-detail-page__contact-item">
                  <span className="listing-detail-page__contact-label">Name:</span>
                  <span className="listing-detail-page__contact-value">
                    {ownerInfo.displayName || ownerInfo.userName}
                  </span>
                </div>
                {ownerInfo.phoneNumber ? (
                  <div className="listing-detail-page__contact-item">
                    <span className="listing-detail-page__contact-label">Phone:</span>
                    <span className="listing-detail-page__contact-value">
                      <a href={`tel:${ownerInfo.phoneNumber}`}>{ownerInfo.phoneNumber}</a>
                    </span>
                  </div>
                ) : (
                  <div className="listing-detail-page__contact-item">
                    <span className="listing-detail-page__contact-label">Phone:</span>
                    <span className="listing-detail-page__contact-value" style={{ color: '#999', fontStyle: 'italic' }}>
                      Not provided
                    </span>
                  </div>
                )}
                {ownerInfo.email ? (
                  <div className="listing-detail-page__contact-item">
                    <span className="listing-detail-page__contact-label">Email:</span>
                    <span className="listing-detail-page__contact-value">
                      <a href={`mailto:${ownerInfo.email}`}>{ownerInfo.email}</a>
                    </span>
                  </div>
                ) : (
                  <div className="listing-detail-page__contact-item">
                    <span className="listing-detail-page__contact-label">Email:</span>
                    <span className="listing-detail-page__contact-value" style={{ color: '#999', fontStyle: 'italic' }}>
                      Not provided
                    </span>
                  </div>
                )}
              </div>
            ) : (
              <div className="listing-detail-page__contact-error">
                <p>Contact information not available</p>
              </div>
            )}
          </div>
        </div>
      </div>

      {listing && (
        <CreateReportModal
          listingId={listing.id}
          listingTitle={listing.title}
          isOpen={showReportModal}
          onClose={() => setShowReportModal(false)}
          onSuccess={() => {
            // Optionally show success message
          }}
        />
      )}
    </div>
  );
};

