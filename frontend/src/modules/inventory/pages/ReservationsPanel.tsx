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
  quantity: z.string().refine((v) => Number(v) > 0, 'Quantity must be greater than zero'),
  referenceType: z.string().min(1, 'Reference type is required').max(50),
  referenceId: z.string().uuid('Must be a valid id'),
});
type FormValues = z.infer<typeof schema>;

interface ReservationDto {
  id: string;
  productId: string;
  warehouseId: string;
  quantity: number;
  referenceType: string;
  referenceId: string;
  status: string;
}

interface AvailableToPromiseDto {
  stockOnHand: number;
  reserved: number;
  available: number;
}

/**
 * Reservations — a soft hold on stock at a Warehouse against a reference
 * document (e.g. a Sales Order line), closing the "Reservations" gap in
 * Phase 1's Inventory scope (05_MODULE_ROADMAP.md). Rendered as a sibling
 * panel under ProductsPage, same pattern as StockLedgerPanel/
 * InventoryValuationPanel. ReferenceType/ReferenceId are plain inputs, not an
 * EntityCombobox — same reasoning as AI's Recommendation.ReferenceId: a
 * deliberately opaque, never-validated cross-module reference. The
 * available-to-promise lookup below the form composes stock-on-hand and
 * active reservations for a picked Product+Warehouse pair before you commit
 * a new hold.
 */
export function ReservationsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const productOptions = useProductOptions(companyId);
  const warehouseOptions = useWarehouseOptions(companyId);

  const { control, handleSubmit, reset, setError, watch, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { productId: '', warehouseId: '', quantity: '1', referenceType: 'SalesOrderLine', referenceId: '' },
  });

  const watchedProductId = watch('productId');
  const watchedWarehouseId = watch('warehouseId');

  const availableToPromiseQuery = useQuery({
    queryKey: ['available-to-promise', companyId, watchedProductId, watchedWarehouseId],
    queryFn: () =>
      apiClient.get<AvailableToPromiseDto>(
        `/inventory/reservations/available-to-promise?companyId=${companyId}&productId=${watchedProductId}&warehouseId=${watchedWarehouseId}`,
      ),
    enabled: Boolean(companyId && watchedProductId && watchedWarehouseId),
  });

  const reservationsQuery = useQuery({
    queryKey: ['reservations', companyId],
    queryFn: () => apiClient.get<PagedResult<ReservationDto>>(`/inventory/reservations?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const createReservation = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<ReservationDto>('/inventory/reservations', {
        companyId,
        productId: values.productId,
        warehouseId: values.warehouseId,
        quantity: Number(values.quantity),
        referenceType: values.referenceType,
        referenceId: values.referenceId,
      }),
    onSuccess: () => {
      reset({ productId: '', warehouseId: '', quantity: '1', referenceType: 'SalesOrderLine', referenceId: '' });
      queryClient.invalidateQueries({ queryKey: ['reservations', companyId] });
      queryClient.invalidateQueries({ queryKey: ['available-to-promise', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const releaseReservation = useMutation({
    mutationFn: (id: string) => apiClient.post<ReservationDto>(`/inventory/reservations/${id}/release`, { companyId }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reservations', companyId] });
      queryClient.invalidateQueries({ queryKey: ['available-to-promise', companyId] });
    },
  });

  const fulfillReservation = useMutation({
    mutationFn: (id: string) => apiClient.post<ReservationDto>(`/inventory/reservations/${id}/fulfill`, { companyId }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reservations', companyId] });
      queryClient.invalidateQueries({ queryKey: ['available-to-promise', companyId] });
    },
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Reservations</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createReservation.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
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
            Quantity
            <Controller
              control={control}
              name="quantity"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.quantity && <span className="text-xs text-danger">{errors.quantity.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Reference type
            <Controller
              control={control}
              name="referenceType"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="SalesOrderLine" {...field} />
              )}
            />
            {errors.referenceType && <span className="text-xs text-danger">{errors.referenceType.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            Reference id
            <Controller
              control={control}
              name="referenceId"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="00000000-0000-0000-0000-000000000000" {...field} />
              )}
            />
            {errors.referenceId && <span className="text-xs text-danger">{errors.referenceId.message}</span>}
          </label>

          {watchedProductId && watchedWarehouseId && availableToPromiseQuery.data && (
            <p className="col-span-full text-xs text-text-muted">
              Available to promise for this product/warehouse: <strong>{availableToPromiseQuery.data.available.toLocaleString()}</strong>{' '}
              ({availableToPromiseQuery.data.stockOnHand.toLocaleString()} on hand − {availableToPromiseQuery.data.reserved.toLocaleString()} reserved)
            </p>
          )}

          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Reserving…' : 'Reserve stock'}</Button>
          </div>
        </form>
        {createReservation.isError && createReservation.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createReservation.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Quantity', render: (row: ReservationDto) => row.quantity.toLocaleString() },
            { header: 'Reference', render: (row: ReservationDto) => `${row.referenceType} ${row.referenceId.slice(0, 8)}…` },
            { header: 'Status', render: (row: ReservationDto) => row.status },
            {
              header: 'Actions',
              render: (row: ReservationDto) =>
                row.status === 'Active' ? (
                  <div className="flex items-center gap-2">
                    <Button type="button" variant="secondary" disabled={fulfillReservation.isPending} onClick={() => fulfillReservation.mutate(row.id)}>
                      Fulfill
                    </Button>
                    <Button type="button" variant="danger" disabled={releaseReservation.isPending} onClick={() => releaseReservation.mutate(row.id)}>
                      Release
                    </Button>
                  </div>
                ) : null,
            },
          ]}
          rows={reservationsQuery.data?.data}
          isLoading={reservationsQuery.isLoading}
          isError={reservationsQuery.isError}
          errorMessage="Could not load reservations."
          emptyMessage="No reservations yet — reserve stock above."
          rowKey={(row) => row.id}
        />
      </Card>
      {(releaseReservation.isError || fulfillReservation.isError) && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not update that reservation.</p>
      )}
    </div>
  );
}
