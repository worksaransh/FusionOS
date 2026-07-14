import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { CrudListPage } from '../../../shared/ui/CrudListPage';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import type { PagedResult } from '../../../shared/api/types';
import { SalesOrdersPanel } from './SalesOrdersPanel';
import { InvoicesPanel } from './InvoicesPanel';
import { DispatchesPanel } from './DispatchesPanel';

const schema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  code: z.string().min(1, 'Code is required').max(30),
  contactEmail: z.string().email('Must be a valid email').optional().or(z.literal('')),
  creditLimit: z
    .string()
    .min(1, 'Credit limit is required')
    .refine((v) => !Number.isNaN(Number(v)) && Number(v) >= 0, 'Credit limit must be a non-negative number'),
});
type FormValues = z.infer<typeof schema>;

interface CustomerDto {
  id: string;
  name: string;
  code: string;
  contactEmail: string | null;
  creditLimit: number;
  isActive: boolean;
  createdAt: string;
}

/** Phase 1 slice — see backend/src/Modules/Sales for the full CQRS handler. */
export function CustomersPage() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const customersQuery = useQuery({
    queryKey: ['customers', companyId],
    queryFn: () => apiClient.get<PagedResult<CustomerDto>>(`/sales/customers?companyId=${companyId}&page=1&pageSize=25`),
    enabled: Boolean(companyId),
  });

  const { register, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { creditLimit: '0' },
  });

  const createCustomer = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<CustomerDto>('/sales/customers', { companyId, ...values, creditLimit: Number(values.creditLimit) }),
    onSuccess: () => {
      reset({ creditLimit: '0' });
      queryClient.invalidateQueries({ queryKey: ['customers', companyId] });
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
    return <p className="text-text-muted">Set an active Company ID in the header above to manage customers.</p>;
  }

  return (
    <div>
      <CrudListPage<CustomerDto>
      title="Customers"
      description="Customer master data — Sales, Phase 1"
      rows={customersQuery.data?.data}
      isLoading={customersQuery.isLoading}
      isError={customersQuery.isError}
      errorMessage="Could not load customers."
      emptyMessage="No customers yet — create the first one above."
      rowKey={(row) => row.id}
      columns={[
        { header: 'Code', render: (row) => row.code },
        { header: 'Name', render: (row) => row.name },
        { header: 'Credit limit', render: (row) => row.creditLimit.toLocaleString() },
        { header: 'Created', render: (row) => new Date(row.createdAt).toLocaleDateString() },
      ]}
      form={
        <form onSubmit={handleSubmit((values) => createCustomer.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Name
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('name')} />
            {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Code
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="CUST-01" {...register('code')} />
            {errors.code && <span className="text-xs text-danger">{errors.code.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Contact email (optional)
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('contactEmail')} />
            {errors.contactEmail && <span className="text-xs text-danger">{errors.contactEmail.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Credit limit
            <input type="text" inputMode="decimal" className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('creditLimit')} />
            {errors.creditLimit && <span className="text-xs text-danger">{errors.creditLimit.message}</span>}
          </label>
          <div className="col-span-2">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create customer'}</Button>
          </div>
        </form>
      }
      />
      <SalesOrdersPanel />
      <InvoicesPanel />
      <DispatchesPanel />
    </div>
  );
}
