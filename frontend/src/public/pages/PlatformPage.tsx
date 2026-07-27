import { useNavigate } from 'react-router-dom';
import { ShieldCheck, Cpu, Database, LogIn } from 'lucide-react';

export function PlatformPage() {
  const navigate = useNavigate();

  return (
    <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-12 space-y-16">
      <div className="text-center max-w-3xl mx-auto space-y-4">
        <span className="rounded-full bg-primary/10 px-3.5 py-1 text-xs font-bold text-primary uppercase tracking-wider">
          Enterprise Cloud Architecture
        </span>
        <h1 className="text-4xl font-extrabold tracking-tight text-text sm:text-5xl">
          The FusionOS Engine: Built for Unlimited Scale
        </h1>
        <p className="text-base text-text-muted">
          CQRS event-driven architecture, granular RBAC security, and multi-tenant database isolation designed to power mission-critical operations.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-8 md:grid-cols-3">
        <div className="rounded-2xl border border-border bg-surface p-6 space-y-3">
          <Database size={28} className="text-primary" />
          <h3 className="text-lg font-bold text-text">CQRS & Event Sourcing</h3>
          <p className="text-xs text-text-muted leading-relaxed">
            Separates write commands from read queries for sub-millisecond data retrieval and immutable transaction logging.
          </p>
        </div>
        <div className="rounded-2xl border border-border bg-surface p-6 space-y-3">
          <ShieldCheck size={28} className="text-emerald-500" />
          <h3 className="text-lg font-bold text-text">Granular RBAC Security</h3>
          <p className="text-xs text-text-muted leading-relaxed">
            Every module route and API command is strictly gated by permission catalog claims, ensuring strict role enforcement.
          </p>
        </div>
        <div className="rounded-2xl border border-border bg-surface p-6 space-y-3">
          <Cpu size={28} className="text-purple-600" />
          <h3 className="text-lg font-bold text-text">Real-Time Sync Engine</h3>
          <p className="text-xs text-text-muted leading-relaxed">
            Instant automatic updates across Inventory, Warehouse, Sales Orders, and General Ledger without manual batch syncs.
          </p>
        </div>
      </div>

      <div className="rounded-3xl border border-border bg-surface p-8 sm:p-12 flex flex-col md:flex-row items-center justify-between gap-6">
        <div>
          <h2 className="text-2xl font-bold text-text">Deploy on Fusion Cloud or On-Premise</h2>
          <p className="text-sm text-text-muted mt-1 max-w-xl">
            Flexible enterprise hosting models with 99.99% SLA guarantee or local air-gapped server installation.
          </p>
        </div>
        <button
          onClick={() => navigate('/login')}
          className="rounded-xl bg-primary px-6 py-3 text-sm font-semibold text-primary-foreground flex items-center gap-2"
        >
          <LogIn size={16} /> Access App Console
        </button>
      </div>
    </div>
  );
}
