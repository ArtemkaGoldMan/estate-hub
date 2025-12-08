import { useParams, useNavigate } from 'react-router-dom';
import { useEffect, useState, useCallback } from 'react';
import { useAuth } from '../../../shared/context/AuthContext';
import { useToast } from '../../../shared/context/ToastContext';
import { useListingQuery, usePhotosQuery } from '../../../entities/listing';
import { useChangeListingStatus } from '../../../entities/listing/api/publish-listing';
import { useDeleteListing } from '../../../entities/listing/api/delete-listing';
import { userApi, type GetUserResponse } from '../../../shared/api/auth/userApi';

export const useDashboardListingDetail = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const { showSuccess, showError } = useToast();
  
  const { listing, loading, error, refetch } = useListingQuery(id || '', {
    enablePolling: true,
    pollInterval: 10000,
  });
  const { photos } = usePhotosQuery(id || '', true);
  const { publishListing, unpublishListing, unarchiveListing, archiveListing, loading: statusLoading } = useChangeListingStatus();
  const { deleteListing, loading: deleteLoading } = useDeleteListing();
  
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [ownerInfo, setOwnerInfo] = useState<GetUserResponse | null>(null);
  const [loadingOwner, setLoadingOwner] = useState(false);
  
  const isOwner = listing && user && listing.ownerId === user.id;
  const isDraft = listing?.status === 'Draft';
  const isPublished = listing?.status === 'Published';
  const isArchived = listing?.status === 'Archived';
  const canPublish = isDraft && listing?.isModerationApproved === true && !listing?.adminUnpublishReason;
  
  useEffect(() => {
    if (!id) {
      navigate('/dashboard', { replace: true });
    }
  }, [id, navigate]);
  
  useEffect(() => {
    if (listing && user && listing.ownerId !== user.id) {
      navigate('/dashboard', { replace: true });
    }
  }, [listing, user, navigate]);
  
  useEffect(() => {
    if (!listing?.ownerId || !user) return;
    
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
  }, [listing?.ownerId, user]);
  
  const handlePublish = useCallback(async () => {
    if (!listing) return;
    try {
      await publishListing(listing.id);
      await refetch();
      showSuccess('Listing published successfully');
    } catch (error) {
      showError(error instanceof Error ? error.message : 'Failed to publish listing');
    }
  }, [listing, publishListing, refetch, showSuccess, showError]);
  
  const handleUnpublish = useCallback(async () => {
    if (!listing) return;
    try {
      await unpublishListing(listing.id);
      await refetch();
      showSuccess('Listing unpublished successfully');
    } catch (error) {
      showError(error instanceof Error ? error.message : 'Failed to unpublish listing');
    }
  }, [listing, unpublishListing, refetch, showSuccess, showError]);
  
  const handleUnarchive = useCallback(async () => {
    if (!listing) return;
    if (!confirm('Are you sure you want to unarchive this listing? It will be moved to draft status.')) {
      return;
    }
    try {
      await unarchiveListing(listing.id);
      await refetch();
      showSuccess('Listing unarchived successfully. It is now in draft status.');
    } catch (error) {
      showError(error instanceof Error ? error.message : 'Failed to unarchive listing');
    }
  }, [listing, unarchiveListing, refetch, showSuccess, showError]);
  
  const handleArchive = useCallback(async () => {
    if (!listing) return;
    if (!confirm('Are you sure you want to archive this listing? It will be hidden from public view.')) {
      return;
    }
    try {
      await archiveListing(listing.id);
      await refetch();
      showSuccess('Listing archived successfully');
    } catch (error) {
      showError(error instanceof Error ? error.message : 'Failed to archive listing');
    }
  }, [listing, archiveListing, refetch, showSuccess, showError]);
  
  const handleDelete = useCallback(async () => {
    if (!listing || !showDeleteConfirm) {
      setShowDeleteConfirm(true);
      return;
    }
    
    try {
      await deleteListing(listing.id);
      navigate('/dashboard', { replace: true });
    } catch (error) {
      showError(error instanceof Error ? error.message : 'Failed to delete listing');
      setShowDeleteConfirm(false);
    }
  }, [listing, showDeleteConfirm, deleteListing, navigate, showError]);
  
  const handleBack = useCallback(() => {
    navigate('/dashboard');
  }, [navigate]);
  
  const handleEdit = useCallback(() => {
    navigate(`/listings/${listing?.id}/edit`);
  }, [listing, navigate]);
  
  return {
    listing,
    photos,
    ownerInfo,
    loading,
    loadingOwner,
    error,
    statusLoading,
    deleteLoading,
    showDeleteConfirm,
    isOwner,
    isDraft,
    isPublished,
    isArchived,
    canPublish,
    handlePublish,
    handleUnpublish,
    handleArchive,
    handleUnarchive,
    handleDelete,
    handleBack,
    handleEdit,
  };
};



