import { Input } from '../../../../shared';

interface ListingFormPropertyDetailsProps {
  squareMeters: number;
  rooms: number;
  floor: number | null;
  floorCount: number | null;
  buildYear: number | null;
  errors: Record<string, string>;
  onSquareMetersChange: (value: number) => void;
  onRoomsChange: (value: number) => void;
  onFloorChange: (value: number | null) => void;
  onFloorCountChange: (value: number | null) => void;
  onBuildYearChange: (value: number | null) => void;
}

export const ListingFormPropertyDetails = ({
  squareMeters,
  rooms,
  floor,
  floorCount,
  buildYear,
  errors,
  onSquareMetersChange,
  onRoomsChange,
  onFloorChange,
  onFloorCountChange,
  onBuildYearChange,
}: ListingFormPropertyDetailsProps) => {
  return (
    <div className="listing-form__section">
      <h2>Property Details</h2>

      <div className="listing-form__field-row">
        <div className="listing-form__field">
          <label htmlFor="squareMeters">Square Meters *</label>
          <Input
            id="squareMeters"
            type="number"
            min="1"
            value={squareMeters || ''}
            onChange={(e) => onSquareMetersChange(parseFloat(e.target.value) || 0)}
            error={errors.squareMeters}
          />
        </div>

        <div className="listing-form__field">
          <label htmlFor="rooms">Rooms *</label>
          <Input
            id="rooms"
            type="number"
            min="1"
            value={rooms || ''}
            onChange={(e) => onRoomsChange(parseInt(e.target.value) || 0)}
            error={errors.rooms}
          />
        </div>
      </div>

      <div className="listing-form__field-row">
        <div className="listing-form__field">
          <label htmlFor="floor">Floor</label>
          <Input
            id="floor"
            type="number"
            value={floor || ''}
            onChange={(e) => onFloorChange(e.target.value ? parseInt(e.target.value) : null)}
            placeholder="Optional"
          />
        </div>

        <div className="listing-form__field">
          <label htmlFor="floorCount">Total Floors</label>
          <Input
            id="floorCount"
            type="number"
            value={floorCount || ''}
            onChange={(e) => onFloorCountChange(e.target.value ? parseInt(e.target.value) : null)}
            placeholder="Optional"
          />
        </div>

        <div className="listing-form__field">
          <label htmlFor="buildYear">Build Year</label>
          <Input
            id="buildYear"
            type="number"
            min="1800"
            max={new Date().getFullYear()}
            value={buildYear || ''}
            onChange={(e) => onBuildYearChange(e.target.value ? parseInt(e.target.value) : null)}
            placeholder="Optional"
          />
        </div>
      </div>
    </div>
  );
};



