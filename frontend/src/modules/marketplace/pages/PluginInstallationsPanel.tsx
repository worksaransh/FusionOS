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
import { usePluginListingOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  pluginListingId: z.string().uuid('Pick a plugin listing'),
});
type FormValues = z.infer<typeof schema>;

interface PluginInstallationDto {
  id: string;
  pluginListingId: string;
  status: string;
  installedAt: string;
  uninstalledAt: string | null;
}

/**
 * Plugin Installations — a company's installs of catalog listings, Installed
 * → Disabled/Uninstalled. Rendered as a sibling panel under
 * PluginListingsPage, same pattern as KpiSnapshotsPanel under
 * KpiDefinitionsPage. This is install bookkeeping only — no real plugin
 * execution/sandboxing runtime exists yet, see PluginInstallation's own
 * class doc comment.
 */
export function PluginInstallationsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const listingOptions = usePluginListingOptions(companyId);

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { pluginListingId: '' },
  });

  const installationsQuery = useQuery({
    queryKey: ['plugin-installations', companyId],
    queryFn: () => apiClient.get<PagedResult<PluginInstallationDto>>(`/marketplace/plugin-installations?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const installPlugin = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<PluginInstallationDto>('/marketplace/plugin-installations', {
        companyId,
        pluginListingId: values.pluginListingId,
      }),
    onSuccess: () => {
      reset({ pluginListingId: '' });
      queryClient.invalidateQueries({ queryKey: ['plugin-installations', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const disableInstallation = useMutation({
    mutationFn: (id: string) => apiClient.post<PluginInstallationDto>(`/marketplace/plugin-installations/${id}/disable`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['plugin-installations', companyId] }),
  });

  const enableInstallation = useMutation({
    mutationFn: (id: string) => apiClient.post<PluginInstallationDto>(`/marketplace/plugin-installations/${id}/enable`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['plugin-installations', companyId] }),
  });

  const uninstallPlugin = useMutation({
    mutationFn: (id: string) => apiClient.post<PluginInstallationDto>(`/marketplace/plugin-installations/${id}/uninstall`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['plugin-installations', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Installed Plugins</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => installPlugin.mutate(values))} className="flex flex-col gap-4 sm:flex-row sm:items-end">
          <label className="flex flex-1 flex-col gap-1 text-sm">
            Plugin listing
            <Controller
              control={control}
              name="pluginListingId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={listingOptions.options}
                  isLoading={listingOptions.isLoading}
                  onSearchChange={listingOptions.onSearchChange}
                  placeholder="Search plugin listings…"
                />
              )}
            />
            {errors.pluginListingId && <span className="text-xs text-danger">{errors.pluginListingId.message}</span>}
          </label>
          <Button type="submit" disabled={isSubmitting} className="w-fit">{isSubmitting ? 'Installing…' : 'Install'}</Button>
        </form>
        {installPlugin.isError && installPlugin.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{installPlugin.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Installed', render: (row: PluginInstallationDto) => new Date(row.installedAt).toLocaleDateString() },
            { header: 'Status', render: (row: PluginInstallationDto) => row.status },
            {
              header: 'Actions',
              render: (row: PluginInstallationDto) => (
                <div className="flex items-center gap-2">
                  {row.status === 'Installed' && (
                    <Button type="button" variant="secondary" disabled={disableInstallation.isPending} onClick={() => disableInstallation.mutate(row.id)}>
                      Disable
                    </Button>
                  )}
                  {row.status === 'Disabled' && (
                    <Button type="button" variant="secondary" disabled={enableInstallation.isPending} onClick={() => enableInstallation.mutate(row.id)}>
                      Enable
                    </Button>
                  )}
                  {row.status !== 'Uninstalled' && (
                    <Button type="button" variant="danger" disabled={uninstallPlugin.isPending} onClick={() => uninstallPlugin.mutate(row.id)}>
                      Uninstall
                    </Button>
                  )}
                </div>
              ),
            },
          ]}
          rows={installationsQuery.data?.data}
          isLoading={installationsQuery.isLoading}
          isError={installationsQuery.isError}
          errorMessage="Could not load installed plugins."
          emptyMessage="No plugins installed yet — install one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {(disableInstallation.isError || enableInstallation.isError || uninstallPlugin.isError) && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not update that installation.</p>
      )}
    </div>
  );
}
