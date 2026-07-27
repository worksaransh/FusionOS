import { CheckCircle2 } from 'lucide-react';

export function IntegrationsPage() {

  const connectors = [
    { name: 'SAP ERP', type: 'Enterprise ERP Sync', desc: 'Real-time bi-directional synchronization of purchase orders, stock ledger, and general ledger accounts.' },
    { name: 'Shopify / WooCommerce', type: 'E-Commerce Marketplace', desc: 'Automatic order ingestion, stock reservation updates, and customer address sync.' },
    { name: 'Tally Prime & Busy', type: 'Accounting Software', desc: 'One-click export and reconciliation for Indian statutory GST compliance and vouchers.' },
    { name: 'Amazon & Flipkart', type: 'Omni-Channel Retail', desc: 'Centralized multi-channel dispatch execution, return tracking, and seller scorecard.' },
    { name: 'WhatsApp & Email Gateway', type: 'Communication Hub', desc: 'Send automated invoice PDFs, order status alerts, and payment reminders via WhatsApp API.' },
    { name: 'Razorpay & Stripe', type: 'Payment Gateways', desc: 'Instant webhook updates for AR invoice settlement and payment link generation.' },
  ];

  return (
    <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-12 space-y-16">
      <div className="text-center max-w-3xl mx-auto space-y-4">
        <span className="rounded-full bg-cyan-500/10 px-3.5 py-1 text-xs font-bold text-cyan-600 dark:text-cyan-400 uppercase tracking-wider">
          Seamless Ecosystem Connections
        </span>
        <h1 className="text-4xl font-extrabold tracking-tight text-text sm:text-5xl">
          FusionOS Integration Hub
        </h1>
        <p className="text-base text-text-muted">
          Connect your existing ERP, e-commerce, banking, and messaging platforms with zero custom code.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
        {connectors.map((c, idx) => (
          <div key={idx} className="rounded-2xl border border-border bg-surface p-6 space-y-3 shadow-2xs hover:border-primary/40 transition-all">
            <div className="flex items-center justify-between">
              <span className="rounded-full bg-surface-muted px-2.5 py-1 text-[10px] font-bold text-primary uppercase">{c.type}</span>
              <CheckCircle2 size={16} className="text-emerald-500" />
            </div>
            <h3 className="text-lg font-bold text-text">{c.name}</h3>
            <p className="text-xs text-text-muted leading-relaxed">{c.desc}</p>
          </div>
        ))}
      </div>
    </div>
  );
}
