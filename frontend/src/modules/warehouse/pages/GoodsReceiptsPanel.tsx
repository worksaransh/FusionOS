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
import {
  usePurchaseOrderOptions,
  useProductOptions,
  useSupplierOptions,
  useWarehouseOptions,
  useZoneOptions,
} from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const lineSchema = z.object({
  productId: z.string().uuid('Pick a product'),
  quantityReceived: z.string().refine((v) => Number(v) > 0, 'Quantity must be greater than zero'),
  unitCost: z.string().refine((v) => v === '' || Number(v) >= 0, 'Unit cost cannot be negative'),
});

const schema = z.object({
  warehouseId: z.string().uuid('Pick a warehouse'),
  zoneId: z.string().uuid('Pick a zone'),
  purchaseOrderId: z.string().refine((v) => v === '' || /^[0-9a-fA-F-]{36}$/.test(v), 'Must be blank or a valid Purchase Order'),
  supplierId: z.string().refine((v) => v === '' || /^[0-9a-fA-F-]{36}$/.test(v), 'Must be blank or a valid Supplier'),
  lines: z.array(lineSchema).min(1, 'At least one line is required'),
});
type FormValues = z.infer<typeof schema>;

interface GoodsReceiptLineDto {
  id: string;
  productId: string;
  quantityReceived: number;
  unitCost: number | null;
}

interface GoodsReceiptDto {
  id: string;
  warehouseId: string;
  zoneId: string;
  purchaseOrderId: string | null;
  supplierId: string | null;
  receivedDate: string;
  lines: GoodsReceiptLineDto[];
}

/**
 * Goods Receipts — next slice after Zones (03_SYSTEM_ARCHITECTURE.md §4.2 event
 * catalog: "GoodsReceived.v1", produced by Warehouse). Warehouse, Zone,
 * Purchase Order, Supplier, and each line's Product are all picked via the
 * shared EntityCombobox rather than pasted raw ids. Recording a receipt here
 * automatically credits the Inventory Stock Ledger via
 * GoodsReceiptLineReceivedConsumer once the Kafka consumer host is running —
 * no manual duplicate Stock Adjustment needed anymore.
 */
export function GoodsReceiptsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [lookupWarehouseId, setLookupWarehouseId] = useState('');

  const warehouseOptions = useWarehouseOptions(companyId);
  const purchaseOrderOptions = usePurchaseOrderOptions(companyId);
  const supplierOptions = useSupplierOptions(companyId);
  const productOptions = useProductOptions(companyId);

  const { control, handleSubmit, watch, reset, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { warehouseId: '', zoneId: '', purchaseOrderId: '', supplierId: '', lines: [{ productId: '', quantityReceived: '1', unitCost: '' }] },
  });
  const { fields, append, remove } = useFieldArray({ control, name: 'lines' });
  const watchedWarehouseId = watch('warehouseId');
  const effectiveWarehouseId = lookupWarehouseId || watchedWarehouseId;
  const zoneOptions = useZoneOptions(companyId, effectiveWarehouseId || undefined);

  const receiptsQuery = useQuery({
    queryKey: ['goods-receipts', companyId, effectiveWarehouseId],
    queryFn: () =>
      apiClient.get<PagedResult<GoodsReceiptDto>>(
        `/warehouse/warehouses/${effectiveWarehouseId}/goods-receipts?companyId=${companyId}&page=1&pageSize=25`,
      ),
    enabled: Boolean(companyId && effectiveWarehouseId),
  });

  const createReceipt = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<GoodsReceiptDto>(`/warehouse/warehouses/${values.warehouseId}/goods-receipts`, {
        companyId,
        zoneId: values.zoneId,
        purchaseOrderId: values.purchaseOrderId || null,
        supplierId: values.supplierId || null,
        lines: values.lines.map((l) => ({
          productId: l.productId,
          quantityReceived: Number(l.quantityReceived),
          unitCost: l.unitCost === '' ? null : Number(l.unitCost),
        })),
      }),
    onSuccess: (_data, variables) => {
      reset({ warehouseId: variables.warehouseId, zoneId: '', purchaseOrderId: '', supplierId: '', lines: [{ productId: '', quantityReceived: '1', unitCost: '' }] });
      setLookupWarehouseId(variables.warehouseId);
      queryClient.invalidateQueries({ queryKey: ['goods-receipts', companyId, variables.warehouseId] });
    },
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Goods Receipts</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createReceipt.mutate(values))} className="flex flex-col gap-4">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <label className="flex flex-col gap-1 text-sm">
              Warehouse
              <Controller
                control={control}
                name="warehouseId"
                render={({ field }) => (
                  <EntityCombobox
                    value={field.value}
                    onChange={(id) => {
                      field.onChange(id);
                      setLookupWarehouseId(id);
                    }}
                    options={warehouseOptions.options}
                    isLoading={warehouseOptions.isLoading}
                    onSearchChange={warehouseOptions.onSearchChange}
                    placeholder="Search warehouses…"
                  />
                )}
              />
              {errors.warehouseId && <span className="text-xs text-danger">{errors.warehouseId.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Zone
              <Controller
                control={control}
                name="zoneId"
                render={({ field }) => (
                  <EntityCombobox
                    value={field.value}
                    onChange={field.onChange}
                    options={zoneOptions.options}
                    isLoading={zoneOptions.isLoading}
                    placeholder={effectiveWarehouseId ? 'Search zones…' : 'Pick a warehouse first'}
                    disabled={!effectiveWarehouseId}
                  />
                )}
              />
              {errors.zoneId && <span className="text-xs text-danger">{errors.zoneId.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Purchase Order (optional)
              <Controller
                control={control}
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
              {errors.purchaseOrderId && <span className="text-xs text-danger">{errors.purchaseOrderId.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Supplier (optional)
              <Controller
                control={control}
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
              {errors.supplierId && <span className="text-xs text-danger">{errors.supplierId.message}</span>}
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
                  Quantity received
                  <Controller
                    control={control}
                    name={`lines.${index}.quantityReceived`}
                    render={({ field: lineField }) => (
                      <input className="w-32 rounded-md border border-border bg-surface px-2 py-1.5" {...lineField} />
                    )}
                  />
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  Unit cost (optional)
                  <Controller
                    control={control}
                    name={`lines.${index}.unitCost`}
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
            <Button type="button" variant="secondary" onClick={() => append({ productId: '', quantityReceived: '1', unitCost: '' })} className="w-fit">
              <Plus size={16} className="mr-1" /> Add line
            </Button>
          </div>

          <Button type="submit" disabled={isSubmitting} className="w-fit">
            {isSubmitting ? 'Recording…' : 'Record goods receipt'}
          </Button>
        </form>
      </Card>

      {receiptsQuery.data && (
        <Card>
          <DataTable
            columns={[
              { header: 'Received', render: (receipt: GoodsReceiptDto) => new Date(receipt.receivedDate).toLocaleString() },
              { header: 'Zone', render: (receipt: GoodsReceiptDto) => zoneOptions.options.find((z) => z.id === receipt.zoneId)?.label ?? receipt.zoneId },
              { header: 'Purchase Order', render: (receipt: GoodsReceiptDto) => receipt.purchaseOrderId ?? '—' },
              { header: 'Lines', render: (receipt: GoodsReceiptDto) => receipt.lines.length },
            ]}
            rows={receiptsQuery.data.data}
            isLoading={receiptsQuery.isLoading}
            emptyMessage="No goods receipts yet for this warehouse."
            rowKey={(receipt) => receipt.id}
          />
        </Card>
      )}
      {createReceipt.isError && createReceipt.error instanceof ApiError && (
        <p role="alert" className="mt-2 text-sm text-danger">{createReceipt.error.problem.title}</p>
      )}
    </div>
  );
}
