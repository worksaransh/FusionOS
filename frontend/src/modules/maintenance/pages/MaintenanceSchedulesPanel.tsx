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
import { useAssetOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const FREQUENCIES = ['Weekly', 'Monthly', 'Quarterly', 'Annual'] as const;
const DUE_FILTERS = ['All', 'DueSoon', 'Overdue'] as const;
type DueFilter = (typeof DUE_FILTERS)[number];

const schema = z.object({
  assetId: z.string().uuid('Pick an asset'),
  frequency: z.enum(FREQUENCIES),
  description: z.string().min(1, 'Description is required').max(1000),
  nextDueDate: z.string().min(1, 'Next due date is required'),
});
type FormValues = z.infer<typeof schema>;

const editSchema = schema.omit({ assetId: true });
type EditFormValues = z.infer<typeof editSchema>;

interface MaintenanceScheduleDto {
  id: string;
  assetId: string;
  description: string;
  frequency: string;
  nextDueDate: string;
  isActive: boolean;
  createdAt: string;
}

/**
 * Maintenance Schedules — preventive-maintenance recurrence plans against an
 * Asset ("every Quarter, next due on this date"), distinct from
 * MaintenanceRequest, which is a single already-reported unit of work.
 * Rendered as a sibling panel under AssetsPage, same stacking convention as
 * MaintenanceRequestsPanel / Finance's many panels under AccountsPage. The
 * due-filter toggle backs the "due soon"/"overdue" views the backend query
 * supports (MaintenanceSchedulesController.List's `dueFilter` param).
 */
export function MaintenanceSchedulesPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [dueFilter, setDueFilter] = useState<DueFilter>('All');
  const [editingScheduleId, setEditingScheduleId] = useState<string | null>(null);

  const assetOptions = useAssetOptions(companyId);

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { assetId: '', frequency: 'Monthly', description: '', nextDueDate: '' },
  });

  const schedulesQuery = useQuery({
    queryKey: ['maintenance-schedules', companyId, dueFilter],
    queryFn: () => {
      const params = new URLSearchParams({ companyId, page: '1', pageSize: '50' });
      if (dueFilter !== 'All') params.set('dueFilter', dueFilter);
      return apiClient.get<PagedResult<MaintenanceScheduleDto>>(`/maintenance/schedules?${params.toString()}`);
    },
    enabled: Boolean(companyId),
  });

  const createSchedule = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<MaintenanceScheduleDto>('/maintenance/schedules', {
        companyId,
        assetId: values.assetId,
        frequency: values.frequency,
        description: values.description,
        nextDueDate: new Date(values.nextDueDate).toISOString(),
      }),
    onSuccess: () => {
      reset({ assetId: '', frequency: 'Monthly', description: '', nextDueDate: '' });
      queryClient.invalidateQueries({ queryKey: ['maintenance-schedules', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  // Soft-deactivate only — MaintenanceSchedulesController exposes this as a dedicated
  // POST .../{id}/deactivate action, same convention as AssetsController.
  const deactivateSchedule = useMutation({
    mutationFn: (scheduleId: string) => apiClient.post<MaintenanceScheduleDto>(`/maintenance/schedules/${scheduleId}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['maintenance-schedules', companyId] }),
  });

  if (!companyId) return null;

  const editingSchedule = schedulesQuery.data?.data.find((s) => s.id === editingScheduleId) ?? null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Maintenance Schedules</h2>
      <p className="mb-3 text-sm text-text-muted">Preventive maintenance recurrence plans against an asset.</p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createSchedule.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-4">
          <label className="flex flex-col gap-1 text-sm">
            Asset
            <Controller
              control={control}
              name="assetId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={assetOptions.options}
                  isLoading={assetOptions.isLoading}
                  onSearchChange={assetOptions.onSearchChange}
                  placeholder="Search assets…"
                />
              )}
            />
            {errors.assetId && <span className="text-xs text-danger">{errors.assetId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Frequency
            <Controller
              control={control}
              name="frequency"
              render={({ field }) => (
                <select className="rounded-md border border-border bg-surface px-2 py-1.5" {...field}>
                  {FREQUENCIES.map((frequency) => (
                    <option key={frequency} value={frequency}>{frequency}</option>
                  ))}
                </select>
              )}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Description
            <Controller
              control={control}
              name="description"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Quarterly oil change" {...field} />
              )}
            />
            {errors.description && <span className="text-xs text-danger">{errors.description.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Next due date
            <Controller
              control={control}
              name="nextDueDate"
              render={({ field }) => (
                <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.nextDueDate && <span className="text-xs text-danger">{errors.nextDueDate.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create schedule'}</Button>
          </div>
        </form>
        {createSchedule.isError && createSchedule.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createSchedule.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <div className="mb-3 flex gap-2">
          {DUE_FILTERS.map((filter) => (
            <Button
              key={filter}
              type="button"
              variant={dueFilter === filter ? 'primary' : 'secondary'}
              onClick={() => setDueFilter(filter)}
            >
              {filter === 'DueSoon' ? 'Due soon' : filter}
            </Button>
          ))}
        </div>
        <DataTable
          columns={[
            { header: 'Asset', render: (row: MaintenanceScheduleDto) => assetOptions.options.find((a) => a.id === row.assetId)?.label ?? row.assetId },
            { header: 'Description', render: (row: MaintenanceScheduleDto) => row.description },
            { header: 'Frequency', render: (row: MaintenanceScheduleDto) => row.frequency },
            { header: 'Next due', render: (row: MaintenanceScheduleDto) => new Date(row.nextDueDate).toLocaleDateString() },
            { header: 'Status', render: (row: MaintenanceScheduleDto) => (row.isActive ? 'Active' : 'Inactive') },
            {
              header: 'Actions',
              render: (row: MaintenanceScheduleDto) => (
                <div className="flex items-center gap-2">
                  <Button type="button" variant="secondary" onClick={() => setEditingScheduleId(row.id)}>
                    Edit
                  </Button>
                  <Button
                    type="button"
                    variant="danger"
                    disabled={!row.isActive || deactivateSchedule.isPending}
                    onClick={() => deactivateSchedule.mutate(row.id)}
                  >
                    {row.isActive ? 'Deactivate' : 'Deactivated'}
                  </Button>
                </div>
              ),
            },
          ]}
          rows={schedulesQuery.data?.data}
          isLoading={schedulesQuery.isLoading}
          isError={schedulesQuery.isError}
          errorMessage="Could not load maintenance schedules."
          emptyMessage="No maintenance schedules yet — create the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {deactivateSchedule.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that schedule.</p>
      )}

      {editingSchedule && (
        <MaintenanceScheduleEditPanel
          companyId={companyId}
          schedule={editingSchedule}
          onClose={() => setEditingScheduleId(null)}
        />
      )}
    </div>
  );
}

interface MaintenanceScheduleEditPanelProps {
  companyId: string;
  schedule: MaintenanceScheduleDto;
  onClose: () => void;
}

function MaintenanceScheduleEditPanel({ companyId, schedule, onClose }: MaintenanceScheduleEditPanelProps) {
  const queryClient = useQueryClient();

  const { control, handleSubmit, formState: { errors, isSubmitting } } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    values: {
      frequency: schedule.frequency as (typeof FREQUENCIES)[number],
      description: schedule.description,
      nextDueDate: schedule.nextDueDate.slice(0, 10),
    },
  });

  const updateSchedule = useMutation({
    mutationFn: (values: EditFormValues) =>
      apiClient.put<MaintenanceScheduleDto>(`/maintenance/schedules/${schedule.id}`, {
        companyId,
        frequency: values.frequency,
        description: values.description,
        nextDueDate: new Date(values.nextDueDate).toISOString(),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['maintenance-schedules', companyId] });
      onClose();
    },
  });

  return (
    <Card className="mt-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-base font-semibold text-text">Edit schedule — {schedule.description}</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form onSubmit={handleSubmit((values) => updateSchedule.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <label className="flex flex-col gap-1 text-sm">
          Frequency
          <Controller
            control={control}
            name="frequency"
            render={({ field }) => (
              <select className="rounded-md border border-border bg-surface px-2 py-1.5" {...field}>
                {FREQUENCIES.map((frequency) => (
                  <option key={frequency} value={frequency}>{frequency}</option>
                ))}
              </select>
            )}
          />
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Description
          <Controller
            control={control}
            name="description"
            render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )}
          />
          {errors.description && <span className="text-xs text-danger">{errors.description.message}</span>}
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Next due date
          <Controller
            control={control}
            name="nextDueDate"
            render={({ field }) => (
              <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )}
          />
          {errors.nextDueDate && <span className="text-xs text-danger">{errors.nextDueDate.message}</span>}
        </label>
        <div className="col-span-full flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save changes'}</Button>
          {updateSchedule.isError && (
            <span role="alert" className="text-sm text-danger">Could not save that schedule.</span>
          )}
        </div>
      </form>
    </Card>
  );
}
