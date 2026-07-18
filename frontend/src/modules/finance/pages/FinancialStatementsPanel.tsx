import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../../../shared/api/client';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';

interface TrialBalanceLineDto {
  accountId: string;
  accountCode: string;
  accountName: string;
  totalDebit: number;
  totalCredit: number;
  netBalance: number;
}
interface TrialBalanceReportDto {
  asOfDate: string;
  lines: TrialBalanceLineDto[];
  totalDebit: number;
  totalCredit: number;
  isBalanced: boolean;
}

interface ProfitAndLossLineDto {
  accountId: string;
  accountCode: string;
  accountName: string;
  amount: number;
}
interface ProfitAndLossReportDto {
  periodStart: string;
  periodEnd: string;
  revenueLines: ProfitAndLossLineDto[];
  expenseLines: ProfitAndLossLineDto[];
  totalRevenue: number;
  totalExpenses: number;
  netIncome: number;
}

interface BalanceSheetLineDto {
  accountId: string;
  accountCode: string;
  accountName: string;
  amount: number;
}
interface BalanceSheetReportDto {
  asOfDate: string;
  assetLines: BalanceSheetLineDto[];
  liabilityLines: BalanceSheetLineDto[];
  equityLines: BalanceSheetLineDto[];
  totalAssets: number;
  totalLiabilities: number;
  totalEquity: number;
  isBalanced: boolean;
}

function todayIsoDate(): string {
  return new Date().toISOString().slice(0, 10);
}

function firstOfMonthIsoDate(): string {
  const d = new Date();
  return new Date(d.getFullYear(), d.getMonth(), 1).toISOString().slice(0, 10);
}

/**
 * GL financial statements — Trial Balance (Phase M6, 2026-07-15, previously
 * backend-only with no frontend at all — a genuine gap this pass closes
 * alongside two new reports), Profit & Loss, and Balance Sheet (both Phase 2
 * closeout, 2026-07-18). All three are canned reports computed entirely from
 * posted JournalEntry activity — no new aggregate, see the backend
 * Reports/Queries handlers for the actual GL folds.
 */
export function FinancialStatementsPanel() {
  const { companyId } = useActiveCompany();

  const [asOfDate, setAsOfDate] = useState(todayIsoDate());
  const [periodStart, setPeriodStart] = useState(firstOfMonthIsoDate());
  const [periodEnd, setPeriodEnd] = useState(todayIsoDate());

  const trialBalanceQuery = useQuery({
    queryKey: ['trial-balance', companyId, asOfDate],
    queryFn: () => apiClient.get<TrialBalanceReportDto>(`/finance/reports/trial-balance?companyId=${companyId}&asOfDate=${asOfDate}`),
    enabled: Boolean(companyId && asOfDate),
  });

  const profitAndLossQuery = useQuery({
    queryKey: ['profit-and-loss', companyId, periodStart, periodEnd],
    queryFn: () => apiClient.get<ProfitAndLossReportDto>(`/finance/reports/profit-and-loss?companyId=${companyId}&periodStart=${periodStart}&periodEnd=${periodEnd}`),
    enabled: Boolean(companyId && periodStart && periodEnd),
  });

  const balanceSheetQuery = useQuery({
    queryKey: ['balance-sheet', companyId, asOfDate],
    queryFn: () => apiClient.get<BalanceSheetReportDto>(`/finance/reports/balance-sheet?companyId=${companyId}&asOfDate=${asOfDate}`),
    enabled: Boolean(companyId && asOfDate),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Financial Statements</h2>

      <Card className="mb-6">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <label className="flex flex-col gap-1 text-sm">
            As of date (Trial Balance / Balance Sheet)
            <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" value={asOfDate} onChange={(e) => setAsOfDate(e.target.value)} />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            P&amp;L period start
            <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" value={periodStart} onChange={(e) => setPeriodStart(e.target.value)} />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            P&amp;L period end
            <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" value={periodEnd} onChange={(e) => setPeriodEnd(e.target.value)} />
          </label>
        </div>
      </Card>

      <h3 className="mb-2 text-base font-semibold text-text">Trial Balance</h3>
      <Card className="mb-4">
        <DataTable
          columns={[
            { header: 'Code', render: (l: TrialBalanceLineDto) => l.accountCode },
            { header: 'Account', render: (l: TrialBalanceLineDto) => l.accountName },
            { header: 'Debit', render: (l: TrialBalanceLineDto) => l.totalDebit.toLocaleString() },
            { header: 'Credit', render: (l: TrialBalanceLineDto) => l.totalCredit.toLocaleString() },
            { header: 'Net', render: (l: TrialBalanceLineDto) => l.netBalance.toLocaleString() },
          ]}
          rows={trialBalanceQuery.data?.lines}
          isLoading={trialBalanceQuery.isLoading}
          isError={trialBalanceQuery.isError}
          errorMessage="Could not load the trial balance."
          emptyMessage="No posted activity as of this date."
          rowKey={(l) => l.accountId}
        />
        {trialBalanceQuery.data && (
          <p className="mt-3 text-sm">
            Total debit: <span className="font-semibold">{trialBalanceQuery.data.totalDebit.toLocaleString()}</span>
            {' · '}Total credit: <span className="font-semibold">{trialBalanceQuery.data.totalCredit.toLocaleString()}</span>
            {' · '}{trialBalanceQuery.data.isBalanced ? '✓ Balanced' : '⚠ Not balanced'}
          </p>
        )}
      </Card>

      <h3 className="mb-2 text-base font-semibold text-text">Profit &amp; Loss</h3>
      <Card className="mb-4">
        <p className="mb-2 text-sm font-medium text-text-muted">Revenue</p>
        <DataTable
          columns={[
            { header: 'Code', render: (l: ProfitAndLossLineDto) => l.accountCode },
            { header: 'Account', render: (l: ProfitAndLossLineDto) => l.accountName },
            { header: 'Amount', render: (l: ProfitAndLossLineDto) => l.amount.toLocaleString() },
          ]}
          rows={profitAndLossQuery.data?.revenueLines}
          isLoading={profitAndLossQuery.isLoading}
          isError={profitAndLossQuery.isError}
          errorMessage="Could not load the profit and loss report."
          emptyMessage="No revenue activity for this period."
          rowKey={(l) => l.accountId}
        />
        <p className="mb-2 mt-4 text-sm font-medium text-text-muted">Expenses</p>
        <DataTable
          columns={[
            { header: 'Code', render: (l: ProfitAndLossLineDto) => l.accountCode },
            { header: 'Account', render: (l: ProfitAndLossLineDto) => l.accountName },
            { header: 'Amount', render: (l: ProfitAndLossLineDto) => l.amount.toLocaleString() },
          ]}
          rows={profitAndLossQuery.data?.expenseLines}
          isLoading={profitAndLossQuery.isLoading}
          emptyMessage="No expense activity for this period."
          rowKey={(l) => l.accountId}
        />
        {profitAndLossQuery.data && (
          <p className="mt-3 text-sm">
            Total revenue: <span className="font-semibold">{profitAndLossQuery.data.totalRevenue.toLocaleString()}</span>
            {' · '}Total expenses: <span className="font-semibold">{profitAndLossQuery.data.totalExpenses.toLocaleString()}</span>
            {' · '}Net income: <span className="font-semibold">{profitAndLossQuery.data.netIncome.toLocaleString()}</span>
          </p>
        )}
      </Card>

      <h3 className="mb-2 text-base font-semibold text-text">Balance Sheet</h3>
      <Card>
        <p className="mb-2 text-sm font-medium text-text-muted">Assets</p>
        <DataTable
          columns={[
            { header: 'Code', render: (l: BalanceSheetLineDto) => l.accountCode },
            { header: 'Account', render: (l: BalanceSheetLineDto) => l.accountName },
            { header: 'Amount', render: (l: BalanceSheetLineDto) => l.amount.toLocaleString() },
          ]}
          rows={balanceSheetQuery.data?.assetLines}
          isLoading={balanceSheetQuery.isLoading}
          isError={balanceSheetQuery.isError}
          errorMessage="Could not load the balance sheet."
          emptyMessage="No asset balances as of this date."
          rowKey={(l) => l.accountId}
        />
        <p className="mb-2 mt-4 text-sm font-medium text-text-muted">Liabilities</p>
        <DataTable
          columns={[
            { header: 'Code', render: (l: BalanceSheetLineDto) => l.accountCode },
            { header: 'Account', render: (l: BalanceSheetLineDto) => l.accountName },
            { header: 'Amount', render: (l: BalanceSheetLineDto) => l.amount.toLocaleString() },
          ]}
          rows={balanceSheetQuery.data?.liabilityLines}
          isLoading={balanceSheetQuery.isLoading}
          emptyMessage="No liability balances as of this date."
          rowKey={(l) => l.accountId}
        />
        <p className="mb-2 mt-4 text-sm font-medium text-text-muted">Equity</p>
        <DataTable
          columns={[
            { header: 'Code', render: (l: BalanceSheetLineDto) => l.accountCode },
            { header: 'Account', render: (l: BalanceSheetLineDto) => l.accountName },
            { header: 'Amount', render: (l: BalanceSheetLineDto) => l.amount.toLocaleString() },
          ]}
          rows={balanceSheetQuery.data?.equityLines}
          isLoading={balanceSheetQuery.isLoading}
          emptyMessage="No equity balances as of this date."
          rowKey={(l) => l.accountId}
        />
        {balanceSheetQuery.data && (
          <p className="mt-3 text-sm">
            Total assets: <span className="font-semibold">{balanceSheetQuery.data.totalAssets.toLocaleString()}</span>
            {' · '}Total liabilities: <span className="font-semibold">{balanceSheetQuery.data.totalLiabilities.toLocaleString()}</span>
            {' · '}Total equity: <span className="font-semibold">{balanceSheetQuery.data.totalEquity.toLocaleString()}</span>
            {' · '}{balanceSheetQuery.data.isBalanced ? '✓ Balanced' : '⚠ Not balanced'}
          </p>
        )}
      </Card>
    </div>
  );
}
