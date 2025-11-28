import { useState } from 'react';
import type { Photo } from '../../api/get-photos';
import { API_CONFIG } from '../../../../shared/config/api';
import './PhotoGallery.css';

interface PhotoGalleryProps {
  photos: Photo[];
  fallbackUrl?: string | null;
  title?: string;
}

export const PhotoGallery = ({ photos, fallbackUrl, title }: PhotoGalleryProps) => {
  const [selectedIndex, setSelectedIndex] = useState(0);
  const [imageErrors, setImageErrors] = useState<Set<number>>(new Set());

  const displayPhotos = photos.length > 0 ? photos : (fallbackUrl ? [{ id: 'fallback', listingId: '', url: fallbackUrl, order: 0 }] : []);
  const currentPhoto = displayPhotos[selectedIndex];

  const handleThumbnailClick = (index: number) => {
    setSelectedIndex(index);
  };

  const handlePrev = () => {
    setSelectedIndex((prev) => (prev > 0 ? prev - 1 : displayPhotos.length - 1));
  };

  const handleNext = () => {
    setSelectedIndex((prev) => (prev < displayPhotos.length - 1 ? prev + 1 : 0));
  };

  const getPhotoUrl = (photo: Photo) => {
    // If URL is already absolute, use it directly
    if (photo.url.startsWith('http://') || photo.url.startsWith('https://')) {
      return photo.url;
    }
    // Otherwise, construct URL from photo ID
    return `${API_CONFIG.assetsBaseUrl}/${photo.id}`;
  };

  const handleImageError = (index: number) => {
    setImageErrors((prev) => new Set(prev).add(index));
  };

  if (displayPhotos.length === 0) {
    return (
      <div className="photo-gallery photo-gallery--empty">
        <div className="photo-gallery__placeholder">No photos available</div>
      </div>
    );
  }

  return (
    <div className="photo-gallery">
      {title && <h2 className="photo-gallery__title">{title}</h2>}
      <div className="photo-gallery__main">
        <div className="photo-gallery__viewer">
          {displayPhotos.length > 1 && (
            <button
              className="photo-gallery__nav photo-gallery__nav--prev"
              onClick={handlePrev}
              aria-label="Previous photo"
            >
              ‹
            </button>
          )}
          <div className="photo-gallery__image-container">
            {currentPhoto && !imageErrors.has(selectedIndex) ? (
              <img
                src={getPhotoUrl(currentPhoto)}
                alt={title || `Photo ${selectedIndex + 1}`}
                className="photo-gallery__main-image"
                onError={() => handleImageError(selectedIndex)}
              />
            ) : (
              <div className="photo-gallery__placeholder">Image not available</div>
            )}
          </div>
          {displayPhotos.length > 1 && (
            <button
              className="photo-gallery__nav photo-gallery__nav--next"
              onClick={handleNext}
              aria-label="Next photo"
            >
              ›
            </button>
          )}
        </div>
        {displayPhotos.length > 1 && (
          <div className="photo-gallery__thumbnails">
            {displayPhotos.map((photo, index) => (
              <button
                key={photo.id}
                className={`photo-gallery__thumbnail ${
                  index === selectedIndex ? 'photo-gallery__thumbnail--active' : ''
                }`}
                onClick={() => handleThumbnailClick(index)}
                aria-label={`View photo ${index + 1}`}
              >
                <img
                  src={getPhotoUrl(photo)}
                  alt={`Thumbnail ${index + 1}`}
                  onError={() => handleImageError(index)}
                />
              </button>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};


