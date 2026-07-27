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
  entityType: z.string().min(1, 'Entity type is required').max(100),
  entityId: z.string().uuid('Must be a valid id'),
  reason: z.string().min(1, 'Reason is required').max(1000),
  inspectionId: z.string().refine((v) => v === '' || /^[0-9a-fA-F-]{36}$/.test(v), 'Must be blank or a valid Inspection id'),
});
type FormValues = z.infer<typeof schema>;

interface QualityHoldDto {
  id: string;
  entityType: string;
  entityId: string;
  reason: string;
  inspectionId: string | null;
  status: string;
  releasedAt: string | null;
  releasedByNote: string | null;
}

/**
 * Quality Holds — a hold/quarantine on some other module's record (EntityType/EntityId,
 * opaque — e.g. "InventoryLot"/"PurchaseOrder"/"SalesOrder"), optionally raised
 * automatically from a failed inspection's "Raise hold on failure" checkbox below, or placed
 * standalone here. Active -> Released. Rendered as a sibling panel under InspectionsPage.
 */
export function QualityHoldsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const holdsQuery = useQuery({
    queryKey: ['quality-holds', companyId],
    queryFn: () => apiClient.get<PagedResult<QualityHoldDto>>(`/quality/quality-holds?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { entityType: '', entityId: '', reason: '', inspectionId: '' },
  });

  const createHold = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<QualityHoldDto>('/quality/quality-holds', {
        companyId,
        entityType: values.entityType,
        entityId: values.entityId,
        reason: values.reason,
        inspectionId: values.inspectionId || null,
      }),
    onSuccess: () => {
      reset({ entityType: '', entityId: '', reason: '', inspectionId: '' });
      queryClient.invalidateQueries({ queryKey: ['quality-holds', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const releaseHold = useMutation({
    mutationFn: (id: string) => apiClient.post<QualityHoldDto>(`/quality/quality-holds/${id}/release`, { companyId, note: null }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['quality-holds', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Quality Holds</h2>
      <p className="mb-3 text-sm text-text-muted">
        A hold/quarantine on another module's record — an Inventory Lot, Purchase Order, Sales Order, etc. — pending investigation.
      </p>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createHold.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Entity type
            <Controller
              control={control}
              name="entityType"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="InventoryLot" {...field} />
              )}
            />
            {errors.entityType && <span className="text-xs text-danger">{errors.entityType.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Entity id
            <Controller
              control={control}
              name="entityId"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="00000000-0000-0000-0000-000000000000" {...field} />
              )}
            />
            {errors.entityId && <span className="text-xs text-danger">{errors.entityId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            Reason
            <Controller
              control={control}
              name="reason"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Pending re-inspection of failed lot" {...field} />
              )}
            />
            {errors.reason && <span className="text-xs text-danger">{errors.reason.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            Triggering inspection id (optional)
            <Controller
              control={control}
              name="inspectionId"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="00000000-0000-0000-0000-000000000000" {...field} />
              )}
            />
            {errors.inspectionId && <span className="text-xs text-danger">{errors.inspectionId.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Placing…' : 'Place hold'}</Button>
          </div>
        </form>
        {createHold.isError && createHold.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createHold.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Entity type', render: (row: QualityHoldDto) => row.entityType },
            { header: 'Entity id', render: (row: QualityHoldDto) => row.entityId },
            { header: 'Reason', render: (row: QualityHoldDto) => row.reason },
            { header: 'Status', render: (row: QualityHoldDto) => row.status },
            {
              header: 'Actions',
              render: (row: QualityHoldDto) =>
                row.status === 'Active' ? (
                  <Button type="button" variant="secondary" disabled={releaseHold.isPending} onClick={() => releaseHold.mutate(row.id)}>
                    Release
                  </Button>
                ) : null,
            },
          ]}
          rows={holdsQuery.data?.data}
          isLoading={holdsQuery.isLoading}
          isError={holdsQuery.isError}
          errorMessage="Could not load quality holds."
          emptyMessage="No quality holds yet — place the first one above, or raise one automatically from a failed inspection below."
          rowKey={(row) => row.id}
        />
      </Card>
      {releaseHold.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not release that quality hold.</p>
      )}
    </div>
  );
}
