import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../../../shared/api/client';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';

interface InventoryValuationLineDto {
  productId: string;
  sku: string;
  name: string;
  onHandQuantity: number;
  weightedAverageUnitCost: number;
  totalValuation: number;
  cumulativeCostOfGoodsSold: number;
  fifoUnitCost: number;
  fifoTotalValuation: number;
  fifoCumulativeCostOfGoodsSold: number;
}

interface InventoryValuationReportDto {
  lines: InventoryValuationLineDto[];
  grandTotalValuation: number;
  grandTotalCostOfGoodsSold: number;
  grandTotalFifoValuation: number;
  grandTotalFifoCostOfGoodsSold: number;
}

/**
 * Inventory valuation report — Weighted Average and FIFO side by side (M9's
 * WeightedAverageCostCalculator plus Phase 1 closeout's FifoCostCalculator,
 * 2026-07-18, closing 05_MODULE_ROADMAP.md's "Inventory Valuation (FIFO,
 * Weighted Average Cost)" line item in full). Both methods are computed from
 * the same server-side ledger fold in one request — see
 * backend/src/Modules/Inventory/.../Domain/Costing/{WeightedAverageCostCalculator,FifoCostCalculator}.cs.
 * This is additive to the existing last-cost StockLedgerPanel/stock-valuation
 * report (Phase M6); it is not a replacement, so both remain available.
 */
export function InventoryValuationPanel() {
  const { companyId } = useActiveCompany();

  const valuationQuery = useQuery({
    queryKey: ['inventory-valuation', companyId],
    queryFn: () => apiClient.get<InventoryValuationReportDto>(`/inventory/reports/inventory-valuation?companyId=${companyId}`),
    enabled: Boolean(companyId),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Inventory Valuation (Weighted Average vs. FIFO)</h2>

      <Card>
        <DataTable
          columns={[
            { header: 'SKU', render: (row: InventoryValuationLineDto) => row.sku },
            { header: 'Name', render: (row: InventoryValuationLineDto) => row.name },
            { header: 'On hand', render: (row: InventoryValuationLineDto) => row.onHandQuantity },
            { header: 'Weighted avg. cost', render: (row: InventoryValuationLineDto) => row.weightedAverageUnitCost.toFixed(2) },
            { header: 'WAC valuation', render: (row: InventoryValuationLineDto) => row.totalValuation.toFixed(2) },
            { header: 'WAC COGS', render: (row: InventoryValuationLineDto) => row.cumulativeCostOfGoodsSold.toFixed(2) },
            { header: 'FIFO cost', render: (row: InventoryValuationLineDto) => row.fifoUnitCost.toFixed(2) },
            { header: 'FIFO valuation', render: (row: InventoryValuationLineDto) => row.fifoTotalValuation.toFixed(2) },
            { header: 'FIFO COGS', render: (row: InventoryValuationLineDto) => row.fifoCumulativeCostOfGoodsSold.toFixed(2) },
          ]}
          rows={valuationQuery.data?.lines}
          isLoading={valuationQuery.isLoading}
          isError={valuationQuery.isError}
          errorMessage="Could not load the inventory valuation report."
          emptyMessage="No stock movements recorded yet."
          rowKey={(row: InventoryValuationLineDto) => row.productId}
        />
        {valuationQuery.data && (
          <p className="mt-3 text-sm">
            WAC grand total: <span className="font-semibold">{valuationQuery.data.grandTotalValuation.toFixed(2)}</span>
            {' valuation · '}
            <span className="font-semibold">{valuationQuery.data.grandTotalCostOfGoodsSold.toFixed(2)}</span>
            {' COGS'}
            {' — FIFO grand total: '}
            <span className="font-semibold">{valuationQuery.data.grandTotalFifoValuation.toFixed(2)}</span>
            {' valuation · '}
            <span className="font-semibold">{valuationQuery.data.grandTotalFifoCostOfGoodsSold.toFixed(2)}</span>
            {' COGS'}
          </p>
        )}
      </Card>
    </div>
  );
}
