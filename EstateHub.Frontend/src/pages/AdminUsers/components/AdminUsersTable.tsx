import { Button } from '../../../shared';
import type { AdminUser } from '../../../shared/api/admin';
import './AdminUsersTable.css';

interface AdminUsersTableProps {
  users: AdminUser[];
  currentUserId: string | undefined;
  actionsLoading: boolean;
  onRemoveRole: (userId: string, role: string) => void;
  onOpenRoleModal: (userId: string) => void;
  onOpenSuspendModal: (userId: string) => void;
  onActivate: (userId: string) => void;
  onOpenDeleteModal: (userId: string) => void;
}

export const AdminUsersTable = ({
  users,
  currentUserId,
  actionsLoading,
  onRemoveRole,
  onOpenRoleModal,
  onOpenSuspendModal,
  onActivate,
  onOpenDeleteModal,
}: AdminUsersTableProps) => {
  if (users.length === 0) {
    return (
      <div className="admin-users-page__table-container">
        <table className="admin-users-page__table">
          <thead>
            <tr>
              <th>Email</th>
              <th>Display Name</th>
              <th>Username</th>
              <th>Roles</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td colSpan={6} className="admin-users-page__empty">
                No users found
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    );
  }

  return (
    <div className="admin-users-page__table-container">
      <table className="admin-users-page__table">
        <thead>
          <tr>
            <th>Email</th>
            <th>Display Name</th>
            <th>Username</th>
            <th>Roles</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {users.map((user) => (
            <tr key={user.id}>
              <td>{user.email}</td>
              <td>{user.displayName}</td>
              <td>{user.userName}</td>
              <td>
                <div className="admin-users-page__roles">
                  {user.roles && user.roles.length > 0 ? (
                    <>
                      {user.roles.map((role) => (
                        <span key={role} className="admin-users-page__role-badge">
                          {role}
                          {!(currentUserId && user.id === currentUserId && role === 'Admin') && (
                            <button
                              type="button"
                              onClick={() => onRemoveRole(user.id, role)}
                              className="admin-users-page__role-remove"
                              title={`Remove ${role} role`}
                            >
                              ×
                            </button>
                          )}
                        </span>
                      ))}
                    </>
                  ) : (
                    <span className="admin-users-page__no-roles">No roles</span>
                  )}
                  <Button
                    size="xs"
                    variant="ghost"
                    onClick={() => onOpenRoleModal(user.id)}
                  >
                    + Add Role
                  </Button>
                </div>
              </td>
              <td>
                {user.isDeleted ? (
                  <span className="admin-users-page__status-badge admin-users-page__status-badge--deleted">
                    Deleted
                  </span>
                ) : (
                  <span className="admin-users-page__status-badge admin-users-page__status-badge--active">
                    Active
                  </span>
                )}
              </td>
              <td>
                <div className="admin-users-page__actions">
                  {!user.isDeleted && (
                    <>
                      <Button
                        size="xs"
                        variant="outline"
                        onClick={() => onOpenSuspendModal(user.id)}
                        disabled={actionsLoading}
                      >
                        Suspend
                      </Button>
                      <Button
                        size="xs"
                        variant="outline"
                        onClick={() => onActivate(user.id)}
                        disabled={actionsLoading}
                      >
                        Activate
                      </Button>
                    </>
                  )}
                  <Button
                    size="xs"
                    variant="danger"
                    onClick={() => onOpenDeleteModal(user.id)}
                    disabled={actionsLoading}
                  >
                    Delete
                  </Button>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

