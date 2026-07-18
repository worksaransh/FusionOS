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
import { useSupplierOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  supplierId: z.string().uuid('Pick a supplier'),
  startDate: z.string().min(1, 'Start date is required'),
  endDate: z.string().min(1, 'End date is required'),
  terms: z.string().min(1, 'Terms are required').max(2000),
});
type FormValues = z.infer<typeof schema>;

interface SupplierContractDto {
  id: string;
  supplierId: string;
  startDate: string;
  endDate: string;
  terms: string;
  status: string;
}

/**
 * Supplier contracts — validity period + terms text, alongside supplier
 * scorecards as the two remaining Procurement-depth items
 * (docs/IMPLEMENTATION_PLAN.md Phase 10 item 2). Deliberately minimal: no
 * pricing schedule, no auto-renewal — just Create and one-way Terminate,
 * matching Supplier.Deactivate()'s own one-way restraint.
 */
export function SupplierContractsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const supplierOptions = useSupplierOptions(companyId);

  const { control, register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { supplierId: '', startDate: '', endDate: '', terms: '' },
  });

  const contractsQuery = useQuery({
    queryKey: ['supplier-contracts', companyId],
    queryFn: () => apiClient.get<PagedResult<SupplierContractDto>>(`/procurement/supplier-contracts?companyId=${companyId}&page=1&pageSize=25`),
    enabled: Boolean(companyId),
  });

  const createContract = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<SupplierContractDto>('/procurement/supplier-contracts', {
        companyId,
        supplierId: values.supplierId,
        startDate: new Date(values.startDate).toISOString(),
        endDate: new Date(values.endDate).toISOString(),
        terms: values.terms,
      }),
    onSuccess: () => {
      reset({ supplierId: '', startDate: '', endDate: '', terms: '' });
      queryClient.invalidateQueries({ queryKey: ['supplier-contracts', companyId] });
    },
  });

  const terminateContract = useMutation({
    mutationFn: (id: string) => apiClient.post(`/procurement/supplier-contracts/${id}/terminate?companyId=${companyId}`, {}),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['supplier-contracts', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Supplier Contracts</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createContract.mutate(values))} className="flex flex-col gap-4">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <label className="flex flex-col gap-1 text-sm">
              Supplier
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
            <label className="flex flex-col gap-1 text-sm">
              Start date
              <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('startDate')} />
              {errors.startDate && <span className="text-xs text-danger">{errors.startDate.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              End date
              <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('endDate')} />
              {errors.endDate && <span className="text-xs text-danger">{errors.endDate.message}</span>}
            </label>
          </div>
          <label className="flex flex-col gap-1 text-sm">
            Terms
            <textarea className="rounded-md border border-border bg-surface px-2 py-1.5" rows={3} {...register('terms')} />
            {errors.terms && <span className="text-xs text-danger">{errors.terms.message}</span>}
          </label>
          <Button type="submit" disabled={isSubmitting} className="w-fit">
            {isSubmitting ? 'Creating…' : 'Create contract'}
          </Button>
        </form>
        {createContract.isError && createContract.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createContract.error.problem.title}</p>
        )}
      </Card>

      {contractsQuery.data && (
        <Card>
          <DataTable
            columns={[
              { header: 'Supplier ID', render: (c: SupplierContractDto) => c.supplierId },
              { header: 'Start', render: (c: SupplierContractDto) => new Date(c.startDate).toLocaleDateString() },
              { header: 'End', render: (c: SupplierContractDto) => new Date(c.endDate).toLocaleDateString() },
              { header: 'Terms', render: (c: SupplierContractDto) => c.terms },
              { header: 'Status', render: (c: SupplierContractDto) => c.status },
              {
                header: '',
                render: (c: SupplierContractDto) =>
                  c.status === 'Active' ? (
                    <Button variant="danger" onClick={() => terminateContract.mutate(c.id)} disabled={terminateContract.isPending}>
                      Terminate
                    </Button>
                  ) : null,
              },
            ]}
            rows={contractsQuery.data.data}
            isLoading={contractsQuery.isLoading}
            emptyMessage="No supplier contracts yet."
            rowKey={(c) => c.id}
          />
        </Card>
      )}
    </div>
  );
}
