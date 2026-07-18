import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../../../shared/api/client';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import type { PagedResult } from '../../../shared/api/types';

interface ArAgingLineDto {
  customerId: string;
  invoiceId: string;
  balance: number;
  chargeDate: string;
  daysOutstanding: number;
  bucket: string;
}
interface ArAgingReportDto {
  lines: ArAgingLineDto[];
  bucket0To30Total: number;
  bucket31To60Total: number;
  bucket61To90Total: number;
  bucket90PlusTotal: number;
  grandTotal: number;
}

interface StockValuationLineDto {
  productId: string;
  sku: string;
  name: string;
  onHandQuantity: number;
  lastUnitCost: number | null;
  extendedValue: number;
}
interface StockValuationReportDto {
  lines: StockValuationLineDto[];
  grandTotalValue: number;
}

interface PoStatusSummaryLineDto {
  status: string;
  count: number;
}
interface PoStatusSummaryReportDto {
  lines: PoStatusSummaryLineDto[];
  totalCount: number;
}

interface SupplierScorecardLineDto {
  supplierId: string;
  orderCount: number;
  totalOrderValue: number;
  averageOrderValue: number;
  fullyReceivedCount: number;
  fullyReceivedRate: number;
}

interface SalesOrderSummaryDto {
  id: string;
}

// Below this many on-hand units a product shows up in the low-stock widget.
// Hardcoded rather than per-product for this first cut of the Dashboard — a
// real reorder-point-per-product setting is a bigger Inventory feature, not
// something to fake here just to make this widget look more sophisticated.
const LOW_STOCK_THRESHOLD = 10;

function KpiCard({ label, value, hint }: { label: string; value: string; hint?: string }) {
  return (
    <Card>
      <p className="text-xs uppercase tracking-wide text-text-muted">{label}</p>
      <p className="mt-1 text-2xl font-semibold text-text">{value}</p>
      {hint && <p className="mt-1 text-xs text-text-muted">{hint}</p>}
    </Card>
  );
}

/**
 * Landing dashboard (Phase M6, 2026-07-15) — KPI widgets built entirely on
 * top of the three canned reports (AR aging, stock valuation, PO status
 * summary) plus the existing Sales Orders list endpoint, rather than
 * inventing any new backend aggregation just for this page. "Pending PO
 * approvals" reuses the PO status summary's Draft bucket instead of a
 * separate query — the report already answers that question. The Supplier
 * Scorecard table (Phase 10 item 2, added 2026-07-16) reuses the same
 * pattern: a canned report computed entirely from existing PurchaseOrder
 * data, with no on-time-delivery metric since no expected-delivery-date
 * field exists on PurchaseOrder to compute one honestly from.
 */
export function DashboardPage() {
  const { companyId } = useActiveCompany();

  const salesOrdersQuery = useQuery({
    queryKey: ['dashboard-sales-orders-count', companyId],
    queryFn: () => apiClient.get<PagedResult<SalesOrderSummaryDto>>(`/sales/sales-orders?companyId=${companyId}&page=1&pageSize=1`),
    enabled: Boolean(companyId),
  });

  const poStatusQuery = useQuery({
    queryKey: ['dashboard-po-status', companyId],
    queryFn: () => apiClient.get<PoStatusSummaryReportDto>(`/procurement/reports/po-status-summary?companyId=${companyId}`),
    enabled: Boolean(companyId),
  });

  const stockValuationQuery = useQuery({
    queryKey: ['dashboard-stock-valuation', companyId],
    queryFn: () => apiClient.get<StockValuationReportDto>(`/inventory/reports/stock-valuation?companyId=${companyId}`),
    enabled: Boolean(companyId),
  });

  const arAgingQuery = useQuery({
    queryKey: ['dashboard-ar-aging', companyId],
    queryFn: () => apiClient.get<ArAgingReportDto>(`/finance/reports/ar-aging?companyId=${companyId}`),
    enabled: Boolean(companyId),
  });

  const supplierScorecardQuery = useQuery({
    queryKey: ['dashboard-supplier-scorecard', companyId],
    queryFn: () => apiClient.get<SupplierScorecardLineDto[]>(`/procurement/reports/supplier-scorecard?companyId=${companyId}`),
    enabled: Boolean(companyId),
  });

  if (!companyId) {
    return <p className="text-sm text-text-muted">Set an active company (top right) to see the dashboard.</p>;
  }

  const pendingApprovals = poStatusQuery.data?.lines.find((l) => l.status === 'Draft')?.count ?? 0;
  const lowStockLines = (stockValuationQuery.data?.lines ?? []).filter((l) => l.onHandQuantity < LOW_STOCK_THRESHOLD);

  return (
    <div>
      <h1 className="mb-1 text-xl font-semibold text-text">Dashboard</h1>
      <p className="mb-6 text-sm text-text-muted">A cross-module snapshot for the active company.</p>

      <div className="mb-8 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <KpiCard
          label="Open Sales Orders"
          value={salesOrdersQuery.isLoading ? '…' : String(salesOrdersQuery.data?.totalCount ?? 0)}
        />
        <KpiCard
          label="Pending PO Approvals"
          value={poStatusQuery.isLoading ? '…' : String(pendingApprovals)}
          hint="Purchase orders still in Draft"
        />
        <KpiCard
          label="Low-Stock Products"
          value={stockValuationQuery.isLoading ? '…' : String(lowStockLines.length)}
          hint={`Below ${LOW_STOCK_THRESHOLD} units on hand`}
        />
        <KpiCard
          label="AR Outstanding"
          value={arAgingQuery.isLoading ? '…' : (arAgingQuery.data?.grandTotal ?? 0).toLocaleString()}
          hint={`${arAgingQuery.data?.lines.length ?? 0} open invoice balances`}
        />
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <div>
          <h2 className="mb-3 text-lg font-semibold text-text">AR Aging</h2>
          <Card className="mb-4">
            <div className="grid grid-cols-4 gap-2 text-center text-sm">
              <div>
                <p className="text-text-muted">0-30</p>
                <p className="font-semibold text-text">{(arAgingQuery.data?.bucket0To30Total ?? 0).toLocaleString()}</p>
              </div>
              <div>
                <p className="text-text-muted">31-60</p>
                <p className="font-semibold text-text">{(arAgingQuery.data?.bucket31To60Total ?? 0).toLocaleString()}</p>
              </div>
              <div>
                <p className="text-text-muted">61-90</p>
                <p className="font-semibold text-text">{(arAgingQuery.data?.bucket61To90Total ?? 0).toLocaleString()}</p>
              </div>
              <div>
                <p className="text-text-muted">90+</p>
                <p className="font-semibold text-text">{(arAgingQuery.data?.bucket90PlusTotal ?? 0).toLocaleString()}</p>
              </div>
            </div>
          </Card>
          <Card>
            <DataTable
              columns={[
                { header: 'Invoice', render: (l: ArAgingLineDto) => l.invoiceId },
                { header: 'Days', render: (l: ArAgingLineDto) => String(l.daysOutstanding) },
                { header: 'Bucket', render: (l: ArAgingLineDto) => l.bucket },
                { header: 'Balance', render: (l: ArAgingLineDto) => l.balance.toLocaleString() },
              ]}
              rows={arAgingQuery.data?.lines}
              isLoading={arAgingQuery.isLoading}
              emptyMessage="No outstanding invoice balances."
              rowKey={(l) => l.invoiceId}
            />
          </Card>
        </div>

        <div>
          <h2 className="mb-3 text-lg font-semibold text-text">Low Stock</h2>
          <Card>
            <DataTable
              columns={[
                { header: 'SKU', render: (l: StockValuationLineDto) => l.sku },
                { header: 'Name', render: (l: StockValuationLineDto) => l.name },
                { header: 'On Hand', render: (l: StockValuationLineDto) => l.onHandQuantity.toLocaleString() },
                { header: 'Ext. Value', render: (l: StockValuationLineDto) => l.extendedValue.toLocaleString() },
              ]}
              rows={lowStockLines}
              isLoading={stockValuationQuery.isLoading}
              emptyMessage="No products below the low-stock threshold."
              rowKey={(l) => l.productId}
            />
          </Card>

          <h2 className="mb-3 mt-6 text-lg font-semibold text-text">Purchase Order Status</h2>
          <Card>
            <DataTable
              columns={[
                { header: 'Status', render: (l: PoStatusSummaryLineDto) => l.status },
                { header: 'Count', render: (l: PoStatusSummaryLineDto) => String(l.count) },
              ]}
              rows={poStatusQuery.data?.lines}
              isLoading={poStatusQuery.isLoading}
              emptyMessage="No purchase orders yet."
              rowKey={(l) => l.status}
            />
          </Card>

          <h2 className="mb-3 mt-6 text-lg font-semibold text-text">Supplier Scorecard</h2>
          <Card>
            <DataTable
              columns={[
                { header: 'Supplier ID', render: (l: SupplierScorecardLineDto) => l.supplierId },
                { header: 'Orders', render: (l: SupplierScorecardLineDto) => String(l.orderCount) },
                { header: 'Total value', render: (l: SupplierScorecardLineDto) => l.totalOrderValue.toLocaleString() },
                { header: 'Avg. order value', render: (l: SupplierScorecardLineDto) => l.averageOrderValue.toLocaleString() },
                { header: 'Fully received %', render: (l: SupplierScorecardLineDto) => `${l.fullyReceivedRate.toFixed(1)}%` },
              ]}
              rows={supplierScorecardQuery.data}
              isLoading={supplierScorecardQuery.isLoading}
              emptyMessage="No purchase orders yet."
              rowKey={(l) => l.supplierId}
            />
          </Card>
        </div>
      </div>
    </div>
  );
}
