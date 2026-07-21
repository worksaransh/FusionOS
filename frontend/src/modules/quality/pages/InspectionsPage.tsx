import { useState } from 'react';
import { Controller, useFieldArray, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Plus, Trash2 } from 'lucide-react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import type { PagedResult } from '../../../shared/api/types';
import { CorrectiveActionsPanel } from './CorrectiveActionsPanel';
import { NonConformanceReportsPanel } from './NonConformanceReportsPanel';

const INSPECTION_TYPES = ['IncomingGoods', 'Production'] as const;

const schema = z.object({
  type: z.enum(INSPECTION_TYPES),
  referenceId: z.string().uuid('Must be a valid id (the work order or goods receipt being inspected)'),
  characteristics: z.array(z.object({ value: z.string().min(1, 'Characteristic cannot be blank').max(200) })).min(1, 'At least one characteristic is required'),
});
type FormValues = z.infer<typeof schema>;

const resultsSchema = z.object({
  results: z.array(z.object({
    characteristic: z.string(),
    passed: z.enum(['true', 'false']),
    notes: z.string().max(500).or(z.literal('')),
  })),
});
type ResultsFormValues = z.infer<typeof resultsSchema>;

interface InspectionItemDto {
  id: string;
  characteristic: string;
  passed: boolean | null;
  notes: string | null;
}

interface InspectionDto {
  id: string;
  type: string;
  referenceId: string;
  status: string;
  items: InspectionItemDto[];
}

/**
 * Inspections — Quality's first real frontend slice (backend has existed
 * since the Manufacturing/CRM/Quality backend-only pass; this closes the
 * "frontend panel deferred" gap flagged in docs/PROJECT_TRACKER.md).
 * ReferenceId (the Work Order or Goods Receipt being inspected) is a plain
 * id input, not an EntityCombobox — Inspection's own class doc comment
 * documents this as a deliberately opaque cross-module reference, never
 * existence-validated even on the backend. Top-level page for /quality,
 * with NonConformanceReportsPanel and CorrectiveActionsPanel rendered as
 * sibling panels below it, same pattern as MaintenanceRequestsPanel under
 * AssetsPage.
 */
export function InspectionsPage() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [recordingInspectionId, setRecordingInspectionId] = useState<string | null>(null);

  const inspectionsQuery = useQuery({
    queryKey: ['inspections', companyId],
    queryFn: () => apiClient.get<PagedResult<InspectionDto>>(`/quality/inspections?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { type: 'IncomingGoods', referenceId: '', characteristics: [{ value: '' }] },
  });
  const { fields, append, remove } = useFieldArray({ control, name: 'characteristics' });

  const createInspection = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<InspectionDto>('/quality/inspections', {
        companyId,
        type: values.type,
        referenceId: values.referenceId,
        characteristics: values.characteristics.map((c) => c.value),
      }),
    onSuccess: () => {
      reset({ type: 'IncomingGoods', referenceId: '', characteristics: [{ value: '' }] });
      queryClient.invalidateQueries({ queryKey: ['inspections', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  if (!companyId) {
    return <p className="text-text-muted">Set an active Company ID in the header above to manage inspections.</p>;
  }

  const recordingInspection = inspectionsQuery.data?.data.find((i) => i.id === recordingInspectionId) ?? null;

  return (
    <div>
      <h1 className="mb-1 text-xl font-semibold text-text">Inspections</h1>
      <p className="mb-4 text-sm text-text-muted">
        Checklist inspection of a Work Order's output or a Goods Receipt — Quality, Phase 5.
      </p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createInspection.mutate(values))} className="flex flex-col gap-4">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <label className="flex flex-col gap-1 text-sm">
              Type
              <Controller
                control={control}
                name="type"
                render={({ field }) => (
                  <select className="rounded-md border border-border bg-surface px-2 py-1.5" {...field}>
                    {INSPECTION_TYPES.map((type) => (
                      <option key={type} value={type}>{type}</option>
                    ))}
                  </select>
                )}
              />
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Reference id (Work Order or Goods Receipt id)
              <Controller
                control={control}
                name="referenceId"
                render={({ field }) => (
                  <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="00000000-0000-0000-0000-000000000000" {...field} />
                )}
              />
              {errors.referenceId && <span className="text-xs text-danger">{errors.referenceId.message}</span>}
            </label>
          </div>

          <div className="flex flex-col gap-2">
            {fields.map((field, index) => (
              <div key={field.id} className="flex items-end gap-2">
                <label className="flex flex-col gap-1 text-sm">
                  Characteristic {index + 1}
                  <Controller
                    control={control}
                    name={`characteristics.${index}.value`}
                    render={({ field: lineField }) => (
                      <input className="w-72 rounded-md border border-border bg-surface px-2 py-1.5" placeholder="e.g. Dimensions within tolerance" {...lineField} />
                    )}
                  />
                </label>
                <Button type="button" variant="secondary" onClick={() => remove(index)} disabled={fields.length === 1}>
                  <Trash2 size={16} />
                </Button>
              </div>
            ))}
            {errors.characteristics && typeof errors.characteristics.message === 'string' && (
              <span className="text-xs text-danger">{errors.characteristics.message}</span>
            )}
            <Button type="button" variant="secondary" onClick={() => append({ value: '' })} className="w-fit">
              <Plus size={16} className="mr-1" /> Add characteristic
            </Button>
          </div>

          <Button type="submit" disabled={isSubmitting} className="w-fit">
            {isSubmitting ? 'Creating…' : 'Create inspection'}
          </Button>
        </form>
        {createInspection.isError && createInspection.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createInspection.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Type', render: (row: InspectionDto) => row.type },
            { header: 'Reference', render: (row: InspectionDto) => row.referenceId },
            { header: 'Characteristics', render: (row: InspectionDto) => row.items.length },
            { header: 'Status', render: (row: InspectionDto) => row.status },
            {
              header: 'Actions',
              render: (row: InspectionDto) =>
                row.status === 'Pending' ? (
                  <Button type="button" variant="secondary" onClick={() => setRecordingInspectionId(row.id)}>
                    Record results
                  </Button>
                ) : null,
            },
          ]}
          rows={inspectionsQuery.data?.data}
          isLoading={inspectionsQuery.isLoading}
          isError={inspectionsQuery.isError}
          errorMessage="Could not load inspections."
          emptyMessage="No inspections yet — create the first one above."
          rowKey={(row) => row.id}
        />
      </Card>

      {recordingInspection && (
        <RecordInspectionResultsPanel
          companyId={companyId}
          inspection={recordingInspection}
          onClose={() => setRecordingInspectionId(null)}
        />
      )}

      <NonConformanceReportsPanel />
      <CorrectiveActionsPanel />
    </div>
  );
}

interface RecordInspectionResultsPanelProps {
  companyId: string;
  inspection: InspectionDto;
  onClose: () => void;
}

function RecordInspectionResultsPanel({ companyId, inspection, onClose }: RecordInspectionResultsPanelProps) {
  const queryClient = useQueryClient();

  const { control, handleSubmit, formState: { isSubmitting } } = useForm<ResultsFormValues>({
    resolver: zodResolver(resultsSchema),
    values: {
      results: inspection.items.map((item) => ({ characteristic: item.characteristic, passed: 'true' as const, notes: '' })),
    },
  });
  const { fields } = useFieldArray({ control, name: 'results' });

  const recordResults = useMutation({
    mutationFn: (values: ResultsFormValues) =>
      apiClient.post<InspectionDto>(`/quality/inspections/${inspection.id}/results`, {
        companyId,
        results: values.results.map((r) => ({ characteristic: r.characteristic, passed: r.passed === 'true', notes: r.notes || null })),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['inspections', companyId] });
      onClose();
    },
  });

  return (
    <Card className="mt-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-base font-semibold text-text">Record results — {inspection.type} inspection</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form onSubmit={handleSubmit((values) => recordResults.mutate(values))} className="flex flex-col gap-3">
        {fields.map((field, index) => (
          <div key={field.id} className="flex items-end gap-3">
            <span className="w-56 text-sm">{field.characteristic}</span>
            <label className="flex flex-col gap-1 text-sm">
              Result
              <Controller
                control={control}
                name={`results.${index}.passed`}
                render={({ field: lineField }) => (
                  <select className="rounded-md border border-border bg-surface px-2 py-1.5" {...lineField}>
                    <option value="true">Pass</option>
                    <option value="false">Fail</option>
                  </select>
                )}
              />
            </label>
            <label className="flex flex-1 flex-col gap-1 text-sm">
              Notes (optional)
              <Controller
                control={control}
                name={`results.${index}.notes`}
                render={({ field: lineField }) => (
                  <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...lineField} />
                )}
              />
            </label>
          </div>
        ))}
        <div className="flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting} className="w-fit">{isSubmitting ? 'Saving…' : 'Submit results'}</Button>
          {recordResults.isError && (
            <span role="alert" className="text-sm text-danger">Could not record those results.</span>
          )}
        </div>
      </form>
    </Card>
  );
}
