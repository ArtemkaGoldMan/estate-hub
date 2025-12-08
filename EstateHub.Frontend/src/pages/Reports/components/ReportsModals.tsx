import { Button } from '../../../shared';
import type { Report } from '../../../entities/report';

interface ResolveModalProps {
  isOpen: boolean;
  report: Report | null;
  resolution: string;
  moderatorNotes: string;
  unpublishListing: boolean;
  unpublishReason: string;
  onResolutionChange: (value: string) => void;
  onModeratorNotesChange: (value: string) => void;
  onUnpublishToggle: (value: boolean) => void;
  onUnpublishReasonChange: (value: string) => void;
  onClose: () => void;
  onConfirm: () => void;
}

export const ResolveModal = ({
  isOpen,
  report,
  resolution,
  moderatorNotes,
  unpublishListing,
  unpublishReason,
  onResolutionChange,
  onModeratorNotesChange,
  onUnpublishToggle,
  onUnpublishReasonChange,
  onClose,
  onConfirm,
}: ResolveModalProps) => {
  if (!isOpen || !report) return null;

  return (
    <div className="reports-page__modal-overlay" onClick={onClose}>
      <div className="reports-page__modal" onClick={(e) => e.stopPropagation()}>
        <h2>Resolve Report</h2>
        <div className="reports-page__modal-content">
          <label>
            Resolution *
            <textarea
              value={resolution}
              onChange={(e) => onResolutionChange(e.target.value)}
              placeholder="Describe the resolution..."
              rows={4}
              required
            />
          </label>
          <label>
            Moderator Notes (optional)
            <textarea
              value={moderatorNotes}
              onChange={(e) => onModeratorNotesChange(e.target.value)}
              placeholder="Internal notes..."
              rows={3}
            />
          </label>
          <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <input
              type="checkbox"
              checked={unpublishListing}
              onChange={(e) => {
                onUnpublishToggle(e.target.checked);
                if (!e.target.checked) {
                  onUnpublishReasonChange('');
                }
              }}
            />
            <span>Unpublish listing</span>
          </label>
          {unpublishListing && (
            <label>
              Unpublish Reason *
              <textarea
                value={unpublishReason}
                onChange={(e) => onUnpublishReasonChange(e.target.value)}
                placeholder="Explain why the listing is being unpublished (this will be visible to the listing owner)..."
                rows={4}
                required
              />
            </label>
          )}
        </div>
        <div className="reports-page__modal-actions">
          <Button variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button variant="primary" onClick={onConfirm} disabled={!resolution.trim()}>
            Resolve
          </Button>
        </div>
      </div>
    </div>
  );
};

interface DismissModalProps {
  isOpen: boolean;
  report: Report | null;
  moderatorNotes: string;
  onModeratorNotesChange: (value: string) => void;
  onClose: () => void;
  onConfirm: () => void;
}

export const DismissModal = ({
  isOpen,
  report,
  moderatorNotes,
  onModeratorNotesChange,
  onClose,
  onConfirm,
}: DismissModalProps) => {
  if (!isOpen || !report) return null;

  return (
    <div className="reports-page__modal-overlay" onClick={onClose}>
      <div className="reports-page__modal" onClick={(e) => e.stopPropagation()}>
        <h2>Dismiss Report</h2>
        <div className="reports-page__modal-content">
          <label>
            Moderator Notes (optional)
            <textarea
              value={moderatorNotes}
              onChange={(e) => onModeratorNotesChange(e.target.value)}
              placeholder="Internal notes..."
              rows={3}
            />
          </label>
        </div>
        <div className="reports-page__modal-actions">
          <Button variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button variant="primary" onClick={onConfirm}>
            Dismiss
          </Button>
        </div>
      </div>
    </div>
  );
};

