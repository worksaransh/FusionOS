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
import { useProductOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  productId: z.string().uuid('Pick a product'),
  minQuantity: z.string().refine((v) => Number(v) > 0, 'Minimum quantity must be greater than zero'),
  discountPercentage: z.string().refine((v) => Number(v) > 0 && Number(v) <= 100, 'Discount must be between 0 and 100'),
});
type FormValues = z.infer<typeof schema>;

interface DiscountRuleDto {
  id: string;
  productId: string;
  minQuantity: number;
  discountPercentage: number;
  isActive: boolean;
}

/**
 * Discount Rules — the tiered, quantity-break half of the "pricing/discount
 * rules engine" (05_MODULE_ROADMAP.md Sales scope; PriceListsPanel already
 * covers the per-customer override-price half). Multiple rules for the same
 * product at different minimum quantities form its tiers, e.g. 10+ units ->
 * 5%, 50+ units -> 10%. This panel only manages the rules themselves — the
 * Sales Order creation flow (SalesOrdersPanel) is where a salesperson looks
 * up the applicable tier for a picked product/quantity as a suggestion, not
 * an automatic override of the line's own discount field.
 */
export function DiscountRulesPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const productOptions = useProductOptions(companyId);

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { productId: '', minQuantity: '1', discountPercentage: '5' },
  });

  const rulesQuery = useQuery({
    queryKey: ['discount-rules', companyId],
    queryFn: () => apiClient.get<PagedResult<DiscountRuleDto>>(`/sales/discount-rules?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const createRule = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<DiscountRuleDto>('/sales/discount-rules', {
        companyId,
        productId: values.productId,
        minQuantity: Number(values.minQuantity),
        discountPercentage: Number(values.discountPercentage),
      }),
    onSuccess: () => {
      reset({ productId: '', minQuantity: '1', discountPercentage: '5' });
      queryClient.invalidateQueries({ queryKey: ['discount-rules', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const deactivateRule = useMutation({
    mutationFn: (id: string) => apiClient.post<DiscountRuleDto>(`/sales/discount-rules/${id}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['discount-rules', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Discount Rules</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createRule.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <label className="flex flex-col gap-1 text-sm">
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
            {errors.productId && <span className="text-xs text-danger">{errors.productId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Minimum quantity
            <Controller
              control={control}
              name="minQuantity"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.minQuantity && <span className="text-xs text-danger">{errors.minQuantity.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Discount %
            <Controller
              control={control}
              name="discountPercentage"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.discountPercentage && <span className="text-xs text-danger">{errors.discountPercentage.message}</span>}
          </label>

          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Add discount tier'}</Button>
          </div>
        </form>
        {createRule.isError && createRule.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createRule.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Product', render: (row: DiscountRuleDto) => row.productId.slice(0, 8) + '…' },
            { header: 'Min. quantity', render: (row: DiscountRuleDto) => row.minQuantity.toLocaleString() },
            { header: 'Discount %', render: (row: DiscountRuleDto) => row.discountPercentage.toLocaleString() },
            { header: 'Status', render: (row: DiscountRuleDto) => (row.isActive ? 'Active' : 'Inactive') },
            {
              header: 'Actions',
              render: (row: DiscountRuleDto) =>
                row.isActive ? (
                  <Button type="button" variant="danger" disabled={deactivateRule.isPending} onClick={() => deactivateRule.mutate(row.id)}>
                    Deactivate
                  </Button>
                ) : null,
            },
          ]}
          rows={rulesQuery.data?.data}
          isLoading={rulesQuery.isLoading}
          isError={rulesQuery.isError}
          errorMessage="Could not load discount rules."
          emptyMessage="No discount rules yet — add a tier above."
          rowKey={(row) => row.id}
        />
      </Card>
      {deactivateRule.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that rule.</p>
      )}
    </div>
  );
}
