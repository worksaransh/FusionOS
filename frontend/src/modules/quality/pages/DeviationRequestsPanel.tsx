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
import { useInspectionOptions, useUserOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  inspectionId: z.string().uuid().or(z.literal('')),
  reason: z.string().min(1, 'Reason is required').max(2000),
  requestedByUserId: z.string().uuid('Pick who is requesting this deviation'),
});
type FormValues = z.infer<typeof schema>;

const decisionSchema = z.object({
  decidedByUserId: z.string().uuid('Pick who is deciding this request'),
  note: z.string().max(2000).or(z.literal('')),
});
type DecisionFormValues = z.infer<typeof decisionSchema>;

interface DeviationRequestDto {
  id: string;
  inspectionId: string | null;
  reason: string;
  status: string;
  requestedByUserId: string;
  approvedOrRejectedByUserId: string | null;
  decisionNote: string | null;
}

/**
 * Deviation/Waiver requests — accepting an out-of-spec inspection result (or a
 * known-in-advance planned deviation, InspectionId left blank) anyway, subject to a
 * separate approver's decision. Pending -> Approved or Pending -> Rejected, both terminal.
 * Rendered as a sibling panel under InspectionsPage, same pattern as
 * CorrectiveActionsPanel/QualityAuditsPanel.
 */
export function DeviationRequestsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [decidingId, setDecidingId] = useState<string | null>(null);

  const inspectionOptions = useInspectionOptions(companyId);
  const userOptions = useUserOptions(companyId);

  const deviationsQuery = useQuery({
    queryKey: ['deviation-requests', companyId],
    queryFn: () => apiClient.get<PagedResult<DeviationRequestDto>>(`/quality/deviations?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { inspectionId: '', reason: '', requestedByUserId: '' },
  });

  const createDeviation = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<DeviationRequestDto>('/quality/deviations', {
        companyId,
        inspectionId: values.inspectionId || null,
        reason: values.reason,
        requestedByUserId: values.requestedByUserId,
      }),
    onSuccess: () => {
      reset({ inspectionId: '', reason: '', requestedByUserId: '' });
      queryClient.invalidateQueries({ queryKey: ['deviation-requests', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const approveDeviation = useMutation({
    mutationFn: ({ id, decidedByUserId, note }: { id: string; decidedByUserId: string; note: string | null }) =>
      apiClient.post<DeviationRequestDto>(`/quality/deviations/${id}/approve`, { companyId, decidedByUserId, note }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['deviation-requests', companyId] });
      setDecidingId(null);
    },
  });

  const rejectDeviation = useMutation({
    mutationFn: ({ id, decidedByUserId, note }: { id: string; decidedByUserId: string; note: string | null }) =>
      apiClient.post<DeviationRequestDto>(`/quality/deviations/${id}/reject`, { companyId, decidedByUserId, note }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['deviation-requests', companyId] });
      setDecidingId(null);
    },
  });

  if (!companyId) return null;

  const decidingRequest = deviationsQuery.data?.data.find((d) => d.id === decidingId) ?? null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Deviation / Waiver Requests</h2>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createDeviation.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            Inspection (optional — leave blank for a planned deviation with no failed inspection yet)
            <Controller
              control={control}
              name="inspectionId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={inspectionOptions.options}
                  isLoading={inspectionOptions.isLoading}
                  placeholder="Search inspections…"
                />
              )}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            Reason
            <Controller
              control={control}
              name="reason"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Why the out-of-spec result should be accepted" {...field} />
              )}
            />
            {errors.reason && <span className="text-xs text-danger">{errors.reason.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Requested by
            <Controller
              control={control}
              name="requestedByUserId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={userOptions.options}
                  isLoading={userOptions.isLoading}
                  onSearchChange={userOptions.onSearchChange}
                  placeholder="Search users…"
                />
              )}
            />
            {errors.requestedByUserId && <span className="text-xs text-danger">{errors.requestedByUserId.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Requesting…' : 'Request deviation'}</Button>
          </div>
        </form>
        {createDeviation.isError && createDeviation.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createDeviation.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            {
              header: 'Inspection',
              render: (row: DeviationRequestDto) =>
                row.inspectionId ? (inspectionOptions.options.find((i) => i.id === row.inspectionId)?.label ?? row.inspectionId) : '—',
            },
            { header: 'Reason', render: (row: DeviationRequestDto) => row.reason },
            { header: 'Requested by', render: (row: DeviationRequestDto) => userOptions.options.find((u) => u.id === row.requestedByUserId)?.label ?? row.requestedByUserId },
            { header: 'Status', render: (row: DeviationRequestDto) => row.status },
            {
              header: 'Actions',
              render: (row: DeviationRequestDto) =>
                row.status === 'Pending' ? (
                  <Button type="button" variant="secondary" onClick={() => setDecidingId(row.id)}>
                    Decide
                  </Button>
                ) : (
                  row.decisionNote ?? null
                ),
            },
          ]}
          rows={deviationsQuery.data?.data}
          isLoading={deviationsQuery.isLoading}
          isError={deviationsQuery.isError}
          errorMessage="Could not load deviation requests."
          emptyMessage="No deviation requests yet — request the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {(approveDeviation.isError || rejectDeviation.isError) && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not update that deviation request.</p>
      )}

      {decidingRequest && (
        <DecisionPanel
          userOptions={userOptions}
          isSubmitting={approveDeviation.isPending || rejectDeviation.isPending}
          onApprove={(decidedByUserId, note) => approveDeviation.mutate({ id: decidingRequest.id, decidedByUserId, note: note || null })}
          onReject={(decidedByUserId, note) => rejectDeviation.mutate({ id: decidingRequest.id, decidedByUserId, note: note || null })}
          onClose={() => setDecidingId(null)}
        />
      )}
    </div>
  );
}

interface DecisionPanelProps {
  userOptions: ReturnType<typeof useUserOptions>;
  isSubmitting: boolean;
  onApprove: (decidedByUserId: string, note: string) => void;
  onReject: (decidedByUserId: string, note: string) => void;
  onClose: () => void;
}

function DecisionPanel({ userOptions, isSubmitting, onApprove, onReject, onClose }: DecisionPanelProps) {
  const { control, handleSubmit, formState: { errors } } = useForm<DecisionFormValues>({
    resolver: zodResolver(decisionSchema),
    defaultValues: { decidedByUserId: '', note: '' },
  });

  return (
    <Card className="mt-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-base font-semibold text-text">Decide deviation request</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form noValidate className="flex flex-col gap-3">
        <label className="flex flex-col gap-1 text-sm">
          Decided by
          <Controller
            control={control}
            name="decidedByUserId"
            render={({ field }) => (
              <EntityCombobox
                value={field.value}
                onChange={field.onChange}
                options={userOptions.options}
                isLoading={userOptions.isLoading}
                onSearchChange={userOptions.onSearchChange}
                placeholder="Search users…"
              />
            )}
          />
          {errors.decidedByUserId && <span className="text-xs text-danger">{errors.decidedByUserId.message}</span>}
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Note (optional)
          <Controller
            control={control}
            name="note"
            render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )}
          />
        </label>
        <div className="flex items-center gap-3">
          <Button
            type="button"
            disabled={isSubmitting}
            onClick={handleSubmit((values) => onApprove(values.decidedByUserId, values.note))}
          >
            Approve
          </Button>
          <Button
            type="button"
            variant="secondary"
            disabled={isSubmitting}
            onClick={handleSubmit((values) => onReject(values.decidedByUserId, values.note))}
          >
            Reject
          </Button>
        </div>
      </form>
    </Card>
  );
}
