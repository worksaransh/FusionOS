import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../../../shared/api/client';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';

interface WorkInProgressLineDto {
  workOrderId: string;
  productId: string;
  quantityToProduce: number;
  totalIssuedValue: number;
}
interface WorkInProgressSummaryReportDto {
  lines: WorkInProgressLineDto[];
  totalWipValue: number;
  valuationNote: string;
}

function formatCurrency(value: number): string {
  return value.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

/**
 * Work-in-Progress Summary — every Released work order's currently-issued
 * material value, and the company-wide WIP total.
 *
 * Material value only: WorkOrderComponent.UnitCost is whatever the BOM
 * author supplied at authoring time — it is never looked up from Inventory
 * automatically, so a component authored with no unit cost contributes zero
 * here even though material has genuinely been issued for it. Labor and
 * overhead are not included. See the backend's own explanation, shown
 * verbatim below via valuationNote.
 */
export function WorkInProgressSummaryPanel() {
  const { companyId } = useActiveCompany();

  const wipSummaryQuery = useQuery({
    queryKey: ['work-in-progress-summary', companyId],
    queryFn: () =>
      apiClient.get<WorkInProgressSummaryReportDto>(`/manufacturing/reports/work-in-progress-summary?companyId=${companyId}`),
    enabled: Boolean(companyId),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Work-in-Progress Summary</h2>
      <p className="mb-3 text-xs text-text-muted">
        Material value currently issued to every Released work order, company-wide. See the note below for what this
        does and doesn&apos;t account for.
      </p>

      <Card>
        <DataTable
          columns={[
            { header: 'Work order', render: (l: WorkInProgressLineDto) => l.workOrderId },
            { header: 'Product', render: (l: WorkInProgressLineDto) => l.productId },
            { header: 'Quantity to produce', render: (l: WorkInProgressLineDto) => l.quantityToProduce },
            { header: 'Issued material value', render: (l: WorkInProgressLineDto) => formatCurrency(l.totalIssuedValue) },
          ]}
          rows={wipSummaryQuery.data?.lines}
          isLoading={wipSummaryQuery.isLoading}
          isError={wipSummaryQuery.isError}
          errorMessage="Could not load the work-in-progress summary."
          emptyMessage="No Released work orders right now."
          rowKey={(l) => l.workOrderId}
        />
        {wipSummaryQuery.data && (
          <p className="mt-3 text-sm">
            Total WIP value: <span className="font-semibold">{formatCurrency(wipSummaryQuery.data.totalWipValue)}</span>
          </p>
        )}
        {wipSummaryQuery.data?.valuationNote && (
          <p className="mt-3 rounded-md border border-border bg-surface-muted p-3 text-xs text-text-muted">
            {wipSummaryQuery.data.valuationNote}
          </p>
        )}
      </Card>
    </div>
  );
}
