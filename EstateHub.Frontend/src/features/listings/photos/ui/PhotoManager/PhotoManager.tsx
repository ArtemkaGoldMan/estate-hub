import { useState, useCallback, useRef } from 'react';
import {
  useUploadPhoto,
  useRemovePhoto,
  useReorderPhotos,
  usePhotosQuery,
  type Photo,
} from '../../../../../entities/listing';
import { Button, LoadingSpinner } from '../../../../../shared';
import { API_CONFIG } from '../../../../../shared/config';
import './PhotoManager.css';

interface PhotoManagerProps {
  listingId: string;
  readonly?: boolean;
}

export const PhotoManager = ({ listingId, readonly = false }: PhotoManagerProps) => {
  const { photos, loading, refetch } = usePhotosQuery(listingId, true);
  const { uploadPhoto, loading: uploading } = useUploadPhoto();
  const { removePhoto, loading: removing } = useRemovePhoto();
  const { reorderPhotos, loading: reordering } = useReorderPhotos();
  
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [uploadError, setUploadError] = useState<string>('');
  const [draggedIndex, setDraggedIndex] = useState<number | null>(null);

  const handleFileSelect = useCallback(
    async (e: React.ChangeEvent<HTMLInputElement>) => {
      const files = e.target.files;
      if (!files || files.length === 0) return;

      setUploadError('');

      try {
        // Upload files one by one
        for (let i = 0; i < files.length; i++) {
          const file = files[i];
          
          // Validate file type
          if (!file.type.startsWith('image/')) {
            setUploadError(`${file.name} is not an image file`);
            continue;
          }

          // Validate file size (5MB max)
          if (file.size > 5 * 1024 * 1024) {
            setUploadError(`${file.name} exceeds 5MB size limit`);
            continue;
          }

          await uploadPhoto(listingId, file);
        }

        // Reset input
        if (fileInputRef.current) {
          fileInputRef.current.value = '';
        }

        // Refetch photos
        await refetch();
      } catch (error) {
        setUploadError(
          error instanceof Error ? error.message : 'Failed to upload photo(s)'
        );
      }
    },
    [listingId, uploadPhoto, refetch]
  );

  const handleRemove = useCallback(
    async (photoId: string) => {
      if (!confirm('Are you sure you want to remove this photo?')) {
        return;
      }

      try {
        await removePhoto(listingId, photoId);
        await refetch();
      } catch (error) {
        setUploadError(
          error instanceof Error ? error.message : 'Failed to remove photo'
        );
      }
    },
    [listingId, removePhoto, refetch]
  );

  const handleMove = useCallback(
    async (fromIndex: number, toIndex: number) => {
      if (fromIndex === toIndex || !photos) return;

      const newPhotos = [...photos];
      const [moved] = newPhotos.splice(fromIndex, 1);
      newPhotos.splice(toIndex, 0, moved);

      // Update order
      const photoOrders = newPhotos.map((photo, index) => ({
        photoId: photo.id,
        order: index,
      }));

      try {
        await reorderPhotos({
          listingId,
          photoOrders,
        });
        await refetch();
      } catch (error) {
        setUploadError(
          error instanceof Error ? error.message : 'Failed to reorder photos'
        );
      }
    },
    [listingId, photos, reorderPhotos, refetch]
  );

  const handleDragStart = useCallback((index: number) => {
    setDraggedIndex(index);
  }, []);

  const handleDragOver = useCallback((e: React.DragEvent, index: number) => {
    e.preventDefault();
    if (draggedIndex === null || draggedIndex === index) return;
    // Visual feedback could be added here
  }, [draggedIndex]);

  const handleDrop = useCallback(
    (e: React.DragEvent, dropIndex: number) => {
      e.preventDefault();
      if (draggedIndex === null || draggedIndex === dropIndex) {
        setDraggedIndex(null);
        return;
      }

      handleMove(draggedIndex, dropIndex);
      setDraggedIndex(null);
    },
    [draggedIndex, handleMove]
  );

  const getPhotoUrl = (photo: Photo) => {
    // If URL is a GridFS URL, use it directly (relative path)
    if (photo.url.startsWith('/api/photo/gridfs/')) {
      // Use the assets base URL or construct from GraphQL URL
      const baseUrl = API_CONFIG.assetsBaseUrl.replace('/api/photo', '') || '';
      return `${baseUrl}${photo.url}`;
    }
    // If it's already a full URL
    if (photo.url.startsWith('http')) {
      return photo.url;
    }
    // Default: use photo ID endpoint
    const baseUrl = API_CONFIG.assetsBaseUrl.replace('/api/photo', '') || '';
    return `${baseUrl}/api/photo/${photo.id}`;
  };

  if (loading) {
    return (
      <div className="photo-manager">
        <LoadingSpinner text="Loading photos..." />
      </div>
    );
  }

  return (
    <div className="photo-manager" onClick={(e) => e.stopPropagation()}>
      <div className="photo-manager__header">
        <h3>Photos ({photos.length})</h3>
        {!readonly && (
          <>
            <input
              ref={fileInputRef}
              type="file"
              accept="image/jpeg,image/jpg,image/png,image/webp"
              multiple
              onChange={handleFileSelect}
              onClick={(e) => e.stopPropagation()}
              style={{ display: 'none' }}
            />
            <Button
              type="button"
              variant="primary"
              size="sm"
              onClick={(e) => {
                e.preventDefault();
                e.stopPropagation();
                fileInputRef.current?.click();
              }}
              disabled={uploading}
              isLoading={uploading}
            >
              {uploading ? 'Uploading...' : '+ Add Photos'}
            </Button>
          </>
        )}
      </div>

      {uploadError && (
        <div className="photo-manager__error">
          <p>{uploadError}</p>
          <button onClick={() => setUploadError('')}>×</button>
        </div>
      )}

      {photos.length === 0 ? (
        <div className="photo-manager__empty">
          <p>No photos yet. Add photos to showcase your property.</p>
        </div>
      ) : (
        <div className="photo-manager__grid">
          {photos.map((photo, index) => (
            <div
              key={photo.id}
              className="photo-manager__item"
              draggable={!readonly && !reordering}
              onDragStart={() => handleDragStart(index)}
              onDragOver={(e) => handleDragOver(e, index)}
              onDrop={(e) => handleDrop(e, index)}
            >
              <div className="photo-manager__image">
                <img src={getPhotoUrl(photo)} alt={`Photo ${index + 1}`} />
                {!readonly && (
                  <div className="photo-manager__overlay">
                    <div className="photo-manager__order">#{index + 1}</div>
                    <div className="photo-manager__actions">
                      {index > 0 && (
                        <button
                          className="photo-manager__action"
                          onClick={() => handleMove(index, index - 1)}
                          disabled={reordering}
                          title="Move up"
                        >
                          ↑
                        </button>
                      )}
                      {index < photos.length - 1 && (
                        <button
                          className="photo-manager__action"
                          onClick={() => handleMove(index, index + 1)}
                          disabled={reordering}
                          title="Move down"
                        >
                          ↓
                        </button>
                      )}
                      <button
                        className="photo-manager__action photo-manager__action--danger"
                        onClick={() => handleRemove(photo.id)}
                        disabled={removing}
                        title="Remove"
                      >
                        ×
                      </button>
                    </div>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {photos.length > 0 && !readonly && (
        <p className="photo-manager__hint">
          Drag photos to reorder, or use the arrow buttons. The first photo will be used as the main image.
        </p>
      )}
    </div>
  );
};

