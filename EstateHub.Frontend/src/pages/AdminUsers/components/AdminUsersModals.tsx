import { Button } from '../../../shared';
import './AdminUsersModals.css';

const ROLES = ['Admin', 'User'];

interface AdminUsersModalsProps {
  showRoleModal: boolean;
  showSuspendModal: boolean;
  showDeleteModal: boolean;
  roleToAssign: string;
  suspendReason: string;
  actionsLoading: boolean;
  onRoleModalClose: () => void;
  onSuspendModalClose: () => void;
  onDeleteModalClose: () => void;
  onRoleToAssignChange: (value: string) => void;
  onSuspendReasonChange: (value: string) => void;
  onAssignRole: () => void;
  onSuspend: () => void;
  onDelete: () => void;
}

export const AdminUsersModals = ({
  showRoleModal,
  showSuspendModal,
  showDeleteModal,
  roleToAssign,
  suspendReason,
  actionsLoading,
  onRoleModalClose,
  onSuspendModalClose,
  onDeleteModalClose,
  onRoleToAssignChange,
  onSuspendReasonChange,
  onAssignRole,
  onSuspend,
  onDelete,
}: AdminUsersModalsProps) => {
  return (
    <>
      {/* Assign Role Modal */}
      {showRoleModal && (
        <div className="admin-users-page__modal-overlay" onClick={onRoleModalClose}>
          <div className="admin-users-page__modal" onClick={(e) => e.stopPropagation()}>
            <h2>Assign Role</h2>
            <div className="admin-users-page__modal-content">
              <label>
                Role:
                <select
                  value={roleToAssign}
                  onChange={(e) => onRoleToAssignChange(e.target.value)}
                  className="admin-users-page__select"
                >
                  <option value="">Select a role...</option>
                  {ROLES.map((role) => (
                    <option key={role} value={role}>
                      {role}
                    </option>
                  ))}
                </select>
              </label>
            </div>
            <div className="admin-users-page__modal-actions">
              <Button variant="ghost" onClick={onRoleModalClose}>
                Cancel
              </Button>
              <Button
                variant="primary"
                onClick={onAssignRole}
                disabled={!roleToAssign || actionsLoading}
              >
                Assign Role
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* Suspend User Modal */}
      {showSuspendModal && (
        <div className="admin-users-page__modal-overlay" onClick={onSuspendModalClose}>
          <div className="admin-users-page__modal" onClick={(e) => e.stopPropagation()}>
            <h2>Suspend User</h2>
            <div className="admin-users-page__modal-content">
              <label>
                Reason:
                <textarea
                  value={suspendReason}
                  onChange={(e) => onSuspendReasonChange(e.target.value)}
                  placeholder="Enter reason for suspension..."
                  rows={4}
                  className="admin-users-page__textarea"
                />
              </label>
            </div>
            <div className="admin-users-page__modal-actions">
              <Button variant="ghost" onClick={onSuspendModalClose}>
                Cancel
              </Button>
              <Button
                variant="danger"
                onClick={onSuspend}
                disabled={!suspendReason.trim() || actionsLoading}
              >
                Suspend User
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* Delete User Modal */}
      {showDeleteModal && (
        <div className="admin-users-page__modal-overlay" onClick={onDeleteModalClose}>
          <div className="admin-users-page__modal" onClick={(e) => e.stopPropagation()}>
            <h2>Delete User</h2>
            <div className="admin-users-page__modal-content">
              <p>Are you sure you want to permanently delete this user? This action cannot be undone.</p>
            </div>
            <div className="admin-users-page__modal-actions">
              <Button variant="ghost" onClick={onDeleteModalClose}>
                Cancel
              </Button>
              <Button variant="danger" onClick={onDelete} disabled={actionsLoading}>
                Delete User
              </Button>
            </div>
          </div>
        </div>
      )}
    </>
  );
};



