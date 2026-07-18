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
}

interface InventoryValuationReportDto {
  lines: InventoryValuationLineDto[];
  grandTotalValuation: number;
  grandTotalCostOfGoodsSold: number;
}

/**
 * Weighted-average-cost inventory valuation report (M9 remaining — Inventory
 * costing, 2026-07-16). Computed server-side by folding each product's full
 * ledger history through WeightedAverageCostCalculator — see
 * backend/src/Modules/Inventory/.../Domain/Costing/WeightedAverageCostCalculator.cs.
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
      <h2 className="mb-3 text-lg font-semibold text-text">Inventory Valuation (Weighted Average)</h2>

      <Card>
        <DataTable
          columns={[
            { header: 'SKU', render: (row: InventoryValuationLineDto) => row.sku },
            { header: 'Name', render: (row: InventoryValuationLineDto) => row.name },
            { header: 'On hand', render: (row: InventoryValuationLineDto) => row.onHandQuantity },
            { header: 'Weighted avg. cost', render: (row: InventoryValuationLineDto) => row.weightedAverageUnitCost.toFixed(2) },
            { header: 'Total valuation', render: (row: InventoryValuationLineDto) => row.totalValuation.toFixed(2) },
            { header: 'Cumulative COGS', render: (row: InventoryValuationLineDto) => row.cumulativeCostOfGoodsSold.toFixed(2) },
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
            Grand total valuation: <span className="font-semibold">{valuationQuery.data.grandTotalValuation.toFixed(2)}</span>
            {' · '}
            Grand total COGS: <span className="font-semibold">{valuationQuery.data.grandTotalCostOfGoodsSold.toFixed(2)}</span>
          </p>
        )}
      </Card>
    </div>
  );
}
