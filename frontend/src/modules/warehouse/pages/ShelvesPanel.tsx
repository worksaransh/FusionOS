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
import { useWarehouseOptions, useZoneOptions, useRackOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  warehouseId: z.string().uuid('Pick a warehouse'),
  zoneId: z.string().uuid('Pick a zone'),
  rackId: z.string().uuid('Pick a rack'),
  name: z.string().min(1, 'Name is required').max(150),
  code: z.string().min(1, 'Code is required').max(20),
});
type FormValues = z.infer<typeof schema>;

// Update command deliberately excludes Code and RackId — only Name is
// editable (see UpdateShelfCommand.cs / ShelvesController.Update), same
// immutability rule as Rack/Zone/Bin's own edit forms.
const editSchema = z.object({
  name: z.string().min(1, 'Name is required').max(150),
});
type EditFormValues = z.infer<typeof editSchema>;

interface ShelfDto {
  id: string;
  rackId: string;
  name: string;
  code: string;
  isActive: boolean;
  createdAt: string;
}

/** Shelves — an optional level nested under Rack (one level deeper than Rack nests under Zone), the finest-grained refinement a Bin can optionally reference. Pick a Warehouse, then a Zone, then a Rack, from searchable dropdowns — same drill-down pattern as RacksPanel/BinsPanel. */
export function ShelvesPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [lookupWarehouseId, setLookupWarehouseId] = useState('');
  const [lookupZoneId, setLookupZoneId] = useState('');
  const [lookupRackId, setLookupRackId] = useState('');
  const [editingShelfId, setEditingShelfId] = useState<string | null>(null);

  const warehouseOptions = useWarehouseOptions(companyId);

  const { control, handleSubmit, watch, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { warehouseId: '', zoneId: '', rackId: '', name: '', code: '' },
  });
  const watchedWarehouseId = watch('warehouseId');
  const watchedZoneId = watch('zoneId');
  const watchedRackId = watch('rackId');
  const effectiveWarehouseId = lookupWarehouseId || watchedWarehouseId;
  const effectiveZoneId = lookupZoneId || watchedZoneId;
  const effectiveRackId = lookupRackId || watchedRackId;
  const zoneOptions = useZoneOptions(companyId, effectiveWarehouseId || undefined);
  const rackOptions = useRackOptions(companyId, effectiveWarehouseId || undefined, effectiveZoneId || undefined);

  const createShelf = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post(
        `/warehouse/warehouses/${values.warehouseId}/zones/${values.zoneId}/racks/${values.rackId}/shelves`,
        { companyId, name: values.name, code: values.code },
      ),
    onSuccess: (_data, variables) => {
      reset({ warehouseId: variables.warehouseId, zoneId: variables.zoneId, rackId: variables.rackId, name: '', code: '' });
      setLookupWarehouseId(variables.warehouseId);
      setLookupZoneId(variables.zoneId);
      setLookupRackId(variables.rackId);
      queryClient.invalidateQueries({ queryKey: ['shelves', companyId, variables.warehouseId, variables.zoneId, variables.rackId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const shelvesQuery = useQuery({
    queryKey: ['shelves', companyId, effectiveWarehouseId, effectiveZoneId, effectiveRackId],
    queryFn: () =>
      apiClient.get<PagedResult<ShelfDto>>(
        `/warehouse/warehouses/${effectiveWarehouseId}/zones/${effectiveZoneId}/racks/${effectiveRackId}/shelves?companyId=${companyId}&page=1&pageSize=25`,
      ),
    enabled: Boolean(companyId && effectiveWarehouseId && effectiveZoneId && effectiveRackId),
  });

  // Soft-deactivate only — hits POST .../racks/{rackId}/shelves/{id}/deactivate, never a DELETE.
  const deactivateShelf = useMutation({
    mutationFn: (id: string) =>
      apiClient.post(
        `/warehouse/warehouses/${effectiveWarehouseId}/zones/${effectiveZoneId}/racks/${effectiveRackId}/shelves/${id}/deactivate`,
        { companyId },
      ),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['shelves', companyId, effectiveWarehouseId, effectiveZoneId, effectiveRackId] }),
  });

  if (!companyId) return null;

  const editingShelf = shelvesQuery.data?.data.find((s) => s.id === editingShelfId) ?? null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Shelves</h2>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createShelf.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
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
            Rack
            <Controller
              control={control}
              name="rackId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={(id) => {
                    field.onChange(id);
                    setLookupRackId(id);
                  }}
                  options={rackOptions.options}
                  isLoading={rackOptions.isLoading}
                  placeholder={effectiveZoneId ? 'Search racks…' : 'Pick a zone first'}
                  disabled={!effectiveZoneId}
                />
              )}
            />
            {errors.rackId && <span className="text-xs text-danger">{errors.rackId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Shelf name
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
            Shelf code
            <Controller
              control={control}
              name="code"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="S-01" {...field} />
              )}
            />
            {errors.code && <span className="text-xs text-danger">{errors.code.message}</span>}
          </label>
          <div className="col-span-2">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create shelf'}</Button>
          </div>
        </form>
      </Card>

      {shelvesQuery.data && (
        <Card>
          <DataTable
            columns={[
              { header: 'Code', render: (shelf: ShelfDto) => shelf.code },
              { header: 'Name', render: (shelf: ShelfDto) => shelf.name },
              { header: 'Status', render: (shelf: ShelfDto) => (shelf.isActive ? 'Active' : 'Inactive') },
              { header: 'Created', render: (shelf: ShelfDto) => new Date(shelf.createdAt).toLocaleDateString() },
              {
                header: 'Actions',
                render: (shelf: ShelfDto) => (
                  <div className="flex items-center gap-2">
                    <Button type="button" variant="secondary" onClick={() => setEditingShelfId(shelf.id)}>
                      Edit
                    </Button>
                    <Button
                      type="button"
                      variant="danger"
                      disabled={!shelf.isActive || (deactivateShelf.isPending && deactivateShelf.variables === shelf.id)}
                      onClick={() => deactivateShelf.mutate(shelf.id)}
                    >
                      {shelf.isActive ? 'Deactivate' : 'Deactivated'}
                    </Button>
                  </div>
                ),
              },
            ]}
            rows={shelvesQuery.data.data}
            isLoading={shelvesQuery.isLoading}
            emptyMessage="No shelves yet for this rack."
            rowKey={(shelf) => shelf.id}
          />
        </Card>
      )}
      {deactivateShelf.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that shelf.</p>
      )}

      {editingShelf && (
        <ShelfEditPanel
          companyId={companyId}
          warehouseId={effectiveWarehouseId}
          zoneId={effectiveZoneId}
          rackId={effectiveRackId}
          shelf={editingShelf}
          onClose={() => setEditingShelfId(null)}
        />
      )}
    </div>
  );
}

interface ShelfEditPanelProps {
  companyId: string;
  warehouseId: string;
  zoneId: string;
  rackId: string;
  shelf: ShelfDto;
  onClose: () => void;
}

function ShelfEditPanel({ companyId, warehouseId, zoneId, rackId, shelf, onClose }: ShelfEditPanelProps) {
  const queryClient = useQueryClient();

  const { register, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    values: { name: shelf.name },
  });

  const updateShelf = useMutation({
    mutationFn: (values: EditFormValues) =>
      apiClient.put<ShelfDto>(
        `/warehouse/warehouses/${warehouseId}/zones/${zoneId}/racks/${rackId}/shelves/${shelf.id}`,
        { companyId, ...values },
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['shelves', companyId, warehouseId, zoneId, rackId] });
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
        <h2 className="text-lg font-semibold text-text">Edit shelf — {shelf.code}</h2>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form noValidate onSubmit={handleSubmit((values) => updateShelf.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <label className="flex flex-col gap-1 text-sm">
          Name
          <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('name')} />
          {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
        </label>
        <div className="col-span-2 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save changes'}</Button>
          {updateShelf.isError && (
            <span role="alert" className="text-sm text-danger">Could not save that shelf.</span>
          )}
        </div>
      </form>
    </Card>
  );
}
