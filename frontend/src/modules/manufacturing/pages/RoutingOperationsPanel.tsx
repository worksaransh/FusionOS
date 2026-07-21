import { useState } from 'react';
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { ArrowDown, ArrowUp, Trash2 } from 'lucide-react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { EntityCombobox } from '../../../shared/ui/EntityCombobox';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import { useBillOfMaterialsOptions } from '../../../shared/api/entityOptions';

const schema = z.object({
  operationName: z.string().min(1, 'Operation name is required').max(200),
  workCenter: z.string().min(1, 'Work center is required').max(100),
  standardMinutes: z.string().refine((v) => Number(v) > 0, 'Standard minutes must be greater than zero'),
});
type FormValues = z.infer<typeof schema>;

interface RoutingOperationDto {
  id: string;
  sequenceNumber: number;
  operationName: string;
  workCenter: string;
  standardMinutes: number;
}

interface BillOfMaterialsDetailDto {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  operations: RoutingOperationDto[];
}

/**
 * Routing Operations — the production routing ("Cut" → "Assemble" → "Paint")
 * attached to one bill of materials. Rendered as a sibling panel under
 * BillsOfMaterialsPage, same pattern as WorkOrdersPanel (mirrors AccountsPage
 * composing Finance's sub-panels). Pick a BOM, then add / reorder (up-down) /
 * remove operations via BillsOfMaterialsController's
 * POST .../{id}/operations, POST .../{id}/operations/reorder and
 * DELETE .../{id}/operations/{operationId} actions.
 */
export function RoutingOperationsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const bomOptions = useBillOfMaterialsOptions(companyId);
  const [bomId, setBomId] = useState('');

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { operationName: '', workCenter: '', standardMinutes: '' },
  });

  const bomQuery = useQuery({
    queryKey: ['bill-of-materials-detail', companyId, bomId],
    queryFn: () => apiClient.get<BillOfMaterialsDetailDto>(`/manufacturing/bills-of-materials/${bomId}?companyId=${companyId}`),
    enabled: Boolean(companyId && bomId),
  });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['bill-of-materials-detail', companyId, bomId] });
    // The BOM list above shows an Operations count column — keep it in sync.
    queryClient.invalidateQueries({ queryKey: ['bills-of-materials', companyId] });
  };

  const addOperation = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<BillOfMaterialsDetailDto>(`/manufacturing/bills-of-materials/${bomId}/operations`, {
        companyId,
        operationName: values.operationName,
        workCenter: values.workCenter,
        standardMinutes: Number(values.standardMinutes),
      }),
    onSuccess: () => {
      reset({ operationName: '', workCenter: '', standardMinutes: '' });
      invalidate();
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  // Reorder takes the whole routing's operation ids in the new order — the
  // up/down buttons below just swap two neighbours in the current sequence.
  const reorderOperations = useMutation({
    mutationFn: (orderedOperationIds: string[]) =>
      apiClient.post<BillOfMaterialsDetailDto>(`/manufacturing/bills-of-materials/${bomId}/operations/reorder`, {
        companyId,
        orderedOperationIds,
      }),
    onSuccess: invalidate,
  });

  const removeOperation = useMutation({
    mutationFn: (operationId: string) =>
      apiClient.delete<BillOfMaterialsDetailDto>(`/manufacturing/bills-of-materials/${bomId}/operations/${operationId}?companyId=${companyId}`),
    onSuccess: invalidate,
  });

  if (!companyId) return null;

  const operations = [...(bomQuery.data?.operations ?? [])].sort((a, b) => a.sequenceNumber - b.sequenceNumber);
  const isMutating = reorderOperations.isPending || removeOperation.isPending;

  const move = (index: number, delta: -1 | 1) => {
    const ids = operations.map((op) => op.id);
    const target = index + delta;
    if (target < 0 || target >= ids.length) return;
    [ids[index], ids[target]] = [ids[target], ids[index]];
    reorderOperations.mutate(ids);
  };

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Routing Operations</h2>
      <p className="mb-3 text-xs text-text-muted">
        The production routing for one bill of materials — the ordered steps (e.g. Cut → Assemble → Paint) a work
        order walks through on the shop floor. Pick a BOM to view and edit its routing.
      </p>

      <Card className="mb-6">
        <label className="flex flex-col gap-1 text-sm sm:max-w-md">
          Bill of materials
          <EntityCombobox
            value={bomId}
            onChange={setBomId}
            options={bomOptions.options}
            isLoading={bomOptions.isLoading}
            onSearchChange={bomOptions.onSearchChange}
            placeholder="Search bills of materials…"
          />
        </label>
      </Card>

      {bomId && (
        <>
          <Card className="mb-6">
            <h3 className="mb-3 text-sm font-semibold text-text">Add operation</h3>
            <form onSubmit={handleSubmit((values) => addOperation.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
              <label className="flex flex-col gap-1 text-sm">
                Operation name
                <Controller
                  control={control}
                  name="operationName"
                  render={({ field }) => (
                    <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="e.g. Assemble" {...field} />
                  )}
                />
                {errors.operationName && <span className="text-xs text-danger">{errors.operationName.message}</span>}
              </label>
              <label className="flex flex-col gap-1 text-sm">
                Work center
                <Controller
                  control={control}
                  name="workCenter"
                  render={({ field }) => (
                    <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="e.g. Assembly Line 1" {...field} />
                  )}
                />
                {errors.workCenter && <span className="text-xs text-danger">{errors.workCenter.message}</span>}
              </label>
              <label className="flex flex-col gap-1 text-sm">
                Standard minutes
                <Controller
                  control={control}
                  name="standardMinutes"
                  render={({ field }) => (
                    <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="e.g. 30" {...field} />
                  )}
                />
                {errors.standardMinutes && <span className="text-xs text-danger">{errors.standardMinutes.message}</span>}
              </label>
              <div className="col-span-full">
                <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Adding…' : 'Add operation'}</Button>
              </div>
            </form>
            {addOperation.isError && addOperation.error instanceof ApiError && (
              <p role="alert" className="mt-2 text-sm text-danger">{addOperation.error.problem.title}</p>
            )}
          </Card>

          <Card>
            <DataTable
              columns={[
                { header: 'Seq', render: (op: RoutingOperationDto) => op.sequenceNumber },
                { header: 'Operation', render: (op: RoutingOperationDto) => op.operationName },
                { header: 'Work center', render: (op: RoutingOperationDto) => op.workCenter },
                { header: 'Std minutes', render: (op: RoutingOperationDto) => op.standardMinutes },
                {
                  header: 'Actions',
                  render: (op: RoutingOperationDto) => {
                    const index = operations.findIndex((o) => o.id === op.id);
                    return (
                      <div className="flex items-center gap-2">
                        <Button
                          type="button"
                          variant="secondary"
                          aria-label={`Move ${op.operationName} up`}
                          disabled={index === 0 || isMutating}
                          onClick={() => move(index, -1)}
                        >
                          <ArrowUp size={16} />
                        </Button>
                        <Button
                          type="button"
                          variant="secondary"
                          aria-label={`Move ${op.operationName} down`}
                          disabled={index === operations.length - 1 || isMutating}
                          onClick={() => move(index, 1)}
                        >
                          <ArrowDown size={16} />
                        </Button>
                        <Button
                          type="button"
                          variant="danger"
                          aria-label={`Remove ${op.operationName}`}
                          disabled={isMutating}
                          onClick={() => removeOperation.mutate(op.id)}
                        >
                          <Trash2 size={16} />
                        </Button>
                      </div>
                    );
                  },
                },
              ]}
              rows={bomQuery.data ? operations : undefined}
              isLoading={bomQuery.isLoading}
              isError={bomQuery.isError}
              errorMessage="Could not load this bill of materials' routing."
              emptyMessage="No routing operations yet — add the first one above."
              rowKey={(op) => op.id}
            />
          </Card>
          {reorderOperations.isError && (
            <p role="alert" className="mt-2 text-sm text-danger">Could not reorder the routing.</p>
          )}
          {removeOperation.isError && (
            <p role="alert" className="mt-2 text-sm text-danger">Could not remove that operation.</p>
          )}
        </>
      )}
    </div>
  );
}
