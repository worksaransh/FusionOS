import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import {
  Sun,
  Moon,
  ChevronDown,
  Menu,
  X,
  LogIn,
  Sparkles,
  Factory,
  Package,
  DollarSign,
  Users,
  Bot,
  Zap,
  Building2,
} from 'lucide-react';
import { useTheme } from '../../shared/theme/ThemeProvider';

interface PublicHeaderProps {
  onOpenDemo: () => void;
}

export function PublicHeader({ onOpenDemo }: PublicHeaderProps) {
  const { theme, toggleTheme } = useTheme();
  const navigate = useNavigate();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [activeMegaMenu, setActiveMegaMenu] = useState<string | null>(null);

  return (
    <header className="sticky top-0 z-40 w-full border-b border-border/80 bg-surface/90 backdrop-blur-md transition-all">
      <div className="mx-auto flex max-w-7xl items-center justify-between px-4 py-3 sm:px-6 lg:px-8">
        {/* Brand Logo */}
        <Link to="/" className="flex items-center gap-2.5 group">
          <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-gradient-to-br from-primary via-purple-600 to-indigo-700 text-white font-black text-lg shadow-md group-hover:scale-105 transition-transform">
            F
          </div>
          <div className="flex flex-col">
            <span className="text-lg font-bold tracking-tight text-text">FusionOS</span>
            <span className="text-[10px] font-semibold text-primary uppercase tracking-widest -mt-1">
              Business OS
            </span>
          </div>
        </Link>

        {/* Desktop Navigation with Mega Dropdowns */}
        <nav className="hidden lg:flex items-center gap-1 text-sm font-medium text-text-muted">
          <Link
            to="/platform"
            className="px-3 py-2 rounded-lg hover:text-text hover:bg-surface-muted transition-colors"
          >
            Platform
          </Link>

          {/* Solutions Mega Menu */}
          <div
            className="relative"
            onMouseEnter={() => setActiveMegaMenu('solutions')}
            onMouseLeave={() => setActiveMegaMenu(null)}
          >
            <button className="flex items-center gap-1 px-3 py-2 rounded-lg hover:text-text hover:bg-surface-muted transition-colors">
              Solutions <ChevronDown size={14} />
            </button>
            {activeMegaMenu === 'solutions' && (
              <div className="absolute left-0 top-full mt-1 w-80 rounded-2xl border border-border bg-surface p-4 shadow-2xl animate-in fade-in slide-in-from-top-2">
                <div className="grid gap-2">
                  <Link
                    to="/solutions"
                    className="flex items-start gap-3 rounded-xl p-2.5 hover:bg-surface-muted transition-colors"
                  >
                    <Factory className="mt-0.5 text-primary" size={18} />
                    <div>
                      <p className="font-semibold text-text text-xs">Manufacturing ERP</p>
                      <p className="text-[11px] text-text-muted">BOM, MRP, Work Orders & Machine Planning</p>
                    </div>
                  </Link>
                  <Link
                    to="/solutions"
                    className="flex items-start gap-3 rounded-xl p-2.5 hover:bg-surface-muted transition-colors"
                  >
                    <Package className="mt-0.5 text-emerald-500" size={18} />
                    <div>
                      <p className="font-semibold text-text text-xs">Trading & Distribution</p>
                      <p className="text-[11px] text-text-muted">Multi-warehouse inventory & fulfillment</p>
                    </div>
                  </Link>
                  <Link
                    to="/solutions"
                    className="flex items-start gap-3 rounded-xl p-2.5 hover:bg-surface-muted transition-colors"
                  >
                    <DollarSign className="mt-0.5 text-amber-500" size={18} />
                    <div>
                      <p className="font-semibold text-text text-xs">Financial Backbone</p>
                      <p className="text-[11px] text-text-muted">GL, AR, AP, GST & Financial Statements</p>
                    </div>
                  </Link>
                </div>
              </div>
            )}
          </div>

          {/* Modules Mega Menu */}
          <div
            className="relative"
            onMouseEnter={() => setActiveMegaMenu('modules')}
            onMouseLeave={() => setActiveMegaMenu(null)}
          >
            <button className="flex items-center gap-1 px-3 py-2 rounded-lg hover:text-text hover:bg-surface-muted transition-colors">
              Modules <ChevronDown size={14} />
            </button>
            {activeMegaMenu === 'modules' && (
              <div className="absolute left-1/2 -translate-x-1/2 top-full mt-1 w-[540px] rounded-2xl border border-border bg-surface p-5 shadow-2xl grid grid-cols-2 gap-3">
                <Link to="/modules" className="flex items-start gap-2.5 rounded-xl p-2 hover:bg-surface-muted">
                  <Package className="text-primary mt-0.5" size={16} />
                  <div>
                    <p className="font-semibold text-text text-xs">Inventory & Stock</p>
                    <p className="text-[10px] text-text-muted">SKU, UOM, Batches & Serials</p>
                  </div>
                </Link>
                <Link to="/modules" className="flex items-start gap-2.5 rounded-xl p-2 hover:bg-surface-muted">
                  <Building2 className="text-purple-500 mt-0.5" size={16} />
                  <div>
                    <p className="font-semibold text-text text-xs">Warehouse Logistics</p>
                    <p className="text-[10px] text-text-muted">Bins, Put Away, Pick & Pack</p>
                  </div>
                </Link>
                <Link to="/modules" className="flex items-start gap-2.5 rounded-xl p-2 hover:bg-surface-muted">
                  <Factory className="text-amber-500 mt-0.5" size={16} />
                  <div>
                    <p className="font-semibold text-text text-xs">Manufacturing & BOM</p>
                    <p className="text-[10px] text-text-muted">Routings, Work Centers & Yield</p>
                  </div>
                </Link>
                <Link to="/modules" className="flex items-start gap-2.5 rounded-xl p-2 hover:bg-surface-muted">
                  <DollarSign className="text-emerald-500 mt-0.5" size={16} />
                  <div>
                    <p className="font-semibold text-text text-xs">Finance & Accounting</p>
                    <p className="text-[10px] text-text-muted">Accounts, Budgets & Tax/GST</p>
                  </div>
                </Link>
                <Link to="/modules" className="flex items-start gap-2.5 rounded-xl p-2 hover:bg-surface-muted">
                  <Users className="text-sky-500 mt-0.5" size={16} />
                  <div>
                    <p className="font-semibold text-text text-xs">CRM & HRMS</p>
                    <p className="text-[10px] text-text-muted">Leads, Deals, Payroll & Leave</p>
                  </div>
                </Link>
                <Link to="/ai" className="flex items-start gap-2.5 rounded-xl p-2 hover:bg-purple-500/10">
                  <Bot className="text-purple-600 mt-0.5" size={16} />
                  <div>
                    <p className="font-semibold text-purple-600 dark:text-purple-400 text-xs">Fusion AI Platform</p>
                    <p className="text-[10px] text-purple-500/80">Copilot, OCR & Forecasting</p>
                  </div>
                </Link>
              </div>
            )}
          </div>

          <Link to="/ai" className="px-3 py-2 rounded-lg text-purple-600 dark:text-purple-400 font-semibold hover:bg-purple-500/10 transition-colors flex items-center gap-1.5">
            <Sparkles size={15} /> AI Features
          </Link>

          <Link to="/pricing" className="px-3 py-2 rounded-lg hover:text-text hover:bg-surface-muted transition-colors">
            Pricing
          </Link>

          <Link to="/security" className="px-3 py-2 rounded-lg hover:text-text hover:bg-surface-muted transition-colors">
            Security
          </Link>

          <Link to="/resources" className="px-3 py-2 rounded-lg hover:text-text hover:bg-surface-muted transition-colors">
            Resources
          </Link>
        </nav>

        {/* Right Header Controls */}
        <div className="flex items-center gap-3">
          {/* Theme Toggle */}
          <button
            onClick={toggleTheme}
            aria-label="Toggle theme"
            className="rounded-xl border border-border p-2 text-text-muted hover:bg-surface-muted hover:text-text transition-colors"
          >
            {theme === 'dark' ? <Sun size={18} /> : <Moon size={18} />}
          </button>

          {/* Persistent LOGIN Button */}
          <button
            onClick={() => navigate('/login')}
            className="flex items-center gap-1.5 rounded-xl border border-border bg-surface px-3.5 py-2 text-xs font-semibold text-text shadow-2xs hover:border-primary/50 hover:bg-surface-muted transition-all"
          >
            <LogIn size={15} className="text-primary" />
            <span>Login</span>
          </button>

          {/* Book Demo Button */}
          <button
            onClick={onOpenDemo}
            className="hidden sm:flex items-center gap-1.5 rounded-xl bg-gradient-to-r from-primary to-purple-600 px-4 py-2 text-xs font-bold text-white shadow-md hover:opacity-95 hover:shadow-lg transition-all"
          >
            <Zap size={14} />
            <span>Book Demo</span>
          </button>

          {/* Mobile Hamburger Toggle */}
          <button
            onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
            className="rounded-xl border border-border p-2 text-text-muted hover:bg-surface-muted lg:hidden"
          >
            {isMobileMenuOpen ? <X size={20} /> : <Menu size={20} />}
          </button>
        </div>
      </div>

      {/* Mobile Drawer Menu */}
      {isMobileMenuOpen && (
        <div className="border-b border-border bg-surface p-4 lg:hidden space-y-3 animate-in slide-in-from-top-2">
          <div className="flex flex-col gap-2 font-medium text-sm">
            <Link to="/platform" onClick={() => setIsMobileMenuOpen(false)} className="px-3 py-2 rounded-lg hover:bg-surface-muted">Platform</Link>
            <Link to="/solutions" onClick={() => setIsMobileMenuOpen(false)} className="px-3 py-2 rounded-lg hover:bg-surface-muted">Solutions</Link>
            <Link to="/modules" onClick={() => setIsMobileMenuOpen(false)} className="px-3 py-2 rounded-lg hover:bg-surface-muted">Modules</Link>
            <Link to="/ai" onClick={() => setIsMobileMenuOpen(false)} className="px-3 py-2 rounded-lg text-purple-600 font-semibold hover:bg-purple-500/10">AI Features</Link>
            <Link to="/pricing" onClick={() => setIsMobileMenuOpen(false)} className="px-3 py-2 rounded-lg hover:bg-surface-muted">Pricing</Link>
            <Link to="/security" onClick={() => setIsMobileMenuOpen(false)} className="px-3 py-2 rounded-lg hover:bg-surface-muted">Security</Link>
            <Link to="/resources" onClick={() => setIsMobileMenuOpen(false)} className="px-3 py-2 rounded-lg hover:bg-surface-muted">Resources</Link>
          </div>
          <div className="pt-3 border-t border-border flex flex-col gap-2">
            <button
              onClick={() => { setIsMobileMenuOpen(false); navigate('/login'); }}
              className="w-full justify-center flex items-center gap-2 rounded-xl border border-border py-2.5 text-sm font-semibold text-text"
            >
              <LogIn size={16} /> Login to FusionOS App
            </button>
            <button
              onClick={() => { setIsMobileMenuOpen(false); onOpenDemo(); }}
              className="w-full justify-center flex items-center gap-2 rounded-xl bg-primary py-2.5 text-sm font-bold text-white"
            >
              <Zap size={16} /> Book Enterprise Demo
            </button>
          </div>
        </div>
      )}
    </header>
  );
}
