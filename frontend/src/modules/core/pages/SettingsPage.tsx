import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { PageHeader } from '../../../shared/ui/PageHeader';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';

const schema = z.object({
  defaultCurrency: z.string().length(3, 'Use a 3-letter ISO 4217 code').toUpperCase(),
  defaultPageSize: z.string().refine((v) => Number(v) >= 1 && Number(v) <= 200, 'Must be between 1 and 200'),
  displayName: z.string().optional(),
  logoUrl: z.string().optional(),
});
type FormValues = z.infer<typeof schema>;

interface CompanySettingsDto {
  companyId: string;
  defaultCurrency: string;
  defaultPageSize: number;
  displayName: string | null;
  logoUrl: string | null;
}

/**
 * Per-company Settings — Phase M5 (2026-07-15), previously 0% per
 * docs/PROJECT_TRACKER.md (no entity, no CQRS, no UI). GET always returns a
 * row: GetCompanySettingsQueryHandler creates sensible defaults on first read
 * rather than requiring a separate bootstrap step, so this page never shows
 * an empty state — just today's defaults until someone changes them.
 */
export function SettingsPage() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const settingsQuery = useQuery({
    queryKey: ['company-settings', companyId],
    queryFn: () => apiClient.get<CompanySettingsDto>(`/core/settings?companyId=${companyId}`),
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    values: settingsQuery.data
      ? {
          defaultCurrency: settingsQuery.data.defaultCurrency,
          defaultPageSize: String(settingsQuery.data.defaultPageSize),
          displayName: settingsQuery.data.displayName ?? '',
          logoUrl: settingsQuery.data.logoUrl ?? '',
        }
      : undefined,
  });

  const updateSettings = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.put<CompanySettingsDto>('/core/settings', {
        companyId,
        defaultCurrency: values.defaultCurrency,
        defaultPageSize: Number(values.defaultPageSize),
        displayName: values.displayName || null,
        logoUrl: values.logoUrl || null,
      }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['company-settings', companyId] }),
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  if (!companyId) {
    return <p className="text-text-muted">Set an active Company ID in the header above to manage settings.</p>;
  }

  return (
    <div>
      <PageHeader title="Settings" description="Per-company defaults — display name, logo, default currency, and list page size." />

      <Card>
        {settingsQuery.isLoading && <p className="text-text-muted">Loading…</p>}
        {settingsQuery.isError && <p className="text-danger">Could not load settings.</p>}

        {settingsQuery.data && (
          <form onSubmit={handleSubmit((values) => updateSettings.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <label className="flex flex-col gap-1 text-sm">
              Display name (optional)
              <Controller
                control={control}
                name="displayName"
                render={({ field }) => (
                  <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Shown in place of the legal name where space is tight" {...field} />
                )}
              />
              {errors.displayName && <span className="text-xs text-danger">{errors.displayName.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Logo URL (optional)
              <Controller
                control={control}
                name="logoUrl"
                render={({ field }) => (
                  <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="https://…" {...field} />
                )}
              />
              {errors.logoUrl && <span className="text-xs text-danger">{errors.logoUrl.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Default currency
              <Controller
                control={control}
                name="defaultCurrency"
                render={({ field }) => (
                  <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="USD" {...field} />
                )}
              />
              {errors.defaultCurrency && <span className="text-xs text-danger">{errors.defaultCurrency.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Default page size
              <Controller
                control={control}
                name="defaultPageSize"
                render={({ field }) => (
                  <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="25" {...field} />
                )}
              />
              {errors.defaultPageSize && <span className="text-xs text-danger">{errors.defaultPageSize.message}</span>}
            </label>
            <div className="col-span-2 flex items-center gap-3">
              <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save settings'}</Button>
              {updateSettings.isError && (
                <span role="alert" className="text-sm text-danger">Could not save settings.</span>
              )}
            </div>
          </form>
        )}
      </Card>
    </div>
  );
}
