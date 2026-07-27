import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../../../shared/api/client';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';

interface TaxPeriodSummaryLineDto {
  taxJurisdictionId: string;
  taxJurisdictionCode: string;
  taxJurisdictionName: string;
  taxRateId: string;
  taxRateCode: string;
  taxRateName: string;
  percentage: number;
  outputTaxCollected: number | null;
  inputTaxPaid: number | null;
  netTaxPayable: number | null;
}
interface TaxPeriodSummaryReportDto {
  periodStart: string;
  periodEnd: string;
  lines: TaxPeriodSummaryLineDto[];
  totalOutputTaxCollected: number | null;
  totalInputTaxPaid: number | null;
  totalNetTaxPayable: number | null;
  dataAvailabilityNote: string;
}

function todayIsoDate(): string {
  return new Date().toISOString().slice(0, 10);
}

function firstOfMonthIsoDate(): string {
  const d = new Date();
  return new Date(d.getFullYear(), d.getMonth(), 1).toISOString().slice(0, 10);
}

function formatAmount(value: number | null): string {
  return value === null ? 'Not available' : value.toLocaleString();
}

/**
 * Tax Period Summary — the tax-filing register a Finance user would start
 * from to file a GST/VAT return: every active tax rate, grouped under its
 * jurisdiction, for a chosen period.
 *
 * Investigated and scoped down (2026-07-22): per-line tax already works
 * (Sales' InvoiceLine / Procurement's PurchaseOrderLine both carry a real
 * TaxRateId + TaxAmount), but that data lives in those modules' own schemas.
 * The only paths by which transactional amounts reach Finance at all —
 * Sales' InvoiceIssued and Procurement's PurchaseOrderGoodsReceiptCosted
 * integration events — do not carry a TaxRateId or TaxAmount today, so
 * output tax collected / input tax paid cannot actually be computed here.
 * This panel is upfront about that: amounts render as "Not available" rather
 * than a fabricated 0, and the backend's own explanation is shown verbatim
 * via dataAvailabilityNote.
 */
export function TaxPeriodSummaryPanel() {
  const { companyId } = useActiveCompany();

  const [periodStart, setPeriodStart] = useState(firstOfMonthIsoDate());
  const [periodEnd, setPeriodEnd] = useState(todayIsoDate());

  const taxPeriodSummaryQuery = useQuery({
    queryKey: ['tax-period-summary', companyId, periodStart, periodEnd],
    queryFn: () =>
      apiClient.get<TaxPeriodSummaryReportDto>(
        `/finance/reports/tax-period-summary?companyId=${companyId}&periodStart=${periodStart}&periodEnd=${periodEnd}`,
      ),
    enabled: Boolean(companyId && periodStart && periodEnd),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Tax Period Summary</h2>
      <p className="mb-3 text-xs text-text-muted">
        Active tax rates by jurisdiction for a filing period. Output tax collected / input tax paid amounts are not yet
        derivable from data Finance can see — see the note below for why.
      </p>

      <Card className="mb-4">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Period start
            <input
              type="date"
              className="rounded-md border border-border bg-surface px-2 py-1.5"
              value={periodStart}
              onChange={(e) => setPeriodStart(e.target.value)}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Period end
            <input
              type="date"
              className="rounded-md border border-border bg-surface px-2 py-1.5"
              value={periodEnd}
              onChange={(e) => setPeriodEnd(e.target.value)}
            />
          </label>
        </div>
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Jurisdiction', render: (l: TaxPeriodSummaryLineDto) => `${l.taxJurisdictionCode} — ${l.taxJurisdictionName}` },
            { header: 'Rate', render: (l: TaxPeriodSummaryLineDto) => `${l.taxRateCode} (${l.taxRateName})` },
            { header: 'Percentage', render: (l: TaxPeriodSummaryLineDto) => `${l.percentage}%` },
            { header: 'Output tax collected', render: (l: TaxPeriodSummaryLineDto) => formatAmount(l.outputTaxCollected) },
            { header: 'Input tax paid', render: (l: TaxPeriodSummaryLineDto) => formatAmount(l.inputTaxPaid) },
            { header: 'Net payable', render: (l: TaxPeriodSummaryLineDto) => formatAmount(l.netTaxPayable) },
          ]}
          rows={taxPeriodSummaryQuery.data?.lines}
          isLoading={taxPeriodSummaryQuery.isLoading}
          isError={taxPeriodSummaryQuery.isError}
          errorMessage="Could not load the tax period summary."
          emptyMessage="No active tax rates for this company yet."
          rowKey={(l) => l.taxRateId}
        />
        {taxPeriodSummaryQuery.data && (
          <p className="mt-3 text-sm">
            Total output tax: <span className="font-semibold">{formatAmount(taxPeriodSummaryQuery.data.totalOutputTaxCollected)}</span>
            {' · '}Total input tax: <span className="font-semibold">{formatAmount(taxPeriodSummaryQuery.data.totalInputTaxPaid)}</span>
            {' · '}Net payable: <span className="font-semibold">{formatAmount(taxPeriodSummaryQuery.data.totalNetTaxPayable)}</span>
          </p>
        )}
        {taxPeriodSummaryQuery.data?.dataAvailabilityNote && (
          <p className="mt-3 rounded-md border border-border bg-surface-muted p-3 text-xs text-text-muted">
            {taxPeriodSummaryQuery.data.dataAvailabilityNote}
          </p>
        )}
      </Card>
    </div>
  );
}
