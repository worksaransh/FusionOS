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

const schema = z.object({
  code: z.string().min(1, 'Code is required').max(50),
  name: z.string().min(1, 'Name is required').max(200),
  capacityHoursPerDay: z.string().refine((v) => Number(v) > 0, 'Capacity must be greater than zero'),
});
type FormValues = z.infer<typeof schema>;

interface WorkCenterDto {
  id: string;
  code: string;
  name: string;
  capacityHoursPerDay: number;
  isActive: boolean;
}

interface WorkCenterCapacityLoadDto {
  workCenterId: string;
  availableHours: number;
  requiredHours: number;
  utilizationPercentage: number;
}

/**
 * WorkCenters — Capacity Planning support (Phase 6, 2026-07-23) master data: a shop-floor
 * work center's daily capacity. Same CRUD-ish shape (create form + list + soft-deactivate)
 * as BillsOfMaterialsPage, rendered as a sibling panel. Each row also gets a "View capacity
 * load" toggle opening a period picker against GetWorkCenterCapacityLoadQuery.
 */
export function WorkCentersPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [capacityRowId, setCapacityRowId] = useState<string | null>(null);

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { code: '', name: '', capacityHoursPerDay: '8' },
  });

  const workCentersQuery = useQuery({
    queryKey: ['work-centers', companyId],
    queryFn: () => apiClient.get<PagedResult<WorkCenterDto>>(`/manufacturing/work-centers?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const createWorkCenter = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<WorkCenterDto>('/manufacturing/work-centers', {
        companyId,
        code: values.code,
        name: values.name,
        capacityHoursPerDay: Number(values.capacityHoursPerDay),
      }),
    onSuccess: () => {
      reset({ code: '', name: '', capacityHoursPerDay: '8' });
      queryClient.invalidateQueries({ queryKey: ['work-centers', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const deactivateWorkCenter = useMutation({
    mutationFn: (workCenterId: string) => apiClient.post<WorkCenterDto>(`/manufacturing/work-centers/${workCenterId}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['work-centers', companyId] }),
  });

  if (!companyId) return null;

  const capacityRow = workCentersQuery.data?.data.find((w) => w.id === capacityRowId);

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Work Centers</h2>
      <p className="mb-3 text-sm text-text-muted">Master data for a shop-floor work center's daily capacity — Capacity Planning, Phase 6.</p>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createWorkCenter.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <label className="flex flex-col gap-1 text-sm">
            Code
            <Controller
              control={control}
              name="code"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="WC-100" {...field} />
              )}
            />
            {errors.code && <span className="text-xs text-danger">{errors.code.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Name
            <Controller
              control={control}
              name="name"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Capacity hours/day
            <Controller
              control={control}
              name="capacityHoursPerDay"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.capacityHoursPerDay && <span className="text-xs text-danger">{errors.capacityHoursPerDay.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create work center'}</Button>
          </div>
        </form>
        {createWorkCenter.isError && createWorkCenter.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createWorkCenter.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Code', render: (row: WorkCenterDto) => row.code },
            { header: 'Name', render: (row: WorkCenterDto) => row.name },
            { header: 'Capacity hours/day', render: (row: WorkCenterDto) => row.capacityHoursPerDay },
            { header: 'Status', render: (row: WorkCenterDto) => (row.isActive ? 'Active' : 'Inactive') },
            {
              header: 'Actions',
              render: (row: WorkCenterDto) => (
                <div className="flex items-center gap-2">
                  <Button type="button" variant="secondary" onClick={() => setCapacityRowId(row.id === capacityRowId ? null : row.id)}>
                    {row.id === capacityRowId ? 'Hide capacity load' : 'View capacity load'}
                  </Button>
                  <Button
                    type="button"
                    variant="danger"
                    disabled={!row.isActive || deactivateWorkCenter.isPending}
                    onClick={() => deactivateWorkCenter.mutate(row.id)}
                  >
                    {row.isActive ? 'Deactivate' : 'Deactivated'}
                  </Button>
                </div>
              ),
            },
          ]}
          rows={workCentersQuery.data?.data}
          isLoading={workCentersQuery.isLoading}
          isError={workCentersQuery.isError}
          errorMessage="Could not load work centers."
          emptyMessage="No work centers yet — create the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {deactivateWorkCenter.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that work center.</p>
      )}

      {capacityRow && (
        <CapacityLoadPanel companyId={companyId} workCenter={capacityRow} onClose={() => setCapacityRowId(null)} />
      )}
    </div>
  );
}

interface CapacityLoadPanelProps {
  companyId: string;
  workCenter: WorkCenterDto;
  onClose: () => void;
}

/**
 * Period picker over GetWorkCenterCapacityLoadQuery — available (CapacityHoursPerDay ×
 * days in period) vs. required (scheduled operation hours matched by work-center
 * name/code, best-effort string match — see the handler's doc comment) hours.
 */
function CapacityLoadPanel({ companyId, workCenter, onClose }: CapacityLoadPanelProps) {
  const today = new Date().toISOString().slice(0, 10);
  const [periodStart, setPeriodStart] = useState(today);
  const [periodEnd, setPeriodEnd] = useState(today);

  const capacityLoadQuery = useQuery({
    queryKey: ['work-center-capacity-load', companyId, workCenter.id, periodStart, periodEnd],
    queryFn: () =>
      apiClient.get<WorkCenterCapacityLoadDto>(
        `/manufacturing/work-centers/${workCenter.id}/capacity-load?companyId=${companyId}&periodStart=${periodStart}&periodEnd=${periodEnd}`,
      ),
    enabled: Boolean(periodStart && periodEnd),
  });

  return (
    <Card className="mt-6">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-lg font-semibold text-text">Capacity load — {workCenter.code}</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>

      <div className="mb-4 flex flex-wrap items-end gap-3">
        <label className="flex flex-col gap-1 text-sm">
          Period start
          <input
            type="date"
            className="rounded-md border border-border bg-surface px-2 py-1.5"
            value={periodStart}
            onChange={(e) => setPeriodStart(e.target.value)}
          />
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Period end
          <input
            type="date"
            className="rounded-md border border-border bg-surface px-2 py-1.5"
            value={periodEnd}
            onChange={(e) => setPeriodEnd(e.target.value)}
          />
        </label>
      </div>

      {capacityLoadQuery.isLoading && <p role="status" className="text-text-muted">Loading…</p>}
      {capacityLoadQuery.isError && <p role="alert" className="text-danger">Could not load capacity for that period.</p>}
      {capacityLoadQuery.data && (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <div className="rounded-md border border-border p-3">
            <p className="text-xs text-text-muted">Available hours</p>
            <p className="text-lg font-semibold text-text">{capacityLoadQuery.data.availableHours.toLocaleString()}</p>
          </div>
          <div className="rounded-md border border-border p-3">
            <p className="text-xs text-text-muted">Required hours</p>
            <p className="text-lg font-semibold text-text">{capacityLoadQuery.data.requiredHours.toLocaleString()}</p>
          </div>
          <div className="rounded-md border border-border p-3">
            <p className="text-xs text-text-muted">Utilization</p>
            <p className="text-lg font-semibold text-text">{capacityLoadQuery.data.utilizationPercentage.toLocaleString()}%</p>
          </div>
        </div>
      )}
    </Card>
  );
}
