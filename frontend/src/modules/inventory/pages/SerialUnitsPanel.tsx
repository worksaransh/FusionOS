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
import { useProductOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  productId: z.string().uuid('Pick a product'),
  serialNumber: z.string().min(1, 'Serial number is required').max(100),
});
type FormValues = z.infer<typeof schema>;

interface SerialUnitDto {
  id: string;
  productId: string;
  serialNumber: string;
  status: string;
  receivedAt: string;
}

// Only the transitions SerialUnit's own Mark* methods support — InStock is
// set at registration only, there is no "revert to InStock" action.
const NEXT_STATUS_ACTIONS: Record<string, { label: string; status: string }[]> = {
  InStock: [
    { label: 'Reserve', status: 'Reserved' },
    { label: 'Sell', status: 'Sold' },
    { label: 'Mark defective', status: 'Defective' },
  ],
  Reserved: [
    { label: 'Sell', status: 'Sold' },
    { label: 'Mark defective', status: 'Defective' },
  ],
  Sold: [
    { label: 'Return', status: 'Returned' },
  ],
  Returned: [
    { label: 'Mark defective', status: 'Defective' },
  ],
  Defective: [],
};

/**
 * Serial-number tracking — a structured record ("what do we know about unit
 * S/N 12345") alongside the existing opaque SerialNumber string already
 * carried by InventoryLedgerEntry/GoodsReceiptLine (Warehouse module), which
 * keeps flowing through its own already-working consumer untouched. Rendered
 * as a sibling panel under ProductsPage, same pattern as BatchesPanel/
 * ReservationsPanel. The "Scan / look up" box is the real "scan a serial and
 * find it" use case (GetSerialUnitBySerialNumberQuery).
 */
export function SerialUnitsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [lookupSerial, setLookupSerial] = useState('');
  const [lookupTrigger, setLookupTrigger] = useState('');

  const productOptions = useProductOptions(companyId);

  const { control, handleSubmit, reset, setError, watch, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { productId: '', serialNumber: '' },
  });

  const watchedProductId = watch('productId');

  const unitsQuery = useQuery({
    queryKey: ['serial-units', companyId, watchedProductId],
    queryFn: () => apiClient.get<PagedResult<SerialUnitDto>>(`/inventory/serial-units?companyId=${companyId}&productId=${watchedProductId}&page=1&pageSize=50`),
    enabled: Boolean(companyId && watchedProductId),
  });

  const lookupQuery = useQuery({
    queryKey: ['serial-unit-lookup', companyId, lookupTrigger],
    queryFn: () => apiClient.get<SerialUnitDto>(`/inventory/serial-units/lookup?companyId=${companyId}&serialNumber=${encodeURIComponent(lookupTrigger)}`),
    enabled: Boolean(companyId && lookupTrigger),
    retry: false,
  });

  const registerUnit = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<SerialUnitDto>('/inventory/serial-units', {
        companyId,
        productId: values.productId,
        serialNumber: values.serialNumber,
      }),
    onSuccess: () => {
      reset({ productId: watchedProductId, serialNumber: '' });
      queryClient.invalidateQueries({ queryKey: ['serial-units', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const updateStatus = useMutation({
    mutationFn: ({ id, newStatus }: { id: string; newStatus: string }) =>
      apiClient.post<SerialUnitDto>(`/inventory/serial-units/${id}/status`, { companyId, newStatus }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['serial-units', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Serial units</h2>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => registerUnit.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
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
            Serial number
            <Controller
              control={control}
              name="serialNumber"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="SN-0001" {...field} />
              )}
            />
            {errors.serialNumber && <span className="text-xs text-danger">{errors.serialNumber.message}</span>}
          </label>
          <div className="flex items-end">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Registering…' : 'Register unit'}</Button>
          </div>
        </form>
        {registerUnit.isError && registerUnit.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{registerUnit.error.problem.title}</p>
        )}
      </Card>

      <Card className="mb-6">
        <h3 className="mb-2 text-sm font-semibold text-text">Scan / look up a serial number</h3>
        <div className="flex items-end gap-3">
          <label className="flex flex-1 flex-col gap-1 text-sm">
            Serial number
            <input
              className="rounded-md border border-border bg-surface px-2 py-1.5"
              placeholder="SN-0001"
              value={lookupSerial}
              onChange={(e) => setLookupSerial(e.target.value)}
            />
          </label>
          <Button type="button" variant="secondary" onClick={() => setLookupTrigger(lookupSerial.trim())} disabled={!lookupSerial.trim()}>
            Look up
          </Button>
        </div>
        {lookupQuery.isFetching && <p className="mt-2 text-sm text-text-muted">Looking up…</p>}
        {lookupQuery.isError && <p role="alert" className="mt-2 text-sm text-danger">No unit found with that serial number.</p>}
        {lookupQuery.data && (
          <p className="mt-2 text-sm text-text">
            <strong>{lookupQuery.data.serialNumber}</strong> — status <strong>{lookupQuery.data.status}</strong>, received{' '}
            {new Date(lookupQuery.data.receivedAt).toLocaleDateString()}
          </p>
        )}
      </Card>

      {!watchedProductId ? (
        <p className="text-sm text-text-muted">Pick a product above to see its serial units.</p>
      ) : (
        <Card>
          <DataTable
            columns={[
              { header: 'Serial number', render: (row: SerialUnitDto) => row.serialNumber },
              { header: 'Status', render: (row: SerialUnitDto) => row.status },
              { header: 'Received', render: (row: SerialUnitDto) => new Date(row.receivedAt).toLocaleDateString() },
              {
                header: 'Actions',
                render: (row: SerialUnitDto) => (
                  <div className="flex flex-wrap items-center gap-2">
                    {(NEXT_STATUS_ACTIONS[row.status] ?? []).map((action) => (
                      <Button
                        key={action.status}
                        type="button"
                        variant="secondary"
                        disabled={updateStatus.isPending}
                        onClick={() => updateStatus.mutate({ id: row.id, newStatus: action.status })}
                      >
                        {action.label}
                      </Button>
                    ))}
                  </div>
                ),
              },
            ]}
            rows={unitsQuery.data?.data}
            isLoading={unitsQuery.isLoading}
            isError={unitsQuery.isError}
            errorMessage="Could not load serial units."
            emptyMessage="No serial units yet for this product — register one above."
            rowKey={(row) => row.id}
          />
        </Card>
      )}
      {updateStatus.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not update that serial unit's status.</p>
      )}
    </div>
  );
}
