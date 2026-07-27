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
  inspectionId: z.string().uuid('Must be a valid Inspection id'),
  issuedByUserId: z.string().uuid('Must be a valid user id'),
});
type FormValues = z.infer<typeof schema>;

interface InspectionItemDto {
  id: string;
  characteristic: string;
  passed: boolean | null;
  notes: string | null;
  measuredValue: number | null;
  lowerControlLimit: number | null;
  upperControlLimit: number | null;
}

interface InspectionDto {
  id: string;
  type: string;
  referenceId: string;
  status: string;
  items: InspectionItemDto[];
}

interface QualityCertificateSummaryDto {
  id: string;
  inspectionId: string;
  certificateNumber: string;
  issuedAt: string;
  issuedByUserId: string;
}

interface QualityCertificateDto extends QualityCertificateSummaryDto {
  inspection: InspectionDto;
}

/**
 * Certificates of Analysis/Conformance (COA/COC) — generated once for a Passed Inspection
 * (idempotent per inspection; the backend rejects a second one for the same InspectionId).
 * No PDF is generated: the "Certificate" view below renders the returned structured data as a
 * simple printable-looking card — see QualityCertificatesController's doc comment for that
 * scope line. Rendered as a sibling panel under InspectionsPage.
 */
export function CertificatesPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [viewingCertificate, setViewingCertificate] = useState<QualityCertificateDto | null>(null);

  const certificatesQuery = useQuery({
    queryKey: ['quality-certificates', companyId],
    queryFn: () => apiClient.get<PagedResult<QualityCertificateSummaryDto>>(`/quality/certificates?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { inspectionId: '', issuedByUserId: '' },
  });

  const generateCertificate = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<QualityCertificateDto>('/quality/certificates', {
        companyId,
        inspectionId: values.inspectionId,
        issuedByUserId: values.issuedByUserId,
      }),
    onSuccess: (result) => {
      reset({ inspectionId: '', issuedByUserId: '' });
      queryClient.invalidateQueries({ queryKey: ['quality-certificates', companyId] });
      setViewingCertificate(result);
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const viewCertificate = useMutation({
    mutationFn: (inspectionId: string) =>
      apiClient.get<QualityCertificateDto>(`/quality/certificates/by-inspection/${inspectionId}?companyId=${companyId}`),
    onSuccess: (result) => setViewingCertificate(result),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Certificates of Analysis/Conformance</h2>
      <p className="mb-3 text-sm text-text-muted">
        Generate a certificate for a Passed inspection — one certificate per inspection.
        The generated data can be viewed/printed below; no PDF is produced.
      </p>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => generateCertificate.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Inspection id (must be Passed)
            <Controller
              control={control}
              name="inspectionId"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="00000000-0000-0000-0000-000000000000" {...field} />
              )}
            />
            {errors.inspectionId && <span className="text-xs text-danger">{errors.inspectionId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Issued by user id
            <Controller
              control={control}
              name="issuedByUserId"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="00000000-0000-0000-0000-000000000000" {...field} />
              )}
            />
            {errors.issuedByUserId && <span className="text-xs text-danger">{errors.issuedByUserId.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Generating…' : 'Generate certificate'}</Button>
          </div>
        </form>
        {generateCertificate.isError && generateCertificate.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{generateCertificate.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Certificate #', render: (row: QualityCertificateSummaryDto) => row.certificateNumber },
            { header: 'Inspection', render: (row: QualityCertificateSummaryDto) => row.inspectionId },
            { header: 'Issued at', render: (row: QualityCertificateSummaryDto) => new Date(row.issuedAt).toLocaleString() },
            {
              header: 'Actions',
              render: (row: QualityCertificateSummaryDto) => (
                <Button type="button" variant="secondary" disabled={viewCertificate.isPending} onClick={() => viewCertificate.mutate(row.inspectionId)}>
                  View
                </Button>
              ),
            },
          ]}
          rows={certificatesQuery.data?.data}
          isLoading={certificatesQuery.isLoading}
          isError={certificatesQuery.isError}
          errorMessage="Could not load certificates."
          emptyMessage="No certificates issued yet — generate one from a Passed inspection above."
          rowKey={(row) => row.id}
        />
      </Card>
      {viewCertificate.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not load that certificate.</p>
      )}

      {viewingCertificate && (
        <CertificateView certificate={viewingCertificate} onClose={() => setViewingCertificate(null)} />
      )}
    </div>
  );
}

function CertificateView({ certificate, onClose }: { certificate: QualityCertificateDto; onClose: () => void }) {
  return (
    <Card className="mt-4 border-2 border-border print:border-black">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-base font-semibold text-text">Certificate of Analysis/Conformance</h3>
        <div className="flex gap-2">
          <Button type="button" variant="secondary" onClick={() => window.print()}>Print</Button>
          <Button type="button" variant="secondary" onClick={onClose}>Close</Button>
        </div>
      </div>

      <dl className="mb-4 grid grid-cols-1 gap-x-6 gap-y-2 text-sm sm:grid-cols-2">
        <div>
          <dt className="text-text-muted">Certificate number</dt>
          <dd className="font-medium text-text">{certificate.certificateNumber}</dd>
        </div>
        <div>
          <dt className="text-text-muted">Issued at</dt>
          <dd className="font-medium text-text">{new Date(certificate.issuedAt).toLocaleString()}</dd>
        </div>
        <div>
          <dt className="text-text-muted">Inspection</dt>
          <dd className="font-medium text-text">{certificate.inspection.type} — {certificate.inspection.referenceId}</dd>
        </div>
        <div>
          <dt className="text-text-muted">Result</dt>
          <dd className="font-medium text-text">{certificate.inspection.status}</dd>
        </div>
      </dl>

      <table className="w-full border-collapse text-sm">
        <thead>
          <tr className="border-b border-border text-left">
            <th className="py-1 pr-3">Characteristic</th>
            <th className="py-1 pr-3">Result</th>
            <th className="py-1 pr-3">Measured value</th>
            <th className="py-1 pr-3">Control limits</th>
            <th className="py-1">Notes</th>
          </tr>
        </thead>
        <tbody>
          {certificate.inspection.items.map((item) => (
            <tr key={item.id} className="border-b border-border/50">
              <td className="py-1 pr-3">{item.characteristic}</td>
              <td className="py-1 pr-3">{item.passed === null ? '—' : item.passed ? 'Pass' : 'Fail'}</td>
              <td className="py-1 pr-3">{item.measuredValue ?? '—'}</td>
              <td className="py-1 pr-3">
                {item.lowerControlLimit ?? '—'} – {item.upperControlLimit ?? '—'}
              </td>
              <td className="py-1">{item.notes ?? ''}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </Card>
  );
}
