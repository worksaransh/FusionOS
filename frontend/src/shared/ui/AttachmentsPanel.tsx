import { useRef, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Download, Trash2 } from 'lucide-react';
import { apiClient, ApiError, type ProblemDetails } from '../api/client';
import { useAuthStore } from '../auth/authStore';
import { useActiveCompany } from '../company/useActiveCompany';
import { Button } from './Button';
import type { PagedResult } from '../api/types';

interface DocumentDto {
  id: string;
  entityType: string;
  entityId: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  uploadedByUserId: string;
  uploadedAt: string;
}

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '/api/v1';

function authHeaders(): HeadersInit {
  const session = useAuthStore.getState().session;
  return session ? { Authorization: `Bearer ${session.accessToken}` } : {};
}

function formatBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

interface AttachmentsPanelProps {
  entityType: string;
  entityId: string;
}

/**
 * Reusable file-attachment widget — a file picker + upload button, and a list
 * of already-attached files each with a download link and a delete button.
 * Any module's page drops this in with just the (entityType, entityId) pair
 * of whatever record the files belong to (an Invoice, a PurchaseOrder, an
 * Employee, anything) — same opaque polymorphic-reference convention the
 * backend's Document entity uses (no compile-time dependency on what kind of
 * record it is). Backed by the generic Core Documents subsystem: file bytes
 * are stored in Postgres (bytea) and capped at 10 MB/file — see
 * FusionOS.Modules.Core.Domain.Documents.Document's doc comment on the
 * backend for why, and the S3/Azure Blob follow-up once one is provisioned.
 *
 * Upload and download don't go through the shared apiClient (shared/api/client.ts)
 * because that helper always JSON-encodes the request body and JSON-decodes
 * the response — the wrong shape for a multipart file upload or a binary file
 * download. Both instead use fetch directly here, attaching the same Bearer
 * token apiClient itself attaches, but skip apiClient's automatic 401/refresh
 * retry (an acceptable simplification for a nested attachment widget — the
 * next list/delete apiClient call it makes still refreshes normally).
 */
export function AttachmentsPanel({ entityType, entityId }: AttachmentsPanelProps) {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [uploadError, setUploadError] = useState<string | null>(null);

  const queryKey = ['documents', companyId, entityType, entityId];

  const documentsQuery = useQuery({
    queryKey,
    queryFn: () =>
      apiClient.get<PagedResult<DocumentDto>>(
        `/core/documents?companyId=${companyId}&entityType=${encodeURIComponent(entityType)}&entityId=${entityId}&page=1&pageSize=50`,
      ),
    enabled: Boolean(companyId && entityId),
  });

  const upload = useMutation({
    mutationFn: async (file: File) => {
      const form = new FormData();
      form.append('companyId', companyId);
      form.append('entityType', entityType);
      form.append('entityId', entityId);
      form.append('file', file);

      const response = await fetch(`${BASE_URL}/core/documents`, {
        method: 'POST',
        headers: authHeaders(),
        body: form,
      });
      if (!response.ok) {
        const problem = (await response.json().catch(() => ({
          title: response.statusText,
          status: response.status,
        }))) as ProblemDetails;
        throw new ApiError(problem);
      }
      return response.json() as Promise<DocumentDto>;
    },
    onSuccess: () => {
      setUploadError(null);
      setSelectedFile(null);
      if (fileInputRef.current) fileInputRef.current.value = '';
      queryClient.invalidateQueries({ queryKey });
    },
    onError: (error) => setUploadError(error instanceof ApiError ? error.problem.title : 'Upload failed.'),
  });

  const removeDocument = useMutation({
    mutationFn: (id: string) => apiClient.delete(`/core/documents/${id}?companyId=${companyId}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey }),
  });

  async function handleDownload(doc: DocumentDto) {
    const response = await fetch(`${BASE_URL}/core/documents/${doc.id}/download?companyId=${companyId}`, {
      headers: authHeaders(),
    });
    if (!response.ok) return;

    const blob = await response.blob();
    const url = URL.createObjectURL(blob);
    const link = window.document.createElement('a');
    link.href = url;
    link.download = doc.fileName;
    link.click();
    URL.revokeObjectURL(url);
  }

  if (!companyId || !entityId) return null;

  return (
    <div className="flex flex-col gap-2 rounded-md border border-border p-3">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h3 className="text-sm font-semibold text-text">Attachments</h3>
        <div className="flex items-center gap-2">
          <input
            ref={fileInputRef}
            type="file"
            className="max-w-[10rem] text-xs text-text-muted"
            onChange={(e) => setSelectedFile(e.target.files?.[0] ?? null)}
          />
          <Button
            type="button"
            variant="secondary"
            disabled={!selectedFile || upload.isPending}
            onClick={() => selectedFile && upload.mutate(selectedFile)}
          >
            {upload.isPending ? 'Uploading…' : 'Upload'}
          </Button>
        </div>
      </div>

      {uploadError && <p role="alert" className="text-xs text-danger">{uploadError}</p>}
      {documentsQuery.isLoading && <p role="status" className="text-xs text-text-muted">Loading…</p>}
      {documentsQuery.isError && <p role="alert" className="text-xs text-danger">Could not load attachments.</p>}

      <ul className="flex flex-col gap-1">
        {documentsQuery.data?.data.map((doc) => (
          <li key={doc.id} className="flex items-center justify-between gap-2 border-b border-border py-1 text-sm last:border-0">
            <button
              type="button"
              className="flex items-center gap-1 truncate text-left text-primary hover:underline"
              onClick={() => handleDownload(doc)}
            >
              <Download size={14} className="shrink-0" />
              <span className="truncate">{doc.fileName}</span>
            </button>
            <span className="shrink-0 text-xs text-text-muted">{formatBytes(doc.fileSizeBytes)}</span>
            <Button
              type="button"
              variant="danger"
              disabled={removeDocument.isPending}
              onClick={() => removeDocument.mutate(doc.id)}
              aria-label={`Delete ${doc.fileName}`}
            >
              <Trash2 size={14} />
            </Button>
          </li>
        ))}
        {documentsQuery.data && documentsQuery.data.data.length === 0 && (
          <li className="text-xs text-text-muted">No files attached yet.</li>
        )}
      </ul>
    </div>
  );
}
