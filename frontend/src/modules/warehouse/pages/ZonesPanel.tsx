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
import { useWarehouseOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  warehouseId: z.string().uuid('Pick a warehouse'),
  name: z.string().min(1, 'Name is required').max(150),
  code: z.string().min(1, 'Code is required').max(20),
});
type FormValues = z.infer<typeof schema>;

// Update command deliberately excludes Code and WarehouseId — only Name is
// editable (see UpdateZoneCommand.cs / ZonesController.Update).
const editSchema = z.object({
  name: z.string().min(1, 'Name is required').max(150),
});
type EditFormValues = z.infer<typeof editSchema>;

interface ZoneDto {
  id: string;
  warehouseId: string;
  name: string;
  code: string;
  isActive: boolean;
  createdAt: string;
}

/** Zones — next slice after Warehouse (05_MODULE_ROADMAP.md Phase 1). Pick a Warehouse from the searchable dropdown. */
export function ZonesPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [lookupWarehouseId, setLookupWarehouseId] = useState('');
  const [editingZoneId, setEditingZoneId] = useState<string | null>(null);

  const warehouseOptions = useWarehouseOptions(companyId);

  const { control, handleSubmit, watch, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { warehouseId: '', name: '', code: '' },
  });
  const watchedWarehouseId = watch('warehouseId');

  const createZone = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post(`/warehouse/warehouses/${values.warehouseId}/zones`, { companyId, name: values.name, code: values.code }),
    onSuccess: (_data, variables) => {
      reset({ warehouseId: variables.warehouseId, name: '', code: '' });
      setLookupWarehouseId(variables.warehouseId);
      queryClient.invalidateQueries({ queryKey: ['zones', companyId, variables.warehouseId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const zonesQuery = useQuery({
    queryKey: ['zones', companyId, lookupWarehouseId || watchedWarehouseId],
    queryFn: () =>
      apiClient.get<PagedResult<ZoneDto>>(
        `/warehouse/warehouses/${lookupWarehouseId || watchedWarehouseId}/zones?companyId=${companyId}&page=1&pageSize=25`,
      ),
    enabled: Boolean(companyId && (lookupWarehouseId || watchedWarehouseId)),
  });

  // Soft-deactivate only — hits POST /{warehouseId}/zones/{id}/deactivate,
  // never a DELETE (apiClient has no `delete` method by design).
  const activeWarehouseId = lookupWarehouseId || watchedWarehouseId;
  const deactivateZone = useMutation({
    mutationFn: (id: string) => apiClient.post(`/warehouse/warehouses/${activeWarehouseId}/zones/${id}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['zones', companyId, activeWarehouseId] }),
  });

  if (!companyId) return null;

  const editingZone = zonesQuery.data?.data.find((z) => z.id === editingZoneId) ?? null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Zones</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createZone.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
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
            Zone name
            <Controller
              control={control}
              name="name"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Zone code
            <Controller
              control={control}
              name="code"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Z-01" {...field} />
              )}
            />
            {errors.code && <span className="text-xs text-danger">{errors.code.message}</span>}
          </label>
          <div className="col-span-2">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create zone'}</Button>
          </div>
        </form>
      </Card>

      {zonesQuery.data && (
        <Card>
          <DataTable
            columns={[
              { header: 'Code', render: (zone: ZoneDto) => zone.code },
              { header: 'Name', render: (zone: ZoneDto) => zone.name },
              { header: 'Status', render: (zone: ZoneDto) => (zone.isActive ? 'Active' : 'Inactive') },
              { header: 'Created', render: (zone: ZoneDto) => new Date(zone.createdAt).toLocaleDateString() },
              {
                header: 'Actions',
                render: (zone: ZoneDto) => (
                  <div className="flex items-center gap-2">
                    <Button type="button" variant="secondary" onClick={() => setEditingZoneId(zone.id)}>
                      Edit
                    </Button>
                    <Button
                      type="button"
                      variant="danger"
                      disabled={!zone.isActive || (deactivateZone.isPending && deactivateZone.variables === zone.id)}
                      onClick={() => deactivateZone.mutate(zone.id)}
                    >
                      {zone.isActive ? 'Deactivate' : 'Deactivated'}
                    </Button>
                  </div>
                ),
              },
            ]}
            rows={zonesQuery.data.data}
            isLoading={zonesQuery.isLoading}
            emptyMessage="No zones yet for this warehouse."
            rowKey={(zone) => zone.id}
          />
        </Card>
      )}
      {deactivateZone.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that zone.</p>
      )}

      {editingZone && (
        <ZoneEditPanel
          companyId={companyId}
          warehouseId={activeWarehouseId}
          zone={editingZone}
          onClose={() => setEditingZoneId(null)}
        />
      )}
    </div>
  );
}

interface ZoneEditPanelProps {
  companyId: string;
  warehouseId: string;
  zone: ZoneDto;
  onClose: () => void;
}

function ZoneEditPanel({ companyId, warehouseId, zone, onClose }: ZoneEditPanelProps) {
  const queryClient = useQueryClient();

  const { register, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    values: { name: zone.name },
  });

  const updateZone = useMutation({
    mutationFn: (values: EditFormValues) =>
      apiClient.put<ZoneDto>(`/warehouse/warehouses/${warehouseId}/zones/${zone.id}`, { companyId, ...values }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['zones', companyId, warehouseId] });
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

  return (
    <Card className="mt-6">
      <div className="mb-3 flex items-center justify-between">
        <h2 className="text-lg font-semibold text-text">Edit zone — {zone.code}</h2>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form onSubmit={handleSubmit((values) => updateZone.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <label className="flex flex-col gap-1 text-sm">
          Name
          <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('name')} />
          {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
        </label>
        <div className="col-span-2 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save changes'}</Button>
          {updateZone.isError && (
            <span role="alert" className="text-sm text-danger">Could not save that zone.</span>
          )}
        </div>
      </form>
    </Card>
  );
}
