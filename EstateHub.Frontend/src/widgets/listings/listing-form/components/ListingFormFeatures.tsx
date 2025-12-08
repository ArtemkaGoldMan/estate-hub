interface ListingFormFeaturesProps {
  hasBalcony: boolean;
  hasElevator: boolean;
  hasParkingSpace: boolean;
  hasSecurity: boolean;
  hasStorageRoom: boolean;
  onFeatureChange: (feature: string, value: boolean) => void;
}

export const ListingFormFeatures = ({
  hasBalcony,
  hasElevator,
  hasParkingSpace,
  hasSecurity,
  hasStorageRoom,
  onFeatureChange,
}: ListingFormFeaturesProps) => {
  const features = [
    { key: 'hasBalcony', label: 'Balcony', value: hasBalcony },
    { key: 'hasElevator', label: 'Elevator', value: hasElevator },
    { key: 'hasParkingSpace', label: 'Parking Space', value: hasParkingSpace },
    { key: 'hasSecurity', label: 'Security', value: hasSecurity },
    { key: 'hasStorageRoom', label: 'Storage Room', value: hasStorageRoom },
  ];

  return (
    <div className="listing-form__section">
      <h2>Features</h2>
      <div className="listing-form__features">
        {features.map((feature) => (
          <label key={feature.key} className="listing-form__checkbox">
            <input
              type="checkbox"
              checked={feature.value}
              onChange={(e) => onFeatureChange(feature.key, e.target.checked)}
            />
            <span>{feature.label}</span>
          </label>
        ))}
      </div>
    </div>
  );
};



