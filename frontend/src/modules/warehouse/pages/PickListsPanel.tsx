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
import { EntityCombobox } from '../../../shared/ui/EntityCombobox';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import {
  useBinOptions,
  useProductOptions,
  useSalesOrderOptions,
  useUserOptions,
  useWarehouseOptions,
  useZoneOptions,
} from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const lineSchema = z.object({
  productId: z.string().uuid('Pick a product'),
  binId: z.string().refine((v) => v === '' || /^[0-9a-fA-F-]{36}$/.test(v), 'Must be blank or a valid bin'),
  quantityToPick: z.string().refine((v) => Number(v) > 0, 'Quantity must be greater than zero'),
});

const schema = z.object({
  warehouseId: z.string().uuid('Pick a warehouse'),
  zoneId: z.string().refine((v) => v === '' || /^[0-9a-fA-F-]{36}$/.test(v), 'Pick a zone (to search bins) or leave blank'),
  salesOrderId: z.string().uuid('Pick a sales order'),
  lines: z.array(lineSchema).min(1, 'At least one line is required'),
});
type FormValues = z.infer<typeof schema>;

interface PickListLineDto {
  id: string;
  productId: string;
  binId: string | null;
  quantityToPick: number;
  quantityPicked: number;
}

interface PickListDto {
  id: string;
  warehouseId: string;
  salesOrderId: string;
  assignedToUserId: string | null;
  status: 'Pending' | 'Assigned' | 'Picked' | 'Packed';
  lines: PickListLineDto[];
  createdAt: string;
}

/**
 * Picking + Packing (docs/IMPLEMENTATION_PLAN.md Phase 9 items 10-11, Phase M9 2026-07-15). A pick
 * list references a Sales Order by id only — this module doesn't validate that it exists or that
 * lines match the order's own lines (see PickList.cs's doc comment: no cross-module FK, same as
 * everywhere else); the Sales Order picker here is just this page calling Sales' own API, same as
 * any other EntityCombobox.
 */
export function PickListsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [lookupWarehouseId, setLookupWarehouseId] = useState('');
  const [managingPickListId, setManagingPickListId] = useState<string | null>(null);

  const warehouseOptions = useWarehouseOptions(companyId);
  const salesOrderOptions = useSalesOrderOptions(companyId);
  const productOptions = useProductOptions(companyId);

  const { control, handleSubmit, watch, reset, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { warehouseId: '', zoneId: '', salesOrderId: '', lines: [{ productId: '', binId: '', quantityToPick: '1' }] },
  });
  const { fields, append, remove } = useFieldArray({ control, name: 'lines' });
  const watchedWarehouseId = watch('warehouseId');
  const watchedZoneId = watch('zoneId');
  const effectiveWarehouseId = lookupWarehouseId || watchedWarehouseId;
  const zoneOptions = useZoneOptions(companyId, effectiveWarehouseId || undefined);
  const binOptions = useBinOptions(companyId, effectiveWarehouseId || undefined, watchedZoneId || undefined);

  const pickListsQuery = useQuery({
    queryKey: ['pick-lists', companyId, effectiveWarehouseId],
    queryFn: () =>
      apiClient.get<PagedResult<PickListDto>>(
        `/warehouse/warehouses/${effectiveWarehouseId}/pick-lists?companyId=${companyId}&page=1&pageSize=25`,
      ),
    enabled: Boolean(companyId && effectiveWarehouseId),
  });

  const createPickList = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<PickListDto>(`/warehouse/warehouses/${values.warehouseId}/pick-lists`, {
        companyId,
        salesOrderId: values.salesOrderId,
        lines: values.lines.map((l) => ({
          productId: l.productId,
          binId: l.binId || null,
          quantityToPick: Number(l.quantityToPick),
        })),
      }),
    onSuccess: (_data, variables) => {
      reset({ warehouseId: variables.warehouseId, zoneId: '', salesOrderId: '', lines: [{ productId: '', binId: '', quantityToPick: '1' }] });
      setLookupWarehouseId(variables.warehouseId);
      queryClient.invalidateQueries({ queryKey: ['pick-lists', companyId, variables.warehouseId] });
    },
  });

  if (!companyId) return null;

  const managingPickList = pickListsQuery.data?.data.find((p) => p.id === managingPickListId) ?? null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Pick Lists (Picking &amp; Packing)</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createPickList.mutate(values))} className="flex flex-col gap-4">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <label className="flex flex-col gap-1 text-sm">
              Warehouse
              <Controller
                control={control}
                name="warehouseId"
                render={({ field }) => (
                  <EntityCombobox
                    value={field.value}
                    onChange={(id) => {
                      field.onChange(id);
                      setLookupWarehouseId(id);
                    }}
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
              Sales order
              <Controller
                control={control}
                name="salesOrderId"
                render={({ field }) => (
                  <EntityCombobox
                    value={field.value}
                    onChange={field.onChange}
                    options={salesOrderOptions.options}
                    isLoading={salesOrderOptions.isLoading}
                    placeholder="Search sales orders…"
                  />
                )}
              />
              {errors.salesOrderId && <span className="text-xs text-danger">{errors.salesOrderId.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Zone (to search bins, optional)
              <Controller
                control={control}
                name="zoneId"
                render={({ field }) => (
                  <EntityCombobox
                    value={field.value}
                    onChange={field.onChange}
                    options={zoneOptions.options}
                    isLoading={zoneOptions.isLoading}
                    placeholder={effectiveWarehouseId ? 'Search zones…' : 'Pick a warehouse first'}
                    disabled={!effectiveWarehouseId}
                  />
                )}
              />
            </label>
          </div>

          <div className="flex flex-col gap-2">
            {fields.map((field, index) => (
              <div key={field.id} className="flex items-end gap-2">
                <label className="flex flex-col gap-1 text-sm">
                  Product
                  <Controller
                    control={control}
                    name={`lines.${index}.productId`}
                    render={({ field: lineField }) => (
                      <EntityCombobox
                        className="w-72"
                        value={lineField.value}
                        onChange={lineField.onChange}
                        options={productOptions.options}
                        isLoading={productOptions.isLoading}
                        onSearchChange={productOptions.onSearchChange}
                        placeholder="Search products…"
                      />
                    )}
                  />
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  Bin (optional)
                  <Controller
                    control={control}
                    name={`lines.${index}.binId`}
                    render={({ field: lineField }) => (
                      <EntityCombobox
                        className="w-56"
                        value={lineField.value}
                        onChange={lineField.onChange}
                        options={binOptions.options}
                        isLoading={binOptions.isLoading}
                        placeholder={watchedZoneId ? 'Search bins…' : 'Pick a zone first'}
                        disabled={!watchedZoneId}
                      />
                    )}
                  />
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  Quantity to pick
                  <Controller
                    control={control}
                    name={`lines.${index}.quantityToPick`}
                    render={({ field: lineField }) => (
                      <input className="w-32 rounded-md border border-border bg-surface px-2 py-1.5" {...lineField} />
                    )}
                  />
                </label>
                <Button type="button" variant="secondary" onClick={() => remove(index)} disabled={fields.length === 1}>
                  <Trash2 size={16} />
                </Button>
              </div>
            ))}
            {errors.lines && typeof errors.lines.message === 'string' && (
              <span className="text-xs text-danger">{errors.lines.message}</span>
            )}
            <Button
              type="button"
              variant="secondary"
              onClick={() => append({ productId: '', binId: '', quantityToPick: '1' })}
              className="w-fit"
            >
              <Plus size={16} className="mr-1" /> Add line
            </Button>
          </div>

          <Button type="submit" disabled={isSubmitting} className="w-fit">
            {isSubmitting ? 'Creating…' : 'Create pick list'}
          </Button>
        </form>
        {createPickList.isError && createPickList.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createPickList.error.problem.title}</p>
        )}
      </Card>

      {pickListsQuery.data && (
        <Card>
          <DataTable
            columns={[
              { header: 'Sales order', render: (p: PickListDto) => salesOrderOptions.options.find((o) => o.id === p.salesOrderId)?.label ?? p.salesOrderId },
              { header: 'Lines', render: (p: PickListDto) => p.lines.length },
              { header: 'Status', render: (p: PickListDto) => p.status },
              {
                header: 'Actions',
                render: (p: PickListDto) => (
                  <Button type="button" variant="secondary" onClick={() => setManagingPickListId(p.id)}>
                    Manage
                  </Button>
                ),
              },
            ]}
            rows={pickListsQuery.data.data}
            isLoading={pickListsQuery.isLoading}
            emptyMessage="No pick lists yet for this warehouse."
            rowKey={(p) => p.id}
          />
        </Card>
      )}

      {managingPickList && (
        <PickListManagePanel
          companyId={companyId}
          warehouseId={effectiveWarehouseId}
          pickList={managingPickList}
          onClose={() => setManagingPickListId(null)}
        />
      )}
    </div>
  );
}

interface PickListManagePanelProps {
  companyId: string;
  warehouseId: string;
  pickList: PickListDto;
  onClose: () => void;
}

function PickListManagePanel({ companyId, warehouseId, pickList, onClose }: PickListManagePanelProps) {
  const queryClient = useQueryClient();
  const userOptions = useUserOptions(companyId);
  const productOptions = useProductOptions(companyId);
  const [assigneeId, setAssigneeId] = useState('');
  const [countInputs, setCountInputs] = useState<Record<string, string>>({});

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['pick-lists', companyId, warehouseId] });

  const assign = useMutation({
    mutationFn: () => apiClient.post<PickListDto>(`/warehouse/warehouses/${warehouseId}/pick-lists/${pickList.id}/assign`, { companyId, assignedToUserId: assigneeId }),
    onSuccess: invalidate,
  });

  const recordPick = useMutation({
    mutationFn: ({ lineId, quantityPicked }: { lineId: string; quantityPicked: number }) =>
      apiClient.post<PickListDto>(`/warehouse/warehouses/${warehouseId}/pick-lists/${pickList.id}/record`, { companyId, lineId, quantityPicked }),
    onSuccess: invalidate,
  });

  const pack = useMutation({
    mutationFn: () => apiClient.post<PickListDto>(`/warehouse/warehouses/${warehouseId}/pick-lists/${pickList.id}/pack`, { companyId }),
    onSuccess: invalidate,
  });

  const isPacked = pickList.status === 'Packed';

  return (
    <Card className="mt-6">
      <div className="mb-3 flex items-center justify-between">
        <h2 className="text-lg font-semibold text-text">Manage pick list — {pickList.status}</h2>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>

      <div className="mb-4 flex items-end gap-2">
        <label className="flex flex-col gap-1 text-sm">
          Assign to
          <EntityCombobox
            className="w-64"
            value={assigneeId}
            onChange={setAssigneeId}
            options={userOptions.options}
            isLoading={userOptions.isLoading}
            onSearchChange={userOptions.onSearchChange}
            placeholder="Search users…"
            disabled={isPacked}
          />
        </label>
        <Button type="button" disabled={!assigneeId || isPacked || assign.isPending} onClick={() => assign.mutate()}>
          {pickList.assignedToUserId ? 'Reassign' : 'Assign'}
        </Button>
        {assign.isError && <span role="alert" className="text-sm text-danger">Could not assign.</span>}
      </div>

      <table className="w-full text-sm">
        <thead>
          <tr className="text-left text-text-muted">
            <th className="pb-2">Product</th>
            <th className="pb-2">To pick</th>
            <th className="pb-2">Picked</th>
            <th className="pb-2">Record</th>
          </tr>
        </thead>
        <tbody>
          {pickList.lines.map((line) => (
            <tr key={line.id} className="border-t border-border">
              <td className="py-2">{productOptions.options.find((p) => p.id === line.productId)?.label ?? line.productId}</td>
              <td className="py-2">{line.quantityToPick}</td>
              <td className="py-2">{line.quantityPicked}</td>
              <td className="py-2">
                {isPacked ? (
                  <span className="text-text-muted">—</span>
                ) : (
                  <div className="flex items-center gap-2">
                    <input
                      className="w-24 rounded-md border border-border bg-surface px-2 py-1"
                      placeholder="Qty"
                      value={countInputs[line.id] ?? ''}
                      onChange={(e) => setCountInputs((prev) => ({ ...prev, [line.id]: e.target.value }))}
                    />
                    <Button
                      type="button"
                      disabled={
                        !countInputs[line.id] ||
                        Number(countInputs[line.id]) < 0 ||
                        !pickList.assignedToUserId ||
                        (recordPick.isPending && recordPick.variables?.lineId === line.id)
                      }
                      onClick={() => recordPick.mutate({ lineId: line.id, quantityPicked: Number(countInputs[line.id]) })}
                    >
                      Record
                    </Button>
                  </div>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      {recordPick.isError && <p role="alert" className="mt-2 text-sm text-danger">Could not record that pick.</p>}

      <div className="mt-4 flex items-center gap-3">
        <Button type="button" disabled={pickList.status !== 'Picked' || pack.isPending} onClick={() => pack.mutate()}>
          {isPacked ? 'Packed' : 'Confirm packed'}
        </Button>
        {pack.isError && <span role="alert" className="text-sm text-danger">Could not pack — every line must be fully picked first.</span>}
      </div>
    </Card>
  );
}
