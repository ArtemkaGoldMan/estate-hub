import { Button, LoadingSpinner } from '../../shared';
import { useProfilePage } from './hooks/useProfilePage';
import {
  ProfilePageHeader,
  ProfileAvatarSection,
  ProfileFormFields,
  ProfileActions,
  ProfileDangerZone,
  ProfileDeleteModal,
} from './components';
import './ProfilePage.css';

export const ProfilePage = () => {
  const {
    profile,
    loading,
    saving,
    error,
    deleting,
    isAuthenticated,
    user,
    displayName,
    phoneNumber,
    country,
    city,
    address,
    postalCode,
    companyName,
    website,
    avatarPreview,
    avatarFile,
    showDeleteModal,
    hasChanges,
    setDisplayName,
    setPhoneNumber,
    setCountry,
    setCity,
    setAddress,
    setPostalCode,
    setCompanyName,
    setWebsite,
    handleAvatarChange,
    handleCancelAvatar,
    handleSave,
    handleCancel,
    handleDeleteAccount,
    setShowDeleteModal,
    navigate,
  } = useProfilePage();

  if (!isAuthenticated || !user) {
    return null;
  }

  if (loading) {
    return (
      <div className="profile-page">
        <div className="profile-page__loading">
          <LoadingSpinner text="Loading profile..." />
        </div>
      </div>
    );
  }

  if (!profile) {
    return (
      <div className="profile-page">
        <div className="profile-page__error">
          <h2>Failed to load profile</h2>
          <p>{error || 'Please try again later.'}</p>
          <Button onClick={() => navigate('/dashboard')}>Back to Dashboard</Button>
        </div>
      </div>
    );
  }

  return (
    <div className="profile-page">
      <ProfilePageHeader onBack={() => navigate('/dashboard')} />

      {error && (
        <div className="profile-page__error-banner">
          <p>{error}</p>
        </div>
      )}

      <div className="profile-page__content">
        <div className="profile-page__section">
          <h2>Profile Information</h2>

          <ProfileAvatarSection
            displayName={displayName}
            avatarPreview={avatarPreview}
            avatarFile={avatarFile}
            onAvatarChange={handleAvatarChange}
            onCancelAvatar={handleCancelAvatar}
          />

          <ProfileFormFields
            profile={profile}
            displayName={displayName}
            phoneNumber={phoneNumber}
            country={country}
            city={city}
            address={address}
            postalCode={postalCode}
            companyName={companyName}
            website={website}
            onDisplayNameChange={setDisplayName}
            onPhoneNumberChange={setPhoneNumber}
            onCountryChange={setCountry}
            onCityChange={setCity}
            onAddressChange={setAddress}
            onPostalCodeChange={setPostalCode}
            onCompanyNameChange={setCompanyName}
            onWebsiteChange={setWebsite}
          />
        </div>

        <div className="profile-page__section">
          <h2>Account Actions</h2>
          <ProfileActions
            saving={saving}
            hasChanges={hasChanges}
            onSave={handleSave}
            onCancel={handleCancel}
          />
        </div>

        <ProfileDangerZone onDeleteClick={() => setShowDeleteModal(true)} />
      </div>

      <ProfileDeleteModal
        isOpen={showDeleteModal}
        deleting={deleting}
        onClose={() => setShowDeleteModal(false)}
        onConfirm={handleDeleteAccount}
      />
    </div>
  );
};

