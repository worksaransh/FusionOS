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
import { useEmployeeOptions, useLeaveRequestOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const ATTENDANCE_STATUSES = ['Present', 'Absent', 'HalfDay', 'OnLeave'] as const;

const schema = z.object({
  employeeId: z.string().uuid('Pick an employee'),
  date: z.string().min(1, 'Date is required'),
  checkInTime: z.string().or(z.literal('')),
  checkOutTime: z.string().or(z.literal('')),
  status: z.enum(ATTENDANCE_STATUSES),
  leaveRequestId: z.string().refine((v) => v === '' || /^[0-9a-fA-F-]{36}$/.test(v), 'Must be blank or a valid leave request'),
});
type FormValues = z.infer<typeof schema>;

const editSchema = z.object({
  checkInTime: z.string().or(z.literal('')),
  checkOutTime: z.string().or(z.literal('')),
  status: z.enum(ATTENDANCE_STATUSES),
  leaveRequestId: z.string().refine((v) => v === '' || /^[0-9a-fA-F-]{36}$/.test(v), 'Must be blank or a valid leave request'),
});
type EditFormValues = z.infer<typeof editSchema>;

interface AttendanceRecordDto {
  id: string;
  employeeId: string;
  date: string;
  checkInTime: string | null;
  checkOutTime: string | null;
  status: string;
  leaveRequestId: string | null;
}

/** ISO datetime -> the "YYYY-MM-DDTHH:mm" shape a `type="datetime-local"` input expects, or '' when absent. */
function toDatetimeLocalValue(iso: string | null): string {
  if (!iso) return '';
  const date = new Date(iso);
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

/**
 * Attendance — an employee's attendance for a single calendar date
 * (05_MODULE_ROADMAP.md's "Attendance" line item). Rendered as a sibling
 * panel under EmployeesPage, same pattern as LeaveRequestsPanel. The optional
 * "linked leave request" picker is scoped to the employee already chosen in
 * this same form, same scoping idea as Warehouse's Zone/Bin pickers being
 * scoped to the warehouse/zone chosen above them.
 */
export function AttendancePanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [editingRecordId, setEditingRecordId] = useState<string | null>(null);

  const employeeOptions = useEmployeeOptions(companyId);

  const { control, handleSubmit, reset, watch, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { employeeId: '', date: '', checkInTime: '', checkOutTime: '', status: 'Present', leaveRequestId: '' },
  });
  const watchedEmployeeId = watch('employeeId');
  const leaveRequestOptions = useLeaveRequestOptions(companyId, watchedEmployeeId || undefined);

  const recordsQuery = useQuery({
    queryKey: ['attendance-records', companyId],
    queryFn: () => apiClient.get<PagedResult<AttendanceRecordDto>>(`/hrms/attendance-records?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const recordAttendance = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<AttendanceRecordDto>('/hrms/attendance-records', {
        companyId,
        employeeId: values.employeeId,
        date: new Date(values.date).toISOString(),
        checkInTime: values.checkInTime ? new Date(values.checkInTime).toISOString() : null,
        checkOutTime: values.checkOutTime ? new Date(values.checkOutTime).toISOString() : null,
        status: values.status,
        leaveRequestId: values.leaveRequestId || null,
      }),
    onSuccess: () => {
      reset({ employeeId: '', date: '', checkInTime: '', checkOutTime: '', status: 'Present', leaveRequestId: '' });
      queryClient.invalidateQueries({ queryKey: ['attendance-records', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  if (!companyId) return null;

  const editingRecord = recordsQuery.data?.data.find((r) => r.id === editingRecordId) ?? null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Attendance</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => recordAttendance.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
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
            Date
            <Controller
              control={control}
              name="date"
              render={({ field }) => (
                <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.date && <span className="text-xs text-danger">{errors.date.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Status
            <Controller
              control={control}
              name="status"
              render={({ field }) => (
                <select className="rounded-md border border-border bg-surface px-2 py-1.5" {...field}>
                  {ATTENDANCE_STATUSES.map((status) => (
                    <option key={status} value={status}>{status}</option>
                  ))}
                </select>
              )}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Check-in (optional)
            <Controller
              control={control}
              name="checkInTime"
              render={({ field }) => (
                <input type="datetime-local" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Check-out (optional)
            <Controller
              control={control}
              name="checkOutTime"
              render={({ field }) => (
                <input type="datetime-local" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.checkOutTime && <span className="text-xs text-danger">{errors.checkOutTime.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Linked leave request (optional)
            <Controller
              control={control}
              name="leaveRequestId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={leaveRequestOptions.options}
                  isLoading={leaveRequestOptions.isLoading}
                  placeholder={watchedEmployeeId ? 'Search leave requests…' : 'Pick an employee first'}
                  disabled={!watchedEmployeeId}
                />
              )}
            />
            {errors.leaveRequestId && <span className="text-xs text-danger">{errors.leaveRequestId.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Recording…' : 'Record attendance'}</Button>
          </div>
        </form>
        {recordAttendance.isError && recordAttendance.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{recordAttendance.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Employee', render: (row: AttendanceRecordDto) => employeeOptions.options.find((e) => e.id === row.employeeId)?.label ?? row.employeeId },
            { header: 'Date', render: (row: AttendanceRecordDto) => new Date(row.date).toLocaleDateString() },
            { header: 'Check-in', render: (row: AttendanceRecordDto) => (row.checkInTime ? new Date(row.checkInTime).toLocaleTimeString() : '—') },
            { header: 'Check-out', render: (row: AttendanceRecordDto) => (row.checkOutTime ? new Date(row.checkOutTime).toLocaleTimeString() : '—') },
            { header: 'Status', render: (row: AttendanceRecordDto) => row.status },
            {
              header: 'Actions',
              render: (row: AttendanceRecordDto) => (
                <Button type="button" variant="secondary" onClick={() => setEditingRecordId(row.id)}>
                  Edit
                </Button>
              ),
            },
          ]}
          rows={recordsQuery.data?.data}
          isLoading={recordsQuery.isLoading}
          isError={recordsQuery.isError}
          errorMessage="Could not load attendance records."
          emptyMessage="No attendance recorded yet — record the first one above."
          rowKey={(row) => row.id}
        />
      </Card>

      {editingRecord && (
        <AttendanceEditPanel
          companyId={companyId}
          record={editingRecord}
          onClose={() => setEditingRecordId(null)}
        />
      )}
    </div>
  );
}

interface AttendanceEditPanelProps {
  companyId: string;
  record: AttendanceRecordDto;
  onClose: () => void;
}

function AttendanceEditPanel({ companyId, record, onClose }: AttendanceEditPanelProps) {
  const queryClient = useQueryClient();
  const leaveRequestOptions = useLeaveRequestOptions(companyId, record.employeeId);

  const { control, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    values: {
      checkInTime: toDatetimeLocalValue(record.checkInTime),
      checkOutTime: toDatetimeLocalValue(record.checkOutTime),
      status: record.status as (typeof ATTENDANCE_STATUSES)[number],
      leaveRequestId: record.leaveRequestId ?? '',
    },
  });

  const updateRecord = useMutation({
    mutationFn: (values: EditFormValues) =>
      apiClient.put<AttendanceRecordDto>(`/hrms/attendance-records/${record.id}`, {
        companyId,
        checkInTime: values.checkInTime ? new Date(values.checkInTime).toISOString() : null,
        checkOutTime: values.checkOutTime ? new Date(values.checkOutTime).toISOString() : null,
        status: values.status,
        leaveRequestId: values.leaveRequestId || null,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['attendance-records', companyId] });
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
    <Card className="mt-6">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-base font-semibold text-text">Edit attendance — {new Date(record.date).toLocaleDateString()}</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form onSubmit={handleSubmit((values) => updateRecord.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <label className="flex flex-col gap-1 text-sm">
          Status
          <Controller
            control={control}
            name="status"
            render={({ field }) => (
              <select className="rounded-md border border-border bg-surface px-2 py-1.5" {...field}>
                {ATTENDANCE_STATUSES.map((status) => (
                  <option key={status} value={status}>{status}</option>
                ))}
              </select>
            )}
          />
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Check-in
          <Controller
            control={control}
            name="checkInTime"
            render={({ field }) => (
              <input type="datetime-local" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )}
          />
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Check-out
          <Controller
            control={control}
            name="checkOutTime"
            render={({ field }) => (
              <input type="datetime-local" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )}
          />
          {errors.checkOutTime && <span className="text-xs text-danger">{errors.checkOutTime.message}</span>}
        </label>
        <label className="flex flex-col gap-1 text-sm sm:col-span-2">
          Linked leave request (optional)
          <Controller
            control={control}
            name="leaveRequestId"
            render={({ field }) => (
              <EntityCombobox
                value={field.value}
                onChange={field.onChange}
                options={leaveRequestOptions.options}
                isLoading={leaveRequestOptions.isLoading}
                placeholder="Search leave requests…"
              />
            )}
          />
          {errors.leaveRequestId && <span className="text-xs text-danger">{errors.leaveRequestId.message}</span>}
        </label>
        <div className="col-span-full flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save changes'}</Button>
          {updateRecord.isError && (
            <span role="alert" className="text-sm text-danger">Could not save that attendance record.</span>
          )}
        </div>
      </form>
    </Card>
  );
}
