import { useNavigate } from 'react-router-dom';
import { Layers, Package, Warehouse, ShoppingBag, TrendingUp, DollarSign, Factory, Users, ShieldCheck, Wrench, BarChart3, Bot, Grid, Cable, LogIn } from 'lucide-react';

export function ModulesPage() {
  const navigate = useNavigate();

  const modulesList = [
    { name: 'Core Platform', icon: <Layers className="text-primary" />, desc: 'Company management, branches, departments, RBAC roles, feature flags, and audit logs.' },
    { name: 'Inventory Control', icon: <Package className="text-emerald-500" />, desc: 'SKU master, UOM conversions, product variants, batch & serial tracking, and valuation.' },
    { name: 'Warehouse Logistics', icon: <Warehouse className="text-purple-500" />, desc: 'Zones, racks, shelves, bins, receiving put-away, cycle counting, and pick/pack dispatches.' },
    { name: 'Procurement & RFQ', icon: <ShoppingBag className="text-amber-500" />, desc: 'Supplier directory, RFQ quotations, PO approval workflows, and vendor ratings.' },
    { name: 'Sales & Invoicing', icon: <TrendingUp className="text-sky-500" />, desc: 'Customer CRM cards, quotations, sales orders, invoices, credit notes, and commissions.' },
    { name: 'Financial Backbone', icon: <DollarSign className="text-emerald-600" />, desc: 'General ledger, AR/AP aging, budgets, cost centers, fixed assets, and GST returns.' },
    { name: 'Manufacturing ERP', icon: <Factory className="text-rose-500" />, desc: 'Multi-level BOM, MRP calculation, work orders, routing templates, and machine planning.' },
    { name: 'CRM & Deals', icon: <Users className="text-indigo-500" />, desc: 'Lead capturing, sales deal stage pipeline, customer logs, and meeting scheduling.' },
    { name: 'HRMS & Payroll', icon: <Users className="text-teal-500" />, desc: 'Employee master, attendance tracking, leave requests, payroll runs, and offboarding.' },
    { name: 'Quality Management', icon: <ShieldCheck className="text-rose-600" />, desc: 'Inspections, Non-Conformance Reports (NCR), CAPA actions, and calibration.' },
    { name: 'Maintenance & Assets', icon: <Wrench className="text-amber-600" />, desc: 'Equipment master, breakdown work orders, preventive schedules, and downtime metrics.' },
    { name: 'Business Intelligence', icon: <BarChart3 className="text-blue-600" />, desc: 'KPI definition engine, automated snapshot reports, and analytical dashboards.' },
    { name: 'AI Platform', icon: <Bot className="text-purple-600" />, desc: 'Fusion Copilot assistant, demand forecasting, invoice OCR, and natural language search.' },
    { name: 'Marketplace', icon: <Grid className="text-pink-500" />, desc: 'Plugin app listings, installations, custom themes, and extensions hub.' },
    { name: 'Integration Hub', icon: <Cable className="text-cyan-500" />, desc: 'SAP, Shopify, Tally, Amazon, WhatsApp connectors, sync logs, and Webhook API keys.' },
  ];

  return (
    <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-12 space-y-16">
      <div className="text-center max-w-3xl mx-auto space-y-4">
        <span className="rounded-full bg-purple-500/10 px-3.5 py-1 text-xs font-bold text-purple-600 dark:text-purple-400 uppercase tracking-wider">
          Complete Business Coverage
        </span>
        <h1 className="text-4xl font-extrabold tracking-tight text-text sm:text-5xl">
          Explore the 15 Core Modules of FusionOS
        </h1>
        <p className="text-base text-text-muted">
          Every module is built with strict type safety, CQRS handlers, and RBAC authorization out of the box.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
        {modulesList.map((m, idx) => (
          <div key={idx} className="rounded-2xl border border-border bg-surface p-6 shadow-2xs space-y-3 hover:border-primary/40 transition-all">
            <div className="rounded-xl bg-surface-muted p-3 w-fit">{m.icon}</div>
            <h3 className="text-lg font-bold text-text">{m.name}</h3>
            <p className="text-xs text-text-muted leading-relaxed">{m.desc}</p>
            <button onClick={() => navigate('/login')} className="text-xs font-semibold text-primary flex items-center gap-1 pt-2">
              <LogIn size={14} /> Open in FusionOS
            </button>
          </div>
        ))}
      </div>
    </div>
  );
}
