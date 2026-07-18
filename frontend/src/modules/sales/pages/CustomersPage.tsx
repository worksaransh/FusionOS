import { useState } from 'react';
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
import { SalesOrdersPanel } from './SalesOrdersPanel';
import { InvoicesPanel } from './InvoicesPanel';
import { DispatchesPanel } from './DispatchesPanel';
import { CreditNotesPanel } from './CreditNotesPanel';
import { QuotationsPanel } from './QuotationsPanel';
import { PriceListsPanel, type PriceListDto } from './PriceListsPanel';
import { CommissionsPanel } from './CommissionsPanel';

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

/** Update endpoint keeps Code immutable — see UpdateCustomerCommand.cs / Customer.UpdateDetails. */
const editSchema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  contactEmail: z.string().email('Must be a valid email').optional().or(z.literal('')),
  creditLimit: z
    .string()
    .min(1, 'Credit limit is required')
    .refine((v) => !Number.isNaN(Number(v)) && Number(v) >= 0, 'Credit limit must be a non-negative number'),
});
type EditFormValues = z.infer<typeof editSchema>;

interface CustomerDto {
  id: string;
  name: string;
  code: string;
  contactEmail: string | null;
  creditLimit: number;
  isActive: boolean;
  createdAt: string;
  priceListId: string | null;
}

/** Phase 1 slice — see backend/src/Modules/Sales for the full CQRS handler. */
export function CustomersPage() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [editingCustomerId, setEditingCustomerId] = useState<string | null>(null);

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

  const deactivateCustomer = useMutation({
    mutationFn: (customerId: string) =>
      apiClient.post<CustomerDto>(`/sales/customers/${customerId}/deactivate?companyId=${companyId}`, {}),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['customers', companyId] }),
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
        { header: 'Status', render: (row) => (row.isActive ? 'Active' : 'Inactive') },
        { header: 'Created', render: (row) => new Date(row.createdAt).toLocaleDateString() },
        {
          header: '',
          render: (row) => (
            <div className="flex items-center gap-2">
              <Button
                variant="secondary"
                onClick={() => setEditingCustomerId((current) => (current === row.id ? null : row.id))}
              >
                {editingCustomerId === row.id ? 'Close' : 'Edit'}
              </Button>
              <Button
                variant="danger"
                disabled={!row.isActive || deactivateCustomer.isPending}
                onClick={() => deactivateCustomer.mutate(row.id)}
              >
                {row.isActive ? 'Deactivate' : 'Deactivated'}
              </Button>
            </div>
          ),
        },
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
      {deactivateCustomer.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that customer.</p>
      )}

      {editingCustomerId && (
        <CustomerEditPanel
          companyId={companyId}
          customer={customersQuery.data?.data.find((c) => c.id === editingCustomerId) ?? null}
          onClose={() => setEditingCustomerId(null)}
        />
      )}

      <QuotationsPanel />
      <SalesOrdersPanel />
      <InvoicesPanel />
      <DispatchesPanel />
      <CreditNotesPanel />
      <PriceListsPanel />
      <CommissionsPanel />
    </div>
  );
}

interface CustomerEditPanelProps {
  companyId: string;
  customer: CustomerDto | null;
  onClose: () => void;
}

/** Inline edit panel for the PUT /sales/customers/{id} endpoint — Code is immutable, so it is not editable here. */
function CustomerEditPanel({ companyId, customer, onClose }: CustomerEditPanelProps) {
  const queryClient = useQueryClient();

  const { register, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    defaultValues: customer
      ? {
          name: customer.name,
          contactEmail: customer.contactEmail ?? '',
          creditLimit: String(customer.creditLimit),
        }
      : undefined,
  });

  const updateCustomer = useMutation({
    mutationFn: (values: EditFormValues) =>
      apiClient.put<CustomerDto>(`/sales/customers/${customer!.id}`, {
        companyId,
        name: values.name,
        contactEmail: values.contactEmail || null,
        creditLimit: Number(values.creditLimit),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['customers', companyId] });
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

  const priceListsQuery = useQuery({
    queryKey: ['price-lists', companyId],
    queryFn: () => apiClient.get<PagedResult<PriceListDto>>(`/sales/price-lists?companyId=${companyId}&page=1&pageSize=25`),
    enabled: Boolean(companyId),
  });

  const [selectedPriceListId, setSelectedPriceListId] = useState(customer?.priceListId ?? '');

  const assignPriceList = useMutation({
    mutationFn: (priceListId: string) =>
      apiClient.post<CustomerDto>(`/sales/customers/${customer!.id}/assign-price-list`, {
        companyId,
        priceListId: priceListId || null,
      }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['customers', companyId] }),
  });

  if (!customer) return null;

  return (
    <Card className="mt-8">
      <div className="mb-3 flex items-center justify-between">
        <h2 className="text-lg font-semibold text-text">Edit customer — {customer.code}</h2>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>

      <form
        onSubmit={handleSubmit((values) => updateCustomer.mutate(values))}
        className="grid grid-cols-1 gap-4 sm:grid-cols-2"
      >
        <label className="flex flex-col gap-1 text-sm">
          Name
          <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('name')} />
          {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Contact email (optional)
          <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('contactEmail')} />
          {errors.contactEmail && <span className="text-xs text-danger">{errors.contactEmail.message}</span>}
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Credit limit
          <input
            type="text"
            inputMode="decimal"
            className="rounded-md border border-border bg-surface px-2 py-1.5"
            {...register('creditLimit')}
          />
          {errors.creditLimit && <span className="text-xs text-danger">{errors.creditLimit.message}</span>}
        </label>
        <div className="col-span-2 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting || updateCustomer.isPending}>
            {isSubmitting || updateCustomer.isPending ? 'Saving…' : 'Save changes'}
          </Button>
          {updateCustomer.isError && (
            <span role="alert" className="text-sm text-danger">Could not save that customer.</span>
          )}
        </div>
      </form>

      <div className="mt-4 flex items-end gap-2 border-t border-border pt-4">
        <label className="flex flex-col gap-1 text-sm">
          Price list
          <select
            className="w-64 rounded-md border border-border bg-surface px-2 py-1.5"
            value={selectedPriceListId}
            onChange={(e) => setSelectedPriceListId(e.target.value)}
          >
            <option value="">None</option>
            {priceListsQuery.data?.data.map((pl) => (
              <option key={pl.id} value={pl.id}>{pl.name}</option>
            ))}
          </select>
        </label>
        <Button
          type="button"
          variant="secondary"
          onClick={() => assignPriceList.mutate(selectedPriceListId)}
          disabled={assignPriceList.isPending}
        >
          Assign price list
        </Button>
      </div>
    </Card>
  );
}
