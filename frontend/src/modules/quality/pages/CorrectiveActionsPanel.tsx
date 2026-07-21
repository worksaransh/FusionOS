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
import { useNonConformanceReportOptions, useUserOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  nonConformanceReportId: z.string().uuid('Pick the non-conformance report this plan addresses'),
  rootCauseDescription: z.string().min(1, 'Root cause description is required').max(2000),
  correctiveActionDescription: z.string().min(1, 'Corrective action description is required').max(2000),
  preventiveActionDescription: z.string().min(1, 'Preventive action description is required').max(2000),
  assignedToUserId: z.string().uuid('Assign this plan to a user'),
  dueDate: z.string().min(1, 'Due date is required'),
});
type FormValues = z.infer<typeof schema>;

interface CorrectiveActionDto {
  id: string;
  nonConformanceReportId: string;
  rootCauseDescription: string;
  correctiveActionDescription: string;
  preventiveActionDescription: string;
  assignedToUserId: string;
  dueDate: string;
  status: string;
  closedAt: string | null;
  verifiedAt: string | null;
}

/**
 * Corrective and Preventive Actions (CAPA) — root cause, the corrective fix, and the
 * preventive measure to stop recurrence, raised against a NonConformanceReport, assigned to
 * a user with a due date. Open -> InProgress -> Closed -> Verified. Rendered as a sibling
 * panel under InspectionsPage, below NonConformanceReportsPanel, same pattern as
 * MaintenanceRequestsPanel under AssetsPage.
 */
export function CorrectiveActionsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const ncrOptions = useNonConformanceReportOptions(companyId);
  const userOptions = useUserOptions(companyId);

  const correctiveActionsQuery = useQuery({
    queryKey: ['corrective-actions', companyId],
    queryFn: () => apiClient.get<PagedResult<CorrectiveActionDto>>(`/quality/corrective-actions?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      nonConformanceReportId: '', rootCauseDescription: '', correctiveActionDescription: '',
      preventiveActionDescription: '', assignedToUserId: '', dueDate: '',
    },
  });

  const createCorrectiveAction = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<CorrectiveActionDto>('/quality/corrective-actions', {
        companyId,
        nonConformanceReportId: values.nonConformanceReportId,
        rootCauseDescription: values.rootCauseDescription,
        correctiveActionDescription: values.correctiveActionDescription,
        preventiveActionDescription: values.preventiveActionDescription,
        assignedToUserId: values.assignedToUserId,
        dueDate: new Date(values.dueDate).toISOString(),
      }),
    onSuccess: () => {
      reset({
        nonConformanceReportId: '', rootCauseDescription: '', correctiveActionDescription: '',
        preventiveActionDescription: '', assignedToUserId: '', dueDate: '',
      });
      queryClient.invalidateQueries({ queryKey: ['corrective-actions', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const startCorrectiveAction = useMutation({
    mutationFn: (id: string) => apiClient.post<CorrectiveActionDto>(`/quality/corrective-actions/${id}/start`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['corrective-actions', companyId] }),
  });

  const closeCorrectiveAction = useMutation({
    mutationFn: (id: string) => apiClient.post<CorrectiveActionDto>(`/quality/corrective-actions/${id}/close`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['corrective-actions', companyId] }),
  });

  const verifyCorrectiveAction = useMutation({
    mutationFn: (id: string) => apiClient.post<CorrectiveActionDto>(`/quality/corrective-actions/${id}/verify`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['corrective-actions', companyId] }),
  });

  if (!companyId) return null;

  const isTransitioning = startCorrectiveAction.isPending || closeCorrectiveAction.isPending || verifyCorrectiveAction.isPending;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Corrective Actions (CAPA)</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createCorrectiveAction.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            Non-conformance report
            <Controller
              control={control}
              name="nonConformanceReportId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={ncrOptions.options}
                  isLoading={ncrOptions.isLoading}
                  placeholder="Search non-conformance reports…"
                />
              )}
            />
            {errors.nonConformanceReportId && <span className="text-xs text-danger">{errors.nonConformanceReportId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            Root cause
            <Controller
              control={control}
              name="rootCauseDescription"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Worn tooling" {...field} />
              )}
            />
            {errors.rootCauseDescription && <span className="text-xs text-danger">{errors.rootCauseDescription.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            Corrective action
            <Controller
              control={control}
              name="correctiveActionDescription"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Replace tooling" {...field} />
              )}
            />
            {errors.correctiveActionDescription && <span className="text-xs text-danger">{errors.correctiveActionDescription.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            Preventive action
            <Controller
              control={control}
              name="preventiveActionDescription"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Add tooling wear checklist" {...field} />
              )}
            />
            {errors.preventiveActionDescription && <span className="text-xs text-danger">{errors.preventiveActionDescription.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Assigned to
            <Controller
              control={control}
              name="assignedToUserId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={userOptions.options}
                  isLoading={userOptions.isLoading}
                  onSearchChange={userOptions.onSearchChange}
                  placeholder="Search users…"
                />
              )}
            />
            {errors.assignedToUserId && <span className="text-xs text-danger">{errors.assignedToUserId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Due date
            <Controller
              control={control}
              name="dueDate"
              render={({ field }) => (
                <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.dueDate && <span className="text-xs text-danger">{errors.dueDate.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Opening…' : 'Open CAPA plan'}</Button>
          </div>
        </form>
        {createCorrectiveAction.isError && createCorrectiveAction.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createCorrectiveAction.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Non-conformance report', render: (row: CorrectiveActionDto) => ncrOptions.options.find((r) => r.id === row.nonConformanceReportId)?.label ?? row.nonConformanceReportId },
            { header: 'Root cause', render: (row: CorrectiveActionDto) => row.rootCauseDescription },
            { header: 'Assigned to', render: (row: CorrectiveActionDto) => userOptions.options.find((u) => u.id === row.assignedToUserId)?.label ?? row.assignedToUserId },
            { header: 'Due', render: (row: CorrectiveActionDto) => new Date(row.dueDate).toLocaleDateString() },
            { header: 'Status', render: (row: CorrectiveActionDto) => row.status },
            {
              header: 'Actions',
              render: (row: CorrectiveActionDto) => (
                <div className="flex items-center gap-2">
                  {row.status === 'Open' && (
                    <Button type="button" variant="secondary" disabled={isTransitioning} onClick={() => startCorrectiveAction.mutate(row.id)}>
                      Start
                    </Button>
                  )}
                  {row.status === 'InProgress' && (
                    <Button type="button" disabled={isTransitioning} onClick={() => closeCorrectiveAction.mutate(row.id)}>
                      Close
                    </Button>
                  )}
                  {row.status === 'Closed' && (
                    <Button type="button" disabled={isTransitioning} onClick={() => verifyCorrectiveAction.mutate(row.id)}>
                      Verify
                    </Button>
                  )}
                </div>
              ),
            },
          ]}
          rows={correctiveActionsQuery.data?.data}
          isLoading={correctiveActionsQuery.isLoading}
          isError={correctiveActionsQuery.isError}
          errorMessage="Could not load corrective actions."
          emptyMessage="No corrective actions yet — open the first plan above."
          rowKey={(row) => row.id}
        />
      </Card>
      {(startCorrectiveAction.isError || closeCorrectiveAction.isError || verifyCorrectiveAction.isError) && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not update that corrective action.</p>
      )}
    </div>
  );
}
