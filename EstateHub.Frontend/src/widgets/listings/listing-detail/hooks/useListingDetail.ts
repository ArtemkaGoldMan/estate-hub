import { useParams, useNavigate, useLocation } from 'react-router-dom';
import { useEffect, useState, useCallback } from 'react';
import { useAuth } from '../../../../shared/context/AuthContext';
import { useToast } from '../../../../shared/context/ToastContext';
import { useListingQuery, usePhotosQuery, useLikeListing, useChangeListingStatus } from '../../../../entities/listing';
import { useAdminUnpublishListing } from '../../../../entities/listing/api/admin-unpublish-listing';
import { userApi, type GetUserResponse } from '../../../../shared/api/auth/userApi';
import { getUserRoles } from '../../../../shared/lib/permissions';

/**
 * Custom hook that contains all business logic for ListingDetailPage
 * Separates logic from presentation
 */
export const useListingDetail = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const { user } = useAuth();
  const { showError, showSuccess } = useToast();
  
  const { listing, loading, error, refetch: refetchListing } = useListingQuery(id || '');
  const { photos } = usePhotosQuery(id || '', true);
  const { toggleLike, loading: likeLoading } = useLikeListing();
  const { unpublishListing, archiveListing, unarchiveListing, loading: statusLoading } = useChangeListingStatus();
  const { adminUnpublishListing, loading: adminUnpublishLoading } = useAdminUnpublishListing();
  
  const [ownerInfo, setOwnerInfo] = useState<GetUserResponse | null>(null);
  const [loadingOwner, setLoadingOwner] = useState(false);
  const [showReportModal, setShowReportModal] = useState(false);
  const [showAdminUnpublishModal, setShowAdminUnpublishModal] = useState(false);
  const [adminUnpublishReason, setAdminUnpublishReason] = useState('');
  
  const isAuthenticated = !!user;
  const userRoles = getUserRoles();
  const isAdmin = userRoles.includes('Admin');
  const isOwner = listing?.ownerId === user?.id;
  
  // Redirect if no ID
  useEffect(() => {
    if (!id) {
      navigate('/listings', { replace: true });
    }
  }, [id, navigate]);
  
  // Fetch owner information
  useEffect(() => {
    if (!listing?.ownerId || !isAuthenticated) return;
    
    const fetchOwnerInfo = async () => {
      setLoadingOwner(true);
      try {
        const owner = await userApi.getUser(listing.ownerId);
        setOwnerInfo(owner);
      } catch (error) {
        if (import.meta.env.DEV) {
          console.error('Failed to fetch owner information:', error);
        }
      } finally {
        setLoadingOwner(false);
      }
    };
    
    fetchOwnerInfo();
  }, [listing?.ownerId, isAuthenticated]);
  
  const handleLike = useCallback(async () => {
    if (!listing) return;
    try {
      await toggleLike(listing.id, listing.isLikedByCurrentUser || false);
    } catch (error) {
      showError(error instanceof Error ? error.message : 'Failed to toggle like');
    }
  }, [listing, toggleLike, showError]);
  
  const handleReport = useCallback(() => {
    setShowReportModal(true);
  }, []);
  
  const handleUnpublish = useCallback(async () => {
    if (!listing) return;
    // For admins, show modal instead of confirm dialog
    if (isAdmin) {
      setShowAdminUnpublishModal(true);
      return;
    }
    // For owners, use simple confirm
    if (!confirm('Are you sure you want to unpublish this listing? It will be moved to draft status.')) {
      return;
    }
    try {
      await unpublishListing(listing.id);
      await refetchListing();
      showSuccess('Listing unpublished successfully');
    } catch (error) {
      showError(error instanceof Error ? error.message : 'Failed to unpublish listing');
    }
  }, [listing, isAdmin, unpublishListing, refetchListing, showSuccess, showError]);

  const handleAdminUnpublishClick = useCallback(() => {
    setShowAdminUnpublishModal(true);
  }, []);

  const handleAdminUnpublishConfirm = useCallback(async () => {
    if (!listing || !adminUnpublishReason.trim()) return;
    try {
      await adminUnpublishListing(listing.id, adminUnpublishReason.trim());
      setShowAdminUnpublishModal(false);
      setAdminUnpublishReason('');
      await refetchListing();
      showSuccess('Listing unpublished successfully');
    } catch (error) {
      showError(error instanceof Error ? error.message : 'Failed to unpublish listing');
    }
  }, [listing, adminUnpublishReason, adminUnpublishListing, refetchListing, showSuccess, showError]);

  const handleAdminUnpublishClose = useCallback(() => {
    setShowAdminUnpublishModal(false);
    setAdminUnpublishReason('');
  }, []);
  
  const handleArchive = useCallback(async () => {
    if (!listing) return;
    if (!confirm('Are you sure you want to archive this listing? It will be hidden from public view.')) {
      return;
    }
    try {
      await archiveListing(listing.id);
      await refetchListing();
      showSuccess('Listing archived successfully');
    } catch (error) {
      showError(error instanceof Error ? error.message : 'Failed to archive listing');
    }
  }, [listing, archiveListing, refetchListing, showSuccess, showError]);
  
  const handleUnarchive = useCallback(async () => {
    if (!listing) return;
    if (!confirm('Are you sure you want to unarchive this listing? It will be moved to draft status.')) {
      return;
    }
    try {
      await unarchiveListing(listing.id);
      await refetchListing();
      showSuccess('Listing unarchived successfully. It is now in draft status.');
    } catch (error) {
      showError(error instanceof Error ? error.message : 'Failed to unarchive listing');
    }
  }, [listing, unarchiveListing, refetchListing, showSuccess, showError]);
  
  const handleBack = useCallback(() => {
    const state = location.state as { from?: string } | null;
    if (state?.from) {
      navigate(state.from);
    } else {
      if (window.history.length > 1) {
        navigate(-1);
      } else {
        navigate('/listings');
      }
    }
  }, [location, navigate]);
  
  return {
    // Data
    listing,
    photos,
    ownerInfo,
    
    // Loading states
    loading,
    loadingOwner,
    likeLoading,
    statusLoading,
    
    // Error
    error,
    
    // Flags
    isAuthenticated,
    isAdmin,
    isOwner,
    
    // Modal state
    showReportModal,
    setShowReportModal,
    showAdminUnpublishModal,
    adminUnpublishReason,
    setAdminUnpublishReason,
    adminUnpublishLoading,
    
    // Handlers
    handleLike,
    handleReport,
    handleUnpublish,
    handleAdminUnpublishClick,
    handleAdminUnpublishConfirm,
    handleAdminUnpublishClose,
    handleArchive,
    handleUnarchive,
    handleBack,
    refetchListing,
  };
};

