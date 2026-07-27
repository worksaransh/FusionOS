import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../../../shared/api/client';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';

interface PipelineStageDto {
  stage: string;
  estimatedValueTotal: number;
  weightedValueTotal: number;
}

interface CrmDashboardDto {
  pipelineByStage: PipelineStageDto[];
  totalPipelineEstimatedValue: number;
  totalPipelineWeightedValue: number;
  wonCount: number;
  lostCount: number;
  winRatePercent: number;
  totalLeadCount: number;
  convertedLeadCount: number;
  leadConversionRatePercent: number;
}

/**
 * CRM dashboard — pipeline value by (open) stage, win rate, and lead conversion rate, all
 * pre-aggregated server-side by GetCrmDashboardQueryHandler (no charting library in this
 * codebase yet, so a simple table + CSS bar, same "Card + DataTable" shape as every other
 * CRM panel). Rendered as a sibling panel under LeadsPage, same pattern as
 * OpportunitiesPanel/CrmAccountsPanel.
 */
export function CrmDashboardPanel() {
  const { companyId } = useActiveCompany();

  const dashboardQuery = useQuery({
    queryKey: ['crm-dashboard', companyId],
    queryFn: () => apiClient.get<CrmDashboardDto>(`/crm/dashboard?companyId=${companyId}`),
    enabled: Boolean(companyId),
  });

  if (!companyId) return null;

  const data = dashboardQuery.data;
  const maxStageValue = data ? Math.max(1, ...data.pipelineByStage.map((s) => s.estimatedValueTotal)) : 1;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">CRM dashboard</h2>

      {dashboardQuery.isLoading && <p role="status" className="text-text-muted">Loading…</p>}
      {dashboardQuery.isError && <p role="alert" className="text-danger">Could not load the CRM dashboard.</p>}

      {data && (
        <>
          <div className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-3">
            <Card>
              <p className="text-xs text-text-muted">Total pipeline value</p>
              <p className="text-xl font-semibold text-text">{data.totalPipelineEstimatedValue.toLocaleString()}</p>
              <p className="text-xs text-text-muted">Weighted: {data.totalPipelineWeightedValue.toLocaleString()}</p>
            </Card>
            <Card>
              <p className="text-xs text-text-muted">Win rate</p>
              <p className="text-xl font-semibold text-text">{data.winRatePercent}%</p>
              <p className="text-xs text-text-muted">{data.wonCount} won / {data.lostCount} lost</p>
            </Card>
            <Card>
              <p className="text-xs text-text-muted">Lead conversion rate</p>
              <p className="text-xl font-semibold text-text">{data.leadConversionRatePercent}%</p>
              <p className="text-xs text-text-muted">{data.convertedLeadCount} converted / {data.totalLeadCount} leads</p>
            </Card>
          </div>

          <Card>
            <h3 className="mb-3 text-base font-semibold text-text">Pipeline value by stage</h3>
            <div className="mb-4 flex flex-col gap-2">
              {data.pipelineByStage.map((stage) => (
                <div key={stage.stage} className="flex items-center gap-3">
                  <span className="w-28 shrink-0 text-sm text-text-muted">{stage.stage}</span>
                  <div className="h-4 flex-1 rounded bg-surface-muted">
                    <div
                      className="h-4 rounded bg-primary"
                      style={{ width: `${(stage.estimatedValueTotal / maxStageValue) * 100}%` }}
                    />
                  </div>
                  <span className="w-24 shrink-0 text-right text-sm text-text">{stage.estimatedValueTotal.toLocaleString()}</span>
                </div>
              ))}
              {data.pipelineByStage.length === 0 && (
                <p className="text-sm text-text-muted">No open opportunities yet.</p>
              )}
            </div>

            <DataTable
              columns={[
                { header: 'Stage', render: (row: PipelineStageDto) => row.stage },
                { header: 'Estimated value', render: (row: PipelineStageDto) => row.estimatedValueTotal.toLocaleString() },
                { header: 'Weighted value', render: (row: PipelineStageDto) => row.weightedValueTotal.toLocaleString() },
              ]}
              rows={data.pipelineByStage}
              isLoading={false}
              emptyMessage="No open opportunities yet."
              rowKey={(row) => row.stage}
            />
          </Card>
        </>
      )}
    </div>
  );
}
