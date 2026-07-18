import { useState } from 'react';
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import { useDebouncedValue } from '../../../shared/hooks/useDebouncedValue';
import type { PagedResult } from '../../../shared/api/types';

const SEARCH_DEBOUNCE_MS = 250;

const schema = z.object({
  code: z.string().min(1, 'Code is required').max(20),
  name: z.string().min(1, 'Name is required').max(200),
});
type FormValues = z.infer<typeof schema>;

// Update command deliberately excludes Code — it's the immutable business key
// (see UpdateTaxJurisdictionCommand.cs / TaxJurisdictionsController.Update),
// same convention as CostCenter's edit form.
const editSchema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
});
type EditFormValues = z.infer<typeof editSchema>;

export interface TaxJurisdictionDto {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  createdAt: string;
}

/**
 * Tax Jurisdictions — M8b, Finance depth. Pure master data (Code/Name/
 * IsActive), same shape as CostCenter (M8a). Represents a taxing authority's
 * scope (e.g. "IN-KA" Karnataka/India, "US-CA" California/USA, or a
 * company-wide "DEFAULT") — one or more named TaxRate rows (see
 * TaxRatesPanel) belong to a jurisdiction created here. Rendered as a
 * sibling panel under AccountsPage, right before TaxRatesPanel.
 */
export function TaxJurisdictionsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [editingTaxJurisdictionId, setEditingTaxJurisdictionId] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebouncedValue(search, SEARCH_DEBOUNCE_MS);

  const taxJurisdictionsQuery = useQuery({
    queryKey: ['tax-jurisdictions', companyId, debouncedSearch],
    queryFn: () => {
      const params = new URLSearchParams({ companyId, page: '1', pageSize: '50' });
      if (debouncedSearch.trim()) params.set('search', debouncedSearch.trim());
      return apiClient.get<PagedResult<TaxJurisdictionDto>>(`/finance/tax-jurisdictions?${params.toString()}`);
    },
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { code: '', name: '' },
  });

  const createTaxJurisdiction = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<TaxJurisdictionDto>('/finance/tax-jurisdictions', {
        companyId,
        code: values.code,
        name: values.name,
      }),
    onSuccess: () => {
      reset({ code: '', name: '' });
      queryClient.invalidateQueries({ queryKey: ['tax-jurisdictions', companyId] });
      queryClient.invalidateQueries({ queryKey: ['tax-jurisdiction-options', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  // Soft-deactivate only — TaxJurisdictionsController exposes this as a
  // dedicated POST .../{id}/deactivate action (never a DELETE), same
  // convention as CostCentersController/AccountsController.
  const deactivateTaxJurisdiction = useMutation({
    mutationFn: (taxJurisdictionId: string) => apiClient.post<TaxJurisdictionDto>(`/finance/tax-jurisdictions/${taxJurisdictionId}/deactivate`, { companyId }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tax-jurisdictions', companyId] });
      queryClient.invalidateQueries({ queryKey: ['tax-jurisdiction-options', companyId] });
    },
  });

  if (!companyId) return null;

  const editingTaxJurisdiction = taxJurisdictionsQuery.data?.data.find((j) => j.id === editingTaxJurisdictionId) ?? null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Tax Jurisdictions</h2>
      <p className="mb-3 text-xs text-text-muted">
        Master data for the multi-jurisdiction tax engine — each jurisdiction owns its own set of named rates (see Tax Rates below). Not yet attached to invoice/journal lines (see TaxJurisdiction.cs).
      </p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createTaxJurisdiction.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Code
            <Controller
              control={control}
              name="code"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="IN-KA" {...field} />
              )}
            />
            {errors.code && <span className="text-xs text-danger">{errors.code.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Name
            <Controller
              control={control}
              name="name"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Karnataka, India" {...field} />
              )}
            />
            {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
          </label>
          <div className="col-span-2">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create tax jurisdiction'}</Button>
          </div>
        </form>
        {createTaxJurisdiction.isError && createTaxJurisdiction.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createTaxJurisdiction.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <label className="mb-3 flex flex-col gap-1 text-sm sm:w-72">
          Search
          <input
            className="rounded-md border border-border bg-surface px-2 py-1.5"
            placeholder="Search by code or name…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </label>
        <DataTable
          columns={[
            { header: 'Code', render: (row: TaxJurisdictionDto) => row.code },
            { header: 'Name', render: (row: TaxJurisdictionDto) => row.name },
            { header: 'Status', render: (row: TaxJurisdictionDto) => (row.isActive ? 'Active' : 'Inactive') },
            { header: 'Created', render: (row: TaxJurisdictionDto) => new Date(row.createdAt).toLocaleDateString() },
            {
              header: 'Actions',
              render: (row: TaxJurisdictionDto) => (
                <div className="flex items-center gap-2">
                  <Button type="button" variant="secondary" onClick={() => setEditingTaxJurisdictionId(row.id)}>
                    Edit
                  </Button>
                  <Button
                    type="button"
                    variant="danger"
                    disabled={!row.isActive || deactivateTaxJurisdiction.isPending}
                    onClick={() => deactivateTaxJurisdiction.mutate(row.id)}
                  >
                    {row.isActive ? 'Deactivate' : 'Deactivated'}
                  </Button>
                </div>
              ),
            },
          ]}
          rows={taxJurisdictionsQuery.data?.data}
          isLoading={taxJurisdictionsQuery.isLoading}
          isError={taxJurisdictionsQuery.isError}
          errorMessage="Could not load tax jurisdictions."
          emptyMessage="No tax jurisdictions yet — create the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {deactivateTaxJurisdiction.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that tax jurisdiction.</p>
      )}

      {editingTaxJurisdiction && (
        <TaxJurisdictionEditPanel
          companyId={companyId}
          taxJurisdiction={editingTaxJurisdiction}
          onClose={() => setEditingTaxJurisdictionId(null)}
        />
      )}
    </div>
  );
}

interface TaxJurisdictionEditPanelProps {
  companyId: string;
  taxJurisdiction: TaxJurisdictionDto;
  onClose: () => void;
}

function TaxJurisdictionEditPanel({ companyId, taxJurisdiction, onClose }: TaxJurisdictionEditPanelProps) {
  const queryClient = useQueryClient();

  const { control, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    values: { name: taxJurisdiction.name },
  });

  const updateTaxJurisdiction = useMutation({
    mutationFn: (values: EditFormValues) =>
      apiClient.put<TaxJurisdictionDto>(`/finance/tax-jurisdictions/${taxJurisdiction.id}`, {
        companyId,
        name: values.name,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tax-jurisdictions', companyId] });
      queryClient.invalidateQueries({ queryKey: ['tax-jurisdiction-options', companyId] });
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
    <Card className="mt-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-base font-semibold text-text">Edit tax jurisdiction — {taxJurisdiction.code}</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form onSubmit={handleSubmit((values) => updateTaxJurisdiction.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
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
        <div className="col-span-2 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save changes'}</Button>
          {updateTaxJurisdiction.isError && (
            <span role="alert" className="text-sm text-danger">Could not save that tax jurisdiction.</span>
          )}
        </div>
      </form>
    </Card>
  );
}
