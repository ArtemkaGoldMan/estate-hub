import { FaExclamationTriangle } from 'react-icons/fa';
import './ListingDetailDraftNotice.css';

export const ListingDetailDraftNotice = () => {
  return (
    <div className="listing-detail-draft-notice">
      <div className="listing-detail-draft-notice__content">
        <strong>
          <FaExclamationTriangle style={{ marginRight: '0.5rem' }} /> Draft Listing
        </strong>
        <p>This listing exists but is not visible to other users. Click "Publish" to make it visible to everyone.</p>
      </div>
    </div>
  );
};



