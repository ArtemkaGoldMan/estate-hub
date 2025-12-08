import { Button } from '../../../shared';

type TabType = 'my-reports' | 'all-reports' | 'moderation-queue';

interface ReportsTabsProps {
  activeTab: TabType;
  setActiveTab: (tab: TabType) => void;
  canViewAllReports: boolean;
  canManageReports: boolean;
}

export const ReportsTabs = ({ activeTab, setActiveTab, canViewAllReports, canManageReports }: ReportsTabsProps) => (
  <div className="reports-page__tabs">
    <Button
      variant={activeTab === 'my-reports' ? 'primary' : 'outline'}
      onClick={() => setActiveTab('my-reports')}
    >
      My Reports
    </Button>
    {canViewAllReports && (
      <Button
        variant={activeTab === 'all-reports' ? 'primary' : 'outline'}
        onClick={() => setActiveTab('all-reports')}
      >
        All Reports
      </Button>
    )}
    {canManageReports && (
      <Button
        variant={activeTab === 'moderation-queue' ? 'primary' : 'outline'}
        onClick={() => setActiveTab('moderation-queue')}
      >
        Moderation Queue
      </Button>
    )}
  </div>
);

