import { sanitizeHtml } from '../../../../shared';
import type { Listing } from '../../../../entities/listing';
import './ListingDetailDescription.css';

interface ListingDetailDescriptionProps {
  listing: Listing;
}

export const ListingDetailDescription = ({ listing }: ListingDetailDescriptionProps) => {
  return (
    <div className="listing-detail-description">
      <h2>Description</h2>
      <div
        className="listing-detail-description__content"
        dangerouslySetInnerHTML={{
          __html: sanitizeHtml(listing.description || '<p>No description provided.</p>'),
        }}
      />
    </div>
  );
};

