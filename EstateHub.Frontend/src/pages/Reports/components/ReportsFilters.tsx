import { Button } from '../../../shared';
import { REPORT_STATUS_LABELS, type ReportStatus } from '../../../entities/report/model/types';

interface ReportsFiltersProps {
  status: ReportStatus | null;
  onStatusChange: (status: ReportStatus | null) => void;
  onClear: () => void;
  disabled: boolean;
}

export const ReportsFilters = ({ status, onStatusChange, onClear, disabled }: ReportsFiltersProps) => (
  <div className="reports-page__filters">
    <select
      value={status || ''}
      onChange={(e) => onStatusChange(e.target.value ? (e.target.value as ReportStatus) : null)}
      disabled={disabled}
    >
      <option value="">All Statuses</option>
      {Object.entries(REPORT_STATUS_LABELS).map(([value, label]) => (
        <option key={value} value={value}>
          {label}
        </option>
      ))}
    </select>
    <Button variant="outline" onClick={onClear} disabled={disabled}>
      Clear Filters
    </Button>
  </div>
);

