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
import { useProductOptions, useSupplierOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const lineSchema = z.object({
  productId: z.string().uuid('Pick a product'),
  quantity: z.string().refine((v) => Number(v) > 0, 'Quantity must be greater than zero'),
});

const schema = z.object({
  lines: z.array(lineSchema).min(1, 'At least one line is required'),
});
type FormValues = z.infer<typeof schema>;

interface RfqLineDto {
  id: string;
  productId: string;
  quantity: number;
}

interface SupplierQuoteLineDto {
  id: string;
  productId: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

interface SupplierQuoteDto {
  id: string;
  supplierId: string;
  submittedAt: string;
  totalAmount: number;
  lines: SupplierQuoteLineDto[];
}

interface RfqDto {
  id: string;
  status: string;
  rfqDate: string;
  awardedSupplierQuoteId: string | null;
  convertedPurchaseOrderId: string | null;
  lines: RfqLineDto[];
  supplierQuotes: SupplierQuoteDto[];
}

interface RfqQuoteComparisonCellDto {
  supplierId: string;
  unitPrice: number;
  lineTotal: number;
}

interface RfqQuoteComparisonLineDto {
  rfqLineId: string;
  productId: string;
  requestedQuantity: number;
  quotes: RfqQuoteComparisonCellDto[];
}

interface RfqQuoteComparisonSupplierDto {
  supplierQuoteId: string;
  supplierId: string;
  supplierName: string | null;
  totalAmount: number;
}

interface RfqQuoteComparisonDto {
  rfqId: string;
  status: string;
  suppliers: RfqQuoteComparisonSupplierDto[];
  lines: RfqQuoteComparisonLineDto[];
}

/**
 * RFQ — the pre-PO stage named in PurchaseOrder's own doc comment as coming
 * "later" (docs/IMPLEMENTATION_PLAN.md Phase 10 item 1). Structurally the
 * closest analogue to QuotationsPanel: Create → Send → (multiple suppliers
 * submit quotes) → Award the winner → Convert to a real PurchaseOrder.
 */
export function RfqsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const productOptions = useProductOptions(companyId);
  const supplierOptions = useSupplierOptions(companyId);

  const { control, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { lines: [{ productId: '', quantity: '1' }] },
  });
  const { fields, append, remove } = useFieldArray({ control, name: 'lines' });

  const rfqsQuery = useQuery({
    queryKey: ['rfqs', companyId],
    queryFn: () => apiClient.get<PagedResult<RfqDto>>(`/procurement/rfqs?companyId=${companyId}&page=1&pageSize=25`),
    enabled: Boolean(companyId),
  });

  const createRfq = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<RfqDto>('/procurement/rfqs', {
        companyId,
        lines: values.lines.map((l) => ({ productId: l.productId, quantity: Number(l.quantity) })),
      }),
    onSuccess: () => {
      reset({ lines: [{ productId: '', quantity: '1' }] });
      queryClient.invalidateQueries({ queryKey: ['rfqs', companyId] });
    },
  });

  const invalidateRfqs = () => queryClient.invalidateQueries({ queryKey: ['rfqs', companyId] });

  const sendRfq = useMutation({
    mutationFn: (id: string) => apiClient.post(`/procurement/rfqs/${id}/send?companyId=${companyId}`, {}),
    onSuccess: invalidateRfqs,
  });

  const submitQuote = useMutation({
    mutationFn: (vars: { rfqId: string; supplierId: string; lines: { productId: string; quantity: number; unitPrice: number }[] }) =>
      apiClient.post(`/procurement/rfqs/${vars.rfqId}/quotes?companyId=${companyId}`, {
        supplierId: vars.supplierId,
        lines: vars.lines,
      }),
    onSuccess: invalidateRfqs,
  });

  const awardRfq = useMutation({
    mutationFn: (vars: { rfqId: string; supplierQuoteId: string }) =>
      apiClient.post(`/procurement/rfqs/${vars.rfqId}/award?companyId=${companyId}`, { supplierQuoteId: vars.supplierQuoteId }),
    onSuccess: invalidateRfqs,
  });

  const convertRfq = useMutation({
    mutationFn: (id: string) => apiClient.post(`/procurement/rfqs/${id}/convert?companyId=${companyId}`, {}),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['rfqs', companyId] });
      queryClient.invalidateQueries({ queryKey: ['purchase-orders', companyId] });
    },
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">RFQs</h2>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createRfq.mutate(values))} className="flex flex-col gap-4">
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
                  Quantity
                  <Controller
                    control={control}
                    name={`lines.${index}.quantity`}
                    render={({ field: lineField }) => (
                      <input className="w-24 rounded-md border border-border bg-surface px-2 py-1.5" {...lineField} />
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
            <Button type="button" variant="secondary" onClick={() => append({ productId: '', quantity: '1' })} className="w-fit">
              <Plus size={16} className="mr-1" /> Add line
            </Button>
          </div>

          <Button type="submit" disabled={isSubmitting} className="w-fit">
            {isSubmitting ? 'Creating…' : 'Create RFQ'}
          </Button>
        </form>
      </Card>

      {rfqsQuery.data && (
        <Card>
          <DataTable
            columns={[
              { header: 'RFQ date', render: (rfq: RfqDto) => new Date(rfq.rfqDate).toLocaleDateString() },
              { header: 'Status', render: (rfq: RfqDto) => rfq.status },
              { header: 'Lines', render: (rfq: RfqDto) => rfq.lines.length },
              { header: 'Quotes received', render: (rfq: RfqDto) => rfq.supplierQuotes.length },
              {
                header: '',
                render: (rfq: RfqDto) => (
                  <RfqRowActions
                    rfq={rfq}
                    companyId={companyId}
                    supplierOptions={supplierOptions}
                    productOptions={productOptions}
                    onSend={() => sendRfq.mutate(rfq.id)}
                    onSubmitQuote={(supplierId, lines) => submitQuote.mutate({ rfqId: rfq.id, supplierId, lines })}
                    onAward={(supplierQuoteId) => awardRfq.mutate({ rfqId: rfq.id, supplierQuoteId })}
                    onConvert={() => convertRfq.mutate(rfq.id)}
                  />
                ),
              },
            ]}
            rows={rfqsQuery.data.data}
            isLoading={rfqsQuery.isLoading}
            emptyMessage="No RFQs yet."
            rowKey={(rfq) => rfq.id}
          />
        </Card>
      )}
      {createRfq.isError && createRfq.error instanceof ApiError && (
        <p role="alert" className="mt-2 text-sm text-danger">{createRfq.error.problem.title}</p>
      )}
    </div>
  );
}

interface RfqRowActionsProps {
  rfq: RfqDto;
  companyId: string;
  supplierOptions: ReturnType<typeof useSupplierOptions>;
  productOptions: ReturnType<typeof useProductOptions>;
  onSend: () => void;
  onSubmitQuote: (supplierId: string, lines: { productId: string; quantity: number; unitPrice: number }[]) => void;
  onAward: (supplierQuoteId: string) => void;
  onConvert: () => void;
}

/**
 * Per-row actions, split out of the DataTable column definition since each
 * status needs its own small piece of local state (the quote sub-form's
 * open/closed toggle and per-line prices, the award selection) — a table
 * column render function has nowhere else to keep that state.
 */
function RfqRowActions({ rfq, companyId, supplierOptions, productOptions, onSend, onSubmitQuote, onAward, onConvert }: RfqRowActionsProps) {
  const [quoting, setQuoting] = useState(false);
  const [supplierId, setSupplierId] = useState('');
  const [prices, setPrices] = useState<Record<string, string>>({});
  const [selectedQuoteId, setSelectedQuoteId] = useState('');

  if (rfq.status === 'Draft') {
    return <Button variant="secondary" onClick={onSend}>Send</Button>;
  }

  if (rfq.status === 'Sent') {
    return (
      <div className="flex flex-col gap-2">
        {!quoting && (
          <Button variant="secondary" onClick={() => setQuoting(true)}>Submit supplier quote</Button>
        )}
        {quoting && (
          <div className="flex flex-col gap-2 rounded-md border border-border p-2">
            <label className="flex flex-col gap-1 text-xs">
              Supplier
              <EntityCombobox
                className="w-64"
                value={supplierId}
                onChange={setSupplierId}
                options={supplierOptions.options}
                isLoading={supplierOptions.isLoading}
                onSearchChange={supplierOptions.onSearchChange}
                placeholder="Search suppliers…"
              />
            </label>
            {rfq.lines.map((line) => (
              <label key={line.id} className="flex items-center justify-between gap-2 text-xs">
                <span>Product {line.productId.slice(0, 8)}… (qty {line.quantity})</span>
                <input
                  className="w-24 rounded-md border border-border bg-surface px-2 py-1"
                  placeholder="Unit price"
                  value={prices[line.productId] ?? ''}
                  onChange={(e) => setPrices((p) => ({ ...p, [line.productId]: e.target.value }))}
                />
              </label>
            ))}
            <div className="flex gap-2">
              <Button
                type="button"
                onClick={() => {
                  const lines = rfq.lines.map((line) => ({
                    productId: line.productId,
                    quantity: line.quantity,
                    unitPrice: Number(prices[line.productId] ?? 0),
                  }));
                  onSubmitQuote(supplierId, lines);
                  setQuoting(false);
                  setSupplierId('');
                  setPrices({});
                }}
                disabled={!supplierId}
              >
                Submit
              </Button>
              <Button type="button" variant="secondary" onClick={() => setQuoting(false)}>Cancel</Button>
            </div>
          </div>
        )}

        {rfq.supplierQuotes.length >= 2 && (
          <QuoteComparisonTable
            companyId={companyId}
            rfqId={rfq.id}
            productOptions={productOptions}
            supplierOptions={supplierOptions}
            onPickQuote={setSelectedQuoteId}
          />
        )}

        {rfq.supplierQuotes.length > 0 && (
          <div className="flex items-center gap-2">
            <select
              className="rounded-md border border-border bg-surface px-2 py-1 text-xs"
              value={selectedQuoteId}
              onChange={(e) => setSelectedQuoteId(e.target.value)}
            >
              <option value="">Pick a quote…</option>
              {rfq.supplierQuotes.map((q) => (
                <option key={q.id} value={q.id}>
                  Supplier {supplierLabel(q.supplierId, supplierOptions)} — {q.totalAmount.toLocaleString()}
                </option>
              ))}
            </select>
            <Button variant="secondary" onClick={() => onAward(selectedQuoteId)} disabled={!selectedQuoteId}>
              Award
            </Button>
          </div>
        )}
      </div>
    );
  }

  if (rfq.status === 'Awarded') {
    return rfq.convertedPurchaseOrderId ? (
      <span className="text-xs text-text-muted">Converted</span>
    ) : (
      <Button variant="secondary" onClick={onConvert}>Convert to PO</Button>
    );
  }

  return null;
}

function productLabel(productId: string, productOptions: ReturnType<typeof useProductOptions>): string {
  return productOptions.options.find((o) => o.id === productId)?.label ?? `Product ${productId.slice(0, 8)}…`;
}

function supplierLabel(supplierId: string, supplierOptions: ReturnType<typeof useSupplierOptions>): string {
  return supplierOptions.options.find((o) => o.id === supplierId)?.label ?? `${supplierId.slice(0, 8)}…`;
}

interface QuoteComparisonTableProps {
  companyId: string;
  rfqId: string;
  productOptions: ReturnType<typeof useProductOptions>;
  supplierOptions: ReturnType<typeof useSupplierOptions>;
  onPickQuote: (supplierQuoteId: string) => void;
}

/**
 * Line-by-line, side-by-side quote comparison — replaces "guess from a
 * dropdown of totals" with the actual per-product prices a decision-maker
 * needs, backed by GetRfqQuoteComparisonQuery (rows = RFQ lines, columns =
 * suppliers, matching the DTO's own row/column choice). Only rendered once
 * 2+ suppliers have quoted; with a single quote there is nothing to compare.
 * Clicking a supplier's total pre-selects that quote in the Award dropdown
 * below, but doesn't award it — Award still requires the explicit button.
 */
function QuoteComparisonTable({ companyId, rfqId, productOptions, supplierOptions, onPickQuote }: QuoteComparisonTableProps) {
  const comparisonQuery = useQuery({
    queryKey: ['rfq-quote-comparison', companyId, rfqId],
    queryFn: () => apiClient.get<RfqQuoteComparisonDto>(`/procurement/rfqs/${rfqId}/quote-comparison?companyId=${companyId}`),
    enabled: Boolean(companyId && rfqId),
  });

  if (comparisonQuery.isLoading) {
    return <p className="text-xs text-text-muted">Loading quote comparison…</p>;
  }
  if (!comparisonQuery.data || comparisonQuery.data.suppliers.length === 0) {
    return null;
  }

  const { suppliers, lines } = comparisonQuery.data;
  const cheapestTotal = Math.min(...suppliers.map((s) => s.totalAmount));

  return (
    <div className="overflow-x-auto rounded-md border border-border">
      <table className="w-full text-xs">
        <thead>
          <tr className="border-b border-border bg-surface-muted">
            <th className="p-2 text-left font-medium">Product</th>
            <th className="p-2 text-right font-medium">Qty</th>
            {suppliers.map((s) => (
              <th key={s.supplierId} className="p-2 text-right font-medium">
                {s.supplierName ?? supplierLabel(s.supplierId, supplierOptions)}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {lines.map((line) => {
            const cellPrices = line.quotes.map((q) => q.unitPrice);
            const cheapestPrice = cellPrices.length > 0 ? Math.min(...cellPrices) : null;
            return (
              <tr key={line.rfqLineId} className="border-b border-border last:border-0">
                <td className="p-2">{productLabel(line.productId, productOptions)}</td>
                <td className="p-2 text-right">{line.requestedQuantity}</td>
                {suppliers.map((s) => {
                  const cell = line.quotes.find((q) => q.supplierId === s.supplierId);
                  const isCheapest = cell !== undefined && cheapestPrice !== null && cell.unitPrice === cheapestPrice;
                  return (
                    <td
                      key={s.supplierId}
                      className={`p-2 text-right ${isCheapest ? 'bg-success/10 font-semibold text-success' : ''}`}
                    >
                      {cell ? cell.unitPrice.toLocaleString() : '—'}
                    </td>
                  );
                })}
              </tr>
            );
          })}
        </tbody>
        <tfoot>
          <tr className="bg-surface-muted font-semibold">
            <td className="p-2" colSpan={2}>Total</td>
            {suppliers.map((s) => (
              <td key={s.supplierId} className={`p-2 text-right ${s.totalAmount === cheapestTotal ? 'text-success' : ''}`}>
                <button
                  type="button"
                  className="underline decoration-dotted underline-offset-2"
                  onClick={() => onPickQuote(s.supplierQuoteId)}
                  title="Pre-select this supplier's quote in the Award dropdown below"
                >
                  {s.totalAmount.toLocaleString()}
                </button>
                {s.totalAmount === cheapestTotal && <span className="ml-1 text-[10px] font-normal">(cheapest)</span>}
              </td>
            ))}
          </tr>
        </tfoot>
      </table>
    </div>
  );
}
