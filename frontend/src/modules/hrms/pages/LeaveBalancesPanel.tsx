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
import { useEmployeeOptions } from '../../../shared/api/entityOptions';

const LEAVE_TYPES = ['Annual', 'Sick', 'Unpaid'] as const;
const CURRENT_YEAR = new Date().getFullYear();

// year/entitledDays stay strings in the form schema (same convention as
// SupplierRatingsPanel's qualityScore/deliveryScore) — z.coerce.number() has
// a mismatched input/output type zodResolver can't reconcile with useForm's
// single type parameter. Converted to real numbers with Number(...) only at
// the mutation call site.
const schema = z.object({
  employeeId: z.string().uuid('Pick an employee'),
  leaveType: z.enum(LEAVE_TYPES),
  year: z.string().refine((v) => Number.isInteger(Number(v)) && Number(v) >= 2000, 'Year is not valid'),
  entitledDays: z.string().refine((v) => Number(v) >= 0 && !Number.isNaN(Number(v)), 'Entitled days cannot be negative'),
});
type FormValues = z.infer<typeof schema>;

interface LeaveBalanceDto {
  id: string;
  employeeId: string;
  leaveType: string;
  year: number;
  entitledDays: number;
  usedDays: number;
  remainingDays: number;
}

/**
 * Leave Balances — an employee's entitlement/accrual per LeaveType/Year
 * (closing the "approval never checks a balance" gap LeaveRequestsPanel's
 * approve/reject actions used to have). Rendered as a sibling panel under
 * EmployeesPage, same pattern as LeaveRequestsPanel/AttendancePanel/
 * PayrollPanel. Initializing a balance and adjusting an existing one share
 * one permission (hrms.leave-balance.adjust — see CreateLeaveBalanceCommand's
 * doc comment), and viewing is hrms.leave-balance.read.
 */
export function LeaveBalancesPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [viewEmployeeId, setViewEmployeeId] = useState('');
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editValue, setEditValue] = useState('');

  const employeeOptions = useEmployeeOptions(companyId);

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { employeeId: '', leaveType: 'Annual', year: String(CURRENT_YEAR), entitledDays: '0' },
  });

  const balancesQuery = useQuery({
    queryKey: ['leave-balances', companyId, viewEmployeeId],
    queryFn: () =>
      apiClient.get<LeaveBalanceDto[]>(`/hrms/leave-balances/by-employee/${viewEmployeeId}?companyId=${companyId}`),
    enabled: Boolean(companyId && viewEmployeeId),
  });

  const createBalance = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<LeaveBalanceDto>('/hrms/leave-balances', {
        companyId,
        employeeId: values.employeeId,
        leaveType: values.leaveType,
        year: Number(values.year),
        entitledDays: Number(values.entitledDays),
      }),
    onSuccess: (result) => {
      reset({ employeeId: result.employeeId, leaveType: 'Annual', year: String(CURRENT_YEAR), entitledDays: '0' });
      setViewEmployeeId(result.employeeId);
      queryClient.invalidateQueries({ queryKey: ['leave-balances', companyId, result.employeeId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const adjustBalance = useMutation({
    mutationFn: ({ id, newEntitledDays }: { id: string; newEntitledDays: number }) =>
      apiClient.post<LeaveBalanceDto>(`/hrms/leave-balances/${id}/adjust`, { companyId, newEntitledDays }),
    onSuccess: () => {
      setEditingId(null);
      queryClient.invalidateQueries({ queryKey: ['leave-balances', companyId, viewEmployeeId] });
    },
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Leave Balances</h2>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createBalance.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-4">
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
            Type
            <Controller
              control={control}
              name="leaveType"
              render={({ field }) => (
                <select className="rounded-md border border-border bg-surface px-2 py-1.5" {...field}>
                  {LEAVE_TYPES.map((type) => (
                    <option key={type} value={type}>{type}</option>
                  ))}
                </select>
              )}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Year
            <Controller
              control={control}
              name="year"
              render={({ field }) => (
                <input type="number" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.year && <span className="text-xs text-danger">{errors.year.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Entitled days
            <Controller
              control={control}
              name="entitledDays"
              render={({ field }) => (
                <input type="number" step="0.5" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.entitledDays && <span className="text-xs text-danger">{errors.entitledDays.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Initializing…' : 'Initialize balance'}</Button>
          </div>
        </form>
        {createBalance.isError && createBalance.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createBalance.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <label className="mb-3 flex flex-col gap-1 text-sm sm:w-72">
          View balances for
          <EntityCombobox
            value={viewEmployeeId}
            onChange={setViewEmployeeId}
            options={employeeOptions.options}
            isLoading={employeeOptions.isLoading}
            onSearchChange={employeeOptions.onSearchChange}
            placeholder="Search employees…"
          />
        </label>
        <DataTable
          columns={[
            { header: 'Type', render: (row: LeaveBalanceDto) => row.leaveType },
            { header: 'Year', render: (row: LeaveBalanceDto) => row.year },
            {
              header: 'Entitled',
              render: (row: LeaveBalanceDto) =>
                editingId === row.id ? (
                  <input
                    type="number"
                    step="0.5"
                    className="w-24 rounded-md border border-border bg-surface px-2 py-1"
                    value={editValue}
                    onChange={(e) => setEditValue(e.target.value)}
                  />
                ) : (
                  row.entitledDays
                ),
            },
            { header: 'Used', render: (row: LeaveBalanceDto) => row.usedDays },
            { header: 'Remaining', render: (row: LeaveBalanceDto) => row.remainingDays },
            {
              header: 'Actions',
              render: (row: LeaveBalanceDto) =>
                editingId === row.id ? (
                  <div className="flex items-center gap-2">
                    <Button
                      type="button"
                      disabled={adjustBalance.isPending}
                      onClick={() => adjustBalance.mutate({ id: row.id, newEntitledDays: Number(editValue) })}
                    >
                      Save
                    </Button>
                    <Button type="button" variant="secondary" onClick={() => setEditingId(null)}>
                      Cancel
                    </Button>
                  </div>
                ) : (
                  <Button
                    type="button"
                    variant="secondary"
                    onClick={() => {
                      setEditingId(row.id);
                      setEditValue(String(row.entitledDays));
                    }}
                  >
                    Adjust
                  </Button>
                ),
            },
          ]}
          rows={balancesQuery.data}
          isLoading={balancesQuery.isLoading}
          isError={balancesQuery.isError}
          errorMessage="Could not load leave balances."
          emptyMessage={viewEmployeeId ? 'No leave balances yet for this employee — initialize one above.' : 'Pick an employee to view their leave balances.'}
          rowKey={(row) => row.id}
        />
        {adjustBalance.isError && adjustBalance.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{adjustBalance.error.problem.title}</p>
        )}
      </Card>
    </div>
  );
}
