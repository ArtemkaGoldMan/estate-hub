import { Button } from '../../../../shared/ui';
import { FaBan, FaExclamationTriangle } from 'react-icons/fa';
import type { Listing } from '../../../../entities/listing';
import './ListingDetailAdminPanel.css';

interface ListingDetailAdminPanelProps {
  listing: Listing;
  statusLoading: boolean;
  onReport: () => void;
  onUnpublishClick: () => void;
}

export const ListingDetailAdminPanel = ({
  listing,
  statusLoading,
  onReport,
  onUnpublishClick,
}: ListingDetailAdminPanelProps) => {
  return (
    <div className="listing-detail-admin-panel">
      <div className="listing-detail-admin-panel__header">
        <h2>Admin Management</h2>
      </div>
      <div className="listing-detail-admin-panel__actions">
        <Button
          variant="outline"
          onClick={onReport}
          style={{ width: '100%' }}
        >
          <FaExclamationTriangle style={{ marginRight: '0.5rem' }} /> Report
        </Button>
        {listing.status === 'Published' && (
          <Button
            variant="outline"
            onClick={onUnpublishClick}
            disabled={statusLoading}
            isLoading={statusLoading}
            style={{ width: '100%' }}
          >
            <FaBan style={{ marginRight: '0.5rem' }} /> Unpublish
          </Button>
        )}
      </div>
    </div>
  );
};

