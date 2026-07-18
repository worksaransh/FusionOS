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

const SEARCH_DEBOUNCE_MS = 250;

const schema = z.object({
  code: z.string().min(1, 'Code is required').max(20),
  name: z.string().min(1, 'Name is required').max(200),
});
type FormValues = z.infer<typeof schema>;

// Update command deliberately excludes Code — it's the immutable business key
// (see UpdateCostCenterCommand.cs / CostCentersController.Update), same
// convention as Account's edit form.
const editSchema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
});
type EditFormValues = z.infer<typeof editSchema>;

interface CostCenterDto {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  createdAt: string;
}

/**
 * Cost Centers — M8a, Finance depth. Pure master data (Code/Name/IsActive),
 * same shape as Account's Chart of Accounts panel minus AccountType/
 * ParentAccountId (no hierarchy — see CostCenter.cs). Rendered as a sibling
 * panel under AccountsPage, same pattern as JournalEntriesPanel and
 * ReceivablesPanel.
 */
export function CostCentersPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [editingCostCenterId, setEditingCostCenterId] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebouncedValue(search, SEARCH_DEBOUNCE_MS);

  const costCentersQuery = useQuery({
    queryKey: ['cost-centers', companyId, debouncedSearch],
    queryFn: () => {
      const params = new URLSearchParams({ companyId, page: '1', pageSize: '50' });
      if (debouncedSearch.trim()) params.set('search', debouncedSearch.trim());
      return apiClient.get<PagedResult<CostCenterDto>>(`/finance/cost-centers?${params.toString()}`);
    },
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { code: '', name: '' },
  });

  const createCostCenter = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<CostCenterDto>('/finance/cost-centers', {
        companyId,
        code: values.code,
        name: values.name,
      }),
    onSuccess: () => {
      reset({ code: '', name: '' });
      queryClient.invalidateQueries({ queryKey: ['cost-centers', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  // Soft-deactivate only — CostCentersController exposes this as a dedicated
  // POST .../{id}/deactivate action (never a DELETE), same convention as
  // AccountsController/ProductsController.
  const deactivateCostCenter = useMutation({
    mutationFn: (costCenterId: string) => apiClient.post<CostCenterDto>(`/finance/cost-centers/${costCenterId}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['cost-centers', companyId] }),
  });

  if (!companyId) return null;

  const editingCostCenter = costCentersQuery.data?.data.find((c) => c.id === editingCostCenterId) ?? null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Cost Centers</h2>
      <p className="mb-3 text-xs text-text-muted">
        Pure master data used to tag spend — not yet attached to journal lines (see CostCenter.cs).
      </p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createCostCenter.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Code
            <Controller
              control={control}
              name="code"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="CC-100" {...field} />
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
          <div className="col-span-2">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create cost center'}</Button>
          </div>
        </form>
        {createCostCenter.isError && createCostCenter.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createCostCenter.error.problem.title}</p>
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
            { header: 'Code', render: (row: CostCenterDto) => row.code },
            { header: 'Name', render: (row: CostCenterDto) => row.name },
            { header: 'Status', render: (row: CostCenterDto) => (row.isActive ? 'Active' : 'Inactive') },
            { header: 'Created', render: (row: CostCenterDto) => new Date(row.createdAt).toLocaleDateString() },
            {
              header: 'Actions',
              render: (row: CostCenterDto) => (
                <div className="flex items-center gap-2">
                  <Button type="button" variant="secondary" onClick={() => setEditingCostCenterId(row.id)}>
                    Edit
                  </Button>
                  <Button
                    type="button"
                    variant="danger"
                    disabled={!row.isActive || deactivateCostCenter.isPending}
                    onClick={() => deactivateCostCenter.mutate(row.id)}
                  >
                    {row.isActive ? 'Deactivate' : 'Deactivated'}
                  </Button>
                </div>
              ),
            },
          ]}
          rows={costCentersQuery.data?.data}
          isLoading={costCentersQuery.isLoading}
          isError={costCentersQuery.isError}
          errorMessage="Could not load cost centers."
          emptyMessage="No cost centers yet — create the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {deactivateCostCenter.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that cost center.</p>
      )}

      {editingCostCenter && (
        <CostCenterEditPanel
          companyId={companyId}
          costCenter={editingCostCenter}
          onClose={() => setEditingCostCenterId(null)}
        />
      )}
    </div>
  );
}

interface CostCenterEditPanelProps {
  companyId: string;
  costCenter: CostCenterDto;
  onClose: () => void;
}

function CostCenterEditPanel({ companyId, costCenter, onClose }: CostCenterEditPanelProps) {
  const queryClient = useQueryClient();

  const { control, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    values: { name: costCenter.name },
  });

  const updateCostCenter = useMutation({
    mutationFn: (values: EditFormValues) =>
      apiClient.put<CostCenterDto>(`/finance/cost-centers/${costCenter.id}`, {
        companyId,
        name: values.name,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cost-centers', companyId] });
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
    <Card className="mt-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-base font-semibold text-text">Edit cost center — {costCenter.code}</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form onSubmit={handleSubmit((values) => updateCostCenter.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
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
        <div className="col-span-2 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save changes'}</Button>
          {updateCostCenter.isError && (
            <span role="alert" className="text-sm text-danger">Could not save that cost center.</span>
          )}
        </div>
      </form>
    </Card>
  );
}
