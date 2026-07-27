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

const schema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  description: z.string().max(2000).or(z.literal('')),
  stepsText: z.string().min(1, 'At least one step is required'),
});
type FormValues = z.infer<typeof schema>;

interface ChecklistItemDto {
  id: string;
  sequenceNumber: number;
  description: string;
}
interface ChecklistTemplateDto {
  id: string;
  name: string;
  description: string | null;
  isActive: boolean;
  items: ChecklistItemDto[];
}

/**
 * Maintenance Checklists/Procedures (Phase 7, 2026-07-23) — reusable named step
 * lists a MaintenanceRequest can optionally be created from (snapshotted onto
 * the request, never a live reference — see ChecklistTemplate.cs).
 */
export function ChecklistTemplatesPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const templatesQuery = useQuery({
    queryKey: ['checklist-templates', companyId],
    queryFn: () => apiClient.get<PagedResult<ChecklistTemplateDto>>(`/maintenance/checklist-templates?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { name: '', description: '', stepsText: '' },
  });

  const createTemplate = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<ChecklistTemplateDto>('/maintenance/checklist-templates', {
        companyId,
        name: values.name,
        description: values.description || null,
        stepDescriptions: values.stepsText.split('\n').map((s) => s.trim()).filter(Boolean),
      }),
    onSuccess: () => {
      reset({ name: '', description: '', stepsText: '' });
      queryClient.invalidateQueries({ queryKey: ['checklist-templates', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const deactivateTemplate = useMutation({
    mutationFn: (id: string) => apiClient.post(`/maintenance/checklist-templates/${id}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['checklist-templates', companyId] }),
  });

  const [expandedId, setExpandedId] = useState<string | null>(null);

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Checklist Templates</h2>
      <p className="mb-3 text-xs text-text-muted">
        Reusable maintenance procedures — pick one when creating a maintenance request to snapshot its steps onto that request.
      </p>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createTemplate.mutate(values))} className="grid grid-cols-1 gap-4">
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
            Description (optional)
            <Controller
              control={control}
              name="description"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Steps (one per line)
            <Controller
              control={control}
              name="stepsText"
              render={({ field }) => (
                <textarea className="min-h-24 rounded-md border border-border bg-surface px-2 py-1.5" placeholder={'Check oil level\nInspect belts\nTest emergency stop'} {...field} />
              )}
            />
            {errors.stepsText && <span className="text-xs text-danger">{errors.stepsText.message}</span>}
          </label>
          <div>
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create template'}</Button>
          </div>
        </form>
        {createTemplate.isError && createTemplate.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createTemplate.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Name', render: (row: ChecklistTemplateDto) => row.name },
            { header: 'Steps', render: (row: ChecklistTemplateDto) => `${row.items.length} step(s)` },
            { header: 'Status', render: (row: ChecklistTemplateDto) => (row.isActive ? 'Active' : 'Inactive') },
            {
              header: 'Actions',
              render: (row: ChecklistTemplateDto) => (
                <div className="flex items-center gap-2">
                  <Button type="button" variant="secondary" onClick={() => setExpandedId(expandedId === row.id ? null : row.id)}>
                    {expandedId === row.id ? 'Hide steps' : 'View steps'}
                  </Button>
                  <Button
                    type="button"
                    variant="danger"
                    disabled={!row.isActive || deactivateTemplate.isPending}
                    onClick={() => deactivateTemplate.mutate(row.id)}
                  >
                    {row.isActive ? 'Deactivate' : 'Deactivated'}
                  </Button>
                </div>
              ),
            },
          ]}
          rows={templatesQuery.data?.data}
          isLoading={templatesQuery.isLoading}
          isError={templatesQuery.isError}
          errorMessage="Could not load checklist templates."
          emptyMessage="No checklist templates yet."
          rowKey={(row) => row.id}
        />
        {expandedId && (
          <ol className="mt-3 list-decimal space-y-1 pl-5 text-sm text-text-muted">
            {templatesQuery.data?.data
              .find((t) => t.id === expandedId)
              ?.items.sort((a, b) => a.sequenceNumber - b.sequenceNumber)
              .map((item) => <li key={item.id}>{item.description}</li>)}
          </ol>
        )}
      </Card>
    </div>
  );
}
