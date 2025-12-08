import { Input } from '../../../../shared';
import type { ListingCategory } from '../../../../entities/listing';

interface ListingFormPricingProps {
  category: ListingCategory;
  pricePln: number | null;
  monthlyRentPln: number | null;
  errors: Record<string, string>;
  onPricePlnChange: (value: number | null) => void;
  onMonthlyRentPlnChange: (value: number | null) => void;
}

export const ListingFormPricing = ({
  category,
  pricePln,
  monthlyRentPln,
  errors,
  onPricePlnChange,
  onMonthlyRentPlnChange,
}: ListingFormPricingProps) => {
  return (
    <div className="listing-form__section">
      <h2>Pricing</h2>

      {category === 'SALE' ? (
        <div className="listing-form__field">
          <label htmlFor="pricePln">Price (PLN) *</label>
          <Input
            id="pricePln"
            type="number"
            min="0"
            step="0.01"
            value={pricePln || ''}
            onChange={(e) => onPricePlnChange(e.target.value ? parseFloat(e.target.value) : null)}
            placeholder="0.00"
            error={errors.pricePln}
          />
        </div>
      ) : (
        <div className="listing-form__field">
          <label htmlFor="monthlyRentPln">Monthly Rent (PLN) *</label>
          <Input
            id="monthlyRentPln"
            type="number"
            min="0"
            step="0.01"
            value={monthlyRentPln || ''}
            onChange={(e) => onMonthlyRentPlnChange(e.target.value ? parseFloat(e.target.value) : null)}
            placeholder="0.00"
            error={errors.monthlyRentPln}
          />
        </div>
      )}
    </div>
  );
};



