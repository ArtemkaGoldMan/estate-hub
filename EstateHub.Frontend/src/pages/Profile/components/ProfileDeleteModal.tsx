import { Button, Modal } from '../../../shared';
import './ProfileDeleteModal.css';

interface ProfileDeleteModalProps {
  isOpen: boolean;
  deleting: boolean;
  onClose: () => void;
  onConfirm: () => void;
}

export const ProfileDeleteModal = ({
  isOpen,
  deleting,
  onClose,
  onConfirm,
}: ProfileDeleteModalProps) => {
  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Delete Account"
    >
      <div className="profile-page__delete-modal">
        <p>Are you sure you want to delete your account? This action cannot be undone.</p>
        <div className="profile-page__delete-modal-actions">
          <Button
            variant="outline"
            onClick={onClose}
            disabled={deleting}
          >
            Cancel
          </Button>
          <Button
            variant="danger"
            onClick={onConfirm}
            disabled={deleting}
            isLoading={deleting}
          >
            {deleting ? 'Deleting...' : 'Delete Account'}
          </Button>
        </div>
      </div>
    </Modal>
  );
};



