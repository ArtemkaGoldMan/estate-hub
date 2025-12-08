import { Button } from '../../shared';
import { PhotoManager } from '../../features/listings/photos/ui/PhotoManager';
import {
  ListingFormBasicInfo,
  ListingFormLocation,
  ListingFormPropertyDetails,
  ListingFormPricing,
  ListingFormFeatures,
} from '../../widgets/listings/listing-form';
import { useCreateListingForm } from './hooks/useCreateListingForm';
import '../../widgets/listings/listing-form/components/ListingForm.css';
import './CreateListingPage.css';

export const CreateListingPage = () => {
  const {
    formData,
    mapPosition,
    errors,
    createdListingId,
    loading,
    error,
    handleSubmit,
    handleInputChange,
    handleMapClick,
    navigate,
  } = useCreateListingForm();

  return (
    <div className="create-listing-page">
      <div className="create-listing-page__header">
        <h1>Create New Listing</h1>
        <Button variant="ghost" onClick={() => navigate('/dashboard')}>
          ← Back to Dashboard
        </Button>
      </div>

      {(error || errors.submit) && (
        <div className="create-listing-page__error">
          <p>
            {error?.message?.includes('expired') || errors.submit?.includes('expired')
              ? 'Your session has expired. You will be redirected to login shortly.'
              : error?.message || 
                errors.submit || 
                'Failed to create listing. Please try again.'}
          </p>
        </div>
      )}

      <form onSubmit={handleSubmit} className="create-listing-page__form">
        <ListingFormBasicInfo
          category={formData.category!}
          propertyType={formData.propertyType!}
          title={formData.title || ''}
          description={formData.description || ''}
          condition={formData.condition!}
          errors={errors}
          onCategoryChange={(value) => handleInputChange('category', value)}
          onPropertyTypeChange={(value) => handleInputChange('propertyType', value)}
          onTitleChange={(value) => handleInputChange('title', value)}
          onDescriptionChange={(value) => handleInputChange('description', value)}
          onConditionChange={(value) => handleInputChange('condition', value)}
        />

        <ListingFormLocation
          addressLine={formData.addressLine}
          district={formData.district || ''}
          city={formData.city || ''}
          postalCode={formData.postalCode}
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
          category={formData.category!}
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

        {createdListingId && (
          <div className="create-listing-page__section" onClick={(e) => e.stopPropagation()}>
            <h2>Photos</h2>
            <p className="create-listing-page__hint">Listing created! You can now upload photos.</p>
            <PhotoManager listingId={createdListingId} />
          </div>
        )}

        <div className="create-listing-page__actions">
          {!createdListingId ? (
            <>
              <Button
                type="submit"
                variant="primary"
                disabled={loading}
                isLoading={loading}
              >
                {loading ? 'Saving...' : 'Save Listing'}
              </Button>
              <Button
                type="button"
                variant="outline"
                onClick={() => navigate('/dashboard')}
                disabled={loading}
              >
                Cancel
              </Button>
            </>
          ) : (
            <>
              <Button
                type="button"
                variant="primary"
                onClick={() => navigate(`/dashboard/listings/${createdListingId}`)}
              >
                Go to Listing
              </Button>
              <Button
                type="button"
                variant="outline"
                onClick={() => navigate('/dashboard')}
              >
                Back to Dashboard
              </Button>
            </>
          )}
        </div>
      </form>
    </div>
  );
};

