/**
 * Report reason enum (matching backend ReportReason.cs)
 * HotChocolate converts C# enum names to UPPERCASE with underscores
 */
export type ReportReason =
  | 'INAPPROPRIATE_CONTENT'
  | 'SPAM'
  | 'FAKE_LISTING'
  | 'WRONG_CATEGORY'
  | 'DUPLICATE_LISTING'
  | 'OUTDATED_INFORMATION'
  | 'OTHER';

/**
 * Report status enum (matching backend ReportStatus.cs)
 * HotChocolate converts C# enum names to UPPERCASE
 */
export type ReportStatus =
  | 'PENDING'
  | 'UNDERREVIEW'
  | 'RESOLVED'
  | 'DISMISSED'
  | 'CLOSED';

/**
 * Report interface (matching backend ReportType.cs)
 */
export interface Report {
  id: string;
  reporterId: string;
  listingId: string;
  reason: ReportReason;
  description: string;
  status: ReportStatus;
  moderatorId?: string | null;
  moderatorNotes?: string | null;
  resolution?: string | null;
  createdAt: string;
  updatedAt: string;
  resolvedAt?: string | null;
  reporterEmail?: string | null;
  moderatorEmail?: string | null;
  listingTitle?: string | null;
}

/**
 * Paged reports response
 */
export interface PagedReports {
  items: Report[];
  total: number;
}

/**
 * Create report input
 */
export interface CreateReportInput {
  listingId: string;
  reason: ReportReason;
  description: string;
}

/**
 * Resolve report input
 */
export interface ResolveReportInput {
  reportId: string;
  resolution: string;
  moderatorNotes?: string | null;
}

/**
 * Dismiss report input
 */
export interface DismissReportInput {
  reportId: string;
  moderatorNotes?: string | null;
}

/**
 * Report filter
 */
export interface ReportFilter {
  status?: ReportStatus | null;
  reason?: ReportReason | null;
  reporterId?: string | null;
  moderatorId?: string | null;
  createdFrom?: string | null;
  createdTo?: string | null;
}

/**
 * Report reason labels
 */
export const REPORT_REASON_LABELS: Record<ReportReason, string> = {
  INAPPROPRIATE_CONTENT: 'Inappropriate Content',
  SPAM: 'Spam',
  FAKE_LISTING: 'Fake Listing',
  WRONG_CATEGORY: 'Wrong Category',
  DUPLICATE_LISTING: 'Duplicate Listing',
  OUTDATED_INFORMATION: 'Outdated Information',
  OTHER: 'Other',
};

/**
 * Report status labels
 */
export const REPORT_STATUS_LABELS: Record<ReportStatus, string> = {
  PENDING: 'Pending',
  UNDERREVIEW: 'Under Review',
  RESOLVED: 'Resolved',
  DISMISSED: 'Dismissed',
  CLOSED: 'Closed',
};

/**
 * Report status colors for UI
 */
export const REPORT_STATUS_COLORS: Record<ReportStatus, string> = {
  PENDING: '#f59e0b', // amber
  UNDERREVIEW: '#3b82f6', // blue
  RESOLVED: '#10b981', // green
  DISMISSED: '#6b7280', // gray
  CLOSED: '#1f2937', // dark gray
};

