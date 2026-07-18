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
import { useLeadOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  leadId: z.string().uuid('Pick a lead'),
  name: z.string().min(1, 'Name is required').max(200),
  estimatedValue: z.string().refine((v) => Number(v) >= 0, 'Estimated value cannot be negative'),
});
type FormValues = z.infer<typeof schema>;

const winSchema = z.object({
  customerCode: z.string().min(1, 'Customer code is required').max(20),
});
type WinFormValues = z.infer<typeof winSchema>;

interface OpportunityDto {
  id: string;
  leadId: string;
  name: string;
  customerName: string;
  contactEmail: string | null;
  estimatedValue: number;
  stage: string;
  customerCode: string | null;
}

/**
 * Opportunities — a deal opened from a qualified lead, Open → Won / Lost.
 * Winning creates the real Sales Customer (WinOpportunityCommand takes the
 * new customer's Code) — surfaced here as a small inline form rather than a
 * one-click action, same "needs one more input" shape as
 * DeactivateCostCenter vs UpdateCostCenter's edit panel. Rendered as a
 * sibling panel under LeadsPage, same pattern as JournalEntriesPanel under
 * AccountsPage.
 */
export function OpportunitiesPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [winningOpportunityId, setWinningOpportunityId] = useState<string | null>(null);

  const leadOptions = useLeadOptions(companyId);

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { leadId: '', name: '', estimatedValue: '0' },
  });

  const opportunitiesQuery = useQuery({
    queryKey: ['opportunities', companyId],
    queryFn: () => apiClient.get<PagedResult<OpportunityDto>>(`/crm/opportunities?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const createOpportunity = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<OpportunityDto>('/crm/opportunities', {
        companyId,
        leadId: values.leadId,
        name: values.name,
        estimatedValue: Number(values.estimatedValue),
      }),
    onSuccess: () => {
      reset({ leadId: '', name: '', estimatedValue: '0' });
      queryClient.invalidateQueries({ queryKey: ['opportunities', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const loseOpportunity = useMutation({
    mutationFn: (id: string) => apiClient.post<OpportunityDto>(`/crm/opportunities/${id}/lose`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['opportunities', companyId] }),
  });

  if (!companyId) return null;

  const winningOpportunity = opportunitiesQuery.data?.data.find((o) => o.id === winningOpportunityId) ?? null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Opportunities</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createOpportunity.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <label className="flex flex-col gap-1 text-sm">
            Lead
            <Controller
              control={control}
              name="leadId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={leadOptions.options}
                  isLoading={leadOptions.isLoading}
                  onSearchChange={leadOptions.onSearchChange}
                  placeholder="Search leads…"
                />
              )}
            />
            {errors.leadId && <span className="text-xs text-danger">{errors.leadId.message}</span>}
          </label>
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
          <label className="flex flex-col gap-1 text-sm">
            Estimated value
            <Controller
              control={control}
              name="estimatedValue"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.estimatedValue && <span className="text-xs text-danger">{errors.estimatedValue.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create opportunity'}</Button>
          </div>
        </form>
        {createOpportunity.isError && createOpportunity.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createOpportunity.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Name', render: (row: OpportunityDto) => row.name },
            { header: 'Estimated value', render: (row: OpportunityDto) => row.estimatedValue.toLocaleString() },
            { header: 'Stage', render: (row: OpportunityDto) => row.stage },
            { header: 'Customer', render: (row: OpportunityDto) => row.customerCode ?? '—' },
            {
              header: 'Actions',
              render: (row: OpportunityDto) =>
                row.stage === 'Open' ? (
                  <div className="flex items-center gap-2">
                    <Button type="button" variant="secondary" onClick={() => setWinningOpportunityId(row.id)}>
                      Win
                    </Button>
                    <Button type="button" variant="danger" disabled={loseOpportunity.isPending} onClick={() => loseOpportunity.mutate(row.id)}>
                      Lose
                    </Button>
                  </div>
                ) : null,
            },
          ]}
          rows={opportunitiesQuery.data?.data}
          isLoading={opportunitiesQuery.isLoading}
          isError={opportunitiesQuery.isError}
          errorMessage="Could not load opportunities."
          emptyMessage="No opportunities yet — create the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {loseOpportunity.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not update that opportunity.</p>
      )}

      {winningOpportunity && (
        <WinOpportunityPanel
          companyId={companyId}
          opportunity={winningOpportunity}
          onClose={() => setWinningOpportunityId(null)}
        />
      )}
    </div>
  );
}

interface WinOpportunityPanelProps {
  companyId: string;
  opportunity: OpportunityDto;
  onClose: () => void;
}

function WinOpportunityPanel({ companyId, opportunity, onClose }: WinOpportunityPanelProps) {
  const queryClient = useQueryClient();

  const { control, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<WinFormValues>({
    resolver: zodResolver(winSchema),
    defaultValues: { customerCode: '' },
  });

  const winOpportunity = useMutation({
    mutationFn: (values: WinFormValues) =>
      apiClient.post<OpportunityDto>(`/crm/opportunities/${opportunity.id}/win`, {
        companyId,
        customerCode: values.customerCode,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['opportunities', companyId] });
      onClose();
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof WinFormValues, { message: messages[0] });
        }
      }
    },
  });

  return (
    <Card className="mt-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-base font-semibold text-text">Win opportunity — {opportunity.name}</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <p className="mb-3 text-xs text-text-muted">
        Winning creates a real Sales Customer with the code below (see OpportunityWon's consumer in Sales).
      </p>
      <form onSubmit={handleSubmit((values) => winOpportunity.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <label className="flex flex-col gap-1 text-sm">
          Customer code
          <Controller
            control={control}
            name="customerCode"
            render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="CUST-100" {...field} />
            )}
          />
          {errors.customerCode && <span className="text-xs text-danger">{errors.customerCode.message}</span>}
        </label>
        <div className="col-span-2 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Confirm win'}</Button>
          {winOpportunity.isError && (
            <span role="alert" className="text-sm text-danger">Could not win that opportunity.</span>
          )}
        </div>
      </form>
    </Card>
  );
}
