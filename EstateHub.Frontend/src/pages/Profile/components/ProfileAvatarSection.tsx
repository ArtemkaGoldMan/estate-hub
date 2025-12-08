import { Button } from '../../../shared';
import './ProfileAvatarSection.css';

interface ProfileAvatarSectionProps {
  displayName: string;
  avatarPreview: string | null;
  avatarFile: File | null;
  onAvatarChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onCancelAvatar: () => void;
}

export const ProfileAvatarSection = ({
  displayName,
  avatarPreview,
  avatarFile,
  onAvatarChange,
  onCancelAvatar,
}: ProfileAvatarSectionProps) => {
  return (
    <div className="profile-page__avatar-section">
      <div className="profile-page__avatar-preview">
        {avatarPreview ? (
          <img src={avatarPreview} alt="Avatar preview" />
        ) : (
          <div className="profile-page__avatar-placeholder">
            {displayName.charAt(0).toUpperCase()}
          </div>
        )}
      </div>
      <div className="profile-page__avatar-controls">
        <label htmlFor="avatar-upload" className="profile-page__avatar-label">
          <span className="profile-page__avatar-button">
            <Button variant="outline">Change Avatar</Button>
          </span>
          <input
            id="avatar-upload"
            type="file"
            accept="image/jpeg,image/jpg,image/png"
            onChange={onAvatarChange}
            style={{ display: 'none' }}
          />
        </label>
        {avatarFile && (
          <Button variant="ghost" size="sm" onClick={onCancelAvatar}>
            Cancel
          </Button>
        )}
      </div>
      <p className="profile-page__avatar-hint">
        JPG, PNG up to 2MB. Recommended: 32x32px minimum.
      </p>
    </div>
  );
};



