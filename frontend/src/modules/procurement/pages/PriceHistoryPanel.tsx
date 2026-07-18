import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../../../shared/api/client';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { EntityCombobox } from '../../../shared/ui/EntityCombobox';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import { useProductOptions } from '../../../shared/api/entityOptions';

const schema = z.object({
  productId: z.string().uuid('Pick a product'),
});
type FormValues = z.infer<typeof schema>;

interface PriceHistoryLineDto {
  purchaseOrderId: string;
  supplierId: string;
  orderDate: string;
  unitPrice: number;
  quantity: number;
}

/**
 * Price History — every historical unit price paid for a Product across all
 * Purchase Orders, closing the "Price history" gap in Phase 1's Procurement
 * scope (05_MODULE_ROADMAP.md). A canned report over existing PurchaseOrder
 * data (no new aggregate), so this is a lookup panel rather than a form —
 * pick a product, see its price trend oldest-first.
 */
export function PriceHistoryPanel() {
  const { companyId } = useActiveCompany();
  const productOptions = useProductOptions(companyId);

  const { control, watch } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { productId: '' },
  });
  const productId = watch('productId');

  const priceHistoryQuery = useQuery({
    queryKey: ['price-history', companyId, productId],
    queryFn: () => apiClient.get<PriceHistoryLineDto[]>(`/procurement/reports/price-history?companyId=${companyId}&productId=${productId}`),
    enabled: Boolean(companyId && productId),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Price History</h2>

      <Card className="mb-6">
        <label className="flex max-w-md flex-col gap-1 text-sm">
          Product
          <Controller
            control={control}
            name="productId"
            render={({ field }) => (
              <EntityCombobox
                value={field.value}
                onChange={field.onChange}
                options={productOptions.options}
                isLoading={productOptions.isLoading}
                onSearchChange={productOptions.onSearchChange}
                placeholder="Search products…"
              />
            )}
          />
        </label>
      </Card>

      {productId && (
        <Card>
          <DataTable
            columns={[
              { header: 'Order date', render: (row: PriceHistoryLineDto) => new Date(row.orderDate).toLocaleDateString() },
              { header: 'Supplier', render: (row: PriceHistoryLineDto) => row.supplierId.slice(0, 8) + '…' },
              { header: 'Unit price', render: (row: PriceHistoryLineDto) => row.unitPrice.toLocaleString() },
              { header: 'Quantity', render: (row: PriceHistoryLineDto) => row.quantity.toLocaleString() },
            ]}
            rows={priceHistoryQuery.data}
            isLoading={priceHistoryQuery.isLoading}
            isError={priceHistoryQuery.isError}
            errorMessage="Could not load price history."
            emptyMessage="No purchase history for this product yet."
            rowKey={(row) => row.purchaseOrderId}
          />
        </Card>
      )}
    </div>
  );
}
