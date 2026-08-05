import { useNavigate, useOutletContext } from 'react-router-dom';
import {
  Sparkles,
  Zap,
  CheckCircle2,
  TrendingUp,
  Package,
  Factory,
  DollarSign,
  ShieldCheck,
  Bot,
  Layers,
  ShoppingBag,
  Users,
  BarChart3,
  LogIn,
  Download,
} from 'lucide-react';

interface PublicOutletContext {
  openDemoModal: () => void;
}

export function LandingHomePage() {
  const navigate = useNavigate();
  const { openDemoModal } = useOutletContext<PublicOutletContext>();

  return (
    <div className="space-y-24 pb-20">
      {/* Hero Section */}
      <section className="relative overflow-hidden pt-12 lg:pt-20">
        {/* Glow Gradients */}
        <div className="absolute top-1/4 left-1/2 -z-10 h-96 w-96 -translate-x-1/2 -translate-y-1/2 rounded-full bg-primary/20 blur-3xl" />
        <div className="absolute top-1/3 right-1/4 -z-10 h-80 w-80 rounded-full bg-purple-500/20 blur-3xl" />

        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 text-center">
          {/* Top Pill Badge */}
          <div className="inline-flex items-center gap-2 rounded-full border border-purple-500/30 bg-purple-500/10 px-4 py-1.5 text-xs font-semibold text-purple-600 dark:text-purple-300 shadow-xs mb-6 animate-pulse">
            <Sparkles size={14} /> The Next-Generation Enterprise ERP & Business Operating System
          </div>

          {/* Main Headline */}
          <h1 className="text-4xl font-extrabold tracking-tight text-text sm:text-6xl lg:text-7xl leading-tight">
            One Platform. <br className="hidden sm:inline" />
            <span className="bg-gradient-to-r from-primary via-purple-600 to-indigo-600 bg-clip-text text-transparent">
              Every Business Operation.
            </span>
          </h1>

          <p className="mt-6 max-w-3xl mx-auto text-base text-text-muted sm:text-xl leading-relaxed">
            Unify Manufacturing, Multi-Location Warehouse, Finance, Procurement, CRM, HRMS, and AI Analytics into a single high-performance operating system designed for modern enterprises.
          </p>

          {/* Call to Actions */}
          <div className="mt-8 flex flex-wrap items-center justify-center gap-4">
            <button
              onClick={openDemoModal}
              className="flex items-center gap-2 rounded-xl bg-gradient-to-r from-primary to-purple-600 px-6 py-3.5 text-sm font-bold text-white shadow-xl hover:opacity-95 hover:scale-105 transition-all"
            >
              <Zap size={18} /> Book Live Demo
            </button>

            <button
              onClick={() => navigate('/login')}
              className="flex items-center gap-2 rounded-xl border border-border bg-surface px-6 py-3.5 text-sm font-semibold text-text shadow-sm hover:border-primary/50 hover:bg-surface-muted transition-all"
            >
              <LogIn size={18} className="text-primary" /> Login to FusionOS
            </button>

            <a
              href="/downloads/FusionOS-Setup.exe"
              download
              className="flex items-center gap-2 rounded-xl border border-border bg-surface px-6 py-3.5 text-sm font-semibold text-text shadow-sm hover:border-primary/50 hover:bg-surface-muted transition-all"
            >
              <Download size={18} className="text-primary" /> Download for Windows
            </a>
          </div>
          <p className="mt-3 text-xs text-text-muted">
            Installs a local FusionOS server on this PC — needs an existing PostgreSQL database.
          </p>

          {/* Trust Badges */}
          <div className="mt-10 flex flex-wrap items-center justify-center gap-6 text-xs text-text-muted">
            <span className="inline-flex items-center gap-1.5 font-medium">
              <CheckCircle2 size={16} className="text-emerald-500" /> SOC2 Type II Certified
            </span>
            <span className="inline-flex items-center gap-1.5 font-medium">
              <CheckCircle2 size={16} className="text-emerald-500" /> Multi-Tenant Isolation
            </span>
            <span className="inline-flex items-center gap-1.5 font-medium">
              <CheckCircle2 size={16} className="text-emerald-500" /> Real-time CQRS Engine
            </span>
          </div>

          {/* Hero Interactive App Mockup Preview */}
          <div className="mt-14 relative mx-auto max-w-5xl rounded-2xl border border-border bg-surface shadow-2xl p-2 sm:p-4 overflow-hidden">
            <div className="flex items-center justify-between border-b border-border pb-3 px-3">
              <div className="flex items-center gap-2">
                <span className="h-3 w-3 rounded-full bg-rose-500/80" />
                <span className="h-3 w-3 rounded-full bg-amber-500/80" />
                <span className="h-3 w-3 rounded-full bg-emerald-500/80" />
                <span className="ml-2 text-xs font-mono text-text-muted">app.fusionos.io/dashboard</span>
              </div>
              <div className="flex items-center gap-2 text-xs text-text-muted">
                <span className="rounded bg-emerald-500/10 px-2 py-0.5 font-semibold text-emerald-500">Live Workspace</span>
              </div>
            </div>

            {/* Dashboard Mock Content */}
            <div className="p-4 sm:p-6 bg-surface-muted/60 text-left space-y-6">
              <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
                <div className="rounded-xl border border-border bg-surface p-4 shadow-2xs">
                  <p className="text-[11px] font-semibold text-text-muted uppercase">Revenue YoY</p>
                  <p className="text-xl font-bold text-text mt-1">$12.4M</p>
                  <span className="text-[10px] font-semibold text-emerald-500">+24.5% vs last quarter</span>
                </div>
                <div className="rounded-xl border border-border bg-surface p-4 shadow-2xs">
                  <p className="text-[11px] font-semibold text-text-muted uppercase">Active Work Orders</p>
                  <p className="text-xl font-bold text-text mt-1">142 Orders</p>
                  <span className="text-[10px] font-semibold text-primary">98.2% Yield Rate</span>
                </div>
                <div className="rounded-xl border border-border bg-surface p-4 shadow-2xs">
                  <p className="text-[11px] font-semibold text-text-muted uppercase">Stock Valuation</p>
                  <p className="text-xl font-bold text-text mt-1">$4.8M</p>
                  <span className="text-[10px] font-semibold text-amber-500">3 Low-Stock Alerts</span>
                </div>
                <div className="rounded-xl border border-border bg-surface p-4 shadow-2xs">
                  <p className="text-[11px] font-semibold text-text-muted uppercase">Fusion AI Suggestions</p>
                  <p className="text-xl font-bold text-purple-600 dark:text-purple-400 mt-1">12 Actions</p>
                  <span className="text-[10px] font-semibold text-purple-500">Automated PO Reorder</span>
                </div>
              </div>

              {/* Sample Operations Table */}
              <div className="rounded-xl border border-border bg-surface p-4 shadow-2xs">
                <div className="flex items-center justify-between mb-3 border-b border-border pb-2">
                  <h4 className="text-xs font-bold text-text uppercase tracking-wider">Live Manufacturing & Order Execution</h4>
                  <span className="text-xs text-text-muted font-mono">15 Active Modules Connected</span>
                </div>
                <div className="space-y-2 text-xs">
                  <div className="flex items-center justify-between p-2 rounded bg-surface-muted">
                    <span className="font-mono font-semibold text-text">WO-9082 — Electric Motor Assembly</span>
                    <span className="rounded bg-emerald-500/10 px-2 py-0.5 text-emerald-600 font-medium">In Production</span>
                    <span className="font-semibold text-text">$45,200</span>
                  </div>
                  <div className="flex items-center justify-between p-2 rounded bg-surface-muted">
                    <span className="font-mono font-semibold text-text">SO-4410 — Acme Logistics Order</span>
                    <span className="rounded bg-sky-500/10 px-2 py-0.5 text-sky-600 font-medium">Dispatched</span>
                    <span className="font-semibold text-text">$128,900</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Customer Logo Section */}
      <section className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 text-center border-y border-border py-10">
        <p className="text-xs font-semibold uppercase tracking-widest text-text-muted mb-6">
          Powering discrete manufacturing, high-velocity trading, and global enterprise operations
        </p>
        <div className="flex flex-wrap items-center justify-center gap-8 opacity-70 grayscale hover:grayscale-0 transition-all font-bold text-lg text-text-muted">
          <span>APEX MANUFACTURING</span>
          <span>•</span>
          <span>GLOBAL LOGISTICS CORP</span>
          <span>•</span>
          <span>NEXUS INDUSTRIAL</span>
          <span>•</span>
          <span>VANGUARD DISTRIBUTORS</span>
          <span>•</span>
          <span>PRECISION LABS</span>
        </div>
      </section>

      {/* Legacy ERP vs FusionOS Comparison */}
      <section className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="text-center max-w-3xl mx-auto mb-16">
          <h2 className="text-3xl font-bold tracking-tight text-text sm:text-4xl">
            Why Leading Enterprises Switch to FusionOS
          </h2>
          <p className="mt-3 text-base text-text-muted">
            Legacy ERP systems require 10+ disconnected tools, slow data silos, and manual spreadsheets. FusionOS brings every department under one real-time cloud backbone.
          </p>
        </div>

        <div className="grid grid-cols-1 gap-8 md:grid-cols-2">
          {/* Legacy ERP Box */}
          <div className="rounded-2xl border border-rose-500/20 bg-rose-500/5 p-8 space-y-4">
            <span className="rounded-full bg-rose-500/10 px-3 py-1 text-xs font-bold text-rose-500">The Old Way — Legacy ERPs</span>
            <h3 className="text-xl font-bold text-text">Fragmented & Fragile Operations</h3>
            <ul className="space-y-3 text-xs text-text-muted">
              <li className="flex items-start gap-2">
                <span className="text-rose-500 font-bold">✕</span> Separate software for Inventory, Accounting, Production, and CRM.
              </li>
              <li className="flex items-start gap-2">
                <span className="text-rose-500 font-bold">✕</span> Slow month-end financial closing due to manual reconciliation.
              </li>
              <li className="flex items-start gap-2">
                <span className="text-rose-500 font-bold">✕</span> No real-time visibility into machine downtime or stockouts.
              </li>
              <li className="flex items-start gap-2">
                <span className="text-rose-500 font-bold">✕</span> High licensing costs with custom integration maintenance code.
              </li>
            </ul>
          </div>

          {/* FusionOS Box */}
          <div className="rounded-2xl border border-primary/30 bg-primary/5 p-8 space-y-4 relative overflow-hidden">
            <div className="absolute right-0 top-0 h-32 w-32 bg-primary/10 rounded-full blur-2xl" />
            <span className="rounded-full bg-primary/10 px-3 py-1 text-xs font-bold text-primary">The FusionOS Advantage</span>
            <h3 className="text-xl font-bold text-text">Unified Real-Time Business OS</h3>
            <ul className="space-y-3 text-xs text-text">
              <li className="flex items-start gap-2">
                <CheckCircle2 size={16} className="text-emerald-500 shrink-0 mt-0.5" />
                <span>15 fully integrated modules sharing a single CQRS database backbone.</span>
              </li>
              <li className="flex items-start gap-2">
                <CheckCircle2 size={16} className="text-emerald-500 shrink-0 mt-0.5" />
                <span>Instant automated financial statements & continuous real-time ledger auditing.</span>
              </li>
              <li className="flex items-start gap-2">
                <CheckCircle2 size={16} className="text-emerald-500 shrink-0 mt-0.5" />
                <span>AI Fusion Copilot forecasting demand, OCR invoices, and stock reorders.</span>
              </li>
              <li className="flex items-start gap-2">
                <CheckCircle2 size={16} className="text-emerald-500 shrink-0 mt-0.5" />
                <span>Predictable enterprise pricing with zero hidden integration connectors fee.</span>
              </li>
            </ul>
          </div>
        </div>
      </section>

      {/* AI Feature Showcase */}
      <section className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 border-y border-border py-16">
        <div className="grid grid-cols-1 gap-12 lg:grid-cols-12 lg:items-center">
          <div className="lg:col-span-5 space-y-6">
            <div className="inline-flex items-center gap-2 rounded-full bg-purple-500/10 px-3.5 py-1 text-xs font-bold text-purple-600 dark:text-purple-300">
              <Bot size={16} /> Embedded AI Engine
            </div>
            <h2 className="text-3xl font-bold tracking-tight text-text sm:text-4xl">
              Meet Fusion Copilot: <br /> Your AI Operations Assistant
            </h2>
            <p className="text-sm text-text-muted leading-relaxed">
              FusionOS incorporates generative AI and predictive machine learning directly into your daily workflows. Ask natural language questions, extract document data instantly, and receive proactive stock warnings.
            </p>
            <div className="space-y-3 text-xs">
              <div className="flex items-center gap-3 p-3 rounded-xl border border-border bg-surface">
                <Sparkles className="text-purple-600" size={18} />
                <div>
                  <p className="font-bold text-text">AI Demand & Stock Forecasting</p>
                  <p className="text-text-muted">Predicts reorder dates based on historical sales velocity.</p>
                </div>
              </div>
              <div className="flex items-center gap-3 p-3 rounded-xl border border-border bg-surface">
                <Zap className="text-amber-500" size={18} />
                <div>
                  <p className="font-bold text-text">Invoice & Document OCR</p>
                  <p className="text-text-muted">Converts PDF vendor bills and receipts straight into AP entries.</p>
                </div>
              </div>
            </div>
          </div>

          <div className="lg:col-span-7 rounded-2xl border border-purple-500/30 bg-surface p-6 shadow-2xl space-y-4">
            <div className="flex items-center gap-2 border-b border-border pb-3">
              <Bot className="text-purple-600" size={20} />
              <span className="font-bold text-sm text-text">Fusion Copilot Assistant</span>
            </div>
            <div className="space-y-3 text-xs">
              <div className="rounded-lg bg-surface-muted p-3 text-text-muted">
                &quot;Show me top 3 inventory items at risk of stockout next week.&quot;
              </div>
              <div className="rounded-lg bg-purple-500/10 border border-purple-500/20 p-3 text-purple-900 dark:text-purple-200">
                <p className="font-bold">Fusion AI Response:</p>
                <p className="mt-1">1. Steel Bearing SK-1002 (42 units left, reorder recommended in 2 days)</p>
                <p>2. Microcontroller MCU-32 (18 units left)</p>
                <p>3. Hydraulic Valve HV-80 (0 units left - 5 pending sales orders affected)</p>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* 15 Modules Matrix Grid */}
      <section className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="text-center max-w-3xl mx-auto mb-16">
          <h2 className="text-3xl font-bold tracking-tight text-text sm:text-4xl">
            15 Fully-Integrated Enterprise Modules
          </h2>
          <p className="mt-3 text-base text-text-muted">
            Everything your company needs to operate seamlessly in one unified platform.
          </p>
        </div>

        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {[
            { icon: <Factory className="text-primary" />, title: 'Manufacturing ERP', desc: 'BOM versions, routing templates, work orders, MRP, and scrap yield.' },
            { icon: <Package className="text-emerald-500" />, title: 'Inventory Control', desc: 'SKU management, multi-UOM conversions, batch & serial unit tracking.' },
            { icon: <Layers className="text-purple-500" />, title: 'Warehouse Logistics', desc: 'Zones, racks, shelves, bins, receiving put-away, and pick lists.' },
            { icon: <ShoppingBag className="text-amber-500" />, title: 'Procurement', desc: 'Supplier directory, RFQ management, PO approvals, and vendor ratings.' },
            { icon: <TrendingUp className="text-sky-500" />, title: 'Sales & Quotations', desc: 'Quotation builder, sales pipeline, invoices, dispatches, and credit notes.' },
            { icon: <DollarSign className="text-emerald-600" />, title: 'Finance Backbone', desc: 'General ledger, accounts payable/receivable, budgets, and GST/Tax.' },
            { icon: <Users className="text-indigo-500" />, title: 'CRM & Pipeline', desc: 'Lead capturing, deal stage tracking, customer contact logs, and meetings.' },
            { icon: <BarChart3 className="text-rose-500" />, title: 'HRMS & Payroll', desc: 'Employee records, attendance logging, leave requests, and payroll runs.' },
            { icon: <ShieldCheck className="text-teal-500" />, title: 'Quality Management', desc: 'Inspection logs, non-conformance reports (NCR), and CAPA tracking.' },
          ].map((m, idx) => (
            <div key={idx} className="rounded-2xl border border-border bg-surface p-6 shadow-2xs hover:border-primary/40 hover:shadow-md transition-all">
              <div className="mb-4 rounded-xl bg-surface-muted p-3 w-fit">{m.icon}</div>
              <h3 className="text-lg font-bold text-text">{m.title}</h3>
              <p className="mt-2 text-xs text-text-muted leading-relaxed">{m.desc}</p>
            </div>
          ))}
        </div>
      </section>

      {/* Final Conversion Call to Action */}
      <section className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="rounded-3xl bg-gradient-to-r from-primary via-purple-600 to-indigo-700 p-8 sm:p-14 text-white text-center shadow-2xl relative overflow-hidden">
          <h2 className="text-3xl font-extrabold sm:text-5xl tracking-tight">
            Ready to Modernize Your Enterprise Operations?
          </h2>
          <p className="mt-4 text-base sm:text-lg max-w-2xl mx-auto opacity-90">
            Schedule a personalized walkthrough with our solution architects or sign in directly to your active workspace.
          </p>
          <div className="mt-8 flex flex-wrap items-center justify-center gap-4">
            <button
              onClick={openDemoModal}
              className="rounded-xl bg-white px-6 py-3.5 text-sm font-bold text-primary shadow-xl hover:bg-slate-100 transition-all flex items-center gap-2"
            >
              <Zap size={18} /> Schedule Live Demo
            </button>
            <button
              onClick={() => navigate('/login')}
              className="rounded-xl border border-white/40 bg-white/10 backdrop-blur-md px-6 py-3.5 text-sm font-semibold text-white hover:bg-white/20 transition-all flex items-center gap-2"
            >
              <LogIn size={18} /> Sign In to FusionOS
            </button>
          </div>
        </div>
      </section>
    </div>
  );
}
