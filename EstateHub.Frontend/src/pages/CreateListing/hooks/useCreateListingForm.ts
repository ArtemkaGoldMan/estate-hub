import { useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  useCreateListing,
  type CreateListingInput,
  type ListingCategory,
} from '../../../entities/listing';
import { validateListingForm } from '../../../widgets/listings/listing-form';

const DEFAULT_CENTER: [number, number] = [52.2297, 21.0122];

export const useCreateListingForm = () => {
  const navigate = useNavigate();
  const { createListing, loading, error } = useCreateListing();

  const [formData, setFormData] = useState<Partial<CreateListingInput>>({
    category: 'SALE',
    propertyType: 'APARTMENT',
    condition: 'GOOD',
    hasBalcony: false,
    hasElevator: false,
    hasParkingSpace: false,
    hasSecurity: false,
    hasStorageRoom: false,
  });

  const [mapPosition, setMapPosition] = useState<[number, number]>(DEFAULT_CENTER);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [createdListingId, setCreatedListingId] = useState<string | null>(null);

  const handleSubmit = useCallback(
    async (e: React.FormEvent) => {
      e.preventDefault();

      const validationErrors = validateListingForm(formData, mapPosition, false);
      setErrors(validationErrors);
      
      if (Object.keys(validationErrors).length > 0) {
        return;
      }

      try {
        if (!formData.category || (formData.category !== 'SALE' && formData.category !== 'RENT')) {
          setErrors({ submit: 'Invalid category selected' });
          return;
        }

        const input: CreateListingInput = {
          category: formData.category as ListingCategory,
          propertyType: formData.propertyType!,
          title: formData.title!,
          description: formData.description!,
          addressLine: formData.addressLine!,
          district: formData.district!,
          city: formData.city!,
          postalCode: formData.postalCode!,
          latitude: mapPosition[0],
          longitude: mapPosition[1],
          squareMeters: formData.squareMeters!,
          rooms: formData.rooms!,
          floor: formData.floor ?? null,
          floorCount: formData.floorCount ?? null,
          buildYear: formData.buildYear ?? null,
          condition: formData.condition!,
          hasBalcony: formData.hasBalcony ?? false,
          hasElevator: formData.hasElevator ?? false,
          hasParkingSpace: formData.hasParkingSpace ?? false,
          hasSecurity: formData.hasSecurity ?? false,
          hasStorageRoom: formData.hasStorageRoom ?? false,
          pricePln: formData.category === 'SALE' ? formData.pricePln ?? null : null,
          monthlyRentPln: formData.category === 'RENT' ? formData.monthlyRentPln ?? null : null,
        };

        const listingId = await createListing(input);
        setCreatedListingId(listingId);
      } catch (err) {
        if (err instanceof Error) {
          setErrors({ submit: err.message });
        } else {
          setErrors({ submit: 'Failed to create listing. Please try again.' });
        }
      }
    },
    [formData, mapPosition, createListing]
  );

  const handleInputChange = useCallback(
    (field: keyof CreateListingInput, value: string | number | boolean | null) => {
      setFormData((prev) => ({ ...prev, [field]: value }));
      setErrors((prev) => {
        if (prev[field]) {
          const newErrors = { ...prev };
          delete newErrors[field];
          return newErrors;
        }
        return prev;
      });
    },
    []
  );

  const handleMapClick = useCallback((lat: number, lng: number) => {
    setMapPosition([lat, lng]);
    setFormData((prev) => ({
      ...prev,
      latitude: lat,
      longitude: lng,
    }));
    setErrors((prev) => {
      if (prev.location) {
        const newErrors = { ...prev };
        delete newErrors.location;
        return newErrors;
      }
      return prev;
    });
  }, []);

  return {
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
  };
};

