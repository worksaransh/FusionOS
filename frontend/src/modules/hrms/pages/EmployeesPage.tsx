import { useEffect, useState } from 'react';
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
import { useDebouncedValue } from '../../../shared/hooks/useDebouncedValue';
import { useDepartmentOptions, useEmployeeOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';
import { AttendancePanel } from './AttendancePanel';
import { LeaveBalancesPanel } from './LeaveBalancesPanel';
import { LeaveRequestsPanel } from './LeaveRequestsPanel';
import { OffboardingPanel } from './OffboardingPanel';
import { OnboardingPanel } from './OnboardingPanel';
import { PayrollPanel } from './PayrollPanel';

const SEARCH_DEBOUNCE_MS = 250;

const schema = z.object({
  code: z.string().min(1, 'Code is required').max(20),
  fullName: z.string().min(1, 'Full name is required').max(200),
  email: z.string().email('Must be a valid email').max(200),
  departmentName: z.string().max(200).or(z.literal('')),
  hireDate: z.string().min(1, 'Hire date is required'),
  departmentId: z.string().refine((v) => v === '' || /^[0-9a-fA-F-]{36}$/.test(v), 'Must be blank or a valid department'),
  managerId: z.string().refine((v) => v === '' || /^[0-9a-fA-F-]{36}$/.test(v), 'Must be blank or a valid employee'),
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
  departmentId: string | null;
  managerId: string | null;
}

interface EmployeeOrgChartDto {
  employee: EmployeeDto;
  directReports: EmployeeDto[];
}

const EMPTY_FORM: FormValues = { code: '', fullName: '', email: '', departmentName: '', hireDate: '', departmentId: '', managerId: '' };

/**
 * Employees — HRMS's first real frontend slice (backend has existed since
 * this pass; this closes the "frontend panel deferred" gap the same day the
 * backend slice was built). Employee records (05_MODULE_ROADMAP.md). This is
 * a distinct, HRMS-owned identity record, not Core's User — a person can be
 * an Employee without ever being a User, and vice versa. Top-level page for
 * /hrms, with LeaveRequestsPanel/LeaveBalancesPanel/AttendancePanel/PayrollPanel/
 * OnboardingPanel/OffboardingPanel rendered as sibling panels below it, same
 * pattern as MaintenanceRequestsPanel under AssetsPage.
 *
 * Org-chart/Department depth pass (2026-07-23): the create form now also
 * accepts an optional Manager (another Employee, same-module) and Department
 * (Core's Department aggregate, cross-module — picked via EntityCombobox over
 * useDepartmentOptions, never a project reference). A row's "Edit" button
 * switches the form into edit mode (PUT .../employees/{id}) since Employee had
 * no update path at all before this pass. A small "direct reports" mini org
 * chart is rendered below the table for whichever employee is selected there.
 */
export function EmployeesPage() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebouncedValue(search, SEARCH_DEBOUNCE_MS);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [orgChartEmployeeId, setOrgChartEmployeeId] = useState<string | null>(null);

  const departmentOptions = useDepartmentOptions(companyId);
  const managerOptions = useEmployeeOptions(companyId);

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
    defaultValues: EMPTY_FORM,
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
        departmentId: values.departmentId || null,
        managerId: values.managerId || null,
      }),
    onSuccess: () => {
      reset(EMPTY_FORM);
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

  // Employee had no Update command at all before this depth pass — only Create/Deactivate. Code,
  // DepartmentName, and HireDate stay immutable via this path (see UpdateEmployeeCommand's own
  // doc comment on the backend); only FullName/Email/DepartmentId/ManagerId are sent.
  const updateEmployee = useMutation({
    mutationFn: (values: FormValues & { id: string }) =>
      apiClient.put<EmployeeDto>(`/hrms/employees/${values.id}`, {
        companyId,
        fullName: values.fullName,
        email: values.email,
        departmentId: values.departmentId || null,
        managerId: values.managerId || null,
      }),
    onSuccess: () => {
      setEditingId(null);
      reset(EMPTY_FORM);
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

  const orgChartQuery = useQuery({
    queryKey: ['employee-org-chart', companyId, orgChartEmployeeId],
    queryFn: () => apiClient.get<EmployeeOrgChartDto>(`/hrms/employees/${orgChartEmployeeId}/org-chart?companyId=${companyId}`),
    enabled: Boolean(companyId && orgChartEmployeeId),
  });

  function startEdit(row: EmployeeDto) {
    setEditingId(row.id);
    reset({
      code: row.code,
      fullName: row.fullName,
      email: row.email,
      departmentName: row.departmentName ?? '',
      hireDate: row.hireDate,
      departmentId: row.departmentId ?? '',
      managerId: row.managerId ?? '',
    });
  }

  function cancelEdit() {
    setEditingId(null);
    reset(EMPTY_FORM);
  }

  // Reset back to create mode if the employee currently being edited disappears from the list
  // (e.g. a concurrent deactivate/search change) — avoids submitting an update for a stale row.
  useEffect(() => {
    if (editingId && employeesQuery.data && !employeesQuery.data.data.some((e) => e.id === editingId)) {
      cancelEdit();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [employeesQuery.data, editingId]);

  if (!companyId) {
    return <p className="text-text-muted">Set an active Company ID in the header above to manage employees.</p>;
  }

  const isEditing = Boolean(editingId);

  return (
    <div>
      <h1 className="mb-1 text-xl font-semibold text-text">Employees</h1>
      <p className="mb-4 text-sm text-text-muted">Employee records — HRMS, Phase 4.</p>

      <Card className="mb-6">
        <form noValidate
          onSubmit={handleSubmit((values) => (isEditing ? updateEmployee.mutate({ ...values, id: editingId! }) : createEmployee.mutate(values)))}
          className="grid grid-cols-1 gap-4 sm:grid-cols-3"
        >
          <label className="flex flex-col gap-1 text-sm">
            Code
            <Controller
              control={control}
              name="code"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5 disabled:opacity-60" placeholder="EMP-01" disabled={isEditing} {...field} />
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
            Department label (optional)
            <Controller
              control={control}
              name="departmentName"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5 disabled:opacity-60" placeholder="Engineering" disabled={isEditing} {...field} />
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
                <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5 disabled:opacity-60" disabled={isEditing} {...field} />
              )}
            />
            {errors.hireDate && <span className="text-xs text-danger">{errors.hireDate.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Department (Core)
            <Controller
              control={control}
              name="departmentId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={departmentOptions.options}
                  isLoading={departmentOptions.isLoading}
                  onSearchChange={departmentOptions.onSearchChange}
                  placeholder="Search departments…"
                />
              )}
            />
            {errors.departmentId && <span className="text-xs text-danger">{errors.departmentId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Manager (optional)
            <Controller
              control={control}
              name="managerId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={managerOptions.options.filter((o) => o.id !== editingId)}
                  isLoading={managerOptions.isLoading}
                  onSearchChange={managerOptions.onSearchChange}
                  placeholder="Search employees…"
                />
              )}
            />
            {errors.managerId && <span className="text-xs text-danger">{errors.managerId.message}</span>}
          </label>
          <div className="col-span-full flex items-center gap-2">
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? 'Saving…' : isEditing ? 'Save changes' : 'Add employee'}
            </Button>
            {isEditing && (
              <Button type="button" variant="secondary" onClick={cancelEdit}>
                Cancel
              </Button>
            )}
          </div>
        </form>
        {createEmployee.isError && createEmployee.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createEmployee.error.problem.title}</p>
        )}
        {updateEmployee.isError && updateEmployee.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{updateEmployee.error.problem.title}</p>
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
            {
              header: 'Manager',
              render: (row: EmployeeDto) => managerOptions.options.find((e) => e.id === row.managerId)?.label ?? (row.managerId ? row.managerId.slice(0, 8) : '—'),
            },
            { header: 'Status', render: (row: EmployeeDto) => (row.isActive ? 'Active' : 'Inactive') },
            {
              header: 'Actions',
              render: (row: EmployeeDto) => (
                <div className="flex items-center gap-2">
                  <Button type="button" variant="secondary" onClick={() => startEdit(row)}>
                    Edit
                  </Button>
                  <Button type="button" variant="secondary" onClick={() => setOrgChartEmployeeId(row.id)}>
                    Reports
                  </Button>
                  <Button
                    type="button"
                    variant="danger"
                    disabled={!row.isActive || deactivateEmployee.isPending}
                    onClick={() => deactivateEmployee.mutate(row.id)}
                  >
                    {row.isActive ? 'Deactivate' : 'Deactivated'}
                  </Button>
                </div>
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

      {orgChartEmployeeId && (
        <Card className="mt-4">
          <div className="mb-2 flex items-center justify-between">
            <h3 className="text-sm font-semibold text-text">
              Direct reports{orgChartQuery.data ? ` — ${orgChartQuery.data.employee.fullName}` : ''}
            </h3>
            <Button type="button" variant="secondary" onClick={() => setOrgChartEmployeeId(null)}>Close</Button>
          </div>
          {orgChartQuery.isLoading && <p className="text-sm text-text-muted">Loading…</p>}
          {orgChartQuery.isError && <p role="alert" className="text-sm text-danger">Could not load the org chart for that employee.</p>}
          {orgChartQuery.data && orgChartQuery.data.directReports.length === 0 && (
            <p className="text-sm text-text-muted">No direct reports — this is a single-level view (see EmployeeOrgChartDto's own doc comment).</p>
          )}
          {orgChartQuery.data && orgChartQuery.data.directReports.length > 0 && (
            <ul className="list-inside list-disc text-sm text-text">
              {orgChartQuery.data.directReports.map((report) => (
                <li key={report.id}>{report.fullName} ({report.code}){report.departmentName ? ` — ${report.departmentName}` : ''}</li>
              ))}
            </ul>
          )}
        </Card>
      )}

      <LeaveRequestsPanel />
      <LeaveBalancesPanel />
      <AttendancePanel />
      <PayrollPanel />
      <OnboardingPanel />
      <OffboardingPanel />
    </div>
  );
}
