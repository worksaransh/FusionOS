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
import { useProductOptions, useSupplierOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const lineSchema = z.object({
  productId: z.string().uuid('Pick a product'),
  quantity: z.string().refine((v) => Number(v) > 0, 'Quantity must be greater than zero'),
  unitPrice: z.string().refine((v) => Number(v) >= 0, 'Unit price cannot be negative'),
});

const schema = z.object({
  supplierId: z.string().uuid('Pick a supplier'),
  lines: z.array(lineSchema).min(1, 'At least one line is required'),
});
type FormValues = z.infer<typeof schema>;

interface PurchaseOrderLineDto {
  id: string;
  productId: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

interface PurchaseOrderDto {
  id: string;
  supplierId: string;
  status: string;
  orderDate: string;
  totalAmount: number;
  lines: PurchaseOrderLineDto[];
}

/**
 * Purchase Orders — next slice after Supplier (05_MODULE_ROADMAP.md Phase 1).
 * Supplier and each line's Product are picked via the shared EntityCombobox.
 */
export function PurchaseOrdersPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const supplierOptions = useSupplierOptions(companyId);
  const productOptions = useProductOptions(companyId);

  const { control, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { supplierId: '', lines: [{ productId: '', quantity: '1', unitPrice: '0' }] },
  });
  const { fields, append, remove } = useFieldArray({ control, name: 'lines' });

  const ordersQuery = useQuery({
    queryKey: ['purchase-orders', companyId],
    queryFn: () => apiClient.get<PagedResult<PurchaseOrderDto>>(`/procurement/purchase-orders?companyId=${companyId}&page=1&pageSize=25`),
    enabled: Boolean(companyId),
  });

  const createOrder = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<PurchaseOrderDto>('/procurement/purchase-orders', {
        companyId,
        supplierId: values.supplierId,
        lines: values.lines.map((l) => ({ productId: l.productId, quantity: Number(l.quantity), unitPrice: Number(l.unitPrice) })),
      }),
    onSuccess: () => {
      reset({ supplierId: '', lines: [{ productId: '', quantity: '1', unitPrice: '0' }] });
      queryClient.invalidateQueries({ queryKey: ['purchase-orders', companyId] });
    },
  });

  const approveOrder = useMutation({
    mutationFn: (id: string) => apiClient.post(`/procurement/purchase-orders/${id}/approve?companyId=${companyId}`, {}),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['purchase-orders', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Purchase Orders</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createOrder.mutate(values))} className="flex flex-col gap-4">
          <label className="flex flex-col gap-1 text-sm">
            Supplier
            <Controller
              control={control}
              name="supplierId"
              render={({ field }) => (
                <EntityCombobox
                  className="w-96"
                  value={field.value}
                  onChange={field.onChange}
                  options={supplierOptions.options}
                  isLoading={supplierOptions.isLoading}
                  onSearchChange={supplierOptions.onSearchChange}
                  placeholder="Search suppliers…"
                />
              )}
            />
            {errors.supplierId && <span className="text-xs text-danger">{errors.supplierId.message}</span>}
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
            {isSubmitting ? 'Creating…' : 'Create purchase order'}
          </Button>
        </form>
      </Card>

      {ordersQuery.data && (
        <Card>
          <DataTable
            columns={[
              { header: 'Order date', render: (order: PurchaseOrderDto) => new Date(order.orderDate).toLocaleDateString() },
              { header: 'Status', render: (order: PurchaseOrderDto) => order.status },
              { header: 'Lines', render: (order: PurchaseOrderDto) => order.lines.length },
              { header: 'Total', render: (order: PurchaseOrderDto) => order.totalAmount.toLocaleString() },
              {
                header: '',
                render: (order: PurchaseOrderDto) =>
                  order.status === 'Draft' ? (
                    <Button variant="secondary" onClick={() => approveOrder.mutate(order.id)} disabled={approveOrder.isPending}>
                      Approve
                    </Button>
                  ) : null,
              },
            ]}
            rows={ordersQuery.data.data}
            isLoading={ordersQuery.isLoading}
            emptyMessage="No purchase orders yet."
            rowKey={(order) => order.id}
          />
        </Card>
      )}
      {createOrder.isError && createOrder.error instanceof ApiError && (
        <p role="alert" className="mt-2 text-sm text-danger">{createOrder.error.problem.title}</p>
      )}
    </div>
  );
}
