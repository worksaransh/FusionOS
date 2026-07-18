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

const LEAVE_TYPES = ['Annual', 'Sick', 'Unpaid'] as const;

const schema = z.object({
  employeeId: z.string().uuid('Pick an employee'),
  type: z.enum(LEAVE_TYPES),
  startDate: z.string().min(1, 'Start date is required'),
  endDate: z.string().min(1, 'End date is required'),
  reason: z.string().max(1000).or(z.literal('')),
});
type FormValues = z.infer<typeof schema>;

interface LeaveRequestDto {
  id: string;
  employeeId: string;
  type: string;
  startDate: string;
  endDate: string;
  reason: string | null;
  status: string;
}

/**
 * Leave Requests — an employee's leave, Requested → Approved/Rejected.
 * Rendered as a sibling panel under EmployeesPage, same pattern as
 * MaintenanceRequestsPanel under AssetsPage. Approve/Reject are both single
 * clicks — unlike CRM's Win or Maintenance's Complete, neither transition
 * needs a further input from the approver.
 */
export function LeaveRequestsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const employeeOptions = useEmployeeOptions(companyId);

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { employeeId: '', type: 'Annual', startDate: '', endDate: '', reason: '' },
  });

  const requestsQuery = useQuery({
    queryKey: ['leave-requests', companyId],
    queryFn: () => apiClient.get<PagedResult<LeaveRequestDto>>(`/hrms/leave-requests?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const createRequest = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<LeaveRequestDto>('/hrms/leave-requests', {
        companyId,
        employeeId: values.employeeId,
        type: values.type,
        startDate: new Date(values.startDate).toISOString(),
        endDate: new Date(values.endDate).toISOString(),
        reason: values.reason || null,
      }),
    onSuccess: () => {
      reset({ employeeId: '', type: 'Annual', startDate: '', endDate: '', reason: '' });
      queryClient.invalidateQueries({ queryKey: ['leave-requests', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const approveRequest = useMutation({
    mutationFn: (id: string) => apiClient.post<LeaveRequestDto>(`/hrms/leave-requests/${id}/approve`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['leave-requests', companyId] }),
  });

  const rejectRequest = useMutation({
    mutationFn: (id: string) => apiClient.post<LeaveRequestDto>(`/hrms/leave-requests/${id}/reject`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['leave-requests', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Leave Requests</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createRequest.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
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
              name="type"
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
            Start date
            <Controller
              control={control}
              name="startDate"
              render={({ field }) => (
                <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.startDate && <span className="text-xs text-danger">{errors.startDate.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            End date
            <Controller
              control={control}
              name="endDate"
              render={({ field }) => (
                <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.endDate && <span className="text-xs text-danger">{errors.endDate.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            Reason (optional)
            <Controller
              control={control}
              name="reason"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Submitting…' : 'Submit request'}</Button>
          </div>
        </form>
        {createRequest.isError && createRequest.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createRequest.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Type', render: (row: LeaveRequestDto) => row.type },
            { header: 'Start', render: (row: LeaveRequestDto) => new Date(row.startDate).toLocaleDateString() },
            { header: 'End', render: (row: LeaveRequestDto) => new Date(row.endDate).toLocaleDateString() },
            { header: 'Reason', render: (row: LeaveRequestDto) => row.reason ?? '—' },
            { header: 'Status', render: (row: LeaveRequestDto) => row.status },
            {
              header: 'Actions',
              render: (row: LeaveRequestDto) =>
                row.status === 'Requested' ? (
                  <div className="flex items-center gap-2">
                    <Button type="button" variant="secondary" disabled={approveRequest.isPending} onClick={() => approveRequest.mutate(row.id)}>
                      Approve
                    </Button>
                    <Button type="button" variant="danger" disabled={rejectRequest.isPending} onClick={() => rejectRequest.mutate(row.id)}>
                      Reject
                    </Button>
                  </div>
                ) : null,
            },
          ]}
          rows={requestsQuery.data?.data}
          isLoading={requestsQuery.isLoading}
          isError={requestsQuery.isError}
          errorMessage="Could not load leave requests."
          emptyMessage="No leave requests yet — submit the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {(approveRequest.isError || rejectRequest.isError) && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not update that leave request.</p>
      )}
    </div>
  );
}
