import { Icon } from 'leaflet';

/**
 * Initialize Leaflet default marker icons
 * Call this once at app startup to fix default marker icon issues
 */
export const setupLeafletIcons = () => {
  // Fix for default marker icon in react-leaflet
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  delete (Icon.Default.prototype as any)._getIconUrl;
  Icon.Default.mergeOptions({
    iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png',
    iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
    shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
  });
};



