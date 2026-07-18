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
import { useAssetOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const REQUEST_TYPES = ['Preventive', 'Breakdown'] as const;

const schema = z.object({
  assetId: z.string().uuid('Pick an asset'),
  type: z.enum(REQUEST_TYPES),
  description: z.string().min(1, 'Description is required').max(1000),
});
type FormValues = z.infer<typeof schema>;

const completeSchema = z.object({
  resolutionNotes: z.string().max(1000).or(z.literal('')),
});
type CompleteFormValues = z.infer<typeof completeSchema>;

interface MaintenanceRequestDto {
  id: string;
  assetId: string;
  type: string;
  description: string;
  status: string;
  reportedAt: string;
  completedAt: string | null;
  resolutionNotes: string | null;
}

/**
 * Maintenance Requests — preventive/breakdown requests against an Asset,
 * Open → InProgress → Completed. Completed requests, listed per Asset, are
 * this module's "maintenance history" (05_MODULE_ROADMAP.md). Rendered as a
 * sibling panel under AssetsPage, same pattern as WorkOrdersPanel under
 * BillsOfMaterialsPage. Completing is a small inline sub-form asking for
 * optional resolution notes, same "one more input needed" shape as CRM's
 * WinOpportunityPanel.
 */
export function MaintenanceRequestsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [completingRequestId, setCompletingRequestId] = useState<string | null>(null);

  const assetOptions = useAssetOptions(companyId);

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { assetId: '', type: 'Breakdown', description: '' },
  });

  const requestsQuery = useQuery({
    queryKey: ['maintenance-requests', companyId],
    queryFn: () => apiClient.get<PagedResult<MaintenanceRequestDto>>(`/maintenance/requests?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const createRequest = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<MaintenanceRequestDto>('/maintenance/requests', {
        companyId,
        assetId: values.assetId,
        type: values.type,
        description: values.description,
      }),
    onSuccess: () => {
      reset({ assetId: '', type: 'Breakdown', description: '' });
      queryClient.invalidateQueries({ queryKey: ['maintenance-requests', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const startRequest = useMutation({
    mutationFn: (id: string) => apiClient.post<MaintenanceRequestDto>(`/maintenance/requests/${id}/start`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['maintenance-requests', companyId] }),
  });

  if (!companyId) return null;

  const completingRequest = requestsQuery.data?.data.find((r) => r.id === completingRequestId) ?? null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Maintenance Requests</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createRequest.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <label className="flex flex-col gap-1 text-sm">
            Asset
            <Controller
              control={control}
              name="assetId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={assetOptions.options}
                  isLoading={assetOptions.isLoading}
                  onSearchChange={assetOptions.onSearchChange}
                  placeholder="Search assets…"
                />
              )}
            />
            {errors.assetId && <span className="text-xs text-danger">{errors.assetId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Type
            <Controller
              control={control}
              name="type"
              render={({ field }) => (
                <select className="rounded-md border border-border bg-surface px-2 py-1.5" {...field}>
                  {REQUEST_TYPES.map((type) => (
                    <option key={type} value={type}>{type}</option>
                  ))}
                </select>
              )}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Description
            <Controller
              control={control}
              name="description"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Motor overheating" {...field} />
              )}
            />
            {errors.description && <span className="text-xs text-danger">{errors.description.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Logging…' : 'Log request'}</Button>
          </div>
        </form>
        {createRequest.isError && createRequest.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createRequest.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Type', render: (row: MaintenanceRequestDto) => row.type },
            { header: 'Description', render: (row: MaintenanceRequestDto) => row.description },
            { header: 'Status', render: (row: MaintenanceRequestDto) => row.status },
            { header: 'Reported', render: (row: MaintenanceRequestDto) => new Date(row.reportedAt).toLocaleDateString() },
            {
              header: 'Actions',
              render: (row: MaintenanceRequestDto) => (
                <div className="flex items-center gap-2">
                  {row.status === 'Open' && (
                    <Button type="button" variant="secondary" disabled={startRequest.isPending} onClick={() => startRequest.mutate(row.id)}>
                      Start
                    </Button>
                  )}
                  {row.status === 'InProgress' && (
                    <Button type="button" onClick={() => setCompletingRequestId(row.id)}>
                      Complete
                    </Button>
                  )}
                </div>
              ),
            },
          ]}
          rows={requestsQuery.data?.data}
          isLoading={requestsQuery.isLoading}
          isError={requestsQuery.isError}
          errorMessage="Could not load maintenance requests."
          emptyMessage="No maintenance requests yet — log the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {startRequest.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not start that maintenance request.</p>
      )}

      {completingRequest && (
        <CompleteMaintenanceRequestPanel
          companyId={companyId}
          request={completingRequest}
          onClose={() => setCompletingRequestId(null)}
        />
      )}
    </div>
  );
}

interface CompleteMaintenanceRequestPanelProps {
  companyId: string;
  request: MaintenanceRequestDto;
  onClose: () => void;
}

function CompleteMaintenanceRequestPanel({ companyId, request, onClose }: CompleteMaintenanceRequestPanelProps) {
  const queryClient = useQueryClient();

  const { control, handleSubmit, formState: { isSubmitting } } = useForm<CompleteFormValues>({
    resolver: zodResolver(completeSchema),
    defaultValues: { resolutionNotes: '' },
  });

  const completeRequest = useMutation({
    mutationFn: (values: CompleteFormValues) =>
      apiClient.post<MaintenanceRequestDto>(`/maintenance/requests/${request.id}/complete`, {
        companyId,
        resolutionNotes: values.resolutionNotes || null,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['maintenance-requests', companyId] });
      onClose();
    },
  });

  return (
    <Card className="mt-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-base font-semibold text-text">Complete request — {request.description}</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form onSubmit={handleSubmit((values) => completeRequest.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <label className="flex flex-col gap-1 text-sm">
          Resolution notes (optional)
          <Controller
            control={control}
            name="resolutionNotes"
            render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )}
          />
        </label>
        <div className="col-span-2 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Mark completed'}</Button>
          {completeRequest.isError && (
            <span role="alert" className="text-sm text-danger">Could not complete that request.</span>
          )}
        </div>
      </form>
    </Card>
  );
}
