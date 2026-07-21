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
  isHeadOffice: z.boolean(),
});
type FormValues = z.infer<typeof schema>;

// Update command deliberately excludes Code — it's the immutable business key
// (see UpdateBranchCommand.cs / BranchesController.Update), same convention as
// Finance's CostCenters edit form.
const editSchema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  isHeadOffice: z.boolean(),
});
type EditFormValues = z.infer<typeof editSchema>;

interface BranchDto {
  id: string;
  companyId: string;
  name: string;
  code: string;
  isHeadOffice: boolean;
  isActive: boolean;
  createdAt: string;
}

/**
 * Branches — locations/sites under a Company. Pure master data (Code/Name/
 * IsHeadOffice/IsActive), same shape as Finance's CostCentersPanel. Rendered
 * as a sibling panel under CompaniesPage, same stacking convention as
 * AccountsPage's panels (Branches/Departments belong conceptually under
 * Companies, not their own top-level route).
 */
export function BranchesPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [editingBranchId, setEditingBranchId] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebouncedValue(search, SEARCH_DEBOUNCE_MS);

  const branchesQuery = useQuery({
    queryKey: ['branches', companyId, debouncedSearch],
    queryFn: () => {
      const params = new URLSearchParams({ companyId, page: '1', pageSize: '50' });
      if (debouncedSearch.trim()) params.set('search', debouncedSearch.trim());
      return apiClient.get<PagedResult<BranchDto>>(`/core/branches?${params.toString()}`);
    },
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { code: '', name: '', isHeadOffice: false },
  });

  const createBranch = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<BranchDto>('/core/branches', {
        companyId,
        code: values.code,
        name: values.name,
        isHeadOffice: values.isHeadOffice,
      }),
    onSuccess: () => {
      reset({ code: '', name: '', isHeadOffice: false });
      queryClient.invalidateQueries({ queryKey: ['branches', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  // Soft-deactivate only — BranchesController exposes this as a dedicated
  // POST .../{id}/deactivate action (never a DELETE), same convention as
  // CostCentersController/AccountsController.
  const deactivateBranch = useMutation({
    mutationFn: (branchId: string) => apiClient.post<BranchDto>(`/core/branches/${branchId}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['branches', companyId] }),
  });

  if (!companyId) return null;

  const editingBranch = branchesQuery.data?.data.find((b) => b.id === editingBranchId) ?? null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Branches</h2>
      <p className="mb-3 text-xs text-text-muted">
        Locations/sites under this company — Departments below can optionally be scoped to one.
      </p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createBranch.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Code
            <Controller
              control={control}
              name="code"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="HQ-01" {...field} />
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
          <label className="flex items-center gap-2 text-sm">
            <Controller
              control={control}
              name="isHeadOffice"
              render={({ field }) => (
                <input
                  type="checkbox"
                  className="rounded border-border"
                  checked={field.value}
                  onChange={(e) => field.onChange(e.target.checked)}
                />
              )}
            />
            Head office
          </label>
          <div className="col-span-2">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create branch'}</Button>
          </div>
        </form>
        {createBranch.isError && createBranch.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createBranch.error.problem.title}</p>
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
            { header: 'Code', render: (row: BranchDto) => row.code },
            { header: 'Name', render: (row: BranchDto) => row.name },
            { header: 'Head office', render: (row: BranchDto) => (row.isHeadOffice ? 'Yes' : 'No') },
            { header: 'Status', render: (row: BranchDto) => (row.isActive ? 'Active' : 'Inactive') },
            { header: 'Created', render: (row: BranchDto) => new Date(row.createdAt).toLocaleDateString() },
            {
              header: 'Actions',
              render: (row: BranchDto) => (
                <div className="flex items-center gap-2">
                  <Button type="button" variant="secondary" onClick={() => setEditingBranchId(row.id)}>
                    Edit
                  </Button>
                  <Button
                    type="button"
                    variant="danger"
                    disabled={!row.isActive || deactivateBranch.isPending}
                    onClick={() => deactivateBranch.mutate(row.id)}
                  >
                    {row.isActive ? 'Deactivate' : 'Deactivated'}
                  </Button>
                </div>
              ),
            },
          ]}
          rows={branchesQuery.data?.data}
          isLoading={branchesQuery.isLoading}
          isError={branchesQuery.isError}
          errorMessage="Could not load branches."
          emptyMessage="No branches yet — create the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {deactivateBranch.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that branch.</p>
      )}

      {editingBranch && (
        <BranchEditPanel
          companyId={companyId}
          branch={editingBranch}
          onClose={() => setEditingBranchId(null)}
        />
      )}
    </div>
  );
}

interface BranchEditPanelProps {
  companyId: string;
  branch: BranchDto;
  onClose: () => void;
}

function BranchEditPanel({ companyId, branch, onClose }: BranchEditPanelProps) {
  const queryClient = useQueryClient();

  const { control, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    values: { name: branch.name, isHeadOffice: branch.isHeadOffice },
  });

  const updateBranch = useMutation({
    mutationFn: (values: EditFormValues) =>
      apiClient.put<BranchDto>(`/core/branches/${branch.id}`, {
        companyId,
        name: values.name,
        isHeadOffice: values.isHeadOffice,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['branches', companyId] });
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
        <h3 className="text-base font-semibold text-text">Edit branch — {branch.code}</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form onSubmit={handleSubmit((values) => updateBranch.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
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
        <label className="flex items-center gap-2 text-sm">
          <Controller
            control={control}
            name="isHeadOffice"
            render={({ field }) => (
              <input
                type="checkbox"
                className="rounded border-border"
                checked={field.value}
                onChange={(e) => field.onChange(e.target.checked)}
              />
            )}
          />
          Head office
        </label>
        <div className="col-span-2 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save changes'}</Button>
          {updateBranch.isError && (
            <span role="alert" className="text-sm text-danger">Could not save that branch.</span>
          )}
        </div>
      </form>
    </Card>
  );
}
