import { useState, useCallback, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../shared/context/AuthContext';
import { useToast } from '../../shared/context/ToastContext';
import {
  useMyReportsQuery,
  useReportsQuery,
  useReportsForModerationQuery,
  useResolveReport,
  useDismissReport,
  useDeleteReport,
} from '../../entities/report';
import { useDeleteListing } from '../../entities/listing/api/delete-listing';
import { getUserRoles, hasPermission, PERMISSIONS } from '../../shared/lib/permissions';
import type { Report, ReportStatus, ReportFilter } from '../../entities/report/model/types';
import { REPORT_STATUS_LABELS, REPORT_STATUS_COLORS, REPORT_REASON_LABELS } from '../../entities/report/model/types';
import { Button, LoadingSpinner } from '../../shared/ui';
import './ReportsPage.css';

const PAGE_SIZE = 10;

type TabType = 'my-reports' | 'all-reports' | 'moderation-queue';

export const ReportsPage = () => {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();
  const { showSuccess, showError } = useToast();
  const [activeTab, setActiveTab] = useState<TabType>('my-reports');
  const [myReportsPage, setMyReportsPage] = useState(1);
  const [allReportsPage, setAllReportsPage] = useState(1);
  const [moderationPage, setModerationPage] = useState(1);
  const [filter, setFilter] = useState<ReportFilter | null>(null);
  const [selectedReport, setSelectedReport] = useState<Report | null>(null);
  const [showResolveModal, setShowResolveModal] = useState(false);
  const [showDismissModal, setShowDismissModal] = useState(false);
  const [resolution, setResolution] = useState('');
  const [moderatorNotes, setModeratorNotes] = useState('');

  const userRoles = getUserRoles();
  const canViewAllReports = hasPermission(userRoles, PERMISSIONS.ViewReports);
  const canManageReports = hasPermission(userRoles, PERMISSIONS.ManageReports);

  // Note: Route protection is now handled by ProtectedRoute component

  // Set default tab based on permissions
  useEffect(() => {
    if (canManageReports && activeTab === 'my-reports') {
      setActiveTab('moderation-queue');
    }
  }, [canManageReports, activeTab]);

  const { data: myReportsData, loading: myReportsLoading, error: myReportsError, refetch: refetchMyReports } =
    useMyReportsQuery(myReportsPage, PAGE_SIZE);

  const { data: allReportsData, loading: allReportsLoading, error: allReportsError, refetch: refetchAllReports } =
    useReportsQuery(filter, allReportsPage, PAGE_SIZE, canViewAllReports);

  const { data: moderationData, loading: moderationLoading, error: moderationError, refetch: refetchModeration } =
    useReportsForModerationQuery(moderationPage, PAGE_SIZE);

  const { resolveReport, loading: resolving } = useResolveReport();
  const { dismissReport, loading: dismissing } = useDismissReport();
  const { deleteReport, loading: deleting } = useDeleteReport();
  const { deleteListing, loading: deletingListing } = useDeleteListing();

  const handleResolve = useCallback(async () => {
    if (!selectedReport || !resolution.trim()) return;

    try {
      await resolveReport({
        reportId: selectedReport.id,
        resolution: resolution.trim(),
        moderatorNotes: moderatorNotes.trim() || null,
      });
      setShowResolveModal(false);
      setResolution('');
      setModeratorNotes('');
      setSelectedReport(null);
      refetchModeration();
      refetchAllReports();
      showSuccess('Report resolved successfully');
    } catch (error) {
      showError(error instanceof Error ? error.message : 'Failed to resolve report');
    }
  }, [selectedReport, resolution, moderatorNotes, resolveReport, refetchModeration, refetchAllReports, showSuccess, showError]);

  const handleDismiss = useCallback(async () => {
    if (!selectedReport) return;

    try {
      await dismissReport({
        reportId: selectedReport.id,
        moderatorNotes: moderatorNotes.trim() || null,
      });
      setShowDismissModal(false);
      setModeratorNotes('');
      setSelectedReport(null);
      refetchModeration();
      refetchAllReports();
      showSuccess('Report dismissed successfully');
    } catch (error) {
      showError(error instanceof Error ? error.message : 'Failed to dismiss report');
    }
  }, [selectedReport, moderatorNotes, dismissReport, refetchModeration, refetchAllReports, showSuccess, showError]);

  const handleDelete = useCallback(async (reportId: string) => {
    if (!confirm('Are you sure you want to delete this report?')) return;

    try {
      await deleteReport(reportId);
      refetchMyReports();
      showSuccess('Report deleted successfully');
    } catch (error) {
      showError(error instanceof Error ? error.message : 'Failed to delete report');
    }
  }, [deleteReport, refetchMyReports, showSuccess, showError]);

  const handleViewListing = useCallback((listingId: string) => {
    navigate(`/listings/${listingId}`);
  }, [navigate]);

  const handleEditListing = useCallback((listingId: string) => {
    navigate(`/listings/${listingId}/edit`);
  }, [navigate]);

  const handleDeleteListing = useCallback(async (listingId: string, reportId: string) => {
    if (!confirm('Are you sure you want to delete this listing? This action cannot be undone.')) {
      return;
    }

    try {
      await deleteListing(listingId);
      // Also resolve the report since we've taken action
      try {
        await resolveReport({
          reportId,
          resolution: 'Listing deleted by administrator.',
          moderatorNotes: 'Listing was deleted as a result of this report.',
        });
      } catch (err) {
        // Report resolution failed, but listing was deleted - show warning
        showError('Listing deleted, but failed to update report status');
      }
      refetchModeration();
      refetchAllReports();
      showSuccess('Listing deleted successfully');
    } catch (error) {
      showError(error instanceof Error ? error.message : 'Failed to delete listing');
    }
  }, [deleteListing, resolveReport, refetchModeration, refetchAllReports, showSuccess, showError]);

  if (!isAuthenticated) {
    return null;
  }

  const activeData =
    activeTab === 'my-reports'
      ? myReportsData
      : activeTab === 'all-reports'
      ? allReportsData
      : moderationData;
  const activeLoading =
    activeTab === 'my-reports'
      ? myReportsLoading
      : activeTab === 'all-reports'
      ? allReportsLoading
      : moderationLoading;
  const activeError =
    activeTab === 'my-reports'
      ? myReportsError
      : activeTab === 'all-reports'
      ? allReportsError
      : moderationError;
  const activePage =
    activeTab === 'my-reports'
      ? myReportsPage
      : activeTab === 'all-reports'
      ? allReportsPage
      : moderationPage;
  const setActivePage =
    activeTab === 'my-reports'
      ? setMyReportsPage
      : activeTab === 'all-reports'
      ? setAllReportsPage
      : setModerationPage;

  const reports = activeData ?? { items: [], total: 0 };
  const totalPages = Math.ceil(reports.total / PAGE_SIZE);

  return (
    <div className="reports-page">
      <div className="reports-page__header">
        <h1>Reports</h1>
      </div>

      <div className="reports-page__tabs">
        <button
          className={`reports-page__tab ${activeTab === 'my-reports' ? 'active' : ''}`}
          onClick={() => setActiveTab('my-reports')}
        >
          My Reports
        </button>
        {canViewAllReports && (
          <button
            className={`reports-page__tab ${activeTab === 'all-reports' ? 'active' : ''}`}
            onClick={() => setActiveTab('all-reports')}
          >
            All Reports
          </button>
        )}
        {canManageReports && (
          <button
            className={`reports-page__tab ${activeTab === 'moderation-queue' ? 'active' : ''}`}
            onClick={() => setActiveTab('moderation-queue')}
          >
            Moderation Queue
          </button>
        )}
      </div>

      {activeTab === 'all-reports' && canViewAllReports && (
        <div className="reports-page__filters">
          <select
            value={filter?.status || ''}
            onChange={(e) =>
              setFilter({
                ...filter,
                status: e.target.value ? (e.target.value as ReportStatus) : null,
              })
            }
          >
            <option value="">All Statuses</option>
            {Object.entries(REPORT_STATUS_LABELS).map(([value, label]) => (
              <option key={value} value={value}>
                {label}
              </option>
            ))}
          </select>
          <Button
            variant="outline"
            onClick={() => setFilter(null)}
            disabled={!filter}
          >
            Clear Filters
          </Button>
        </div>
      )}

      {activeLoading && (
        <div className="reports-page__loading">
          <LoadingSpinner />
        </div>
      )}

      {activeError && (
        <div className="reports-page__error">
          <p>Failed to load reports. Please try again.</p>
        </div>
      )}

      {!activeLoading && !activeError && (
        <>
          {reports.items.length === 0 ? (
            <div className="reports-page__empty">
              <p>No reports found.</p>
            </div>
          ) : (
            <div className="reports-page__list">
              {reports.items.map((report) => (
                <div key={report.id} className="reports-page__card">
                  <div className="reports-page__card-header">
                    <div className="reports-page__card-title">
                      <h3>
                        {report.listingTitle || `Listing ${report.listingId.slice(0, 8)}`}
                      </h3>
                      <span
                        className="reports-page__status"
                        style={{ color: REPORT_STATUS_COLORS[report.status] }}
                      >
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
                    <Button
                      variant="outline"
                      onClick={() => handleViewListing(report.listingId)}
                    >
                      View Listing
                    </Button>
                    {canManageReports && (
                      <>
                        <Button
                          variant="outline"
                          onClick={() => handleEditListing(report.listingId)}
                          disabled={deletingListing}
                        >
                          Edit Listing
                        </Button>
                        <Button
                          variant="danger"
                          onClick={() => handleDeleteListing(report.listingId, report.id)}
                          disabled={deletingListing}
                        >
                          Delete Listing
                        </Button>
                        {report.status === 'PENDING' && (
                          <>
                            <Button
                              variant="primary"
                              onClick={() => {
                                setSelectedReport(report);
                                setShowResolveModal(true);
                              }}
                              disabled={resolving || dismissing || deletingListing}
                            >
                              Resolve
                            </Button>
                            <Button
                              variant="outline"
                              onClick={() => {
                                setSelectedReport(report);
                                setShowDismissModal(true);
                              }}
                              disabled={resolving || dismissing || deletingListing}
                            >
                              Dismiss
                            </Button>
                          </>
                        )}
                      </>
                    )}
                    {activeTab === 'my-reports' && report.status !== 'RESOLVED' && (
                      <Button
                        variant="danger"
                        onClick={() => handleDelete(report.id)}
                        disabled={deleting}
                      >
                        Delete Report
                      </Button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}

          {totalPages > 1 && (
            <div className="reports-page__pagination">
              <Button
                variant="outline"
                onClick={() => setActivePage(Math.max(1, activePage - 1))}
                disabled={activePage === 1}
              >
                Previous
              </Button>
              <span>
                Page {activePage} of {totalPages}
              </span>
              <Button
                variant="outline"
                onClick={() => setActivePage(Math.min(totalPages, activePage + 1))}
                disabled={activePage === totalPages}
              >
                Next
              </Button>
            </div>
          )}
        </>
      )}

      {/* Resolve Modal */}
      {showResolveModal && selectedReport && (
        <div className="reports-page__modal-overlay" onClick={() => setShowResolveModal(false)}>
          <div className="reports-page__modal" onClick={(e) => e.stopPropagation()}>
            <h2>Resolve Report</h2>
            <div className="reports-page__modal-content">
              <label>
                Resolution *
                <textarea
                  value={resolution}
                  onChange={(e) => setResolution(e.target.value)}
                  placeholder="Describe the resolution..."
                  rows={4}
                  required
                />
              </label>
              <label>
                Moderator Notes (optional)
                <textarea
                  value={moderatorNotes}
                  onChange={(e) => setModeratorNotes(e.target.value)}
                  placeholder="Internal notes..."
                  rows={3}
                />
              </label>
            </div>
            <div className="reports-page__modal-actions">
              <Button variant="outline" onClick={() => setShowResolveModal(false)}>
                Cancel
              </Button>
              <Button variant="primary" onClick={handleResolve} disabled={!resolution.trim() || resolving}>
                {resolving ? 'Resolving...' : 'Resolve'}
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* Dismiss Modal */}
      {showDismissModal && selectedReport && (
        <div className="reports-page__modal-overlay" onClick={() => setShowDismissModal(false)}>
          <div className="reports-page__modal" onClick={(e) => e.stopPropagation()}>
            <h2>Dismiss Report</h2>
            <div className="reports-page__modal-content">
              <p>Are you sure you want to dismiss this report? No action will be taken.</p>
              <label>
                Moderator Notes (optional)
                <textarea
                  value={moderatorNotes}
                  onChange={(e) => setModeratorNotes(e.target.value)}
                  placeholder="Reason for dismissal..."
                  rows={3}
                />
              </label>
            </div>
            <div className="reports-page__modal-actions">
              <Button variant="outline" onClick={() => setShowDismissModal(false)}>
                Cancel
              </Button>
              <Button variant="primary" onClick={handleDismiss} disabled={dismissing}>
                {dismissing ? 'Dismissing...' : 'Dismiss'}
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

