import { Button, LoadingSpinner, Pagination } from '../../shared';
import { useAdminUsersPage } from './hooks/useAdminUsersPage';
import {
  AdminUsersPageHeader,
  AdminUsersFilters,
  AdminUsersTable,
  AdminUsersModals,
} from './components';
import './AdminUsersPage.css';

export const AdminUsersPage = () => {
  const {
    canManageUsers,
    page,
    search,
    includeDeleted,
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
    setIncludeDeleted,
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
  } = useAdminUsersPage();

  if (!canManageUsers) {
    return null;
  }

  return (
    <div className="admin-users-page">
      <AdminUsersPageHeader />

      <AdminUsersFilters
        search={search}
        includeDeleted={includeDeleted}
        onSearchChange={setSearch}
        onIncludeDeletedChange={setIncludeDeleted}
      />

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
          <AdminUsersTable
            users={filteredUsers}
            currentUserId={currentUser?.id}
            actionsLoading={actionsLoading}
            onRemoveRole={handleRemoveRole}
            onOpenRoleModal={handleOpenRoleModal}
            onOpenSuspendModal={handleOpenSuspendModal}
            onActivate={handleActivate}
            onOpenDeleteModal={handleOpenDeleteModal}
          />

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

      <AdminUsersModals
        showRoleModal={showRoleModal}
        showSuspendModal={showSuspendModal}
        showDeleteModal={showDeleteModal}
        roleToAssign={roleToAssign}
        suspendReason={suspendReason}
        actionsLoading={actionsLoading}
        onRoleModalClose={() => setShowRoleModal(false)}
        onSuspendModalClose={() => setShowSuspendModal(false)}
        onDeleteModalClose={() => setShowDeleteModal(false)}
        onRoleToAssignChange={setRoleToAssign}
        onSuspendReasonChange={setSuspendReason}
        onAssignRole={handleAssignRole}
        onSuspend={handleSuspend}
        onDelete={handleDelete}
      />
    </div>
  );
};

