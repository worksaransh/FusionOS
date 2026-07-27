import { useState } from 'react';
import { Controller, useFieldArray, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Plus, Trash2 } from 'lucide-react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { EntityCombobox } from '../../../shared/ui/EntityCombobox';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import { useCustomerOptions, useInvoiceOptions, useProductOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const lineSchema = z.object({
  productId: z.string().uuid('Pick a product'),
  quantity: z.string().refine((v) => Number(v) > 0, 'Quantity must be greater than zero'),
});

const schema = z.object({
  invoiceId: z.string().uuid('Pick an invoice'),
  customerId: z.string().uuid('Pick a customer'),
  reason: z.string().min(1, 'A reason is required').max(500),
  lines: z.array(lineSchema).min(1, 'At least one line is required'),
});
type FormValues = z.infer<typeof schema>;

interface SalesReturnLineDto {
  id: string;
  productId: string;
  quantity: number;
  conditionNotes: string | null;
  restocked: boolean;
}

interface SalesReturnDto {
  id: string;
  invoiceId: string;
  customerId: string;
  reason: string;
  status: string;
  requestDate: string;
  rejectionReason: string | null;
  lines: SalesReturnLineDto[];
}

/** Local, per-row draft of each line's Complete()-time inputs, keyed by line id. */
type CompletionDraft = Record<string, { restocked: boolean; conditionNotes: string }>;

/**
 * Sales returns (RMA) — the physical-return workflow this codebase was missing:
 * a customer's REQUEST to send goods back against an Invoice, its Approval/
 * Rejection, and — once the goods physically arrive and are inspected — its
 * Completion (with a per-line restocked flag and condition notes). Deliberately
 * a distinct concern from CreditNotesPanel: a return can be Rejected with no
 * credit note ever created, or Completed and only later, separately, and
 * manually followed by a credit note (see CreditNotesPanel's optional
 * "sales return" reference) — this panel never auto-creates one.
 */
export function SalesReturnsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [completionDrafts, setCompletionDrafts] = useState<Record<string, CompletionDraft>>({});
  const [rejectingId, setRejectingId] = useState<string | null>(null);
  const [rejectionReason, setRejectionReason] = useState('');

  const invoiceOptions = useInvoiceOptions(companyId);
  const customerOptions = useCustomerOptions(companyId);
  const productOptions = useProductOptions(companyId);

  const { control, handleSubmit, reset, register, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { invoiceId: '', customerId: '', reason: '', lines: [{ productId: '', quantity: '1' }] },
  });
  const { fields, append, remove } = useFieldArray({ control, name: 'lines' });

  const salesReturnsQuery = useQuery({
    queryKey: ['sales-returns', companyId],
    queryFn: () => apiClient.get<PagedResult<SalesReturnDto>>(`/sales/returns?companyId=${companyId}&page=1&pageSize=25`),
    enabled: Boolean(companyId),
  });

  const createSalesReturn = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<SalesReturnDto>('/sales/returns', {
        companyId,
        invoiceId: values.invoiceId,
        customerId: values.customerId,
        reason: values.reason,
        lines: values.lines.map((l) => ({ productId: l.productId, quantity: Number(l.quantity) })),
      }),
    onSuccess: () => {
      reset({ invoiceId: '', customerId: '', reason: '', lines: [{ productId: '', quantity: '1' }] });
      queryClient.invalidateQueries({ queryKey: ['sales-returns', companyId] });
    },
  });

  const approveSalesReturn = useMutation({
    mutationFn: (id: string) => apiClient.post(`/sales/returns/${id}/approve?companyId=${companyId}`, {}),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['sales-returns', companyId] }),
  });

  const rejectSalesReturn = useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) =>
      apiClient.post(`/sales/returns/${id}/reject?companyId=${companyId}`, { rejectionReason: reason }),
    onSuccess: () => {
      setRejectingId(null);
      setRejectionReason('');
      queryClient.invalidateQueries({ queryKey: ['sales-returns', companyId] });
    },
  });

  const completeSalesReturn = useMutation({
    mutationFn: ({ id, lines }: { id: string; lines: SalesReturnLineDto[] }) => {
      const draft = completionDrafts[id] ?? {};
      return apiClient.post(`/sales/returns/${id}/complete?companyId=${companyId}`, {
        lines: lines.map((l) => ({
          lineId: l.id,
          restocked: draft[l.id]?.restocked ?? false,
          conditionNotes: draft[l.id]?.conditionNotes || null,
        })),
      });
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['sales-returns', companyId] }),
  });

  const updateDraft = (returnId: string, lineId: string, patch: Partial<{ restocked: boolean; conditionNotes: string }>) => {
    setCompletionDrafts((current) => ({
      ...current,
      [returnId]: {
        ...current[returnId],
        [lineId]: { ...{ restocked: false, conditionNotes: '' }, ...current[returnId]?.[lineId], ...patch },
      },
    }));
  };

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Sales returns (RMA)</h2>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createSalesReturn.mutate(values))} className="flex flex-col gap-4">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
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
          </div>

          <label className="flex flex-col gap-1 text-sm">
            Reason
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Wrong size, changed mind, etc." {...register('reason')} />
            {errors.reason && <span className="text-xs text-danger">{errors.reason.message}</span>}
          </label>

          <div className="flex flex-col gap-2">
            {fields.map((field, index) => (
              <div key={field.id} className="flex items-end gap-2">
                <label className="flex flex-col gap-1 text-sm">
                  Product
                  <Controller
                    control={control}
                    name={`lines.${index}.productId`}
                    render={({ field: lineField }) => (
                      <EntityCombobox
                        className="w-72"
                        value={lineField.value}
                        onChange={lineField.onChange}
                        options={productOptions.options}
                        isLoading={productOptions.isLoading}
                        onSearchChange={productOptions.onSearchChange}
                        placeholder="Search products…"
                      />
                    )}
                  />
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  Quantity
                  <Controller
                    control={control}
                    name={`lines.${index}.quantity`}
                    render={({ field: lineField }) => (
                      <input className="w-24 rounded-md border border-border bg-surface px-2 py-1.5" {...lineField} />
                    )}
                  />
                </label>
                <Button type="button" variant="secondary" onClick={() => remove(index)} disabled={fields.length === 1}>
                  <Trash2 size={16} />
                </Button>
              </div>
            ))}
            {errors.lines && typeof errors.lines.message === 'string' && (
              <span className="text-xs text-danger">{errors.lines.message}</span>
            )}
            <Button type="button" variant="secondary" onClick={() => append({ productId: '', quantity: '1' })} className="w-fit">
              <Plus size={16} className="mr-1" /> Add line
            </Button>
          </div>

          <Button type="submit" disabled={isSubmitting} className="w-fit">
            {isSubmitting ? 'Requesting…' : 'Request return'}
          </Button>
        </form>
      </Card>

      {salesReturnsQuery.data && (
        <Card>
          <DataTable
            columns={[
              { header: 'Date', render: (r: SalesReturnDto) => new Date(r.requestDate).toLocaleDateString() },
              { header: 'Reason', render: (r: SalesReturnDto) => r.reason },
              { header: 'Status', render: (r: SalesReturnDto) => r.status },
              { header: 'Lines', render: (r: SalesReturnDto) => r.lines.length },
              {
                header: 'Actions',
                render: (r: SalesReturnDto) => (
                  <div className="flex flex-wrap items-center gap-2">
                    {r.status === 'Requested' && (
                      <>
                        <Button variant="secondary" onClick={() => approveSalesReturn.mutate(r.id)} disabled={approveSalesReturn.isPending}>
                          Approve
                        </Button>
                        <Button
                          variant="danger"
                          onClick={() => setRejectingId((current) => (current === r.id ? null : r.id))}
                        >
                          Reject
                        </Button>
                      </>
                    )}
                  </div>
                ),
              },
            ]}
            rows={salesReturnsQuery.data.data}
            isLoading={salesReturnsQuery.isLoading}
            emptyMessage="No sales returns yet."
            rowKey={(r: SalesReturnDto) => r.id}
          />
        </Card>
      )}

      {rejectingId && (
        <Card className="mt-4">
          <div className="flex items-end gap-2">
            <label className="flex flex-1 flex-col gap-1 text-sm">
              Rejection reason
              <input
                className="rounded-md border border-border bg-surface px-2 py-1.5"
                value={rejectionReason}
                onChange={(e) => setRejectionReason(e.target.value)}
                placeholder="Outside the return window, etc."
              />
            </label>
            <Button
              variant="danger"
              disabled={!rejectionReason.trim() || rejectSalesReturn.isPending}
              onClick={() => rejectSalesReturn.mutate({ id: rejectingId, reason: rejectionReason })}
            >
              Confirm reject
            </Button>
            <Button variant="secondary" onClick={() => setRejectingId(null)}>Cancel</Button>
          </div>
        </Card>
      )}

      {salesReturnsQuery.data?.data.filter((r) => r.status === 'Approved').map((r) => (
        <Card key={r.id} className="mt-4">
          <h3 className="mb-2 text-sm font-semibold text-text">Complete return — inspection results</h3>
          <div className="flex flex-col gap-2">
            {r.lines.map((line) => {
              const draft = completionDrafts[r.id]?.[line.id] ?? { restocked: false, conditionNotes: '' };
              return (
                <div key={line.id} className="flex flex-wrap items-end gap-2 rounded-md border border-border p-2">
                  <span className="text-sm text-text-muted">Qty {line.quantity}</span>
                  <label className="flex items-center gap-1 text-sm">
                    <input
                      type="checkbox"
                      checked={draft.restocked}
                      onChange={(e) => updateDraft(r.id, line.id, { restocked: e.target.checked })}
                    />
                    Restocked
                  </label>
                  <label className="flex flex-col gap-1 text-sm">
                    Condition notes
                    <input
                      className="w-64 rounded-md border border-border bg-surface px-2 py-1.5"
                      value={draft.conditionNotes}
                      onChange={(e) => updateDraft(r.id, line.id, { conditionNotes: e.target.value })}
                      placeholder="Damaged, unopened/resellable, etc."
                    />
                  </label>
                </div>
              );
            })}
            <Button
              className="w-fit"
              disabled={completeSalesReturn.isPending}
              onClick={() => completeSalesReturn.mutate({ id: r.id, lines: r.lines })}
            >
              {completeSalesReturn.isPending ? 'Completing…' : 'Complete return'}
            </Button>
          </div>
        </Card>
      ))}

      {createSalesReturn.isError && createSalesReturn.error instanceof ApiError && (
        <p role="alert" className="mt-2 text-sm text-danger">{createSalesReturn.error.problem.title}</p>
      )}
    </div>
  );
}
