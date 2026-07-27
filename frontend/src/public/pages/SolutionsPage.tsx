import { Factory, Package, ArrowRight } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

export function SolutionsPage() {
  const navigate = useNavigate();

  return (
    <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-12 space-y-16">
      <div className="text-center max-w-3xl mx-auto space-y-4">
        <span className="rounded-full bg-emerald-500/10 px-3.5 py-1 text-xs font-bold text-emerald-600 dark:text-emerald-400 uppercase tracking-wider">
          Tailored Industry Blueprints
        </span>
        <h1 className="text-4xl font-extrabold tracking-tight text-text sm:text-5xl">
          Enterprise Solutions Tailored for Your Business Model
        </h1>
        <p className="text-base text-text-muted">
          Whether you manage discrete manufacturing, high-velocity wholesale trading, or multi-channel retail, FusionOS adapts to your specific operational needs.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-8 md:grid-cols-2">
        <div className="rounded-2xl border border-border bg-surface p-8 space-y-4 shadow-xs">
          <Factory size={32} className="text-primary" />
          <h2 className="text-xl font-bold text-text">Discrete & Process Manufacturing ERP</h2>
          <p className="text-xs text-text-muted leading-relaxed">
            Multi-level Bills of Materials (BOM), versions, operations routing templates, work center capacity planning, scrap/rework logging, and automated Material Requirements Planning (MRP).
          </p>
          <button onClick={() => navigate('/login')} className="text-xs font-bold text-primary flex items-center gap-1">
            Launch Manufacturing Module <ArrowRight size={14} />
          </button>
        </div>

        <div className="rounded-2xl border border-border bg-surface p-8 space-y-4 shadow-xs">
          <Package size={32} className="text-purple-600" />
          <h2 className="text-xl font-bold text-text">Trading & Multi-Location Wholesale</h2>
          <p className="text-xs text-text-muted leading-relaxed">
            Inter-warehouse stock transfers, batch expiry tracking, barcode scanning, purchase order approvals, tiered pricing rules, and sales commission calculations.
          </p>
          <button onClick={() => navigate('/login')} className="text-xs font-bold text-primary flex items-center gap-1">
            Launch Trading Module <ArrowRight size={14} />
          </button>
        </div>
      </div>
    </div>
  );
}
