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

const CURRENCY_CODE_REGEX = /^[A-Za-z]{3}$/;

const schema = z.object({
  fromCurrencyCode: z.string().regex(CURRENCY_CODE_REGEX, 'Must be a 3-letter ISO 4217 code, e.g. USD'),
  toCurrencyCode: z.string().regex(CURRENCY_CODE_REGEX, 'Must be a 3-letter ISO 4217 code, e.g. EUR'),
  rate: z.string().refine((v) => Number(v) > 0, 'Rate must be greater than zero'),
  effectiveDate: z.string().min(1, 'Effective date is required'),
});
type FormValues = z.infer<typeof schema>;

// Update deliberately excludes From/To currency codes — the currency pair is
// the immutable business key, see UpdateExchangeRateCommand.cs /
// ExchangeRatesController.Update.
const editSchema = z.object({
  rate: z.string().refine((v) => Number(v) > 0, 'Rate must be greater than zero'),
  effectiveDate: z.string().min(1, 'Effective date is required'),
});
type EditFormValues = z.infer<typeof editSchema>;

const convertSchema = z.object({
  fromCurrencyCode: z.string().regex(CURRENCY_CODE_REGEX, 'Must be a 3-letter ISO 4217 code'),
  toCurrencyCode: z.string().regex(CURRENCY_CODE_REGEX, 'Must be a 3-letter ISO 4217 code'),
  amount: z.string().refine((v) => v.trim() !== '' && !Number.isNaN(Number(v)), 'Enter a numeric amount'),
});
type ConvertFormValues = z.infer<typeof convertSchema>;

interface ExchangeRateDto {
  id: string;
  fromCurrencyCode: string;
  toCurrencyCode: string;
  rate: number;
  effectiveDate: string;
  isActive: boolean;
  createdAt: string;
}

interface ConversionResultDto {
  originalAmount: number;
  fromCurrencyCode: string;
  toCurrencyCode: string;
  convertedAmount: number;
  rateUsed: number;
  effectiveDateOfRateUsed: string;
}

/**
 * Exchange Rates — M8e, Finance depth: multi-currency support. Master data
 * for dated FX rates between two ISO 4217 currency codes, same CRUD shape as
 * CostCentersPanel/TaxRatesPanel, plus a small "convert an amount" utility
 * widget wired to GET .../exchange-rates/convert (ConvertAmountQuery). No
 * existing screen anywhere in the app (Accounts, Journal Entries, Payables,
 * Receivables, ...) has been made currency-aware yet — this panel only
 * manages rates and answers "what would this convert to right now," see
 * ExchangeRate.cs's own class doc comment for the scope line. Rendered as a
 * sibling panel under AccountsPage, right after BankStatementLinesPanel.
 */
export function ExchangeRatesPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [editingExchangeRateId, setEditingExchangeRateId] = useState<string | null>(null);

  const exchangeRatesQuery = useQuery({
    queryKey: ['exchange-rates', companyId],
    queryFn: () => apiClient.get<PagedResult<ExchangeRateDto>>(`/finance/exchange-rates?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { fromCurrencyCode: '', toCurrencyCode: '', rate: '', effectiveDate: '' },
  });

  const createExchangeRate = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<ExchangeRateDto>('/finance/exchange-rates', {
        companyId,
        fromCurrencyCode: values.fromCurrencyCode.toUpperCase(),
        toCurrencyCode: values.toCurrencyCode.toUpperCase(),
        rate: Number(values.rate),
        effectiveDate: new Date(values.effectiveDate).toISOString(),
      }),
    onSuccess: () => {
      reset({ fromCurrencyCode: '', toCurrencyCode: '', rate: '', effectiveDate: '' });
      queryClient.invalidateQueries({ queryKey: ['exchange-rates', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  // Soft-deactivate only — ExchangeRatesController exposes this as a dedicated
  // POST .../{id}/deactivate action (never a DELETE), same convention as
  // every other M8 sub-slice.
  const deactivateExchangeRate = useMutation({
    mutationFn: (exchangeRateId: string) => apiClient.post<ExchangeRateDto>(`/finance/exchange-rates/${exchangeRateId}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['exchange-rates', companyId] }),
  });

  if (!companyId) return null;

  const editingExchangeRate = exchangeRatesQuery.data?.data.find((r) => r.id === editingExchangeRateId) ?? null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Exchange Rates</h2>
      <p className="mb-3 text-xs text-text-muted">
        Dated FX rate master data plus a conversion utility — no existing screen anywhere in FusionOS is
        currency-aware yet (Accounts, Journal Entries, Payables, Receivables all still assume a single implicit
        currency). This is foundational rate data and an on-demand conversion query only.
      </p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createExchangeRate.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            From currency
            <Controller
              control={control}
              name="fromCurrencyCode"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5 uppercase" placeholder="USD" maxLength={3} {...field} />
              )}
            />
            {errors.fromCurrencyCode && <span className="text-xs text-danger">{errors.fromCurrencyCode.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            To currency
            <Controller
              control={control}
              name="toCurrencyCode"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5 uppercase" placeholder="EUR" maxLength={3} {...field} />
              )}
            />
            {errors.toCurrencyCode && <span className="text-xs text-danger">{errors.toCurrencyCode.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Rate (1 From = Rate To)
            <Controller
              control={control}
              name="rate"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="0.92" {...field} />
              )}
            />
            {errors.rate && <span className="text-xs text-danger">{errors.rate.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Effective date
            <Controller
              control={control}
              name="effectiveDate"
              render={({ field }) => (
                <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.effectiveDate && <span className="text-xs text-danger">{errors.effectiveDate.message}</span>}
          </label>
          <div className="col-span-2">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create exchange rate'}</Button>
          </div>
        </form>
        {createExchangeRate.isError && createExchangeRate.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createExchangeRate.error.problem.title}</p>
        )}
      </Card>

      <Card className="mb-6">
        <DataTable
          columns={[
            { header: 'From', render: (row: ExchangeRateDto) => row.fromCurrencyCode },
            { header: 'To', render: (row: ExchangeRateDto) => row.toCurrencyCode },
            { header: 'Rate', render: (row: ExchangeRateDto) => row.rate.toLocaleString(undefined, { maximumFractionDigits: 6 }) },
            { header: 'Effective', render: (row: ExchangeRateDto) => new Date(row.effectiveDate).toLocaleDateString() },
            { header: 'Status', render: (row: ExchangeRateDto) => (row.isActive ? 'Active' : 'Inactive') },
            {
              header: 'Actions',
              render: (row: ExchangeRateDto) => (
                <div className="flex items-center gap-2">
                  <Button type="button" variant="secondary" onClick={() => setEditingExchangeRateId(row.id)}>
                    Edit
                  </Button>
                  <Button
                    type="button"
                    variant="danger"
                    disabled={!row.isActive || deactivateExchangeRate.isPending}
                    onClick={() => deactivateExchangeRate.mutate(row.id)}
                  >
                    {row.isActive ? 'Deactivate' : 'Deactivated'}
                  </Button>
                </div>
              ),
            },
          ]}
          rows={exchangeRatesQuery.data?.data}
          isLoading={exchangeRatesQuery.isLoading}
          isError={exchangeRatesQuery.isError}
          errorMessage="Could not load exchange rates."
          emptyMessage="No exchange rates yet — create the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {deactivateExchangeRate.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that exchange rate.</p>
      )}

      {editingExchangeRate && (
        <ExchangeRateEditPanel
          companyId={companyId}
          exchangeRate={editingExchangeRate}
          onClose={() => setEditingExchangeRateId(null)}
        />
      )}

      <ConvertAmountWidget companyId={companyId} />
    </div>
  );
}

interface ExchangeRateEditPanelProps {
  companyId: string;
  exchangeRate: ExchangeRateDto;
  onClose: () => void;
}

function ExchangeRateEditPanel({ companyId, exchangeRate, onClose }: ExchangeRateEditPanelProps) {
  const queryClient = useQueryClient();

  const { control, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    values: {
      rate: String(exchangeRate.rate),
      effectiveDate: exchangeRate.effectiveDate.slice(0, 10),
    },
  });

  const updateExchangeRate = useMutation({
    mutationFn: (values: EditFormValues) =>
      apiClient.put<ExchangeRateDto>(`/finance/exchange-rates/${exchangeRate.id}`, {
        companyId,
        rate: Number(values.rate),
        effectiveDate: new Date(values.effectiveDate).toISOString(),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['exchange-rates', companyId] });
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
        <h3 className="text-base font-semibold text-text">Edit rate — {exchangeRate.fromCurrencyCode} → {exchangeRate.toCurrencyCode}</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form onSubmit={handleSubmit((values) => updateExchangeRate.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <label className="flex flex-col gap-1 text-sm">
          Rate
          <Controller
            control={control}
            name="rate"
            render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )}
          />
          {errors.rate && <span className="text-xs text-danger">{errors.rate.message}</span>}
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Effective date
          <Controller
            control={control}
            name="effectiveDate"
            render={({ field }) => (
              <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )}
          />
          {errors.effectiveDate && <span className="text-xs text-danger">{errors.effectiveDate.message}</span>}
        </label>
        <div className="col-span-2 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save changes'}</Button>
          {updateExchangeRate.isError && (
            <span role="alert" className="text-sm text-danger">Could not save that exchange rate.</span>
          )}
        </div>
      </form>
    </Card>
  );
}

/** The one bit of actual "convert an amount" behavior this slice ships — a pure, on-demand call to GET .../exchange-rates/convert (ConvertAmountQuery), not wired into any other screen's totals. */
function ConvertAmountWidget({ companyId }: { companyId: string }) {
  const [result, setResult] = useState<ConversionResultDto | null>(null);

  const { control, handleSubmit, formState: { errors } } = useForm<ConvertFormValues>({
    resolver: zodResolver(convertSchema),
    defaultValues: { fromCurrencyCode: '', toCurrencyCode: '', amount: '' },
  });

  const convert = useMutation({
    mutationFn: (values: ConvertFormValues) => {
      const params = new URLSearchParams({
        companyId,
        from: values.fromCurrencyCode.toUpperCase(),
        to: values.toCurrencyCode.toUpperCase(),
        amount: values.amount,
      });
      return apiClient.get<ConversionResultDto>(`/finance/exchange-rates/convert?${params.toString()}`);
    },
    onSuccess: (data) => setResult(data),
  });

  return (
    <Card className="mt-6">
      <h3 className="mb-3 text-sm font-semibold text-text">Convert an amount</h3>
      <p className="mb-3 text-xs text-text-muted">
        Looks up the latest active rate for the pair and multiplies — a pure, read-only utility, not wired into any
        other screen's totals.
      </p>
      <form onSubmit={handleSubmit((values) => convert.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-4">
        <label className="flex flex-col gap-1 text-sm">
          From
          <Controller
            control={control}
            name="fromCurrencyCode"
            render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5 uppercase" placeholder="USD" maxLength={3} {...field} />
            )}
          />
          {errors.fromCurrencyCode && <span className="text-xs text-danger">{errors.fromCurrencyCode.message}</span>}
        </label>
        <label className="flex flex-col gap-1 text-sm">
          To
          <Controller
            control={control}
            name="toCurrencyCode"
            render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5 uppercase" placeholder="EUR" maxLength={3} {...field} />
            )}
          />
          {errors.toCurrencyCode && <span className="text-xs text-danger">{errors.toCurrencyCode.message}</span>}
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Amount
          <Controller
            control={control}
            name="amount"
            render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="100" {...field} />
            )}
          />
          {errors.amount && <span className="text-xs text-danger">{errors.amount.message}</span>}
        </label>
        <div className="flex items-end">
          <Button type="submit" disabled={convert.isPending}>{convert.isPending ? 'Converting…' : 'Convert'}</Button>
        </div>
      </form>
      {convert.isError && convert.error instanceof ApiError && (
        <p role="alert" className="mt-2 text-sm text-danger">{convert.error.problem.title}</p>
      )}
      {result && (
        <p className="mt-3 text-sm text-text">
          {result.originalAmount.toLocaleString()} {result.fromCurrencyCode} ={' '}
          <span className="font-semibold">{result.convertedAmount.toLocaleString(undefined, { maximumFractionDigits: 6 })} {result.toCurrencyCode}</span>{' '}
          (rate {result.rateUsed} as of {new Date(result.effectiveDateOfRateUsed).toLocaleDateString()})
        </p>
      )}
    </Card>
  );
}
