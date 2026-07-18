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
import { useBinOptions, useProductOptions, useWarehouseOptions, useZoneOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  warehouseId: z.string().uuid('Pick a warehouse'),
  zoneId: z.string().uuid('Pick a zone'),
  binId: z.string().uuid('Pick a bin'),
  productId: z.string().uuid('Pick a product'),
});
type FormValues = z.infer<typeof schema>;

interface StockOnHandDto {
  productId: string;
  warehouseId: string | null;
  quantity: number;
}

interface CycleCountDto {
  id: string;
  warehouseId: string;
  zoneId: string;
  binId: string;
  productId: string;
  startedBy: string;
  systemQuantitySnapshot: number;
  countedQuantity: number | null;
  varianceQuantity: number | null;
  status: 'Pending' | 'Completed';
  createdAt: string;
}

/**
 * Cycle counting — Warehouse-side (docs/IMPLEMENTATION_PLAN.md Phase 9:
 * "Cycle counting (warehouse side) — same concept as Inventory's, scoped to
 * a warehouse/zone/bin"). Two steps: start a count (this reads Inventory's
 * current on-hand quantity as the system-quantity snapshot — this module has
 * no cross-module read of its own, so the frontend does the one read and
 * hands the number to Start), then record what was physically counted.
 * A variance automatically becomes a Stock Ledger adjustment via
 * CycleCountVarianceRecordedConsumer — no separate "Adjust Stock" step needed.
 */
export function CycleCountsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [lookupWarehouseId, setLookupWarehouseId] = useState('');
  const [countInputs, setCountInputs] = useState<Record<string, string>>({});

  const warehouseOptions = useWarehouseOptions(companyId);
  const productOptions = useProductOptions(companyId);

  const { control, handleSubmit, watch, reset, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { warehouseId: '', zoneId: '', binId: '', productId: '' },
  });
  const watchedWarehouseId = watch('warehouseId');
  const watchedZoneId = watch('zoneId');
  const watchedProductId = watch('productId');
  const effectiveWarehouseId = lookupWarehouseId || watchedWarehouseId;
  const zoneOptions = useZoneOptions(companyId, effectiveWarehouseId || undefined);
  const binOptions = useBinOptions(companyId, effectiveWarehouseId || undefined, watchedZoneId || undefined);

  const onHandQuery = useQuery({
    queryKey: ['stock-on-hand', companyId, watchedProductId, effectiveWarehouseId],
    queryFn: () =>
      apiClient.get<StockOnHandDto>(
        `/inventory/stock/on-hand?companyId=${companyId}&productId=${watchedProductId}&warehouseId=${effectiveWarehouseId}`,
      ),
    enabled: Boolean(companyId && watchedProductId && effectiveWarehouseId),
  });

  const startCycleCount = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<CycleCountDto>(`/warehouse/warehouses/${values.warehouseId}/cycle-counts`, {
        companyId,
        zoneId: values.zoneId,
        binId: values.binId,
        productId: values.productId,
        systemQuantitySnapshot: onHandQuery.data?.quantity ?? 0,
      }),
    onSuccess: (_data, variables) => {
      reset({ warehouseId: variables.warehouseId, zoneId: '', binId: '', productId: '' });
      setLookupWarehouseId(variables.warehouseId);
      queryClient.invalidateQueries({ queryKey: ['cycle-counts', companyId, variables.warehouseId] });
    },
  });

  const cycleCountsQuery = useQuery({
    queryKey: ['cycle-counts', companyId, effectiveWarehouseId],
    queryFn: () =>
      apiClient.get<PagedResult<CycleCountDto>>(
        `/warehouse/warehouses/${effectiveWarehouseId}/cycle-counts?companyId=${companyId}&page=1&pageSize=25`,
      ),
    enabled: Boolean(companyId && effectiveWarehouseId),
  });

  const recordCycleCount = useMutation({
    mutationFn: ({ id, countedQuantity }: { id: string; countedQuantity: number }) =>
      apiClient.post<CycleCountDto>(`/warehouse/warehouses/${effectiveWarehouseId}/cycle-counts/${id}/record`, {
        companyId,
        countedQuantity,
      }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['cycle-counts', companyId, effectiveWarehouseId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Cycle Counts</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => startCycleCount.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
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
            Bin
            <Controller
              control={control}
              name="binId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={binOptions.options}
                  isLoading={binOptions.isLoading}
                  placeholder={watchedZoneId ? 'Search bins…' : 'Pick a zone first'}
                  disabled={!watchedZoneId}
                />
              )}
            />
            {errors.binId && <span className="text-xs text-danger">{errors.binId.message}</span>}
          </label>
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
          <div className="col-span-2 flex items-center gap-3">
            <p className="text-sm text-text-muted">
              System quantity on hand:{' '}
              <span className="font-medium text-text">
                {watchedProductId && effectiveWarehouseId
                  ? (onHandQuery.data?.quantity.toLocaleString() ?? (onHandQuery.isLoading ? 'Loading…' : '—'))
                  : 'Pick a warehouse + product'}
              </span>
            </p>
          </div>
          <div className="col-span-2">
            <Button type="submit" disabled={isSubmitting || !watchedProductId}>
              {isSubmitting ? 'Starting…' : 'Start cycle count'}
            </Button>
          </div>
        </form>
        {startCycleCount.isError && startCycleCount.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{startCycleCount.error.problem.title}</p>
        )}
      </Card>

      {cycleCountsQuery.data && (
        <Card>
          <DataTable
            columns={[
              { header: 'Bin', render: (c: CycleCountDto) => binOptions.options.find((b) => b.id === c.binId)?.label ?? c.binId },
              { header: 'Product', render: (c: CycleCountDto) => productOptions.options.find((p) => p.id === c.productId)?.label ?? c.productId },
              { header: 'System qty', render: (c: CycleCountDto) => c.systemQuantitySnapshot.toLocaleString() },
              { header: 'Counted qty', render: (c: CycleCountDto) => c.countedQuantity?.toLocaleString() ?? '—' },
              { header: 'Variance', render: (c: CycleCountDto) => c.varianceQuantity?.toLocaleString() ?? '—' },
              { header: 'Status', render: (c: CycleCountDto) => c.status },
              {
                header: 'Actions',
                render: (c: CycleCountDto) =>
                  c.status === 'Pending' ? (
                    <div className="flex items-center gap-2">
                      <input
                        className="w-24 rounded-md border border-border bg-surface px-2 py-1"
                        placeholder="Counted"
                        value={countInputs[c.id] ?? ''}
                        onChange={(e) => setCountInputs((prev) => ({ ...prev, [c.id]: e.target.value }))}
                      />
                      <Button
                        type="button"
                        disabled={
                          !countInputs[c.id] ||
                          Number(countInputs[c.id]) < 0 ||
                          (recordCycleCount.isPending && recordCycleCount.variables?.id === c.id)
                        }
                        onClick={() => recordCycleCount.mutate({ id: c.id, countedQuantity: Number(countInputs[c.id]) })}
                      >
                        Record
                      </Button>
                    </div>
                  ) : (
                    <span className="text-text-muted">—</span>
                  ),
              },
            ]}
            rows={cycleCountsQuery.data.data}
            isLoading={cycleCountsQuery.isLoading}
            emptyMessage="No cycle counts yet for this warehouse."
            rowKey={(c) => c.id}
          />
        </Card>
      )}
      {recordCycleCount.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not record that count.</p>
      )}
    </div>
  );
}
