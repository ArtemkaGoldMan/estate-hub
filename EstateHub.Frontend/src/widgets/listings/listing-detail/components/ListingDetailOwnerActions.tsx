import { Button } from '../../../../shared/ui';
import { FaEdit, FaTrash, FaBullhorn, FaFileAlt, FaArchive } from 'react-icons/fa';
import type { Listing } from '../../../../entities/listing';
import './ListingDetailOwnerActions.css';

interface ListingDetailOwnerActionsProps {
  listing: Listing;
  statusLoading: boolean;
  deleteLoading: boolean;
  showDeleteConfirm: boolean;
  canPublish: boolean;
  onEdit: () => void;
  onPublish: () => void;
  onUnpublish: () => void;
  onArchive: () => void;
  onUnarchive: () => void;
  onDelete: () => void;
}

export const ListingDetailOwnerActions = ({
  listing,
  statusLoading,
  deleteLoading,
  showDeleteConfirm,
  canPublish,
  onEdit,
  onPublish,
  onUnpublish,
  onArchive,
  onUnarchive,
  onDelete,
}: ListingDetailOwnerActionsProps) => {
  const isDraft = listing.status === 'Draft';
  const isPublished = listing.status === 'Published';
  const isArchived = listing.status === 'Archived';

  return (
    <div className="listing-detail-owner-actions">
      <h2>Manage Listing</h2>
      <div className="listing-detail-owner-actions__buttons">
        <Button
          variant="outline"
          onClick={onEdit}
          style={{ width: '100%' }}
        >
          <FaEdit style={{ marginRight: '0.5rem' }} /> Edit
        </Button>

        {isDraft && canPublish && (
          <Button
            variant="primary"
            onClick={onPublish}
            disabled={statusLoading}
            isLoading={statusLoading}
            style={{ width: '100%' }}
          >
            <FaBullhorn style={{ marginRight: '0.5rem' }} /> Publish
          </Button>
        )}

        {isPublished && (
          <Button
            variant="outline"
            onClick={onUnpublish}
            disabled={statusLoading}
            isLoading={statusLoading}
            style={{ width: '100%' }}
          >
            <FaFileAlt style={{ marginRight: '0.5rem' }} /> Unpublish
          </Button>
        )}

        {!isArchived && (
          <Button
            variant="outline"
            onClick={onArchive}
            disabled={statusLoading}
            isLoading={statusLoading}
            style={{ width: '100%' }}
          >
            <FaArchive style={{ marginRight: '0.5rem' }} /> Archive
          </Button>
        )}

        {isArchived && (
          <Button
            variant="outline"
            onClick={onUnarchive}
            disabled={statusLoading}
            isLoading={statusLoading}
            style={{ width: '100%' }}
          >
            <FaArchive style={{ marginRight: '0.5rem' }} /> Unarchive
          </Button>
        )}

        <Button
          variant="outline"
          onClick={onDelete}
          disabled={deleteLoading}
          isLoading={deleteLoading && showDeleteConfirm}
          style={{ width: '100%', color: '#ef4444' }}
        >
          <FaTrash style={{ marginRight: '0.5rem' }} /> {showDeleteConfirm ? 'Confirm Delete' : 'Delete'}
        </Button>
      </div>
    </div>
  );
};



