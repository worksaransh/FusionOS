import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { CrudListPage } from '../../../shared/ui/CrudListPage';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import type { PagedResult } from '../../../shared/api/types';
import { PurchaseOrdersPanel } from './PurchaseOrdersPanel';

const schema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  code: z.string().min(1, 'Code is required').max(30),
  contactEmail: z.string().email('Must be a valid email').optional().or(z.literal('')),
  contactPhone: z.string().optional(),
});
type FormValues = z.infer<typeof schema>;

interface SupplierDto {
  id: string;
  name: string;
  code: string;
  contactEmail: string | null;
  contactPhone: string | null;
  isActive: boolean;
  createdAt: string;
}

/** Phase 1 slice — see backend/src/Modules/Procurement for the full CQRS handler. */
export function SuppliersPage() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const suppliersQuery = useQuery({
    queryKey: ['suppliers', companyId],
    queryFn: () => apiClient.get<PagedResult<SupplierDto>>(`/procurement/suppliers?companyId=${companyId}&page=1&pageSize=25`),
    enabled: Boolean(companyId),
  });

  const { register, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
  });

  const createSupplier = useMutation({
    mutationFn: (values: FormValues) => apiClient.post<SupplierDto>('/procurement/suppliers', { companyId, ...values }),
    onSuccess: () => {
      reset();
      queryClient.invalidateQueries({ queryKey: ['suppliers', companyId] });
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
    return <p className="text-text-muted">Set an active Company ID in the header above to manage suppliers.</p>;
  }

  return (
    <div>
      <CrudListPage<SupplierDto>
      title="Suppliers"
      description="Supplier master data — Procurement, Phase 1"
      rows={suppliersQuery.data?.data}
      isLoading={suppliersQuery.isLoading}
      isError={suppliersQuery.isError}
      errorMessage="Could not load suppliers."
      emptyMessage="No suppliers yet — create the first one above."
      rowKey={(row) => row.id}
      columns={[
        { header: 'Code', render: (row) => row.code },
        { header: 'Name', render: (row) => row.name },
        { header: 'Email', render: (row) => row.contactEmail ?? '—' },
        { header: 'Created', render: (row) => new Date(row.createdAt).toLocaleDateString() },
      ]}
      form={
        <form onSubmit={handleSubmit((values) => createSupplier.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Name
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('name')} />
            {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Code
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="SUP-01" {...register('code')} />
            {errors.code && <span className="text-xs text-danger">{errors.code.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Contact email (optional)
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('contactEmail')} />
            {errors.contactEmail && <span className="text-xs text-danger">{errors.contactEmail.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Contact phone (optional)
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('contactPhone')} />
          </label>
          <div className="col-span-2">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create supplier'}</Button>
          </div>
        </form>
      }
      />
      <PurchaseOrdersPanel />
    </div>
  );
}
