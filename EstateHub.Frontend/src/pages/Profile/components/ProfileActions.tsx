import { Button } from '../../../shared';
import './ProfileActions.css';

interface ProfileActionsProps {
  saving: boolean;
  hasChanges: boolean;
  onSave: () => void;
  onCancel: () => void;
}

export const ProfileActions = ({
  saving,
  hasChanges,
  onSave,
  onCancel,
}: ProfileActionsProps) => {
  return (
    <div className="profile-page__actions">
      <Button
        variant="primary"
        onClick={onSave}
        disabled={saving || !hasChanges}
        isLoading={saving}
      >
        {saving ? 'Saving...' : 'Save Changes'}
      </Button>

      <Button
        variant="outline"
        onClick={onCancel}
        disabled={saving}
      >
        Cancel
      </Button>
    </div>
  );
};



