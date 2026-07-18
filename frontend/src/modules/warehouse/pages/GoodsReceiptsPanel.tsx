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
  usePurchaseOrderOptions,
  useProductOptions,
  useSupplierOptions,
  useWarehouseOptions,
  useZoneOptions,
} from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const lineSchema = z.object({
  productId: z.string().uuid('Pick a product'),
  quantityReceived: z.string().refine((v) => Number(v) > 0, 'Quantity must be greater than zero'),
  unitCost: z.string().refine((v) => v === '' || Number(v) >= 0, 'Unit cost cannot be negative'),
  batchNumber: z.string().max(100, 'Batch number cannot exceed 100 characters'),
  serialNumber: z.string().max(100, 'Serial number cannot exceed 100 characters'),
});

const schema = z.object({
  warehouseId: z.string().uuid('Pick a warehouse'),
  zoneId: z.string().uuid('Pick a zone'),
  purchaseOrderId: z.string().refine((v) => v === '' || /^[0-9a-fA-F-]{36}$/.test(v), 'Must be blank or a valid Purchase Order'),
  supplierId: z.string().refine((v) => v === '' || /^[0-9a-fA-F-]{36}$/.test(v), 'Must be blank or a valid Supplier'),
  lines: z.array(lineSchema).min(1, 'At least one line is required'),
});
type FormValues = z.infer<typeof schema>;

interface GoodsReceiptLineDto {
  id: string;
  productId: string;
  quantityReceived: number;
  unitCost: number | null;
  batchNumber: string | null;
  serialNumber: string | null;
  suggestedBinId: string | null;
  putAwayBinId: string | null;
  isPutAway: boolean;
}

interface GoodsReceiptDto {
  id: string;
  warehouseId: string;
  zoneId: string;
  purchaseOrderId: string | null;
  supplierId: string | null;
  receivedDate: string;
  lines: GoodsReceiptLineDto[];
}

// M9-remaining e: Multi-UOM — minimal shape needed to read a product's
// registered alternate units for the receiving-line conversion helper below.
interface ProductUnitOfMeasureDetailDto {
  unitOfMeasure: string;
  unitOfMeasureConversions: { alternateUnitOfMeasure: string; conversionFactor: number }[];
}

/**
 * Goods Receipts — next slice after Zones (03_SYSTEM_ARCHITECTURE.md §4.2 event
 * catalog: "GoodsReceived.v1", produced by Warehouse). Warehouse, Zone,
 * Purchase Order, Supplier, and each line's Product are all picked via the
 * shared EntityCombobox rather than pasted raw ids. Recording a receipt here
 * automatically credits the Inventory Stock Ledger via
 * GoodsReceiptLineReceivedConsumer once the Kafka consumer host is running —
 * no manual duplicate Stock Adjustment needed anymore.
 */
export function GoodsReceiptsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [lookupWarehouseId, setLookupWarehouseId] = useState('');
  const [managingReceiptId, setManagingReceiptId] = useState<string | null>(null);

  const warehouseOptions = useWarehouseOptions(companyId);
  const purchaseOrderOptions = usePurchaseOrderOptions(companyId);
  const supplierOptions = useSupplierOptions(companyId);
  const productOptions = useProductOptions(companyId);

  const { control, handleSubmit, watch, reset, setValue, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { warehouseId: '', zoneId: '', purchaseOrderId: '', supplierId: '', lines: [{ productId: '', quantityReceived: '1', unitCost: '', batchNumber: '', serialNumber: '' }] },
  });
  const { fields, append, remove } = useFieldArray({ control, name: 'lines' });
  const watchedWarehouseId = watch('warehouseId');
  const effectiveWarehouseId = lookupWarehouseId || watchedWarehouseId;
  const zoneOptions = useZoneOptions(companyId, effectiveWarehouseId || undefined);

  const receiptsQuery = useQuery({
    queryKey: ['goods-receipts', companyId, effectiveWarehouseId],
    queryFn: () =>
      apiClient.get<PagedResult<GoodsReceiptDto>>(
        `/warehouse/warehouses/${effectiveWarehouseId}/goods-receipts?companyId=${companyId}&page=1&pageSize=25`,
      ),
    enabled: Boolean(companyId && effectiveWarehouseId),
  });

  const createReceipt = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<GoodsReceiptDto>(`/warehouse/warehouses/${values.warehouseId}/goods-receipts`, {
        companyId,
        zoneId: values.zoneId,
        purchaseOrderId: values.purchaseOrderId || null,
        supplierId: values.supplierId || null,
        lines: values.lines.map((l) => ({
          productId: l.productId,
          quantityReceived: Number(l.quantityReceived),
          unitCost: l.unitCost === '' ? null : Number(l.unitCost),
          batchNumber: l.batchNumber === '' ? null : l.batchNumber,
          serialNumber: l.serialNumber === '' ? null : l.serialNumber,
        })),
      }),
    onSuccess: (_data, variables) => {
      reset({ warehouseId: variables.warehouseId, zoneId: '', purchaseOrderId: '', supplierId: '', lines: [{ productId: '', quantityReceived: '1', unitCost: '', batchNumber: '', serialNumber: '' }] });
      setLookupWarehouseId(variables.warehouseId);
      queryClient.invalidateQueries({ queryKey: ['goods-receipts', companyId, variables.warehouseId] });
    },
  });

  if (!companyId) return null;

  const managingReceipt = receiptsQuery.data?.data.find((r) => r.id === managingReceiptId) ?? null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Goods Receipts</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createReceipt.mutate(values))} className="flex flex-col gap-4">
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
              Zone
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
              {errors.zoneId && <span className="text-xs text-danger">{errors.zoneId.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Purchase Order (optional)
              <Controller
                control={control}
                name="purchaseOrderId"
                render={({ field }) => (
                  <EntityCombobox
                    value={field.value}
                    onChange={field.onChange}
                    options={purchaseOrderOptions.options}
                    isLoading={purchaseOrderOptions.isLoading}
                    placeholder="Search purchase orders…"
                  />
                )}
              />
              {errors.purchaseOrderId && <span className="text-xs text-danger">{errors.purchaseOrderId.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Supplier (optional)
              <Controller
                control={control}
                name="supplierId"
                render={({ field }) => (
                  <EntityCombobox
                    value={field.value}
                    onChange={field.onChange}
                    options={supplierOptions.options}
                    isLoading={supplierOptions.isLoading}
                    onSearchChange={supplierOptions.onSearchChange}
                    placeholder="Search suppliers…"
                  />
                )}
              />
              {errors.supplierId && <span className="text-xs text-danger">{errors.supplierId.message}</span>}
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
                  Quantity received
                  <Controller
                    control={control}
                    name={`lines.${index}.quantityReceived`}
                    render={({ field: lineField }) => (
                      <input className="w-32 rounded-md border border-border bg-surface px-2 py-1.5" {...lineField} />
                    )}
                  />
                </label>
                <LineUnitConversionHelper
                  companyId={companyId}
                  productId={watch(`lines.${index}.productId`)}
                  onApply={(quantityInBaseUnit) => setValue(`lines.${index}.quantityReceived`, quantityInBaseUnit)}
                />
                <label className="flex flex-col gap-1 text-sm">
                  Unit cost (optional)
                  <Controller
                    control={control}
                    name={`lines.${index}.unitCost`}
                    render={({ field: lineField }) => (
                      <input className="w-32 rounded-md border border-border bg-surface px-2 py-1.5" {...lineField} />
                    )}
                  />
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  Batch/lot no. (optional)
                  <Controller
                    control={control}
                    name={`lines.${index}.batchNumber`}
                    render={({ field: lineField }) => (
                      <input className="w-32 rounded-md border border-border bg-surface px-2 py-1.5" {...lineField} />
                    )}
                  />
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  Serial no. (optional)
                  <Controller
                    control={control}
                    name={`lines.${index}.serialNumber`}
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
            <Button type="button" variant="secondary" onClick={() => append({ productId: '', quantityReceived: '1', unitCost: '', batchNumber: '', serialNumber: '' })} className="w-fit">
              <Plus size={16} className="mr-1" /> Add line
            </Button>
          </div>

          <Button type="submit" disabled={isSubmitting} className="w-fit">
            {isSubmitting ? 'Recording…' : 'Record goods receipt'}
          </Button>
        </form>
      </Card>

      {receiptsQuery.data && (
        <Card>
          <DataTable
            columns={[
              { header: 'Received', render: (receipt: GoodsReceiptDto) => new Date(receipt.receivedDate).toLocaleString() },
              { header: 'Zone', render: (receipt: GoodsReceiptDto) => zoneOptions.options.find((z) => z.id === receipt.zoneId)?.label ?? receipt.zoneId },
              { header: 'Purchase Order', render: (receipt: GoodsReceiptDto) => receipt.purchaseOrderId ?? '—' },
              { header: 'Lines', render: (receipt: GoodsReceiptDto) => receipt.lines.length },
              {
                header: 'Putaway',
                render: (receipt: GoodsReceiptDto) =>
                  `${receipt.lines.filter((l) => l.isPutAway).length}/${receipt.lines.length} put away`,
              },
              {
                header: 'Actions',
                render: (receipt: GoodsReceiptDto) => (
                  <Button type="button" variant="secondary" onClick={() => setManagingReceiptId(receipt.id)}>
                    Manage putaway
                  </Button>
                ),
              },
            ]}
            rows={receiptsQuery.data.data}
            isLoading={receiptsQuery.isLoading}
            emptyMessage="No goods receipts yet for this warehouse."
            rowKey={(receipt) => receipt.id}
          />
        </Card>
      )}
      {createReceipt.isError && createReceipt.error instanceof ApiError && (
        <p role="alert" className="mt-2 text-sm text-danger">{createReceipt.error.problem.title}</p>
      )}

      {managingReceipt && (
        <GoodsReceiptPutawayPanel
          companyId={companyId}
          warehouseId={effectiveWarehouseId}
          receipt={managingReceipt}
          onClose={() => setManagingReceiptId(null)}
        />
      )}
    </div>
  );
}

interface LineUnitConversionHelperProps {
  companyId: string;
  productId: string;
  onApply: (quantityInBaseUnit: string) => void;
}

// M9-remaining e: Multi-UOM — lets a receiving clerk enter the quantity in
// whatever alternate unit the goods actually arrived in (e.g. "3 BOX") and
// have it converted into the product's base UOM for the line's
// quantityReceived field, since GoodsReceiptLine itself only ever stores a
// base-UOM quantity (ledger/costing consistency — see
// ProductUnitOfMeasureConversion.cs). The conversion math happens here, at
// the point the alternate-UOM quantity is captured, per this codebase's
// "caller does cross-module math" convention — there is no synchronous
// inter-module call into Inventory from Warehouse.
function LineUnitConversionHelper({ companyId, productId, onApply }: LineUnitConversionHelperProps) {
  const [alternateUnit, setAlternateUnit] = useState('');
  const [alternateQuantity, setAlternateQuantity] = useState('');

  const productQuery = useQuery({
    queryKey: ['product-uom-detail', companyId, productId],
    queryFn: () => apiClient.get<ProductUnitOfMeasureDetailDto>(`/inventory/products/${productId}?companyId=${companyId}`),
    enabled: Boolean(companyId && productId),
  });

  const conversions = productQuery.data?.unitOfMeasureConversions ?? [];
  if (!productId || conversions.length === 0) return null;

  const selectedConversion = conversions.find((c) => c.alternateUnitOfMeasure === alternateUnit);

  return (
    <label className="flex flex-col gap-1 text-sm">
      Receive in alt. unit (optional)
      <div className="flex gap-1">
        <select
          className="rounded-md border border-border bg-surface px-2 py-1.5"
          value={alternateUnit}
          onChange={(e) => {
            setAlternateUnit(e.target.value);
            setAlternateQuantity('');
          }}
        >
          <option value="">{productQuery.data?.unitOfMeasure ?? 'Base unit'}</option>
          {conversions.map((c) => (
            <option key={c.alternateUnitOfMeasure} value={c.alternateUnitOfMeasure}>
              {c.alternateUnitOfMeasure} (= {c.conversionFactor} {productQuery.data?.unitOfMeasure})
            </option>
          ))}
        </select>
        {alternateUnit && (
          <input
            className="w-20 rounded-md border border-border bg-surface px-2 py-1.5"
            placeholder="Qty"
            value={alternateQuantity}
            onChange={(e) => {
              setAlternateQuantity(e.target.value);
              const parsed = Number(e.target.value);
              if (selectedConversion && !Number.isNaN(parsed)) {
                onApply(String(parsed * selectedConversion.conversionFactor));
              }
            }}
          />
        )}
      </div>
    </label>
  );
}

interface GoodsReceiptPutawayPanelProps {
  companyId: string;
  warehouseId: string;
  receipt: GoodsReceiptDto;
  onClose: () => void;
}

/**
 * Putaway (docs/IMPLEMENTATION_PLAN.md item 12: "a suggested/confirmed putaway
 * location on Goods Receipt") — the last remaining item of Phase M9's WMS-depth
 * scope. "Suggest" asks the backend for its placeholder first-active-bin-in-zone
 * heuristic (GoodsReceiptsController's own doc comment explains it isn't a real
 * slotting algorithm); "Confirm" always requires an explicit bin pick via
 * EntityCombobox, whether or not a suggestion was ever requested — a worker can
 * freely override the suggestion, same restraint as PickList's Assign/Record.
 */
function GoodsReceiptPutawayPanel({ companyId, warehouseId, receipt, onClose }: GoodsReceiptPutawayPanelProps) {
  const queryClient = useQueryClient();
  const productOptions = useProductOptions(companyId);
  const binOptions = useBinOptions(companyId, warehouseId, receipt.zoneId);
  const [confirmBinInputs, setConfirmBinInputs] = useState<Record<string, string>>({});

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['goods-receipts', companyId, warehouseId] });

  const suggest = useMutation({
    mutationFn: (lineId: string) =>
      apiClient.post<GoodsReceiptDto>(`/warehouse/warehouses/${warehouseId}/goods-receipts/${receipt.id}/lines/${lineId}/suggest-putaway`, { companyId }),
    onSuccess: invalidate,
  });

  const confirm = useMutation({
    mutationFn: ({ lineId, binId }: { lineId: string; binId: string }) =>
      apiClient.post<GoodsReceiptDto>(`/warehouse/warehouses/${warehouseId}/goods-receipts/${receipt.id}/lines/${lineId}/confirm-putaway`, { companyId, binId }),
    onSuccess: invalidate,
  });

  const binLabel = (binId: string | null) => (binId ? binOptions.options.find((b) => b.id === binId)?.label ?? binId : '—');

  return (
    <Card className="mt-6">
      <div className="mb-3 flex items-center justify-between">
        <h2 className="text-lg font-semibold text-text">Manage putaway</h2>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>

      <table className="w-full text-sm">
        <thead>
          <tr className="text-left text-text-muted">
            <th className="pb-2">Product</th>
            <th className="pb-2">Qty received</th>
            <th className="pb-2">Batch/Serial</th>
            <th className="pb-2">Suggested bin</th>
            <th className="pb-2">Put-away bin</th>
            <th className="pb-2">Actions</th>
          </tr>
        </thead>
        <tbody>
          {receipt.lines.map((line) => (
            <tr key={line.id} className="border-t border-border">
              <td className="py-2">{productOptions.options.find((p) => p.id === line.productId)?.label ?? line.productId}</td>
              <td className="py-2">{line.quantityReceived}</td>
              <td className="py-2">{line.batchNumber ?? line.serialNumber ?? '—'}</td>
              <td className="py-2">{binLabel(line.suggestedBinId)}</td>
              <td className="py-2">{binLabel(line.putAwayBinId)}</td>
              <td className="py-2">
                <div className="flex items-center gap-2">
                  <Button
                    type="button"
                    variant="secondary"
                    disabled={suggest.isPending && suggest.variables === line.id}
                    onClick={() => suggest.mutate(line.id)}
                  >
                    Suggest
                  </Button>
                  <EntityCombobox
                    className="w-48"
                    value={confirmBinInputs[line.id] ?? ''}
                    onChange={(id) => setConfirmBinInputs((prev) => ({ ...prev, [line.id]: id }))}
                    options={binOptions.options}
                    isLoading={binOptions.isLoading}
                    placeholder="Pick a bin…"
                  />
                  <Button
                    type="button"
                    disabled={!confirmBinInputs[line.id] || (confirm.isPending && confirm.variables?.lineId === line.id)}
                    onClick={() => confirm.mutate({ lineId: line.id, binId: confirmBinInputs[line.id] })}
                  >
                    {line.isPutAway ? 'Re-confirm' : 'Confirm'}
                  </Button>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      {suggest.isError && <p role="alert" className="mt-2 text-sm text-danger">Could not suggest a bin — is there an active bin in this receipt's zone?</p>}
      {confirm.isError && <p role="alert" className="mt-2 text-sm text-danger">Could not confirm putaway — pick a bin that belongs to this receipt's zone.</p>}
    </Card>
  );
}
