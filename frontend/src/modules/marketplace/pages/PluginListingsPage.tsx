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
import { PluginInstallationsPanel } from './PluginInstallationsPanel';

const SEARCH_DEBOUNCE_MS = 250;

const CATEGORIES = ['Plugin', 'Theme', 'ReportPack', 'WorkflowPack', 'IndustryExtension', 'AiAgent'] as const;

const schema = z.object({
  code: z.string().min(1, 'Code is required').max(20),
  name: z.string().min(1, 'Name is required').max(200),
  publisher: z.string().min(1, 'Publisher is required').max(200),
  category: z.enum(CATEGORIES),
});
type FormValues = z.infer<typeof schema>;

interface PluginListingDto {
  id: string;
  code: string;
  name: string;
  publisher: string;
  category: string;
  isActive: boolean;
}

/**
 * Plugin Listings — Marketplace's first real frontend slice (backend has
 * existed since this pass; this closes the "frontend panel deferred" gap the
 * same day the backend slice was built). The extension catalog
 * (05_MODULE_ROADMAP.md's Marketplace line item — Plugins/Themes/Report
 * packs/Workflow packs/Industry extensions/AI agents, one category enum).
 * Top-level page for /marketplace, with PluginInstallationsPanel rendered as
 * a sibling panel below it, same pattern as KpiSnapshotsPanel under
 * KpiDefinitionsPage.
 */
export function PluginListingsPage() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebouncedValue(search, SEARCH_DEBOUNCE_MS);

  const listingsQuery = useQuery({
    queryKey: ['plugin-listings', companyId, debouncedSearch],
    queryFn: () => {
      const params = new URLSearchParams({ companyId, page: '1', pageSize: '50' });
      if (debouncedSearch.trim()) params.set('search', debouncedSearch.trim());
      return apiClient.get<PagedResult<PluginListingDto>>(`/marketplace/plugin-listings?${params.toString()}`);
    },
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { code: '', name: '', publisher: '', category: 'Plugin' },
  });

  const createListing = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<PluginListingDto>('/marketplace/plugin-listings', {
        companyId,
        code: values.code,
        name: values.name,
        publisher: values.publisher,
        category: values.category,
      }),
    onSuccess: () => {
      reset({ code: '', name: '', publisher: '', category: 'Plugin' });
      queryClient.invalidateQueries({ queryKey: ['plugin-listings', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  // Soft-deactivate only — PluginListingsController exposes this as a
  // dedicated POST .../{id}/deactivate action, same convention as
  // AssetsController/EmployeesController/KpiDefinitionsController.
  const deactivateListing = useMutation({
    mutationFn: (listingId: string) => apiClient.post<PluginListingDto>(`/marketplace/plugin-listings/${listingId}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['plugin-listings', companyId] }),
  });

  if (!companyId) {
    return <p className="text-text-muted">Set an active Company ID in the header above to manage the plugin catalog.</p>;
  }

  return (
    <div>
      <h1 className="mb-1 text-xl font-semibold text-text">Plugin Listings</h1>
      <p className="mb-4 text-sm text-text-muted">The extension catalog — Marketplace, Phase 8.</p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createListing.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Code
            <Controller
              control={control}
              name="code"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="WH-SCAN" {...field} />
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
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Warehouse Scanner" {...field} />
              )}
            />
            {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Publisher
            <Controller
              control={control}
              name="publisher"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.publisher && <span className="text-xs text-danger">{errors.publisher.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Category
            <Controller
              control={control}
              name="category"
              render={({ field }) => (
                <select className="rounded-md border border-border bg-surface px-2 py-1.5" {...field}>
                  {CATEGORIES.map((category) => (
                    <option key={category} value={category}>{category}</option>
                  ))}
                </select>
              )}
            />
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Add listing'}</Button>
          </div>
        </form>
        {createListing.isError && createListing.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createListing.error.problem.title}</p>
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
            { header: 'Code', render: (row: PluginListingDto) => row.code },
            { header: 'Name', render: (row: PluginListingDto) => row.name },
            { header: 'Publisher', render: (row: PluginListingDto) => row.publisher },
            { header: 'Category', render: (row: PluginListingDto) => row.category },
            { header: 'Status', render: (row: PluginListingDto) => (row.isActive ? 'Active' : 'Delisted') },
            {
              header: 'Actions',
              render: (row: PluginListingDto) => (
                <Button
                  type="button"
                  variant="danger"
                  disabled={!row.isActive || deactivateListing.isPending}
                  onClick={() => deactivateListing.mutate(row.id)}
                >
                  {row.isActive ? 'Delist' : 'Delisted'}
                </Button>
              ),
            },
          ]}
          rows={listingsQuery.data?.data}
          isLoading={listingsQuery.isLoading}
          isError={listingsQuery.isError}
          errorMessage="Could not load plugin listings."
          emptyMessage="No listings yet — add the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {deactivateListing.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not delist that plugin.</p>
      )}

      <PluginInstallationsPanel />
    </div>
  );
}
