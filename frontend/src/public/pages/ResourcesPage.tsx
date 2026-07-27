import { FileText, Code, BookOpen, ExternalLink } from 'lucide-react';
import { Link } from 'react-router-dom';

export function ResourcesPage() {
  return (
    <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-12 space-y-16">
      <div className="text-center max-w-3xl mx-auto space-y-4">
        <span className="rounded-full bg-primary/10 px-3.5 py-1 text-xs font-bold text-primary uppercase tracking-wider">
          Developer & Operator Hub
        </span>
        <h1 className="text-4xl font-extrabold tracking-tight text-text sm:text-5xl">
          FusionOS Resources & Documentation
        </h1>
        <p className="text-base text-text-muted">
          Comprehensive API specifications, developer guides, user manuals, and release updates.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-8 md:grid-cols-3">
        <div className="rounded-2xl border border-border bg-surface p-6 space-y-3">
          <Code size={28} className="text-primary" />
          <h3 className="text-lg font-bold text-text">REST API & Webhooks</h3>
          <p className="text-xs text-text-muted leading-relaxed">
            Standard HTTP OpenAPI specifications for programmatic access to Inventory, Sales, and Finance entities.
          </p>
          <Link to="/login" className="text-xs font-semibold text-primary inline-flex items-center gap-1">
            Explore API Keys <ExternalLink size={12} />
          </Link>
        </div>
        <div className="rounded-2xl border border-border bg-surface p-6 space-y-3">
          <FileText size={28} className="text-purple-600" />
          <h3 className="text-lg font-bold text-text">Module User Manuals</h3>
          <p className="text-xs text-text-muted leading-relaxed">
            Step-by-step documentation for setting up UOM conversions, multi-level BOMs, and GST tax filing.
          </p>
        </div>
        <div className="rounded-2xl border border-border bg-surface p-6 space-y-3">
          <BookOpen size={28} className="text-emerald-500" />
          <h3 className="text-lg font-bold text-text">Release Notes & Roadmap</h3>
          <p className="text-xs text-text-muted leading-relaxed">
            Stay up to date with new CQRS module releases, UI enhancements, and security patches.
          </p>
        </div>
      </div>
    </div>
  );
}
