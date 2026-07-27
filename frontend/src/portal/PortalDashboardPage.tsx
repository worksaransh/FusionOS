import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { portalApiClient, PortalApiError } from './portalApiClient';
import { Card } from '../shared/ui/Card';
import { Button } from '../shared/ui/Button';
import { DataTable } from '../shared/ui/DataTable';
import type { PagedResult } from '../shared/api/types';

interface QuotationDto {
  id: string;
  status: string;
  quotationDate: string;
  totalAmount: number;
}

interface SalesOrderDto {
  id: string;
  status: string;
  orderDate: string;
  totalAmount: number;
}

interface InvoiceDto {
  id: string;
  status: string;
  invoiceDate: string;
  totalAmount: number;
}

interface DispatchDto {
  id: string;
  salesOrderId: string;
  dispatchDate: string;
}

type Tab = 'quotations' | 'orders' | 'invoices' | 'dispatches';

const TABS: { key: Tab; label: string }[] = [
  { key: 'quotations', label: 'Quotations' },
  { key: 'orders', label: 'Sales Orders' },
  { key: 'invoices', label: 'Invoices' },
  { key: 'dispatches', label: 'Dispatches' },
];

/**
 * The Customer Portal's one real screen (Customer Portal, 2026-07-22) - a
 * deliberately minimal, read-mostly view of the signed-in customer's own
 * Quotations/Sales Orders/Invoices/Dispatches, with Accept/Reject on pending
 * Quotations. Every request goes through portalApiClient, which attaches the
 * portal's own token; the backend (PortalController) derives which
 * customer's data to return purely from that token's own claim, never from
 * anything sent here - there is no customerId anywhere in this file.
 */
export function PortalDashboardPage() {
  const [tab, setTab] = useState<Tab>('quotations');
  const queryClient = useQueryClient();

  const quotationsQuery = useQuery({
    queryKey: ['portal', 'quotations'],
    queryFn: () => portalApiClient.get<PagedResult<QuotationDto>>('/sales/portal/quotations?page=1&pageSize=25'),
    enabled: tab === 'quotations',
  });

  const salesOrdersQuery = useQuery({
    queryKey: ['portal', 'sales-orders'],
    queryFn: () => portalApiClient.get<PagedResult<SalesOrderDto>>('/sales/portal/sales-orders?page=1&pageSize=25'),
    enabled: tab === 'orders',
  });

  const invoicesQuery = useQuery({
    queryKey: ['portal', 'invoices'],
    queryFn: () => portalApiClient.get<PagedResult<InvoiceDto>>('/sales/portal/invoices?page=1&pageSize=25'),
    enabled: tab === 'invoices',
  });

  const dispatchesQuery = useQuery({
    queryKey: ['portal', 'dispatches'],
    queryFn: () => portalApiClient.get<PagedResult<DispatchDto>>('/sales/portal/dispatches?page=1&pageSize=25'),
    enabled: tab === 'dispatches',
  });

  const acceptQuotation = useMutation({
    mutationFn: (id: string) => portalApiClient.post(`/sales/portal/quotations/${id}/accept`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['portal', 'quotations'] }),
  });

  const rejectQuotation = useMutation({
    mutationFn: (id: string) => portalApiClient.post(`/sales/portal/quotations/${id}/reject`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['portal', 'quotations'] }),
  });

  return (
    <div>
      <h1 className="mb-4 text-xl font-semibold text-text">My account</h1>

      <div className="mb-4 flex gap-2 border-b border-border">
        {TABS.map((t) => (
          <button
            key={t.key}
            type="button"
            onClick={() => setTab(t.key)}
            className={`px-3 py-2 text-sm font-medium ${
              tab === t.key ? 'border-b-2 border-primary text-text' : 'text-text-muted'
            }`}
          >
            {t.label}
          </button>
        ))}
      </div>

      {tab === 'quotations' && (
        <Card>
          <DataTable
            columns={[
              { header: 'Date', render: (q: QuotationDto) => new Date(q.quotationDate).toLocaleDateString() },
              { header: 'Status', render: (q: QuotationDto) => q.status },
              { header: 'Total', render: (q: QuotationDto) => q.totalAmount.toLocaleString() },
              {
                header: '',
                render: (q: QuotationDto) =>
                  q.status === 'Draft' ? (
                    <div className="flex items-center gap-2">
                      <Button variant="secondary" onClick={() => acceptQuotation.mutate(q.id)} disabled={acceptQuotation.isPending}>
                        Accept
                      </Button>
                      <Button variant="danger" onClick={() => rejectQuotation.mutate(q.id)} disabled={rejectQuotation.isPending}>
                        Reject
                      </Button>
                    </div>
                  ) : null,
              },
            ]}
            rows={quotationsQuery.data?.data}
            isLoading={quotationsQuery.isLoading}
            isError={quotationsQuery.isError}
            errorMessage={quotationsQuery.error instanceof PortalApiError ? quotationsQuery.error.problem.title : 'Could not load quotations.'}
            emptyMessage="No quotations yet."
            rowKey={(q) => q.id}
          />
        </Card>
      )}

      {tab === 'orders' && (
        <Card>
          <DataTable
            columns={[
              { header: 'Date', render: (o: SalesOrderDto) => new Date(o.orderDate).toLocaleDateString() },
              { header: 'Status', render: (o: SalesOrderDto) => o.status },
              { header: 'Total', render: (o: SalesOrderDto) => o.totalAmount.toLocaleString() },
            ]}
            rows={salesOrdersQuery.data?.data}
            isLoading={salesOrdersQuery.isLoading}
            isError={salesOrdersQuery.isError}
            errorMessage={salesOrdersQuery.error instanceof PortalApiError ? salesOrdersQuery.error.problem.title : 'Could not load sales orders.'}
            emptyMessage="No sales orders yet."
            rowKey={(o) => o.id}
          />
        </Card>
      )}

      {tab === 'invoices' && (
        <Card>
          <DataTable
            columns={[
              { header: 'Date', render: (i: InvoiceDto) => new Date(i.invoiceDate).toLocaleDateString() },
              { header: 'Status', render: (i: InvoiceDto) => i.status },
              { header: 'Total', render: (i: InvoiceDto) => i.totalAmount.toLocaleString() },
            ]}
            rows={invoicesQuery.data?.data}
            isLoading={invoicesQuery.isLoading}
            isError={invoicesQuery.isError}
            errorMessage={invoicesQuery.error instanceof PortalApiError ? invoicesQuery.error.problem.title : 'Could not load invoices.'}
            emptyMessage="No invoices yet."
            rowKey={(i) => i.id}
          />
        </Card>
      )}

      {tab === 'dispatches' && (
        <Card>
          <DataTable
            columns={[
              { header: 'Date', render: (d: DispatchDto) => new Date(d.dispatchDate).toLocaleDateString() },
              { header: 'Sales order', render: (d: DispatchDto) => d.salesOrderId },
            ]}
            rows={dispatchesQuery.data?.data}
            isLoading={dispatchesQuery.isLoading}
            isError={dispatchesQuery.isError}
            errorMessage={dispatchesQuery.error instanceof PortalApiError ? dispatchesQuery.error.problem.title : 'Could not load dispatches.'}
            emptyMessage="No dispatches yet."
            rowKey={(d) => d.id}
          />
        </Card>
      )}
    </div>
  );
}
