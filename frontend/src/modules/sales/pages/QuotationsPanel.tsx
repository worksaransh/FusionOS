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
import { useCustomerOptions, useProductOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const lineSchema = z.object({
  productId: z.string().uuid('Pick a product'),
  quantity: z.string().refine((v) => Number(v) > 0, 'Quantity must be greater than zero'),
  unitPrice: z.string().refine((v) => Number(v) >= 0, 'Unit price cannot be negative'),
});

const schema = z.object({
  customerId: z.string().uuid('Pick a customer'),
  lines: z.array(lineSchema).min(1, 'At least one line is required'),
});
type FormValues = z.infer<typeof schema>;

interface QuotationLineDto {
  id: string;
  productId: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

interface QuotationDto {
  id: string;
  customerId: string;
  status: string;
  quotationDate: string;
  convertedSalesOrderId: string | null;
  totalAmount: number;
  lines: QuotationLineDto[];
}

/**
 * Quotations — a pre-Sales-Order stage, convertible into a real Sales Order
 * once accepted (docs/IMPLEMENTATION_PLAN.md Phase 10 item 8). Customer and
 * each line's Product are picked via the shared EntityCombobox, same
 * structure as SalesOrdersPanel. Converting creates a real SalesOrder in the
 * same request — see ConvertQuotationToSalesOrderCommandHandler.
 */
export function QuotationsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const customerOptions = useCustomerOptions(companyId);
  const productOptions = useProductOptions(companyId);

  const { control, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { customerId: '', lines: [{ productId: '', quantity: '1', unitPrice: '0' }] },
  });
  const { fields, append, remove } = useFieldArray({ control, name: 'lines' });

  const quotationsQuery = useQuery({
    queryKey: ['quotations', companyId],
    queryFn: () => apiClient.get<PagedResult<QuotationDto>>(`/sales/quotations?companyId=${companyId}&page=1&pageSize=25`),
    enabled: Boolean(companyId),
  });

  const createQuotation = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<QuotationDto>('/sales/quotations', {
        companyId,
        customerId: values.customerId,
        lines: values.lines.map((l) => ({ productId: l.productId, quantity: Number(l.quantity), unitPrice: Number(l.unitPrice) })),
      }),
    onSuccess: () => {
      reset({ customerId: '', lines: [{ productId: '', quantity: '1', unitPrice: '0' }] });
      queryClient.invalidateQueries({ queryKey: ['quotations', companyId] });
    },
  });

  const acceptQuotation = useMutation({
    mutationFn: (id: string) => apiClient.post(`/sales/quotations/${id}/accept?companyId=${companyId}`, {}),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['quotations', companyId] }),
  });

  const rejectQuotation = useMutation({
    mutationFn: (id: string) => apiClient.post(`/sales/quotations/${id}/reject?companyId=${companyId}`, {}),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['quotations', companyId] }),
  });

  const convertQuotation = useMutation({
    mutationFn: (id: string) => apiClient.post(`/sales/quotations/${id}/convert?companyId=${companyId}`, {}),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['quotations', companyId] });
      queryClient.invalidateQueries({ queryKey: ['sales-orders', companyId] });
    },
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Quotations</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createQuotation.mutate(values))} className="flex flex-col gap-4">
          <label className="flex flex-col gap-1 text-sm">
            Customer
            <Controller
              control={control}
              name="customerId"
              render={({ field }) => (
                <EntityCombobox
                  className="w-96"
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
            {isSubmitting ? 'Creating…' : 'Create quotation'}
          </Button>
        </form>
      </Card>

      {quotationsQuery.data && (
        <Card>
          <DataTable
            columns={[
              { header: 'Date', render: (quotation: QuotationDto) => new Date(quotation.quotationDate).toLocaleDateString() },
              { header: 'Status', render: (quotation: QuotationDto) => quotation.status },
              { header: 'Lines', render: (quotation: QuotationDto) => quotation.lines.length },
              { header: 'Total', render: (quotation: QuotationDto) => quotation.totalAmount.toLocaleString() },
              {
                header: '',
                render: (quotation: QuotationDto) => {
                  if (quotation.status === 'Draft') {
                    return (
                      <div className="flex items-center gap-2">
                        <Button variant="secondary" onClick={() => acceptQuotation.mutate(quotation.id)} disabled={acceptQuotation.isPending}>
                          Accept
                        </Button>
                        <Button variant="danger" onClick={() => rejectQuotation.mutate(quotation.id)} disabled={rejectQuotation.isPending}>
                          Reject
                        </Button>
                      </div>
                    );
                  }
                  if (quotation.status === 'Accepted') {
                    return (
                      <Button variant="secondary" onClick={() => convertQuotation.mutate(quotation.id)} disabled={convertQuotation.isPending}>
                        Convert to sales order
                      </Button>
                    );
                  }
                  return null;
                },
              },
            ]}
            rows={quotationsQuery.data.data}
            isLoading={quotationsQuery.isLoading}
            emptyMessage="No quotations yet."
            rowKey={(quotation) => quotation.id}
          />
        </Card>
      )}
      {createQuotation.isError && createQuotation.error instanceof ApiError && (
        <p role="alert" className="mt-2 text-sm text-danger">{createQuotation.error.problem.title}</p>
      )}
      {convertQuotation.isError && convertQuotation.error instanceof ApiError && (
        <p role="alert" className="mt-2 text-sm text-danger">{convertQuotation.error.problem.title}</p>
      )}
    </div>
  );
}
