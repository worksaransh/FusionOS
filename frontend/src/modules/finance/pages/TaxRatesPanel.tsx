import { useState } from 'react';
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
import { useTaxJurisdictionOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  taxJurisdictionId: z.string().uuid('Pick a tax jurisdiction'),
  code: z.string().min(1, 'Code is required').max(20),
  name: z.string().min(1, 'Name is required').max(200),
  percentage: z.string().refine((v) => Number(v) >= 0 && Number(v) <= 100, 'Percentage must be between 0 and 100'),
});
type FormValues = z.infer<typeof schema>;

// Update command deliberately excludes Code and TaxJurisdictionId — only
// Name/Percentage are editable (see UpdateTaxRateCommand.cs /
// TaxRatesController.Update), same immutability rule as Bin's own edit form.
const editSchema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  percentage: z.string().refine((v) => Number(v) >= 0 && Number(v) <= 100, 'Percentage must be between 0 and 100'),
});
type EditFormValues = z.infer<typeof editSchema>;

interface TaxRateDto {
  id: string;
  taxJurisdictionId: string;
  code: string;
  name: string;
  percentage: number;
  isActive: boolean;
  createdAt: string;
}

/**
 * Tax Rates — M8b, Finance depth. Nests under a Tax Jurisdiction (pick one
 * from a searchable dropdown, same "pick the parent, then manage its
 * children" pattern as Warehouse's BinsPanel picking a Warehouse then a
 * Zone), rather than being embedded inside TaxJurisdictionsPanel. Not yet
 * attached to invoice/journal lines — see TaxRate.cs's own doc comment.
 */
export function TaxRatesPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [lookupTaxJurisdictionId, setLookupTaxJurisdictionId] = useState('');
  const [editingTaxRateId, setEditingTaxRateId] = useState<string | null>(null);

  const taxJurisdictionOptions = useTaxJurisdictionOptions(companyId);

  const { control, handleSubmit, watch, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { taxJurisdictionId: '', code: '', name: '', percentage: '0' },
  });
  const watchedTaxJurisdictionId = watch('taxJurisdictionId');
  const effectiveTaxJurisdictionId = lookupTaxJurisdictionId || watchedTaxJurisdictionId;

  const createTaxRate = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<TaxRateDto>('/finance/tax-rates', {
        companyId,
        taxJurisdictionId: values.taxJurisdictionId,
        code: values.code,
        name: values.name,
        percentage: Number(values.percentage),
      }),
    onSuccess: (_data, variables) => {
      reset({ taxJurisdictionId: variables.taxJurisdictionId, code: '', name: '', percentage: '0' });
      setLookupTaxJurisdictionId(variables.taxJurisdictionId);
      queryClient.invalidateQueries({ queryKey: ['tax-rates', companyId, variables.taxJurisdictionId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const taxRatesQuery = useQuery({
    queryKey: ['tax-rates', companyId, effectiveTaxJurisdictionId],
    queryFn: () =>
      apiClient.get<PagedResult<TaxRateDto>>(
        `/finance/tax-rates?companyId=${companyId}&taxJurisdictionId=${effectiveTaxJurisdictionId}&page=1&pageSize=50`,
      ),
    enabled: Boolean(companyId && effectiveTaxJurisdictionId),
  });

  // Soft-deactivate only — hits POST /tax-rates/{id}/deactivate, never a DELETE.
  const deactivateTaxRate = useMutation({
    mutationFn: (id: string) => apiClient.post(`/finance/tax-rates/${id}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['tax-rates', companyId, effectiveTaxJurisdictionId] }),
  });

  if (!companyId) return null;

  const editingTaxRate = taxRatesQuery.data?.data.find((r) => r.id === editingTaxRateId) ?? null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Tax Rates</h2>
      <p className="mb-3 text-xs text-text-muted">
        Named rates within a tax jurisdiction (e.g. "GST-STANDARD" at 18%). Pick a jurisdiction below to manage its rates.
      </p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createTaxRate.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            Tax jurisdiction
            <Controller
              control={control}
              name="taxJurisdictionId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={(id) => {
                    field.onChange(id);
                    setLookupTaxJurisdictionId(id);
                  }}
                  options={taxJurisdictionOptions.options}
                  isLoading={taxJurisdictionOptions.isLoading}
                  onSearchChange={taxJurisdictionOptions.onSearchChange}
                  placeholder="Search tax jurisdictions…"
                />
              )}
            />
            {errors.taxJurisdictionId && <span className="text-xs text-danger">{errors.taxJurisdictionId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Rate code
            <Controller
              control={control}
              name="code"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="GST-STANDARD" {...field} />
              )}
            />
            {errors.code && <span className="text-xs text-danger">{errors.code.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Rate name
            <Controller
              control={control}
              name="name"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="GST 18%" {...field} />
              )}
            />
            {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Percentage
            <Controller
              control={control}
              name="percentage"
              render={({ field }) => (
                <input type="number" step="0.01" min="0" max="100" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.percentage && <span className="text-xs text-danger">{errors.percentage.message}</span>}
          </label>
          <div className="col-span-2">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create tax rate'}</Button>
          </div>
        </form>
        {createTaxRate.isError && createTaxRate.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createTaxRate.error.problem.title}</p>
        )}
      </Card>

      {effectiveTaxJurisdictionId ? (
        <Card>
          <DataTable
            columns={[
              { header: 'Code', render: (row: TaxRateDto) => row.code },
              { header: 'Name', render: (row: TaxRateDto) => row.name },
              { header: 'Percentage', render: (row: TaxRateDto) => `${row.percentage}%` },
              { header: 'Status', render: (row: TaxRateDto) => (row.isActive ? 'Active' : 'Inactive') },
              { header: 'Created', render: (row: TaxRateDto) => new Date(row.createdAt).toLocaleDateString() },
              {
                header: 'Actions',
                render: (row: TaxRateDto) => (
                  <div className="flex items-center gap-2">
                    <Button type="button" variant="secondary" onClick={() => setEditingTaxRateId(row.id)}>
                      Edit
                    </Button>
                    <Button
                      type="button"
                      variant="danger"
                      disabled={!row.isActive || (deactivateTaxRate.isPending && deactivateTaxRate.variables === row.id)}
                      onClick={() => deactivateTaxRate.mutate(row.id)}
                    >
                      {row.isActive ? 'Deactivate' : 'Deactivated'}
                    </Button>
                  </div>
                ),
              },
            ]}
            rows={taxRatesQuery.data?.data}
            isLoading={taxRatesQuery.isLoading}
            isError={taxRatesQuery.isError}
            errorMessage="Could not load tax rates."
            emptyMessage="No tax rates yet for this jurisdiction."
            rowKey={(row) => row.id}
          />
        </Card>
      ) : (
        <Card>
          <p className="text-sm text-text-muted">Pick a tax jurisdiction above to see its rates.</p>
        </Card>
      )}
      {deactivateTaxRate.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that tax rate.</p>
      )}

      {editingTaxRate && (
        <TaxRateEditPanel
          companyId={companyId}
          taxJurisdictionId={effectiveTaxJurisdictionId}
          taxRate={editingTaxRate}
          onClose={() => setEditingTaxRateId(null)}
        />
      )}
    </div>
  );
}

interface TaxRateEditPanelProps {
  companyId: string;
  taxJurisdictionId: string;
  taxRate: TaxRateDto;
  onClose: () => void;
}

function TaxRateEditPanel({ companyId, taxJurisdictionId, taxRate, onClose }: TaxRateEditPanelProps) {
  const queryClient = useQueryClient();

  const { control, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    values: { name: taxRate.name, percentage: String(taxRate.percentage) },
  });

  const updateTaxRate = useMutation({
    mutationFn: (values: EditFormValues) =>
      apiClient.put<TaxRateDto>(`/finance/tax-rates/${taxRate.id}`, {
        companyId,
        name: values.name,
        percentage: Number(values.percentage),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tax-rates', companyId, taxJurisdictionId] });
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

  return (
    <Card className="mt-6">
      <div className="mb-3 flex items-center justify-between">
        <h2 className="text-lg font-semibold text-text">Edit tax rate — {taxRate.code}</h2>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form onSubmit={handleSubmit((values) => updateTaxRate.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <label className="flex flex-col gap-1 text-sm">
          Name
          <Controller
            control={control}
            name="name"
            render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )}
          />
          {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Percentage
          <Controller
            control={control}
            name="percentage"
            render={({ field }) => (
              <input type="number" step="0.01" min="0" max="100" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )}
          />
          {errors.percentage && <span className="text-xs text-danger">{errors.percentage.message}</span>}
        </label>
        <div className="col-span-2 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save changes'}</Button>
          {updateTaxRate.isError && (
            <span role="alert" className="text-sm text-danger">Could not save that tax rate.</span>
          )}
        </div>
      </form>
    </Card>
  );
}
