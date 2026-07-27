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
import type { PagedResult } from '../../../shared/api/types';

const AUDIT_TYPES = ['Internal', 'External'] as const;
const SEVERITIES = ['Minor', 'Major', 'Critical'] as const;

const schema = z.object({
  auditType: z.enum(AUDIT_TYPES),
  scheduledDate: z.string().min(1, 'Scheduled date is required'),
  auditor: z.string().min(1, 'Auditor is required').max(200),
  scope: z.string().min(1, 'Scope is required').max(2000),
});
type FormValues = z.infer<typeof schema>;

const findingSchema = z.object({
  description: z.string().min(1, 'Finding description is required').max(2000),
  severity: z.enum(SEVERITIES),
});
type FindingFormValues = z.infer<typeof findingSchema>;

interface AuditFindingDto {
  id: string;
  description: string;
  severity: string;
  isResolved: boolean;
}

interface QualityAuditDto {
  id: string;
  auditType: string;
  scheduledDate: string;
  auditor: string;
  scope: string;
  status: string;
  findings: AuditFindingDto[];
}

/**
 * Quality Audits — scheduled internal/external audits with a free-text scope, moving
 * forward-only Scheduled -> InProgress -> Completed, with findings recorded (and
 * individually resolvable) along the way. Rendered as a sibling panel under
 * InspectionsPage, same pattern as CorrectiveActionsPanel/NonConformanceReportsPanel.
 */
export function QualityAuditsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [findingAuditId, setFindingAuditId] = useState<string | null>(null);

  const auditsQuery = useQuery({
    queryKey: ['quality-audits', companyId],
    queryFn: () => apiClient.get<PagedResult<QualityAuditDto>>(`/quality/audits?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { auditType: 'Internal', scheduledDate: '', auditor: '', scope: '' },
  });

  const createAudit = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<QualityAuditDto>('/quality/audits', {
        companyId,
        auditType: values.auditType,
        scheduledDate: new Date(values.scheduledDate).toISOString(),
        auditor: values.auditor,
        scope: values.scope,
      }),
    onSuccess: () => {
      reset({ auditType: 'Internal', scheduledDate: '', auditor: '', scope: '' });
      queryClient.invalidateQueries({ queryKey: ['quality-audits', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const startAudit = useMutation({
    mutationFn: (id: string) => apiClient.post<QualityAuditDto>(`/quality/audits/${id}/start`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['quality-audits', companyId] }),
  });

  const completeAudit = useMutation({
    mutationFn: (id: string) => apiClient.post<QualityAuditDto>(`/quality/audits/${id}/complete`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['quality-audits', companyId] }),
  });

  const resolveFinding = useMutation({
    mutationFn: ({ auditId, findingId }: { auditId: string; findingId: string }) =>
      apiClient.post<QualityAuditDto>(`/quality/audits/${auditId}/findings/${findingId}/resolve`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['quality-audits', companyId] }),
  });

  if (!companyId) return null;

  const isTransitioning = startAudit.isPending || completeAudit.isPending;
  const findingAudit = auditsQuery.data?.data.find((a) => a.id === findingAuditId) ?? null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Quality Audits</h2>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createAudit.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Audit type
            <Controller
              control={control}
              name="auditType"
              render={({ field }) => (
                <select className="rounded-md border border-border bg-surface px-2 py-1.5" {...field}>
                  {AUDIT_TYPES.map((type) => (
                    <option key={type} value={type}>{type}</option>
                  ))}
                </select>
              )}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Scheduled date
            <Controller
              control={control}
              name="scheduledDate"
              render={({ field }) => (
                <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.scheduledDate && <span className="text-xs text-danger">{errors.scheduledDate.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Auditor
            <Controller
              control={control}
              name="auditor"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Auditor name" {...field} />
              )}
            />
            {errors.auditor && <span className="text-xs text-danger">{errors.auditor.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            Scope
            <Controller
              control={control}
              name="scope"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="What is being audited" {...field} />
              )}
            />
            {errors.scope && <span className="text-xs text-danger">{errors.scope.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Scheduling…' : 'Schedule audit'}</Button>
          </div>
        </form>
        {createAudit.isError && createAudit.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createAudit.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Type', render: (row: QualityAuditDto) => row.auditType },
            { header: 'Scheduled', render: (row: QualityAuditDto) => new Date(row.scheduledDate).toLocaleDateString() },
            { header: 'Auditor', render: (row: QualityAuditDto) => row.auditor },
            { header: 'Scope', render: (row: QualityAuditDto) => row.scope },
            { header: 'Findings', render: (row: QualityAuditDto) => `${row.findings.filter((f) => f.isResolved).length}/${row.findings.length} resolved` },
            { header: 'Status', render: (row: QualityAuditDto) => row.status },
            {
              header: 'Actions',
              render: (row: QualityAuditDto) => (
                <div className="flex items-center gap-2">
                  {row.status === 'Scheduled' && (
                    <Button type="button" variant="secondary" disabled={isTransitioning} onClick={() => startAudit.mutate(row.id)}>
                      Start
                    </Button>
                  )}
                  {row.status === 'InProgress' && (
                    <Button type="button" disabled={isTransitioning} onClick={() => completeAudit.mutate(row.id)}>
                      Complete
                    </Button>
                  )}
                  {row.status !== 'Completed' && (
                    <Button type="button" variant="secondary" onClick={() => setFindingAuditId(row.id)}>
                      Add finding
                    </Button>
                  )}
                </div>
              ),
            },
          ]}
          rows={auditsQuery.data?.data}
          isLoading={auditsQuery.isLoading}
          isError={auditsQuery.isError}
          errorMessage="Could not load quality audits."
          emptyMessage="No quality audits yet — schedule the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {(startAudit.isError || completeAudit.isError || resolveFinding.isError) && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not update that quality audit.</p>
      )}

      {findingAudit && (
        <RecordFindingPanel
          companyId={companyId}
          audit={findingAudit}
          onResolve={(findingId) => resolveFinding.mutate({ auditId: findingAudit.id, findingId })}
          onClose={() => setFindingAuditId(null)}
        />
      )}
    </div>
  );
}

interface RecordFindingPanelProps {
  companyId: string;
  audit: QualityAuditDto;
  onResolve: (findingId: string) => void;
  onClose: () => void;
}

function RecordFindingPanel({ companyId, audit, onResolve, onClose }: RecordFindingPanelProps) {
  const queryClient = useQueryClient();

  const { control, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FindingFormValues>({
    resolver: zodResolver(findingSchema),
    defaultValues: { description: '', severity: 'Minor' },
  });

  const recordFinding = useMutation({
    mutationFn: (values: FindingFormValues) =>
      apiClient.post(`/quality/audits/${audit.id}/findings`, { companyId, description: values.description, severity: values.severity }),
    onSuccess: () => {
      reset({ description: '', severity: 'Minor' });
      queryClient.invalidateQueries({ queryKey: ['quality-audits', companyId] });
    },
  });

  return (
    <Card className="mt-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-base font-semibold text-text">Findings — {audit.auditType} audit</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>

      <ul className="mb-4 flex flex-col gap-2">
        {audit.findings.map((finding) => (
          <li key={finding.id} className="flex items-center justify-between rounded-md border border-border px-3 py-2 text-sm">
            <span>{finding.severity} — {finding.description} {finding.isResolved && <em className="text-text-muted">(resolved)</em>}</span>
            {!finding.isResolved && (
              <Button type="button" variant="secondary" onClick={() => onResolve(finding.id)}>Resolve</Button>
            )}
          </li>
        ))}
        {audit.findings.length === 0 && <li className="text-sm text-text-muted">No findings recorded yet.</li>}
      </ul>

      <form noValidate onSubmit={handleSubmit((values) => recordFinding.mutate(values))} className="flex flex-col gap-3 sm:flex-row sm:items-end">
        <label className="flex flex-1 flex-col gap-1 text-sm">
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
          Severity
          <Controller
            control={control}
            name="severity"
            render={({ field }) => (
              <select className="rounded-md border border-border bg-surface px-2 py-1.5" {...field}>
                {SEVERITIES.map((s) => (
                  <option key={s} value={s}>{s}</option>
                ))}
              </select>
            )}
          />
        </label>
        <Button type="submit" disabled={isSubmitting} className="w-fit">{isSubmitting ? 'Recording…' : 'Record finding'}</Button>
      </form>
    </Card>
  );
}
