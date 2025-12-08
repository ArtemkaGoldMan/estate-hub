import { FaCheckCircle, FaTimesCircle, FaClock, FaInfoCircle } from 'react-icons/fa';
import type { Listing } from '../../../../entities/listing';
import './ListingDetailModerationStatus.css';

interface ListingDetailModerationStatusProps {
  listing: Listing;
}

export const ListingDetailModerationStatus = ({ listing }: ListingDetailModerationStatusProps) => {
  const isDraft = listing.status === 'Draft';
  
  if (!isDraft) return null;

  const moderationStatus = listing.isModerationApproved === null
    ? 'pending'
    : listing.isModerationApproved === true
    ? 'approved'
    : 'rejected';

  const canPublish = listing.isModerationApproved === true && !listing.adminUnpublishReason;

  const statusConfig = {
    pending: {
      icon: <FaClock />,
      text: 'Pending Moderation',
      description: 'Your listing is waiting for moderation review. You will be notified once it\'s reviewed.',
      className: 'listing-detail-moderation-status--pending',
    },
    approved: {
      icon: <FaCheckCircle />,
      text: 'Moderation Approved',
      description: canPublish
        ? 'Your listing has been approved! You can now publish it to make it visible to everyone.'
        : 'Your listing was approved but cannot be published due to admin restrictions. Please make changes and resubmit.',
      className: canPublish
        ? 'listing-detail-moderation-status--approved'
        : 'listing-detail-moderation-status--approved-restricted',
    },
    rejected: {
      icon: <FaTimesCircle />,
      text: 'Moderation Rejected',
      description: listing.moderationRejectionReason
        ? `Reason: ${listing.moderationRejectionReason}`
        : 'Your listing did not pass moderation. Please review and make necessary changes.',
      className: 'listing-detail-moderation-status--rejected',
    },
  };

  const config = statusConfig[moderationStatus];

  return (
    <div className={`listing-detail-moderation-status ${config.className}`}>
      <div className="listing-detail-moderation-status__icon">
        {config.icon}
      </div>
      <div className="listing-detail-moderation-status__content">
        <strong>{config.text}</strong>
        <p>{config.description}</p>
        {listing.adminUnpublishReason && (
          <div className="listing-detail-moderation-status__admin-note">
            <FaInfoCircle style={{ marginRight: '0.5rem' }} />
            <strong>Admin Note:</strong> {listing.adminUnpublishReason}
          </div>
        )}
      </div>
    </div>
  );
};



