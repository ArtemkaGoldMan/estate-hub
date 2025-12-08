import { Marker, useMapEvents } from 'react-leaflet';

interface LocationPickerProps {
  position: [number, number] | null;
  onPositionChange: (lat: number, lng: number) => void;
}

export const LocationPicker = ({ position, onPositionChange }: LocationPickerProps) => {
  useMapEvents({
    click(e) {
      onPositionChange(e.latlng.lat, e.latlng.lng);
    },
  });

  return position ? <Marker position={position} /> : null;
};



