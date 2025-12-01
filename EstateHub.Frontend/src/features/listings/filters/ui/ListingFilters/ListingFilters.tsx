import { useMemo } from 'react';
import { Input, Dropdown, Button } from '../../../../../shared';
import {
  LISTING_CATEGORIES,
  LISTING_CONDITIONS,
  PROPERTY_TYPES,
  type ListingFilter,
} from '../../../../../entities/listing';
import type { ListingsFiltersState } from '../../model/types';
import './ListingFilters.css';

interface ListingFiltersProps {
  filters: ListingsFiltersState;
  onFiltersChange: (filters: ListingsFiltersState) => void;
  onReset?: () => void;
}

type NumericField = keyof Pick<
  ListingsFiltersState,
  'minPrice' | 'maxPrice' | 'minMeters' | 'maxMeters' | 'minRooms' | 'maxRooms'
>;

interface ToggleConfig {
  key: keyof ListingsFiltersState;
  label: string;
}

const TOGGLE_FIELDS: ToggleConfig[] = [
  { key: 'hasBalcony', label: 'Balcony' },
  { key: 'hasElevator', label: 'Elevator' },
  { key: 'hasParkingSpace', label: 'Parking' },
  { key: 'hasSecurity', label: 'Security' },
  { key: 'hasStorageRoom', label: 'Storage' },
];

const asNumber = (value?: string) => {
  if (value === undefined || value === null || value === '') {
    return undefined;
  }

  const numeric = Number(value);
  return Number.isNaN(numeric) ? undefined : numeric;
};

export const ListingFilters = ({
  filters,
  onFiltersChange,
  onReset,
}: ListingFiltersProps) => {
  const dropdownOptions = useMemo(() => {
    return {
      categories: LISTING_CATEGORIES.map((category) => ({
        value: category,
        label: category === 'SALE' ? 'For Sale' : 'For Rent',
      })),
      propertyTypes: PROPERTY_TYPES.map((type) => ({
        value: type,
        label: type,
      })),
      conditions: LISTING_CONDITIONS.map((condition) => ({
        value: condition,
        label:
          condition === 'NEEDS_RENOVATION'
            ? 'Needs Renovation'
            : condition === 'NEW'
              ? 'New'
              : 'Good',
      })),
    };
  }, []);

  const updateFilter = (patch: Partial<ListingsFiltersState>) => {
    onFiltersChange({
      ...filters,
      ...patch,
    });
  };

  const handleNumericChange = (key: NumericField, value: string) => {
    updateFilter({
      [key]: asNumber(value),
    } as Partial<ListingsFiltersState>);
  };

  const handleDropdownChange = (
    key: keyof ListingFilter,
    value?: string | number
  ) => {
    updateFilter({
      [key]: value === undefined || value === '' ? undefined : (value as never),
    });
  };

  const handleToggleChange = (key: ToggleConfig['key']) => {
    updateFilter({
      [key]: !(filters[key] ?? false),
    } as Partial<ListingsFiltersState>);
  };

  return (
    <section className="listing-filters">
      <div className="listing-filters__search">
        <Input
          label="Search listings"
          placeholder="City, district, title..."
          value={filters.search ?? ''}
          onChange={(event) => updateFilter({ search: event.target.value })}
          fullWidth
        />
      </div>

      <div className="listing-filters__row">
        <div className="listing-filters__field">
          <Input
            label="City"
            placeholder="Warsaw"
            value={filters.city ?? ''}
            onChange={(event) => updateFilter({ city: event.target.value })}
            fullWidth
          />
        </div>

        <div className="listing-filters__field">
          <Input
            label="District"
            placeholder="Mokotów"
            value={filters.district ?? ''}
            onChange={(event) => updateFilter({ district: event.target.value })}
            fullWidth
          />
        </div>
      </div>

      <div className="listing-filters__row">
        <div className="listing-filters__field">
          <Dropdown
            label="Category"
            placeholder="Any"
            value={filters.category}
            options={dropdownOptions.categories}
            onChange={(value) => handleDropdownChange('category', value)}
          />
        </div>

        <div className="listing-filters__field">
          <Dropdown
            label="Property type"
            placeholder="Any"
            value={filters.propertyType}
            options={dropdownOptions.propertyTypes}
            onChange={(value) => handleDropdownChange('propertyType', value)}
          />
        </div>
      </div>

      <div className="listing-filters__row">
        <div className="listing-filters__field">
          <Dropdown
            label="Condition"
            placeholder="Any"
            value={filters.condition}
            options={dropdownOptions.conditions}
            onChange={(value) => handleDropdownChange('condition', value)}
          />
        </div>
      </div>

      <div className="listing-filters__row">
        <div className="listing-filters__field">
          <Input
            label="Price from"
            placeholder="200 000"
            type="number"
            min={0}
            value={filters.minPrice ?? ''}
            onChange={(event) => handleNumericChange('minPrice', event.target.value)}
            fullWidth
          />
        </div>

        <div className="listing-filters__field">
          <Input
            label="Price to"
            placeholder="1 000 000"
            type="number"
            min={0}
            value={filters.maxPrice ?? ''}
            onChange={(event) => handleNumericChange('maxPrice', event.target.value)}
            fullWidth
          />
        </div>
      </div>

      <div className="listing-filters__row">
        <div className="listing-filters__field">
          <Input
            label="Min area (m²)"
            placeholder="40"
            type="number"
            min={0}
            value={filters.minMeters ?? ''}
            onChange={(event) => handleNumericChange('minMeters', event.target.value)}
            fullWidth
          />
        </div>

        <div className="listing-filters__field">
          <Input
            label="Max area (m²)"
            placeholder="120"
            type="number"
            min={0}
            value={filters.maxMeters ?? ''}
            onChange={(event) => handleNumericChange('maxMeters', event.target.value)}
            fullWidth
          />
        </div>
      </div>

      <div className="listing-filters__row">
        <div className="listing-filters__field">
          <Input
            label="Min rooms"
            placeholder="2"
            type="number"
            min={0}
            value={filters.minRooms ?? ''}
            onChange={(event) => handleNumericChange('minRooms', event.target.value)}
            fullWidth
          />
        </div>

        <div className="listing-filters__field">
          <Input
            label="Max rooms"
            placeholder="5"
            type="number"
            min={0}
            value={filters.maxRooms ?? ''}
            onChange={(event) => handleNumericChange('maxRooms', event.target.value)}
            fullWidth
          />
        </div>
      </div>

      <div className="listing-filters__toggles">
        {TOGGLE_FIELDS.map(({ key, label }) => (
          <label key={key} className="listing-filters__toggle">
            <input
              type="checkbox"
              checked={Boolean(filters[key])}
              onChange={() => handleToggleChange(key)}
            />
            <span>{label}</span>
          </label>
        ))}
      </div>

      <div className="listing-filters__actions">
        <Button
          type="button"
          variant="ghost"
          onClick={onReset}
          className="listing-filters__reset"
        >
          Reset filters
        </Button>
      </div>
    </section>
  );
};


