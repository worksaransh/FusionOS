import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../../../shared/api/client';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import type { PagedResult } from '../../../shared/api/types';

const UPCOMING_PAGE_SIZE = 100;
const OPEN_REQUESTS_PAGE_SIZE = 200;
const EXPIRING_WARRANTY_WITHIN_DAYS = 30;

interface MaintenanceRequestLite {
  id: string;
  status: string;
}

interface MaintenanceScheduleLite {
  id: string;
  assetId: string;
  description: string;
  frequency: string;
  nextDueDate: string;
  isActive: boolean;
}

interface AssetWithWarrantyLite {
  id: string;
  code: string;
  name: string;
  warrantyExpirationDate: string | null;
  isUnderWarranty: boolean;
}

/**
 * Maintenance Dashboard — a due-date-sorted upcoming-maintenance list plus a
 * handful of KPI tiles (open requests, overdue schedules, expiring
 * warranties). Scope note: this is deliberately NOT a calendar-grid widget —
 * a full month/week calendar visualization is out of scope for this pass;
 * "upcoming maintenance" here means a plain list of active
 * MaintenanceSchedules sorted by NextDueDate, using the existing
 * MaintenanceSchedulesController.List `dueFilter` param for the overdue
 * count. Composed entirely from existing endpoints (Assets/expiring-warranty,
 * MaintenanceRequests, MaintenanceSchedules) — no new aggregating backend
 * query was added since three small client-side fetches is no less clean
 * here. Rendered as the FIRST sibling panel under AssetsPage (dashboards
 * conventionally go at the top).
 *
 * Open-requests / overdue-schedules counts are derived client-side from a
 * single generously-paged fetch each, since neither ListMaintenanceRequestsQuery
 * nor the schedule list expose a small in the response — same
 * pragmatic one-page approach used elsewhere in this codebase
 * (see useEntityOptionsWithoutSearch's doc comment) for a company's
 * currently-modest request/schedule volume.
 */
export function MaintenanceDashboardPanel() {
  const { companyId } = useActiveCompany();

  const requestsQuery = useQuery({
    queryKey: ['maintenance-dashboard-requests', companyId],
    queryFn: () =>
      apiClient.get<PagedResult<MaintenanceRequestLite>>(
        `/maintenance/requests?companyId=${companyId}&page=1&pageSize=${OPEN_REQUESTS_PAGE_SIZE}`,
      ),
    enabled: Boolean(companyId),
  });

  const schedulesQuery = useQuery({
    queryKey: ['maintenance-dashboard-schedules', companyId],
    queryFn: () =>
      apiClient.get<PagedResult<MaintenanceScheduleLite>>(
        `/maintenance/schedules?companyId=${companyId}&page=1&pageSize=${UPCOMING_PAGE_SIZE}`,
      ),
    enabled: Boolean(companyId),
  });

  const overdueSchedulesQuery = useQuery({
    queryKey: ['maintenance-dashboard-overdue-schedules', companyId],
    queryFn: () =>
      apiClient.get<PagedResult<MaintenanceScheduleLite>>(
        `/maintenance/schedules?companyId=${companyId}&dueFilter=Overdue&page=1&pageSize=1`,
      ),
    enabled: Boolean(companyId),
  });

  const expiringWarrantyQuery = useQuery({
    queryKey: ['maintenance-dashboard-expiring-warranty', companyId],
    queryFn: () =>
      apiClient.get<AssetWithWarrantyLite[]>(
        `/maintenance/assets/expiring-warranty?companyId=${companyId}&withinDays=${EXPIRING_WARRANTY_WITHIN_DAYS}`,
      ),
    enabled: Boolean(companyId),
  });

  if (!companyId) return null;

  const openRequestsCount = requestsQuery.data?.data.filter((r) => r.status !== 'Completed').length ?? 0;
  const overdueSchedulesCount = overdueSchedulesQuery.data?.totalCount ?? 0;
  const expiringWarrantyCount = expiringWarrantyQuery.data?.length ?? 0;

  const upcomingSchedules = (schedulesQuery.data?.data ?? [])
    .filter((s) => s.isActive)
    .slice()
    .sort((a, b) => new Date(a.nextDueDate).getTime() - new Date(b.nextDueDate).getTime());

  return (
    <div>
      <h1 className="mb-1 text-xl font-semibold text-text">Maintenance Dashboard</h1>
      <p className="mb-4 text-sm text-text-muted">
        A due-date-sorted upcoming maintenance list, not a calendar grid — see this panel's doc comment for scope.
      </p>

      <div className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-3">
        <KpiTile
          label="Open maintenance requests"
          value={requestsQuery.isLoading ? '…' : openRequestsCount}
          tone="default"
        />
        <KpiTile
          label="Overdue schedules"
          value={overdueSchedulesQuery.isLoading ? '…' : overdueSchedulesCount}
          tone={overdueSchedulesCount > 0 ? 'danger' : 'default'}
        />
        <KpiTile
          label={`Warranties expiring (${EXPIRING_WARRANTY_WITHIN_DAYS}d)`}
          value={expiringWarrantyQuery.isLoading ? '…' : expiringWarrantyCount}
          tone={expiringWarrantyCount > 0 ? 'warning' : 'default'}
        />
      </div>

      <Card>
        <h2 className="mb-3 text-lg font-semibold text-text">Upcoming maintenance</h2>
        <DataTable
          columns={[
            { header: 'Description', render: (row: MaintenanceScheduleLite) => row.description },
            { header: 'Frequency', render: (row: MaintenanceScheduleLite) => row.frequency },
            { header: 'Next due', render: (row: MaintenanceScheduleLite) => new Date(row.nextDueDate).toLocaleDateString() },
          ]}
          rows={upcomingSchedules}
          isLoading={schedulesQuery.isLoading}
          isError={schedulesQuery.isError}
          errorMessage="Could not load upcoming maintenance."
          emptyMessage="No active maintenance schedules yet."
          rowKey={(row) => row.id}
        />
      </Card>
    </div>
  );
}

interface KpiTileProps {
  label: string;
  value: number | string;
  tone: 'default' | 'warning' | 'danger';
}

function KpiTile({ label, value, tone }: KpiTileProps) {
  const valueClass = tone === 'danger' ? 'text-danger' : tone === 'warning' ? 'text-warning' : 'text-text';

  return (
    <Card>
      <p className="mb-1 text-sm text-text-muted">{label}</p>
      <p className={`text-2xl font-semibold ${valueClass}`}>{value}</p>
    </Card>
  );
}
