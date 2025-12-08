import { Button } from '../../../shared';
import './ProfilePageHeader.css';

interface ProfilePageHeaderProps {
  onBack: () => void;
}

export const ProfilePageHeader = ({ onBack }: ProfilePageHeaderProps) => {
  return (
    <div className="profile-page__header">
      <h1>Profile & Settings</h1>
      <Button variant="ghost" onClick={onBack}>
        ← Back to Dashboard
      </Button>
    </div>
  );
};



