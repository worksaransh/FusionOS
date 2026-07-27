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
  name: z.string().min(1, 'Name is required').max(100),
  color: z.string().max(20).or(z.literal('')),
});
type FormValues = z.infer<typeof schema>;

interface TagDto {
  id: string;
  name: string;
  color: string | null;
}

/**
 * Customer segmentation/tagging (Phase 7, 2026-07-23) — tag master data. Attaching a
 * tag to a specific Lead/Contact/Account/Opportunity is done via
 * POST /crm/tags/{id}/attach (backend-complete); this panel manages the tag
 * register itself. Per-entity attach/detach UI on each of those four panels is a
 * reasonable follow-up rather than duplicated four times here.
 */
export function TagsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const tagsQuery = useQuery({
    queryKey: ['tags', companyId],
    queryFn: () => apiClient.get<PagedResult<TagDto>>(`/crm/tags?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { name: '', color: '' },
  });

  const createTag = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<TagDto>('/crm/tags', { companyId, name: values.name, color: values.color || null }),
    onSuccess: () => {
      reset({ name: '', color: '' });
      queryClient.invalidateQueries({ queryKey: ['tags', companyId] });
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
      <h2 className="mb-3 text-lg font-semibold text-text">Tags</h2>
      <p className="mb-3 text-xs text-text-muted">
        Segmentation labels attachable to any Lead/Contact/Account/Opportunity via the tag API.
      </p>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createTag.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Name
            <Controller control={control} name="name" render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="VIP" {...field} />
            )} />
            {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Colour (optional)
            <Controller control={control} name="color" render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="#22c55e" {...field} />
            )} />
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create tag'}</Button>
          </div>
        </form>
        {createTag.isError && createTag.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createTag.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            {
              header: 'Tag',
              render: (row: TagDto) => (
                <span
                  className="rounded-full px-2 py-0.5 text-xs"
                  style={{ backgroundColor: row.color ?? undefined, border: row.color ? undefined : '1px solid var(--color-border)' }}
                >
                  {row.name}
                </span>
              ),
            },
          ]}
          rows={tagsQuery.data?.data}
          isLoading={tagsQuery.isLoading}
          isError={tagsQuery.isError}
          errorMessage="Could not load tags."
          emptyMessage="No tags yet."
          rowKey={(row) => row.id}
        />
      </Card>
    </div>
  );
}
