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
  credentialReference: string | null;
}

interface ConnectorSyncRunDto {
  id: string;
  connectorConnectionId: string;
  startedAt: string;
  completedAt: string | null;
  status: string;
  recordsProcessed: number;
  errorMessage: string | null;
}

/**
 * Connector Connections — a company's connections to catalog connectors,
 * Connected → Disconnected, or flagged Error. Rendered as a sibling panel
 * under IntegrationConnectorsPage, same pattern as PluginInstallationsPanel
 * under PluginListingsPage.
 *
 * Phase 8 (2026-07-25) adds the credential-reference and sync-run-history UI:
 * "Configure credential" only ever sets a config-key NAME (e.g.
 * "SHOPIFY_STORE_ACME_API_KEY") — never a real API key/token/secret value, so this
 * is a plain text input, not a password field, and nothing here ever touches a real
 * credential. "Trigger sync" and the sync-run history table are the visible surface
 * of ConnectorSyncRun / TriggerConnectorSyncCommand / IConnectorSyncAdapter.
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

  const configureCredential = useMutation({
    mutationFn: ({ id, credentialReference }: { id: string; credentialReference: string }) =>
      apiClient.post<ConnectorConnectionDto>(`/integration-hub/connections/${id}/credential`, { companyId, credentialReference }),
    onSuccess: () => {
      setEditingCredentialId(null);
      queryClient.invalidateQueries({ queryKey: ['connector-connections', companyId] });
    },
  });

  const triggerSync = useMutation({
    mutationFn: (id: string) => apiClient.post<ConnectorSyncRunDto>(`/integration-hub/connections/${id}/sync`, { companyId }),
    onSuccess: (_data, id) => {
      queryClient.invalidateQueries({ queryKey: ['connector-sync-runs', companyId, id] });
    },
  });

  const [editingCredentialId, setEditingCredentialId] = useState<string | null>(null);
  const [credentialDraft, setCredentialDraft] = useState('');
  const [expandedHistoryId, setExpandedHistoryId] = useState<string | null>(null);

  const syncRunsQuery = useQuery({
    queryKey: ['connector-sync-runs', companyId, expandedHistoryId],
    queryFn: () =>
      apiClient.get<PagedResult<ConnectorSyncRunDto>>(
        `/integration-hub/connections/${expandedHistoryId}/sync-runs?companyId=${companyId}&page=1&pageSize=25`,
      ),
    enabled: Boolean(companyId) && Boolean(expandedHistoryId),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Connections</h2>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => connectConnector.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
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
              header: 'Credential reference',
              render: (row: ConnectorConnectionDto) =>
                editingCredentialId === row.id ? (
                  <div className="flex items-center gap-2">
                    <input
                      className="w-48 rounded-md border border-border bg-surface px-2 py-1 text-sm"
                      placeholder="SHOPIFY_STORE_ACME_API_KEY"
                      value={credentialDraft}
                      onChange={(e) => setCredentialDraft(e.target.value)}
                      autoFocus
                    />
                    <Button
                      type="button"
                      disabled={configureCredential.isPending || !credentialDraft.trim()}
                      onClick={() => configureCredential.mutate({ id: row.id, credentialReference: credentialDraft.trim() })}
                    >
                      Save
                    </Button>
                    <Button type="button" variant="secondary" onClick={() => setEditingCredentialId(null)}>
                      Cancel
                    </Button>
                  </div>
                ) : (
                  <div className="flex items-center gap-2">
                    <span className="text-sm text-text-muted">{row.credentialReference ?? 'Not configured'}</span>
                    <Button
                      type="button"
                      variant="secondary"
                      disabled={row.status === 'Disconnected'}
                      onClick={() => {
                        setEditingCredentialId(row.id);
                        setCredentialDraft(row.credentialReference ?? '');
                      }}
                    >
                      Configure
                    </Button>
                  </div>
                ),
            },
            {
              header: 'Actions',
              render: (row: ConnectorConnectionDto) => (
                <div className="flex flex-wrap items-center gap-2">
                  <Button
                    type="button"
                    variant="secondary"
                    disabled={row.status === 'Disconnected' || triggerSync.isPending}
                    onClick={() => triggerSync.mutate(row.id)}
                  >
                    Trigger sync
                  </Button>
                  <Button
                    type="button"
                    variant="secondary"
                    onClick={() => setExpandedHistoryId(expandedHistoryId === row.id ? null : row.id)}
                  >
                    {expandedHistoryId === row.id ? 'Hide sync history' : 'Sync history'}
                  </Button>
                  {row.status !== 'Disconnected' && (
                    <>
                      <Button type="button" variant="secondary" disabled={markErrorConnection.isPending} onClick={() => markErrorConnection.mutate(row.id)}>
                        Flag error
                      </Button>
                      <Button type="button" variant="danger" disabled={disconnectConnection.isPending} onClick={() => disconnectConnection.mutate(row.id)}>
                        Disconnect
                      </Button>
                    </>
                  )}
                </div>
              ),
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
      {(disconnectConnection.isError || markErrorConnection.isError || configureCredential.isError || triggerSync.isError) && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not update that connection.</p>
      )}

      {expandedHistoryId && (
        <Card className="mt-4">
          <h3 className="mb-3 text-sm font-semibold text-text">Sync run history</h3>
          <DataTable
            columns={[
              { header: 'Started', render: (row: ConnectorSyncRunDto) => new Date(row.startedAt).toLocaleString() },
              { header: 'Completed', render: (row: ConnectorSyncRunDto) => (row.completedAt ? new Date(row.completedAt).toLocaleString() : '—') },
              { header: 'Status', render: (row: ConnectorSyncRunDto) => row.status },
              { header: 'Records processed', render: (row: ConnectorSyncRunDto) => row.recordsProcessed },
              { header: 'Error', render: (row: ConnectorSyncRunDto) => row.errorMessage ?? '—' },
            ]}
            rows={syncRunsQuery.data?.data}
            isLoading={syncRunsQuery.isLoading}
            isError={syncRunsQuery.isError}
            errorMessage="Could not load sync run history."
            emptyMessage="No sync runs yet — trigger one above."
            rowKey={(row) => row.id}
          />
        </Card>
      )}
    </div>
  );
}
