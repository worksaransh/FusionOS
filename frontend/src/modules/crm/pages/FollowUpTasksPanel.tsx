import { useState } from 'react';
import { Controller, useForm, useWatch } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { EntityCombobox } from '../../../shared/ui/EntityCombobox';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import { useCrmAccountOptions, useContactOptions, useLeadOptions, useOpportunityOptions, useUserOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const TARGET_TYPES = ['Lead', 'Opportunity', 'Account', 'Contact'] as const;
type TargetType = (typeof TARGET_TYPES)[number];

const schema = z.object({
  title: z.string().min(1, 'Title is required').max(200),
  description: z.string().max(2000).or(z.literal('')),
  dueDate: z.string().min(1, 'Due date is required'),
  entityType: z.enum(TARGET_TYPES),
  entityId: z.string().uuid('Pick a record this task is about'),
  assignedToUserId: z.string().refine((v) => v === '' || /^[0-9a-fA-F-]{36}$/.test(v), 'Must be blank or a valid user'),
});
type FormValues = z.infer<typeof schema>;

interface FollowUpTaskDto {
  id: string;
  title: string;
  description: string | null;
  dueDate: string;
  entityType: string;
  entityId: string;
  assignedToUserId: string | null;
  isCompleted: boolean;
  completedAt: string | null;
  createdAt: string;
}

type CompletionFilter = 'all' | 'open' | 'completed';

/**
 * Follow-up tasks/reminders — CRM depth pass (duplicate-detection/lead-scoring/reminders
 * slice). A small open → Completed to-do against a Lead, Opportunity, Account, or
 * Contact — same opaque (entityType, entityId) polymorphic reference as ActivitiesPanel
 * (see FollowUpTask.cs). Unlike Activities, a task has a real (if tiny) lifecycle, so
 * this panel also offers a Complete action and overdue/open/completed filtering.
 * Rendered as a sibling panel under LeadsPage, same stacking pattern as
 * OpportunitiesPanel/CrmAccountsPanel/ContactsPanel/ActivitiesPanel.
 */
export function FollowUpTasksPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const leadOptions = useLeadOptions(companyId);
  const opportunityOptions = useOpportunityOptions(companyId);
  const accountOptions = useCrmAccountOptions(companyId);
  const contactOptions = useContactOptions(companyId);
  const userOptions = useUserOptions(companyId);

  const [completionFilter, setCompletionFilter] = useState<CompletionFilter>('open');
  const [overdueOnly, setOverdueOnly] = useState(false);

  const tasksQuery = useQuery({
    queryKey: ['follow-up-tasks', companyId, completionFilter, overdueOnly],
    queryFn: () => {
      const params = new URLSearchParams({ companyId, page: '1', pageSize: '50' });
      if (completionFilter !== 'all') params.set('isCompleted', String(completionFilter === 'completed'));
      if (overdueOnly) params.set('overdueOnly', 'true');
      return apiClient.get<PagedResult<FollowUpTaskDto>>(`/crm/tasks?${params.toString()}`);
    },
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setValue, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { title: '', description: '', dueDate: '', entityType: 'Lead', entityId: '', assignedToUserId: '' },
  });

  const entityType = useWatch({ control, name: 'entityType' }) as TargetType;
  const targetOptions = pickTargetOptions(entityType, { leadOptions, opportunityOptions, accountOptions, contactOptions });

  const createTask = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<FollowUpTaskDto>('/crm/tasks', {
        companyId,
        title: values.title,
        description: values.description || null,
        dueDate: new Date(values.dueDate).toISOString(),
        entityType: values.entityType,
        entityId: values.entityId,
        assignedToUserId: values.assignedToUserId || null,
      }),
    onSuccess: () => {
      reset({ title: '', description: '', dueDate: '', entityType: 'Lead', entityId: '', assignedToUserId: '' });
      queryClient.invalidateQueries({ queryKey: ['follow-up-tasks', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const completeTask = useMutation({
    mutationFn: (id: string) => apiClient.post<FollowUpTaskDto>(`/crm/tasks/${id}/complete`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['follow-up-tasks', companyId] }),
  });

  if (!companyId) return null;

  const now = Date.now();

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Follow-up tasks</h2>
      <p className="mb-3 text-xs text-text-muted">
        Reminders/to-dos against a Lead, Opportunity, Account, or Contact — create one, assign it to a user, and complete it once it's done.
      </p>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createTask.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Title
            <Controller
              control={control}
              name="title"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.title && <span className="text-xs text-danger">{errors.title.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Due date
            <Controller
              control={control}
              name="dueDate"
              render={({ field }) => (
                <input type="datetime-local" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.dueDate && <span className="text-xs text-danger">{errors.dueDate.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Related to
            <Controller
              control={control}
              name="entityType"
              render={({ field }) => (
                <select
                  className="rounded-md border border-border bg-surface px-2 py-1.5"
                  {...field}
                  onChange={(e) => {
                    field.onChange(e);
                    setValue('entityId', '');
                  }}
                >
                  {TARGET_TYPES.map((t) => (
                    <option key={t} value={t}>{t}</option>
                  ))}
                </select>
              )}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Record
            <Controller
              control={control}
              name="entityId"
              render={({ field }) => (
                <EntityCombobox
                  key={entityType}
                  value={field.value}
                  onChange={field.onChange}
                  options={targetOptions.options}
                  isLoading={targetOptions.isLoading}
                  onSearchChange={targetOptions.onSearchChange}
                  placeholder={`Search ${entityType.toLowerCase()}s…`}
                />
              )}
            />
            {errors.entityId && <span className="text-xs text-danger">{errors.entityId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Assigned to (optional)
            <Controller
              control={control}
              name="assignedToUserId"
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
            {errors.assignedToUserId && <span className="text-xs text-danger">{errors.assignedToUserId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            Description (optional)
            <Controller
              control={control}
              name="description"
              render={({ field }) => (
                <textarea className="min-h-16 rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.description && <span className="text-xs text-danger">{errors.description.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create task'}</Button>
          </div>
        </form>
        {createTask.isError && createTask.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createTask.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <div className="mb-3 flex flex-wrap items-end gap-4">
          <label className="flex flex-col gap-1 text-sm">
            Status
            <select
              className="rounded-md border border-border bg-surface px-2 py-1.5"
              value={completionFilter}
              onChange={(e) => setCompletionFilter(e.target.value as CompletionFilter)}
            >
              <option value="all">All</option>
              <option value="open">Open</option>
              <option value="completed">Completed</option>
            </select>
          </label>
          <label className="flex items-center gap-2 text-sm">
            <input type="checkbox" checked={overdueOnly} onChange={(e) => setOverdueOnly(e.target.checked)} />
            Overdue only
          </label>
        </div>
        <DataTable
          columns={[
            { header: 'Title', render: (row: FollowUpTaskDto) => row.title },
            { header: 'Related to', render: (row: FollowUpTaskDto) => `${row.entityType} · ${row.entityId.slice(0, 8)}…` },
            {
              header: 'Due',
              render: (row: FollowUpTaskDto) => {
                const overdue = !row.isCompleted && new Date(row.dueDate).getTime() < now;
                return (
                  <span className={overdue ? 'text-danger' : undefined}>
                    {new Date(row.dueDate).toLocaleString()}{overdue ? ' (overdue)' : ''}
                  </span>
                );
              },
            },
            { header: 'Status', render: (row: FollowUpTaskDto) => (row.isCompleted ? 'Completed' : 'Open') },
            {
              header: 'Actions',
              render: (row: FollowUpTaskDto) =>
                row.isCompleted ? (
                  <span className="text-text-muted">—</span>
                ) : (
                  <Button type="button" variant="secondary" disabled={completeTask.isPending} onClick={() => completeTask.mutate(row.id)}>
                    Complete
                  </Button>
                ),
            },
          ]}
          rows={tasksQuery.data?.data}
          isLoading={tasksQuery.isLoading}
          isError={tasksQuery.isError}
          errorMessage="Could not load follow-up tasks."
          emptyMessage="No follow-up tasks yet — create the first one above."
          rowKey={(row) => row.id}
        />
        {completeTask.isError && (
          <p role="alert" className="mt-2 text-sm text-danger">Could not complete that task.</p>
        )}
      </Card>
    </div>
  );
}

interface TargetOptionsBag {
  leadOptions: ReturnType<typeof useLeadOptions>;
  opportunityOptions: ReturnType<typeof useOpportunityOptions>;
  accountOptions: ReturnType<typeof useCrmAccountOptions>;
  contactOptions: ReturnType<typeof useContactOptions>;
}

interface TargetOptions {
  options: { id: string; label: string }[];
  isLoading: boolean;
  onSearchChange?: (search: string) => void;
}

/** Same picker-switching helper as ActivitiesPanel.pickTargetOptions — kept as its own copy here since the two panels don't share a parent component to hoist it into. */
function pickTargetOptions(entityType: TargetType, bag: TargetOptionsBag): TargetOptions {
  switch (entityType) {
    case 'Lead':
      return bag.leadOptions;
    case 'Opportunity':
      return { options: bag.opportunityOptions.options, isLoading: bag.opportunityOptions.isLoading };
    case 'Account':
      return bag.accountOptions;
    case 'Contact':
      return bag.contactOptions;
    default:
      return bag.leadOptions;
  }
}
