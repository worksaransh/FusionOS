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
import { useProductOptions, usePurchaseOrderOptions, useWarehouseOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  purchaseOrderId: z.string().uuid('Pick a purchase order'),
  productId: z.string().uuid('Pick a product'),
  warehouseId: z.string().uuid('Pick a warehouse'),
  quantity: z.string().refine((v) => Number(v) > 0, 'Quantity must be greater than zero'),
  reason: z.string().min(1, 'Reason is required').max(500),
});
type FormValues = z.infer<typeof schema>;

interface VendorReturnDto {
  id: string;
  purchaseOrderId: string;
  productId: string;
  warehouseId: string;
  quantity: number;
  reason: string;
  status: string;
  returnDate: string;
}

/**
 * Vendor Returns — sends a Product back to the supplier against a Purchase
 * Order, closing the "Vendor returns" gap in Phase 1's Procurement scope
 * (05_MODULE_ROADMAP.md). Rendered as a sibling panel under SuppliersPage,
 * same pattern as PurchaseOrdersPanel/RfqsPanel/SupplierContractsPanel.
 * Completing a return debits Inventory's stock ledger asynchronously (a
 * cross-module event, since Procurement cannot call Inventory directly) —
 * the UI itself does not wait on that, it only reflects this return's own
 * Pending -> Completed/Cancelled status.
 */
export function VendorReturnsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const purchaseOrderOptions = usePurchaseOrderOptions(companyId);
  const productOptions = useProductOptions(companyId);
  const warehouseOptions = useWarehouseOptions(companyId);

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { purchaseOrderId: '', productId: '', warehouseId: '', quantity: '1', reason: '' },
  });

  const vendorReturnsQuery = useQuery({
    queryKey: ['vendor-returns', companyId],
    queryFn: () => apiClient.get<PagedResult<VendorReturnDto>>(`/procurement/vendor-returns?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const createVendorReturn = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<VendorReturnDto>('/procurement/vendor-returns', {
        companyId,
        purchaseOrderId: values.purchaseOrderId,
        productId: values.productId,
        warehouseId: values.warehouseId,
        quantity: Number(values.quantity),
        reason: values.reason,
      }),
    onSuccess: () => {
      reset({ purchaseOrderId: '', productId: '', warehouseId: '', quantity: '1', reason: '' });
      queryClient.invalidateQueries({ queryKey: ['vendor-returns', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const completeVendorReturn = useMutation({
    mutationFn: (id: string) => apiClient.post<VendorReturnDto>(`/procurement/vendor-returns/${id}/complete`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['vendor-returns', companyId] }),
  });

  const cancelVendorReturn = useMutation({
    mutationFn: (id: string) => apiClient.post<VendorReturnDto>(`/procurement/vendor-returns/${id}/cancel`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['vendor-returns', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Vendor Returns</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createVendorReturn.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <label className="flex flex-col gap-1 text-sm">
            Purchase order
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
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            Reason
            <Controller
              control={control}
              name="reason"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Damaged in transit" {...field} />
              )}
            />
            {errors.reason && <span className="text-xs text-danger">{errors.reason.message}</span>}
          </label>

          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create vendor return'}</Button>
          </div>
        </form>
        {createVendorReturn.isError && createVendorReturn.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createVendorReturn.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Quantity', render: (row: VendorReturnDto) => row.quantity.toLocaleString() },
            { header: 'Reason', render: (row: VendorReturnDto) => row.reason },
            { header: 'Status', render: (row: VendorReturnDto) => row.status },
            {
              header: 'Actions',
              render: (row: VendorReturnDto) =>
                row.status === 'Pending' ? (
                  <div className="flex items-center gap-2">
                    <Button type="button" variant="secondary" disabled={completeVendorReturn.isPending} onClick={() => completeVendorReturn.mutate(row.id)}>
                      Complete
                    </Button>
                    <Button type="button" variant="danger" disabled={cancelVendorReturn.isPending} onClick={() => cancelVendorReturn.mutate(row.id)}>
                      Cancel
                    </Button>
                  </div>
                ) : null,
            },
          ]}
          rows={vendorReturnsQuery.data?.data}
          isLoading={vendorReturnsQuery.isLoading}
          isError={vendorReturnsQuery.isError}
          errorMessage="Could not load vendor returns."
          emptyMessage="No vendor returns yet — create one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {(completeVendorReturn.isError || cancelVendorReturn.isError) && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not update that vendor return.</p>
      )}
    </div>
  );
}
