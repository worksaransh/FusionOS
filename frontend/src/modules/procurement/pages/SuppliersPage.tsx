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
import { PriceHistoryPanel } from './PriceHistoryPanel';
import { PurchaseOrdersPanel } from './PurchaseOrdersPanel';
import { RfqsPanel } from './RfqsPanel';
import { SupplierContractsPanel } from './SupplierContractsPanel';
import { VendorReturnsPanel } from './VendorReturnsPanel';

const schema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  code: z.string().min(1, 'Code is required').max(30),
  contactEmail: z.string().email('Must be a valid email').optional().or(z.literal('')),
  contactPhone: z.string().optional(),
});
type FormValues = z.infer<typeof schema>;

// UpdateSupplierCommand deliberately keeps Code (the business key) immutable —
// only Name/ContactEmail/ContactPhone are ever sent to the PUT endpoint (see
// SuppliersController.Update / UpdateSupplierRequest).
const editSchema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  contactEmail: z.string().email('Must be a valid email').optional().or(z.literal('')),
  contactPhone: z.string().optional(),
});
type EditFormValues = z.infer<typeof editSchema>;

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
  const [editingSupplierId, setEditingSupplierId] = useState<string | null>(null);

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

  // Soft-deactivate only — SuppliersController exposes this as a dedicated
  // POST .../{id}/deactivate action (never a DELETE) since a Supplier may be
  // referenced by historical Purchase Orders (shared/api/client.ts has no
  // `delete` verb by design, see 08_API_STANDARDS.md).
  const deactivateSupplier = useMutation({
    mutationFn: (supplierId: string) => apiClient.post<SupplierDto>(`/procurement/suppliers/${supplierId}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['suppliers', companyId] }),
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
        { header: 'Status', render: (row) => (row.isActive ? 'Active' : 'Inactive') },
        { header: 'Created', render: (row) => new Date(row.createdAt).toLocaleDateString() },
        {
          header: 'Actions',
          render: (row) => (
            <div className="flex items-center gap-2">
              <Button type="button" variant="secondary" onClick={() => setEditingSupplierId(row.id)}>
                Edit
              </Button>
              <Button
                type="button"
                variant="danger"
                disabled={!row.isActive || deactivateSupplier.isPending}
                onClick={() => deactivateSupplier.mutate(row.id)}
              >
                {row.isActive ? 'Deactivate' : 'Deactivated'}
              </Button>
            </div>
          ),
        },
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
      {deactivateSupplier.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that supplier.</p>
      )}

      {editingSupplierId && (
        <SupplierEditPanel
          companyId={companyId}
          supplier={suppliersQuery.data?.data.find((s) => s.id === editingSupplierId) ?? null}
          onClose={() => setEditingSupplierId(null)}
        />
      )}

      <PurchaseOrdersPanel />
      <RfqsPanel />
      <SupplierContractsPanel />
      <VendorReturnsPanel />
      <PriceHistoryPanel />
    </div>
  );
}

interface SupplierEditPanelProps {
  companyId: string;
  supplier: SupplierDto | null;
  onClose: () => void;
}

function SupplierEditPanel({ companyId, supplier, onClose }: SupplierEditPanelProps) {
  const queryClient = useQueryClient();

  const { register, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    defaultValues: {
      name: supplier?.name ?? '',
      contactEmail: supplier?.contactEmail ?? '',
      contactPhone: supplier?.contactPhone ?? '',
    },
  });

  const updateSupplier = useMutation({
    mutationFn: (values: EditFormValues) =>
      apiClient.put<SupplierDto>(`/procurement/suppliers/${supplier!.id}`, {
        companyId,
        name: values.name,
        contactEmail: values.contactEmail || null,
        contactPhone: values.contactPhone || null,
      }),
    onSuccess: () => {
      onClose();
      queryClient.invalidateQueries({ queryKey: ['suppliers', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof EditFormValues, { message: messages[0] });
        }
      }
    },
  });

  if (!supplier) return null;

  return (
    <Card className="mt-8">
      <div className="mb-3 flex items-center justify-between">
        <h2 className="text-lg font-semibold text-text">Edit supplier — {supplier.code}</h2>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>

      <form onSubmit={handleSubmit((values) => updateSupplier.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <label className="flex flex-col gap-1 text-sm">
          Name
          <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('name')} />
          {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Code (immutable)
          <input className="rounded-md border border-border bg-surface-muted px-2 py-1.5 text-text-muted" value={supplier.code} disabled />
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
        <div className="col-span-2 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save changes'}</Button>
          {updateSupplier.isError && (
            <span role="alert" className="text-sm text-danger">Could not update that supplier.</span>
          )}
        </div>
      </form>
    </Card>
  );
}
