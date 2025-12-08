import { Button } from '../../../shared';
import {
  REPORT_STATUS_LABELS,
  REPORT_STATUS_COLORS,
  REPORT_REASON_LABELS,
  type Report,
} from '../../../entities/report/model/types';

type TabType = 'my-reports' | 'all-reports' | 'moderation-queue';

interface ReportsListProps {
  reports: Report[];
  activeTab: TabType;
  canManageReports: boolean;
  deletingListing: boolean;
  resolving: boolean;
  dismissing: boolean;
  deleting: boolean;
  onViewListing: (listingId: string) => void;
  onEditListing: (listingId: string) => void;
  onDeleteListing: (listingId: string, reportId: string) => void;
  onDeleteReport: (reportId: string) => void;
  onResolve: (report: Report) => void;
  onDismiss: (report: Report) => void;
}

export const ReportsList = ({
  reports,
  activeTab,
  canManageReports,
  deletingListing,
  resolving,
  dismissing,
  deleting,
  onViewListing,
  onEditListing,
  onDeleteListing,
  onDeleteReport,
  onResolve,
  onDismiss,
}: ReportsListProps) => {
  if (reports.length === 0) {
    return (
      <div className="reports-page__empty">
        <p>No reports found.</p>
      </div>
    );
  }

  return (
    <div className="reports-page__list">
      {reports.map((report) => (
        <div key={report.id} className="reports-page__card">
          <div className="reports-page__card-header">
            <div className="reports-page__card-title">
              <h3>{report.listingTitle || `Listing ${report.listingId.slice(0, 8)}`}</h3>
              <span className="reports-page__status" style={{ color: REPORT_STATUS_COLORS[report.status] }}>
                {REPORT_STATUS_LABELS[report.status]}
              </span>
            </div>
            <div className="reports-page__card-meta">
              <span>Reason: {REPORT_REASON_LABELS[report.reason]}</span>
              <span>Created: {new Date(report.createdAt).toLocaleDateString()}</span>
            </div>
          </div>

          <div className="reports-page__card-body">
            <p>{report.description}</p>
            {report.resolution && (
              <div className="reports-page__resolution">
                <strong>Resolution:</strong> {report.resolution}
              </div>
            )}
            {report.moderatorNotes && (
              <div className="reports-page__notes">
                <strong>Moderator Notes:</strong> {report.moderatorNotes}
              </div>
            )}
          </div>

          <div className="reports-page__card-actions">
            <Button variant="outline" onClick={() => onViewListing(report.listingId)}>
              View Listing
            </Button>
            {canManageReports && (
              <>
                <Button variant="outline" onClick={() => onEditListing(report.listingId)} disabled={deletingListing}>
                  Edit Listing
                </Button>
                <Button variant="danger" onClick={() => onDeleteListing(report.listingId, report.id)} disabled={deletingListing}>
                  Delete Listing
                </Button>
                {report.status === 'PENDING' && (
                  <>
                    <Button
                      variant="primary"
                      onClick={() => onResolve(report)}
                      disabled={resolving || dismissing || deletingListing}
                    >
                      Resolve
                    </Button>
                    <Button
                      variant="outline"
                      onClick={() => onDismiss(report)}
                      disabled={resolving || dismissing || deletingListing}
                    >
                      Dismiss
                    </Button>
                  </>
                )}
              </>
            )}
            {activeTab === 'my-reports' && report.status !== 'RESOLVED' && (
              <Button variant="danger" onClick={() => onDeleteReport(report.id)} disabled={deleting}>
                Delete Report
              </Button>
            )}
          </div>
        </div>
      ))}
    </div>
  );
};

