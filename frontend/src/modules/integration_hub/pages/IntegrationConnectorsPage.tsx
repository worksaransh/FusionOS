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
import { ConnectorConnectionsPanel } from './ConnectorConnectionsPanel';

const SEARCH_DEBOUNCE_MS = 250;

const CATEGORIES = ['Ecommerce', 'Shipping', 'Payment', 'Messaging', 'Email'] as const;

const schema = z.object({
  code: z.string().min(1, 'Code is required').max(20),
  name: z.string().min(1, 'Name is required').max(200),
  provider: z.string().min(1, 'Provider is required').max(100),
  category: z.enum(CATEGORIES),
});
type FormValues = z.infer<typeof schema>;

interface IntegrationConnectorDto {
  id: string;
  code: string;
  name: string;
  provider: string;
  category: string;
  isActive: boolean;
}

/**
 * Integration Connectors — Integration Hub's first real frontend slice
 * (backend has existed since this pass; this closes the "frontend panel
 * deferred" gap the same day the backend slice was built). The connector
 * catalog (05_MODULE_ROADMAP.md's IntegrationHub line item — Shopify,
 * WooCommerce, Amazon, Flipkart, ONDC, Shiprocket, Delhivery, Razorpay,
 * Stripe, WhatsApp, Email). Top-level page for /integration_hub, with
 * ConnectorConnectionsPanel rendered as a sibling panel below it, same
 * pattern as PluginInstallationsPanel under PluginListingsPage.
 */
export function IntegrationConnectorsPage() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebouncedValue(search, SEARCH_DEBOUNCE_MS);

  const connectorsQuery = useQuery({
    queryKey: ['integration-connectors', companyId, debouncedSearch],
    queryFn: () => {
      const params = new URLSearchParams({ companyId, page: '1', pageSize: '50' });
      if (debouncedSearch.trim()) params.set('search', debouncedSearch.trim());
      return apiClient.get<PagedResult<IntegrationConnectorDto>>(`/integration-hub/connectors?${params.toString()}`);
    },
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { code: '', name: '', provider: '', category: 'Ecommerce' },
  });

  const createConnector = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<IntegrationConnectorDto>('/integration-hub/connectors', {
        companyId,
        code: values.code,
        name: values.name,
        provider: values.provider,
        category: values.category,
      }),
    onSuccess: () => {
      reset({ code: '', name: '', provider: '', category: 'Ecommerce' });
      queryClient.invalidateQueries({ queryKey: ['integration-connectors', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  // Soft-deactivate only — IntegrationConnectorsController exposes this as a
  // dedicated POST .../{id}/deactivate action, same convention as
  // PluginListingsController/KpiDefinitionsController.
  const deactivateConnector = useMutation({
    mutationFn: (connectorId: string) => apiClient.post<IntegrationConnectorDto>(`/integration-hub/connectors/${connectorId}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['integration-connectors', companyId] }),
  });

  if (!companyId) {
    return <p className="text-text-muted">Set an active Company ID in the header above to manage integration connectors.</p>;
  }

  return (
    <div>
      <h1 className="mb-1 text-xl font-semibold text-text">Integration Connectors</h1>
      <p className="mb-4 text-sm text-text-muted">The connector catalog — Integration Hub, Phase 9.</p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createConnector.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Code
            <Controller
              control={control}
              name="code"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="SHOPIFY" {...field} />
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
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Shopify Store Sync" {...field} />
              )}
            />
            {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Provider
            <Controller
              control={control}
              name="provider"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Shopify" {...field} />
              )}
            />
            {errors.provider && <span className="text-xs text-danger">{errors.provider.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Category
            <Controller
              control={control}
              name="category"
              render={({ field }) => (
                <select className="rounded-md border border-border bg-surface px-2 py-1.5" {...field}>
                  {CATEGORIES.map((category) => (
                    <option key={category} value={category}>{category}</option>
                  ))}
                </select>
              )}
            />
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Add connector'}</Button>
          </div>
        </form>
        {createConnector.isError && createConnector.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createConnector.error.problem.title}</p>
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
            { header: 'Code', render: (row: IntegrationConnectorDto) => row.code },
            { header: 'Name', render: (row: IntegrationConnectorDto) => row.name },
            { header: 'Provider', render: (row: IntegrationConnectorDto) => row.provider },
            { header: 'Category', render: (row: IntegrationConnectorDto) => row.category },
            { header: 'Status', render: (row: IntegrationConnectorDto) => (row.isActive ? 'Active' : 'Inactive') },
            {
              header: 'Actions',
              render: (row: IntegrationConnectorDto) => (
                <Button
                  type="button"
                  variant="danger"
                  disabled={!row.isActive || deactivateConnector.isPending}
                  onClick={() => deactivateConnector.mutate(row.id)}
                >
                  {row.isActive ? 'Deactivate' : 'Deactivated'}
                </Button>
              ),
            },
          ]}
          rows={connectorsQuery.data?.data}
          isLoading={connectorsQuery.isLoading}
          isError={connectorsQuery.isError}
          errorMessage="Could not load integration connectors."
          emptyMessage="No connectors yet — add the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {deactivateConnector.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that connector.</p>
      )}

      <ConnectorConnectionsPanel />
    </div>
  );
}
