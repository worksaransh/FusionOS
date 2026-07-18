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
import { useKpiDefinitionOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  kpiDefinitionId: z.string().uuid('Pick a KPI'),
  value: z.string().refine((v) => !Number.isNaN(Number(v)), 'Must be a number'),
  notes: z.string().max(1000).or(z.literal('')),
});
type FormValues = z.infer<typeof schema>;

interface KpiSnapshotDto {
  id: string;
  kpiDefinitionId: string;
  value: number;
  recordedAt: string;
  notes: string | null;
}

/**
 * KPI Snapshots — manually-recorded, point-in-time KPI values, the time
 * series a dashboard chart would render (05_MODULE_ROADMAP.md's "Dashboards"/
 * "Charts" line items). Rendered as a sibling panel under KpiDefinitionsPage,
 * same pattern as LeaveRequestsPanel under EmployeesPage. No edit/delete —
 * same "append-only, corrections are new entries" convention as the
 * Inventory ledger's own panels.
 */
export function KpiSnapshotsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const kpiOptions = useKpiDefinitionOptions(companyId);

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { kpiDefinitionId: '', value: '', notes: '' },
  });

  const snapshotsQuery = useQuery({
    queryKey: ['kpi-snapshots', companyId],
    queryFn: () => apiClient.get<PagedResult<KpiSnapshotDto>>(`/bi/kpi-snapshots?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const recordSnapshot = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<KpiSnapshotDto>('/bi/kpi-snapshots', {
        companyId,
        kpiDefinitionId: values.kpiDefinitionId,
        value: Number(values.value),
        notes: values.notes || null,
      }),
    onSuccess: () => {
      reset({ kpiDefinitionId: '', value: '', notes: '' });
      queryClient.invalidateQueries({ queryKey: ['kpi-snapshots', companyId] });
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

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">KPI Snapshots</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => recordSnapshot.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <label className="flex flex-col gap-1 text-sm">
            KPI
            <Controller
              control={control}
              name="kpiDefinitionId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={kpiOptions.options}
                  isLoading={kpiOptions.isLoading}
                  onSearchChange={kpiOptions.onSearchChange}
                  placeholder="Search KPIs…"
                />
              )}
            />
            {errors.kpiDefinitionId && <span className="text-xs text-danger">{errors.kpiDefinitionId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Value
            <Controller
              control={control}
              name="value"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="97.5" {...field} />
              )}
            />
            {errors.value && <span className="text-xs text-danger">{errors.value.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Notes (optional)
            <Controller
              control={control}
              name="notes"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Week 28" {...field} />
              )}
            />
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Recording…' : 'Record value'}</Button>
          </div>
        </form>
        {recordSnapshot.isError && recordSnapshot.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{recordSnapshot.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Recorded', render: (row: KpiSnapshotDto) => new Date(row.recordedAt).toLocaleString() },
            { header: 'Value', render: (row: KpiSnapshotDto) => row.value.toLocaleString() },
            { header: 'Notes', render: (row: KpiSnapshotDto) => row.notes ?? '—' },
          ]}
          rows={snapshotsQuery.data?.data}
          isLoading={snapshotsQuery.isLoading}
          isError={snapshotsQuery.isError}
          errorMessage="Could not load KPI snapshots."
          emptyMessage="No KPI values recorded yet — record the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
    </div>
  );
}
