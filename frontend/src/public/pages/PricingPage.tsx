import { useState } from 'react';
import { useOutletContext } from 'react-router-dom';
import { Check } from 'lucide-react';

interface PublicOutletContext {
  openDemoModal: () => void;
}

export function PricingPage() {
  const { openDemoModal } = useOutletContext<PublicOutletContext>();
  const [isAnnual, setIsAnnual] = useState(true);

  const tiers = [
    {
      name: 'Starter',
      priceMonthly: '$499',
      priceAnnual: '$399',
      desc: 'Essential trading & inventory ERP for growing companies.',
      features: ['Up to 10 Staff Users', 'Core, Inventory & Sales Modules', 'Standard Financial Ledger', 'Community Support'],
      cta: 'Start Trial',
      highlighted: false,
    },
    {
      name: 'Growth',
      priceMonthly: '$1,299',
      priceAnnual: '$999',
      desc: 'Complete trading, warehouse, and finance operating system.',
      features: ['Up to 50 Staff Users', 'Inventory + Multi-Warehouse', 'Full General Ledger, AP/AR & GST', 'Standard Fusion AI Copilot', 'Priority Email Support'],
      cta: 'Get Started',
      highlighted: true,
    },
    {
      name: 'Enterprise',
      priceMonthly: 'Custom',
      priceAnnual: 'Custom',
      desc: 'Full 15-module ERP for large manufacturers & distributors.',
      features: ['Unlimited Staff Users', 'All 15 Business Modules (BOM, MRP)', 'Dedicated Fusion AI Copilot & OCR', 'Custom SAP/Shopify/Tally Connectors', '24/7 SLA Dedicated Architect'],
      cta: 'Contact Sales',
      highlighted: false,
    },
  ];

  return (
    <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-12 space-y-16">
      {/* Header */}
      <div className="text-center max-w-3xl mx-auto space-y-4">
        <span className="rounded-full bg-primary/10 px-3.5 py-1 text-xs font-bold text-primary uppercase tracking-wider">
          Transparent Enterprise Pricing
        </span>
        <h1 className="text-4xl font-extrabold tracking-tight text-text sm:text-5xl">
          Simple, Predictable Plans for Every Stage
        </h1>
        <p className="text-base text-text-muted">
          Zero hidden user add-on fees or expensive connector maintenance.
        </p>

        {/* Billing Toggle */}
        <div className="pt-4 flex items-center justify-center gap-3">
          <span className={`text-xs font-semibold ${!isAnnual ? 'text-text' : 'text-text-muted'}`}>Monthly</span>
          <button
            onClick={() => setIsAnnual(!isAnnual)}
            className="relative h-6 w-11 rounded-full bg-primary p-0.5 transition-colors"
          >
            <span
              className={`block h-5 w-5 rounded-full bg-white transition-transform ${
                isAnnual ? 'translate-x-5' : 'translate-x-0'
              }`}
            />
          </button>
          <span className={`text-xs font-semibold ${isAnnual ? 'text-text' : 'text-text-muted'}`}>
            Annual Billing <span className="ml-1 rounded bg-emerald-500/10 px-2 py-0.5 text-[10px] font-bold text-emerald-500">Save 20%</span>
          </span>
        </div>
      </div>

      {/* Pricing Cards */}
      <div className="grid grid-cols-1 gap-8 md:grid-cols-3">
        {tiers.map((tier, idx) => (
          <div
            key={idx}
            className={`rounded-3xl border p-8 space-y-6 relative flex flex-col justify-between transition-all ${
              tier.highlighted
                ? 'border-primary bg-surface shadow-xl ring-2 ring-primary/20 scale-105'
                : 'border-border bg-surface shadow-xs'
            }`}
          >
            {tier.highlighted && (
              <span className="absolute -top-3.5 left-1/2 -translate-x-1/2 rounded-full bg-gradient-to-r from-primary to-purple-600 px-3 py-1 text-[10px] font-extrabold uppercase tracking-wider text-white shadow-md">
                Most Popular
              </span>
            )}
            <div className="space-y-4">
              <h3 className="text-xl font-bold text-text">{tier.name}</h3>
              <p className="text-xs text-text-muted">{tier.desc}</p>
              <div className="pt-2">
                <span className="text-4xl font-extrabold tracking-tight text-text">
                  {isAnnual ? tier.priceAnnual : tier.priceMonthly}
                </span>
                {tier.priceMonthly !== 'Custom' && <span className="text-xs text-text-muted font-medium"> / month</span>}
              </div>
              <ul className="space-y-3 text-xs text-text border-t border-border pt-6">
                {tier.features.map((f, fi) => (
                  <li key={fi} className="flex items-center gap-2">
                    <Check size={16} className="text-emerald-500 shrink-0" />
                    <span>{f}</span>
                  </li>
                ))}
              </ul>
            </div>

            <div className="pt-6">
              <button
                onClick={openDemoModal}
                className={`w-full py-3 rounded-xl text-xs font-bold shadow-md transition-all ${
                  tier.highlighted
                    ? 'bg-primary text-white hover:bg-primary-hover'
                    : 'border border-border bg-surface text-text hover:bg-surface-muted'
                }`}
              >
                {tier.cta}
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
