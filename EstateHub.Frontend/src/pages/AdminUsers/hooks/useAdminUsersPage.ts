import { useState, useCallback } from 'react';
import { useToast } from '../../../shared/context/ToastContext';
import { useAuth } from '../../../shared/context/AuthContext';
import { useAdminUsers, useAdminUserActions } from '../../../shared/api/admin';
import { getUserRoles, hasPermission, PERMISSIONS } from '../../../shared/lib/permissions';
import { UserFriendlyError } from '../../../shared/lib/errorParser';

const PAGE_SIZE = 20;

export const useAdminUsersPage = () => {
  const { showSuccess, showError } = useToast();
  const { user: currentUser } = useAuth();
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

  const { data: usersData, loading: usersLoading, error: usersError, refetch: refetchUsers } =
    useAdminUsers(page, PAGE_SIZE, includeDeleted);

  const {
    assignRole,
    removeRole,
    suspendUser,
    activateUser,
    deleteUser,
    loading: actionsLoading,
  } = useAdminUserActions();

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
  }, [selectedUser, roleToAssign, assignRole, refetchUsers, showSuccess, showError]);

  const handleRemoveRole = useCallback(async (userId: string, role: string) => {
    if (currentUser && userId === currentUser.id && role === 'Admin') {
      showError('You cannot remove your own Admin role');
      return;
    }

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
  }, [currentUser, removeRole, refetchUsers, showError, showSuccess]);

  const handleSuspend = useCallback(async () => {
    if (!selectedUser || !suspendReason.trim()) return;

    try {
      await suspendUser(selectedUser, suspendReason.trim());
      setShowSuspendModal(false);
      setSuspendReason('');
      setSelectedUser(null);
      refetchUsers();
      showSuccess('User suspended successfully');
    } catch (error) {
      if (error instanceof UserFriendlyError) {
        showError(error.userMessage);
      } else {
        showError(error instanceof Error ? error.message : 'Failed to suspend user');
      }
    }
  }, [selectedUser, suspendReason, suspendUser, refetchUsers, showSuccess, showError]);

  const handleActivate = useCallback(async (userId: string) => {
    if (!confirm('Are you sure you want to activate this user?')) {
      return;
    }

    try {
      await activateUser(userId);
      refetchUsers();
      showSuccess('User activated successfully');
    } catch (error) {
      if (error instanceof UserFriendlyError) {
        showError(error.userMessage);
      } else {
        showError(error instanceof Error ? error.message : 'Failed to activate user');
      }
    }
  }, [activateUser, refetchUsers, showSuccess, showError]);

  const handleDelete = useCallback(async () => {
    if (!selectedUser) return;

    try {
      await deleteUser(selectedUser);
      setShowDeleteModal(false);
      setSelectedUser(null);
      refetchUsers();
      showSuccess('User deleted successfully');
    } catch (error) {
      if (error instanceof UserFriendlyError) {
        showError(error.userMessage);
      } else {
        showError(error instanceof Error ? error.message : 'Failed to delete user');
      }
    }
  }, [selectedUser, deleteUser, refetchUsers, showSuccess, showError]);

  const handleIncludeDeletedChange = useCallback((checked: boolean) => {
    setIncludeDeleted(checked);
    setPage(1);
  }, []);

  const handleOpenRoleModal = useCallback((userId: string) => {
    setSelectedUser(userId);
    setShowRoleModal(true);
  }, []);

  const handleOpenSuspendModal = useCallback((userId: string) => {
    setSelectedUser(userId);
    setShowSuspendModal(true);
  }, []);

  const handleOpenDeleteModal = useCallback((userId: string) => {
    setSelectedUser(userId);
    setShowDeleteModal(true);
  }, []);

  return {
    canManageUsers,
    page,
    search,
    includeDeleted,
    selectedUser,
    showSuspendModal,
    showDeleteModal,
    showRoleModal,
    suspendReason,
    roleToAssign,
    usersData,
    usersLoading,
    usersError,
    actionsLoading,
    filteredUsers,
    currentUser,
    PAGE_SIZE,
    setPage,
    setSearch,
    setIncludeDeleted: handleIncludeDeletedChange,
    setSuspendReason,
    setRoleToAssign,
    setShowSuspendModal,
    setShowDeleteModal,
    setShowRoleModal,
    handleAssignRole,
    handleRemoveRole,
    handleSuspend,
    handleActivate,
    handleDelete,
    handleOpenRoleModal,
    handleOpenSuspendModal,
    handleOpenDeleteModal,
    refetchUsers,
  };
};



