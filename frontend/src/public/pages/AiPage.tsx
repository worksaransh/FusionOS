import { useNavigate } from 'react-router-dom';
import { Sparkles, FileText, LogIn } from 'lucide-react';

export function AiPage() {
  const navigate = useNavigate();

  return (
    <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-12 space-y-16">
      <div className="text-center max-w-3xl mx-auto space-y-4">
        <span className="rounded-full bg-purple-500/10 px-3.5 py-1 text-xs font-bold text-purple-600 dark:text-purple-400 uppercase tracking-wider">
          AI-Powered Enterprise Intelligence
        </span>
        <h1 className="text-4xl font-extrabold tracking-tight text-text sm:text-5xl">
          Fusion Copilot & Automated Predictive AI
        </h1>
        <p className="text-base text-text-muted">
          Transform raw operational data into proactive recommendations, automated invoice entry, and real-time inventory demand planning.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-8 md:grid-cols-2">
        <div className="rounded-2xl border border-purple-500/30 bg-purple-500/5 p-8 space-y-4">
          <Sparkles className="text-purple-600" size={32} />
          <h2 className="text-xl font-bold text-text">Demand & Reorder Forecasting</h2>
          <p className="text-xs text-text-muted leading-relaxed">
            Machine learning algorithms analyze seasonal order velocity, supplier lead times, and current stock on-hand to suggest optimal Purchase Order quantities before stockouts occur.
          </p>
        </div>

        <div className="rounded-2xl border border-border bg-surface p-8 space-y-4">
          <FileText className="text-primary" size={32} />
          <h2 className="text-xl font-bold text-text">Optical Character Recognition (OCR)</h2>
          <p className="text-xs text-text-muted leading-relaxed">
            Upload PDF supplier invoices, bills of lading, and shipping receipts. Fusion AI automatically parses SKUs, quantities, and totals straight into Accounts Payable entries.
          </p>
        </div>
      </div>

      <div className="rounded-3xl bg-gradient-to-r from-purple-900 to-indigo-900 text-white p-8 sm:p-12 text-center space-y-6">
        <h2 className="text-3xl font-extrabold">Experience AI-Driven Enterprise Operations</h2>
        <p className="text-sm max-w-xl mx-auto opacity-90">
          Fusion Copilot is pre-integrated into your active workspace. Sign in to test queries and AI narrative reports.
        </p>
        <button
          onClick={() => navigate('/login')}
          className="rounded-xl bg-white px-6 py-3 text-sm font-bold text-purple-900 hover:bg-slate-100 transition-all inline-flex items-center gap-2"
        >
          <LogIn size={16} /> Sign In to Access Fusion AI
        </button>
      </div>
    </div>
  );
}
