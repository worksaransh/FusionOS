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
  batchNumber: z.string().min(1, 'Batch number is required').max(50),
  quantityReceived: z.string().refine((v) => Number(v) > 0, 'Quantity must be greater than zero'),
  expiryDate: z.string().optional(),
});
type FormValues = z.infer<typeof schema>;

interface BatchDto {
  id: string;
  productId: string;
  batchNumber: string;
  expiryDate: string | null;
  quantityReceived: number;
  quantityRemaining: number;
  createdAt: string;
}

/**
 * Batch/lot tracking — a structured record ("what do we know about batch
 * B-001") alongside the existing opaque BatchNumber string already carried by
 * InventoryLedgerEntry/GoodsReceiptLine (Warehouse module), which keeps
 * flowing through its own already-working consumer untouched. Rendered as a
 * sibling panel under ProductsPage, same pattern as ReservationsPanel/
 * TransfersPanel. Consuming a batch here only tracks how much of it remains —
 * it does not itself post a ledger entry (see Batch.Consume's doc comment on
 * the backend).
 */
export function BatchesPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [consumeQuantity, setConsumeQuantity] = useState<Record<string, string>>({});
  const [expiryEdits, setExpiryEdits] = useState<Record<string, string>>({});

  const productOptions = useProductOptions(companyId);

  const { control, handleSubmit, reset, setError, watch, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { productId: '', batchNumber: '', quantityReceived: '1', expiryDate: '' },
  });

  const watchedProductId = watch('productId');

  const batchesQuery = useQuery({
    queryKey: ['batches', companyId, watchedProductId],
    queryFn: () => apiClient.get<PagedResult<BatchDto>>(`/inventory/batches?companyId=${companyId}&productId=${watchedProductId}&page=1&pageSize=50`),
    enabled: Boolean(companyId && watchedProductId),
  });

  const createBatch = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<BatchDto>('/inventory/batches', {
        companyId,
        productId: values.productId,
        batchNumber: values.batchNumber,
        quantityReceived: Number(values.quantityReceived),
        expiryDate: values.expiryDate ? new Date(values.expiryDate).toISOString() : null,
      }),
    onSuccess: () => {
      reset({ productId: watchedProductId, batchNumber: '', quantityReceived: '1', expiryDate: '' });
      queryClient.invalidateQueries({ queryKey: ['batches', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const consumeBatch = useMutation({
    mutationFn: ({ id, quantity }: { id: string; quantity: number }) =>
      apiClient.post<BatchDto>(`/inventory/batches/${id}/consume`, { companyId, quantity }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['batches', companyId] }),
  });

  const adjustExpiry = useMutation({
    mutationFn: ({ id, newExpiry }: { id: string; newExpiry: string | null }) =>
      apiClient.post<BatchDto>(`/inventory/batches/${id}/expiry`, { companyId, newExpiry }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['batches', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Batches</h2>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createBatch.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-4">
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
            Batch number
            <Controller
              control={control}
              name="batchNumber"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="B-001" {...field} />
              )}
            />
            {errors.batchNumber && <span className="text-xs text-danger">{errors.batchNumber.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Quantity received
            <Controller
              control={control}
              name="quantityReceived"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.quantityReceived && <span className="text-xs text-danger">{errors.quantityReceived.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Expiry date (optional)
            <Controller
              control={control}
              name="expiryDate"
              render={({ field }) => (
                <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
          </label>

          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create batch'}</Button>
          </div>
        </form>
        {createBatch.isError && createBatch.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createBatch.error.problem.title}</p>
        )}
      </Card>

      {!watchedProductId ? (
        <p className="text-sm text-text-muted">Pick a product above to see its batches.</p>
      ) : (
        <Card>
          <DataTable
            columns={[
              { header: 'Batch number', render: (row: BatchDto) => row.batchNumber },
              { header: 'Expiry', render: (row: BatchDto) => (row.expiryDate ? new Date(row.expiryDate).toLocaleDateString() : '—') },
              { header: 'Received', render: (row: BatchDto) => row.quantityReceived.toLocaleString() },
              { header: 'Remaining', render: (row: BatchDto) => row.quantityRemaining.toLocaleString() },
              {
                header: 'Consume',
                render: (row: BatchDto) => (
                  <div className="flex items-center gap-2">
                    <input
                      className="w-20 rounded-md border border-border bg-surface px-2 py-1 text-sm"
                      placeholder="Qty"
                      value={consumeQuantity[row.id] ?? ''}
                      onChange={(e) => setConsumeQuantity((prev) => ({ ...prev, [row.id]: e.target.value }))}
                    />
                    <Button
                      type="button"
                      variant="secondary"
                      disabled={consumeBatch.isPending || !Number(consumeQuantity[row.id])}
                      onClick={() => consumeBatch.mutate({ id: row.id, quantity: Number(consumeQuantity[row.id]) })}
                    >
                      Consume
                    </Button>
                  </div>
                ),
              },
              {
                header: 'Adjust expiry',
                render: (row: BatchDto) => (
                  <div className="flex items-center gap-2">
                    <input
                      type="date"
                      className="rounded-md border border-border bg-surface px-2 py-1 text-sm"
                      value={expiryEdits[row.id] ?? ''}
                      onChange={(e) => setExpiryEdits((prev) => ({ ...prev, [row.id]: e.target.value }))}
                    />
                    <Button
                      type="button"
                      variant="secondary"
                      disabled={adjustExpiry.isPending}
                      onClick={() =>
                        adjustExpiry.mutate({
                          id: row.id,
                          newExpiry: expiryEdits[row.id] ? new Date(expiryEdits[row.id]).toISOString() : null,
                        })
                      }
                    >
                      Save
                    </Button>
                  </div>
                ),
              },
            ]}
            rows={batchesQuery.data?.data}
            isLoading={batchesQuery.isLoading}
            isError={batchesQuery.isError}
            errorMessage="Could not load batches."
            emptyMessage="No batches yet for this product — create one above."
            rowKey={(row) => row.id}
          />
        </Card>
      )}
      {(consumeBatch.isError || adjustExpiry.isError) && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not update that batch.</p>
      )}
    </div>
  );
}
