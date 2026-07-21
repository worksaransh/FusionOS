import { useEffect, useRef, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { CrudListPage } from '../../../shared/ui/CrudListPage';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import type { PagedResult } from '../../../shared/api/types';
import { BarcodeLabel } from '../../../shared/barcode/BarcodeLabel';
import { StockLedgerPanel } from './StockLedgerPanel';
import { InventoryValuationPanel } from './InventoryValuationPanel';
import { ReservationsPanel } from './ReservationsPanel';
import { TransfersPanel } from './TransfersPanel';

const schema = z.object({
  sku: z.string().min(1, 'SKU is required').max(50),
  name: z.string().min(1, 'Name is required').max(200),
  unitOfMeasure: z.string().min(1, 'Unit of measure is required').max(20),
  description: z.string().optional(),
});
type FormValues = z.infer<typeof schema>;

// Update command deliberately excludes Sku — it's the immutable business key
// (see UpdateProductCommand's doc comment: "Sku is intentionally not editable
// here — see Product.UpdateDetails").
const editSchema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  unitOfMeasure: z.string().min(1, 'Unit of measure is required').max(20),
  description: z.string().optional(),
});
type EditFormValues = z.infer<typeof editSchema>;

interface UnitOfMeasureConversionDto {
  alternateUnitOfMeasure: string;
  conversionFactor: number;
}

interface ProductVariantDto {
  id: string;
  variantSku: string;
  attributes: string;
  isActive: boolean;
}

interface ProductDto {
  id: string;
  sku: string;
  name: string;
  description: string | null;
  unitOfMeasure: string;
  isActive: boolean;
  createdAt: string;
  unitOfMeasureConversions: UnitOfMeasureConversionDto[];
  variants: ProductVariantDto[];
  barcode: string | null;
}

// Barcode/QR support (2026-07-21). Barcode itself is any string (a QR payload could be a
// case-sensitive URL, for instance) — the backend doesn't restrict its charset, so this schema
// doesn't either. Code 39's narrower charset is only enforced where it matters: by BarcodeLabel
// when it tries to render a scannable image (shared/barcode/code39.ts).
const barcodeSchema = z.object({
  barcode: z.string().max(64, 'Must be 64 characters or fewer').optional(),
});
type BarcodeFormValues = z.infer<typeof barcodeSchema>;

// M9-remaining e: Multi-UOM. This is a small standalone schema/form for the
// add-conversion input inside ProductEditPanel — kept separate from
// editSchema/EditFormValues since it edits a child collection, not the
// product's own scalar fields.
const uomConversionSchema = z.object({
  alternateUnitOfMeasure: z.string().min(1, 'Required').max(20),
  conversionFactor: z
    .string()
    .refine((v) => !Number.isNaN(Number(v)) && Number(v) > 0, 'Must be greater than zero'),
});
type UomConversionFormValues = z.infer<typeof uomConversionSchema>;

// Phase 1 closeout (2026-07-18): Variants. Small standalone schema/form for
// the add-variant input inside ProductEditPanel, same separation from
// editSchema as uomConversionSchema above.
const variantSchema = z.object({
  variantSku: z.string().min(1, 'Required').max(50),
  attributes: z.string().min(1, 'Required').max(500),
});
type VariantFormValues = z.infer<typeof variantSchema>;

/** Phase 1 slice — see backend/src/Modules/Inventory for the full CQRS handler. */
export function ProductsPage() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [editingProductId, setEditingProductId] = useState<string | null>(null);
  // Barcode/QR support (2026-07-21) — a barcode lookup can find a product outside the current
  // list page (products list is paginated), so the matched product is held here rather than
  // relying solely on `productsQuery`'s current page to contain it.
  const [scannedProduct, setScannedProduct] = useState<ProductDto | null>(null);
  const editPanelRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    if (editingProductId) {
      editPanelRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }, [editingProductId]);

  const productsQuery = useQuery({
    queryKey: ['products', companyId],
    queryFn: () => apiClient.get<PagedResult<ProductDto>>(`/inventory/products?companyId=${companyId}&page=1&pageSize=25`),
    enabled: Boolean(companyId),
  });

  const { register, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
  });

  const createProduct = useMutation({
    mutationFn: (values: FormValues) => apiClient.post<ProductDto>('/inventory/products', { companyId, ...values }),
    onSuccess: () => {
      reset();
      queryClient.invalidateQueries({ queryKey: ['products', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  // Soft-deactivate only — hits POST /{id}/deactivate, never a DELETE
  // (apiClient has no `delete` method by design; see shared/api/client.ts).
  const deactivateProduct = useMutation({
    mutationFn: (id: string) => apiClient.post(`/inventory/products/${id}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['products', companyId] }),
  });

  if (!companyId) {
    return <p className="text-text-muted">Set an active Company ID in the header above to manage products.</p>;
  }

  return (
    <div>
      <BarcodeLookupPanel
        companyId={companyId}
        onFound={(product) => {
          setScannedProduct(product);
          setEditingProductId(product.id);
        }}
      />
      <CrudListPage<ProductDto>
      title="Products"
      description="SKU / reference data — Inventory, Phase 1"
      rows={productsQuery.data?.data}
      isLoading={productsQuery.isLoading}
      isError={productsQuery.isError}
      errorMessage="Could not load products."
      emptyMessage="No products yet — create the first one above."
      rowKey={(row) => row.id}
      columns={[
        { header: 'SKU', render: (row) => row.sku },
        { header: 'Name', render: (row) => row.name },
        { header: 'UoM', render: (row) => row.unitOfMeasure },
        { header: 'Status', render: (row) => (row.isActive ? 'Active' : 'Inactive') },
        { header: 'Created', render: (row) => new Date(row.createdAt).toLocaleDateString() },
        {
          header: 'Actions',
          render: (row) => (
            <div className="flex items-center gap-2">
              <Button type="button" variant="secondary" onClick={() => setEditingProductId(row.id)}>
                Edit
              </Button>
              <Button
                type="button"
                variant="danger"
                disabled={!row.isActive || (deactivateProduct.isPending && deactivateProduct.variables === row.id)}
                onClick={() => deactivateProduct.mutate(row.id)}
              >
                {row.isActive ? 'Deactivate' : 'Deactivated'}
              </Button>
            </div>
          ),
        },
      ]}
      form={
        <form onSubmit={handleSubmit((values) => createProduct.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            SKU
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('sku')} />
            {errors.sku && <span className="text-xs text-danger">{errors.sku.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Name
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('name')} />
            {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Unit of measure
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="PCS" {...register('unitOfMeasure')} />
            {errors.unitOfMeasure && <span className="text-xs text-danger">{errors.unitOfMeasure.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Description (optional)
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('description')} />
          </label>
          <div className="col-span-2">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create product'}</Button>
          </div>
        </form>
      }
      />
      {deactivateProduct.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that product.</p>
      )}

      {editingProductId && (
        <div ref={editPanelRef}>
          <ProductEditPanel
            companyId={companyId}
            product={
              productsQuery.data?.data.find((p) => p.id === editingProductId) ??
              (scannedProduct?.id === editingProductId ? scannedProduct : null)
            }
            onClose={() => {
              setEditingProductId(null);
              setScannedProduct(null);
            }}
          />
        </div>
      )}

      <StockLedgerPanel />
      <InventoryValuationPanel />
      <ReservationsPanel />
      <TransfersPanel />
    </div>
  );
}

interface ProductEditPanelProps {
  companyId: string;
  product: ProductDto | null;
  onClose: () => void;
}

function ProductEditPanel({ companyId, product, onClose }: ProductEditPanelProps) {
  const queryClient = useQueryClient();

  const { register, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    values: product
      ? { name: product.name, unitOfMeasure: product.unitOfMeasure, description: product.description ?? '' }
      : undefined,
  });

  const updateProduct = useMutation({
    mutationFn: (values: EditFormValues) =>
      apiClient.put<ProductDto>(`/inventory/products/${product!.id}`, { companyId, ...values }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products', companyId] });
      onClose();
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof EditFormValues, { message: messages[0] });
        }
      }
    },
  });

  if (!product) return null;

  return (
    <Card className="mt-8">
      <div className="mb-3 flex items-center justify-between">
        <h2 className="text-lg font-semibold text-text">Edit product — {product.sku}</h2>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form onSubmit={handleSubmit((values) => updateProduct.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <label className="flex flex-col gap-1 text-sm">
          Name
          <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('name')} />
          {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Unit of measure
          <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('unitOfMeasure')} />
          {errors.unitOfMeasure && <span className="text-xs text-danger">{errors.unitOfMeasure.message}</span>}
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Description (optional)
          <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('description')} />
        </label>
        <div className="col-span-2 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save changes'}</Button>
          {updateProduct.isError && (
            <span role="alert" className="text-sm text-danger">Could not save that product.</span>
          )}
        </div>
      </form>

      <UnitOfMeasureConversionsPanel companyId={companyId} product={product} />
      <ProductVariantsPanel companyId={companyId} product={product} />
      <ProductBarcodePanel companyId={companyId} product={product} />
    </Card>
  );
}

interface UnitOfMeasureConversionsPanelProps {
  companyId: string;
  product: ProductDto;
}

// M9-remaining e: Multi-UOM — lets a product record alternate units
// ("BOX" = 12 "PCS") so ordering/receiving/dispatching can happen in
// whichever unit is convenient; conversion into the base UOM for
// ledger/costing consistency is done by the caller at the point of use
// (see ProductUnitOfMeasureConversion.cs doc comment — this codebase has
// no synchronous cross-module aggregate reads, so the math lives wherever
// the alternate-UOM quantity is captured, not here).
function UnitOfMeasureConversionsPanel({ companyId, product }: UnitOfMeasureConversionsPanelProps) {
  const queryClient = useQueryClient();

  const { register, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<UomConversionFormValues>({
    resolver: zodResolver(uomConversionSchema),
  });

  const addConversion = useMutation({
    mutationFn: (values: UomConversionFormValues) =>
      apiClient.post<ProductDto>(`/inventory/products/${product.id}/unit-of-measure-conversions`, {
        companyId,
        alternateUnitOfMeasure: values.alternateUnitOfMeasure,
        conversionFactor: Number(values.conversionFactor),
      }),
    onSuccess: () => {
      reset();
      queryClient.invalidateQueries({ queryKey: ['products', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof UomConversionFormValues, { message: messages[0] });
        }
      }
    },
  });

  // Modeled as a POST action, not a DELETE — apiClient has no `delete` method by design.
  const removeConversion = useMutation({
    mutationFn: (alternateUnitOfMeasure: string) =>
      apiClient.post<ProductDto>(`/inventory/products/${product.id}/unit-of-measure-conversions/remove`, { companyId, alternateUnitOfMeasure }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['products', companyId] }),
  });

  return (
    <div className="mt-6 border-t border-border pt-4">
      <h3 className="mb-2 text-sm font-semibold text-text">Alternate units of measure</h3>
      {product.unitOfMeasureConversions.length === 0 ? (
        <p className="text-sm text-text-muted">No alternate units recorded — base unit is {product.unitOfMeasure}.</p>
      ) : (
        <table className="mb-3 w-full text-sm">
          <thead>
            <tr className="text-left text-text-muted">
              <th className="pb-1">Alternate unit</th>
              <th className="pb-1">1 alt. unit =</th>
              <th className="pb-1"></th>
            </tr>
          </thead>
          <tbody>
            {product.unitOfMeasureConversions.map((c) => (
              <tr key={c.alternateUnitOfMeasure} className="border-t border-border">
                <td className="py-1">{c.alternateUnitOfMeasure}</td>
                <td className="py-1">{c.conversionFactor} {product.unitOfMeasure}</td>
                <td className="py-1 text-right">
                  <Button
                    type="button"
                    variant="danger"
                    disabled={removeConversion.isPending && removeConversion.variables === c.alternateUnitOfMeasure}
                    onClick={() => removeConversion.mutate(c.alternateUnitOfMeasure)}
                  >
                    Remove
                  </Button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
      <form
        onSubmit={handleSubmit((values) => addConversion.mutate(values))}
        className="grid grid-cols-1 gap-3 sm:grid-cols-3 sm:items-end"
      >
        <label className="flex flex-col gap-1 text-sm">
          Alternate unit
          <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="BOX" {...register('alternateUnitOfMeasure')} />
          {errors.alternateUnitOfMeasure && <span className="text-xs text-danger">{errors.alternateUnitOfMeasure.message}</span>}
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Conversion factor (in {product.unitOfMeasure})
          <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('conversionFactor')} />
          {errors.conversionFactor && <span className="text-xs text-danger">{errors.conversionFactor.message}</span>}
        </label>
        <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Add / update unit'}</Button>
      </form>
      {addConversion.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not save that unit conversion.</p>
      )}
      {removeConversion.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not remove that unit conversion.</p>
      )}
    </div>
  );
}

interface ProductVariantsPanelProps {
  companyId: string;
  product: ProductDto;
}

// Phase 1 closeout (2026-07-18): Variants — records each sellable variation
// of this product (e.g. a color/size combination) as its own variant SKU.
// Attributes is a free-form description, not a structured attribute set (see
// ProductVariant.cs doc comment); deactivating a variant never removes the
// row (soft-deactivate only, same convention as the product itself).
function ProductVariantsPanel({ companyId, product }: ProductVariantsPanelProps) {
  const queryClient = useQueryClient();

  const { register, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<VariantFormValues>({
    resolver: zodResolver(variantSchema),
  });

  const addVariant = useMutation({
    mutationFn: (values: VariantFormValues) =>
      apiClient.post<ProductDto>(`/inventory/products/${product.id}/variants`, {
        companyId,
        variantSku: values.variantSku,
        attributes: values.attributes,
      }),
    onSuccess: () => {
      reset();
      queryClient.invalidateQueries({ queryKey: ['products', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof VariantFormValues, { message: messages[0] });
        }
      }
    },
  });

  const deactivateVariant = useMutation({
    mutationFn: (variantId: string) =>
      apiClient.post<ProductDto>(`/inventory/products/${product.id}/variants/${variantId}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['products', companyId] }),
  });

  return (
    <div className="mt-6 border-t border-border pt-4">
      <h3 className="mb-2 text-sm font-semibold text-text">Variants</h3>
      {product.variants.length === 0 ? (
        <p className="text-sm text-text-muted">No variants recorded yet.</p>
      ) : (
        <table className="mb-3 w-full text-sm">
          <thead>
            <tr className="text-left text-text-muted">
              <th className="pb-1">Variant SKU</th>
              <th className="pb-1">Attributes</th>
              <th className="pb-1">Status</th>
              <th className="pb-1"></th>
            </tr>
          </thead>
          <tbody>
            {product.variants.map((v) => (
              <tr key={v.id} className="border-t border-border">
                <td className="py-1">{v.variantSku}</td>
                <td className="py-1">{v.attributes}</td>
                <td className="py-1">{v.isActive ? 'Active' : 'Inactive'}</td>
                <td className="py-1 text-right">
                  {v.isActive && (
                    <Button
                      type="button"
                      variant="danger"
                      disabled={deactivateVariant.isPending && deactivateVariant.variables === v.id}
                      onClick={() => deactivateVariant.mutate(v.id)}
                    >
                      Deactivate
                    </Button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
      <form
        onSubmit={handleSubmit((values) => addVariant.mutate(values))}
        className="grid grid-cols-1 gap-3 sm:grid-cols-3 sm:items-end"
      >
        <label className="flex flex-col gap-1 text-sm">
          Variant SKU
          <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder={`${product.sku}-RED-M`} {...register('variantSku')} />
          {errors.variantSku && <span className="text-xs text-danger">{errors.variantSku.message}</span>}
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Attributes
          <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Color: Red, Size: M" {...register('attributes')} />
          {errors.attributes && <span className="text-xs text-danger">{errors.attributes.message}</span>}
        </label>
        <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Add variant'}</Button>
      </form>
      {addVariant.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not save that variant.</p>
      )}
      {deactivateVariant.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that variant.</p>
      )}
    </div>
  );
}

interface BarcodeLookupPanelProps {
  companyId: string;
  onFound: (product: ProductDto) => void;
}

// Barcode/QR support (2026-07-21) — the actual point of this feature for a warehouse worker: a
// USB barcode scanner "gun" behaves exactly like a keyboard (types the encoded characters, then
// Enter), so a plain text input that reacts to Enter is all a scanner integration needs — no
// camera/decoding code required on this end. Hits GET by-barcode/{barcode} (new
// GetProductByBarcodeQuery, gated by the existing "inventory.product.read" permission).
function BarcodeLookupPanel({ companyId, onFound }: BarcodeLookupPanelProps) {
  const [barcodeInput, setBarcodeInput] = useState('');

  const lookup = useMutation({
    mutationFn: (barcode: string) =>
      apiClient.get<ProductDto>(`/inventory/products/by-barcode/${encodeURIComponent(barcode)}?companyId=${companyId}`),
    onSuccess: (product) => onFound(product),
  });

  const notFound = lookup.isError && lookup.error instanceof ApiError && lookup.error.problem.status === 404;
  const trimmedInput = barcodeInput.trim();

  const runLookup = () => {
    if (trimmedInput) lookup.mutate(trimmedInput);
  };

  return (
    <Card className="mb-6">
      <h2 className="mb-1 text-sm font-semibold text-text">Scan / look up by barcode</h2>
      <p className="mb-3 text-xs text-text-muted">
        Scan with a USB barcode scanner (it types the code and presses Enter for you) or type a
        barcode/QR value and press Enter — jumps straight to that product below.
      </p>
      <div className="flex items-end gap-2">
        <label className="flex flex-1 flex-col gap-1 text-sm">
          Barcode
          <input
            className="rounded-md border border-border bg-surface px-2 py-1.5"
            value={barcodeInput}
            onChange={(event) => setBarcodeInput(event.target.value)}
            onKeyDown={(event) => {
              if (event.key === 'Enter') {
                event.preventDefault();
                runLookup();
              }
            }}
          />
        </label>
        <Button type="button" disabled={!trimmedInput || lookup.isPending} onClick={runLookup}>
          {lookup.isPending ? 'Looking up…' : 'Look up'}
        </Button>
      </div>
      {lookup.isSuccess && (
        <p className="mt-2 text-sm text-text">
          Found <strong>{lookup.data.sku}</strong> — {lookup.data.name}. Opened it below.
        </p>
      )}
      {notFound && (
        <p role="alert" className="mt-2 text-sm text-danger">No product matches barcode &quot;{trimmedInput}&quot;.</p>
      )}
      {lookup.isError && !notFound && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not look up that barcode.</p>
      )}
    </Card>
  );
}

interface ProductBarcodePanelProps {
  companyId: string;
  product: ProductDto;
}

// Barcode/QR support (2026-07-21) — assigns/clears the product's single canonical barcode/QR-
// payload value (see Product.AssignBarcode's doc comment for the full scope decision: this
// stores the VALUE and looks up by it, it doesn't render QR images itself). Clearing the field
// and saving sends barcode: null, which AssignProductBarcodeCommand treats as "remove it".
function ProductBarcodePanel({ companyId, product }: ProductBarcodePanelProps) {
  const queryClient = useQueryClient();

  const { register, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<BarcodeFormValues>({
    resolver: zodResolver(barcodeSchema),
    values: { barcode: product.barcode ?? '' },
  });

  const assignBarcode = useMutation({
    mutationFn: (values: BarcodeFormValues) =>
      apiClient.post<ProductDto>(`/inventory/products/${product.id}/barcode`, {
        companyId,
        barcode: values.barcode?.trim() ? values.barcode.trim() : null,
      }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['products', companyId] }),
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof BarcodeFormValues, { message: messages[0] });
        }
      }
    },
  });

  return (
    <div className="mt-6 border-t border-border pt-4">
      <h3 className="mb-2 text-sm font-semibold text-text">Barcode / QR value</h3>
      <p className="mb-3 text-xs text-text-muted">
        FusionOS stores one canonical barcode/QR-payload string per product and can look products
        up by it (e.g. from a USB scanner gun) — see the lookup box above. It does not generate QR
        images itself; encode this same value with your label printer/software for QR or other
        symbologies. A scannable Code 39 barcode (uppercase A-Z, 0-9, and - . $ / + % space only)
        can be previewed below whenever the value fits that character set.
      </p>
      <form onSubmit={handleSubmit((values) => assignBarcode.mutate(values))} className="flex items-end gap-2">
        <label className="flex flex-1 flex-col gap-1 text-sm">
          Barcode value
          <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="e.g. 012345678905" {...register('barcode')} />
          {errors.barcode && <span className="text-xs text-danger">{errors.barcode.message}</span>}
        </label>
        <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save barcode'}</Button>
      </form>
      {assignBarcode.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">
          Could not save that barcode — it may already be assigned to another product.
        </p>
      )}
      {product.barcode && (
        <div className="mt-4">
          <BarcodeLabel value={product.barcode} />
        </div>
      )}
    </div>
  );
}
