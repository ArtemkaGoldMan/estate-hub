import { Button } from '../../../../shared/ui';
import type { Listing } from '../../../../entities/listing';
import './AdminUnpublishModal.css';

interface AdminUnpublishModalProps {
  isOpen: boolean;
  listing: Listing | null;
  reason: string;
  onReasonChange: (value: string) => void;
  onClose: () => void;
  onConfirm: () => void;
  loading?: boolean;
}

export const AdminUnpublishModal = ({
  isOpen,
  listing,
  reason,
  onReasonChange,
  onClose,
  onConfirm,
  loading = false,
}: AdminUnpublishModalProps) => {
  if (!isOpen || !listing) return null;

  return (
    <div className="admin-unpublish-modal__overlay" onClick={onClose}>
      <div className="admin-unpublish-modal" onClick={(e) => e.stopPropagation()}>
        <h2>Unpublish Listing</h2>
        <div className="admin-unpublish-modal__content">
          <p className="admin-unpublish-modal__info">
            You are about to unpublish the listing <strong>"{listing.title}"</strong>.
            Please provide a reason that will be visible to the listing owner.
          </p>
          <label>
            Unpublish Reason *
            <textarea
              value={reason}
              onChange={(e) => onReasonChange(e.target.value)}
              placeholder="Explain why the listing is being unpublished (this will be visible to the listing owner)..."
              rows={6}
              required
              disabled={loading}
            />
          </label>
        </div>
        <div className="admin-unpublish-modal__actions">
          <Button variant="outline" onClick={onClose} disabled={loading}>
            Cancel
          </Button>
          <Button 
            variant="primary" 
            onClick={onConfirm} 
            disabled={!reason.trim() || loading}
            isLoading={loading}
          >
            Unpublish
          </Button>
        </div>
      </div>
    </div>
  );
};

