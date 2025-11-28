import { useState } from 'react';
import { useCreateReport } from '../../../../entities/report';
import type { ReportReason } from '../../../../entities/report/model/types';
import { REPORT_REASON_LABELS } from '../../../../entities/report/model/types';
import { Button } from '../../../../shared/ui';
import './CreateReportModal.css';

interface CreateReportModalProps {
  listingId: string;
  listingTitle: string;
  isOpen: boolean;
  onClose: () => void;
  onSuccess?: () => void;
}

export const CreateReportModal = ({
  listingId,
  listingTitle,
  isOpen,
  onClose,
  onSuccess,
}: CreateReportModalProps) => {
  const [reason, setReason] = useState<ReportReason>('OTHER');
  const [description, setDescription] = useState('');
  const { createReport, loading, error } = useCreateReport();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!description.trim()) {
      return;
    }

    try {
      await createReport({
        listingId,
        reason,
        description: description.trim(),
      });
      setDescription('');
      setReason('OTHER');
      onSuccess?.();
      onClose();
    } catch (err) {
      // Error is handled by the hook
    }
  };

  if (!isOpen) {
    return null;
  }

  return (
    <div className="create-report-modal__overlay" onClick={onClose}>
      <div className="create-report-modal" onClick={(e) => e.stopPropagation()}>
        <div className="create-report-modal__header">
          <h2>Report Listing</h2>
          <button className="create-report-modal__close" onClick={onClose}>
            ×
          </button>
        </div>

        <form onSubmit={handleSubmit} className="create-report-modal__form">
          <div className="create-report-modal__info">
            <p>
              <strong>Listing:</strong> {listingTitle}
            </p>
          </div>

          <div className="create-report-modal__field">
            <label htmlFor="reason">
              Reason <span className="required">*</span>
            </label>
            <select
              id="reason"
              value={reason}
              onChange={(e) => setReason(e.target.value as ReportReason)}
              required
            >
              {Object.entries(REPORT_REASON_LABELS).map(([value, label]) => (
                <option key={value} value={value}>
                  {label}
                </option>
              ))}
            </select>
          </div>

          <div className="create-report-modal__field">
            <label htmlFor="description">
              Description <span className="required">*</span>
            </label>
            <textarea
              id="description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Please provide details about why you are reporting this listing..."
              rows={6}
              required
              minLength={10}
            />
            <small>Minimum 10 characters</small>
          </div>

          {error && (
            <div className="create-report-modal__error">
              {String(error?.message || 'Failed to create report. Please try again.')}
            </div>
          )}

          <div className="create-report-modal__actions">
            <Button type="button" variant="outline" onClick={onClose} disabled={loading}>
              Cancel
            </Button>
            <Button type="submit" variant="primary" disabled={loading || !description.trim()}>
              {loading ? 'Submitting...' : 'Submit Report'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};

