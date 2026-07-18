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
import { LeaveRequestsPanel } from './LeaveRequestsPanel';

const SEARCH_DEBOUNCE_MS = 250;

const schema = z.object({
  code: z.string().min(1, 'Code is required').max(20),
  fullName: z.string().min(1, 'Full name is required').max(200),
  email: z.string().email('Must be a valid email').max(200),
  departmentName: z.string().max(200).or(z.literal('')),
  hireDate: z.string().min(1, 'Hire date is required'),
});
type FormValues = z.infer<typeof schema>;

interface EmployeeDto {
  id: string;
  code: string;
  fullName: string;
  email: string;
  departmentName: string | null;
  hireDate: string;
  isActive: boolean;
}

/**
 * Employees — HRMS's first real frontend slice (backend has existed since
 * this pass; this closes the "frontend panel deferred" gap the same day the
 * backend slice was built). Employee records (05_MODULE_ROADMAP.md). This is
 * a distinct, HRMS-owned identity record, not Core's User — a person can be
 * an Employee without ever being a User, and vice versa. Top-level page for
 * /hrms, with LeaveRequestsPanel rendered as a sibling panel below it, same
 * pattern as MaintenanceRequestsPanel under AssetsPage.
 */
export function EmployeesPage() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebouncedValue(search, SEARCH_DEBOUNCE_MS);

  const employeesQuery = useQuery({
    queryKey: ['employees', companyId, debouncedSearch],
    queryFn: () => {
      const params = new URLSearchParams({ companyId, page: '1', pageSize: '50' });
      if (debouncedSearch.trim()) params.set('search', debouncedSearch.trim());
      return apiClient.get<PagedResult<EmployeeDto>>(`/hrms/employees?${params.toString()}`);
    },
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { code: '', fullName: '', email: '', departmentName: '', hireDate: '' },
  });

  const createEmployee = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<EmployeeDto>('/hrms/employees', {
        companyId,
        code: values.code,
        fullName: values.fullName,
        email: values.email,
        departmentName: values.departmentName || null,
        hireDate: new Date(values.hireDate).toISOString(),
      }),
    onSuccess: () => {
      reset({ code: '', fullName: '', email: '', departmentName: '', hireDate: '' });
      queryClient.invalidateQueries({ queryKey: ['employees', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  // Soft-deactivate only — EmployeesController exposes this as a dedicated
  // POST .../{id}/deactivate action, same convention as AssetsController.
  const deactivateEmployee = useMutation({
    mutationFn: (employeeId: string) => apiClient.post<EmployeeDto>(`/hrms/employees/${employeeId}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['employees', companyId] }),
  });

  if (!companyId) {
    return <p className="text-text-muted">Set an active Company ID in the header above to manage employees.</p>;
  }

  return (
    <div>
      <h1 className="mb-1 text-xl font-semibold text-text">Employees</h1>
      <p className="mb-4 text-sm text-text-muted">Employee records — HRMS, Phase 4.</p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createEmployee.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <label className="flex flex-col gap-1 text-sm">
            Code
            <Controller
              control={control}
              name="code"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="EMP-01" {...field} />
              )}
            />
            {errors.code && <span className="text-xs text-danger">{errors.code.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Full name
            <Controller
              control={control}
              name="fullName"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.fullName && <span className="text-xs text-danger">{errors.fullName.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Email
            <Controller
              control={control}
              name="email"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.email && <span className="text-xs text-danger">{errors.email.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Department (optional)
            <Controller
              control={control}
              name="departmentName"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Engineering" {...field} />
              )}
            />
            {errors.departmentName && <span className="text-xs text-danger">{errors.departmentName.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Hire date
            <Controller
              control={control}
              name="hireDate"
              render={({ field }) => (
                <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.hireDate && <span className="text-xs text-danger">{errors.hireDate.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Add employee'}</Button>
          </div>
        </form>
        {createEmployee.isError && createEmployee.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createEmployee.error.problem.title}</p>
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
            { header: 'Code', render: (row: EmployeeDto) => row.code },
            { header: 'Name', render: (row: EmployeeDto) => row.fullName },
            { header: 'Email', render: (row: EmployeeDto) => row.email },
            { header: 'Department', render: (row: EmployeeDto) => row.departmentName ?? '—' },
            { header: 'Status', render: (row: EmployeeDto) => (row.isActive ? 'Active' : 'Inactive') },
            {
              header: 'Actions',
              render: (row: EmployeeDto) => (
                <Button
                  type="button"
                  variant="danger"
                  disabled={!row.isActive || deactivateEmployee.isPending}
                  onClick={() => deactivateEmployee.mutate(row.id)}
                >
                  {row.isActive ? 'Deactivate' : 'Deactivated'}
                </Button>
              ),
            },
          ]}
          rows={employeesQuery.data?.data}
          isLoading={employeesQuery.isLoading}
          isError={employeesQuery.isError}
          errorMessage="Could not load employees."
          emptyMessage="No employees yet — add the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {deactivateEmployee.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that employee.</p>
      )}

      <LeaveRequestsPanel />
    </div>
  );
}
