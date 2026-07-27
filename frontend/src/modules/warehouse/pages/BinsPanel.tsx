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
import { useWarehouseOptions, useZoneOptions, useRackOptions, useShelfOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  warehouseId: z.string().uuid('Pick a warehouse'),
  zoneId: z.string().uuid('Pick a zone'),
  name: z.string().min(1, 'Name is required').max(150),
  code: z.string().min(1, 'Code is required').max(20),
  // Rack is a UI-only narrowing field (Shelf's list endpoint nests under a
  // Rack, not directly under a Zone) — never sent to CreateBinCommand, only
  // used to filter the Shelf picker below. Shelf itself is optional too:
  // Bin.ShelfId is an additive refinement on top of the required ZoneId
  // (see Bin.cs / AssignBinShelfCommand.cs), assigned via a follow-up
  // POST {id}/shelf call once the bin exists, same reasoning as
  // WarehousesPage's create-then-optionally-refine pattern.
  rackId: z.string().optional(),
  shelfId: z.string().optional(),
});
type FormValues = z.infer<typeof schema>;

// Update command deliberately excludes Code and ZoneId — only Name is
// editable (see UpdateBinCommand.cs / BinsController.Update), same
// immutability rule as Zone's own edit form. Shelf assignment is handled by
// a separate mutation (AssignBinShelfCommand via POST {id}/shelf) below,
// not folded into this form/command.
const editSchema = z.object({
  name: z.string().min(1, 'Name is required').max(150),
});
type EditFormValues = z.infer<typeof editSchema>;

interface BinDto {
  id: string;
  zoneId: string;
  name: string;
  code: string;
  isActive: boolean;
  createdAt: string;
  shelfId: string | null;
}

/** Bins — nests under Zone (docs/IMPLEMENTATION_PLAN.md Phase 9 "bins" item, Phase M9 2026-07-15). Pick a Warehouse, then a Zone, from searchable dropdowns. */
export function BinsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [lookupWarehouseId, setLookupWarehouseId] = useState('');
  const [lookupZoneId, setLookupZoneId] = useState('');
  const [editingBinId, setEditingBinId] = useState<string | null>(null);

  const warehouseOptions = useWarehouseOptions(companyId);

  const { control, handleSubmit, watch, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { warehouseId: '', zoneId: '', name: '', code: '', rackId: '', shelfId: '' },
  });
  const watchedWarehouseId = watch('warehouseId');
  const watchedZoneId = watch('zoneId');
  const watchedRackId = watch('rackId');
  const effectiveWarehouseId = lookupWarehouseId || watchedWarehouseId;
  const effectiveZoneId = lookupZoneId || watchedZoneId;
  const zoneOptions = useZoneOptions(companyId, effectiveWarehouseId || undefined);
  const rackOptions = useRackOptions(companyId, effectiveWarehouseId || undefined, effectiveZoneId || undefined);
  const shelfOptions = useShelfOptions(companyId, effectiveWarehouseId || undefined, effectiveZoneId || undefined, watchedRackId || undefined);

  const createBin = useMutation({
    mutationFn: async (values: FormValues) => {
      const created = await apiClient.post<BinDto>(
        `/warehouse/warehouses/${values.warehouseId}/zones/${values.zoneId}/bins`,
        { companyId, name: values.name, code: values.code },
      );
      // Shelf is optional and additive (Bin.ShelfId) — assigned via a
      // separate follow-up call once the bin exists, not part of CreateBinCommand.
      if (values.shelfId) {
        await apiClient.post(`/warehouse/warehouses/${values.warehouseId}/zones/${values.zoneId}/bins/${created.id}/shelf`, {
          companyId,
          shelfId: values.shelfId,
        });
      }
      return created;
    },
    onSuccess: (_data, variables) => {
      reset({ warehouseId: variables.warehouseId, zoneId: variables.zoneId, name: '', code: '', rackId: '', shelfId: '' });
      setLookupWarehouseId(variables.warehouseId);
      setLookupZoneId(variables.zoneId);
      queryClient.invalidateQueries({ queryKey: ['bins', companyId, variables.warehouseId, variables.zoneId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const binsQuery = useQuery({
    queryKey: ['bins', companyId, effectiveWarehouseId, effectiveZoneId],
    queryFn: () =>
      apiClient.get<PagedResult<BinDto>>(
        `/warehouse/warehouses/${effectiveWarehouseId}/zones/${effectiveZoneId}/bins?companyId=${companyId}&page=1&pageSize=25`,
      ),
    enabled: Boolean(companyId && effectiveWarehouseId && effectiveZoneId),
  });

  // Soft-deactivate only — hits POST /{warehouseId}/zones/{zoneId}/bins/{id}/deactivate, never a DELETE.
  const deactivateBin = useMutation({
    mutationFn: (id: string) =>
      apiClient.post(`/warehouse/warehouses/${effectiveWarehouseId}/zones/${effectiveZoneId}/bins/${id}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['bins', companyId, effectiveWarehouseId, effectiveZoneId] }),
  });

  if (!companyId) return null;

  const editingBin = binsQuery.data?.data.find((b) => b.id === editingBinId) ?? null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Bins</h2>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createBin.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
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
            Rack (optional)
            <Controller
              control={control}
              name="rackId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value ?? ''}
                  onChange={(id) => field.onChange(id)}
                  options={rackOptions.options}
                  isLoading={rackOptions.isLoading}
                  placeholder={effectiveZoneId ? 'Search racks…' : 'Pick a zone first'}
                  disabled={!effectiveZoneId}
                />
              )}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Shelf (optional)
            <Controller
              control={control}
              name="shelfId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value ?? ''}
                  onChange={(id) => field.onChange(id)}
                  options={shelfOptions.options}
                  isLoading={shelfOptions.isLoading}
                  placeholder={watchedRackId ? 'Search shelves…' : 'Pick a rack first'}
                  disabled={!watchedRackId}
                />
              )}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Bin name
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
            Bin code
            <Controller
              control={control}
              name="code"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="A-01-03" {...field} />
              )}
            />
            {errors.code && <span className="text-xs text-danger">{errors.code.message}</span>}
          </label>
          <div className="col-span-2">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create bin'}</Button>
          </div>
        </form>
      </Card>

      {binsQuery.data && (
        <Card>
          <DataTable
            columns={[
              { header: 'Code', render: (bin: BinDto) => bin.code },
              { header: 'Name', render: (bin: BinDto) => bin.name },
              { header: 'Status', render: (bin: BinDto) => (bin.isActive ? 'Active' : 'Inactive') },
              { header: 'Created', render: (bin: BinDto) => new Date(bin.createdAt).toLocaleDateString() },
              {
                header: 'Actions',
                render: (bin: BinDto) => (
                  <div className="flex items-center gap-2">
                    <Button type="button" variant="secondary" onClick={() => setEditingBinId(bin.id)}>
                      Edit
                    </Button>
                    <Button
                      type="button"
                      variant="danger"
                      disabled={!bin.isActive || (deactivateBin.isPending && deactivateBin.variables === bin.id)}
                      onClick={() => deactivateBin.mutate(bin.id)}
                    >
                      {bin.isActive ? 'Deactivate' : 'Deactivated'}
                    </Button>
                  </div>
                ),
              },
            ]}
            rows={binsQuery.data.data}
            isLoading={binsQuery.isLoading}
            emptyMessage="No bins yet for this zone."
            rowKey={(bin) => bin.id}
          />
        </Card>
      )}
      {deactivateBin.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that bin.</p>
      )}

      {editingBin && (
        <BinEditPanel
          companyId={companyId}
          warehouseId={effectiveWarehouseId}
          zoneId={effectiveZoneId}
          bin={editingBin}
          onClose={() => setEditingBinId(null)}
        />
      )}
    </div>
  );
}

interface BinEditPanelProps {
  companyId: string;
  warehouseId: string;
  zoneId: string;
  bin: BinDto;
  onClose: () => void;
}

function BinEditPanel({ companyId, warehouseId, zoneId, bin, onClose }: BinEditPanelProps) {
  const queryClient = useQueryClient();

  const { register, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    values: { name: bin.name },
  });

  const updateBin = useMutation({
    mutationFn: (values: EditFormValues) =>
      apiClient.put<BinDto>(`/warehouse/warehouses/${warehouseId}/zones/${zoneId}/bins/${bin.id}`, { companyId, ...values }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['bins', companyId, warehouseId, zoneId] });
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
        <h2 className="text-lg font-semibold text-text">Edit bin — {bin.code}</h2>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form noValidate onSubmit={handleSubmit((values) => updateBin.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <label className="flex flex-col gap-1 text-sm">
          Name
          <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('name')} />
          {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
        </label>
        <div className="col-span-2 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save changes'}</Button>
          {updateBin.isError && (
            <span role="alert" className="text-sm text-danger">Could not save that bin.</span>
          )}
        </div>
      </form>

      <ShelfAssignmentSection companyId={companyId} warehouseId={warehouseId} zoneId={zoneId} bin={bin} />
    </Card>
  );
}

interface ShelfAssignmentSectionProps {
  companyId: string;
  warehouseId: string;
  zoneId: string;
  bin: BinDto;
}

/**
 * Assigns (or clears) this bin's optional Shelf refinement via
 * AssignBinShelfCommand (POST {id}/shelf) — separate from Name/UpdateBin
 * above since it's a distinct command, not a field on UpdateBinCommand.
 * Only offers racks/shelves that belong to this bin's own zone (Rack nests
 * under Zone, Shelf nests under Rack), matching the backend's
 * zone-consistency rejection in AssignBinShelfCommandHandler.
 */
function ShelfAssignmentSection({ companyId, warehouseId, zoneId, bin }: ShelfAssignmentSectionProps) {
  const queryClient = useQueryClient();
  const [rackId, setRackId] = useState('');
  const [shelfId, setShelfId] = useState('');

  const rackOptions = useRackOptions(companyId, warehouseId, zoneId);
  const shelfOptions = useShelfOptions(companyId, warehouseId, zoneId, rackId || undefined);

  const assignShelf = useMutation({
    mutationFn: (nextShelfId: string | null) =>
      apiClient.post<BinDto>(`/warehouse/warehouses/${warehouseId}/zones/${zoneId}/bins/${bin.id}/shelf`, {
        companyId,
        shelfId: nextShelfId,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['bins', companyId, warehouseId, zoneId] });
    },
  });

  return (
    <div className="col-span-2 mt-6 border-t border-border pt-4">
      <h3 className="mb-2 text-sm font-semibold text-text">Shelf location (optional)</h3>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <label className="flex flex-col gap-1 text-sm">
          Rack
          <EntityCombobox
            value={rackId}
            onChange={setRackId}
            options={rackOptions.options}
            isLoading={rackOptions.isLoading}
            placeholder="Search racks…"
          />
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Shelf
          <EntityCombobox
            value={shelfId}
            onChange={setShelfId}
            options={shelfOptions.options}
            isLoading={shelfOptions.isLoading}
            placeholder={rackId ? 'Search shelves…' : 'Pick a rack first'}
            disabled={!rackId}
          />
        </label>
        <div className="col-span-2 flex items-center gap-3">
          <Button type="button" disabled={!shelfId || assignShelf.isPending} onClick={() => assignShelf.mutate(shelfId)}>
            {assignShelf.isPending ? 'Assigning…' : 'Assign shelf'}
          </Button>
          {bin.shelfId && (
            <Button type="button" variant="secondary" disabled={assignShelf.isPending} onClick={() => assignShelf.mutate(null)}>
              Clear shelf
            </Button>
          )}
          {assignShelf.isError && (
            <span role="alert" className="text-sm text-danger">Could not assign that shelf — check it belongs to this bin's zone.</span>
          )}
        </div>
      </div>
    </div>
  );
}
