import { Controller, useForm, useWatch } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { EntityCombobox } from '../../../shared/ui/EntityCombobox';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import { useCustomerOptions, useInvoiceOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  customerId: z.string().uuid('Pick a customer'),
  invoiceId: z.string().uuid('Pick an invoice'),
  amount: z.string().refine((v) => Number(v) > 0, 'Amount must be greater than zero'),
  reference: z.string().optional(),
});
type FormValues = z.infer<typeof schema>;

interface CustomerBalanceDto {
  customerId: string;
  balance: number;
}

interface ArLedgerEntryDto {
  id: string;
  customerId: string;
  invoiceId: string;
  amount: number;
  description: string;
  transactionDate: string;
}

/**
 * Accounts Receivable payments — Phase M4 (2026-07-15). Previously the AR
 * ledger only ever increased via InvoiceIssuedConsumer; this panel is the
 * first place a payment can be recorded against a specific invoice, backed
 * by RecordPaymentCommand (ReceivablesController POST /payments). The
 * customer's balance shown below is always re-fetched via
 * GetCustomerBalanceQuery after a successful payment, same
 * recompute-don't-cache philosophy as the rest of the AR ledger.
 */
export function ReceivablesPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const customerOptions = useCustomerOptions(companyId);
  const invoiceOptions = useInvoiceOptions(companyId);

  const { control, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { customerId: '', invoiceId: '', amount: '', reference: '' },
  });
  const selectedCustomerId = useWatch({ control, name: 'customerId' });

  const balanceQuery = useQuery({
    queryKey: ['customer-balance', companyId, selectedCustomerId],
    queryFn: () => apiClient.get<CustomerBalanceDto>(`/finance/receivables/balance?companyId=${companyId}&customerId=${selectedCustomerId}`),
    enabled: Boolean(companyId && selectedCustomerId),
  });

  const ledgerQuery = useQuery({
    queryKey: ['ar-ledger', companyId, selectedCustomerId],
    queryFn: () => apiClient.get<PagedResult<ArLedgerEntryDto>>(`/finance/receivables/ledger?companyId=${companyId}&customerId=${selectedCustomerId}&page=1&pageSize=25`),
    enabled: Boolean(companyId && selectedCustomerId),
  });

  const recordPayment = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<ArLedgerEntryDto>('/finance/receivables/payments', {
        companyId,
        customerId: values.customerId,
        invoiceId: values.invoiceId,
        amount: Number(values.amount),
        paymentDate: null,
        reference: values.reference || null,
      }),
    onSuccess: (_data, values) => {
      reset({ customerId: values.customerId, invoiceId: '', amount: '', reference: '' });
      queryClient.invalidateQueries({ queryKey: ['customer-balance', companyId, values.customerId] });
      queryClient.invalidateQueries({ queryKey: ['ar-ledger', companyId, values.customerId] });
    },
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Accounts Receivable — Payments</h2>
      <p className="mb-3 text-xs text-text-muted">
        Record a customer payment against a specific invoice. A payment can never exceed that invoice's outstanding balance.
      </p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => recordPayment.mutate(values))} className="flex flex-col gap-4">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <label className="flex flex-col gap-1 text-sm">
              Customer
              <Controller
                control={control}
                name="customerId"
                render={({ field }) => (
                  <EntityCombobox
                    value={field.value}
                    onChange={field.onChange}
                    options={customerOptions.options}
                    isLoading={customerOptions.isLoading}
                    onSearchChange={customerOptions.onSearchChange}
                    placeholder="Search customers…"
                  />
                )}
              />
              {errors.customerId && <span className="text-xs text-danger">{errors.customerId.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Invoice
              <Controller
                control={control}
                name="invoiceId"
                render={({ field }) => (
                  <EntityCombobox
                    value={field.value}
                    onChange={field.onChange}
                    options={invoiceOptions.options}
                    isLoading={invoiceOptions.isLoading}
                    placeholder="Search invoices…"
                  />
                )}
              />
              {errors.invoiceId && <span className="text-xs text-danger">{errors.invoiceId.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Amount
              <Controller
                control={control}
                name="amount"
                render={({ field }) => (
                  <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="0.00" {...field} />
                )}
              />
              {errors.amount && <span className="text-xs text-danger">{errors.amount.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Reference (optional)
              <Controller
                control={control}
                name="reference"
                render={({ field }) => (
                  <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="UTR / cheque no." {...field} />
                )}
              />
            </label>
          </div>

          {selectedCustomerId && balanceQuery.data && (
            <p className="text-sm text-text-muted">
              Current balance for this customer: <span className="font-semibold text-text">{balanceQuery.data.balance.toLocaleString()}</span>
            </p>
          )}

          <Button type="submit" disabled={isSubmitting} className="w-fit">
            {isSubmitting ? 'Recording…' : 'Record payment'}
          </Button>
        </form>
        {recordPayment.isError && recordPayment.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{recordPayment.error.problem.title}</p>
        )}
      </Card>

      {selectedCustomerId && ledgerQuery.data && (
        <Card>
          <DataTable
            columns={[
              { header: 'Date', render: (entry: ArLedgerEntryDto) => new Date(entry.transactionDate).toLocaleDateString() },
              { header: 'Description', render: (entry: ArLedgerEntryDto) => entry.description },
              { header: 'Amount', render: (entry: ArLedgerEntryDto) => entry.amount.toLocaleString() },
            ]}
            rows={ledgerQuery.data.data}
            isLoading={ledgerQuery.isLoading}
            emptyMessage="No ledger entries for this customer yet."
            rowKey={(entry) => entry.id}
          />
        </Card>
      )}
    </div>
  );
}
