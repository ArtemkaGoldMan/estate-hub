import { Button } from '../../../shared';
import './ProfileDangerZone.css';

interface ProfileDangerZoneProps {
  onDeleteClick: () => void;
}

export const ProfileDangerZone = ({ onDeleteClick }: ProfileDangerZoneProps) => {
  return (
    <div className="profile-page__section profile-page__section--danger">
      <h2>Danger Zone</h2>
      <p className="profile-page__danger-text">
        Once you delete your account, there is no going back. Please be certain.
      </p>
      <Button
        variant="danger"
        onClick={onDeleteClick}
        className="profile-page__delete-button"
      >
        Delete Account
      </Button>
    </div>
  );
};



