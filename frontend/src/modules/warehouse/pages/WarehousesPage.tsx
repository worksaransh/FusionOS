import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { CrudListPage } from '../../../shared/ui/CrudListPage';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import type { PagedResult } from '../../../shared/api/types';
import { ZonesPanel } from './ZonesPanel';
import { BinsPanel } from './BinsPanel';
import { GoodsReceiptsPanel } from './GoodsReceiptsPanel';
import { CycleCountsPanel } from './CycleCountsPanel';
import { PickListsPanel } from './PickListsPanel';

const schema = z.object({
  name: z.string().min(1, 'Name is required').max(150),
  code: z.string().min(1, 'Code is required').max(20),
  address: z.string().optional(),
});
type FormValues = z.infer<typeof schema>;

// Update command deliberately excludes Code — it's the immutable business key
// (see UpdateWarehouseCommand.cs / WarehousesController.Update, which only ever
// touches Name/Address/BranchId).
const editSchema = z.object({
  name: z.string().min(1, 'Name is required').max(150),
  address: z.string().optional(),
});
type EditFormValues = z.infer<typeof editSchema>;

interface WarehouseDto {
  id: string;
  name: string;
  code: string;
  address: string | null;
  isActive: boolean;
  createdAt: string;
}

/** Phase 1 slice — see backend/src/Modules/Warehouse for the full CQRS handler. */
export function WarehousesPage() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [editingWarehouseId, setEditingWarehouseId] = useState<string | null>(null);

  const warehousesQuery = useQuery({
    queryKey: ['warehouses', companyId],
    queryFn: () => apiClient.get<PagedResult<WarehouseDto>>(`/warehouse/warehouses?companyId=${companyId}&page=1&pageSize=25`),
    enabled: Boolean(companyId),
  });

  const { register, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
  });

  const createWarehouse = useMutation({
    mutationFn: (values: FormValues) => apiClient.post<WarehouseDto>('/warehouse/warehouses', { companyId, branchId: null, ...values }),
    onSuccess: () => {
      reset();
      queryClient.invalidateQueries({ queryKey: ['warehouses', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  // Soft-deactivate only — hits POST /{id}/deactivate, never a DELETE
  // (apiClient has no `delete` method by design; see shared/api/client.ts).
  const deactivateWarehouse = useMutation({
    mutationFn: (id: string) => apiClient.post(`/warehouse/warehouses/${id}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['warehouses', companyId] }),
  });

  if (!companyId) {
    return <p className="text-text-muted">Set an active Company ID in the header above to manage warehouses.</p>;
  }

  return (
    <div>
      <CrudListPage<WarehouseDto>
      title="Warehouses"
      description="Physical locations — Warehouse Management, Phase 1"
      rows={warehousesQuery.data?.data}
      isLoading={warehousesQuery.isLoading}
      isError={warehousesQuery.isError}
      errorMessage="Could not load warehouses."
      emptyMessage="No warehouses yet — create the first one above."
      rowKey={(row) => row.id}
      columns={[
        { header: 'Code', render: (row) => row.code },
        { header: 'Name', render: (row) => row.name },
        { header: 'Address', render: (row) => row.address ?? '—' },
        { header: 'Status', render: (row) => (row.isActive ? 'Active' : 'Inactive') },
        { header: 'Created', render: (row) => new Date(row.createdAt).toLocaleDateString() },
        {
          header: 'Actions',
          render: (row) => (
            <div className="flex items-center gap-2">
              <Button type="button" variant="secondary" onClick={() => setEditingWarehouseId(row.id)}>
                Edit
              </Button>
              <Button
                type="button"
                variant="danger"
                disabled={!row.isActive || (deactivateWarehouse.isPending && deactivateWarehouse.variables === row.id)}
                onClick={() => deactivateWarehouse.mutate(row.id)}
              >
                {row.isActive ? 'Deactivate' : 'Deactivated'}
              </Button>
            </div>
          ),
        },
      ]}
      form={
        <form onSubmit={handleSubmit((values) => createWarehouse.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Name
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('name')} />
            {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Code
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="WH-01" {...register('code')} />
            {errors.code && <span className="text-xs text-danger">{errors.code.message}</span>}
          </label>
          <label className="col-span-2 flex flex-col gap-1 text-sm">
            Address (optional)
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('address')} />
          </label>
          <div className="col-span-2">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create warehouse'}</Button>
          </div>
        </form>
      }
      />
      {deactivateWarehouse.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that warehouse.</p>
      )}

      {editingWarehouseId && (
        <WarehouseEditPanel
          companyId={companyId}
          warehouse={warehousesQuery.data?.data.find((w) => w.id === editingWarehouseId) ?? null}
          onClose={() => setEditingWarehouseId(null)}
        />
      )}

      <ZonesPanel />
      <BinsPanel />
      <GoodsReceiptsPanel />
      <CycleCountsPanel />
      <PickListsPanel />
    </div>
  );
}

interface WarehouseEditPanelProps {
  companyId: string;
  warehouse: WarehouseDto | null;
  onClose: () => void;
}

function WarehouseEditPanel({ companyId, warehouse, onClose }: WarehouseEditPanelProps) {
  const queryClient = useQueryClient();

  const { register, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    values: warehouse ? { name: warehouse.name, address: warehouse.address ?? '' } : undefined,
  });

  const updateWarehouse = useMutation({
    mutationFn: (values: EditFormValues) =>
      apiClient.put<WarehouseDto>(`/warehouse/warehouses/${warehouse!.id}`, { companyId, branchId: null, ...values }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['warehouses', companyId] });
      onClose();
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof EditFormValues, { message: messages[0] });
        }
      }
    },
  });

  if (!warehouse) return null;

  return (
    <Card className="mt-8">
      <div className="mb-3 flex items-center justify-between">
        <h2 className="text-lg font-semibold text-text">Edit warehouse — {warehouse.code}</h2>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form onSubmit={handleSubmit((values) => updateWarehouse.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <label className="flex flex-col gap-1 text-sm">
          Name
          <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('name')} />
          {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
        </label>
        <label className="col-span-2 flex flex-col gap-1 text-sm">
          Address (optional)
          <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('address')} />
        </label>
        <div className="col-span-2 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save changes'}</Button>
          {updateWarehouse.isError && (
            <span role="alert" className="text-sm text-danger">Could not save that warehouse.</span>
          )}
        </div>
      </form>
    </Card>
  );
}
