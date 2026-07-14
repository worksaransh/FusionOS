import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { CrudListPage } from '../../../shared/ui/CrudListPage';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import type { PagedResult } from '../../../shared/api/types';
import { StockLedgerPanel } from './StockLedgerPanel';

const schema = z.object({
  sku: z.string().min(1, 'SKU is required').max(50),
  name: z.string().min(1, 'Name is required').max(200),
  unitOfMeasure: z.string().min(1, 'Unit of measure is required').max(20),
  description: z.string().optional(),
});
type FormValues = z.infer<typeof schema>;

interface ProductDto {
  id: string;
  sku: string;
  name: string;
  description: string | null;
  unitOfMeasure: string;
  isActive: boolean;
  createdAt: string;
}

/** Phase 1 slice — see backend/src/Modules/Inventory for the full CQRS handler. */
export function ProductsPage() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

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

  if (!companyId) {
    return <p className="text-text-muted">Set an active Company ID in the header above to manage products.</p>;
  }

  return (
    <div>
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
        { header: 'Created', render: (row) => new Date(row.createdAt).toLocaleDateString() },
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
      <StockLedgerPanel />
    </div>
  );
}
