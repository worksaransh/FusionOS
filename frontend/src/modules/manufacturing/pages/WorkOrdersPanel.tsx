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
import { useBillOfMaterialsOptions, useWarehouseOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  billOfMaterialsId: z.string().uuid('Pick a bill of materials'),
  warehouseId: z.string().uuid('Pick a warehouse'),
  quantityToProduce: z.string().refine((v) => Number(v) > 0, 'Quantity must be greater than zero'),
});
type FormValues = z.infer<typeof schema>;

interface WorkOrderDto {
  id: string;
  billOfMaterialsId: string;
  productId: string;
  warehouseId: string;
  quantityToProduce: number;
  status: string;
}

/**
 * Work Orders — an order to manufacture a quantity of a product from a bill
 * of materials, Draft → Released → Completed. Rendered as a sibling panel
 * under BillsOfMaterialsPage, same pattern as JournalEntriesPanel under
 * AccountsPage. Completing raises WorkOrderCompleted (consumed by Inventory
 * to post the real stock movements) — this panel only surfaces the action.
 */
export function WorkOrdersPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const bomOptions = useBillOfMaterialsOptions(companyId);
  const warehouseOptions = useWarehouseOptions(companyId);

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { billOfMaterialsId: '', warehouseId: '', quantityToProduce: '1' },
  });

  const workOrdersQuery = useQuery({
    queryKey: ['work-orders', companyId],
    queryFn: () => apiClient.get<PagedResult<WorkOrderDto>>(`/manufacturing/work-orders?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const createWorkOrder = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<WorkOrderDto>('/manufacturing/work-orders', {
        companyId,
        billOfMaterialsId: values.billOfMaterialsId,
        warehouseId: values.warehouseId,
        quantityToProduce: Number(values.quantityToProduce),
      }),
    onSuccess: () => {
      reset({ billOfMaterialsId: '', warehouseId: '', quantityToProduce: '1' });
      queryClient.invalidateQueries({ queryKey: ['work-orders', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const releaseWorkOrder = useMutation({
    mutationFn: (id: string) => apiClient.post<WorkOrderDto>(`/manufacturing/work-orders/${id}/release`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['work-orders', companyId] }),
  });

  const completeWorkOrder = useMutation({
    mutationFn: (id: string) => apiClient.post<WorkOrderDto>(`/manufacturing/work-orders/${id}/complete`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['work-orders', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Work Orders</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createWorkOrder.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <label className="flex flex-col gap-1 text-sm">
            Bill of materials
            <Controller
              control={control}
              name="billOfMaterialsId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={bomOptions.options}
                  isLoading={bomOptions.isLoading}
                  onSearchChange={bomOptions.onSearchChange}
                  placeholder="Search bills of materials…"
                />
              )}
            />
            {errors.billOfMaterialsId && <span className="text-xs text-danger">{errors.billOfMaterialsId.message}</span>}
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
            Quantity to produce
            <Controller
              control={control}
              name="quantityToProduce"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.quantityToProduce && <span className="text-xs text-danger">{errors.quantityToProduce.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create work order'}</Button>
          </div>
        </form>
        {createWorkOrder.isError && createWorkOrder.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createWorkOrder.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Status', render: (row: WorkOrderDto) => row.status },
            { header: 'Quantity to produce', render: (row: WorkOrderDto) => row.quantityToProduce },
            {
              header: 'Actions',
              render: (row: WorkOrderDto) => (
                <div className="flex items-center gap-2">
                  {row.status === 'Draft' && (
                    <Button type="button" variant="secondary" disabled={releaseWorkOrder.isPending} onClick={() => releaseWorkOrder.mutate(row.id)}>
                      Release
                    </Button>
                  )}
                  {row.status === 'Released' && (
                    <Button type="button" disabled={completeWorkOrder.isPending} onClick={() => completeWorkOrder.mutate(row.id)}>
                      Complete
                    </Button>
                  )}
                </div>
              ),
            },
          ]}
          rows={workOrdersQuery.data?.data}
          isLoading={workOrdersQuery.isLoading}
          isError={workOrdersQuery.isError}
          errorMessage="Could not load work orders."
          emptyMessage="No work orders yet — create the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {(releaseWorkOrder.isError || completeWorkOrder.isError) && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not update that work order.</p>
      )}
    </div>
  );
}
