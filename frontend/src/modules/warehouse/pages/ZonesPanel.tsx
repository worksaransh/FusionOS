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

  if (!companyId) return null;

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
              { header: 'Created', render: (zone: ZoneDto) => new Date(zone.createdAt).toLocaleDateString() },
            ]}
            rows={zonesQuery.data.data}
            isLoading={zonesQuery.isLoading}
            emptyMessage="No zones yet for this warehouse."
            rowKey={(zone) => zone.id}
          />
        </Card>
      )}
    </div>
  );
}
