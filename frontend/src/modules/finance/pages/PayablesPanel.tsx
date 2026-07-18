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
import { useSupplierOptions, usePurchaseOrderOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const chargeSchema = z.object({
  supplierId: z.string().uuid('Pick a supplier'),
  purchaseOrderId: z.string().refine((v) => v === '' || /^[0-9a-fA-F-]{36}$/.test(v), 'Must be blank or a valid Purchase Order'),
  amount: z.string().refine((v) => Number(v) > 0, 'Amount must be greater than zero'),
  description: z.string().min(1, 'Description is required').max(500),
});
type ChargeFormValues = z.infer<typeof chargeSchema>;

const paymentSchema = z.object({
  supplierId: z.string().uuid('Pick a supplier'),
  purchaseOrderId: z.string().refine((v) => v === '' || /^[0-9a-fA-F-]{36}$/.test(v), 'Must be blank or a valid Purchase Order'),
  amount: z.string().refine((v) => Number(v) > 0, 'Amount must be greater than zero'),
  reference: z.string().optional(),
});
type PaymentFormValues = z.infer<typeof paymentSchema>;

interface SupplierBalanceDto {
  supplierId: string;
  balance: number;
}

interface ApLedgerEntryDto {
  id: string;
  supplierId: string;
  purchaseOrderId: string | null;
  amount: number;
  description: string;
  transactionDate: string;
}

/**
 * Accounts Payable — Phase M8c (2026-07-17), the mirror image of
 * ReceivablesPanel. Unlike AR, there's no consumer that auto-populates the
 * charge side yet (Procurement has no Supplier Invoice/Bill aggregate and no
 * PurchaseOrder-issued-implies-owed-money event — see ApLedgerEntry's class
 * doc comment for the scope decision), so this panel exposes both a
 * "record a bill charge" form (RecordBillChargeCommand) and a "record a
 * payment" form (RecordPaymentCommand), backed by PayablesController's
 * `/charges` and `/payments` actions respectively. Purchase Order is an
 * optional picker on both forms — an ad-hoc supplier bill has no PO at all.
 * The supplier's balance shown below is always re-fetched via
 * GetSupplierBalanceQuery after either action succeeds, same
 * recompute-don't-cache philosophy as ReceivablesPanel.
 */
export function PayablesPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const supplierOptions = useSupplierOptions(companyId);
  const purchaseOrderOptions = usePurchaseOrderOptions(companyId);

  const {
    control: chargeControl,
    handleSubmit: handleChargeSubmit,
    reset: resetCharge,
    formState: { errors: chargeErrors, isSubmitting: isChargeSubmitting },
  } = useForm<ChargeFormValues>({
    resolver: zodResolver(chargeSchema),
    defaultValues: { supplierId: '', purchaseOrderId: '', amount: '', description: '' },
  });

  const {
    control: paymentControl,
    handleSubmit: handlePaymentSubmit,
    reset: resetPayment,
    formState: { errors: paymentErrors, isSubmitting: isPaymentSubmitting },
  } = useForm<PaymentFormValues>({
    resolver: zodResolver(paymentSchema),
    defaultValues: { supplierId: '', purchaseOrderId: '', amount: '', reference: '' },
  });

  const selectedChargeSupplierId = useWatch({ control: chargeControl, name: 'supplierId' });
  const selectedPaymentSupplierId = useWatch({ control: paymentControl, name: 'supplierId' });
  // The ledger/balance shown below tracks whichever form's supplier was most
  // recently picked — same single-supplier-view idea as ReceivablesPanel,
  // just needing to pick between two independent forms' watched fields.
  const activeSupplierId = selectedPaymentSupplierId || selectedChargeSupplierId;

  const balanceQuery = useQuery({
    queryKey: ['supplier-balance', companyId, activeSupplierId],
    queryFn: () => apiClient.get<SupplierBalanceDto>(`/finance/payables/balance?companyId=${companyId}&supplierId=${activeSupplierId}`),
    enabled: Boolean(companyId && activeSupplierId),
  });

  const ledgerQuery = useQuery({
    queryKey: ['ap-ledger', companyId, activeSupplierId],
    queryFn: () => apiClient.get<PagedResult<ApLedgerEntryDto>>(`/finance/payables/ledger?companyId=${companyId}&supplierId=${activeSupplierId}&page=1&pageSize=25`),
    enabled: Boolean(companyId && activeSupplierId),
  });

  const recordCharge = useMutation({
    mutationFn: (values: ChargeFormValues) =>
      apiClient.post<ApLedgerEntryDto>('/finance/payables/charges', {
        companyId,
        supplierId: values.supplierId,
        purchaseOrderId: values.purchaseOrderId || null,
        amount: Number(values.amount),
        description: values.description,
      }),
    onSuccess: (_data, values) => {
      resetCharge({ supplierId: values.supplierId, purchaseOrderId: '', amount: '', description: '' });
      queryClient.invalidateQueries({ queryKey: ['supplier-balance', companyId, values.supplierId] });
      queryClient.invalidateQueries({ queryKey: ['ap-ledger', companyId, values.supplierId] });
    },
  });

  const recordPayment = useMutation({
    mutationFn: (values: PaymentFormValues) =>
      apiClient.post<ApLedgerEntryDto>('/finance/payables/payments', {
        companyId,
        supplierId: values.supplierId,
        purchaseOrderId: values.purchaseOrderId || null,
        amount: Number(values.amount),
        paymentDate: null,
        reference: values.reference || null,
      }),
    onSuccess: (_data, values) => {
      resetPayment({ supplierId: values.supplierId, purchaseOrderId: '', amount: '', reference: '' });
      queryClient.invalidateQueries({ queryKey: ['supplier-balance', companyId, values.supplierId] });
      queryClient.invalidateQueries({ queryKey: ['ap-ledger', companyId, values.supplierId] });
    },
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Accounts Payable</h2>
      <p className="mb-3 text-xs text-text-muted">
        Record a bill charge owed to a supplier, or record a payment made to a supplier. Purchase Order is optional —
        an ad-hoc supplier bill has no PO at all. A payment can never exceed the supplier's outstanding balance.
      </p>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <Card>
          <h3 className="mb-3 text-sm font-semibold text-text">Record a bill charge</h3>
          <form onSubmit={handleChargeSubmit((values) => recordCharge.mutate(values))} className="flex flex-col gap-4">
            <label className="flex flex-col gap-1 text-sm">
              Supplier
              <Controller
                control={chargeControl}
                name="supplierId"
                render={({ field }) => (
                  <EntityCombobox
                    value={field.value}
                    onChange={field.onChange}
                    options={supplierOptions.options}
                    isLoading={supplierOptions.isLoading}
                    onSearchChange={supplierOptions.onSearchChange}
                    placeholder="Search suppliers…"
                  />
                )}
              />
              {chargeErrors.supplierId && <span className="text-xs text-danger">{chargeErrors.supplierId.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Purchase Order (optional)
              <Controller
                control={chargeControl}
                name="purchaseOrderId"
                render={({ field }) => (
                  <EntityCombobox
                    value={field.value}
                    onChange={field.onChange}
                    options={purchaseOrderOptions.options}
                    isLoading={purchaseOrderOptions.isLoading}
                    placeholder="Search purchase orders…"
                  />
                )}
              />
              {chargeErrors.purchaseOrderId && <span className="text-xs text-danger">{chargeErrors.purchaseOrderId.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Amount
              <Controller
                control={chargeControl}
                name="amount"
                render={({ field }) => (
                  <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="0.00" {...field} />
                )}
              />
              {chargeErrors.amount && <span className="text-xs text-danger">{chargeErrors.amount.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Description
              <Controller
                control={chargeControl}
                name="description"
                render={({ field }) => (
                  <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="e.g. PO 123 — office supplies" {...field} />
                )}
              />
              {chargeErrors.description && <span className="text-xs text-danger">{chargeErrors.description.message}</span>}
            </label>
            <Button type="submit" disabled={isChargeSubmitting} className="w-fit">
              {isChargeSubmitting ? 'Recording…' : 'Record bill charge'}
            </Button>
          </form>
          {recordCharge.isError && recordCharge.error instanceof ApiError && (
            <p role="alert" className="mt-2 text-sm text-danger">{recordCharge.error.problem.title}</p>
          )}
        </Card>

        <Card>
          <h3 className="mb-3 text-sm font-semibold text-text">Record a payment</h3>
          <form onSubmit={handlePaymentSubmit((values) => recordPayment.mutate(values))} className="flex flex-col gap-4">
            <label className="flex flex-col gap-1 text-sm">
              Supplier
              <Controller
                control={paymentControl}
                name="supplierId"
                render={({ field }) => (
                  <EntityCombobox
                    value={field.value}
                    onChange={field.onChange}
                    options={supplierOptions.options}
                    isLoading={supplierOptions.isLoading}
                    onSearchChange={supplierOptions.onSearchChange}
                    placeholder="Search suppliers…"
                  />
                )}
              />
              {paymentErrors.supplierId && <span className="text-xs text-danger">{paymentErrors.supplierId.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Purchase Order (optional)
              <Controller
                control={paymentControl}
                name="purchaseOrderId"
                render={({ field }) => (
                  <EntityCombobox
                    value={field.value}
                    onChange={field.onChange}
                    options={purchaseOrderOptions.options}
                    isLoading={purchaseOrderOptions.isLoading}
                    placeholder="Search purchase orders…"
                  />
                )}
              />
              {paymentErrors.purchaseOrderId && <span className="text-xs text-danger">{paymentErrors.purchaseOrderId.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Amount
              <Controller
                control={paymentControl}
                name="amount"
                render={({ field }) => (
                  <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="0.00" {...field} />
                )}
              />
              {paymentErrors.amount && <span className="text-xs text-danger">{paymentErrors.amount.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Reference (optional)
              <Controller
                control={paymentControl}
                name="reference"
                render={({ field }) => (
                  <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Wire / cheque no." {...field} />
                )}
              />
            </label>

            {selectedPaymentSupplierId && balanceQuery.data && (
              <p className="text-sm text-text-muted">
                Current balance for this supplier: <span className="font-semibold text-text">{balanceQuery.data.balance.toLocaleString()}</span>
              </p>
            )}

            <Button type="submit" disabled={isPaymentSubmitting} className="w-fit">
              {isPaymentSubmitting ? 'Recording…' : 'Record payment'}
            </Button>
          </form>
          {recordPayment.isError && recordPayment.error instanceof ApiError && (
            <p role="alert" className="mt-2 text-sm text-danger">{recordPayment.error.problem.title}</p>
          )}
        </Card>
      </div>

      {activeSupplierId && balanceQuery.data && (
        <p className="mt-4 text-sm text-text-muted">
          Outstanding balance: <span className="font-semibold text-text">{balanceQuery.data.balance.toLocaleString()}</span>
        </p>
      )}

      {activeSupplierId && ledgerQuery.data && (
        <Card className="mt-4">
          <DataTable
            columns={[
              { header: 'Date', render: (entry: ApLedgerEntryDto) => new Date(entry.transactionDate).toLocaleDateString() },
              { header: 'Description', render: (entry: ApLedgerEntryDto) => entry.description },
              { header: 'Amount', render: (entry: ApLedgerEntryDto) => entry.amount.toLocaleString() },
            ]}
            rows={ledgerQuery.data.data}
            isLoading={ledgerQuery.isLoading}
            emptyMessage="No ledger entries for this supplier yet."
            rowKey={(entry) => entry.id}
          />
        </Card>
      )}
    </div>
  );
}
