import { useState } from 'react';
import { Controller, useFieldArray, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Plus, Trash2 } from 'lucide-react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { EntityCombobox } from '../../../shared/ui/EntityCombobox';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import { useUserOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  entityType: z.string().min(1, 'Entity type is required (e.g. PurchaseOrder)'),
  entityId: z.string().uuid('Must be a valid id'),
  approvers: z.array(z.object({ userId: z.string().uuid('Pick a user') })).min(1, 'At least one approval step is required'),
});
type FormValues = z.infer<typeof schema>;

interface ApprovalStepDto {
  id: string;
  stepNumber: number;
  approverUserId: string;
  decision: string;
  decidedAt: string | null;
  comments: string | null;
}

interface ApprovalRequestDto {
  id: string;
  entityType: string;
  entityId: string;
  requestedBy: string;
  status: string;
  currentStepNumber: number;
  steps: ApprovalStepDto[];
  createdAt: string;
}

/**
 * Generic multi-step Approval workflow engine (Phase M7, 2026-07-15) —
 * frontend for FusionOS.Modules.Core.Domain.Workflow.ApprovalRequest. Not
 * wired into any specific module's existing Approve() action yet (see the
 * domain's own doc comment for why) — this page is where any (EntityType,
 * EntityId) pair can be submitted for approval directly, and where the
 * signed-in user sees and decides whatever's pending for them right now.
 */
export function ApprovalsPage() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const userOptions = useUserOptions(companyId);
  const [commentsByRequestId, setCommentsByRequestId] = useState<Record<string, string>>({});

  const { control, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { entityType: '', entityId: '', approvers: [{ userId: '' }] },
  });
  const { fields, append, remove } = useFieldArray({ control, name: 'approvers' });

  const pendingQuery = useQuery({
    queryKey: ['approvals-pending-for-me', companyId],
    queryFn: () => apiClient.get<PagedResult<ApprovalRequestDto>>(`/core/approvals/pending-for-me?companyId=${companyId}&page=1&pageSize=25`),
    enabled: Boolean(companyId),
  });

  const createRequest = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<ApprovalRequestDto>('/core/approvals', {
        companyId,
        entityType: values.entityType.trim(),
        entityId: values.entityId,
        approverUserIds: values.approvers.map((a) => a.userId),
      }),
    onSuccess: () => {
      reset({ entityType: '', entityId: '', approvers: [{ userId: '' }] });
      queryClient.invalidateQueries({ queryKey: ['approvals-pending-for-me', companyId] });
    },
  });

  const decide = useMutation({
    mutationFn: ({ id, approve }: { id: string; approve: boolean }) =>
      apiClient.post<ApprovalRequestDto>(`/core/approvals/${id}/decide`, {
        companyId,
        approve,
        comments: commentsByRequestId[id] || null,
      }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['approvals-pending-for-me', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div>
      <h1 className="mb-1 text-xl font-semibold text-text">Approvals</h1>
      <p className="mb-6 text-sm text-text-muted">
        Submit a multi-step approval request for anything, and decide whatever's currently waiting on you.
      </p>

      <h2 className="mb-3 text-lg font-semibold text-text">My Pending Approvals</h2>
      <Card className="mb-8">
        <DataTable
          columns={[
            { header: 'Entity', render: (r: ApprovalRequestDto) => `${r.entityType} ${r.entityId}` },
            { header: 'Step', render: (r: ApprovalRequestDto) => `${r.currentStepNumber} of ${r.steps.length}` },
            { header: 'Requested', render: (r: ApprovalRequestDto) => new Date(r.createdAt).toLocaleDateString() },
            {
              header: 'Comments',
              render: (r: ApprovalRequestDto) => (
                <input
                  className="w-48 rounded-md border border-border bg-surface px-2 py-1 text-sm"
                  placeholder="Optional comments"
                  value={commentsByRequestId[r.id] ?? ''}
                  onChange={(e) => setCommentsByRequestId((prev) => ({ ...prev, [r.id]: e.target.value }))}
                />
              ),
            },
            {
              header: '',
              render: (r: ApprovalRequestDto) => (
                <div className="flex gap-2">
                  <Button variant="secondary" onClick={() => decide.mutate({ id: r.id, approve: true })} disabled={decide.isPending}>
                    Approve
                  </Button>
                  <Button variant="secondary" onClick={() => decide.mutate({ id: r.id, approve: false })} disabled={decide.isPending}>
                    Reject
                  </Button>
                </div>
              ),
            },
          ]}
          rows={pendingQuery.data?.data}
          isLoading={pendingQuery.isLoading}
          emptyMessage="Nothing is waiting on your approval right now."
          rowKey={(r) => r.id}
        />
        {decide.isError && decide.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{decide.error.problem.title}</p>
        )}
      </Card>

      <h2 className="mb-3 text-lg font-semibold text-text">Request an Approval</h2>
      <Card>
        <form onSubmit={handleSubmit((values) => createRequest.mutate(values))} className="flex flex-col gap-4">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <label className="flex flex-col gap-1 text-sm">
              Entity type
              <Controller
                control={control}
                name="entityType"
                render={({ field }) => (
                  <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="e.g. PurchaseOrder" {...field} />
                )}
              />
              {errors.entityType && <span className="text-xs text-danger">{errors.entityType.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Entity id
              <Controller
                control={control}
                name="entityId"
                render={({ field }) => (
                  <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="GUID of the thing being approved" {...field} />
                )}
              />
              {errors.entityId && <span className="text-xs text-danger">{errors.entityId.message}</span>}
            </label>
          </div>

          <div className="flex flex-col gap-2">
            <span className="text-sm">Approval steps, in order</span>
            {fields.map((field, index) => (
              <div key={field.id} className="flex items-center gap-2">
                <span className="w-6 text-xs text-text-muted">{index + 1}.</span>
                <Controller
                  control={control}
                  name={`approvers.${index}.userId`}
                  render={({ field: approverField }) => (
                    <EntityCombobox
                      className="w-72"
                      value={approverField.value}
                      onChange={approverField.onChange}
                      options={userOptions.options}
                      isLoading={userOptions.isLoading}
                      onSearchChange={userOptions.onSearchChange}
                      placeholder="Search users…"
                    />
                  )}
                />
                <Button type="button" variant="secondary" onClick={() => remove(index)} disabled={fields.length === 1}>
                  <Trash2 size={16} />
                </Button>
              </div>
            ))}
            {errors.approvers && typeof errors.approvers.message === 'string' && (
              <span className="text-xs text-danger">{errors.approvers.message}</span>
            )}
            <Button type="button" variant="secondary" onClick={() => append({ userId: '' })} className="w-fit">
              <Plus size={16} className="mr-1" /> Add step
            </Button>
          </div>

          <Button type="submit" disabled={isSubmitting} className="w-fit">
            {isSubmitting ? 'Submitting…' : 'Submit for approval'}
          </Button>
        </form>
        {createRequest.isError && createRequest.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createRequest.error.problem.title}</p>
        )}
      </Card>
    </div>
  );
}
