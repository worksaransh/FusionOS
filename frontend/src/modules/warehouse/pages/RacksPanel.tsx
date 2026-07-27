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
import { useWarehouseOptions, useZoneOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  warehouseId: z.string().uuid('Pick a warehouse'),
  zoneId: z.string().uuid('Pick a zone'),
  name: z.string().min(1, 'Name is required').max(150),
  code: z.string().min(1, 'Code is required').max(20),
});
type FormValues = z.infer<typeof schema>;

// Update command deliberately excludes Code and ZoneId — only Name is
// editable (see UpdateRackCommand.cs / RacksController.Update), same
// immutability rule as Zone/Bin's own edit forms.
const editSchema = z.object({
  name: z.string().min(1, 'Name is required').max(150),
});
type EditFormValues = z.infer<typeof editSchema>;

interface RackDto {
  id: string;
  zoneId: string;
  name: string;
  code: string;
  isActive: boolean;
  createdAt: string;
}

/** Racks — an optional level nested under Zone (one level deeper than Zone nests under Warehouse), giving a Bin a more precise location via Rack -> Shelf. Pick a Warehouse, then a Zone, from searchable dropdowns — same drill-down pattern as BinsPanel. */
export function RacksPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [lookupWarehouseId, setLookupWarehouseId] = useState('');
  const [lookupZoneId, setLookupZoneId] = useState('');
  const [editingRackId, setEditingRackId] = useState<string | null>(null);

  const warehouseOptions = useWarehouseOptions(companyId);

  const { control, handleSubmit, watch, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { warehouseId: '', zoneId: '', name: '', code: '' },
  });
  const watchedWarehouseId = watch('warehouseId');
  const watchedZoneId = watch('zoneId');
  const effectiveWarehouseId = lookupWarehouseId || watchedWarehouseId;
  const effectiveZoneId = lookupZoneId || watchedZoneId;
  const zoneOptions = useZoneOptions(companyId, effectiveWarehouseId || undefined);

  const createRack = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post(`/warehouse/warehouses/${values.warehouseId}/zones/${values.zoneId}/racks`, { companyId, name: values.name, code: values.code }),
    onSuccess: (_data, variables) => {
      reset({ warehouseId: variables.warehouseId, zoneId: variables.zoneId, name: '', code: '' });
      setLookupWarehouseId(variables.warehouseId);
      setLookupZoneId(variables.zoneId);
      queryClient.invalidateQueries({ queryKey: ['racks', companyId, variables.warehouseId, variables.zoneId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const racksQuery = useQuery({
    queryKey: ['racks', companyId, effectiveWarehouseId, effectiveZoneId],
    queryFn: () =>
      apiClient.get<PagedResult<RackDto>>(
        `/warehouse/warehouses/${effectiveWarehouseId}/zones/${effectiveZoneId}/racks?companyId=${companyId}&page=1&pageSize=25`,
      ),
    enabled: Boolean(companyId && effectiveWarehouseId && effectiveZoneId),
  });

  // Soft-deactivate only — hits POST /{warehouseId}/zones/{zoneId}/racks/{id}/deactivate, never a DELETE.
  const deactivateRack = useMutation({
    mutationFn: (id: string) =>
      apiClient.post(`/warehouse/warehouses/${effectiveWarehouseId}/zones/${effectiveZoneId}/racks/${id}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['racks', companyId, effectiveWarehouseId, effectiveZoneId] }),
  });

  if (!companyId) return null;

  const editingRack = racksQuery.data?.data.find((r) => r.id === editingRackId) ?? null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Racks</h2>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createRack.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
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
                  onChange={(id) => {
                    field.onChange(id);
                    setLookupZoneId(id);
                  }}
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
            Rack name
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
            Rack code
            <Controller
              control={control}
              name="code"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="R-01" {...field} />
              )}
            />
            {errors.code && <span className="text-xs text-danger">{errors.code.message}</span>}
          </label>
          <div className="col-span-2">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create rack'}</Button>
          </div>
        </form>
      </Card>

      {racksQuery.data && (
        <Card>
          <DataTable
            columns={[
              { header: 'Code', render: (rack: RackDto) => rack.code },
              { header: 'Name', render: (rack: RackDto) => rack.name },
              { header: 'Status', render: (rack: RackDto) => (rack.isActive ? 'Active' : 'Inactive') },
              { header: 'Created', render: (rack: RackDto) => new Date(rack.createdAt).toLocaleDateString() },
              {
                header: 'Actions',
                render: (rack: RackDto) => (
                  <div className="flex items-center gap-2">
                    <Button type="button" variant="secondary" onClick={() => setEditingRackId(rack.id)}>
                      Edit
                    </Button>
                    <Button
                      type="button"
                      variant="danger"
                      disabled={!rack.isActive || (deactivateRack.isPending && deactivateRack.variables === rack.id)}
                      onClick={() => deactivateRack.mutate(rack.id)}
                    >
                      {rack.isActive ? 'Deactivate' : 'Deactivated'}
                    </Button>
                  </div>
                ),
              },
            ]}
            rows={racksQuery.data.data}
            isLoading={racksQuery.isLoading}
            emptyMessage="No racks yet for this zone."
            rowKey={(rack) => rack.id}
          />
        </Card>
      )}
      {deactivateRack.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that rack.</p>
      )}

      {editingRack && (
        <RackEditPanel
          companyId={companyId}
          warehouseId={effectiveWarehouseId}
          zoneId={effectiveZoneId}
          rack={editingRack}
          onClose={() => setEditingRackId(null)}
        />
      )}
    </div>
  );
}

interface RackEditPanelProps {
  companyId: string;
  warehouseId: string;
  zoneId: string;
  rack: RackDto;
  onClose: () => void;
}

function RackEditPanel({ companyId, warehouseId, zoneId, rack, onClose }: RackEditPanelProps) {
  const queryClient = useQueryClient();

  const { register, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    values: { name: rack.name },
  });

  const updateRack = useMutation({
    mutationFn: (values: EditFormValues) =>
      apiClient.put<RackDto>(`/warehouse/warehouses/${warehouseId}/zones/${zoneId}/racks/${rack.id}`, { companyId, ...values }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['racks', companyId, warehouseId, zoneId] });
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
        <h2 className="text-lg font-semibold text-text">Edit rack — {rack.code}</h2>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form noValidate onSubmit={handleSubmit((values) => updateRack.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <label className="flex flex-col gap-1 text-sm">
          Name
          <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('name')} />
          {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
        </label>
        <div className="col-span-2 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save changes'}</Button>
          {updateRack.isError && (
            <span role="alert" className="text-sm text-danger">Could not save that rack.</span>
          )}
        </div>
      </form>
    </Card>
  );
}
