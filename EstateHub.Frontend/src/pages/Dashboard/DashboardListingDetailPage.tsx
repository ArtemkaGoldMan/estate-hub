import { useParams, useNavigate } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { useAuth } from '../../shared/context/AuthContext';
import { useToast } from '../../shared/context/ToastContext';
import { useListingQuery, usePhotosQuery } from '../../entities/listing';
import { useChangeListingStatus } from '../../entities/listing/api/publish-listing';
import { useDeleteListing } from '../../entities/listing/api/delete-listing';
import { PhotoGallery } from '../../entities/listing/ui';
import { Button, LoadingSpinner } from '../../shared/ui';
import { formatCurrency } from '../../shared/lib/formatCurrency';
import { sanitizeHtml } from '../../shared/lib/sanitizeHtml';
import { userApi, type GetUserResponse } from '../../shared/api/auth/userApi';
import { MapContainer, TileLayer, Marker, Popup } from 'react-leaflet';
import { FaExclamationTriangle, FaCheckCircle, FaTimesCircle, FaClock, FaInfoCircle, FaEdit, FaTrash, FaBullhorn, FaFileAlt, FaArchive } from 'react-icons/fa';
import 'leaflet/dist/leaflet.css';
import { Icon } from 'leaflet';
import './DashboardListingDetailPage.css';

// Fix for default marker icon in react-leaflet
// eslint-disable-next-line @typescript-eslint/no-explicit-any
delete (Icon.Default.prototype as any)._getIconUrl;
Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png',
  iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
});

export const DashboardListingDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const { showSuccess, showError } = useToast();
  
  // Enable polling when moderation is pending (will be determined after listing loads)
  const { listing, loading, error, refetch } = useListingQuery(id || '', {
    enablePolling: true, // Enable polling - it will automatically start/stop based on moderation status
    pollInterval: 10000, // Poll every 10 seconds
  });
  const { photos } = usePhotosQuery(id || '', true);
  const { publishListing, unpublishListing, unarchiveListing, archiveListing, loading: statusLoading } = useChangeListingStatus();
  const { deleteListing, loading: deleteLoading } = useDeleteListing();
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [ownerInfo, setOwnerInfo] = useState<GetUserResponse | null>(null);
  const [loadingOwner, setLoadingOwner] = useState(false);
  
  const isOwner = listing && user && listing.ownerId === user.id;
  const isDraft = listing?.status === 'Draft';
  const isPublished = listing?.status === 'Published';
  const isArchived = listing?.status === 'Archived';
  
  // Moderation status helpers
  const moderationStatus = isDraft
    ? listing?.isModerationApproved === null
      ? 'pending'
      : listing.isModerationApproved === true
      ? 'approved'
      : 'rejected'
    : null;
  
  useEffect(() => {
    if (!id) {
      navigate('/dashboard', { replace: true });
    }
  }, [id, navigate]);

  useEffect(() => {
    // Redirect if not owner
    if (listing && user && listing.ownerId !== user.id) {
      navigate('/dashboard', { replace: true });
    }
  }, [listing, user, navigate]);

  // Fetch owner information when listing is loaded
  useEffect(() => {
    if (!listing?.ownerId || !user) return;

    const fetchOwnerInfo = async () => {
      setLoadingOwner(true);
      try {
        const owner = await userApi.getUser(listing.ownerId);
        setOwnerInfo(owner);
      } catch (error) {
        console.error('Failed to fetch owner information:', error);
      } finally {
        setLoadingOwner(false);
      }
    };

    fetchOwnerInfo();
  }, [listing?.ownerId, user]);

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

  const handleUnarchive = async () => {
    if (!listing) return;
    if (!confirm('Are you sure you want to unarchive this listing? It will be moved to draft status.')) {
      return;
    }
    try {
      await unarchiveListing(listing.id);
      await refetch();
      showSuccess('Listing unarchived successfully. It is now in draft status.');
    } catch (error) {
      showError(error instanceof Error ? error.message : 'Failed to unarchive listing');
    }
  };

  const handleArchive = async () => {
    if (!listing) return;
    if (!confirm('Are you sure you want to archive this listing? It will be hidden from public view.')) {
      return;
    }
    try {
      await archiveListing(listing.id);
      await refetch();
      showSuccess('Listing archived successfully');
    } catch (error) {
      showError(error instanceof Error ? error.message : 'Failed to archive listing');
    }
  };

  const handleDelete = async () => {
    if (!listing || !showDeleteConfirm) {
      setShowDeleteConfirm(true);
      return;
    }

    try {
      await deleteListing(listing.id);
      navigate('/dashboard', { replace: true });
    } catch (error) {
      showError(error instanceof Error ? error.message : 'Failed to delete listing');
      setShowDeleteConfirm(false);
    }
  };

  const handleBack = () => {
    navigate('/dashboard');
  };

  const handleEdit = () => {
    navigate(`/listings/${listing?.id}/edit`);
  };

  // Cannot publish if admin unpublished (must make changes and re-moderate first)
  const canPublish = isDraft && listing?.isModerationApproved === true && !listing?.adminUnpublishReason;

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
      {/* Header - Only back button */}
      <div className="dashboard-listing-detail-page__header">
        <Button variant="ghost" onClick={handleBack}>
          ← Back to Dashboard
        </Button>
      </div>

      {/* Draft Notice */}
      {isDraft && (
        <div className="dashboard-listing-detail-page__draft-notice">
          <div className="dashboard-listing-detail-page__draft-notice-content">
            <strong><FaExclamationTriangle style={{ marginRight: '0.5rem' }} /> Draft Listing</strong>
            <p>This listing exists but is not visible to other users. Click "Publish" to make it visible to everyone.</p>
          </div>
        </div>
      )}

      {/* Main Content - Two Column Layout */}
      <div className="dashboard-listing-detail-page__content">
        {/* Left Column - Listing Information */}
        <div className="dashboard-listing-detail-page__main-content">
          {/* Gallery */}
          <div className="dashboard-listing-detail-page__gallery">
            <PhotoGallery
              photos={photos}
              fallbackUrl={listing.firstPhotoUrl}
              title={listing.title}
            />
          </div>

          {/* Title and Price */}
          <div className="dashboard-listing-detail-page__title-section">
            <h1 className="dashboard-listing-detail-page__title">{listing.title}</h1>
            <div className="dashboard-listing-detail-page__price">
              {listing.category === 'SALE' && listing.pricePln && (
                <span className="dashboard-listing-detail-page__price-main">
                  {formatCurrency(listing.pricePln)}
                </span>
              )}
              {listing.category === 'RENT' && listing.monthlyRentPln && (
                <span className="dashboard-listing-detail-page__price-main">
                  {formatCurrency(listing.monthlyRentPln)}/month
                </span>
              )}
            </div>
          </div>

          {/* Location */}
          <div className="dashboard-listing-detail-page__location">
            <span className="dashboard-listing-detail-page__location-city">{listing.city}</span>
            {listing.district && (
              <>
                <span className="dashboard-listing-detail-page__location-separator">,</span>
                <span className="dashboard-listing-detail-page__location-district">{listing.district}</span>
              </>
            )}
          </div>

          {/* Description */}
          <div className="dashboard-listing-detail-page__description">
            <h2>Description</h2>
            <div
              className="dashboard-listing-detail-page__description-content"
              dangerouslySetInnerHTML={{
                __html: sanitizeHtml(listing.description || '<p>No description provided.</p>'),
              }}
            />
          </div>

          {/* Property Details */}
          <div className="dashboard-listing-detail-page__specs">
            <h2>Property Details</h2>
            <div className="dashboard-listing-detail-page__specs-grid">
              <div className="dashboard-listing-detail-page__spec">
                <span className="dashboard-listing-detail-page__spec-label">Property Type</span>
                <span className="dashboard-listing-detail-page__spec-value">{listing.propertyType}</span>
              </div>
              <div className="dashboard-listing-detail-page__spec">
                <span className="dashboard-listing-detail-page__spec-label">Category</span>
                <span className="dashboard-listing-detail-page__spec-value">{listing.category}</span>
              </div>
              <div className="dashboard-listing-detail-page__spec">
                <span className="dashboard-listing-detail-page__spec-label">Square Meters</span>
                <span className="dashboard-listing-detail-page__spec-value">{listing.squareMeters} m²</span>
              </div>
              <div className="dashboard-listing-detail-page__spec">
                <span className="dashboard-listing-detail-page__spec-label">Rooms</span>
                <span className="dashboard-listing-detail-page__spec-value">{listing.rooms}</span>
              </div>
              {listing.floor !== null && listing.floor !== undefined && (
                <div className="dashboard-listing-detail-page__spec">
                  <span className="dashboard-listing-detail-page__spec-label">Floor</span>
                  <span className="dashboard-listing-detail-page__spec-value">
                    {listing.floor}
                    {listing.floorCount && ` / ${listing.floorCount}`}
                  </span>
                </div>
              )}
              {listing.buildYear && (
                <div className="dashboard-listing-detail-page__spec">
                  <span className="dashboard-listing-detail-page__spec-label">Build Year</span>
                  <span className="dashboard-listing-detail-page__spec-value">{listing.buildYear}</span>
                </div>
              )}
              <div className="dashboard-listing-detail-page__spec">
                <span className="dashboard-listing-detail-page__spec-label">Condition</span>
                <span className="dashboard-listing-detail-page__spec-value">{listing.condition}</span>
              </div>
            </div>
          </div>

          {/* Features */}
          <div className="dashboard-listing-detail-page__features">
            <h2>Features</h2>
            <div className="dashboard-listing-detail-page__features-list">
              {listing.hasBalcony && (
                <span className="dashboard-listing-detail-page__feature">Balcony</span>
              )}
              {listing.hasElevator && (
                <span className="dashboard-listing-detail-page__feature">Elevator</span>
              )}
              {listing.hasParkingSpace && (
                <span className="dashboard-listing-detail-page__feature">Parking</span>
              )}
              {listing.hasSecurity && (
                <span className="dashboard-listing-detail-page__feature">Security</span>
              )}
              {listing.hasStorageRoom && (
                <span className="dashboard-listing-detail-page__feature">Storage Room</span>
              )}
              {!listing.hasBalcony &&
                !listing.hasElevator &&
                !listing.hasParkingSpace &&
                !listing.hasSecurity &&
                !listing.hasStorageRoom && (
                  <span className="dashboard-listing-detail-page__feature--none">No features listed</span>
                )}
            </div>
          </div>

          {/* Map Section */}
          <div className="dashboard-listing-detail-page__map-section">
            <h2>Location</h2>
            <div className="dashboard-listing-detail-page__map">
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
            <div className="dashboard-listing-detail-page__map-address">
              <p>
                <strong>{listing.city}</strong>
                {listing.district && `, ${listing.district}`}
              </p>
            </div>
          </div>

          {/* Contact Information Section */}
          <div className="dashboard-listing-detail-page__contact-section">
            <h2>Contact Information</h2>
            {loadingOwner ? (
              <div className="dashboard-listing-detail-page__contact-loading">
                <LoadingSpinner text="Loading contact info..." />
              </div>
            ) : ownerInfo ? (
              <div className="dashboard-listing-detail-page__contact-info">
                <div className="dashboard-listing-detail-page__contact-item">
                  <span className="dashboard-listing-detail-page__contact-label">Name:</span>
                  <span className="dashboard-listing-detail-page__contact-value">
                    {ownerInfo.displayName || ownerInfo.userName}
                  </span>
                </div>
                {ownerInfo.phoneNumber ? (
                  <div className="dashboard-listing-detail-page__contact-item">
                    <span className="dashboard-listing-detail-page__contact-label">Phone:</span>
                    <span className="dashboard-listing-detail-page__contact-value">
                      <a href={`tel:${ownerInfo.phoneNumber}`}>{ownerInfo.phoneNumber}</a>
                    </span>
                  </div>
                ) : (
                  <div className="dashboard-listing-detail-page__contact-item">
                    <span className="dashboard-listing-detail-page__contact-label">Phone:</span>
                    <span className="dashboard-listing-detail-page__contact-value" style={{ color: '#999', fontStyle: 'italic' }}>
                      Not provided
                    </span>
                  </div>
                )}
                {ownerInfo.email ? (
                  <div className="dashboard-listing-detail-page__contact-item">
                    <span className="dashboard-listing-detail-page__contact-label">Email:</span>
                    <span className="dashboard-listing-detail-page__contact-value">
                      <a href={`mailto:${ownerInfo.email}`}>{ownerInfo.email}</a>
                    </span>
                  </div>
                ) : (
                  <div className="dashboard-listing-detail-page__contact-item">
                    <span className="dashboard-listing-detail-page__contact-label">Email:</span>
                    <span className="dashboard-listing-detail-page__contact-value" style={{ color: '#999', fontStyle: 'italic' }}>
                      Not provided
                    </span>
                  </div>
                )}
              </div>
            ) : (
              <div className="dashboard-listing-detail-page__contact-error">
                <p>Contact information not available</p>
              </div>
            )}
          </div>
        </div>

        {/* Right Column - Management Panel */}
        <div className="dashboard-listing-detail-page__management-panel">
          {/* Admin Unpublish Section - Show FIRST when listing was unpublished by admin */}
          {listing.adminUnpublishReason && (
            <div className="dashboard-listing-detail-page__management-section">
              <h2><FaExclamationTriangle style={{ marginRight: '0.5rem' }} /> Admin Action Required</h2>
              <div className="dashboard-listing-detail-page__moderation-review--rejected">
                <div className="dashboard-listing-detail-page__moderation-status-icon"><FaExclamationTriangle /></div>
                <div className="dashboard-listing-detail-page__moderation-status-text">
                  <strong>Listing Unpublished by Administrator</strong>
                  <div className="dashboard-listing-detail-page__moderation-reason">
                    <p><strong>Reason:</strong></p>
                    <p>{listing.adminUnpublishReason}</p>
                  </div>
                  <div className="dashboard-listing-detail-page__moderation-message">
                    <p><strong>Your listing has been unpublished due to a report. Please review the reason above, make necessary changes, and the listing will need to be re-moderated before it can be republished.</strong></p>
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* Moderation Review Section - Only for draft listings, hide if admin unpublished */}
          {isDraft && !listing?.adminUnpublishReason && (
            <div className="dashboard-listing-detail-page__management-section">
              <h2>Moderator Review</h2>
              <div className="dashboard-listing-detail-page__moderation-review">
                {moderationStatus === 'approved' && (
                  <div className="dashboard-listing-detail-page__moderation-review--approved">
                    <div className="dashboard-listing-detail-page__moderation-status-icon"><FaCheckCircle /></div>
                    <div className="dashboard-listing-detail-page__moderation-status-text">
                      <strong>Status: OK</strong>
                      <p>Your listing has been approved by moderation. You can publish it now.</p>
                    </div>
                  </div>
                )}
                
                {moderationStatus === 'rejected' && (
                  <div className="dashboard-listing-detail-page__moderation-review--rejected">
                    <div className="dashboard-listing-detail-page__moderation-status-icon"><FaTimesCircle /></div>
                    <div className="dashboard-listing-detail-page__moderation-status-text">
                      <strong>Status: Rejected</strong>
                      {listing.moderationRejectionReason && (
                        <div className="dashboard-listing-detail-page__moderation-reason">
                          <p><strong>Reason:</strong></p>
                          <p>{listing.moderationRejectionReason}</p>
                        </div>
                      )}
                      {listing.moderationRejectionReason?.includes('currently unavailable') ? (
                        <div className="dashboard-listing-detail-page__moderation-message">
                          <p><strong><FaExclamationTriangle style={{ marginRight: '0.5rem' }} /> Moderation service is temporarily unavailable. Please try again later.</strong></p>
                          <p>The system will automatically retry moderation when you update your listing.</p>
                        </div>
                      ) : (
                        <div className="dashboard-listing-detail-page__moderation-message">
                          <p><strong>Change your listing info to test one more time on moderator</strong></p>
                        </div>
                      )}
                    </div>
                  </div>
                )}
                
                {moderationStatus === 'pending' && (
                  <div className="dashboard-listing-detail-page__moderation-review--pending">
                    <div className="dashboard-listing-detail-page__moderation-status-icon"><FaClock /></div>
                    <div className="dashboard-listing-detail-page__moderation-status-text">
                      <strong>Status: Pending</strong>
                      <p>Moderation is being checked. Please wait...</p>
                    </div>
                  </div>
                )}
                
                {moderationStatus === null && (
                  <div className="dashboard-listing-detail-page__moderation-review--not-checked">
                    <div className="dashboard-listing-detail-page__moderation-status-icon"><FaInfoCircle /></div>
                    <div className="dashboard-listing-detail-page__moderation-status-text">
                      <strong>Status: Not checked yet</strong>
                      <p>Moderation will be checked automatically.</p>
                    </div>
                  </div>
                )}
              </div>
            </div>
          )}

          {/* Action Buttons */}
          <div className="dashboard-listing-detail-page__management-section">
            <h2>Management</h2>
            <div className="dashboard-listing-detail-page__management-actions">
              {isArchived ? (
                // Archive mode: only show Unarchive button
                <Button
                  variant="outline"
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
                    onClick={handleEdit}
                    disabled={statusLoading || deleteLoading}
                    style={{ width: '100%' }}
                  >
                    <FaEdit style={{ marginRight: '0.5rem' }} /> Edit
                  </Button>
                  
                  {isDraft && (
                    <Button
                      variant="primary"
                      onClick={handlePublish}
                      disabled={statusLoading || deleteLoading || !canPublish}
                      isLoading={statusLoading}
                      style={{ width: '100%' }}
                      title={
                        listing?.adminUnpublishReason
                          ? 'Listing was unpublished by admin. Make changes and re-moderate before publishing.'
                          : !canPublish
                          ? 'Content must pass moderation before publishing'
                          : 'Publish listing'
                      }
                    >
                      <FaBullhorn style={{ marginRight: '0.5rem' }} /> Publish
                    </Button>
                  )}
                  
                  {isPublished && (
                    <Button
                      variant="outline"
                      onClick={handleUnpublish}
                      disabled={statusLoading || deleteLoading}
                      isLoading={statusLoading}
                      style={{ width: '100%' }}
                    >
                      <FaFileAlt style={{ marginRight: '0.5rem' }} /> Unpublish
                    </Button>
                  )}
                  
                  {!isArchived && (
                    <Button
                      variant="outline"
                      onClick={handleArchive}
                      disabled={statusLoading || deleteLoading}
                      isLoading={statusLoading}
                      style={{ width: '100%' }}
                    >
                      <FaArchive style={{ marginRight: '0.5rem' }} /> Archive
                    </Button>
                  )}
                  
                  {!showDeleteConfirm ? (
                    <Button
                      variant="danger"
                      onClick={handleDelete}
                      disabled={statusLoading || deleteLoading}
                      style={{ width: '100%' }}
                    >
                      <FaTrash style={{ marginRight: '0.5rem' }} /> Delete
                    </Button>
                  ) : (
                    <>
                      <Button
                        variant="danger"
                        onClick={handleDelete}
                        disabled={statusLoading || deleteLoading}
                        isLoading={deleteLoading}
                        style={{ width: '100%' }}
                      >
                        ✓ Confirm Delete
                      </Button>
                      <Button
                        variant="outline"
                        onClick={() => setShowDeleteConfirm(false)}
                        disabled={statusLoading || deleteLoading}
                        style={{ width: '100%' }}
                      >
                        Cancel
                      </Button>
                    </>
                  )}
                </>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

