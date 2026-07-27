import { Fragment, useState } from 'react';
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
import { useBillOfMaterialsOptions, useMachineOptions, useProductOptions, useWarehouseOptions, useWorkCenterOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  billOfMaterialsId: z.string().uuid('Pick a bill of materials'),
  warehouseId: z.string().uuid('Pick a warehouse'),
  quantityToProduce: z.string().refine((v) => Number(v) > 0, 'Quantity must be greater than zero'),
});
type FormValues = z.infer<typeof schema>;

interface WorkOrderComponentDto {
  id: string;
  componentProductId: string;
  quantityRequired: number;
  quantityIssued: number;
}

interface WorkOrderOperationDto {
  id: string;
  routingOperationId: string;
  sequenceNumber: number;
  operationName: string;
  workCenter: string;
  standardMinutes: number;
  status: string;
  machineId: string | null;
  scheduledStart: string | null;
  scheduledEnd: string | null;
  actualStart: string | null;
  actualEnd: string | null;
}

interface WorkOrderDto {
  id: string;
  billOfMaterialsId: string;
  productId: string;
  warehouseId: string;
  quantityToProduce: number;
  status: string;
  components: WorkOrderComponentDto[];
  quantityGoodProduced: number | null;
  quantityScrapped: number;
  yieldPercentage: number | null;
  operations: WorkOrderOperationDto[];
  sourceWorkOrderId: string | null;
  quantityReworked: number;
}

/**
 * Work Orders — an order to manufacture a quantity of a product from a bill
 * of materials, Draft → Released → Completed. Rendered as a sibling panel
 * under BillsOfMaterialsPage, same pattern as JournalEntriesPanel under
 * AccountsPage. Completing raises WorkOrderCompleted (consumed by Inventory
 * to post the real stock movements) — this panel only surfaces the action.
 * A Released work order gets a "Manage" button (same pattern as
 * PickListsPanel's PickListManagePanel) opening shop-floor material
 * issue/return per component plus the completion form with its optional
 * scrap/yield quantities.
 */
export function WorkOrdersPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const bomOptions = useBillOfMaterialsOptions(companyId);
  const warehouseOptions = useWarehouseOptions(companyId);
  const [managingWorkOrderId, setManagingWorkOrderId] = useState<string | null>(null);

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { billOfMaterialsId: '', warehouseId: '', quantityToProduce: '1' },
  });

  const workOrdersQuery = useQuery({
    queryKey: ['work-orders', companyId],
    queryFn: () => apiClient.get<PagedResult<WorkOrderDto>>(`/manufacturing/work-orders?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const createWorkOrder = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<WorkOrderDto>('/manufacturing/work-orders', {
        companyId,
        billOfMaterialsId: values.billOfMaterialsId,
        warehouseId: values.warehouseId,
        quantityToProduce: Number(values.quantityToProduce),
      }),
    onSuccess: () => {
      reset({ billOfMaterialsId: '', warehouseId: '', quantityToProduce: '1' });
      queryClient.invalidateQueries({ queryKey: ['work-orders', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const releaseWorkOrder = useMutation({
    mutationFn: (id: string) => apiClient.post<WorkOrderDto>(`/manufacturing/work-orders/${id}/release`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['work-orders', companyId] }),
  });

  // Cancels a not-yet-completed work order (WorkOrder.Cancel — Draft or Released only;
  // a Completed order's stock movements have already posted, so it can't be cancelled).
  // Same confirm-then-mutate pattern as PurchaseOrdersPanel's Cancel/Reject buttons.
  const cancelWorkOrder = useMutation({
    mutationFn: (id: string) => apiClient.post<WorkOrderDto>(`/manufacturing/work-orders/${id}/cancel`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['work-orders', companyId] }),
  });

  // Sends the scrapped-but-unclaimed remainder of a Completed work order into a new
  // rework order (CreateReworkWorkOrderCommand) — prompts for the quantity, capped at
  // what's still reworkable, then just invalidates the list so the new order appears.
  const createRework = useMutation({
    mutationFn: ({ id, quantityToRework }: { id: string; quantityToRework: number }) =>
      apiClient.post<WorkOrderDto>(`/manufacturing/work-orders/${id}/rework`, { companyId, quantityToRework }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['work-orders', companyId] }),
  });

  const handleCreateRework = (row: WorkOrderDto) => {
    const remaining = row.quantityScrapped - row.quantityReworked;
    if (remaining <= 0) return;
    const input = window.prompt(`Quantity to rework (up to ${remaining}):`, String(remaining));
    if (input === null) return;
    const quantity = Number(input);
    if (!Number.isFinite(quantity) || quantity <= 0 || quantity > remaining) return;
    createRework.mutate({ id: row.id, quantityToRework: quantity });
  };

  if (!companyId) return null;

  // Only a Released order can be managed — if it just completed (or the list
  // refetched into another state), the manage card disappears with it.
  const managingWorkOrder = workOrdersQuery.data?.data.find(
    (w) => w.id === managingWorkOrderId && w.status === 'Released',
  );

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Work Orders</h2>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createWorkOrder.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <label className="flex flex-col gap-1 text-sm">
            Bill of materials
            <Controller
              control={control}
              name="billOfMaterialsId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={bomOptions.options}
                  isLoading={bomOptions.isLoading}
                  onSearchChange={bomOptions.onSearchChange}
                  placeholder="Search bills of materials…"
                />
              )}
            />
            {errors.billOfMaterialsId && <span className="text-xs text-danger">{errors.billOfMaterialsId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Warehouse
            <Controller
              control={control}
              name="warehouseId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={warehouseOptions.options}
                  isLoading={warehouseOptions.isLoading}
                  onSearchChange={warehouseOptions.onSearchChange}
                  placeholder="Search warehouses…"
                />
              )}
            />
            {errors.warehouseId && <span className="text-xs text-danger">{errors.warehouseId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Quantity to produce
            <Controller
              control={control}
              name="quantityToProduce"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.quantityToProduce && <span className="text-xs text-danger">{errors.quantityToProduce.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create work order'}</Button>
          </div>
        </form>
        {createWorkOrder.isError && createWorkOrder.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createWorkOrder.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Status', render: (row: WorkOrderDto) => row.status },
            { header: 'Quantity to produce', render: (row: WorkOrderDto) => row.quantityToProduce },
            { header: 'Good produced', render: (row: WorkOrderDto) => row.quantityGoodProduced ?? '—' },
            { header: 'Scrapped', render: (row: WorkOrderDto) => (row.status === 'Completed' ? row.quantityScrapped : '—') },
            { header: 'Yield', render: (row: WorkOrderDto) => (row.yieldPercentage != null ? `${row.yieldPercentage.toLocaleString()}%` : '—') },
            {
              header: 'Actions',
              render: (row: WorkOrderDto) => (
                <div className="flex items-center gap-2">
                  {row.status === 'Draft' && (
                    <Button type="button" variant="secondary" disabled={releaseWorkOrder.isPending} onClick={() => releaseWorkOrder.mutate(row.id)}>
                      Release
                    </Button>
                  )}
                  {row.status === 'Released' && (
                    <Button type="button" variant="secondary" onClick={() => setManagingWorkOrderId(row.id)}>
                      Manage
                    </Button>
                  )}
                  {(row.status === 'Draft' || row.status === 'Released') && (
                    <Button
                      type="button"
                      variant="secondary"
                      disabled={cancelWorkOrder.isPending}
                      onClick={() => {
                        if (window.confirm('Cancel this work order? This cannot be undone.')) cancelWorkOrder.mutate(row.id);
                      }}
                    >
                      Cancel
                    </Button>
                  )}
                  {row.status === 'Completed' && row.quantityScrapped - row.quantityReworked > 0 && (
                    <Button
                      type="button"
                      variant="secondary"
                      disabled={createRework.isPending}
                      onClick={() => handleCreateRework(row)}
                    >
                      Create rework order ({row.quantityScrapped - row.quantityReworked} left)
                    </Button>
                  )}
                </div>
              ),
            },
          ]}
          rows={workOrdersQuery.data?.data}
          isLoading={workOrdersQuery.isLoading}
          isError={workOrdersQuery.isError}
          errorMessage="Could not load work orders."
          emptyMessage="No work orders yet — create the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {releaseWorkOrder.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not release that work order.</p>
      )}
      {cancelWorkOrder.isError && cancelWorkOrder.error instanceof ApiError && (
        <p role="alert" className="mt-2 text-sm text-danger">{cancelWorkOrder.error.problem.title}</p>
      )}
      {createRework.isError && createRework.error instanceof ApiError && (
        <p role="alert" className="mt-2 text-sm text-danger">{createRework.error.problem.title}</p>
      )}

      {managingWorkOrder && (
        <WorkOrderManagePanel
          companyId={companyId}
          workOrder={managingWorkOrder}
          onClose={() => setManagingWorkOrderId(null)}
        />
      )}
    </div>
  );
}

interface WorkOrderManagePanelProps {
  companyId: string;
  workOrder: WorkOrderDto;
  onClose: () => void;
}

/** Blank means "not entered"; otherwise it must parse to a non-negative number. */
function isValidOptionalQuantity(value: string): boolean {
  if (value.trim() === '') return true;
  const n = Number(value);
  return Number.isFinite(n) && n >= 0;
}

/**
 * Shop-floor view of one Released work order (same manage-card pattern as
 * PickListsPanel's PickListManagePanel): per-component material issue/return
 * against WorkOrdersController's POST .../materials/issue and
 * .../materials/return, then completion with optional good/scrapped
 * quantities (CompleteWorkOrderCommand's scrap/yield recording — leave both
 * blank for the default 100%-yield completion of the full planned quantity).
 */
function WorkOrderManagePanel({ companyId, workOrder, onClose }: WorkOrderManagePanelProps) {
  const queryClient = useQueryClient();
  const productOptions = useProductOptions(companyId);
  const [quantityInputs, setQuantityInputs] = useState<Record<string, string>>({});
  const [goodQuantity, setGoodQuantity] = useState('');
  const [scrappedQuantity, setScrappedQuantity] = useState('');

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['work-orders', companyId] });

  const issueMaterial = useMutation({
    mutationFn: ({ componentProductId, quantity }: { componentProductId: string; quantity: number }) =>
      apiClient.post<WorkOrderDto>(`/manufacturing/work-orders/${workOrder.id}/materials/issue`, {
        companyId,
        componentProductId,
        quantity,
      }),
    onSuccess: invalidate,
  });

  const returnMaterial = useMutation({
    mutationFn: ({ componentProductId, quantity }: { componentProductId: string; quantity: number }) =>
      apiClient.post<WorkOrderDto>(`/manufacturing/work-orders/${workOrder.id}/materials/return`, {
        companyId,
        componentProductId,
        quantity,
      }),
    onSuccess: invalidate,
  });

  const startOperation = useMutation({
    mutationFn: (operationId: string) =>
      apiClient.post<WorkOrderDto>(`/manufacturing/work-orders/${workOrder.id}/operations/${operationId}/start`, { companyId }),
    onSuccess: invalidate,
  });

  const completeOperation = useMutation({
    mutationFn: (operationId: string) =>
      apiClient.post<WorkOrderDto>(`/manufacturing/work-orders/${workOrder.id}/operations/${operationId}/complete`, { companyId }),
    onSuccess: invalidate,
  });

  // Machine Scheduling (Phase 6, 2026-07-23): assigns a machine + time window to a
  // Pending operation. ScheduleWorkOrderOperationCommandHandler does the cross-work-order
  // conflict check server-side (see its doc comment) — this mutation just surfaces
  // whatever ValidationException message comes back (e.g. "already scheduled on work
  // order X, operation Y…") the same way every other mutation here does.
  const [schedulingOperationId, setSchedulingOperationId] = useState<string | null>(null);
  const scheduleOperation = useMutation({
    mutationFn: ({ operationId, machineId, scheduledStart, scheduledEnd }: { operationId: string; machineId: string; scheduledStart: string; scheduledEnd: string }) =>
      apiClient.post<WorkOrderDto>(`/manufacturing/work-orders/${workOrder.id}/operations/${operationId}/schedule`, {
        companyId,
        machineId,
        scheduledStart,
        scheduledEnd,
      }),
    onSuccess: () => {
      invalidate();
      setSchedulingOperationId(null);
    },
  });

  const completeWorkOrder = useMutation({
    mutationFn: () =>
      apiClient.post<WorkOrderDto>(`/manufacturing/work-orders/${workOrder.id}/complete`, {
        companyId,
        quantityGoodProduced: goodQuantity.trim() === '' ? null : Number(goodQuantity),
        quantityScrapped: scrappedQuantity.trim() === '' ? null : Number(scrappedQuantity),
      }),
    onSuccess: () => {
      invalidate();
      onClose();
    },
  });

  const isMovingMaterial = issueMaterial.isPending || returnMaterial.isPending;
  const completionInputsValid = isValidOptionalQuantity(goodQuantity) && isValidOptionalQuantity(scrappedQuantity);

  return (
    <Card className="mt-6">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-lg font-semibold text-text">Manage work order — {workOrder.status}</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>

      <h4 className="mb-2 text-sm font-semibold text-text">Materials</h4>
      <table className="w-full text-sm">
        <thead>
          <tr className="text-left text-text-muted">
            <th className="pb-2">Component</th>
            <th className="pb-2">Required</th>
            <th className="pb-2">Issued</th>
            <th className="pb-2">Issue / return</th>
          </tr>
        </thead>
        <tbody>
          {workOrder.components.map((component) => {
            const input = quantityInputs[component.componentProductId] ?? '';
            const quantity = Number(input);
            const inputInvalid = !input || !Number.isFinite(quantity) || quantity <= 0;
            return (
              <tr key={component.id} className="border-t border-border">
                <td className="py-2">
                  {productOptions.options.find((p) => p.id === component.componentProductId)?.label ?? component.componentProductId}
                </td>
                <td className="py-2">{component.quantityRequired}</td>
                <td className="py-2">{component.quantityIssued}</td>
                <td className="py-2">
                  <div className="flex items-center gap-2">
                    <input
                      className="w-24 rounded-md border border-border bg-surface px-2 py-1"
                      placeholder="Qty"
                      value={input}
                      onChange={(e) =>
                        setQuantityInputs((prev) => ({ ...prev, [component.componentProductId]: e.target.value }))
                      }
                    />
                    <Button
                      type="button"
                      variant="secondary"
                      disabled={inputInvalid || isMovingMaterial}
                      onClick={() => issueMaterial.mutate({ componentProductId: component.componentProductId, quantity })}
                    >
                      Issue
                    </Button>
                    <Button
                      type="button"
                      variant="secondary"
                      disabled={inputInvalid || isMovingMaterial || component.quantityIssued <= 0}
                      onClick={() => returnMaterial.mutate({ componentProductId: component.componentProductId, quantity })}
                    >
                      Return
                    </Button>
                  </div>
                </td>
              </tr>
            );
          })}
          {workOrder.components.length === 0 && (
            <tr>
              <td colSpan={4} className="py-4 text-center text-text-muted">
                This work order has no snapshotted components.
              </td>
            </tr>
          )}
        </tbody>
      </table>
      {issueMaterial.isError && issueMaterial.error instanceof ApiError && (
        <p role="alert" className="mt-2 text-sm text-danger">{issueMaterial.error.problem.title}</p>
      )}
      {returnMaterial.isError && returnMaterial.error instanceof ApiError && (
        <p role="alert" className="mt-2 text-sm text-danger">{returnMaterial.error.problem.title}</p>
      )}

      <h4 className="mb-2 mt-6 text-sm font-semibold text-text">Operations</h4>
      <table className="w-full text-sm">
        <thead>
          <tr className="text-left text-text-muted">
            <th className="pb-2">Seq</th>
            <th className="pb-2">Operation</th>
            <th className="pb-2">Work center</th>
            <th className="pb-2">Status</th>
            <th className="pb-2">Scheduled</th>
            <th className="pb-2">Action</th>
          </tr>
        </thead>
        <tbody>
          {[...workOrder.operations].sort((a, b) => a.sequenceNumber - b.sequenceNumber).map((operation) => (
            <Fragment key={operation.id}>
              <tr className="border-t border-border">
                <td className="py-2">{operation.sequenceNumber}</td>
                <td className="py-2">{operation.operationName}</td>
                <td className="py-2">{operation.workCenter}</td>
                <td className="py-2">{operation.status}</td>
                <td className="py-2">
                  {operation.scheduledStart
                    ? `${new Date(operation.scheduledStart).toLocaleString()} – ${operation.scheduledEnd ? new Date(operation.scheduledEnd).toLocaleString() : ''}`
                    : '—'}
                </td>
                <td className="py-2">
                  <div className="flex items-center gap-2">
                    {operation.status === 'Pending' && (
                      <Button
                        type="button"
                        variant="secondary"
                        disabled={startOperation.isPending}
                        onClick={() => startOperation.mutate(operation.id)}
                      >
                        Start
                      </Button>
                    )}
                    {operation.status === 'InProgress' && (
                      <Button
                        type="button"
                        variant="secondary"
                        disabled={completeOperation.isPending}
                        onClick={() => completeOperation.mutate(operation.id)}
                      >
                        Complete
                      </Button>
                    )}
                    {operation.status === 'Pending' && !operation.machineId && (
                      <Button
                        type="button"
                        variant="secondary"
                        onClick={() => setSchedulingOperationId(schedulingOperationId === operation.id ? null : operation.id)}
                      >
                        Schedule
                      </Button>
                    )}
                  </div>
                </td>
              </tr>
              {schedulingOperationId === operation.id && (
                <tr className="border-t border-border bg-surface-muted">
                  <td colSpan={6} className="py-2">
                    <ScheduleOperationForm
                      companyId={companyId}
                      onSchedule={(values) => scheduleOperation.mutate({ operationId: operation.id, ...values })}
                      onCancel={() => setSchedulingOperationId(null)}
                      isPending={scheduleOperation.isPending}
                      fieldErrors={
                        scheduleOperation.isError && scheduleOperation.error instanceof ApiError
                          ? scheduleOperation.error.problem.errors
                          : undefined
                      }
                    />
                  </td>
                </tr>
              )}
            </Fragment>
          ))}
          {workOrder.operations.length === 0 && (
            <tr>
              <td colSpan={6} className="py-4 text-center text-text-muted">
                This work order has no snapshotted routing operations.
              </td>
            </tr>
          )}
        </tbody>
      </table>
      {startOperation.isError && startOperation.error instanceof ApiError && (
        <p role="alert" className="mt-2 text-sm text-danger">{startOperation.error.problem.title}</p>
      )}
      {completeOperation.isError && completeOperation.error instanceof ApiError && (
        <p role="alert" className="mt-2 text-sm text-danger">{completeOperation.error.problem.title}</p>
      )}
      {scheduleOperation.isError && scheduleOperation.error instanceof ApiError && (
        <p role="alert" className="mt-2 text-sm text-danger">{scheduleOperation.error.problem.title}</p>
      )}

      <h4 className="mb-2 mt-6 text-sm font-semibold text-text">Complete</h4>
      <p className="mb-3 text-xs text-text-muted">
        Optionally record scrap/yield — leave both blank to complete the full planned quantity
        ({workOrder.quantityToProduce}) with no scrap.
      </p>
      <div className="flex flex-wrap items-end gap-2">
        <label className="flex flex-col gap-1 text-sm">
          Good quantity produced (optional)
          <input
            className="w-40 rounded-md border border-border bg-surface px-2 py-1.5"
            placeholder={String(workOrder.quantityToProduce)}
            value={goodQuantity}
            onChange={(e) => setGoodQuantity(e.target.value)}
          />
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Quantity scrapped (optional)
          <input
            className="w-40 rounded-md border border-border bg-surface px-2 py-1.5"
            placeholder="0"
            value={scrappedQuantity}
            onChange={(e) => setScrappedQuantity(e.target.value)}
          />
        </label>
        <Button
          type="button"
          disabled={!completionInputsValid || completeWorkOrder.isPending}
          onClick={() => completeWorkOrder.mutate()}
        >
          {completeWorkOrder.isPending ? 'Completing…' : 'Complete work order'}
        </Button>
      </div>
      {!completionInputsValid && (
        <p className="mt-1 text-xs text-danger">Quantities must be blank or non-negative numbers.</p>
      )}
      {completeWorkOrder.isError && completeWorkOrder.error instanceof ApiError && (
        <p role="alert" className="mt-2 text-sm text-danger">{completeWorkOrder.error.problem.title}</p>
      )}
    </Card>
  );
}

interface ScheduleOperationFormProps {
  companyId: string;
  isPending: boolean;
  onSchedule: (values: { machineId: string; scheduledStart: string; scheduledEnd: string }) => void;
  onCancel: () => void;
  /** ProblemDetails.errors from the last failed schedule attempt, keyed by field name (e.g. "ScheduledStart" for a machine-conflict overlap). */
  fieldErrors?: Record<string, string[]>;
}

/**
 * Inline form for ScheduleWorkOrderOperationCommand (Machine Scheduling, Phase 6,
 * 2026-07-23): pick a work center (just to narrow the machine combobox — machines nest
 * under one work center), then a machine, then a start/end window. The server does the
 * real work — checking every other Released work order's operations on that machine for
 * a time overlap — so this form only validates that the window itself is non-empty and
 * end is after start before enabling Confirm. A machine-schedule conflict comes back as a
 * ValidationException on ScheduledStart (see ScheduleWorkOrderOperationCommandHandler) with
 * the specific overlap detail (conflicting work order/operation/window) — rendered here
 * under the Scheduled start input, same per-field pattern as BillsOfMaterialsPage, rather
 * than only the generic problem.title the panel's fallback paragraph shows.
 */
function ScheduleOperationForm({ companyId, isPending, onSchedule, onCancel, fieldErrors }: ScheduleOperationFormProps) {
  const workCenterOptions = useWorkCenterOptions(companyId);
  const [workCenterId, setWorkCenterId] = useState('');
  const machineOptions = useMachineOptions(companyId, workCenterId || undefined);
  const [machineId, setMachineId] = useState('');
  const [scheduledStart, setScheduledStart] = useState('');
  const [scheduledEnd, setScheduledEnd] = useState('');

  const isValid = Boolean(machineId && scheduledStart && scheduledEnd && new Date(scheduledEnd) > new Date(scheduledStart));

  return (
    <div className="flex flex-wrap items-end gap-2">
      <label className="flex flex-col gap-1 text-sm">
        Work center
        <EntityCombobox
          className="w-56"
          value={workCenterId}
          onChange={(id) => {
            setWorkCenterId(id);
            setMachineId('');
          }}
          options={workCenterOptions.options}
          isLoading={workCenterOptions.isLoading}
          placeholder="Search work centers…"
        />
      </label>
      <label className="flex flex-col gap-1 text-sm">
        Machine
        <EntityCombobox
          className="w-56"
          value={machineId}
          onChange={setMachineId}
          options={machineOptions.options}
          isLoading={machineOptions.isLoading}
          placeholder={workCenterId ? 'Search machines…' : 'Pick a work center first'}
          disabled={!workCenterId}
        />
      </label>
      <label className="flex flex-col gap-1 text-sm">
        Scheduled start
        <input
          type="datetime-local"
          className="rounded-md border border-border bg-surface px-2 py-1.5"
          value={scheduledStart}
          onChange={(e) => setScheduledStart(e.target.value)}
        />
        {fieldErrors?.ScheduledStart && <span className="text-xs text-danger">{fieldErrors.ScheduledStart[0]}</span>}
      </label>
      <label className="flex flex-col gap-1 text-sm">
        Scheduled end
        <input
          type="datetime-local"
          className="rounded-md border border-border bg-surface px-2 py-1.5"
          value={scheduledEnd}
          onChange={(e) => setScheduledEnd(e.target.value)}
        />
      </label>
      <Button
        type="button"
        disabled={!isValid || isPending}
        onClick={() =>
          onSchedule({
            machineId,
            scheduledStart: new Date(scheduledStart).toISOString(),
            scheduledEnd: new Date(scheduledEnd).toISOString(),
          })
        }
      >
        {isPending ? 'Scheduling…' : 'Confirm schedule'}
      </Button>
      <Button type="button" variant="secondary" onClick={onCancel}>Cancel</Button>
    </div>
  );
}
