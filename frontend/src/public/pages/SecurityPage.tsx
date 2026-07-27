import { Lock, Key, Database } from 'lucide-react';

export function SecurityPage() {
  return (
    <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-12 space-y-16">
      <div className="text-center max-w-3xl mx-auto space-y-4">
        <span className="rounded-full bg-emerald-500/10 px-3.5 py-1 text-xs font-bold text-emerald-600 dark:text-emerald-400 uppercase tracking-wider">
          Enterprise Security & Compliance
        </span>
        <h1 className="text-4xl font-extrabold tracking-tight text-text sm:text-5xl">
          Bank-Grade Security for Your Core Data
        </h1>
        <p className="text-base text-text-muted">
          FusionOS enforces military-grade data protection, zero-trust RBAC access controls, and SOC 2 compliance.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-8 md:grid-cols-3">
        <div className="rounded-2xl border border-border bg-surface p-6 space-y-3">
          <Lock size={28} className="text-primary" />
          <h3 className="text-lg font-bold text-text">AES-256 & TLS 1.3 Encryption</h3>
          <p className="text-xs text-text-muted leading-relaxed">
            Data is encrypted at rest using AES-256 and in transit via TLS 1.3 encryption protocols.
          </p>
        </div>
        <div className="rounded-2xl border border-border bg-surface p-6 space-y-3">
          <Key size={28} className="text-purple-600" />
          <h3 className="text-lg font-bold text-text">Fine-Grained RBAC Controls</h3>
          <p className="text-xs text-text-muted leading-relaxed">
            Assign custom permission codes to roles to ensure staff only access authorized screens and actions.
          </p>
        </div>
        <div className="rounded-2xl border border-border bg-surface p-6 space-y-3">
          <Database size={28} className="text-emerald-500" />
          <h3 className="text-lg font-bold text-text">Immutable Audit Logging</h3>
          <p className="text-xs text-text-muted leading-relaxed">
            Every system mutation, user login, and record edit is recorded in an tamper-proof audit trail.
          </p>
        </div>
      </div>
    </div>
  );
}
