import { useState, useCallback } from 'react';
import { useToast } from '../../shared/context/ToastContext';
import { useAdminUsers, useAdminUserStats, useAdminUserActions } from '../../shared/api/admin';
import { getUserRoles, hasPermission, PERMISSIONS } from '../../shared/lib/permissions';
import { UserFriendlyError } from '../../shared/lib/errorParser';
import { Button, LoadingSpinner, Pagination, Input } from '../../shared/ui';
import './AdminUsersPage.css';

const PAGE_SIZE = 20;
const ROLES = ['Admin', 'User'];

export const AdminUsersPage = () => {
  const { showSuccess, showError } = useToast();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [includeDeleted, setIncludeDeleted] = useState(false);
  const [selectedUser, setSelectedUser] = useState<string | null>(null);
  const [showSuspendModal, setShowSuspendModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [suspendReason, setSuspendReason] = useState('');
  const [roleToAssign, setRoleToAssign] = useState('');
  const [showRoleModal, setShowRoleModal] = useState(false);

  const userRoles = getUserRoles();
  const canManageUsers = hasPermission(userRoles, PERMISSIONS.UserManagement);

  // Note: Route protection is now handled by AdminRoute component

  const { data: usersData, loading: usersLoading, error: usersError, refetch: refetchUsers } =
    useAdminUsers(page, PAGE_SIZE, includeDeleted);

  const { data: statsData, refetch: refetchStats } = useAdminUserStats();

  const {
    assignRole,
    removeRole,
    suspendUser,
    activateUser,
    deleteUser,
    loading: actionsLoading,
  } = useAdminUserActions();

  // Filter users by search
  const filteredUsers = usersData?.items.filter((user) => {
    if (!search.trim()) return true;
    const searchLower = search.toLowerCase();
    return (
      user.email.toLowerCase().includes(searchLower) ||
      user.userName.toLowerCase().includes(searchLower) ||
      user.displayName.toLowerCase().includes(searchLower)
    );
  }) || [];

  const handleAssignRole = useCallback(async () => {
    if (!selectedUser || !roleToAssign) return;

    try {
      await assignRole(selectedUser, roleToAssign);
      setShowRoleModal(false);
      setRoleToAssign('');
      setSelectedUser(null);
      refetchUsers();
      showSuccess(`Role "${roleToAssign}" assigned successfully`);
    } catch (error) {
      if (error instanceof UserFriendlyError) {
        showError(error.userMessage);
      } else {
        showError(error instanceof Error ? error.message : 'Failed to assign role');
      }
    }
  }, [selectedUser, roleToAssign, assignRole, refetchUsers]);

  const handleRemoveRole = useCallback(async (userId: string, role: string) => {
    if (!confirm(`Are you sure you want to remove the "${role}" role from this user?`)) {
      return;
    }

    try {
      await removeRole(userId, role);
      refetchUsers();
      showSuccess(`Role "${role}" removed successfully`);
    } catch (error) {
      if (error instanceof UserFriendlyError) {
        showError(error.userMessage);
      } else {
        showError(error instanceof Error ? error.message : 'Failed to remove role');
      }
    }
  }, [removeRole, refetchUsers]);

  const handleSuspend = useCallback(async () => {
    if (!selectedUser || !suspendReason.trim()) return;

    try {
      await suspendUser(selectedUser, suspendReason.trim());
      setShowSuspendModal(false);
      setSuspendReason('');
      setSelectedUser(null);
      refetchUsers();
      refetchStats();
      showSuccess('User suspended successfully');
    } catch (error) {
      if (error instanceof UserFriendlyError) {
        showError(error.userMessage);
      } else {
        showError(error instanceof Error ? error.message : 'Failed to suspend user');
      }
    }
  }, [selectedUser, suspendReason, suspendUser, refetchUsers, refetchStats]);

  const handleActivate = useCallback(async (userId: string) => {
    if (!confirm('Are you sure you want to activate this user?')) {
      return;
    }

    try {
      await activateUser(userId);
      refetchUsers();
      refetchStats();
      showSuccess('User activated successfully');
    } catch (error) {
      if (error instanceof UserFriendlyError) {
        showError(error.userMessage);
      } else {
        showError(error instanceof Error ? error.message : 'Failed to activate user');
      }
    }
  }, [activateUser, refetchUsers, refetchStats]);

  const handleDelete = useCallback(async () => {
    if (!selectedUser) return;

    try {
      await deleteUser(selectedUser);
      setShowDeleteModal(false);
      setSelectedUser(null);
      refetchUsers();
      refetchStats();
      showSuccess('User deleted successfully');
    } catch (error) {
      if (error instanceof UserFriendlyError) {
        showError(error.userMessage);
      } else {
        showError(error instanceof Error ? error.message : 'Failed to delete user');
      }
    }
  }, [selectedUser, deleteUser, refetchUsers, refetchStats]);

  if (!canManageUsers) {
    return null;
  }

  return (
    <div className="admin-users-page">
      <div className="admin-users-page__header">
        <h1>User Management</h1>
        <p>Manage users, roles, and account status</p>
      </div>

      {/* Statistics Dashboard */}
      {statsData && (
        <div className="admin-users-page__stats">
          <div className="admin-users-page__stat-card">
            <div className="admin-users-page__stat-value">{statsData.totalUsers}</div>
            <div className="admin-users-page__stat-label">Total Users</div>
          </div>
          <div className="admin-users-page__stat-card">
            <div className="admin-users-page__stat-value">{statsData.activeUsers}</div>
            <div className="admin-users-page__stat-label">Active Users</div>
          </div>
          <div className="admin-users-page__stat-card">
            <div className="admin-users-page__stat-value">{statsData.suspendedUsers}</div>
            <div className="admin-users-page__stat-label">Suspended</div>
          </div>
          <div className="admin-users-page__stat-card">
            <div className="admin-users-page__stat-value">{statsData.newUsersThisMonth}</div>
            <div className="admin-users-page__stat-label">New This Month</div>
          </div>
        </div>
      )}

      {/* Filters */}
      <div className="admin-users-page__filters">
        <Input
          type="text"
          placeholder="Search by email, username, or display name..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="admin-users-page__search"
        />
        <label className="admin-users-page__checkbox-label">
          <input
            type="checkbox"
            checked={includeDeleted}
            onChange={(e) => {
              setIncludeDeleted(e.target.checked);
              setPage(1);
            }}
          />
          Include deleted users
        </label>
      </div>

      {/* Users Table */}
      {usersError && (
        <div className="admin-users-page__error">
          <p>Failed to load users: {usersError.message}</p>
          <Button onClick={() => refetchUsers()}>Retry</Button>
        </div>
      )}

      {usersLoading && (
        <div className="admin-users-page__loading">
          <LoadingSpinner />
          <p>Loading users...</p>
        </div>
      )}

      {!usersLoading && !usersError && (
        <>
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
                {filteredUsers.length === 0 ? (
                  <tr>
                    <td colSpan={6} className="admin-users-page__empty">
                      No users found
                    </td>
                  </tr>
                ) : (
                  filteredUsers.map((user) => (
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
                                  <button
                                    type="button"
                                    onClick={() => handleRemoveRole(user.id, role)}
                                    className="admin-users-page__role-remove"
                                    title={`Remove ${role} role`}
                                  >
                                    ×
                                  </button>
                                </span>
                              ))}
                            </>
                          ) : (
                            <span className="admin-users-page__no-roles">No roles</span>
                          )}
                          <Button
                            size="xs"
                            variant="ghost"
                            onClick={() => {
                              setSelectedUser(user.id);
                              setShowRoleModal(true);
                            }}
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
                                onClick={() => {
                                  setSelectedUser(user.id);
                                  setShowSuspendModal(true);
                                }}
                                disabled={actionsLoading}
                              >
                                Suspend
                              </Button>
                              <Button
                                size="xs"
                                variant="outline"
                                onClick={() => handleActivate(user.id)}
                                disabled={actionsLoading}
                              >
                                Activate
                              </Button>
                            </>
                          )}
                          <Button
                            size="xs"
                            variant="danger"
                            onClick={() => {
                              setSelectedUser(user.id);
                              setShowDeleteModal(true);
                            }}
                            disabled={actionsLoading}
                          >
                            Delete
                          </Button>
                        </div>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>

          {usersData && usersData.total > PAGE_SIZE && (
            <div className="admin-users-page__pagination">
              <Pagination
                currentPage={page}
                totalItems={usersData.total}
                pageSize={PAGE_SIZE}
                onPageChange={setPage}
                disabled={usersLoading}
              />
            </div>
          )}
        </>
      )}

      {/* Assign Role Modal */}
      {showRoleModal && (
        <div className="admin-users-page__modal-overlay" onClick={() => setShowRoleModal(false)}>
          <div className="admin-users-page__modal" onClick={(e) => e.stopPropagation()}>
            <h2>Assign Role</h2>
            <div className="admin-users-page__modal-content">
              <label>
                Role:
                <select
                  value={roleToAssign}
                  onChange={(e) => setRoleToAssign(e.target.value)}
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
              <Button variant="ghost" onClick={() => setShowRoleModal(false)}>
                Cancel
              </Button>
              <Button variant="primary" onClick={handleAssignRole} disabled={!roleToAssign || actionsLoading}>
                Assign Role
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* Suspend User Modal */}
      {showSuspendModal && (
        <div className="admin-users-page__modal-overlay" onClick={() => setShowSuspendModal(false)}>
          <div className="admin-users-page__modal" onClick={(e) => e.stopPropagation()}>
            <h2>Suspend User</h2>
            <div className="admin-users-page__modal-content">
              <label>
                Reason:
                <textarea
                  value={suspendReason}
                  onChange={(e) => setSuspendReason(e.target.value)}
                  placeholder="Enter reason for suspension..."
                  rows={4}
                  className="admin-users-page__textarea"
                />
              </label>
            </div>
            <div className="admin-users-page__modal-actions">
              <Button variant="ghost" onClick={() => setShowSuspendModal(false)}>
                Cancel
              </Button>
              <Button
                variant="danger"
                onClick={handleSuspend}
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
        <div className="admin-users-page__modal-overlay" onClick={() => setShowDeleteModal(false)}>
          <div className="admin-users-page__modal" onClick={(e) => e.stopPropagation()}>
            <h2>Delete User</h2>
            <div className="admin-users-page__modal-content">
              <p>Are you sure you want to permanently delete this user? This action cannot be undone.</p>
            </div>
            <div className="admin-users-page__modal-actions">
              <Button variant="ghost" onClick={() => setShowDeleteModal(false)}>
                Cancel
              </Button>
              <Button variant="danger" onClick={handleDelete} disabled={actionsLoading}>
                Delete User
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

