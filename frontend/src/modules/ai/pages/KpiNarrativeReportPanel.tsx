import { useState } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import type { PagedResult } from '../../../shared/api/types';

interface KpiDefinitionDto {
  id: string;
  code: string;
  name: string;
  unit: string | null;
  isActive: boolean;
}

interface KpiNarrativeReportDto {
  kpiDefinitionId: string;
  kpiName: string;
  narrative: string;
  average: number;
  min: number;
  max: number;
  percentChange: number;
  trendDirection: string;
  snapshotCount: number;
}

/**
 * KPI Narrative Report — 12_AI_PLATFORM.md §4 "Automated report generation":
 * builds on Business Intelligence's own KPI snapshot history rather than a
 * parallel reporting pipeline. The four stat numbers and the narrative
 * paragraph below are genuinely computed by the backend from real KpiSnapshot
 * values (see GenerateKpiNarrativeReportQueryHandler), not a canned template.
 */
export function KpiNarrativeReportPanel() {
  const { companyId } = useActiveCompany();
  const [kpiDefinitionId, setKpiDefinitionId] = useState('');
  const [lookbackDays, setLookbackDays] = useState(90);

  const kpisQuery = useQuery({
    queryKey: ['kpi-definitions-for-report', companyId],
    queryFn: () => apiClient.get<PagedResult<KpiDefinitionDto>>(`/bi/kpi-definitions?companyId=${companyId}&page=1&pageSize=100`),
    enabled: Boolean(companyId),
  });

  const generateReport = useMutation({
    mutationFn: () =>
      apiClient.get<KpiNarrativeReportDto>(
        `/ai/reports/kpi-narrative/${kpiDefinitionId}?companyId=${companyId}&lookbackDays=${lookbackDays}`,
      ),
  });

  if (!companyId) {
    return null;
  }

  const kpis = kpisQuery.data?.data ?? [];

  return (
    <Card className="mt-6">
      <h2 className="mb-1 text-lg font-semibold text-text">KPI Narrative Report</h2>
      <p className="mb-4 text-sm text-text-muted">
        Generates a plain-English summary and real trend numbers from a KPI&rsquo;s own recorded
        snapshot history — Business Intelligence, Phase 6 data, Phase 8 narrative layer.
      </p>

      <form noValidate
        onSubmit={(e) => {
          e.preventDefault();
          if (kpiDefinitionId) generateReport.mutate();
        }}
        className="mb-4 grid grid-cols-1 gap-3 sm:grid-cols-3"
      >
        <label className="flex flex-col gap-1 text-sm">
          KPI
          <select
            className="rounded-md border border-border bg-surface px-2 py-1.5"
            value={kpiDefinitionId}
            onChange={(e) => setKpiDefinitionId(e.target.value)}
          >
            <option value="">Select a KPI…</option>
            {kpis.map((kpi) => (
              <option key={kpi.id} value={kpi.id}>{kpi.name} ({kpi.code})</option>
            ))}
          </select>
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Lookback days
          <input
            type="number"
            min={1}
            className="rounded-md border border-border bg-surface px-2 py-1.5"
            value={lookbackDays}
            onChange={(e) => setLookbackDays(Number(e.target.value) || 90)}
          />
        </label>
        <div className="flex items-end">
          <Button type="submit" disabled={generateReport.isPending || !kpiDefinitionId}>
            {generateReport.isPending ? 'Generating…' : 'Generate report'}
          </Button>
        </div>
      </form>

      {generateReport.isError && generateReport.error instanceof ApiError && (
        <p role="alert" className="mb-3 text-sm text-danger">{generateReport.error.problem.title}</p>
      )}

      {generateReport.data && (
        <div>
          <p className="mb-4 text-sm text-text">{generateReport.data.narrative}</p>
          <div className="grid grid-cols-2 gap-3 sm:grid-cols-5">
            <Stat label="Average" value={generateReport.data.average.toFixed(1)} />
            <Stat label="Min" value={generateReport.data.min.toFixed(1)} />
            <Stat label="Max" value={generateReport.data.max.toFixed(1)} />
            <Stat label="% Change" value={`${generateReport.data.percentChange.toFixed(1)}%`} />
            <Stat label="Trend" value={generateReport.data.trendDirection} />
          </div>
          <p className="mt-3 text-xs text-text-muted">Based on {generateReport.data.snapshotCount} snapshot(s).</p>
        </div>
      )}
    </Card>
  );
}

function Stat({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md border border-border p-3 text-center">
      <div className="text-xs text-text-muted">{label}</div>
      <div className="text-lg font-semibold text-text">{value}</div>
    </div>
  );
}
