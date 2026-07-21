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
import { useCrmAccountOptions, useContactOptions, useLeadOptions, useOpportunityOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const ACTIVITY_TYPES = ['Call', 'Email', 'Meeting', 'Note'] as const;
const TARGET_TYPES = ['Lead', 'Opportunity', 'Account', 'Contact'] as const;
type TargetType = (typeof TARGET_TYPES)[number];

const schema = z.object({
  entityType: z.enum(TARGET_TYPES),
  entityId: z.string().uuid('Pick a record to log this against'),
  type: z.enum(ACTIVITY_TYPES),
  notes: z.string().min(1, 'Notes are required').max(2000),
});
type FormValues = z.infer<typeof schema>;

interface ActivityDto {
  id: string;
  entityType: string;
  entityId: string;
  type: string;
  notes: string;
  createdAt: string;
  createdBy: string;
}

/**
 * Activities — CRM depth pass (2026-07-20). A logged interaction (call/email/meeting/
 * note) against a Lead, Opportunity, Account, or Contact — same opaque
 * (entityType, entityId) polymorphic reference as Core's ApprovalRequest (see
 * Activity.cs). A point-in-time log entry, not a lifecycle record: create + list
 * only, no edit/deactivate — same reasoning an audit-log row is never edited.
 * Rendered as a sibling panel under LeadsPage, same stacking pattern as
 * OpportunitiesPanel/CrmAccountsPanel/ContactsPanel.
 */
export function ActivitiesPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const leadOptions = useLeadOptions(companyId);
  const opportunityOptions = useOpportunityOptions(companyId);
  const accountOptions = useCrmAccountOptions(companyId);
  const contactOptions = useContactOptions(companyId);

  const activitiesQuery = useQuery({
    queryKey: ['activities', companyId],
    queryFn: () => apiClient.get<PagedResult<ActivityDto>>(`/crm/activities?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setValue, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { entityType: 'Lead', entityId: '', type: 'Call', notes: '' },
  });

  const entityType = useWatch({ control, name: 'entityType' }) as TargetType;
  const targetOptions = pickTargetOptions(entityType, { leadOptions, opportunityOptions, accountOptions, contactOptions });

  const createActivity = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<ActivityDto>('/crm/activities', {
        companyId,
        entityType: values.entityType,
        entityId: values.entityId,
        type: values.type,
        notes: values.notes,
      }),
    onSuccess: () => {
      reset({ entityType: 'Lead', entityId: '', type: 'Call', notes: '' });
      queryClient.invalidateQueries({ queryKey: ['activities', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Activities</h2>
      <p className="mb-3 text-xs text-text-muted">
        Log a call, email, meeting, or note against a Lead, Opportunity, Account, or Contact.
      </p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createActivity.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
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
            Type
            <Controller
              control={control}
              name="type"
              render={({ field }) => (
                <select className="rounded-md border border-border bg-surface px-2 py-1.5" {...field}>
                  {ACTIVITY_TYPES.map((t) => (
                    <option key={t} value={t}>{t}</option>
                  ))}
                </select>
              )}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            Notes
            <Controller
              control={control}
              name="notes"
              render={({ field }) => (
                <textarea className="min-h-24 rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.notes && <span className="text-xs text-danger">{errors.notes.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Logging…' : 'Log activity'}</Button>
          </div>
        </form>
        {createActivity.isError && createActivity.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createActivity.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Related to', render: (row: ActivityDto) => `${row.entityType} · ${row.entityId.slice(0, 8)}…` },
            { header: 'Type', render: (row: ActivityDto) => row.type },
            { header: 'Notes', render: (row: ActivityDto) => row.notes },
            { header: 'Logged', render: (row: ActivityDto) => new Date(row.createdAt).toLocaleString() },
          ]}
          rows={activitiesQuery.data?.data}
          isLoading={activitiesQuery.isLoading}
          isError={activitiesQuery.isError}
          errorMessage="Could not load activities."
          emptyMessage="No activities logged yet — log the first one above."
          rowKey={(row) => row.id}
        />
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

/**
 * Picks the right entity-option hook result for whichever target type the form's
 * "Related to" select currently holds — normalized to one shape since
 * useOpportunityOptions (no server-side search yet) doesn't return an
 * onSearchChange, unlike the other three.
 */
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
