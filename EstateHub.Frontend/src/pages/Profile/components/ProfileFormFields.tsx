import { Input } from '../../../shared';
import type { GetUserResponse } from '../../../shared/api/auth';
import './ProfileFormFields.css';

interface ProfileFormFieldsProps {
  profile: GetUserResponse;
  displayName: string;
  phoneNumber: string;
  country: string;
  city: string;
  address: string;
  postalCode: string;
  companyName: string;
  website: string;
  onDisplayNameChange: (value: string) => void;
  onPhoneNumberChange: (value: string) => void;
  onCountryChange: (value: string) => void;
  onCityChange: (value: string) => void;
  onAddressChange: (value: string) => void;
  onPostalCodeChange: (value: string) => void;
  onCompanyNameChange: (value: string) => void;
  onWebsiteChange: (value: string) => void;
}

export const ProfileFormFields = ({
  profile,
  displayName,
  phoneNumber,
  country,
  city,
  address,
  postalCode,
  companyName,
  website,
  onDisplayNameChange,
  onPhoneNumberChange,
  onCountryChange,
  onCityChange,
  onAddressChange,
  onPostalCodeChange,
  onCompanyNameChange,
  onWebsiteChange,
}: ProfileFormFieldsProps) => {
  return (
    <>
      <div className="profile-page__field">
        <label htmlFor="email">Email</label>
        <Input
          id="email"
          type="email"
          value={profile.email}
          disabled
          readOnly
        />
        <p className="profile-page__field-hint">Email cannot be changed</p>
      </div>

      <div className="profile-page__field">
        <label htmlFor="displayName">Display Name</label>
        <Input
          id="displayName"
          type="text"
          value={displayName}
          onChange={(e) => onDisplayNameChange(e.target.value)}
          placeholder="Enter your display name"
          maxLength={50}
        />
      </div>

      <div className="profile-page__field">
        <label htmlFor="phoneNumber">Phone Number</label>
        <Input
          id="phoneNumber"
          type="tel"
          value={phoneNumber}
          onChange={(e) => onPhoneNumberChange(e.target.value)}
          placeholder="Enter your phone number"
        />
      </div>

      <div className="profile-page__field">
        <label htmlFor="country">Country</label>
        <Input
          id="country"
          type="text"
          value={country}
          onChange={(e) => onCountryChange(e.target.value)}
          placeholder="Enter your country"
        />
      </div>

      <div className="profile-page__field">
        <label htmlFor="city">City</label>
        <Input
          id="city"
          type="text"
          value={city}
          onChange={(e) => onCityChange(e.target.value)}
          placeholder="Enter your city"
        />
      </div>

      <div className="profile-page__field">
        <label htmlFor="address">Address</label>
        <Input
          id="address"
          type="text"
          value={address}
          onChange={(e) => onAddressChange(e.target.value)}
          placeholder="Enter your address"
        />
      </div>

      <div className="profile-page__field">
        <label htmlFor="postalCode">Postal Code</label>
        <Input
          id="postalCode"
          type="text"
          value={postalCode}
          onChange={(e) => onPostalCodeChange(e.target.value)}
          placeholder="Enter your postal code"
        />
      </div>

      <div className="profile-page__field">
        <label htmlFor="companyName">Company Name</label>
        <Input
          id="companyName"
          type="text"
          value={companyName}
          onChange={(e) => onCompanyNameChange(e.target.value)}
          placeholder="Enter your company name"
        />
      </div>

      <div className="profile-page__field">
        <label htmlFor="website">Website</label>
        <Input
          id="website"
          type="url"
          value={website}
          onChange={(e) => onWebsiteChange(e.target.value)}
          placeholder="Enter your website URL"
        />
      </div>

      <div className="profile-page__field">
        <label>User ID</label>
        <Input
          type="text"
          value={profile.id}
          disabled
          readOnly
        />
      </div>
    </>
  );
};



