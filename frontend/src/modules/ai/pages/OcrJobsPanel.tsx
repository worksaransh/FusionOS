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
  documentId: z.string().uuid('Must be a valid Document id (from Core File Management)'),
});
type FormValues = z.infer<typeof schema>;

interface OcrJobDto {
  id: string;
  documentId: string;
  status: string;
  extractedFieldsJson: string | null;
  failureReason: string | null;
  draftReferenceType: string | null;
  draftReferenceId: string | null;
  createdAt: string;
}

/**
 * OCR job tracking — Phase 8 "AI & Integrations", `docs/blueprint/12_AI_PLATFORM.md` §4.
 * There is no real OCR engine, vision-model API, or Python service reachable in this
 * environment (see the backend's IOcrProvider/UnavailableOcrProvider doc comments for the
 * full reasoning), so every submitted job here will honestly show "Failed" with a message
 * explaining exactly what's missing — that is expected and correct, not a bug in this UI.
 * What's real: the job is durably recorded, queryable, and the pipeline shape (submit →
 * Processing → Completed/Failed → optionally linked to a draft record) is fully wired for
 * the day a real provider is registered in AiModule.cs.
 *
 * Document id is a plain text input, not a document picker — Core's File Management has no
 * reusable document-browser component in the frontend yet (nothing under
 * frontend/src/modules to grep for), and DocumentId is itself a deliberately opaque,
 * never-validated cross-module reference (same convention as Recommendations' ReferenceId
 * field above), so a raw id input is the honest choice here too.
 */
export function OcrJobsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const ocrJobsQuery = useQuery({
    queryKey: ['ocr-jobs', companyId],
    queryFn: () => apiClient.get<PagedResult<OcrJobDto>>(`/ai/ocr-jobs?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { documentId: '' },
  });

  const submitOcrJob = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<OcrJobDto>('/ai/ocr-jobs', {
        companyId,
        documentId: values.documentId,
      }),
    onSuccess: () => {
      reset({ documentId: '' });
      queryClient.invalidateQueries({ queryKey: ['ocr-jobs', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  if (!companyId) {
    return null;
  }

  return (
    <div className="mt-8">
      <h2 className="mb-1 text-lg font-semibold text-text">OCR / Invoice Processing</h2>
      <p className="mb-4 text-sm text-text-muted">
        Submit an already-uploaded Core document for OCR extraction — Phase 8, AI &amp; Integrations.
        No real OCR provider is configured in this environment, so jobs will honestly fail with a
        clear reason rather than fabricate extracted data.
      </p>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => submitOcrJob.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            Document id (from Core File Management)
            <Controller
              control={control}
              name="documentId"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="00000000-0000-0000-0000-000000000000" {...field} />
              )}
            />
            {errors.documentId && <span className="text-xs text-danger">{errors.documentId.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Submitting…' : 'Submit for OCR'}</Button>
          </div>
        </form>
        {submitOcrJob.isError && submitOcrJob.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{submitOcrJob.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Document id', render: (row: OcrJobDto) => row.documentId },
            { header: 'Status', render: (row: OcrJobDto) => row.status },
            { header: 'Failure reason', render: (row: OcrJobDto) => row.failureReason ?? '—' },
            { header: 'Draft reference', render: (row: OcrJobDto) => (row.draftReferenceType ? `${row.draftReferenceType} / ${row.draftReferenceId}` : '—') },
          ]}
          rows={ocrJobsQuery.data?.data}
          isLoading={ocrJobsQuery.isLoading}
          isError={ocrJobsQuery.isError}
          errorMessage="Could not load OCR jobs."
          emptyMessage="No OCR jobs yet — submit a document above."
          rowKey={(row) => row.id}
        />
      </Card>
    </div>
  );
}
