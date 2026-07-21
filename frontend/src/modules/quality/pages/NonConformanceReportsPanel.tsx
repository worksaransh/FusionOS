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
import { useInspectionOptions, useUserOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const SEVERITIES = ['Minor', 'Major', 'Critical'] as const;

const schema = z.object({
  inspectionId: z.string().refine((v) => v === '' || /^[0-9a-fA-F-]{36}$/.test(v), 'Must be blank or a valid Inspection'),
  description: z.string().min(1, 'Description is required').max(2000),
  severity: z.enum(SEVERITIES),
  raisedByUserId: z.string().uuid('Pick who raised this'),
});
type FormValues = z.infer<typeof schema>;

interface NonConformanceReportDto {
  id: string;
  inspectionId: string | null;
  description: string;
  severity: string;
  status: string;
  raisedByUserId: string;
  raisedAt: string;
  closedAt: string | null;
}

/**
 * Non-Conformance Reports (NCR) — a defect or deviation raised either against a formal
 * Inspection (linked by id) or standalone, Open -> UnderReview -> Closed. Rendered as a
 * sibling panel under InspectionsPage, same pattern as MaintenanceRequestsPanel under
 * AssetsPage. CorrectiveActionsPanel below links its CAPA plans back to one of these.
 */
export function NonConformanceReportsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const inspectionOptions = useInspectionOptions(companyId);
  const userOptions = useUserOptions(companyId);

  const reportsQuery = useQuery({
    queryKey: ['non-conformance-reports', companyId],
    queryFn: () => apiClient.get<PagedResult<NonConformanceReportDto>>(`/quality/non-conformance-reports?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { inspectionId: '', description: '', severity: 'Minor', raisedByUserId: '' },
  });

  const createReport = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<NonConformanceReportDto>('/quality/non-conformance-reports', {
        companyId,
        inspectionId: values.inspectionId || null,
        description: values.description,
        severity: values.severity,
        raisedByUserId: values.raisedByUserId,
      }),
    onSuccess: () => {
      reset({ inspectionId: '', description: '', severity: 'Minor', raisedByUserId: '' });
      queryClient.invalidateQueries({ queryKey: ['non-conformance-reports', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const updateStatus = useMutation({
    mutationFn: ({ id, status }: { id: string; status: string }) =>
      apiClient.post<NonConformanceReportDto>(`/quality/non-conformance-reports/${id}/status`, { companyId, status }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['non-conformance-reports', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Non-Conformance Reports</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createReport.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Inspection (optional — leave blank for a standalone NCR)
            <Controller
              control={control}
              name="inspectionId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={inspectionOptions.options}
                  isLoading={inspectionOptions.isLoading}
                  placeholder="Search inspections…"
                />
              )}
            />
            {errors.inspectionId && <span className="text-xs text-danger">{errors.inspectionId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Severity
            <Controller
              control={control}
              name="severity"
              render={({ field }) => (
                <select className="rounded-md border border-border bg-surface px-2 py-1.5" {...field}>
                  {SEVERITIES.map((severity) => (
                    <option key={severity} value={severity}>{severity}</option>
                  ))}
                </select>
              )}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            Description
            <Controller
              control={control}
              name="description"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Bracket dimension out of tolerance" {...field} />
              )}
            />
            {errors.description && <span className="text-xs text-danger">{errors.description.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Raised by
            <Controller
              control={control}
              name="raisedByUserId"
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
            {errors.raisedByUserId && <span className="text-xs text-danger">{errors.raisedByUserId.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Raising…' : 'Raise NCR'}</Button>
          </div>
        </form>
        {createReport.isError && createReport.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createReport.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Description', render: (row: NonConformanceReportDto) => row.description },
            { header: 'Severity', render: (row: NonConformanceReportDto) => row.severity },
            { header: 'Status', render: (row: NonConformanceReportDto) => row.status },
            { header: 'Inspection', render: (row: NonConformanceReportDto) => (row.inspectionId ? inspectionOptions.options.find((i) => i.id === row.inspectionId)?.label ?? row.inspectionId : 'Standalone') },
            { header: 'Raised', render: (row: NonConformanceReportDto) => new Date(row.raisedAt).toLocaleDateString() },
            {
              header: 'Actions',
              render: (row: NonConformanceReportDto) => (
                <div className="flex items-center gap-2">
                  {row.status === 'Open' && (
                    <Button
                      type="button"
                      variant="secondary"
                      disabled={updateStatus.isPending}
                      onClick={() => updateStatus.mutate({ id: row.id, status: 'UnderReview' })}
                    >
                      Move to review
                    </Button>
                  )}
                  {row.status !== 'Closed' && (
                    <Button
                      type="button"
                      disabled={updateStatus.isPending}
                      onClick={() => updateStatus.mutate({ id: row.id, status: 'Closed' })}
                    >
                      Close
                    </Button>
                  )}
                </div>
              ),
            },
          ]}
          rows={reportsQuery.data?.data}
          isLoading={reportsQuery.isLoading}
          isError={reportsQuery.isError}
          errorMessage="Could not load non-conformance reports."
          emptyMessage="No non-conformance reports yet — raise the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {updateStatus.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not update that non-conformance report's status.</p>
      )}
    </div>
  );
}
