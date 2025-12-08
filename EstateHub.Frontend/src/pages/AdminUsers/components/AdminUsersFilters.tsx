import { Input } from '../../../shared';
import './AdminUsersFilters.css';

interface AdminUsersFiltersProps {
  search: string;
  includeDeleted: boolean;
  onSearchChange: (value: string) => void;
  onIncludeDeletedChange: (checked: boolean) => void;
}

export const AdminUsersFilters = ({
  search,
  includeDeleted,
  onSearchChange,
  onIncludeDeletedChange,
}: AdminUsersFiltersProps) => {
  return (
    <div className="admin-users-page__filters">
      <Input
        type="text"
        placeholder="Search by email, username, or display name..."
        value={search}
        onChange={(e) => onSearchChange(e.target.value)}
        className="admin-users-page__search"
      />
      <label className="admin-users-page__checkbox-label">
        <input
          type="checkbox"
          checked={includeDeleted}
          onChange={(e) => onIncludeDeletedChange(e.target.checked)}
        />
        Include deleted users
      </label>
    </div>
  );
};



