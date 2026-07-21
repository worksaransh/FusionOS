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
import { useBranchOptions } from '../../../shared/api/entityOptions';
import { useDebouncedValue } from '../../../shared/hooks/useDebouncedValue';
import type { PagedResult } from '../../../shared/api/types';

const SEARCH_DEBOUNCE_MS = 250;
const GUID_OR_BLANK = /^[0-9a-fA-F-]{36}$/;

const schema = z.object({
  code: z.string().min(1, 'Code is required').max(20),
  name: z.string().min(1, 'Name is required').max(200),
  branchId: z.string().refine((v) => v === '' || GUID_OR_BLANK.test(v), 'Must be blank or a valid Branch'),
  parentDepartmentId: z.string().refine((v) => v === '' || GUID_OR_BLANK.test(v), 'Must be blank or a valid Department'),
});
type FormValues = z.infer<typeof schema>;

// Update command deliberately excludes Code — it's the immutable business key
// (see UpdateDepartmentCommand.cs / DepartmentsController.Update), same
// convention as Branch's/Account's edit form.
const editSchema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  branchId: z.string().refine((v) => v === '' || GUID_OR_BLANK.test(v), 'Must be blank or a valid Branch'),
  parentDepartmentId: z.string().refine((v) => v === '' || GUID_OR_BLANK.test(v), 'Must be blank or a valid Department'),
});
type EditFormValues = z.infer<typeof editSchema>;

interface DepartmentDto {
  id: string;
  companyId: string;
  branchId: string | null;
  name: string;
  code: string;
  parentDepartmentId: string | null;
  isActive: boolean;
  createdAt: string;
}

/**
 * Departments — optionally scoped to a Branch, optionally nested under a
 * parent Department (self-reference, same idea as Account's ParentAccountId).
 * Department's own list (already fetched for this panel's table) backs the
 * parent-department picker client-side, same one-page/client-filtered
 * approach as Zone/Bin's pickers — no dedicated useDepartmentOptions hook
 * exists yet, and this panel's own current page is a reasonable source for
 * "pick a sibling department" at this scale. The Branch picker instead uses
 * the shared server-side-searchable useBranchOptions hook.
 */
export function DepartmentsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [editingDepartmentId, setEditingDepartmentId] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebouncedValue(search, SEARCH_DEBOUNCE_MS);

  const branchOptions = useBranchOptions(companyId);

  const departmentsQuery = useQuery({
    queryKey: ['departments', companyId, debouncedSearch],
    queryFn: () => {
      const params = new URLSearchParams({ companyId, page: '1', pageSize: '50' });
      if (debouncedSearch.trim()) params.set('search', debouncedSearch.trim());
      return apiClient.get<PagedResult<DepartmentDto>>(`/core/departments?${params.toString()}`);
    },
    enabled: Boolean(companyId),
  });

  const departmentOptions = (departmentsQuery.data?.data ?? []).map((d) => ({ id: d.id, label: `${d.code} — ${d.name}` }));

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { code: '', name: '', branchId: '', parentDepartmentId: '' },
  });

  const createDepartment = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<DepartmentDto>('/core/departments', {
        companyId,
        branchId: values.branchId || null,
        code: values.code,
        name: values.name,
        parentDepartmentId: values.parentDepartmentId || null,
      }),
    onSuccess: () => {
      reset({ code: '', name: '', branchId: '', parentDepartmentId: '' });
      queryClient.invalidateQueries({ queryKey: ['departments', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  // Soft-deactivate only — DepartmentsController exposes this as a dedicated
  // POST .../{id}/deactivate action (never a DELETE), same convention as
  // BranchesController/CostCentersController.
  const deactivateDepartment = useMutation({
    mutationFn: (departmentId: string) => apiClient.post<DepartmentDto>(`/core/departments/${departmentId}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['departments', companyId] }),
  });

  if (!companyId) return null;

  const editingDepartment = departmentsQuery.data?.data.find((d) => d.id === editingDepartmentId) ?? null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Departments</h2>
      <p className="mb-3 text-xs text-text-muted">
        Optionally scoped to a Branch and/or nested under a parent Department.
      </p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createDepartment.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Code
            <Controller
              control={control}
              name="code"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="ENG-01" {...field} />
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
            Branch (optional)
            <Controller
              control={control}
              name="branchId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={branchOptions.options}
                  isLoading={branchOptions.isLoading}
                  onSearchChange={branchOptions.onSearchChange}
                  placeholder="Search branches…"
                />
              )}
            />
            {errors.branchId && <span className="text-xs text-danger">{errors.branchId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Parent department (optional)
            <Controller
              control={control}
              name="parentDepartmentId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={departmentOptions}
                  placeholder="Search departments…"
                />
              )}
            />
            {errors.parentDepartmentId && <span className="text-xs text-danger">{errors.parentDepartmentId.message}</span>}
          </label>
          <div className="col-span-2">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create department'}</Button>
          </div>
        </form>
        {createDepartment.isError && createDepartment.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createDepartment.error.problem.title}</p>
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
            { header: 'Code', render: (row: DepartmentDto) => row.code },
            { header: 'Name', render: (row: DepartmentDto) => row.name },
            { header: 'Branch', render: (row: DepartmentDto) => (row.branchId ? branchOptions.options.find((b) => b.id === row.branchId)?.label ?? row.branchId : '—') },
            { header: 'Parent department', render: (row: DepartmentDto) => (row.parentDepartmentId ? departmentOptions.find((d) => d.id === row.parentDepartmentId)?.label ?? row.parentDepartmentId : '—') },
            { header: 'Status', render: (row: DepartmentDto) => (row.isActive ? 'Active' : 'Inactive') },
            { header: 'Created', render: (row: DepartmentDto) => new Date(row.createdAt).toLocaleDateString() },
            {
              header: 'Actions',
              render: (row: DepartmentDto) => (
                <div className="flex items-center gap-2">
                  <Button type="button" variant="secondary" onClick={() => setEditingDepartmentId(row.id)}>
                    Edit
                  </Button>
                  <Button
                    type="button"
                    variant="danger"
                    disabled={!row.isActive || deactivateDepartment.isPending}
                    onClick={() => deactivateDepartment.mutate(row.id)}
                  >
                    {row.isActive ? 'Deactivate' : 'Deactivated'}
                  </Button>
                </div>
              ),
            },
          ]}
          rows={departmentsQuery.data?.data}
          isLoading={departmentsQuery.isLoading}
          isError={departmentsQuery.isError}
          errorMessage="Could not load departments."
          emptyMessage="No departments yet — create the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {deactivateDepartment.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that department.</p>
      )}

      {editingDepartment && (
        <DepartmentEditPanel
          companyId={companyId}
          department={editingDepartment}
          departmentOptions={departmentOptions.filter((d) => d.id !== editingDepartment.id)}
          onClose={() => setEditingDepartmentId(null)}
        />
      )}
    </div>
  );
}

interface DepartmentEditPanelProps {
  companyId: string;
  department: DepartmentDto;
  departmentOptions: { id: string; label: string }[];
  onClose: () => void;
}

function DepartmentEditPanel({ companyId, department, departmentOptions, onClose }: DepartmentEditPanelProps) {
  const queryClient = useQueryClient();
  const branchOptions = useBranchOptions(companyId);

  const { control, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    values: {
      name: department.name,
      branchId: department.branchId ?? '',
      parentDepartmentId: department.parentDepartmentId ?? '',
    },
  });

  const updateDepartment = useMutation({
    mutationFn: (values: EditFormValues) =>
      apiClient.put<DepartmentDto>(`/core/departments/${department.id}`, {
        companyId,
        name: values.name,
        branchId: values.branchId || null,
        parentDepartmentId: values.parentDepartmentId || null,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['departments', companyId] });
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
        <h3 className="text-base font-semibold text-text">Edit department — {department.code}</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form onSubmit={handleSubmit((values) => updateDepartment.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
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
          Branch (optional)
          <Controller
            control={control}
            name="branchId"
            render={({ field }) => (
              <EntityCombobox
                value={field.value}
                onChange={field.onChange}
                options={branchOptions.options}
                isLoading={branchOptions.isLoading}
                onSearchChange={branchOptions.onSearchChange}
                placeholder="Search branches…"
              />
            )}
          />
          {errors.branchId && <span className="text-xs text-danger">{errors.branchId.message}</span>}
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Parent department (optional)
          <Controller
            control={control}
            name="parentDepartmentId"
            render={({ field }) => (
              <EntityCombobox
                value={field.value}
                onChange={field.onChange}
                options={departmentOptions}
                placeholder="Search departments…"
              />
            )}
          />
          {errors.parentDepartmentId && <span className="text-xs text-danger">{errors.parentDepartmentId.message}</span>}
        </label>
        <div className="col-span-2 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save changes'}</Button>
          {updateDepartment.isError && (
            <span role="alert" className="text-sm text-danger">Could not save that department.</span>
          )}
        </div>
      </form>
    </Card>
  );
}
