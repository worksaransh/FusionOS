import { useState } from 'react';
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
import { useBillOfMaterialsOptions, useProductOptions, useWarehouseOptions } from '../../../shared/api/entityOptions';
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
        <form onSubmit={handleSubmit((values) => createWorkOrder.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
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
