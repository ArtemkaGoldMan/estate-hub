export type ListingCategory = 'SALE' | 'RENT';

export type PropertyType =
  | 'APARTMENT'
  | 'HOUSE'
  | 'STUDIO'
  | 'ROOM'
  | 'OTHER';

export type ListingCondition = 'NEW' | 'GOOD' | 'NEEDS_RENOVATION';

export type ListingStatus = 'Draft' | 'Published' | 'Archived';

export interface Listing {
  id: string;
  ownerId: string;
  title: string;
  description: string;
  pricePln?: number | null;
  monthlyRentPln?: number | null;
  status: ListingStatus;
  category: ListingCategory;
  propertyType: PropertyType;
  addressLine: string;
  city: string;
  district: string;
  postalCode: string;
  latitude: number;
  longitude: number;
  squareMeters: number;
  rooms: number;
  floor?: number | null;
  floorCount?: number | null;
  buildYear?: number | null;
  condition: ListingCondition;
  hasBalcony: boolean;
  hasElevator: boolean;
  hasParkingSpace: boolean;
  hasSecurity: boolean;
  hasStorageRoom: boolean;
  createdAt: string;
  updatedAt: string;
  publishedAt?: string | null;
  archivedAt?: string | null;
  firstPhotoUrl?: string | null;
  isLikedByCurrentUser?: boolean;
  isModerationApproved?: boolean | null;
  moderationCheckedAt?: string | null;
  moderationRejectionReason?: string | null;
  adminUnpublishReason?: string | null;
}

export interface ListingFilter {
  city?: string;
  district?: string;
  minPrice?: number;
  maxPrice?: number;
  minMeters?: number;
  maxMeters?: number;
  minRooms?: number;
  maxRooms?: number;
  hasElevator?: boolean;
  hasParkingSpace?: boolean;
  category?: ListingCategory;
  propertyType?: PropertyType;
  condition?: ListingCondition;
  hasBalcony?: boolean;
  hasSecurity?: boolean;
  hasStorageRoom?: boolean;
}

export interface ListingsRequest {
  page: number;
  pageSize: number;
  filter?: ListingFilter;
  search?: string;
}

export interface ListingsResponse {
  items: Listing[];
  total: number;
}

export interface MapBounds {
  latMin: number;
  latMax: number;
  lonMin: number;
  lonMax: number;
}


