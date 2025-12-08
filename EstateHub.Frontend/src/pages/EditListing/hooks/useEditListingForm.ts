import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '../../../shared/context/AuthContext';
import { useToast } from '../../../shared/context/ToastContext';
import {
  useListingQuery,
  useUpdateListing,
  useDeleteListing,
  type UpdateListingInput,
} from '../../../entities/listing';
import { validateListingForm, type ValidationErrors } from '../../../widgets/listings/listing-form';
import { UserFriendlyError } from '../../../shared/lib/errorParser';

export const useEditListingForm = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const { showError } = useToast();

  const { listing, loading: listingLoading, error: listingError } = useListingQuery(id || '');
  const { updateListing, loading: updating, error: updateError } = useUpdateListing();
  const { deleteListing, loading: deleting, error: deleteError } = useDeleteListing();

  const [formData, setFormData] = useState<Partial<UpdateListingInput>>({});
  const [mapPosition, setMapPosition] = useState<[number, number] | null>(null);
  const [errors, setErrors] = useState<ValidationErrors>({});
  const [showDeleteModal, setShowDeleteModal] = useState(false);

  useEffect(() => {
    if (listing && user) {
      if (listing.ownerId !== user.id) {
        navigate('/dashboard', { replace: true });
        return;
      }

      setFormData({
        title: listing.title,
        description: listing.description,
        addressLine: listing.addressLine || '',
        city: listing.city,
        district: listing.district,
        postalCode: listing.postalCode || '',
        squareMeters: listing.squareMeters,
        rooms: listing.rooms,
        floor: listing.floor ?? null,
        floorCount: listing.floorCount ?? null,
        buildYear: listing.buildYear ?? null,
        condition: listing.condition,
        hasBalcony: listing.hasBalcony,
        hasElevator: listing.hasElevator,
        hasParkingSpace: listing.hasParkingSpace,
        hasSecurity: listing.hasSecurity,
        hasStorageRoom: listing.hasStorageRoom,
        pricePln: listing.pricePln ?? null,
        monthlyRentPln: listing.monthlyRentPln ?? null,
      });

      setMapPosition([listing.latitude, listing.longitude]);
    }
  }, [listing, navigate, user]);

  const handleInputChange = useCallback((field: keyof UpdateListingInput, value: string | number | boolean | null) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
    setErrors((prev) => {
      if (prev[field]) {
        const newErrors = { ...prev };
        delete newErrors[field];
        return newErrors;
      }
      return prev;
    });
  }, []);

  const handleMapClick = useCallback((lat: number, lng: number) => {
    setMapPosition([lat, lng]);
    setErrors((prev) => {
      if (prev.location) {
        const newErrors = { ...prev };
        delete newErrors.location;
        return newErrors;
      }
      return prev;
    });
  }, []);

  const handleSubmit = useCallback(
    async (e: React.FormEvent) => {
      e.preventDefault();
      if (!id || !mapPosition || !listing) return;

      const validationErrors = validateListingForm(formData, mapPosition, true);
      setErrors(validationErrors);
      if (Object.keys(validationErrors).length > 0) return;

      try {
        const input: UpdateListingInput = {
          title: formData.title,
          description: formData.description,
          addressLine: formData.addressLine,
          district: formData.district,
          city: formData.city,
          postalCode: formData.postalCode,
          latitude: mapPosition[0],
          longitude: mapPosition[1],
          squareMeters: formData.squareMeters,
          rooms: formData.rooms,
          floor: formData.floor ?? null,
          floorCount: formData.floorCount ?? null,
          buildYear: formData.buildYear ?? null,
          condition: formData.condition,
          hasBalcony: formData.hasBalcony,
          hasElevator: formData.hasElevator,
          hasParkingSpace: formData.hasParkingSpace,
          hasSecurity: formData.hasSecurity,
          hasStorageRoom: formData.hasStorageRoom,
          pricePln: listing.category === 'SALE' ? formData.pricePln ?? null : null,
          monthlyRentPln: listing.category === 'RENT' ? formData.monthlyRentPln ?? null : null,
        };

        await updateListing(id, input);
        navigate(`/dashboard/listings/${id}`);
      } catch {
        // handled by mutation
      }
    },
    [formData, mapPosition, updateListing, navigate, id, listing]
  );

  const handleDelete = useCallback(async () => {
    if (!id) return;
    try {
      await deleteListing(id);
      navigate('/dashboard', { replace: true });
    } catch (err) {
      if (err instanceof UserFriendlyError) {
        showError(err.userMessage);
      } else {
        showError(err instanceof Error ? err.message : 'Failed to delete listing');
      }
    }
  }, [id, deleteListing, navigate, showError]);

  const combinedError = useMemo(() => updateError || deleteError || listingError, [updateError, deleteError, listingError]);

  return {
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
  };
};

