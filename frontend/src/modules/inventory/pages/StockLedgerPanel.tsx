import { useState } from 'react';
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { EntityCombobox } from '../../../shared/ui/EntityCombobox';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import { useProductOptions, useWarehouseOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  productId: z.string().uuid('Pick a product'),
  warehouseId: z.string().uuid('Pick a warehouse'),
  quantityDelta: z
    .string()
    .refine((v) => !Number.isNaN(Number(v)) && Number(v) !== 0, 'Quantity delta must be a non-zero number'),
  reason: z.string().min(1, 'Reason is required').max(500),
});
type FormValues = z.infer<typeof schema>;

interface LedgerEntryDto {
  id: string;
  productId: string;
  warehouseId: string;
  quantityDelta: number;
  unitCost: number | null;
  reason: string;
  transactionDate: string;
}

/**
 * Stock Ledger — next slice after Product (04_DATABASE_GUIDELINES.md §12).
 * Product and Warehouse are picked via the shared EntityCombobox
 * (shared/ui/EntityCombobox.tsx) rather than pasted raw ids.
 */
export function StockLedgerPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [lookupProductId, setLookupProductId] = useState('');

  const productOptions = useProductOptions(companyId);
  const warehouseOptions = useWarehouseOptions(companyId);

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { productId: '', warehouseId: '', quantityDelta: '', reason: '' },
  });

  const adjustStock = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post('/inventory/stock/adjustments', {
        companyId,
        productId: values.productId,
        warehouseId: values.warehouseId,
        quantityDelta: Number(values.quantityDelta),
        reason: values.reason,
        unitCost: null,
      }),
    onSuccess: (_data, variables) => {
      reset({ productId: '', warehouseId: '', quantityDelta: '', reason: '' });
      setLookupProductId(variables.productId);
      queryClient.invalidateQueries({ queryKey: ['stock-on-hand', companyId, variables.productId] });
      queryClient.invalidateQueries({ queryKey: ['ledger', companyId, variables.productId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const stockOnHandQuery = useQuery({
    queryKey: ['stock-on-hand', companyId, lookupProductId],
    queryFn: () => apiClient.get<{ quantityOnHand: number }>(`/inventory/stock/on-hand?companyId=${companyId}&productId=${lookupProductId}`),
    enabled: Boolean(companyId && lookupProductId),
  });

  const ledgerQuery = useQuery({
    queryKey: ['ledger', companyId, lookupProductId],
    queryFn: () => apiClient.get<PagedResult<LedgerEntryDto>>(`/inventory/stock/ledger?companyId=${companyId}&productId=${lookupProductId}&page=1&pageSize=25`),
    enabled: Boolean(companyId && lookupProductId),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Stock Ledger</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => adjustStock.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Product
            <Controller
              control={control}
              name="productId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={productOptions.options}
                  isLoading={productOptions.isLoading}
                  onSearchChange={productOptions.onSearchChange}
                  placeholder="Search products…"
                />
              )}
            />
            {errors.productId && <span className="text-xs text-danger">{errors.productId.message}</span>}
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
          <label className="flex flex-col gap-1 text-sm">
            Quantity delta (+ receipt, - issue)
            <Controller
              control={control}
              name="quantityDelta"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.quantityDelta && <span className="text-xs text-danger">{errors.quantityDelta.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Reason
            <Controller
              control={control}
              name="reason"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.reason && <span className="text-xs text-danger">{errors.reason.message}</span>}
          </label>
          <div className="col-span-2">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Recording…' : 'Record adjustment'}</Button>
          </div>
        </form>
      </Card>

      <Card className="mb-4">
        <label className="flex flex-col gap-1 text-sm">
          View ledger for product
          <EntityCombobox
            value={lookupProductId}
            onChange={setLookupProductId}
            options={productOptions.options}
            isLoading={productOptions.isLoading}
            onSearchChange={productOptions.onSearchChange}
            placeholder="Search products…"
          />
        </label>
        {stockOnHandQuery.data && (
          <p className="mt-3 text-sm">
            Quantity on hand: <span className="font-semibold">{stockOnHandQuery.data.quantityOnHand}</span>
          </p>
        )}
      </Card>

      {ledgerQuery.data && (
        <Card>
          <DataTable
            columns={[
              { header: 'Date', render: (entry: LedgerEntryDto) => new Date(entry.transactionDate).toLocaleString() },
              { header: 'Warehouse', render: (entry: LedgerEntryDto) => warehouseOptions.options.find((w) => w.id === entry.warehouseId)?.label ?? entry.warehouseId },
              { header: 'Qty delta', render: (entry: LedgerEntryDto) => entry.quantityDelta },
              { header: 'Reason', render: (entry: LedgerEntryDto) => entry.reason },
            ]}
            rows={ledgerQuery.data.data}
            isLoading={ledgerQuery.isLoading}
            emptyMessage="No ledger entries for this product yet."
            rowKey={(entry) => entry.id}
          />
        </Card>
      )}
    </div>
  );
}
