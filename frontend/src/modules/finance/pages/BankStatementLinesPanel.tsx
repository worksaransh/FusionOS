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
import { useBankAccountOptions, useJournalEntryOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const pickerSchema = z.object({
  bankAccountId: z.string().uuid('Pick a bank account'),
});
type PickerFormValues = z.infer<typeof pickerSchema>;

const lineSchema = z.object({
  transactionDate: z.string().min(1, 'Date is required'),
  amount: z.string().refine((v) => Number(v) !== 0 && v.trim() !== '', 'Amount cannot be zero'),
  description: z.string().min(1, 'Description is required').max(500),
});
type LineFormValues = z.infer<typeof lineSchema>;

const reconcileSchema = z.object({
  matchedJournalEntryId: z.string().refine((v) => v === '' || /^[0-9a-fA-F-]{36}$/.test(v), 'Must be blank or a valid Journal Entry'),
});
type ReconcileFormValues = z.infer<typeof reconcileSchema>;

interface BankStatementLineDto {
  id: string;
  bankAccountId: string;
  transactionDate: string;
  amount: number;
  description: string;
  isReconciled: boolean;
  reconciledAt: string | null;
  matchedJournalEntryId: string | null;
}

interface ReconciliationSummaryDto {
  bankAccountId: string;
  totalLines: number;
  reconciledCount: number;
  unreconciledCount: number;
  unreconciledTotalAmount: number;
}

interface JournalEntryMatchCandidateDto {
  journalEntryId: string;
  entryDate: string;
  reference: string | null;
  amount: number;
}

/**
 * Bank Statement Lines — M8d, Finance depth: bank reconciliation. Pick a
 * bank account (via the new useBankAccountOptions hook), then record
 * manually-entered statement lines against it (no bank-feed/file-import —
 * see BankStatementLine.cs's class doc comment) and reconcile/unreconcile
 * them one at a time against an optional JournalEntry (no auto-matching
 * algorithm — same doc comment: the user always confirms the match
 * themselves). Each line now has a "Suggest matches" action wired to
 * SuggestMatchesForStatementLineQuery (GET .../{id}/match-suggestions) —
 * same-amount, within-date-window candidates the user can click to fill the
 * matched-journal-entry field below, falling back to the manual
 * EntityCombobox search when there are no suggestions or the user wants a
 * different entry. Mirrors PayablesPanel's "pick an entity, then manage its
 * records" shape, plus the reconciliation summary rollup from
 * GetReconciliationSummaryQuery.
 */
export function BankStatementLinesPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [suggestingLineId, setSuggestingLineId] = useState<string | null>(null);

  const bankAccountOptions = useBankAccountOptions(companyId);
  const journalEntryOptions = useJournalEntryOptions(companyId);

  const { control: pickerControl } = useForm<PickerFormValues>({
    resolver: zodResolver(pickerSchema),
    defaultValues: { bankAccountId: '' },
  });
  const bankAccountId = useWatch({ control: pickerControl, name: 'bankAccountId' });

  const summaryQuery = useQuery({
    queryKey: ['bank-reconciliation-summary', companyId, bankAccountId],
    queryFn: () => apiClient.get<ReconciliationSummaryDto>(`/finance/bank-accounts/${bankAccountId}/statement-lines/summary?companyId=${companyId}`),
    enabled: Boolean(companyId && bankAccountId),
  });

  const linesQuery = useQuery({
    queryKey: ['bank-statement-lines', companyId, bankAccountId],
    queryFn: () => apiClient.get<PagedResult<BankStatementLineDto>>(`/finance/bank-accounts/${bankAccountId}/statement-lines?companyId=${companyId}&page=1&pageSize=25`),
    enabled: Boolean(companyId && bankAccountId),
  });

  const {
    control: lineControl,
    handleSubmit: handleLineSubmit,
    reset: resetLine,
    formState: { errors: lineErrors, isSubmitting: isLineSubmitting },
  } = useForm<LineFormValues>({
    resolver: zodResolver(lineSchema),
    defaultValues: { transactionDate: '', amount: '', description: '' },
  });

  const recordLine = useMutation({
    mutationFn: (values: LineFormValues) =>
      apiClient.post<BankStatementLineDto>(`/finance/bank-accounts/${bankAccountId}/statement-lines`, {
        companyId,
        transactionDate: new Date(values.transactionDate).toISOString(),
        amount: Number(values.amount),
        description: values.description,
      }),
    onSuccess: () => {
      resetLine({ transactionDate: '', amount: '', description: '' });
      queryClient.invalidateQueries({ queryKey: ['bank-statement-lines', companyId, bankAccountId] });
      queryClient.invalidateQueries({ queryKey: ['bank-reconciliation-summary', companyId, bankAccountId] });
    },
  });

  const {
    control: reconcileControl,
    handleSubmit: handleReconcileSubmit,
    reset: resetReconcile,
    setValue: setReconcileValue,
    formState: { errors: reconcileErrors },
  } = useForm<ReconcileFormValues>({
    resolver: zodResolver(reconcileSchema),
    defaultValues: { matchedJournalEntryId: '' },
  });

  const matchSuggestionsQuery = useQuery({
    queryKey: ['bank-statement-line-match-suggestions', companyId, bankAccountId, suggestingLineId],
    queryFn: () =>
      apiClient.get<JournalEntryMatchCandidateDto[]>(
        `/finance/bank-accounts/${bankAccountId}/statement-lines/${suggestingLineId}/match-suggestions?companyId=${companyId}`,
      ),
    enabled: Boolean(companyId && bankAccountId && suggestingLineId),
  });

  const reconcileLine = useMutation({
    mutationFn: ({ lineId, matchedJournalEntryId }: { lineId: string; matchedJournalEntryId: string }) =>
      apiClient.post<BankStatementLineDto>(`/finance/bank-accounts/${bankAccountId}/statement-lines/${lineId}/reconcile`, {
        companyId,
        matchedJournalEntryId: matchedJournalEntryId || null,
      }),
    onSuccess: () => {
      resetReconcile({ matchedJournalEntryId: '' });
      queryClient.invalidateQueries({ queryKey: ['bank-statement-lines', companyId, bankAccountId] });
      queryClient.invalidateQueries({ queryKey: ['bank-reconciliation-summary', companyId, bankAccountId] });
    },
  });

  const unreconcileLine = useMutation({
    mutationFn: (lineId: string) =>
      apiClient.post<BankStatementLineDto>(`/finance/bank-accounts/${bankAccountId}/statement-lines/${lineId}/unreconcile`, { companyId }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['bank-statement-lines', companyId, bankAccountId] });
      queryClient.invalidateQueries({ queryKey: ['bank-reconciliation-summary', companyId, bankAccountId] });
    },
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Bank Reconciliation</h2>
      <p className="mb-3 text-xs text-text-muted">
        Pick a bank account, record its statement lines by hand (no bank-feed/file import in this slice), and mark
        each line reconciled or unreconciled — optionally against a matching journal entry you pick yourself (no
        auto-matching algorithm).
      </p>

      <Card className="mb-6">
        <label className="flex flex-col gap-1 text-sm sm:w-96">
          Bank account
          <Controller
            control={pickerControl}
            name="bankAccountId"
            render={({ field }) => (
              <EntityCombobox
                value={field.value}
                onChange={field.onChange}
                options={bankAccountOptions.options}
                isLoading={bankAccountOptions.isLoading}
                onSearchChange={bankAccountOptions.onSearchChange}
                placeholder="Search bank accounts…"
              />
            )}
          />
        </label>
      </Card>

      {bankAccountId && (
        <>
          {summaryQuery.data && (
            <Card className="mb-6">
              <h3 className="mb-2 text-sm font-semibold text-text">Reconciliation summary</h3>
              <div className="grid grid-cols-2 gap-3 text-sm sm:grid-cols-4">
                <p>Total lines: <span className="font-semibold text-text">{summaryQuery.data.totalLines}</span></p>
                <p>Reconciled: <span className="font-semibold text-text">{summaryQuery.data.reconciledCount}</span></p>
                <p>Unreconciled: <span className="font-semibold text-text">{summaryQuery.data.unreconciledCount}</span></p>
                <p>Unreconciled total: <span className="font-semibold text-text">{summaryQuery.data.unreconciledTotalAmount.toLocaleString()}</span></p>
              </div>
            </Card>
          )}

          <Card className="mb-6">
            <h3 className="mb-3 text-sm font-semibold text-text">Record a statement line</h3>
            <form onSubmit={handleLineSubmit((values) => recordLine.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
              <label className="flex flex-col gap-1 text-sm">
                Date
                <Controller
                  control={lineControl}
                  name="transactionDate"
                  render={({ field }) => (
                    <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
                  )}
                />
                {lineErrors.transactionDate && <span className="text-xs text-danger">{lineErrors.transactionDate.message}</span>}
              </label>
              <label className="flex flex-col gap-1 text-sm">
                Amount (+ deposit / − withdrawal)
                <Controller
                  control={lineControl}
                  name="amount"
                  render={({ field }) => (
                    <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="0.00" {...field} />
                  )}
                />
                {lineErrors.amount && <span className="text-xs text-danger">{lineErrors.amount.message}</span>}
              </label>
              <label className="flex flex-col gap-1 text-sm">
                Description
                <Controller
                  control={lineControl}
                  name="description"
                  render={({ field }) => (
                    <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
                  )}
                />
                {lineErrors.description && <span className="text-xs text-danger">{lineErrors.description.message}</span>}
              </label>
              <div className="col-span-3">
                <Button type="submit" disabled={isLineSubmitting}>{isLineSubmitting ? 'Recording…' : 'Record statement line'}</Button>
              </div>
            </form>
            {recordLine.isError && recordLine.error instanceof ApiError && (
              <p role="alert" className="mt-2 text-sm text-danger">{recordLine.error.problem.title}</p>
            )}
          </Card>

          <Card className="mb-6">
            <h3 className="mb-3 text-sm font-semibold text-text">Reconcile a line</h3>
            <p className="mb-3 text-xs text-text-muted">Pick a journal entry match (optional), then click Reconcile on the line below.</p>
            <label className="flex flex-col gap-1 text-sm sm:w-96">
              Matched journal entry (optional)
              <Controller
                control={reconcileControl}
                name="matchedJournalEntryId"
                render={({ field }) => (
                  <EntityCombobox
                    value={field.value}
                    onChange={field.onChange}
                    options={journalEntryOptions.options}
                    isLoading={journalEntryOptions.isLoading}
                    placeholder="Search journal entries…"
                  />
                )}
              />
              {reconcileErrors.matchedJournalEntryId && <span className="text-xs text-danger">{reconcileErrors.matchedJournalEntryId.message}</span>}
            </label>
          </Card>

          <Card>
            <DataTable
              columns={[
                { header: 'Date', render: (line: BankStatementLineDto) => new Date(line.transactionDate).toLocaleDateString() },
                { header: 'Description', render: (line: BankStatementLineDto) => line.description },
                { header: 'Amount', render: (line: BankStatementLineDto) => line.amount.toLocaleString() },
                { header: 'Status', render: (line: BankStatementLineDto) => (line.isReconciled ? 'Reconciled' : 'Unreconciled') },
                { header: 'Reconciled at', render: (line: BankStatementLineDto) => (line.reconciledAt ? new Date(line.reconciledAt).toLocaleString() : '—') },
                {
                  header: 'Actions',
                  render: (line: BankStatementLineDto) => (
                    <div className="flex items-center gap-2">
                      <Button
                        type="button"
                        variant="secondary"
                        disabled={line.isReconciled}
                        onClick={() => setSuggestingLineId(line.id)}
                      >
                        Suggest matches
                      </Button>
                      <Button
                        type="button"
                        variant="secondary"
                        disabled={line.isReconciled || reconcileLine.isPending}
                        onClick={handleReconcileSubmit((values) =>
                          reconcileLine.mutate({ lineId: line.id, matchedJournalEntryId: values.matchedJournalEntryId }),
                        )}
                      >
                        Reconcile
                      </Button>
                      <Button
                        type="button"
                        variant="danger"
                        disabled={!line.isReconciled || unreconcileLine.isPending}
                        onClick={() => unreconcileLine.mutate(line.id)}
                      >
                        Unreconcile
                      </Button>
                    </div>
                  ),
                },
              ]}
              rows={linesQuery.data?.data}
              isLoading={linesQuery.isLoading}
              isError={linesQuery.isError}
              errorMessage="Could not load statement lines."
              emptyMessage="No statement lines yet — record the first one above."
              rowKey={(line) => line.id}
            />
          </Card>
          {(reconcileLine.isError || unreconcileLine.isError) && (
            <p role="alert" className="mt-2 text-sm text-danger">Could not update that statement line's reconciliation state.</p>
          )}

          {(() => {
            const suggestingLine = linesQuery.data?.data.find((line) => line.id === suggestingLineId) ?? null;
            if (!suggestingLine) return null;
            return (
              <Card className="mt-4">
                <div className="mb-3 flex items-center justify-between">
                  <h3 className="text-base font-semibold text-text">
                    Suggested matches — {suggestingLine.description} ({suggestingLine.amount.toLocaleString()})
                  </h3>
                  <Button variant="secondary" onClick={() => setSuggestingLineId(null)}>Close</Button>
                </div>
                <p className="mb-3 text-xs text-text-muted">
                  Same-amount candidates within a few days of this line's date — pick one to fill the "Matched journal
                  entry" field above, then click Reconcile on the line. No auto-matching: you still confirm the match
                  yourself.
                </p>
                {matchSuggestionsQuery.isLoading && <p role="status" className="text-text-muted">Loading…</p>}
                {matchSuggestionsQuery.isError && (
                  <p role="alert" className="text-danger">Could not load match suggestions.</p>
                )}
                {matchSuggestionsQuery.data && matchSuggestionsQuery.data.length === 0 && (
                  <p className="text-sm text-text-muted">
                    No candidates found — use the manual "Matched journal entry" search above instead.
                  </p>
                )}
                {matchSuggestionsQuery.data && matchSuggestionsQuery.data.length > 0 && (
                  <ul className="flex flex-col gap-2">
                    {matchSuggestionsQuery.data.map((candidate) => (
                      <li
                        key={candidate.journalEntryId}
                        className="flex flex-wrap items-center justify-between gap-2 rounded-md border border-border p-2 text-sm"
                      >
                        <span>
                          {candidate.reference ?? candidate.journalEntryId.slice(0, 8)} ·{' '}
                          {new Date(candidate.entryDate).toLocaleDateString()} · {candidate.amount.toLocaleString()}
                        </span>
                        <Button
                          type="button"
                          variant="secondary"
                          onClick={() => {
                            setReconcileValue('matchedJournalEntryId', candidate.journalEntryId);
                            setSuggestingLineId(null);
                          }}
                        >
                          Use this match
                        </Button>
                      </li>
                    ))}
                  </ul>
                )}
              </Card>
            );
          })()}
        </>
      )}
    </div>
  );
}
