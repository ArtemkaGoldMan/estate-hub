import type { ListingFilter } from '../../../../entities/listing';

export interface ListingsFiltersState extends ListingFilter {
  search?: string;
}


