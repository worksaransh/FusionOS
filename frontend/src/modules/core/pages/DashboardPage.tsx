import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { ShoppingBag, ShoppingCart, AlertTriangle, DollarSign, CreditCard, Plus, ArrowRight } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { apiClient } from '../../../shared/api/client';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { StatsCard } from '../../../shared/ui/StatsCard';
import { Button } from '../../../shared/ui/Button';
import { StatusBadge } from '../../../shared/ui/StatusBadge';
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

interface ApAgingLineDto {
  supplierId: string;
  balance: number;
  oldestChargeDate: string;
  daysOutstanding: number;
  bucket: string;
}
interface ApAgingReportDto {
  lines: ApAgingLineDto[];
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

const LOW_STOCK_THRESHOLD = 10;

/** Formats a client-side "as of" freshness timestamp captured at query-success time (Phase 2.3). */
function formatAsOf(date: Date | undefined): string {
  if (!date) return '…';
  return `as of ${date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`;
}

/** Small inline freshness stamp shown next to a widget header. */
function AsOfStamp({ date }: { date: Date | undefined }) {
  return <span className="text-xs font-normal text-text-muted">{formatAsOf(date)}</span>;
}

export function DashboardPage() {
  const { companyId } = useActiveCompany();
  const navigate = useNavigate();

  // Date-range filter (Phase 2.3) shared by the AR/AP aging widgets. Empty string = no filter (all-time), matching the backend's optional fromDate/toDate.
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');

  // Client-side "as of" freshness timestamps, captured whenever each widget's query last succeeded.
  const [arAgingFetchedAt, setArAgingFetchedAt] = useState<Date>();
  const [apAgingFetchedAt, setApAgingFetchedAt] = useState<Date>();
  const [stockValuationFetchedAt, setStockValuationFetchedAt] = useState<Date>();

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
    queryFn: () =>
      apiClient.get<StockValuationReportDto>(`/inventory/reports/stock-valuation?companyId=${companyId}`).then((data) => {
        setStockValuationFetchedAt(new Date());
        return data;
      }),
    enabled: Boolean(companyId),
  });

  const arAgingQuery = useQuery({
    queryKey: ['dashboard-ar-aging', companyId, fromDate, toDate],
    queryFn: () =>
      apiClient
        .get<ArAgingReportDto>(
          `/finance/reports/ar-aging?companyId=${companyId}${fromDate ? `&fromDate=${fromDate}` : ''}${toDate ? `&toDate=${toDate}` : ''}`,
        )
        .then((data) => {
          setArAgingFetchedAt(new Date());
          return data;
        }),
    enabled: Boolean(companyId),
  });

  const supplierScorecardQuery = useQuery({
    queryKey: ['dashboard-supplier-scorecard', companyId],
    queryFn: () => apiClient.get<SupplierScorecardLineDto[]>(`/procurement/reports/supplier-scorecard?companyId=${companyId}`),
    enabled: Boolean(companyId),
  });

  const apAgingQuery = useQuery({
    queryKey: ['dashboard-ap-aging', companyId, fromDate, toDate],
    queryFn: () =>
      apiClient
        .get<ApAgingReportDto>(
          `/finance/reports/ap-aging?companyId=${companyId}${fromDate ? `&fromDate=${fromDate}` : ''}${toDate ? `&toDate=${toDate}` : ''}`,
        )
        .then((data) => {
          setApAgingFetchedAt(new Date());
          return data;
        }),
    enabled: Boolean(companyId),
  });

  if (!companyId) {
    return <p className="text-sm text-text-muted">Set an active company (top right) to see the dashboard.</p>;
  }

  const pendingApprovals = poStatusQuery.data?.lines.find((l) => l.status === 'Draft')?.count ?? 0;
  const lowStockLines = (stockValuationQuery.data?.lines ?? []).filter((l) => l.onHandQuantity < LOW_STOCK_THRESHOLD);

  return (
    <div className="space-y-8">
      {/* Header Banner & Quick Action Buttons */}
      <div className="flex flex-wrap items-center justify-between gap-4 border-b border-border pb-5">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-text">Executive Operating Dashboard</h1>
          <p className="text-sm text-text-muted">Cross-module operational oversight for Active Company.</p>
        </div>

        <div className="flex flex-wrap gap-2">
          <Button type="button" onClick={() => navigate('/inventory')} className="text-xs">
            <Plus size={14} className="mr-1" /> New Product
          </Button>
          <Button type="button" variant="secondary" onClick={() => navigate('/sales')} className="text-xs">
            <ShoppingCart size={14} className="mr-1" /> Sales Order
          </Button>
          <Button type="button" variant="secondary" onClick={() => navigate('/procurement')} className="text-xs">
            <ShoppingBag size={14} className="mr-1" /> Purchase Order
          </Button>
        </div>
      </div>

      {/* AR/AP Aging Date-Range Filter */}
      <div className="flex flex-wrap items-end gap-3 rounded-md border border-border bg-surface p-3">
        <div className="flex flex-col gap-1">
          <label htmlFor="dashboard-from-date" className="text-xs font-medium text-text-muted">
            From date
          </label>
          <input
            id="dashboard-from-date"
            type="date"
            value={fromDate}
            onChange={(e) => setFromDate(e.target.value)}
            className="rounded border border-border bg-surface px-2 py-1 text-sm text-text"
          />
        </div>
        <div className="flex flex-col gap-1">
          <label htmlFor="dashboard-to-date" className="text-xs font-medium text-text-muted">
            To date
          </label>
          <input
            id="dashboard-to-date"
            type="date"
            value={toDate}
            onChange={(e) => setToDate(e.target.value)}
            className="rounded border border-border bg-surface px-2 py-1 text-sm text-text"
          />
        </div>
        {(fromDate || toDate) && (
          <Button
            type="button"
            variant="secondary"
            className="text-xs"
            onClick={() => {
              setFromDate('');
              setToDate('');
            }}
          >
            Clear range
          </Button>
        )}
        <p className="text-xs text-text-muted">Filters the AR/AP Aging widgets below. Leave blank for all-time.</p>
      </div>

      {/* KPI Cards Grid */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-5">
        <StatsCard
          title="Open Sales Orders"
          value={salesOrdersQuery.isLoading ? '…' : String(salesOrdersQuery.data?.totalCount ?? 0)}
          description="Active customer fulfillment pipeline"
          icon={<ShoppingCart size={20} />}
        />
        <StatsCard
          title="Pending PO Approvals"
          value={poStatusQuery.isLoading ? '…' : String(pendingApprovals)}
          description="Draft purchase orders awaiting sign-off"
          icon={<ShoppingBag size={20} />}
        />
        <StatsCard
          title="Low-Stock Items"
          value={stockValuationQuery.isLoading ? '…' : String(lowStockLines.length)}
          description={`Items below threshold (${LOW_STOCK_THRESHOLD} units)`}
          icon={<AlertTriangle size={20} />}
        />
        <StatsCard
          title="AR Outstanding"
          value={arAgingQuery.isLoading ? '…' : `$${(arAgingQuery.data?.grandTotal ?? 0).toLocaleString()}`}
          description={`${arAgingQuery.data?.lines.length ?? 0} open invoice balances`}
          icon={<DollarSign size={20} />}
        />
        <StatsCard
          title="AP Outstanding"
          value={apAgingQuery.isLoading ? '…' : `$${(apAgingQuery.data?.grandTotal ?? 0).toLocaleString()}`}
          description={`${apAgingQuery.data?.lines.length ?? 0} open supplier payables`}
          icon={<CreditCard size={20} />}
        />
      </div>

      {/* Main Analytical Grid */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {/* Left Column: Receivables & Payables */}
        <div className="space-y-6">
          <div>
            <div className="flex items-center justify-between mb-3">
              <h2 className="flex items-baseline gap-2 text-lg font-semibold text-text">
                Accounts Receivable (AR) Aging
                <AsOfStamp date={arAgingFetchedAt} />
              </h2>
              <Button type="button" variant="secondary" onClick={() => navigate('/finance')} className="text-xs">
                View GL <ArrowRight size={12} className="ml-1" />
              </Button>
            </div>
            <p className="mb-3 text-2xl font-bold text-text">
              Total AR Outstanding: <span className="text-emerald-600 dark:text-emerald-400">${(arAgingQuery.data?.grandTotal ?? 0).toLocaleString()}</span>
            </p>
            <Card className="mb-4">
              <div className="grid grid-cols-4 gap-2 text-center text-xs sm:text-sm">
                <div className="border-r border-border pr-2">
                  <p className="text-text-muted">0-30 Days</p>
                  <p className="font-bold text-emerald-600 dark:text-emerald-400">
                    ${(arAgingQuery.data?.bucket0To30Total ?? 0).toLocaleString()}
                  </p>
                </div>
                <div className="border-r border-border pr-2">
                  <p className="text-text-muted">31-60 Days</p>
                  <p className="font-bold text-amber-600 dark:text-amber-400">
                    ${(arAgingQuery.data?.bucket31To60Total ?? 0).toLocaleString()}
                  </p>
                </div>
                <div className="border-r border-border pr-2">
                  <p className="text-text-muted">61-90 Days</p>
                  <p className="font-bold text-rose-500">
                    ${(arAgingQuery.data?.bucket61To90Total ?? 0).toLocaleString()}
                  </p>
                </div>
                <div>
                  <p className="text-text-muted">90+ Days</p>
                  <p className="font-bold text-rose-700 dark:text-rose-400">
                    ${(arAgingQuery.data?.bucket90PlusTotal ?? 0).toLocaleString()}
                  </p>
                </div>
              </div>
            </Card>
            <Card>
              <DataTable
                columns={[
                  { header: 'Invoice ID', accessorKey: 'invoiceId', render: (l: ArAgingLineDto) => <span className="font-mono">{l.invoiceId}</span> },
                  { header: 'Days', accessorKey: 'daysOutstanding', render: (l: ArAgingLineDto) => String(l.daysOutstanding) },
                  { header: 'Bucket', render: (l: ArAgingLineDto) => <StatusBadge status={l.bucket} /> },
                  { header: 'Balance', accessorKey: 'balance', render: (l: ArAgingLineDto) => `$${l.balance.toLocaleString()}` },
                ]}
                rows={arAgingQuery.data?.lines}
                isLoading={arAgingQuery.isLoading}
                emptyMessage="No outstanding invoice balances."
                rowKey={(l) => l.invoiceId}
              />
            </Card>
          </div>

          <div>
            <h2 className="mb-3 flex items-baseline gap-2 text-lg font-semibold text-text">
              Accounts Payable (AP) Aging
              <AsOfStamp date={apAgingFetchedAt} />
            </h2>
            <p className="mb-3 text-2xl font-bold text-text">
              Total AP Outstanding: <span className="text-rose-500">${(apAgingQuery.data?.grandTotal ?? 0).toLocaleString()}</span>
            </p>
            <Card className="mb-4">
              <div className="grid grid-cols-4 gap-2 text-center text-xs sm:text-sm">
                <div className="border-r border-border pr-2">
                  <p className="text-text-muted">0-30 Days</p>
                  <p className="font-bold text-text">${(apAgingQuery.data?.bucket0To30Total ?? 0).toLocaleString()}</p>
                </div>
                <div className="border-r border-border pr-2">
                  <p className="text-text-muted">31-60 Days</p>
                  <p className="font-bold text-text">${(apAgingQuery.data?.bucket31To60Total ?? 0).toLocaleString()}</p>
                </div>
                <div className="border-r border-border pr-2">
                  <p className="text-text-muted">61-90 Days</p>
                  <p className="font-bold text-text">${(apAgingQuery.data?.bucket61To90Total ?? 0).toLocaleString()}</p>
                </div>
                <div>
                  <p className="text-text-muted">90+ Days</p>
                  <p className="font-bold text-text">${(apAgingQuery.data?.bucket90PlusTotal ?? 0).toLocaleString()}</p>
                </div>
              </div>
            </Card>
            <Card>
              <DataTable
                columns={[
                  { header: 'Supplier ID', accessorKey: 'supplierId', render: (l: ApAgingLineDto) => <span className="font-mono">{l.supplierId}</span> },
                  { header: 'Days', accessorKey: 'daysOutstanding', render: (l: ApAgingLineDto) => String(l.daysOutstanding) },
                  { header: 'Bucket', render: (l: ApAgingLineDto) => <StatusBadge status={l.bucket} /> },
                  { header: 'Balance', accessorKey: 'balance', render: (l: ApAgingLineDto) => `$${l.balance.toLocaleString()}` },
                ]}
                rows={apAgingQuery.data?.lines}
                isLoading={apAgingQuery.isLoading}
                emptyMessage="No outstanding supplier balances."
                rowKey={(l) => l.supplierId}
              />
            </Card>
          </div>
        </div>

        {/* Right Column: Inventory & Procurement */}
        <div className="space-y-6">
          <div>
            <div className="flex items-center justify-between mb-3">
              <h2 className="flex items-baseline gap-2 text-lg font-semibold text-text">
                Low-Stock Stock Valuation Alert
                <AsOfStamp date={stockValuationFetchedAt} />
              </h2>
              <Button type="button" variant="secondary" onClick={() => navigate('/inventory')} className="text-xs">
                Inventory Catalog <ArrowRight size={12} className="ml-1" />
              </Button>
            </div>
            <Card>
              <DataTable
                columns={[
                  { header: 'SKU', accessorKey: 'sku', render: (l: StockValuationLineDto) => <span className="font-mono font-medium">{l.sku}</span> },
                  { header: 'Name', accessorKey: 'name', render: (l: StockValuationLineDto) => l.name },
                  { header: 'On Hand', accessorKey: 'onHandQuantity', render: (l: StockValuationLineDto) => <span className="font-bold text-rose-500">{l.onHandQuantity.toLocaleString()}</span> },
                  { header: 'Ext. Value', accessorKey: 'extendedValue', render: (l: StockValuationLineDto) => `$${l.extendedValue.toLocaleString()}` },
                ]}
                rows={lowStockLines}
                isLoading={stockValuationQuery.isLoading}
                emptyMessage="All product stock levels are healthy."
                rowKey={(l) => l.productId}
              />
            </Card>
          </div>

          <div>
            <h2 className="mb-3 text-lg font-semibold text-text">Purchase Order Status Overview</h2>
            <Card>
              <DataTable
                columns={[
                  { header: 'Status', accessorKey: 'status', render: (l: PoStatusSummaryLineDto) => <StatusBadge status={l.status} /> },
                  { header: 'Count', accessorKey: 'count', render: (l: PoStatusSummaryLineDto) => <span className="font-semibold">{l.count}</span> },
                ]}
                rows={poStatusQuery.data?.lines}
                isLoading={poStatusQuery.isLoading}
                emptyMessage="No purchase orders yet."
                rowKey={(l) => l.status}
              />
            </Card>
          </div>

          <div>
            <h2 className="mb-3 text-lg font-semibold text-text">Supplier Performance Scorecard</h2>
            <Card>
              <DataTable
                columns={[
                  { header: 'Supplier ID', accessorKey: 'supplierId', render: (l: SupplierScorecardLineDto) => <span className="font-mono">{l.supplierId}</span> },
                  { header: 'Orders', accessorKey: 'orderCount', render: (l: SupplierScorecardLineDto) => String(l.orderCount) },
                  { header: 'Total Value', accessorKey: 'totalOrderValue', render: (l: SupplierScorecardLineDto) => `$${l.totalOrderValue.toLocaleString()}` },
                  { header: 'Received %', accessorKey: 'fullyReceivedRate', render: (l: SupplierScorecardLineDto) => <span className="font-medium text-emerald-600 dark:text-emerald-400">{l.fullyReceivedRate.toFixed(1)}%</span> },
                ]}
                rows={supplierScorecardQuery.data}
                isLoading={supplierScorecardQuery.isLoading}
                emptyMessage="No supplier metrics available."
                rowKey={(l) => l.supplierId}
              />
            </Card>
          </div>
        </div>
      </div>
    </div>
  );
}

