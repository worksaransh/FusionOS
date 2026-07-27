import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../../../shared/api/client';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { EntityCombobox } from '../../../shared/ui/EntityCombobox';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import { useAssetOptions } from '../../../shared/api/entityOptions';

interface DowntimeMetricsAssetLineDto {
  assetId: string;
  assetCode: string;
  assetName: string;
  breakdownCount: number;
  totalDowntimeMinutes: number;
  mttrMinutes: number | null;
  mtbfMinutes: number | null;
}

interface DowntimeMetricsReportDto {
  periodStart: string;
  periodEnd: string;
  lines: DowntimeMetricsAssetLineDto[];
  totalBreakdownCount: number;
  totalDowntimeMinutes: number;
  companyWideMttrMinutes: number | null;
  companyWideMtbfMinutes: number | null;
}

function defaultPeriodStart(): string {
  const d = new Date();
  d.setMonth(d.getMonth() - 3);
  return d.toISOString().slice(0, 10);
}

function defaultPeriodEnd(): string {
  return new Date().toISOString().slice(0, 10);
}

function formatMinutes(value: number | null): string {
  return value != null ? `${value.toLocaleString(undefined, { maximumFractionDigits: 1 })} min` : '—';
}

/**
 * Downtime metrics (MTBF/MTTR) — a period-scoped report over completed
 * maintenance requests (05_MODULE_ROADMAP.md's Maintenance metrics line item).
 * MTTR averages ActualDowntimeMinutes across completed requests; MTBF averages
 * the gap between consecutive Breakdown-type requests' creation timestamps.
 * Both render as "—", not 0, when there isn't enough data in the period to
 * compute them (fewer than 2 breakdowns for MTBF, zero completed requests for
 * MTTR) — see GetDowntimeMetricsReportQueryHandler's doc comment for the exact
 * methodology and its documented simplification. Rendered as a sibling panel
 * under AssetsPage, same "period picker + table" shape as Finance's period
 * report panels (e.g. GetProfitAndLossReport's UI).
 */
export function DowntimeMetricsPanel() {
  const { companyId } = useActiveCompany();
  const [periodStart, setPeriodStart] = useState(defaultPeriodStart());
  const [periodEnd, setPeriodEnd] = useState(defaultPeriodEnd());
  const [assetId, setAssetId] = useState('');

  const assetOptions = useAssetOptions(companyId);

  const reportQuery = useQuery({
    queryKey: ['downtime-metrics-report', companyId, assetId, periodStart, periodEnd],
    queryFn: () => {
      const params = new URLSearchParams({
        companyId: companyId!,
        periodStart: new Date(periodStart).toISOString(),
        periodEnd: new Date(periodEnd).toISOString(),
      });
      if (assetId) params.set('assetId', assetId);
      return apiClient.get<DowntimeMetricsReportDto>(`/maintenance/reports/downtime-metrics?${params.toString()}`);
    },
    enabled: Boolean(companyId && periodStart && periodEnd),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Downtime Metrics (MTBF / MTTR)</h2>

      <Card className="mb-6">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-4">
          <label className="flex flex-col gap-1 text-sm">
            Period start
            <input
              type="date"
              className="rounded-md border border-border bg-surface px-2 py-1.5"
              value={periodStart}
              onChange={(e) => setPeriodStart(e.target.value)}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Period end
            <input
              type="date"
              className="rounded-md border border-border bg-surface px-2 py-1.5"
              value={periodEnd}
              onChange={(e) => setPeriodEnd(e.target.value)}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            Asset (optional — leave blank for all assets)
            <EntityCombobox
              value={assetId}
              onChange={setAssetId}
              options={assetOptions.options}
              isLoading={assetOptions.isLoading}
              onSearchChange={assetOptions.onSearchChange}
              placeholder="All assets"
            />
          </label>
        </div>
      </Card>

      <Card className="mb-6">
        <h3 className="mb-2 text-sm font-semibold text-text">Company-wide</h3>
        {reportQuery.isLoading && <p role="status" className="text-text-muted">Loading…</p>}
        {reportQuery.isError && <p role="alert" className="text-danger">Could not load the downtime metrics report.</p>}
        {reportQuery.data && (
          <dl className="grid grid-cols-2 gap-4 text-sm sm:grid-cols-4">
            <div>
              <dt className="text-text-muted">Breakdowns</dt>
              <dd className="text-base font-semibold text-text">{reportQuery.data.totalBreakdownCount}</dd>
            </div>
            <div>
              <dt className="text-text-muted">Total downtime</dt>
              <dd className="text-base font-semibold text-text">{formatMinutes(reportQuery.data.totalDowntimeMinutes)}</dd>
            </div>
            <div>
              <dt className="text-text-muted">MTTR</dt>
              <dd className="text-base font-semibold text-text">{formatMinutes(reportQuery.data.companyWideMttrMinutes)}</dd>
            </div>
            <div>
              <dt className="text-text-muted">MTBF</dt>
              <dd className="text-base font-semibold text-text">{formatMinutes(reportQuery.data.companyWideMtbfMinutes)}</dd>
            </div>
          </dl>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Asset', render: (row: DowntimeMetricsAssetLineDto) => `${row.assetCode} — ${row.assetName}` },
            { header: 'Breakdowns', render: (row: DowntimeMetricsAssetLineDto) => row.breakdownCount },
            { header: 'Total downtime', render: (row: DowntimeMetricsAssetLineDto) => formatMinutes(row.totalDowntimeMinutes) },
            { header: 'MTTR', render: (row: DowntimeMetricsAssetLineDto) => formatMinutes(row.mttrMinutes) },
            { header: 'MTBF', render: (row: DowntimeMetricsAssetLineDto) => formatMinutes(row.mtbfMinutes) },
          ]}
          rows={reportQuery.data?.lines}
          isLoading={reportQuery.isLoading}
          isError={reportQuery.isError}
          errorMessage="Could not load per-asset downtime metrics."
          emptyMessage="No completed maintenance requests in this period."
          rowKey={(row) => row.assetId}
        />
      </Card>
    </div>
  );
}
