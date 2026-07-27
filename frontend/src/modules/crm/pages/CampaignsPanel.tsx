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
import type { PagedResult } from '../../../shared/api/types';

const MEMBER_ENTITY_TYPES = ['Lead', 'Contact'] as const;

const schema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  description: z.string().max(2000).or(z.literal('')),
  startDate: z.string().min(1, 'Start date is required'),
  endDate: z.string().or(z.literal('')),
});
type FormValues = z.infer<typeof schema>;

const memberSchema = z.object({
  memberEntityType: z.enum(MEMBER_ENTITY_TYPES),
  memberEntityId: z.string().uuid('Enter a valid id'),
});
type MemberFormValues = z.infer<typeof memberSchema>;

interface CampaignDto {
  id: string;
  name: string;
  description: string | null;
  startDate: string;
  endDate: string | null;
  isActive: boolean;
}
interface CampaignMembershipDto {
  id: string;
  campaignId: string;
  entityType: string;
  entityId: string;
  addedAt: string;
}
interface CampaignWithMembersDto {
  campaign: CampaignDto;
  members: CampaignMembershipDto[];
}

/** Marketing campaigns + lead/contact membership (Phase 7, 2026-07-23). */
export function CampaignsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [managingCampaignId, setManagingCampaignId] = useState<string | null>(null);

  const campaignsQuery = useQuery({
    queryKey: ['campaigns', companyId],
    queryFn: () => apiClient.get<PagedResult<CampaignDto>>(`/crm/campaigns?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { name: '', description: '', startDate: '', endDate: '' },
  });

  const createCampaign = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<CampaignDto>('/crm/campaigns', {
        companyId,
        name: values.name,
        description: values.description || null,
        startDate: values.startDate,
        endDate: values.endDate || null,
      }),
    onSuccess: () => {
      reset({ name: '', description: '', startDate: '', endDate: '' });
      queryClient.invalidateQueries({ queryKey: ['campaigns', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const deactivateCampaign = useMutation({
    mutationFn: (id: string) => apiClient.post(`/crm/campaigns/${id}/deactivate?companyId=${companyId}`, undefined),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['campaigns', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Campaigns</h2>
      <p className="mb-3 text-xs text-text-muted">Marketing campaigns and which leads/contacts are enrolled in each.</p>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createCampaign.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            Name
            <Controller control={control} name="name" render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )} />
            {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            Description (optional)
            <Controller control={control} name="description" render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )} />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Start date
            <Controller control={control} name="startDate" render={({ field }) => (
              <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )} />
            {errors.startDate && <span className="text-xs text-danger">{errors.startDate.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            End date (optional)
            <Controller control={control} name="endDate" render={({ field }) => (
              <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )} />
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create campaign'}</Button>
          </div>
        </form>
        {createCampaign.isError && createCampaign.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createCampaign.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Name', render: (row: CampaignDto) => row.name },
            { header: 'Start', render: (row: CampaignDto) => new Date(row.startDate).toLocaleDateString() },
            { header: 'End', render: (row: CampaignDto) => (row.endDate ? new Date(row.endDate).toLocaleDateString() : '—') },
            { header: 'Status', render: (row: CampaignDto) => (row.isActive ? 'Active' : 'Inactive') },
            {
              header: 'Actions',
              render: (row: CampaignDto) => (
                <div className="flex items-center gap-2">
                  <Button type="button" variant="secondary" onClick={() => setManagingCampaignId(row.id)}>Manage members</Button>
                  <Button
                    type="button"
                    variant="danger"
                    disabled={!row.isActive || deactivateCampaign.isPending}
                    onClick={() => deactivateCampaign.mutate(row.id)}
                  >
                    {row.isActive ? 'Deactivate' : 'Deactivated'}
                  </Button>
                </div>
              ),
            },
          ]}
          rows={campaignsQuery.data?.data}
          isLoading={campaignsQuery.isLoading}
          isError={campaignsQuery.isError}
          errorMessage="Could not load campaigns."
          emptyMessage="No campaigns yet."
          rowKey={(row) => row.id}
        />
      </Card>

      {managingCampaignId && (
        <CampaignMembersPanel companyId={companyId} campaignId={managingCampaignId} onClose={() => setManagingCampaignId(null)} />
      )}
    </div>
  );
}

interface CampaignMembersPanelProps {
  companyId: string;
  campaignId: string;
  onClose: () => void;
}

function CampaignMembersPanel({ companyId, campaignId, onClose }: CampaignMembersPanelProps) {
  const queryClient = useQueryClient();

  const detailQuery = useQuery({
    queryKey: ['campaign', companyId, campaignId],
    queryFn: () => apiClient.get<CampaignWithMembersDto>(`/crm/campaigns/${campaignId}?companyId=${companyId}`),
  });

  const { control, handleSubmit, reset, formState: { isSubmitting } } = useForm<MemberFormValues>({
    resolver: zodResolver(memberSchema),
    defaultValues: { memberEntityType: 'Lead', memberEntityId: '' },
  });

  const addMember = useMutation({
    mutationFn: (values: MemberFormValues) =>
      apiClient.post(`/crm/campaigns/${campaignId}/members`, { companyId, ...values }),
    onSuccess: () => {
      reset({ memberEntityType: 'Lead', memberEntityId: '' });
      queryClient.invalidateQueries({ queryKey: ['campaign', companyId, campaignId] });
    },
  });

  const removeMember = useMutation({
    mutationFn: (member: CampaignMembershipDto) =>
      apiClient.delete(`/crm/campaigns/${campaignId}/members?companyId=${companyId}&memberEntityType=${member.entityType}&memberEntityId=${member.entityId}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['campaign', companyId, campaignId] }),
  });

  return (
    <Card className="mt-6">
      <div className="mb-3 flex items-center justify-between">
        <h2 className="text-lg font-semibold text-text">Campaign members — {detailQuery.data?.campaign.name}</h2>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>

      <form noValidate onSubmit={handleSubmit((values) => addMember.mutate(values))} className="mb-4 grid grid-cols-1 gap-3 sm:grid-cols-3">
        <label className="flex flex-col gap-1 text-sm">
          Type
          <Controller control={control} name="memberEntityType" render={({ field }) => (
            <select className="rounded-md border border-border bg-surface px-2 py-1.5" {...field}>
              {MEMBER_ENTITY_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
            </select>
          )} />
        </label>
        <label className="flex flex-col gap-1 text-sm sm:col-span-2">
          Entity id
          <Controller control={control} name="memberEntityId" render={({ field }) => (
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Lead/Contact id" {...field} />
          )} />
        </label>
        <div className="sm:col-span-3">
          <Button type="submit" disabled={isSubmitting}>Add member</Button>
        </div>
      </form>

      <ul className="space-y-2 text-sm">
        {detailQuery.data?.members.map((member) => (
          <li key={member.id} className="flex items-center justify-between rounded-md border border-border px-3 py-2">
            <span>{member.entityType} — {member.entityId}</span>
            <Button type="button" variant="danger" disabled={removeMember.isPending} onClick={() => removeMember.mutate(member)}>
              Remove
            </Button>
          </li>
        ))}
        {detailQuery.data && detailQuery.data.members.length === 0 && (
          <li className="text-text-muted">No members enrolled yet.</li>
        )}
      </ul>
    </Card>
  );
}
