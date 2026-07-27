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

const createJobSchema = z.object({
  sourceSystem: z.string().min(1, 'Source system is required').max(100),
});
type CreateJobFormValues = z.infer<typeof createJobSchema>;

const extractSchema = z.object({
  tallyXmlContent: z.string().min(1, 'Paste the Tally XML export content'),
});
type ExtractFormValues = z.infer<typeof extractSchema>;

interface MigrationJobDto {
  id: string;
  sourceSystem: string;
  status: string;
  totalRecordsExtracted: number;
  totalRecordsLoaded: number;
  totalRecordsFailed: number;
  reconciliationNote: string | null;
  createdAt: string;
}

interface MigrationStagingRecordDto {
  id: string;
  migrationJobId: string;
  recordType: string;
  rawDataJson: string;
  mappingStatus: string;
  validationErrors: string | null;
  targetEntityType: string | null;
  targetEntityId: string | null;
  createdAt: string;
}

/**
 * Migration Jobs — the legacy-system migration pipeline (11_SAP_MIGRATION.md §2):
 * Create a job, paste a Tally XML export and Extract it into staging records, then
 * Validate → Load → Reconcile in sequence. Rendered as a sibling panel under
 * IntegrationConnectorsPage, same stacking pattern as ConnectorConnectionsPanel.
 * Distinct from that panel's connector-sync bookkeeping — this is a one-time (or
 * few-times, during a parallel-run period) bulk import from a system being retired,
 * not ongoing sync of a live connected system.
 */
export function MigrationJobsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [selectedJobId, setSelectedJobId] = useState<string | null>(null);
  const [stagingStatusFilter, setStagingStatusFilter] = useState<string>('');

  const jobsQuery = useQuery({
    queryKey: ['migration-jobs', companyId],
    queryFn: () => apiClient.get<PagedResult<MigrationJobDto>>(`/integration-hub/migrations?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const selectedJob = jobsQuery.data?.data.find((j) => j.id === selectedJobId) ?? null;

  const stagingRecordsQuery = useQuery({
    queryKey: ['migration-staging-records', companyId, selectedJobId, stagingStatusFilter],
    queryFn: () => {
      const params = new URLSearchParams({ companyId: companyId!, page: '1', pageSize: '100' });
      if (stagingStatusFilter) params.set('mappingStatus', stagingStatusFilter);
      return apiClient.get<PagedResult<MigrationStagingRecordDto>>(`/integration-hub/migrations/${selectedJobId}/staging-records?${params.toString()}`);
    },
    enabled: Boolean(companyId) && Boolean(selectedJobId),
  });

  const { control: createControl, handleSubmit: handleCreateSubmit, reset: resetCreate, formState: { errors: createErrors, isSubmitting: isCreating } } = useForm<CreateJobFormValues>({
    resolver: zodResolver(createJobSchema),
    defaultValues: { sourceSystem: 'Tally' },
  });

  const { control: extractControl, handleSubmit: handleExtractSubmit, reset: resetExtract, formState: { errors: extractErrors } } = useForm<ExtractFormValues>({
    resolver: zodResolver(extractSchema),
    defaultValues: { tallyXmlContent: '' },
  });

  const createJob = useMutation({
    mutationFn: (values: CreateJobFormValues) =>
      apiClient.post<MigrationJobDto>('/integration-hub/migrations', { companyId, sourceSystem: values.sourceSystem }),
    onSuccess: (job) => {
      resetCreate({ sourceSystem: 'Tally' });
      setSelectedJobId(job.id);
      queryClient.invalidateQueries({ queryKey: ['migration-jobs', companyId] });
    },
  });

  const extractTally = useMutation({
    mutationFn: (values: ExtractFormValues) =>
      apiClient.post<MigrationJobDto>(`/integration-hub/migrations/${selectedJobId}/extract-tally`, {
        companyId,
        tallyXmlContent: values.tallyXmlContent,
      }),
    onSuccess: () => {
      resetExtract({ tallyXmlContent: '' });
      queryClient.invalidateQueries({ queryKey: ['migration-jobs', companyId] });
      queryClient.invalidateQueries({ queryKey: ['migration-staging-records', companyId, selectedJobId] });
    },
  });

  const validateJob = useMutation({
    mutationFn: () => apiClient.post<MigrationJobDto>(`/integration-hub/migrations/${selectedJobId}/validate`, { companyId }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['migration-jobs', companyId] });
      queryClient.invalidateQueries({ queryKey: ['migration-staging-records', companyId, selectedJobId] });
    },
  });

  const loadJob = useMutation({
    mutationFn: () => apiClient.post<MigrationJobDto>(`/integration-hub/migrations/${selectedJobId}/load`, { companyId }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['migration-jobs', companyId] });
      queryClient.invalidateQueries({ queryKey: ['migration-staging-records', companyId, selectedJobId] });
    },
  });

  const reconcileJob = useMutation({
    mutationFn: () => apiClient.post<MigrationJobDto>(`/integration-hub/migrations/${selectedJobId}/reconcile`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['migration-jobs', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-1 text-lg font-semibold text-text">Legacy System Migration</h2>
      <p className="mb-3 text-sm text-text-muted">
        Extract → Validate → Load → Reconcile — one-time bulk import from a retiring source system (Tally export supported now).
      </p>

      <Card className="mb-6">
        <form noValidate onSubmit={handleCreateSubmit((values) => createJob.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-[1fr_auto]">
          <label className="flex flex-col gap-1 text-sm">
            Source system
            <Controller
              control={createControl}
              name="sourceSystem"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Tally" {...field} />
              )}
            />
            {createErrors.sourceSystem && <span className="text-xs text-danger">{createErrors.sourceSystem.message}</span>}
          </label>
          <div className="flex items-end">
            <Button type="submit" disabled={isCreating}>{isCreating ? 'Creating…' : 'New migration job'}</Button>
          </div>
        </form>
        {createJob.isError && createJob.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createJob.error.problem.title}</p>
        )}
      </Card>

      <Card className="mb-6">
        <DataTable
          columns={[
            { header: 'Source', render: (row: MigrationJobDto) => row.sourceSystem },
            { header: 'Status', render: (row: MigrationJobDto) => row.status },
            { header: 'Extracted', render: (row: MigrationJobDto) => row.totalRecordsExtracted },
            { header: 'Loaded', render: (row: MigrationJobDto) => row.totalRecordsLoaded },
            { header: 'Failed', render: (row: MigrationJobDto) => row.totalRecordsFailed },
            { header: 'Created', render: (row: MigrationJobDto) => new Date(row.createdAt).toLocaleString() },
            {
              header: 'Actions',
              render: (row: MigrationJobDto) => (
                <Button type="button" variant={row.id === selectedJobId ? 'primary' : 'secondary'} onClick={() => setSelectedJobId(row.id)}>
                  {row.id === selectedJobId ? 'Selected' : 'Select'}
                </Button>
              ),
            },
          ]}
          rows={jobsQuery.data?.data}
          isLoading={jobsQuery.isLoading}
          isError={jobsQuery.isError}
          errorMessage="Could not load migration jobs."
          emptyMessage="No migration jobs yet — create one above."
          rowKey={(row) => row.id}
        />
      </Card>

      {selectedJob && (
        <Card className="mb-6">
          <h3 className="mb-3 text-base font-semibold text-text">
            {selectedJob.sourceSystem} migration — {selectedJob.status}
          </h3>

          {selectedJob.reconciliationNote && (
            <p className="mb-3 rounded-md border border-border bg-surface-muted p-2 text-sm text-text">{selectedJob.reconciliationNote}</p>
          )}

          {(selectedJob.status === 'Created' || selectedJob.status === 'Failed') && (
            <form noValidate onSubmit={handleExtractSubmit((values) => extractTally.mutate(values))} className="mb-4 flex flex-col gap-2">
              <label className="flex flex-col gap-1 text-sm">
                Tally XML export content
                <Controller
                  control={extractControl}
                  name="tallyXmlContent"
                  render={({ field }) => (
                    <textarea
                      className="h-40 rounded-md border border-border bg-surface px-2 py-1.5 font-mono text-xs"
                      placeholder="<ENVELOPE>...</ENVELOPE>"
                      {...field}
                    />
                  )}
                />
                {extractErrors.tallyXmlContent && <span className="text-xs text-danger">{extractErrors.tallyXmlContent.message}</span>}
              </label>
              <div>
                <Button type="submit" disabled={extractTally.isPending}>{extractTally.isPending ? 'Extracting…' : 'Extract'}</Button>
              </div>
              {extractTally.isError && extractTally.error instanceof ApiError && (
                <p role="alert" className="text-sm text-danger">{extractTally.error.problem.title}</p>
              )}
            </form>
          )}

          <div className="mb-4 flex flex-wrap items-center gap-2">
            <Button type="button" disabled={selectedJob.status !== 'Extracted' || validateJob.isPending} onClick={() => validateJob.mutate()}>
              {validateJob.isPending ? 'Validating…' : 'Validate'}
            </Button>
            <Button type="button" disabled={selectedJob.status !== 'Validated' || loadJob.isPending} onClick={() => loadJob.mutate()}>
              {loadJob.isPending ? 'Loading…' : 'Load'}
            </Button>
            <Button type="button" disabled={selectedJob.status !== 'Completed' || reconcileJob.isPending} onClick={() => reconcileJob.mutate()}>
              {reconcileJob.isPending ? 'Reconciling…' : 'Reconcile'}
            </Button>
          </div>
          {(validateJob.isError || loadJob.isError || reconcileJob.isError) && (
            <p role="alert" className="mb-3 text-sm text-danger">That pipeline step failed — check the job's status and staging records.</p>
          )}

          <label className="mb-3 flex flex-col gap-1 text-sm sm:w-72">
            Filter staging records by status
            <select
              className="rounded-md border border-border bg-surface px-2 py-1.5"
              value={stagingStatusFilter}
              onChange={(e) => setStagingStatusFilter(e.target.value)}
            >
              <option value="">All</option>
              <option value="Pending">Pending</option>
              <option value="Mapped">Mapped</option>
              <option value="Failed">Failed</option>
            </select>
          </label>

          <DataTable
            columns={[
              { header: 'Record type', render: (row: MigrationStagingRecordDto) => row.recordType },
              { header: 'Mapping status', render: (row: MigrationStagingRecordDto) => row.mappingStatus },
              { header: 'Validation errors', render: (row: MigrationStagingRecordDto) => row.validationErrors ?? '—' },
              { header: 'Loaded as', render: (row: MigrationStagingRecordDto) => (row.targetEntityType ? `${row.targetEntityType} (${row.targetEntityId})` : '—') },
            ]}
            rows={stagingRecordsQuery.data?.data}
            isLoading={stagingRecordsQuery.isLoading}
            isError={stagingRecordsQuery.isError}
            errorMessage="Could not load staging records."
            emptyMessage="No staging records yet — run Extract above."
            rowKey={(row) => row.id}
          />
        </Card>
      )}
    </div>
  );
}
