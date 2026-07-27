import { Link } from 'react-router-dom';
import { Sparkles, Shield, CheckCircle2, ArrowRight } from 'lucide-react';

export function PublicFooter() {
  return (
    <footer className="border-t border-border bg-surface text-text text-sm pt-16 pb-12">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 space-y-12">
        {/* Upper Newsletter & Mission */}
        <div className="grid grid-cols-1 gap-8 lg:grid-cols-12 lg:items-center border-b border-border pb-12">
          <div className="lg:col-span-7">
            <div className="flex items-center gap-2 text-primary font-bold text-xs uppercase tracking-wider mb-2">
              <Sparkles size={16} /> Enterprise ERP Evolution
            </div>
            <h3 className="text-2xl font-bold tracking-tight text-text sm:text-3xl">
              One Platform. Every Business Operation.
            </h3>
            <p className="mt-2 text-sm text-text-muted max-w-xl">
              Join leading manufacturers, trading firms, and enterprises unifying their core operating systems with FusionOS.
            </p>
          </div>
          <div className="lg:col-span-5 flex flex-col sm:flex-row gap-2">
            <input
              type="email"
              placeholder="Enter your work email"
              className="flex-1 rounded-xl border border-border bg-surface-muted px-4 py-2.5 text-sm focus:border-primary focus:outline-none"
            />
            <button className="rounded-xl bg-primary px-5 py-2.5 text-sm font-semibold text-primary-foreground shadow-md hover:bg-primary-hover transition-colors flex items-center justify-center gap-1.5 whitespace-nowrap">
              Get Updates <ArrowRight size={16} />
            </button>
          </div>
        </div>

        {/* Footer Navigation Columns */}
        <div className="grid grid-cols-2 gap-8 md:grid-cols-5 lg:gap-12">
          <div className="col-span-2 space-y-4">
            <div className="flex items-center gap-2.5">
              <div className="flex h-8 w-8 items-center justify-center rounded-xl bg-gradient-to-br from-primary to-purple-600 text-white font-bold">
                F
              </div>
              <span className="text-lg font-bold text-text">FusionOS</span>
            </div>
            <p className="text-xs text-text-muted leading-relaxed max-w-sm">
              The AI-native Business Operating System empowering modern companies to manage Manufacturing, Inventory, Finance, CRM, and HR from a single high-performance engine.
            </p>
            <div className="flex items-center gap-3 text-xs text-text-muted">
              <span className="inline-flex items-center gap-1 text-emerald-500 font-medium">
                <CheckCircle2 size={14} /> Systems Operational
              </span>
              <span>•</span>
              <span className="inline-flex items-center gap-1 text-text-muted">
                <Shield size={14} /> SOC2 Type II Certified
              </span>
            </div>
          </div>

          <div>
            <p className="font-semibold text-xs text-text uppercase tracking-wider mb-4">Product & Platform</p>
            <ul className="space-y-2.5 text-xs text-text-muted">
              <li><Link to="/platform" className="hover:text-text transition-colors">Platform Architecture</Link></li>
              <li><Link to="/ai" className="hover:text-text transition-colors">Fusion AI Copilot</Link></li>
              <li><Link to="/modules" className="hover:text-text transition-colors">Core Modules</Link></li>
              <li><Link to="/pricing" className="hover:text-text transition-colors">Enterprise Pricing</Link></li>
              <li><Link to="/integrations" className="hover:text-text transition-colors">Integration Hub</Link></li>
            </ul>
          </div>

          <div>
            <p className="font-semibold text-xs text-text uppercase tracking-wider mb-4">Solutions</p>
            <ul className="space-y-2.5 text-xs text-text-muted">
              <li><Link to="/solutions" className="hover:text-text transition-colors">Manufacturing ERP</Link></li>
              <li><Link to="/solutions" className="hover:text-text transition-colors">Trading & Distribution</Link></li>
              <li><Link to="/solutions" className="hover:text-text transition-colors">Warehouse Logistics</Link></li>
              <li><Link to="/solutions" className="hover:text-text transition-colors">Financial Backbone</Link></li>
              <li><Link to="/solutions" className="hover:text-text transition-colors">CRM & Sales Pipeline</Link></li>
            </ul>
          </div>

          <div>
            <p className="font-semibold text-xs text-text uppercase tracking-wider mb-4">Resources & Company</p>
            <ul className="space-y-2.5 text-xs text-text-muted">
              <li><Link to="/resources" className="hover:text-text transition-colors">Documentation & APIs</Link></li>
              <li><Link to="/security" className="hover:text-text transition-colors">Security Center</Link></li>
              <li><Link to="/careers" className="hover:text-text transition-colors">Careers</Link></li>
              <li><Link to="/contact" className="hover:text-text transition-colors">Contact Sales</Link></li>
              <li><Link to="/login" className="hover:text-text font-semibold text-primary transition-colors">Staff Login</Link></li>
            </ul>
          </div>
        </div>

        {/* Bottom Legal Copyright */}
        <div className="border-t border-border pt-8 flex flex-col sm:flex-row items-center justify-between gap-4 text-xs text-text-muted">
          <p>© {new Date().getFullYear()} FusionOS Inc. All rights reserved.</p>
          <div className="flex items-center gap-6">
            <Link to="/security" className="hover:text-text">Privacy Policy</Link>
            <Link to="/security" className="hover:text-text">Terms of Service</Link>
            <Link to="/security" className="hover:text-text">Cookie Preferences</Link>
            <Link to="/security" className="hover:text-text">Security Trust Center</Link>
          </div>
        </div>
      </div>
    </footer>
  );
}
