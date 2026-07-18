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
  unitPrice: z.string().refine((v) => Number(v) >= 0, 'Unit price cannot be negative'),
});

const schema = z.object({
  invoiceId: z.string().uuid('Pick an invoice'),
  customerId: z.string().uuid('Pick a customer'),
  reason: z.string().min(1, 'A reason is required').max(500),
  lines: z.array(lineSchema).min(1, 'At least one line is required'),
});
type FormValues = z.infer<typeof schema>;

interface CreditNoteLineDto {
  id: string;
  productId: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

interface CreditNoteDto {
  id: string;
  invoiceId: string;
  customerId: string;
  reason: string;
  status: string;
  creditNoteDate: string;
  totalAmount: number;
  lines: CreditNoteLineDto[];
}

/**
 * Returns/credit notes — a return-from-customer flow that reverses an Invoice
 * and, once Issued, posts a credit against the customer's AR balance
 * (docs/IMPLEMENTATION_PLAN.md Phase 10 item 9). Invoice, Customer, and each
 * line's Product are picked via the shared EntityCombobox, same structure as
 * InvoicesPanel. The actual AR ledger posting from this is Finance's job — see
 * CreditNoteIssuedConsumer.
 */
export function CreditNotesPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const invoiceOptions = useInvoiceOptions(companyId);
  const customerOptions = useCustomerOptions(companyId);
  const productOptions = useProductOptions(companyId);

  const { control, handleSubmit, reset, register, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { invoiceId: '', customerId: '', reason: '', lines: [{ productId: '', quantity: '1', unitPrice: '0' }] },
  });
  const { fields, append, remove } = useFieldArray({ control, name: 'lines' });

  const creditNotesQuery = useQuery({
    queryKey: ['credit-notes', companyId],
    queryFn: () => apiClient.get<PagedResult<CreditNoteDto>>(`/sales/credit-notes?companyId=${companyId}&page=1&pageSize=25`),
    enabled: Boolean(companyId),
  });

  const createCreditNote = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<CreditNoteDto>('/sales/credit-notes', {
        companyId,
        invoiceId: values.invoiceId,
        customerId: values.customerId,
        reason: values.reason,
        lines: values.lines.map((l) => ({ productId: l.productId, quantity: Number(l.quantity), unitPrice: Number(l.unitPrice) })),
      }),
    onSuccess: () => {
      reset({ invoiceId: '', customerId: '', reason: '', lines: [{ productId: '', quantity: '1', unitPrice: '0' }] });
      queryClient.invalidateQueries({ queryKey: ['credit-notes', companyId] });
    },
  });

  const issueCreditNote = useMutation({
    mutationFn: (id: string) => apiClient.post(`/sales/credit-notes/${id}/issue?companyId=${companyId}`, {}),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['credit-notes', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Credit notes</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createCreditNote.mutate(values))} className="flex flex-col gap-4">
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
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Damaged goods, wrong item, etc." {...register('reason')} />
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
                <label className="flex flex-col gap-1 text-sm">
                  Unit price
                  <Controller
                    control={control}
                    name={`lines.${index}.unitPrice`}
                    render={({ field: lineField }) => (
                      <input className="w-28 rounded-md border border-border bg-surface px-2 py-1.5" {...lineField} />
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
            <Button type="button" variant="secondary" onClick={() => append({ productId: '', quantity: '1', unitPrice: '0' })} className="w-fit">
              <Plus size={16} className="mr-1" /> Add line
            </Button>
          </div>

          <Button type="submit" disabled={isSubmitting} className="w-fit">
            {isSubmitting ? 'Creating…' : 'Create credit note'}
          </Button>
        </form>
      </Card>

      {creditNotesQuery.data && (
        <Card>
          <DataTable
            columns={[
              { header: 'Date', render: (creditNote: CreditNoteDto) => new Date(creditNote.creditNoteDate).toLocaleDateString() },
              { header: 'Reason', render: (creditNote: CreditNoteDto) => creditNote.reason },
              { header: 'Status', render: (creditNote: CreditNoteDto) => creditNote.status },
              { header: 'Lines', render: (creditNote: CreditNoteDto) => creditNote.lines.length },
              { header: 'Total', render: (creditNote: CreditNoteDto) => creditNote.totalAmount.toLocaleString() },
              {
                header: '',
                render: (creditNote: CreditNoteDto) =>
                  creditNote.status === 'Draft' ? (
                    <Button variant="secondary" onClick={() => issueCreditNote.mutate(creditNote.id)} disabled={issueCreditNote.isPending}>
                      Issue
                    </Button>
                  ) : null,
              },
            ]}
            rows={creditNotesQuery.data.data}
            isLoading={creditNotesQuery.isLoading}
            emptyMessage="No credit notes yet."
            rowKey={(creditNote) => creditNote.id}
          />
        </Card>
      )}
      {createCreditNote.isError && createCreditNote.error instanceof ApiError && (
        <p role="alert" className="mt-2 text-sm text-danger">{createCreditNote.error.problem.title}</p>
      )}
    </div>
  );
}
