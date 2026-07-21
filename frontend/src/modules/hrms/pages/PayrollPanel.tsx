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
import { useEmployeeOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  employeeId: z.string().uuid('Pick an employee'),
  periodMonth: z.string().min(1, 'Month is required'),
  periodYear: z.string().min(1, 'Year is required'),
  baseSalary: z.string().min(1, 'Base salary is required'),
});
type FormValues = z.infer<typeof schema>;

interface PayrollRecordDto {
  id: string;
  employeeId: string;
  periodMonth: number;
  periodYear: number;
  baseSalary: number;
  grossPay: number;
  status: string;
  approvedAt: string | null;
  paidAt: string | null;
}

/**
 * Payroll — a deliberately minimal skeleton, Draft -> Approved -> Paid, one
 * record per employee per period (05_MODULE_ROADMAP.md's "Payroll" line
 * item). This is NOT a payroll engine: gross pay always equals base salary
 * — no allowances, deductions, or tax withholding are calculated anywhere in
 * this slice (see PayrollRecord.cs's own doc comment on the backend). The
 * callout below exists so this limitation is visible here too, not just in
 * backend code comments. Rendered as a sibling panel under EmployeesPage,
 * same pattern as LeaveRequestsPanel/AttendancePanel.
 */
export function PayrollPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const employeeOptions = useEmployeeOptions(companyId);

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { employeeId: '', periodMonth: '', periodYear: '', baseSalary: '' },
  });

  const recordsQuery = useQuery({
    queryKey: ['payroll-records', companyId],
    queryFn: () => apiClient.get<PagedResult<PayrollRecordDto>>(`/hrms/payroll-records?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const createDraft = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<PayrollRecordDto>('/hrms/payroll-records', {
        companyId,
        employeeId: values.employeeId,
        periodMonth: Number(values.periodMonth),
        periodYear: Number(values.periodYear),
        baseSalary: Number(values.baseSalary),
      }),
    onSuccess: () => {
      reset({ employeeId: '', periodMonth: '', periodYear: '', baseSalary: '' });
      queryClient.invalidateQueries({ queryKey: ['payroll-records', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const approveRecord = useMutation({
    mutationFn: (id: string) => apiClient.post<PayrollRecordDto>(`/hrms/payroll-records/${id}/approve`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['payroll-records', companyId] }),
  });

  const markPaidRecord = useMutation({
    mutationFn: (id: string) => apiClient.post<PayrollRecordDto>(`/hrms/payroll-records/${id}/mark-paid`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['payroll-records', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-1 text-lg font-semibold text-text">Payroll</h2>
      <p className="mb-3 text-sm text-text-muted">
        Payroll skeleton only — gross pay always equals base salary. No allowances, deductions, or tax
        withholding are calculated in this slice.
      </p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createDraft.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-4">
          <label className="flex flex-col gap-1 text-sm">
            Employee
            <Controller
              control={control}
              name="employeeId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={employeeOptions.options}
                  isLoading={employeeOptions.isLoading}
                  onSearchChange={employeeOptions.onSearchChange}
                  placeholder="Search employees…"
                />
              )}
            />
            {errors.employeeId && <span className="text-xs text-danger">{errors.employeeId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Period month (1–12)
            <Controller
              control={control}
              name="periodMonth"
              render={({ field }) => (
                <input type="number" min={1} max={12} className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.periodMonth && <span className="text-xs text-danger">{errors.periodMonth.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Period year
            <Controller
              control={control}
              name="periodYear"
              render={({ field }) => (
                <input type="number" min={2000} className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.periodYear && <span className="text-xs text-danger">{errors.periodYear.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Base salary
            <Controller
              control={control}
              name="baseSalary"
              render={({ field }) => (
                <input type="number" min={0} step="0.01" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.baseSalary && <span className="text-xs text-danger">{errors.baseSalary.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create draft'}</Button>
          </div>
        </form>
        {createDraft.isError && createDraft.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createDraft.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Employee', render: (row: PayrollRecordDto) => employeeOptions.options.find((e) => e.id === row.employeeId)?.label ?? row.employeeId },
            { header: 'Period', render: (row: PayrollRecordDto) => `${row.periodMonth}/${row.periodYear}` },
            { header: 'Base salary', render: (row: PayrollRecordDto) => row.baseSalary.toLocaleString() },
            { header: 'Gross pay', render: (row: PayrollRecordDto) => row.grossPay.toLocaleString() },
            { header: 'Status', render: (row: PayrollRecordDto) => row.status },
            {
              header: 'Actions',
              render: (row: PayrollRecordDto) => (
                <div className="flex items-center gap-2">
                  {row.status === 'Draft' && (
                    <Button type="button" variant="secondary" disabled={approveRecord.isPending} onClick={() => approveRecord.mutate(row.id)}>
                      Approve
                    </Button>
                  )}
                  {row.status === 'Approved' && (
                    <Button type="button" variant="secondary" disabled={markPaidRecord.isPending} onClick={() => markPaidRecord.mutate(row.id)}>
                      Mark paid
                    </Button>
                  )}
                </div>
              ),
            },
          ]}
          rows={recordsQuery.data?.data}
          isLoading={recordsQuery.isLoading}
          isError={recordsQuery.isError}
          errorMessage="Could not load payroll records."
          emptyMessage="No payroll records yet — create the first draft above."
          rowKey={(row) => row.id}
        />
      </Card>
      {(approveRecord.isError || markPaidRecord.isError) && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not update that payroll record.</p>
      )}
    </div>
  );
}
