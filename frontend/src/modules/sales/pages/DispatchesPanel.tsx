import { Controller, useFieldArray, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Plus, Trash2 } from 'lucide-react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { EntityCombobox } from '../../../shared/ui/EntityCombobox';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import { useProductOptions, useSalesOrderOptions, useWarehouseOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const lineSchema = z.object({
  productId: z.string().uuid('Pick a product'),
  quantityDispatched: z.string().refine((v) => Number(v) > 0, 'Quantity must be greater than zero'),
});

const schema = z.object({
  salesOrderId: z.string().uuid('Pick a sales order'),
  warehouseId: z.string().uuid('Pick a warehouse'),
  lines: z.array(lineSchema).min(1, 'At least one line is required'),
});
type FormValues = z.infer<typeof schema>;

interface DispatchLineDto {
  id: string;
  productId: string;
  quantityDispatched: number;
}

interface DispatchDto {
  id: string;
  salesOrderId: string;
  warehouseId: string;
  dispatchDate: string;
  lines: DispatchLineDto[];
}

/**
 * Dispatches — next slice after Sales Order (05_MODULE_ROADMAP.md Phase 1:
 * Sales capability list — "Dispatch"). Sales Order, Warehouse, and each
 * line's Product are picked via the shared EntityCombobox. Recording a
 * dispatch here automatically debits the Inventory Stock Ledger via
 * DispatchLineDispatchedConsumer once the Kafka consumer host is running.
 */
export function DispatchesPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const salesOrderOptions = useSalesOrderOptions(companyId);
  const warehouseOptions = useWarehouseOptions(companyId);
  const productOptions = useProductOptions(companyId);

  const { control, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { salesOrderId: '', warehouseId: '', lines: [{ productId: '', quantityDispatched: '1' }] },
  });
  const { fields, append, remove } = useFieldArray({ control, name: 'lines' });

  const dispatchesQuery = useQuery({
    queryKey: ['dispatches', companyId],
    queryFn: () => apiClient.get<PagedResult<DispatchDto>>(`/sales/dispatches?companyId=${companyId}&page=1&pageSize=25`),
    enabled: Boolean(companyId),
  });

  const createDispatch = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<DispatchDto>('/sales/dispatches', {
        companyId,
        salesOrderId: values.salesOrderId,
        warehouseId: values.warehouseId,
        lines: values.lines.map((l) => ({ productId: l.productId, quantityDispatched: Number(l.quantityDispatched) })),
      }),
    onSuccess: () => {
      reset({ salesOrderId: '', warehouseId: '', lines: [{ productId: '', quantityDispatched: '1' }] });
      queryClient.invalidateQueries({ queryKey: ['dispatches', companyId] });
    },
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Dispatches</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createDispatch.mutate(values))} className="flex flex-col gap-4">
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
              Warehouse
              <Controller
                control={control}
                name="warehouseId"
                render={({ field }) => (
                  <EntityCombobox
                    value={field.value}
                    onChange={field.onChange}
                    options={warehouseOptions.options}
                    isLoading={warehouseOptions.isLoading}
                    onSearchChange={warehouseOptions.onSearchChange}
                    placeholder="Search warehouses…"
                  />
                )}
              />
              {errors.warehouseId && <span className="text-xs text-danger">{errors.warehouseId.message}</span>}
            </label>
          </div>

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
                  Quantity dispatched
                  <Controller
                    control={control}
                    name={`lines.${index}.quantityDispatched`}
                    render={({ field: lineField }) => (
                      <input className="w-32 rounded-md border border-border bg-surface px-2 py-1.5" {...lineField} />
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
            <Button type="button" variant="secondary" onClick={() => append({ productId: '', quantityDispatched: '1' })} className="w-fit">
              <Plus size={16} className="mr-1" /> Add line
            </Button>
          </div>

          <Button type="submit" disabled={isSubmitting} className="w-fit">
            {isSubmitting ? 'Recording…' : 'Record dispatch'}
          </Button>
        </form>
      </Card>

      {dispatchesQuery.data && (
        <Card>
          <DataTable
            columns={[
              { header: 'Date', render: (dispatch: DispatchDto) => new Date(dispatch.dispatchDate).toLocaleString() },
              { header: 'Warehouse', render: (dispatch: DispatchDto) => warehouseOptions.options.find((w) => w.id === dispatch.warehouseId)?.label ?? dispatch.warehouseId },
              { header: 'Lines', render: (dispatch: DispatchDto) => dispatch.lines.length },
            ]}
            rows={dispatchesQuery.data.data}
            isLoading={dispatchesQuery.isLoading}
            emptyMessage="No dispatches yet."
            rowKey={(dispatch) => dispatch.id}
          />
        </Card>
      )}
    </div>
  );
}
