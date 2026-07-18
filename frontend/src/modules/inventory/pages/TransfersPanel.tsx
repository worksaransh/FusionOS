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
  sourceWarehouseId: z.string().uuid('Pick a source warehouse'),
  destinationWarehouseId: z.string().uuid('Pick a destination warehouse'),
  quantity: z.string().refine((v) => Number(v) > 0, 'Quantity must be greater than zero'),
});
type FormValues = z.infer<typeof schema>;

interface TransferDto {
  id: string;
  productId: string;
  sourceWarehouseId: string;
  destinationWarehouseId: string;
  quantity: number;
  status: string;
  transferDate: string;
}

/**
 * Transfers — moves a Product's stock from one Warehouse to another, closing
 * the "Transfers" gap in Phase 1's Inventory scope (05_MODULE_ROADMAP.md).
 * Rendered as a sibling panel under ProductsPage, same pattern as
 * ReservationsPanel. Completing a transfer posts the actual stock movement
 * (checked against source warehouse stock server-side); cancelling never
 * moves stock at all.
 */
export function TransfersPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const productOptions = useProductOptions(companyId);
  const sourceWarehouseOptions = useWarehouseOptions(companyId);
  const destinationWarehouseOptions = useWarehouseOptions(companyId);

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { productId: '', sourceWarehouseId: '', destinationWarehouseId: '', quantity: '1' },
  });

  const transfersQuery = useQuery({
    queryKey: ['transfers', companyId],
    queryFn: () => apiClient.get<PagedResult<TransferDto>>(`/inventory/transfers?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const createTransfer = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<TransferDto>('/inventory/transfers', {
        companyId,
        productId: values.productId,
        sourceWarehouseId: values.sourceWarehouseId,
        destinationWarehouseId: values.destinationWarehouseId,
        quantity: Number(values.quantity),
      }),
    onSuccess: () => {
      reset({ productId: '', sourceWarehouseId: '', destinationWarehouseId: '', quantity: '1' });
      queryClient.invalidateQueries({ queryKey: ['transfers', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const completeTransfer = useMutation({
    mutationFn: (id: string) => apiClient.post<TransferDto>(`/inventory/transfers/${id}/complete`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['transfers', companyId] }),
  });

  const cancelTransfer = useMutation({
    mutationFn: (id: string) => apiClient.post<TransferDto>(`/inventory/transfers/${id}/cancel`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['transfers', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Transfers</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createTransfer.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-4">
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
            From warehouse
            <Controller
              control={control}
              name="sourceWarehouseId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={sourceWarehouseOptions.options}
                  isLoading={sourceWarehouseOptions.isLoading}
                  onSearchChange={sourceWarehouseOptions.onSearchChange}
                  placeholder="Search warehouses…"
                />
              )}
            />
            {errors.sourceWarehouseId && <span className="text-xs text-danger">{errors.sourceWarehouseId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            To warehouse
            <Controller
              control={control}
              name="destinationWarehouseId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={destinationWarehouseOptions.options}
                  isLoading={destinationWarehouseOptions.isLoading}
                  onSearchChange={destinationWarehouseOptions.onSearchChange}
                  placeholder="Search warehouses…"
                />
              )}
            />
            {errors.destinationWarehouseId && <span className="text-xs text-danger">{errors.destinationWarehouseId.message}</span>}
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

          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create transfer'}</Button>
          </div>
        </form>
        {createTransfer.isError && createTransfer.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createTransfer.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Quantity', render: (row: TransferDto) => row.quantity.toLocaleString() },
            { header: 'From', render: (row: TransferDto) => row.sourceWarehouseId.slice(0, 8) + '…' },
            { header: 'To', render: (row: TransferDto) => row.destinationWarehouseId.slice(0, 8) + '…' },
            { header: 'Status', render: (row: TransferDto) => row.status },
            {
              header: 'Actions',
              render: (row: TransferDto) =>
                row.status === 'Pending' ? (
                  <div className="flex items-center gap-2">
                    <Button type="button" variant="secondary" disabled={completeTransfer.isPending} onClick={() => completeTransfer.mutate(row.id)}>
                      Complete
                    </Button>
                    <Button type="button" variant="danger" disabled={cancelTransfer.isPending} onClick={() => cancelTransfer.mutate(row.id)}>
                      Cancel
                    </Button>
                  </div>
                ) : null,
            },
          ]}
          rows={transfersQuery.data?.data}
          isLoading={transfersQuery.isLoading}
          isError={transfersQuery.isError}
          errorMessage="Could not load transfers."
          emptyMessage="No transfers yet — create one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {(completeTransfer.isError || cancelTransfer.isError) && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not update that transfer.</p>
      )}
    </div>
  );
}
