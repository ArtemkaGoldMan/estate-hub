import { Button, LoadingSpinner } from '../../shared/ui';
import './ReportsPage.css';
import { ReportsTabs } from './components/ReportsTabs';
import { ReportsFilters } from './components/ReportsFilters';
import { ReportsList } from './components/ReportsList';
import { ResolveModal, DismissModal } from './components/ReportsModals';
import { useReportsPage } from './hooks/useReportsPage';

export const ReportsPage = () => {
  const {
    isAuthenticated,
    activeTab,
    setActiveTab,
    filter,
    setFilter,
    canViewAllReports,
    canManageReports,
    activeLoading,
    activeError,
    reports,
    activePage,
    setActivePage,
    totalPages,
    selectedReport,
    setSelectedReport,
    showResolveModal,
    setShowResolveModal,
    showDismissModal,
    setShowDismissModal,
    resolution,
    setResolution,
    moderatorNotes,
    setModeratorNotes,
    unpublishListing,
    setUnpublishListing,
    unpublishReason,
    setUnpublishReason,
    handleResolve,
    handleDismiss,
    handleDeleteReport,
    handleViewListing,
    handleEditListing,
    handleDeleteListing,
    resolving,
    dismissing,
    deleting,
    deletingListing,
  } = useReportsPage();

  if (!isAuthenticated) return null;

  return (
    <div className="reports-page">
      <div className="reports-page__header">
        <h1>Reports</h1>
      </div>

      <ReportsTabs
        activeTab={activeTab}
        setActiveTab={setActiveTab}
        canViewAllReports={canViewAllReports}
        canManageReports={canManageReports}
      />

      {activeTab === 'all-reports' && canViewAllReports && (
        <ReportsFilters
          status={filter?.status || null}
          onStatusChange={(status) => setFilter(status ? { ...filter, status } : null)}
          onClear={() => setFilter(null)}
          disabled={activeLoading}
        />
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
          <ReportsList
            reports={reports.items}
            activeTab={activeTab}
            canManageReports={canManageReports}
            deletingListing={deletingListing}
            resolving={resolving}
            dismissing={dismissing}
            deleting={deleting}
            onViewListing={handleViewListing}
            onEditListing={handleEditListing}
            onDeleteListing={handleDeleteListing}
            onDeleteReport={handleDeleteReport}
            onResolve={(report) => {
              setSelectedReport(report);
              setShowResolveModal(true);
            }}
            onDismiss={(report) => {
              setSelectedReport(report);
              setShowDismissModal(true);
            }}
          />

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

      <ResolveModal
        isOpen={showResolveModal}
        report={selectedReport}
        resolution={resolution}
        moderatorNotes={moderatorNotes}
        unpublishListing={unpublishListing}
        unpublishReason={unpublishReason}
        onResolutionChange={setResolution}
        onModeratorNotesChange={setModeratorNotes}
        onUnpublishToggle={setUnpublishListing}
        onUnpublishReasonChange={setUnpublishReason}
        onClose={() => {
          setShowResolveModal(false);
          setUnpublishListing(false);
          setUnpublishReason('');
        }}
        onConfirm={handleResolve}
      />

      <DismissModal
        isOpen={showDismissModal}
        report={selectedReport}
        moderatorNotes={moderatorNotes}
        onModeratorNotesChange={setModeratorNotes}
        onClose={() => setShowDismissModal(false)}
        onConfirm={handleDismiss}
      />
    </div>
  );
};

