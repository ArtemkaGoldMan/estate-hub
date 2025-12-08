import { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../../shared/context/AuthContext';
import { useToast } from '../../../shared/context/ToastContext';
import {
  useMyReportsQuery,
  useReportsQuery,
  useReportsForModerationQuery,
  useResolveReport,
  useDismissReport,
  useDeleteReport,
  type Report,
  type ReportFilter,
} from '../../../entities/report';
import { useDeleteListing } from '../../../entities/listing/api/delete-listing';
import { getUserRoles, hasPermission, PERMISSIONS } from '../../../shared/lib/permissions';

const PAGE_SIZE = 10;
type TabType = 'my-reports' | 'all-reports' | 'moderation-queue';

export const useReportsPage = () => {
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
  const [unpublishListing, setUnpublishListing] = useState(false);
  const [unpublishReason, setUnpublishReason] = useState('');

  const userRoles = getUserRoles();
  const canViewAllReports = hasPermission(userRoles, PERMISSIONS.ViewReports);
  const canManageReports = hasPermission(userRoles, PERMISSIONS.ManageReports);

  useEffect(() => {
    if (canManageReports && activeTab === 'my-reports') {
      setActiveTab('moderation-queue');
    }
  }, [canManageReports, activeTab]);

  // Only query the active tab to reduce unnecessary network requests
  const {
    data: myReportsData,
    loading: myReportsLoading,
    error: myReportsError,
    refetch: refetchMyReports,
  } = useMyReportsQuery(myReportsPage, PAGE_SIZE, {
    skip: activeTab !== 'my-reports',
  });

  const {
    data: allReportsData,
    loading: allReportsLoading,
    error: allReportsError,
    refetch: refetchAllReports,
  } = useReportsQuery(filter, allReportsPage, PAGE_SIZE, canViewAllReports && activeTab === 'all-reports');

  const {
    data: moderationData,
    loading: moderationLoading,
    error: moderationError,
    refetch: refetchModeration,
  } = useReportsForModerationQuery(moderationPage, PAGE_SIZE, {
    skip: activeTab !== 'moderation-queue',
  });

  const { resolveReport, loading: resolving } = useResolveReport();
  const { dismissReport, loading: dismissing } = useDismissReport();
  const { deleteReport, loading: deleting } = useDeleteReport();
  const { deleteListing, loading: deletingListing } = useDeleteListing();

  const handleResolve = useCallback(async () => {
    if (!selectedReport || !resolution.trim()) return;
    if (unpublishListing && !unpublishReason.trim()) {
      showError('Unpublish reason is required when unpublishing a listing');
      return;
    }

    try {
      await resolveReport({
        reportId: selectedReport.id,
        resolution: resolution.trim(),
        moderatorNotes: moderatorNotes.trim() || null,
        unpublishListing,
        unpublishReason: unpublishListing ? unpublishReason.trim() : null,
      });
      setShowResolveModal(false);
      setResolution('');
      setModeratorNotes('');
      setUnpublishListing(false);
      setUnpublishReason('');
      setSelectedReport(null);
      refetchModeration();
      refetchAllReports();
      showSuccess('Report resolved successfully');
    } catch (error) {
      showError(error instanceof Error ? error.message : 'Failed to resolve report');
    }
  }, [
    selectedReport,
    resolution,
    moderatorNotes,
    unpublishListing,
    unpublishReason,
    resolveReport,
    refetchModeration,
    refetchAllReports,
    showSuccess,
    showError,
  ]);

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

  const handleDeleteReport = useCallback(
    async (reportId: string) => {
      if (!confirm('Are you sure you want to delete this report?')) return;

      try {
        await deleteReport(reportId);
        refetchMyReports();
        showSuccess('Report deleted successfully');
      } catch (error) {
        showError(error instanceof Error ? error.message : 'Failed to delete report');
      }
    },
    [deleteReport, refetchMyReports, showSuccess, showError]
  );

  const handleViewListing = useCallback(
    (listingId: string) => {
      navigate(`/listings/${listingId}`);
    },
    [navigate]
  );

  const handleEditListing = useCallback(
    (listingId: string) => {
      navigate(`/listings/${listingId}/edit`);
    },
    [navigate]
  );

  const handleDeleteListing = useCallback(
    async (listingId: string, reportId: string) => {
      if (!confirm('Are you sure you want to delete this listing? This action cannot be undone.')) {
        return;
      }

      try {
        await deleteListing(listingId);
        try {
          await resolveReport({
            reportId,
            resolution: 'Listing deleted by administrator.',
            moderatorNotes: 'Listing was deleted as a result of this report.',
          });
        } catch {
          showError('Listing deleted, but failed to update report status');
        }
        refetchModeration();
        refetchAllReports();
        showSuccess('Listing deleted successfully');
      } catch (error) {
        showError(error instanceof Error ? error.message : 'Failed to delete listing');
      }
    },
    [deleteListing, resolveReport, refetchModeration, refetchAllReports, showSuccess, showError]
  );

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

  const activePage = activeTab === 'my-reports' ? myReportsPage : activeTab === 'all-reports' ? allReportsPage : moderationPage;
  const setActivePage = activeTab === 'my-reports' ? setMyReportsPage : activeTab === 'all-reports' ? setAllReportsPage : setModerationPage;

  const reports = activeData ?? { items: [], total: 0 };
  const totalPages = Math.ceil(reports.total / PAGE_SIZE);

  return {
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
  };
};

