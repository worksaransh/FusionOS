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
import { KpiSnapshotsPanel } from './KpiSnapshotsPanel';

const SEARCH_DEBOUNCE_MS = 250;

const schema = z.object({
  code: z.string().min(1, 'Code is required').max(20),
  name: z.string().min(1, 'Name is required').max(200),
  unit: z.string().max(20).or(z.literal('')),
});
type FormValues = z.infer<typeof schema>;

interface KpiDefinitionDto {
  id: string;
  code: string;
  name: string;
  unit: string | null;
  isActive: boolean;
}

/**
 * KPI Definitions — Business Intelligence's first real frontend slice
 * (backend has existed since this pass; this closes the "frontend panel
 * deferred" gap the same day the backend slice was built). The KPI catalog
 * (05_MODULE_ROADMAP.md's "KPIs" line item). Top-level page for /bi, with
 * KpiSnapshotsPanel rendered as a sibling panel below it, same pattern as
 * LeaveRequestsPanel under EmployeesPage.
 */
export function KpiDefinitionsPage() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebouncedValue(search, SEARCH_DEBOUNCE_MS);

  const kpisQuery = useQuery({
    queryKey: ['kpi-definitions', companyId, debouncedSearch],
    queryFn: () => {
      const params = new URLSearchParams({ companyId, page: '1', pageSize: '50' });
      if (debouncedSearch.trim()) params.set('search', debouncedSearch.trim());
      return apiClient.get<PagedResult<KpiDefinitionDto>>(`/bi/kpi-definitions?${params.toString()}`);
    },
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { code: '', name: '', unit: '' },
  });

  const createKpi = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<KpiDefinitionDto>('/bi/kpi-definitions', {
        companyId,
        code: values.code,
        name: values.name,
        unit: values.unit || null,
      }),
    onSuccess: () => {
      reset({ code: '', name: '', unit: '' });
      queryClient.invalidateQueries({ queryKey: ['kpi-definitions', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  // Soft-deactivate only — KpiDefinitionsController exposes this as a
  // dedicated POST .../{id}/deactivate action, same convention as
  // AssetsController/EmployeesController.
  const deactivateKpi = useMutation({
    mutationFn: (kpiId: string) => apiClient.post<KpiDefinitionDto>(`/bi/kpi-definitions/${kpiId}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['kpi-definitions', companyId] }),
  });

  if (!companyId) {
    return <p className="text-text-muted">Set an active Company ID in the header above to manage KPI definitions.</p>;
  }

  return (
    <div>
      <h1 className="mb-1 text-xl font-semibold text-text">KPI Definitions</h1>
      <p className="mb-4 text-sm text-text-muted">The KPI catalog — Business Intelligence, Phase 6.</p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createKpi.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <label className="flex flex-col gap-1 text-sm">
            Code
            <Controller
              control={control}
              name="code"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="OTD" {...field} />
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
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="On-Time Delivery" {...field} />
              )}
            />
            {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Unit (optional)
            <Controller
              control={control}
              name="unit"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="%" {...field} />
              )}
            />
            {errors.unit && <span className="text-xs text-danger">{errors.unit.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Define KPI'}</Button>
          </div>
        </form>
        {createKpi.isError && createKpi.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createKpi.error.problem.title}</p>
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
            { header: 'Code', render: (row: KpiDefinitionDto) => row.code },
            { header: 'Name', render: (row: KpiDefinitionDto) => row.name },
            { header: 'Unit', render: (row: KpiDefinitionDto) => row.unit ?? '—' },
            { header: 'Status', render: (row: KpiDefinitionDto) => (row.isActive ? 'Active' : 'Inactive') },
            {
              header: 'Actions',
              render: (row: KpiDefinitionDto) => (
                <Button
                  type="button"
                  variant="danger"
                  disabled={!row.isActive || deactivateKpi.isPending}
                  onClick={() => deactivateKpi.mutate(row.id)}
                >
                  {row.isActive ? 'Deactivate' : 'Deactivated'}
                </Button>
              ),
            },
          ]}
          rows={kpisQuery.data?.data}
          isLoading={kpisQuery.isLoading}
          isError={kpisQuery.isError}
          errorMessage="Could not load KPI definitions."
          emptyMessage="No KPIs defined yet — create the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {deactivateKpi.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that KPI.</p>
      )}

      <KpiSnapshotsPanel />
    </div>
  );
}
