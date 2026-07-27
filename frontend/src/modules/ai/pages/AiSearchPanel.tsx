import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';

interface AiSearchResultDto {
  recognizedIntent: string;
  explanation: string;
  results: unknown;
}

/** Normalizes whatever shape `results` came back as (a PagedResult, a plain
 * array, or null/undefined for the fallback case) into a flat row array —
 * the underlying intent's query shape varies, so this renders permissively
 * rather than assuming one fixed table per intent. */
function extractRows(results: unknown): Record<string, unknown>[] {
  if (!results) return [];
  if (Array.isArray(results)) return results as Record<string, unknown>[];
  if (typeof results === 'object' && results !== null && 'data' in results) {
    const data = (results as { data: unknown }).data;
    if (Array.isArray(data)) return data as Record<string, unknown>[];
  }
  return [results as Record<string, unknown>];
}

function formatCell(value: unknown): string {
  if (value === null || value === undefined) return '—';
  if (typeof value === 'object') return JSON.stringify(value);
  return String(value);
}

/**
 * AI Search — 12_AI_PLATFORM.md §4 "Natural-language search": a translation
 * layer over existing RBAC-scoped read queries, never a bypass of them.
 * Renders permissively (a generic key/value table) because the resolved
 * intent's result shape genuinely varies (invoices vs. leads vs. stock
 * levels vs. maintenance requests) — see AiSearchIntentResolver on the
 * backend for the exact, honestly-documented keyword list this endpoint
 * matches against (not a real NLP model).
 */
export function AiSearchPanel() {
  const { companyId } = useActiveCompany();
  const [queryText, setQueryText] = useState('');

  const search = useMutation({
    mutationFn: () => apiClient.post<AiSearchResultDto>('/ai/search', { companyId, queryText }),
  });

  if (!companyId) {
    return null;
  }

  const rows = search.data ? extractRows(search.data.results) : [];
  const columns = rows.length > 0 ? Object.keys(rows[0]) : [];

  return (
    <Card className="mt-6">
      <h2 className="mb-1 text-lg font-semibold text-text">AI Search</h2>
      <p className="mb-4 text-sm text-text-muted">
        Search across FusionOS data in plain English — a translation layer over the same
        RBAC-scoped read APIs the rest of the app uses, not a shortcut around them. Try things
        like &ldquo;overdue invoices&rdquo;, &ldquo;low stock products&rdquo;, or &ldquo;open leads&rdquo;.
      </p>

      <form noValidate
        onSubmit={(e) => {
          e.preventDefault();
          if (queryText.trim()) search.mutate();
        }}
        className="mb-4 flex flex-col gap-2 sm:flex-row"
      >
        <input
          className="flex-1 rounded-md border border-border bg-surface px-2 py-1.5 text-sm"
          placeholder="e.g. overdue invoices"
          value={queryText}
          onChange={(e) => setQueryText(e.target.value)}
        />
        <Button type="submit" disabled={search.isPending || !queryText.trim()}>
          {search.isPending ? 'Searching…' : 'Search'}
        </Button>
      </form>

      {search.isError && search.error instanceof ApiError && (
        <p role="alert" className="mb-3 text-sm text-danger">{search.error.problem.title}</p>
      )}

      {search.data && (
        <div>
          <p className="mb-3 text-sm text-text">
            <span className="font-medium">{search.data.recognizedIntent}</span> — {search.data.explanation}
          </p>

          {rows.length > 0 ? (
            <div className="overflow-x-auto">
              <table className="w-full min-w-[32rem] text-left text-sm">
                <thead>
                  <tr className="border-b border-border text-text-muted">
                    {columns.map((col) => (
                      <th key={col} scope="col" className="py-2 pr-4 whitespace-nowrap">{col}</th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {rows.map((row, index) => (
                    // eslint-disable-next-line react/no-array-index-key -- rows have no stable id across arbitrary result shapes
                    <tr key={index} className="border-b border-border last:border-0">
                      {columns.map((col) => (
                        <td key={col} className="py-2 pr-4">{formatCell(row[col])}</td>
                      ))}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <p className="text-sm text-text-muted">No matching records.</p>
          )}
        </div>
      )}
    </Card>
  );
}
