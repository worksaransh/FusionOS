import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import type { PagedResult } from '../../../shared/api/types';

const definitionSchema = z.object({
  name: z.string().min(1, 'Name is required').max(100),
});
type DefinitionFormValues = z.infer<typeof definitionSchema>;

const valueSchema = z.object({
  value: z.string().min(1, 'Value is required').max(100),
});
type ValueFormValues = z.infer<typeof valueSchema>;

export interface AttributeDefinitionDto {
  id: string;
  name: string;
  isActive: boolean;
  createdAt: string;
}

export interface AttributeValueDto {
  id: string;
  attributeDefinitionId: string;
  value: string;
  isActive: boolean;
  createdAt: string;
}

/**
 * Attribute Definitions — the structured, reusable counterpart to
 * ProductVariant's free-text Attributes string (see ProductVariant.cs and
 * AttributeDefinition.cs doc comments for why the free-text field stays as
 * is). A Definition (e.g. "Color") owns a controlled set of Values (e.g.
 * "Red"/"Blue"/"Green"), shared across every product in the company — not
 * per-variant free text. Rendered as a sibling panel under ProductsPage, same
 * pattern as StockLedgerPanel/ReservationsPanel. Managing which values a
 * given variant carries happens inline on the variant itself, inside
 * ProductVariantsPanel (see ProductsPage.tsx).
 */
export function AttributeDefinitionsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [expandedDefinitionId, setExpandedDefinitionId] = useState<string | null>(null);

  const definitionsQuery = useQuery({
    queryKey: ['attribute-definitions', companyId],
    queryFn: () => apiClient.get<PagedResult<AttributeDefinitionDto>>(`/inventory/attribute-definitions?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const { register, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<DefinitionFormValues>({
    resolver: zodResolver(definitionSchema),
  });

  const createDefinition = useMutation({
    mutationFn: (values: DefinitionFormValues) =>
      apiClient.post<AttributeDefinitionDto>('/inventory/attribute-definitions', { companyId, name: values.name }),
    onSuccess: () => {
      reset();
      queryClient.invalidateQueries({ queryKey: ['attribute-definitions', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof DefinitionFormValues, { message: messages[0] });
        }
      }
    },
  });

  // Soft-deactivate only — POST .../{id}/deactivate, never a DELETE, same
  // convention as CostCentersController/ProductsController.
  const deactivateDefinition = useMutation({
    mutationFn: (id: string) => apiClient.post<AttributeDefinitionDto>(`/inventory/attribute-definitions/${id}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['attribute-definitions', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Attribute Definitions</h2>
      <p className="mb-3 text-xs text-text-muted">
        Reusable, controlled attribute dimensions (e.g. "Color") shared across every product — each owns a set of
        Values (e.g. "Red", "Blue"). Assign values to a specific variant from that variant's row on the product edit
        panel above.
      </p>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createDefinition.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Name
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Color" {...register('name')} />
            {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
          </label>
          <div className="col-span-2">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create attribute'}</Button>
          </div>
        </form>
        {createDefinition.isError && createDefinition.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createDefinition.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Name', render: (row: AttributeDefinitionDto) => row.name },
            { header: 'Status', render: (row: AttributeDefinitionDto) => (row.isActive ? 'Active' : 'Inactive') },
            { header: 'Created', render: (row: AttributeDefinitionDto) => new Date(row.createdAt).toLocaleDateString() },
            {
              header: 'Actions',
              render: (row: AttributeDefinitionDto) => (
                <div className="flex items-center gap-2">
                  <Button type="button" variant="secondary" onClick={() => setExpandedDefinitionId(expandedDefinitionId === row.id ? null : row.id)}>
                    {expandedDefinitionId === row.id ? 'Hide values' : 'Manage values'}
                  </Button>
                  <Button
                    type="button"
                    variant="danger"
                    disabled={!row.isActive || (deactivateDefinition.isPending && deactivateDefinition.variables === row.id)}
                    onClick={() => deactivateDefinition.mutate(row.id)}
                  >
                    {row.isActive ? 'Deactivate' : 'Deactivated'}
                  </Button>
                </div>
              ),
            },
          ]}
          rows={definitionsQuery.data?.data}
          isLoading={definitionsQuery.isLoading}
          isError={definitionsQuery.isError}
          errorMessage="Could not load attribute definitions."
          emptyMessage="No attribute definitions yet — create the first one above (e.g. Color, Size)."
          rowKey={(row) => row.id}
        />
      </Card>
      {deactivateDefinition.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that attribute definition.</p>
      )}

      {expandedDefinitionId && companyId && (
        <AttributeValuesPanel
          companyId={companyId}
          definition={definitionsQuery.data?.data.find((d) => d.id === expandedDefinitionId) ?? null}
        />
      )}
    </div>
  );
}

interface AttributeValuesPanelProps {
  companyId: string;
  definition: AttributeDefinitionDto | null;
}

function AttributeValuesPanel({ companyId, definition }: AttributeValuesPanelProps) {
  const queryClient = useQueryClient();

  const valuesQuery = useQuery({
    queryKey: ['attribute-values', companyId, definition?.id],
    queryFn: () => apiClient.get<AttributeValueDto[]>(`/inventory/attribute-definitions/${definition!.id}/values?companyId=${companyId}`),
    enabled: Boolean(companyId && definition),
  });

  const { register, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<ValueFormValues>({
    resolver: zodResolver(valueSchema),
  });

  const createValue = useMutation({
    mutationFn: (values: ValueFormValues) =>
      apiClient.post<AttributeValueDto>(`/inventory/attribute-definitions/${definition!.id}/values`, { companyId, value: values.value }),
    onSuccess: () => {
      reset();
      queryClient.invalidateQueries({ queryKey: ['attribute-values', companyId, definition?.id] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof ValueFormValues, { message: messages[0] });
        }
      }
    },
  });

  const deactivateValue = useMutation({
    mutationFn: (valueId: string) =>
      apiClient.post<AttributeValueDto>(`/inventory/attribute-definitions/${definition!.id}/values/${valueId}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['attribute-values', companyId, definition?.id] }),
  });

  if (!definition) return null;

  return (
    <Card className="mt-4">
      <h3 className="mb-2 text-base font-semibold text-text">Values for "{definition.name}"</h3>
      {valuesQuery.isLoading ? (
        <p className="text-sm text-text-muted">Loading…</p>
      ) : valuesQuery.data && valuesQuery.data.length > 0 ? (
        <table className="mb-3 w-full text-sm">
          <thead>
            <tr className="text-left text-text-muted">
              <th className="pb-1">Value</th>
              <th className="pb-1">Status</th>
              <th className="pb-1"></th>
            </tr>
          </thead>
          <tbody>
            {valuesQuery.data.map((v) => (
              <tr key={v.id} className="border-t border-border">
                <td className="py-1">{v.value}</td>
                <td className="py-1">{v.isActive ? 'Active' : 'Inactive'}</td>
                <td className="py-1 text-right">
                  {v.isActive && (
                    <Button
                      type="button"
                      variant="danger"
                      disabled={deactivateValue.isPending && deactivateValue.variables === v.id}
                      onClick={() => deactivateValue.mutate(v.id)}
                    >
                      Deactivate
                    </Button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      ) : (
        <p className="mb-3 text-sm text-text-muted">No values yet — add the first one below.</p>
      )}
      <form noValidate onSubmit={handleSubmit((values) => createValue.mutate(values))} className="grid grid-cols-1 gap-3 sm:grid-cols-3 sm:items-end">
        <label className="flex flex-col gap-1 text-sm sm:col-span-2">
          New value
          <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Red" {...register('value')} />
          {errors.value && <span className="text-xs text-danger">{errors.value.message}</span>}
        </label>
        <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Adding…' : 'Add value'}</Button>
      </form>
      {createValue.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not add that value.</p>
      )}
      {deactivateValue.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that value.</p>
      )}
    </Card>
  );
}
