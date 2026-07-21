import { useState } from 'react';
import { Controller, FormProvider, useFieldArray, useForm, useFormContext, useWatch } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Plus, Trash2 } from 'lucide-react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { AttachmentsPanel } from '../../../shared/ui/AttachmentsPanel';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { EntityCombobox } from '../../../shared/ui/EntityCombobox';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import { useCustomerOptions, useProductOptions, useSalesOrderOptions, useTaxJurisdictionOptions, useTaxRateOptions, useUserOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const lineSchema = z.object({
  productId: z.string().uuid('Pick a product'),
  quantity: z.string().refine((v) => Number(v) > 0, 'Quantity must be greater than zero'),
  unitPrice: z.string().refine((v) => Number(v) >= 0, 'Unit price cannot be negative'),
  taxJurisdictionId: z.string().optional(),
  taxRateId: z.string().optional(),
  taxAmount: z.string().optional(),
});

const schema = z.object({
  salesOrderId: z.string().uuid('Pick a sales order'),
  customerId: z.string().uuid('Pick a customer'),
  salesPersonId: z.string().uuid().optional().or(z.literal('')),
  lines: z.array(lineSchema).min(1, 'At least one line is required'),
});
type FormValues = z.infer<typeof schema>;

const EMPTY_LINE = { productId: '', quantity: '1', unitPrice: '0', taxJurisdictionId: '', taxRateId: '', taxAmount: '0' };

interface InvoiceLineDto {
  id: string;
  productId: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
  taxRateId: string | null;
  taxAmount: number;
}

interface InvoiceDto {
  id: string;
  salesOrderId: string;
  customerId: string;
  status: string;
  invoiceDate: string;
  totalAmount: number;
  lines: InvoiceLineDto[];
  salesPersonId: string | null;
}

interface LineTaxResultDto {
  netAmount: number;
  taxRateId: string;
  taxRateCode: string;
  percentage: number;
  taxAmount: number;
  grossAmount: number;
}

/**
 * Invoices — next slice after Sales Order (05_MODULE_ROADMAP.md Phase 1: Sales
 * capability list — "Invoice"). Sales Order, Customer, and each line's Product
 * are picked via the shared EntityCombobox. Salesperson is optional (opaque
 * cross-module reference into Core's User, never existence-validated — same
 * convention as ProductId — feeds the Sales Commissions summary report,
 * docs/IMPLEMENTATION_PLAN.md Phase 10 item 11) and reuses the same
 * useUserOptions hook as the Approvals page.
 *
 * Tax (Phase 2 closeout, 2026-07-18): each line optionally picks a tax
 * jurisdiction then a rate within it, and a "Calculate tax" button calls
 * Finance's CalculateLineTaxQuery (previously wired end-to-end in the schema
 * but never actually invoked by any UI) to fill in the line's tax amount —
 * a lookup the user triggers and can see, not a silent auto-calculation on
 * every keystroke.
 */
export function InvoicesPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [expandedInvoiceId, setExpandedInvoiceId] = useState<string | null>(null);

  const salesOrderOptions = useSalesOrderOptions(companyId);
  const customerOptions = useCustomerOptions(companyId);
  const productOptions = useProductOptions(companyId);
  const userOptions = useUserOptions(companyId);

  const formMethods = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { salesOrderId: '', customerId: '', salesPersonId: '', lines: [EMPTY_LINE] },
  });
  const { control, handleSubmit, reset, formState: { errors, isSubmitting } } = formMethods;
  const { fields, append, remove } = useFieldArray({ control, name: 'lines' });

  const invoicesQuery = useQuery({
    queryKey: ['invoices', companyId],
    queryFn: () => apiClient.get<PagedResult<InvoiceDto>>(`/sales/invoices?companyId=${companyId}&page=1&pageSize=25`),
    enabled: Boolean(companyId),
  });

  const createInvoice = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<InvoiceDto>('/sales/invoices', {
        companyId,
        salesOrderId: values.salesOrderId,
        customerId: values.customerId,
        salesPersonId: values.salesPersonId || null,
        lines: values.lines.map((l) => ({
          productId: l.productId,
          quantity: Number(l.quantity),
          unitPrice: Number(l.unitPrice),
          taxRateId: l.taxRateId || null,
          taxAmount: Number(l.taxAmount || 0),
        })),
      }),
    onSuccess: () => {
      reset({ salesOrderId: '', customerId: '', salesPersonId: '', lines: [EMPTY_LINE] });
      queryClient.invalidateQueries({ queryKey: ['invoices', companyId] });
    },
  });

  const issueInvoice = useMutation({
    mutationFn: (id: string) => apiClient.post(`/sales/invoices/${id}/issue?companyId=${companyId}`, {}),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['invoices', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Invoices</h2>

      <Card className="mb-6">
        <FormProvider {...formMethods}>
          <form onSubmit={handleSubmit((values) => createInvoice.mutate(values))} className="flex flex-col gap-4">
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <label className="flex flex-col gap-1 text-sm">
                Sales Order
                <Controller
                  control={control}
                  name="salesOrderId"
                  render={({ field }) => (
                    <EntityCombobox
                      value={field.value}
                      onChange={field.onChange}
                      options={salesOrderOptions.options}
                      isLoading={salesOrderOptions.isLoading}
                      placeholder="Search sales orders…"
                    />
                  )}
                />
                {errors.salesOrderId && <span className="text-xs text-danger">{errors.salesOrderId.message}</span>}
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
              <label className="flex flex-col gap-1 text-sm">
                Salesperson (optional)
                <Controller
                  control={control}
                  name="salesPersonId"
                  render={({ field }) => (
                    <EntityCombobox
                      value={field.value ?? ''}
                      onChange={field.onChange}
                      options={userOptions.options}
                      isLoading={userOptions.isLoading}
                      onSearchChange={userOptions.onSearchChange}
                      placeholder="Search users…"
                    />
                  )}
                />
                {errors.salesPersonId && <span className="text-xs text-danger">{errors.salesPersonId.message}</span>}
              </label>
            </div>

            <div className="flex flex-col gap-2">
              {fields.map((field, index) => (
                <InvoiceLineFields
                  key={field.id}
                  index={index}
                  companyId={companyId}
                  productOptions={productOptions}
                  onRemove={() => remove(index)}
                  removeDisabled={fields.length === 1}
                />
              ))}
              {errors.lines && typeof errors.lines.message === 'string' && (
                <span className="text-xs text-danger">{errors.lines.message}</span>
              )}
              <Button type="button" variant="secondary" onClick={() => append(EMPTY_LINE)} className="w-fit">
                <Plus size={16} className="mr-1" /> Add line
              </Button>
            </div>

            <Button type="submit" disabled={isSubmitting} className="w-fit">
              {isSubmitting ? 'Creating…' : 'Create invoice'}
            </Button>
          </form>
        </FormProvider>
      </Card>

      {invoicesQuery.data && (
        <Card>
          <DataTable
            columns={[
              { header: 'Date', render: (invoice: InvoiceDto) => new Date(invoice.invoiceDate).toLocaleDateString() },
              { header: 'Status', render: (invoice: InvoiceDto) => invoice.status },
              { header: 'Lines', render: (invoice: InvoiceDto) => invoice.lines.length },
              { header: 'Total', render: (invoice: InvoiceDto) => invoice.totalAmount.toLocaleString() },
              {
                header: '',
                render: (invoice: InvoiceDto) =>
                  invoice.status === 'Draft' ? (
                    <Button variant="secondary" onClick={() => issueInvoice.mutate(invoice.id)} disabled={issueInvoice.isPending}>
                      Issue
                    </Button>
                  ) : null,
              },
              {
                header: '',
                render: (invoice: InvoiceDto) => (
                  <Button
                    variant="secondary"
                    onClick={() => setExpandedInvoiceId((current) => (current === invoice.id ? null : invoice.id))}
                  >
                    {expandedInvoiceId === invoice.id ? 'Hide attachments' : 'Attachments'}
                  </Button>
                ),
              },
            ]}
            rows={invoicesQuery.data.data}
            isLoading={invoicesQuery.isLoading}
            emptyMessage="No invoices yet."
            rowKey={(invoice) => invoice.id}
          />
        </Card>
      )}

      {expandedInvoiceId && (
        <Card className="mt-4">
          <AttachmentsPanel entityType="Invoice" entityId={expandedInvoiceId} />
        </Card>
      )}
      {createInvoice.isError && createInvoice.error instanceof ApiError && (
        <p role="alert" className="mt-2 text-sm text-danger">{createInvoice.error.problem.title}</p>
      )}
    </div>
  );
}

interface InvoiceLineFieldsProps {
  index: number;
  companyId: string;
  productOptions: ReturnType<typeof useProductOptions>;
  onRemove: () => void;
  removeDisabled: boolean;
}

/** One invoice line's fields, including its own tax jurisdiction/rate pickers and "Calculate tax" action — a dedicated subcomponent since each line needs its own tax-rate options scoped to whichever jurisdiction it picked. */
function InvoiceLineFields({ index, companyId, productOptions, onRemove, removeDisabled }: InvoiceLineFieldsProps) {
  const { control, setValue } = useFormContext<FormValues>();
  const taxJurisdictionId = useWatch({ control, name: `lines.${index}.taxJurisdictionId` });
  const taxRateId = useWatch({ control, name: `lines.${index}.taxRateId` });
  const quantity = useWatch({ control, name: `lines.${index}.quantity` });
  const unitPrice = useWatch({ control, name: `lines.${index}.unitPrice` });

  const taxJurisdictionOptions = useTaxJurisdictionOptions(companyId);
  const taxRateOptions = useTaxRateOptions(companyId, taxJurisdictionId || undefined);

  const calculateTax = useMutation({
    mutationFn: () => {
      const netAmount = Number(quantity) * Number(unitPrice);
      return apiClient.get<LineTaxResultDto>(`/finance/tax-rates/calculate-line-tax?companyId=${companyId}&taxRateId=${taxRateId}&netAmount=${netAmount}`);
    },
    onSuccess: (result) => setValue(`lines.${index}.taxAmount`, String(result.taxAmount)),
  });

  return (
    <div className="flex flex-wrap items-end gap-2 rounded-md border border-border p-2">
      <label className="flex flex-col gap-1 text-sm">
        Product
        <Controller
          control={control}
          name={`lines.${index}.productId`}
          render={({ field: lineField }) => (
            <EntityCombobox
              className="w-64"
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
            <input className="w-20 rounded-md border border-border bg-surface px-2 py-1.5" {...lineField} />
          )}
        />
      </label>
      <label className="flex flex-col gap-1 text-sm">
        Unit price
        <Controller
          control={control}
          name={`lines.${index}.unitPrice`}
          render={({ field: lineField }) => (
            <input className="w-24 rounded-md border border-border bg-surface px-2 py-1.5" {...lineField} />
          )}
        />
      </label>
      <label className="flex flex-col gap-1 text-sm">
        Tax jurisdiction
        <Controller
          control={control}
          name={`lines.${index}.taxJurisdictionId`}
          render={({ field: lineField }) => (
            <EntityCombobox
              className="w-48"
              value={lineField.value ?? ''}
              onChange={(value) => {
                lineField.onChange(value);
                setValue(`lines.${index}.taxRateId`, '');
              }}
              options={taxJurisdictionOptions.options}
              isLoading={taxJurisdictionOptions.isLoading}
              onSearchChange={taxJurisdictionOptions.onSearchChange}
              placeholder="Optional…"
            />
          )}
        />
      </label>
      <label className="flex flex-col gap-1 text-sm">
        Tax rate
        <Controller
          control={control}
          name={`lines.${index}.taxRateId`}
          render={({ field: lineField }) => (
            <EntityCombobox
              className="w-48"
              value={lineField.value ?? ''}
              onChange={lineField.onChange}
              options={taxRateOptions.options}
              isLoading={taxRateOptions.isLoading}
              placeholder={taxJurisdictionId ? 'Pick a rate…' : 'Pick a jurisdiction first'}
            />
          )}
        />
      </label>
      <Button type="button" variant="secondary" disabled={!taxRateId || calculateTax.isPending} onClick={() => calculateTax.mutate()}>
        {calculateTax.isPending ? 'Calculating…' : 'Calculate tax'}
      </Button>
      <label className="flex flex-col gap-1 text-sm">
        Tax amount
        <Controller
          control={control}
          name={`lines.${index}.taxAmount`}
          render={({ field: lineField }) => (
            <input className="w-24 rounded-md border border-border bg-surface px-2 py-1.5" {...lineField} readOnly />
          )}
        />
      </label>
      <Button type="button" variant="secondary" onClick={onRemove} disabled={removeDisabled}>
        <Trash2 size={16} />
      </Button>
    </div>
  );
}
