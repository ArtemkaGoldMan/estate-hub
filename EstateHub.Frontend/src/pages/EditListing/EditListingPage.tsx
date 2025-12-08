import { Button, LoadingSpinner, Modal } from '../../shared';
import {
  ListingFormBasicInfo,
  ListingFormLocation,
  ListingFormPropertyDetails,
  ListingFormPricing,
  ListingFormFeatures,
} from '../../widgets/listings/listing-form';
import { PhotoManager } from '../../features/listings/photos/ui/PhotoManager';
import { useEditListingForm } from './hooks/useEditListingForm';
import '../../widgets/listings/listing-form/components/ListingForm.css';
import './EditListingPage.css';

export const EditListingPage = () => {
  const {
    listing,
    listingLoading,
    combinedError,
    formData,
    mapPosition,
    errors,
    updating,
    deleting,
    showDeleteModal,
    setShowDeleteModal,
    handleSubmit,
    handleInputChange,
    handleMapClick,
    handleDelete,
  } = useEditListingForm();

  if (listingLoading) {
    return (
      <div className="edit-listing-page">
        <div className="edit-listing-page__loading">
          <LoadingSpinner text="Loading listing..." />
        </div>
      </div>
    );
  }

  const errorMessage =
    combinedError && combinedError instanceof Error
      ? combinedError.message
      : combinedError
      ? String(combinedError)
      : null;

  if (combinedError || !listing) {
    return (
      <div className="edit-listing-page">
        <div className="edit-listing-page__error">
          <h2>Listing not found</h2>
          <p>{errorMessage || 'Sorry, we could not find the listing you are looking for.'}</p>
          <Button onClick={() => window.history.back()}>Back</Button>
        </div>
      </div>
    );
  }

  return (
    <div className="edit-listing-page">
      <div className="edit-listing-page__header">
        <h1>Edit Listing</h1>
        <Button variant="ghost" onClick={() => window.history.back()}>
          ← Back
        </Button>
      </div>

      {combinedError && (
        <div className="edit-listing-page__error-banner">
          <p>{errorMessage}</p>
        </div>
      )}

      <form onSubmit={handleSubmit} className="edit-listing-page__form">
        <ListingFormBasicInfo
          category={listing.category}
          propertyType={listing.propertyType}
          title={formData.title || ''}
          description={formData.description || ''}
          condition={formData.condition || 'GOOD'}
          errors={errors}
          onCategoryChange={() => {}}
          onPropertyTypeChange={() => {}}
          onTitleChange={(value) => handleInputChange('title', value)}
          onDescriptionChange={(value) => handleInputChange('description', value)}
          onConditionChange={(value) => handleInputChange('condition', value)}
        />

        <ListingFormLocation
          addressLine={formData.addressLine || ''}
          district={formData.district || ''}
          city={formData.city || ''}
          postalCode={formData.postalCode || ''}
          mapPosition={mapPosition}
          errors={errors}
          onAddressLineChange={(value) => handleInputChange('addressLine', value)}
          onDistrictChange={(value) => handleInputChange('district', value)}
          onCityChange={(value) => handleInputChange('city', value)}
          onPostalCodeChange={(value) => handleInputChange('postalCode', value)}
          onMapPositionChange={handleMapClick}
        />

        <ListingFormPropertyDetails
          squareMeters={formData.squareMeters || 0}
          rooms={formData.rooms || 0}
          floor={formData.floor ?? null}
          floorCount={formData.floorCount ?? null}
          buildYear={formData.buildYear ?? null}
          errors={errors}
          onSquareMetersChange={(value) => handleInputChange('squareMeters', value)}
          onRoomsChange={(value) => handleInputChange('rooms', value)}
          onFloorChange={(value) => handleInputChange('floor', value)}
          onFloorCountChange={(value) => handleInputChange('floorCount', value)}
          onBuildYearChange={(value) => handleInputChange('buildYear', value)}
        />

        <ListingFormPricing
          category={listing.category}
          pricePln={formData.pricePln ?? null}
          monthlyRentPln={formData.monthlyRentPln ?? null}
          errors={errors}
          onPricePlnChange={(value) => handleInputChange('pricePln', value)}
          onMonthlyRentPlnChange={(value) => handleInputChange('monthlyRentPln', value)}
        />

        <ListingFormFeatures
          hasBalcony={formData.hasBalcony ?? false}
          hasElevator={formData.hasElevator ?? false}
          hasParkingSpace={formData.hasParkingSpace ?? false}
          hasSecurity={formData.hasSecurity ?? false}
          hasStorageRoom={formData.hasStorageRoom ?? false}
          onFeatureChange={(feature, value) => handleInputChange(feature as any, value)}
        />

        <div className="edit-listing-page__section" onClick={(e) => e.stopPropagation()}>
          <h2>Photos</h2>
          <PhotoManager listingId={listing.id} />
        </div>

        <div className="edit-listing-page__actions">
          <Button type="submit" variant="primary" disabled={updating || deleting} isLoading={updating}>
            {updating ? 'Saving...' : 'Save Changes'}
          </Button>
          <Button
            type="button"
            variant="outline"
            onClick={() => window.history.back()}
            disabled={updating || deleting}
          >
            Cancel
          </Button>
          <Button
            type="button"
            variant="danger"
            onClick={() => setShowDeleteModal(true)}
            disabled={updating || deleting}
            className="edit-listing-page__delete-button"
          >
            Delete Listing
          </Button>
        </div>
      </form>

      <Modal isOpen={showDeleteModal} onClose={() => setShowDeleteModal(false)} title="Delete Listing">
        <div className="edit-listing-page__delete-modal">
          <p>Are you sure you want to delete this listing? This action cannot be undone.</p>
          <div className="edit-listing-page__delete-modal-actions">
            <Button variant="outline" onClick={() => setShowDeleteModal(false)} disabled={deleting}>
              Cancel
            </Button>
            <Button variant="danger" onClick={handleDelete} disabled={deleting} isLoading={deleting}>
              {deleting ? 'Deleting...' : 'Delete Listing'}
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
};

