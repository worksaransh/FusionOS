import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../../../shared/api/client';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';

interface GstReturnComponentLineDto {
  gstComponent: string;
  outputTaxCollected: number;
  inputTaxCredit: number;
  netPayable: number;
}
interface GstReturnSummaryReportDto {
  periodStart: string;
  periodEnd: string;
  components: GstReturnComponentLineDto[];
  totalOutputTaxCollected: number;
  totalInputTaxCredit: number;
  totalNetPayable: number;
}

function todayIsoDate(): string {
  return new Date().toISOString().slice(0, 10);
}

function firstOfMonthIsoDate(): string {
  const d = new Date();
  return new Date(d.getFullYear(), d.getMonth(), 1).toISOString().slice(0, 10);
}

/**
 * GST Return Summary — GST filing support (2026-07-22). Unlike
 * TaxPeriodSummaryPanel (the rate-register scaffold with "Not available"
 * amounts), this reads real TaxTransaction rows recorded by
 * InvoiceIssuedConsumer/PurchaseOrderGoodsReceiptCostedConsumer, grouped by
 * CGST/SGST/IGST — the breakdown a GSTR-1/GSTR-3B filing needs.
 */
export function GstReturnSummaryPanel() {
  const { companyId } = useActiveCompany();

  const [periodStart, setPeriodStart] = useState(firstOfMonthIsoDate());
  const [periodEnd, setPeriodEnd] = useState(todayIsoDate());

  const gstReturnQuery = useQuery({
    queryKey: ['gst-return-summary', companyId, periodStart, periodEnd],
    queryFn: () =>
      apiClient.get<GstReturnSummaryReportDto>(
        `/finance/reports/gst-return-summary?companyId=${companyId}&periodStart=${periodStart}&periodEnd=${periodEnd}`,
      ),
    enabled: Boolean(companyId && periodStart && periodEnd),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">GST Return Summary</h2>
      <p className="mb-3 text-xs text-text-muted">
        Output tax collected, input tax credit, and net payable by GST component (CGST/SGST/IGST) for a filing period —
        the GSTR-1/GSTR-3B-style breakdown.
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
            { header: 'GST component', render: (l: GstReturnComponentLineDto) => l.gstComponent },
            { header: 'Output tax collected', render: (l: GstReturnComponentLineDto) => l.outputTaxCollected.toLocaleString() },
            { header: 'Input tax credit', render: (l: GstReturnComponentLineDto) => l.inputTaxCredit.toLocaleString() },
            { header: 'Net payable', render: (l: GstReturnComponentLineDto) => l.netPayable.toLocaleString() },
          ]}
          rows={gstReturnQuery.data?.components}
          isLoading={gstReturnQuery.isLoading}
          isError={gstReturnQuery.isError}
          errorMessage="Could not load the GST return summary."
          emptyMessage="No GST activity recorded for this period yet."
          rowKey={(l) => l.gstComponent}
        />
        {gstReturnQuery.data && (
          <p className="mt-3 text-sm">
            Total output tax: <span className="font-semibold">{gstReturnQuery.data.totalOutputTaxCollected.toLocaleString()}</span>
            {' · '}Total input tax credit: <span className="font-semibold">{gstReturnQuery.data.totalInputTaxCredit.toLocaleString()}</span>
            {' · '}Net payable: <span className="font-semibold">{gstReturnQuery.data.totalNetPayable.toLocaleString()}</span>
          </p>
        )}
      </Card>
    </div>
  );
}
