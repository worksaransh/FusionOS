import { useState } from 'react';
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import { useDebouncedValue } from '../../../shared/hooks/useDebouncedValue';
import type { PagedResult } from '../../../shared/api/types';
import { MaintenanceRequestsPanel } from './MaintenanceRequestsPanel';
import { MaintenanceSchedulesPanel } from './MaintenanceSchedulesPanel';

const SEARCH_DEBOUNCE_MS = 250;

const schema = z.object({
  code: z.string().min(1, 'Code is required').max(20),
  name: z.string().min(1, 'Name is required').max(200),
  location: z.string().max(200).or(z.literal('')),
});
type FormValues = z.infer<typeof schema>;

interface AssetDto {
  id: string;
  code: string;
  name: string;
  location: string | null;
  isActive: boolean;
  createdAt: string;
}

/**
 * Assets — Maintenance's first real frontend slice (backend has existed
 * since this pass; this closes the "frontend panel deferred" gap the same
 * day the backend slice was built). The machine register
 * (05_MODULE_ROADMAP.md's "Machine register" line item). Top-level page for
 * /maintenance, with MaintenanceRequestsPanel rendered as a sibling panel
 * below it, same pattern as WorkOrdersPanel under BillsOfMaterialsPage.
 */
export function AssetsPage() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebouncedValue(search, SEARCH_DEBOUNCE_MS);

  const assetsQuery = useQuery({
    queryKey: ['assets', companyId, debouncedSearch],
    queryFn: () => {
      const params = new URLSearchParams({ companyId, page: '1', pageSize: '50' });
      if (debouncedSearch.trim()) params.set('search', debouncedSearch.trim());
      return apiClient.get<PagedResult<AssetDto>>(`/maintenance/assets?${params.toString()}`);
    },
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { code: '', name: '', location: '' },
  });

  const createAsset = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<AssetDto>('/maintenance/assets', {
        companyId,
        code: values.code,
        name: values.name,
        location: values.location || null,
      }),
    onSuccess: () => {
      reset({ code: '', name: '', location: '' });
      queryClient.invalidateQueries({ queryKey: ['assets', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  // Soft-deactivate only — AssetsController exposes this as a dedicated
  // POST .../{id}/deactivate action, same convention as CostCentersController.
  const deactivateAsset = useMutation({
    mutationFn: (assetId: string) => apiClient.post<AssetDto>(`/maintenance/assets/${assetId}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['assets', companyId] }),
  });

  if (!companyId) {
    return <p className="text-text-muted">Set an active Company ID in the header above to manage assets.</p>;
  }

  return (
    <div>
      <h1 className="mb-1 text-xl font-semibold text-text">Assets</h1>
      <p className="mb-4 text-sm text-text-muted">The machine register — Maintenance, Phase 5.</p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createAsset.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <label className="flex flex-col gap-1 text-sm">
            Code
            <Controller
              control={control}
              name="code"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="GEN-01" {...field} />
              )}
            />
            {errors.code && <span className="text-xs text-danger">{errors.code.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Name
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
            Location (optional)
            <Controller
              control={control}
              name="location"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Plant A, Bay 3" {...field} />
              )}
            />
            {errors.location && <span className="text-xs text-danger">{errors.location.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Register asset'}</Button>
          </div>
        </form>
        {createAsset.isError && createAsset.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createAsset.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <label className="mb-3 flex flex-col gap-1 text-sm sm:w-72">
          Search
          <input
            className="rounded-md border border-border bg-surface px-2 py-1.5"
            placeholder="Search by code or name…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </label>
        <DataTable
          columns={[
            { header: 'Code', render: (row: AssetDto) => row.code },
            { header: 'Name', render: (row: AssetDto) => row.name },
            { header: 'Location', render: (row: AssetDto) => row.location ?? '—' },
            { header: 'Status', render: (row: AssetDto) => (row.isActive ? 'Active' : 'Inactive') },
            {
              header: 'Actions',
              render: (row: AssetDto) => (
                <Button
                  type="button"
                  variant="danger"
                  disabled={!row.isActive || deactivateAsset.isPending}
                  onClick={() => deactivateAsset.mutate(row.id)}
                >
                  {row.isActive ? 'Deactivate' : 'Deactivated'}
                </Button>
              ),
            },
          ]}
          rows={assetsQuery.data?.data}
          isLoading={assetsQuery.isLoading}
          isError={assetsQuery.isError}
          errorMessage="Could not load assets."
          emptyMessage="No assets yet — register the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {deactivateAsset.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that asset.</p>
      )}

      <MaintenanceRequestsPanel />
      <MaintenanceSchedulesPanel />
    </div>
  );
}
