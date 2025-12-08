import type { CreateListingInput, UpdateListingInput } from '../../../../entities/listing';

type FormData = Partial<CreateListingInput | UpdateListingInput>;

export interface ValidationErrors {
  [key: string]: string;
}

export const validateListingForm = (
  formData: FormData,
  mapPosition: [number, number] | null,
  isEditMode = false
): ValidationErrors => {
  const errors: ValidationErrors = {};

  if (!formData.title?.trim()) errors.title = 'Title is required';
  if (!formData.description?.trim()) errors.description = 'Description is required';
  
  // Validate address fields (required in both create and edit mode)
  if (!formData.addressLine?.trim()) errors.addressLine = 'Address is required';
  if (!formData.postalCode?.trim()) errors.postalCode = 'Postal code is required';
  
  if (!isEditMode) {
    const createData = formData as Partial<CreateListingInput>;
    
    if (createData.category) {
      if (createData.category === 'SALE' && (!createData.pricePln || createData.pricePln <= 0)) {
        errors.pricePln = 'Price is required for sale listings';
      }
      if (createData.category === 'RENT' && (!createData.monthlyRentPln || createData.monthlyRentPln <= 0)) {
        errors.monthlyRentPln = 'Monthly rent is required for rental listings';
      }
    }
  } else {
    // For edit mode, validate only if values are provided
    if (formData.pricePln !== null && formData.pricePln !== undefined && formData.pricePln <= 0) {
      errors.pricePln = 'Price must be greater than 0';
    }
    if (formData.monthlyRentPln !== null && formData.monthlyRentPln !== undefined && formData.monthlyRentPln <= 0) {
      errors.monthlyRentPln = 'Monthly rent must be greater than 0';
    }
  }
  
  if (!formData.district?.trim()) errors.district = 'District is required';
  if (!formData.city?.trim()) errors.city = 'City is required';
  
  if (!formData.squareMeters || formData.squareMeters <= 0) {
    errors.squareMeters = 'Square meters must be greater than 0';
  }
  
  if (!formData.rooms || formData.rooms <= 0) {
    errors.rooms = 'Number of rooms must be greater than 0';
  }
  
  if (!mapPosition || mapPosition.length !== 2) {
    errors.location = 'Please select a location on the map';
  }

  return errors;
};

