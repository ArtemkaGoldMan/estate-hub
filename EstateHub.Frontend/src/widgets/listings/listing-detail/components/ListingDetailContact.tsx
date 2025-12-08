import { LoadingSpinner } from '../../../../shared/ui';
import type { GetUserResponse } from '../../../../shared/api/auth/userApi';
import './ListingDetailContact.css';

interface ListingDetailContactProps {
  isAuthenticated: boolean;
  loadingOwner: boolean;
  ownerInfo: GetUserResponse | null;
}

export const ListingDetailContact = ({
  isAuthenticated,
  loadingOwner,
  ownerInfo,
}: ListingDetailContactProps) => {
  return (
    <div className="listing-detail-contact">
      <h2>Contact Information</h2>
      {!isAuthenticated ? (
        <div className="listing-detail-contact__login-prompt">
          <p>Please <a href="/login">log in</a> to view contact information</p>
        </div>
      ) : loadingOwner ? (
        <div className="listing-detail-contact__loading">
          <LoadingSpinner text="Loading contact info..." />
        </div>
      ) : ownerInfo ? (
        <div className="listing-detail-contact__info">
          <div className="listing-detail-contact__item">
            <span className="listing-detail-contact__label">Name:</span>
            <span className="listing-detail-contact__value">
              {ownerInfo.displayName || ownerInfo.userName}
            </span>
          </div>
          {ownerInfo.phoneNumber ? (
            <div className="listing-detail-contact__item">
              <span className="listing-detail-contact__label">Phone:</span>
              <span className="listing-detail-contact__value">
                <a href={`tel:${ownerInfo.phoneNumber}`}>{ownerInfo.phoneNumber}</a>
              </span>
            </div>
          ) : (
            <div className="listing-detail-contact__item">
              <span className="listing-detail-contact__label">Phone:</span>
              <span className="listing-detail-contact__value--empty">Not provided</span>
            </div>
          )}
          {ownerInfo.email ? (
            <div className="listing-detail-contact__item">
              <span className="listing-detail-contact__label">Email:</span>
              <span className="listing-detail-contact__value">
                <a href={`mailto:${ownerInfo.email}`}>{ownerInfo.email}</a>
              </span>
            </div>
          ) : (
            <div className="listing-detail-contact__item">
              <span className="listing-detail-contact__label">Email:</span>
              <span className="listing-detail-contact__value--empty">Not provided</span>
            </div>
          )}
        </div>
      ) : (
        <div className="listing-detail-contact__error">
          <p>Contact information not available</p>
        </div>
      )}
    </div>
  );
};

