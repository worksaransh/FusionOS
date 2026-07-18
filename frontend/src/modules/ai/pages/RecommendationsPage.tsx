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
  type: z.string().min(1, 'Type is required').max(100),
  referenceId: z.string().uuid('Must be a valid id (the record this recommendation is about)'),
  summary: z.string().min(1, 'Summary is required').max(2000),
  modelVersion: z.string().min(1, 'Model version is required').max(50),
});
type FormValues = z.infer<typeof schema>;

interface RecommendationDto {
  id: string;
  type: string;
  referenceId: string;
  summary: string;
  modelVersion: string;
  status: string;
  decidedAt: string | null;
}

/**
 * Recommendations — AI Platform's first real frontend slice (backend has
 * existed since this pass; this closes the "frontend panel deferred" gap
 * the same day the backend slice was built). The human-in-the-loop record
 * 12_AI_PLATFORM.md §3/§5 describes: every recommendation stays Pending
 * until a person explicitly Accepts or Dismisses it — this UI is that
 * confirmation surface, not a place recommendations get auto-applied.
 * ReferenceId is a plain id input, not an EntityCombobox — same reasoning as
 * Quality's Inspection.ReferenceId: a deliberately opaque, never-validated
 * cross-module reference. Recording a recommendation here is a manual
 * stand-in for the real model-fed producer described in 12_AI_PLATFORM.md
 * §3.1, which doesn't exist yet — see Recommendation's own class doc comment.
 */
export function RecommendationsPage() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const recommendationsQuery = useQuery({
    queryKey: ['recommendations', companyId],
    queryFn: () => apiClient.get<PagedResult<RecommendationDto>>(`/ai/recommendations?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { type: '', referenceId: '', summary: '', modelVersion: '' },
  });

  const recordRecommendation = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<RecommendationDto>('/ai/recommendations', {
        companyId,
        type: values.type,
        referenceId: values.referenceId,
        summary: values.summary,
        modelVersion: values.modelVersion,
      }),
    onSuccess: () => {
      reset({ type: '', referenceId: '', summary: '', modelVersion: '' });
      queryClient.invalidateQueries({ queryKey: ['recommendations', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const acceptRecommendation = useMutation({
    mutationFn: (id: string) => apiClient.post<RecommendationDto>(`/ai/recommendations/${id}/accept`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['recommendations', companyId] }),
  });

  const dismissRecommendation = useMutation({
    mutationFn: (id: string) => apiClient.post<RecommendationDto>(`/ai/recommendations/${id}/dismiss`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['recommendations', companyId] }),
  });

  if (!companyId) {
    return <p className="text-text-muted">Set an active Company ID in the header above to manage recommendations.</p>;
  }

  return (
    <div>
      <h1 className="mb-1 text-xl font-semibold text-text">Recommendations</h1>
      <p className="mb-4 text-sm text-text-muted">
        AI-generated insights awaiting a human decision — AI Platform, Phase 7.
      </p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => recordRecommendation.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Type
            <Controller
              control={control}
              name="type"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="ReorderSuggestion" {...field} />
              )}
            />
            {errors.type && <span className="text-xs text-danger">{errors.type.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Reference id
            <Controller
              control={control}
              name="referenceId"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="00000000-0000-0000-0000-000000000000" {...field} />
              )}
            />
            {errors.referenceId && <span className="text-xs text-danger">{errors.referenceId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            Summary
            <Controller
              control={control}
              name="summary"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Recommended reorder qty based on 90-day demand trend + current lead time" {...field} />
              )}
            />
            {errors.summary && <span className="text-xs text-danger">{errors.summary.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Model version
            <Controller
              control={control}
              name="modelVersion"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="v1.0.0" {...field} />
              )}
            />
            {errors.modelVersion && <span className="text-xs text-danger">{errors.modelVersion.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Recording…' : 'Record recommendation'}</Button>
          </div>
        </form>
        {recordRecommendation.isError && recordRecommendation.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{recordRecommendation.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Type', render: (row: RecommendationDto) => row.type },
            { header: 'Summary', render: (row: RecommendationDto) => row.summary },
            { header: 'Model', render: (row: RecommendationDto) => row.modelVersion },
            { header: 'Status', render: (row: RecommendationDto) => row.status },
            {
              header: 'Actions',
              render: (row: RecommendationDto) =>
                row.status === 'Pending' ? (
                  <div className="flex items-center gap-2">
                    <Button type="button" variant="secondary" disabled={acceptRecommendation.isPending} onClick={() => acceptRecommendation.mutate(row.id)}>
                      Accept
                    </Button>
                    <Button type="button" variant="danger" disabled={dismissRecommendation.isPending} onClick={() => dismissRecommendation.mutate(row.id)}>
                      Dismiss
                    </Button>
                  </div>
                ) : null,
            },
          ]}
          rows={recommendationsQuery.data?.data}
          isLoading={recommendationsQuery.isLoading}
          isError={recommendationsQuery.isError}
          errorMessage="Could not load recommendations."
          emptyMessage="No recommendations yet — record the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {(acceptRecommendation.isError || dismissRecommendation.isError) && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not update that recommendation.</p>
      )}
    </div>
  );
}
