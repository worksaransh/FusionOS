import { useState } from 'react';
import { X, CheckCircle2, Sparkles } from 'lucide-react';
import { Button } from '../../shared/ui/Button';

interface DemoRequestModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export function DemoRequestModal({ isOpen, onClose }: DemoRequestModalProps) {
  const [submitted, setSubmitted] = useState(false);
  const [formData, setFormData] = useState({
    fullName: '',
    workEmail: '',
    companyName: '',
    companySize: '50-200',
    primaryInterest: 'Manufacturing ERP',
  });

  if (!isOpen) return null;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitted(true);
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="fixed inset-0 bg-black/60 backdrop-blur-sm" onClick={onClose} aria-hidden="true" />
      <div className="relative z-10 w-full max-w-lg overflow-hidden rounded-2xl border border-border bg-surface p-6 shadow-2xl sm:p-8">
        <button
          onClick={onClose}
          className="absolute right-4 top-4 rounded-lg p-1.5 text-text-muted hover:bg-surface-muted hover:text-text transition-colors"
        >
          <X size={20} />
        </button>

        {submitted ? (
          <div className="py-8 text-center space-y-4">
            <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-emerald-500/10 text-emerald-500">
              <CheckCircle2 size={36} />
            </div>
            <h3 className="text-xl font-bold text-text">Demo Request Received!</h3>
            <p className="text-sm text-text-muted max-w-sm mx-auto">
              Thank you, <span className="font-semibold text-text">{formData.fullName}</span>. An enterprise solutions specialist from FusionOS will reach out to <span className="font-semibold text-text">{formData.workEmail}</span> within 2 hours.
            </p>
            <Button type="button" onClick={() => { setSubmitted(false); onClose(); }}>
              Done
            </Button>
          </div>
        ) : (
          <div>
            <div className="flex items-center gap-2 text-purple-600 dark:text-purple-400 text-xs font-bold uppercase tracking-wider mb-2">
              <Sparkles size={16} /> 1-on-1 Personalized Walkthrough
            </div>
            <h2 className="text-2xl font-bold tracking-tight text-text">Schedule a FusionOS Enterprise Demo</h2>
            <p className="mt-1 text-sm text-text-muted mb-6">
              Discover how FusionOS unifies your manufacturing, inventory, finance, and operations in one intelligent platform.
            </p>

            <form onSubmit={handleSubmit} className="space-y-4">
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <label className="flex flex-col gap-1 text-xs font-medium text-text">
                  Full Name *
                  <input
                    required
                    type="text"
                    placeholder="Jane Doe"
                    value={formData.fullName}
                    onChange={(e) => setFormData({ ...formData, fullName: e.target.value })}
                    className="rounded-lg border border-border bg-surface px-3 py-2 text-sm focus:border-primary focus:outline-none"
                  />
                </label>
                <label className="flex flex-col gap-1 text-xs font-medium text-text">
                  Work Email *
                  <input
                    required
                    type="email"
                    placeholder="jane@company.com"
                    value={formData.workEmail}
                    onChange={(e) => setFormData({ ...formData, workEmail: e.target.value })}
                    className="rounded-lg border border-border bg-surface px-3 py-2 text-sm focus:border-primary focus:outline-none"
                  />
                </label>
              </div>

              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <label className="flex flex-col gap-1 text-xs font-medium text-text">
                  Company Name *
                  <input
                    required
                    type="text"
                    placeholder="Acme Global Inc."
                    value={formData.companyName}
                    onChange={(e) => setFormData({ ...formData, companyName: e.target.value })}
                    className="rounded-lg border border-border bg-surface px-3 py-2 text-sm focus:border-primary focus:outline-none"
                  />
                </label>
                <label className="flex flex-col gap-1 text-xs font-medium text-text">
                  Company Size
                  <select
                    value={formData.companySize}
                    onChange={(e) => setFormData({ ...formData, companySize: e.target.value })}
                    className="rounded-lg border border-border bg-surface px-3 py-2 text-sm focus:border-primary focus:outline-none"
                  >
                    <option value="1-50">1 - 50 employees</option>
                    <option value="50-200">50 - 200 employees</option>
                    <option value="200-1000">200 - 1,000 employees</option>
                    <option value="1000+">1,000+ Enterprise</option>
                  </select>
                </label>
              </div>

              <label className="flex flex-col gap-1 text-xs font-medium text-text">
                Primary Module Interest
                <select
                  value={formData.primaryInterest}
                  onChange={(e) => setFormData({ ...formData, primaryInterest: e.target.value })}
                  className="rounded-lg border border-border bg-surface px-3 py-2 text-sm focus:border-primary focus:outline-none"
                >
                  <option value="Manufacturing ERP">Manufacturing & Production ERP</option>
                  <option value="Inventory & Warehouse">Inventory & Multi-Location Warehouse</option>
                  <option value="Finance & Accounting">Financial Backbone & GL/AR/AP</option>
                  <option value="CRM & Sales">CRM & Sales Pipeline</option>
                  <option value="Full OS Suite">Full Enterprise FusionOS Suite</option>
                </select>
              </label>

              <div className="pt-2">
                <Button type="submit" className="w-full py-2.5 text-sm font-semibold">
                  Request Live Demo
                </Button>
              </div>
            </form>
          </div>
        )}
      </div>
    </div>
  );
}
