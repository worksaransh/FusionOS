import { useState } from 'react';
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { PageHeader } from '../../../shared/ui/PageHeader';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import { useDebouncedValue } from '../../../shared/hooks/useDebouncedValue';
import type { PagedResult } from '../../../shared/api/types';

const SEARCH_DEBOUNCE_MS = 250;

// rolloutPercentage stays a string in the form schema (same convention as
// SettingsPage's defaultPageSize) — react-hook-form's <input type="number"> still
// yields a string value, and z.coerce.number() has a mismatched input/output type
// that zodResolver can't reconcile with useForm's single type parameter. Converted
// to a real number with Number(...) only at the mutation call site.
const createSchema = z.object({
  key: z.string().min(1, 'Key is required').max(100),
  name: z.string().min(1, 'Name is required').max(200),
  description: z.string().max(1000).optional(),
  rolloutPercentage: z.string().refine((v) => Number.isInteger(Number(v)) && Number(v) >= 0 && Number(v) <= 100, 'Must be a whole number between 0 and 100'),
});
type CreateFormValues = z.infer<typeof createSchema>;

// Update deliberately excludes Key — it's the immutable business key (see
// UpdateFeatureFlagCommand.cs / FeatureFlagsController.Update), same convention as
// Cost Center's edit form excluding Code.
const editSchema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  description: z.string().max(1000).optional(),
  rolloutPercentage: z.string().refine((v) => Number.isInteger(Number(v)) && Number(v) >= 0 && Number(v) <= 100, 'Must be a whole number between 0 and 100'),
});
type EditFormValues = z.infer<typeof editSchema>;

interface FeatureFlagDto {
  id: string;
  key: string;
  name: string;
  description: string | null;
  isEnabled: boolean;
  rolloutPercentage: number;
  createdAt: string;
}

/**
 * Net-new per-company feature flags — FusionOS had no feature-flag system anywhere
 * (no FeatureManagement library, no flag table) before this. A flag is IsEnabled
 * (on/off) plus an optional RolloutPercentage gradual-rollout knob (0-100, default
 * 100) — a deterministic hash of Key+caller-id decides who falls inside that
 * percentage (see FeatureFlag.Evaluate on the backend). Toggle is a dedicated
 * POST .../{id}/toggle action, kept separate from the general Update PUT, same
 * convention as Cost Centers' Deactivate and Marketplace's Enable/Disable.
 */
export function FeatureFlagsPage() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [editingFlagId, setEditingFlagId] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebouncedValue(search, SEARCH_DEBOUNCE_MS);

  const flagsQuery = useQuery({
    queryKey: ['feature-flags', companyId, debouncedSearch],
    queryFn: () => {
      const params = new URLSearchParams({ companyId, page: '1', pageSize: '50' });
      if (debouncedSearch.trim()) params.set('search', debouncedSearch.trim());
      return apiClient.get<PagedResult<FeatureFlagDto>>(`/core/feature-flags?${params.toString()}`);
    },
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<CreateFormValues>({
    resolver: zodResolver(createSchema),
    defaultValues: { key: '', name: '', description: '', rolloutPercentage: '100' },
  });

  const createFlag = useMutation({
    mutationFn: (values: CreateFormValues) =>
      apiClient.post<FeatureFlagDto>('/core/feature-flags', {
        companyId,
        key: values.key,
        name: values.name,
        description: values.description || null,
        rolloutPercentage: Number(values.rolloutPercentage),
      }),
    onSuccess: () => {
      reset({ key: '', name: '', description: '', rolloutPercentage: '100' });
      queryClient.invalidateQueries({ queryKey: ['feature-flags', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof CreateFormValues, { message: messages[0] });
        }
      }
    },
  });

  const toggleFlag = useMutation({
    mutationFn: (flagId: string) => apiClient.post<FeatureFlagDto>(`/core/feature-flags/${flagId}/toggle`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['feature-flags', companyId] }),
  });

  if (!companyId) {
    return <p className="text-text-muted">Set an active Company ID in the header above to manage feature flags.</p>;
  }

  const editingFlag = flagsQuery.data?.data.find((f) => f.id === editingFlagId) ?? null;

  return (
    <div>
      <PageHeader
        title="Feature Flags"
        description="Per-company on/off switches with an optional gradual-rollout percentage — no external flag service, this is FusionOS's own."
      />

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createFlag.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Key
            <Controller
              control={control}
              name="key"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="new-dashboard-widget" {...field} />
              )}
            />
            {errors.key && <span className="text-xs text-danger">{errors.key.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Name
            <Controller
              control={control}
              name="name"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="New Dashboard Widget" {...field} />
              )}
            />
            {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
          </label>
          <label className="col-span-2 flex flex-col gap-1 text-sm">
            Description (optional)
            <Controller
              control={control}
              name="description"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.description && <span className="text-xs text-danger">{errors.description.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Rollout percentage
            <Controller
              control={control}
              name="rolloutPercentage"
              render={({ field }) => (
                <input type="number" min={0} max={100} className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.rolloutPercentage && <span className="text-xs text-danger">{errors.rolloutPercentage.message}</span>}
          </label>
          <div className="col-span-2">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create feature flag'}</Button>
          </div>
        </form>
        {createFlag.isError && createFlag.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createFlag.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <label className="mb-3 flex flex-col gap-1 text-sm sm:w-72">
          Search
          <input
            className="rounded-md border border-border bg-surface px-2 py-1.5"
            placeholder="Search by key or name…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </label>
        <DataTable
          columns={[
            { header: 'Key', render: (row: FeatureFlagDto) => <span className="font-mono text-xs">{row.key}</span> },
            { header: 'Name', render: (row: FeatureFlagDto) => row.name },
            { header: 'Rollout', render: (row: FeatureFlagDto) => `${row.rolloutPercentage}%` },
            { header: 'Status', render: (row: FeatureFlagDto) => (row.isEnabled ? 'Enabled' : 'Disabled') },
            { header: 'Created', render: (row: FeatureFlagDto) => new Date(row.createdAt).toLocaleDateString() },
            {
              header: 'Actions',
              render: (row: FeatureFlagDto) => (
                <div className="flex items-center gap-2">
                  <Button type="button" variant="secondary" onClick={() => setEditingFlagId(row.id)}>
                    Edit
                  </Button>
                  <Button
                    type="button"
                    variant={row.isEnabled ? 'danger' : 'primary'}
                    disabled={toggleFlag.isPending}
                    onClick={() => toggleFlag.mutate(row.id)}
                  >
                    {row.isEnabled ? 'Disable' : 'Enable'}
                  </Button>
                </div>
              ),
            },
          ]}
          rows={flagsQuery.data?.data}
          isLoading={flagsQuery.isLoading}
          isError={flagsQuery.isError}
          errorMessage="Could not load feature flags."
          emptyMessage="No feature flags yet — create the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {toggleFlag.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not toggle that feature flag.</p>
      )}

      {editingFlag && (
        <FeatureFlagEditPanel
          companyId={companyId}
          flag={editingFlag}
          onClose={() => setEditingFlagId(null)}
        />
      )}
    </div>
  );
}

interface FeatureFlagEditPanelProps {
  companyId: string;
  flag: FeatureFlagDto;
  onClose: () => void;
}

function FeatureFlagEditPanel({ companyId, flag, onClose }: FeatureFlagEditPanelProps) {
  const queryClient = useQueryClient();

  const { control, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    values: { name: flag.name, description: flag.description ?? '', rolloutPercentage: String(flag.rolloutPercentage) },
  });

  const updateFlag = useMutation({
    mutationFn: (values: EditFormValues) =>
      apiClient.put<FeatureFlagDto>(`/core/feature-flags/${flag.id}`, {
        companyId,
        name: values.name,
        description: values.description || null,
        rolloutPercentage: Number(values.rolloutPercentage),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['feature-flags', companyId] });
      onClose();
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof EditFormValues, { message: messages[0] });
        }
      }
    },
  });

  return (
    <Card className="mt-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-base font-semibold text-text">Edit feature flag — {flag.key}</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form onSubmit={handleSubmit((values) => updateFlag.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
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
          Rollout percentage
          <Controller
            control={control}
            name="rolloutPercentage"
            render={({ field }) => (
              <input type="number" min={0} max={100} className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )}
          />
          {errors.rolloutPercentage && <span className="text-xs text-danger">{errors.rolloutPercentage.message}</span>}
        </label>
        <label className="col-span-2 flex flex-col gap-1 text-sm">
          Description
          <Controller
            control={control}
            name="description"
            render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )}
          />
          {errors.description && <span className="text-xs text-danger">{errors.description.message}</span>}
        </label>
        <div className="col-span-2 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save changes'}</Button>
          {updateFlag.isError && (
            <span role="alert" className="text-sm text-danger">Could not save that feature flag.</span>
          )}
        </div>
      </form>
    </Card>
  );
}
