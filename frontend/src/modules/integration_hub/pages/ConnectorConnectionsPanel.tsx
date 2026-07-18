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
import { useIntegrationConnectorOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  integrationConnectorId: z.string().uuid('Pick a connector'),
  label: z.string().min(1, 'Label is required').max(200),
});
type FormValues = z.infer<typeof schema>;

interface ConnectorConnectionDto {
  id: string;
  integrationConnectorId: string;
  label: string;
  status: string;
  connectedAt: string;
  disconnectedAt: string | null;
}

/**
 * Connector Connections — a company's connections to catalog connectors,
 * Connected → Disconnected, or flagged Error. Rendered as a sibling panel
 * under IntegrationConnectorsPage, same pattern as PluginInstallationsPanel
 * under PluginListingsPage. No credential/API-key input here — this is
 * connection bookkeeping only, see ConnectorConnection's own class doc
 * comment for why real secret storage is deliberately out of scope.
 */
export function ConnectorConnectionsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const connectorOptions = useIntegrationConnectorOptions(companyId);

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { integrationConnectorId: '', label: '' },
  });

  const connectionsQuery = useQuery({
    queryKey: ['connector-connections', companyId],
    queryFn: () => apiClient.get<PagedResult<ConnectorConnectionDto>>(`/integration-hub/connections?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const connectConnector = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<ConnectorConnectionDto>('/integration-hub/connections', {
        companyId,
        integrationConnectorId: values.integrationConnectorId,
        label: values.label,
      }),
    onSuccess: () => {
      reset({ integrationConnectorId: '', label: '' });
      queryClient.invalidateQueries({ queryKey: ['connector-connections', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const disconnectConnection = useMutation({
    mutationFn: (id: string) => apiClient.post<ConnectorConnectionDto>(`/integration-hub/connections/${id}/disconnect`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['connector-connections', companyId] }),
  });

  const markErrorConnection = useMutation({
    mutationFn: (id: string) => apiClient.post<ConnectorConnectionDto>(`/integration-hub/connections/${id}/mark-error`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['connector-connections', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Connections</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => connectConnector.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Connector
            <Controller
              control={control}
              name="integrationConnectorId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={connectorOptions.options}
                  isLoading={connectorOptions.isLoading}
                  onSearchChange={connectorOptions.onSearchChange}
                  placeholder="Search connectors…"
                />
              )}
            />
            {errors.integrationConnectorId && <span className="text-xs text-danger">{errors.integrationConnectorId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Label
            <Controller
              control={control}
              name="label"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Main Shopify Store" {...field} />
              )}
            />
            {errors.label && <span className="text-xs text-danger">{errors.label.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Connecting…' : 'Connect'}</Button>
          </div>
        </form>
        {connectConnector.isError && connectConnector.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{connectConnector.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Label', render: (row: ConnectorConnectionDto) => row.label },
            { header: 'Connected', render: (row: ConnectorConnectionDto) => new Date(row.connectedAt).toLocaleDateString() },
            { header: 'Status', render: (row: ConnectorConnectionDto) => row.status },
            {
              header: 'Actions',
              render: (row: ConnectorConnectionDto) =>
                row.status !== 'Disconnected' ? (
                  <div className="flex items-center gap-2">
                    <Button type="button" variant="secondary" disabled={markErrorConnection.isPending} onClick={() => markErrorConnection.mutate(row.id)}>
                      Flag error
                    </Button>
                    <Button type="button" variant="danger" disabled={disconnectConnection.isPending} onClick={() => disconnectConnection.mutate(row.id)}>
                      Disconnect
                    </Button>
                  </div>
                ) : null,
            },
          ]}
          rows={connectionsQuery.data?.data}
          isLoading={connectionsQuery.isLoading}
          isError={connectionsQuery.isError}
          errorMessage="Could not load connections."
          emptyMessage="No connections yet — connect one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {(disconnectConnection.isError || markErrorConnection.isError) && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not update that connection.</p>
      )}
    </div>
  );
}
