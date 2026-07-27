import { useEffect, useMemo, useState } from 'react';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import {
  Menu,
  Moon,
  Sun,
  Search,
  LogOut,
  X,
  Bell,
  CheckSquare,
  Sparkles,
  LayoutDashboard,
  Layers,
  Package,
  Warehouse as WarehouseIcon,
  ShoppingBag,
  TrendingUp,
  DollarSign,
  Factory,
  Users,
  UserCheck,
  ShieldCheck,
  Wrench,
  BarChart3,
  Bot,
  Grid,
  Cable,
  Building2,
} from 'lucide-react';
import { useQuery } from '@tanstack/react-query';
import { useTheme } from '../../shared/theme/ThemeProvider';
import { useActiveCompany } from '../../shared/company/useActiveCompany';
import { useAuthStore } from '../../shared/auth/authStore';
import { apiClient } from '../../shared/api/client';
import type { PagedResult } from '../../shared/api/types';
import { MODULES } from '../modules';
import { CommandPalette } from './CommandPalette';

function useUnreadNotificationCount(companyId: string) {
  const query = useQuery({
    queryKey: ['unread-notification-count', companyId],
    queryFn: () => apiClient.get<PagedResult<unknown>>(`/core/notifications?companyId=${companyId}&unreadOnly=true&page=1&pageSize=1`),
    enabled: Boolean(companyId),
    refetchInterval: 60_000,
  });
  return query.data?.totalCount ?? 0;
}

const moduleIcons: Record<string, React.ReactNode> = {
  core: <Layers size={16} />,
  inventory: <Package size={16} />,
  warehouse: <WarehouseIcon size={16} />,
  procurement: <ShoppingBag size={16} />,
  sales: <TrendingUp size={16} />,
  finance: <DollarSign size={16} />,
  manufacturing: <Factory size={16} />,
  crm: <Users size={16} />,
  hrms: <UserCheck size={16} />,
  quality: <ShieldCheck size={16} />,
  maintenance: <Wrench size={16} />,
  bi: <BarChart3 size={16} />,
  ai: <Bot size={16} />,
  marketplace: <Grid size={16} />,
  integration_hub: <Cable size={16} />,
};

export function AppShell() {
  const { theme, toggleTheme } = useTheme();
  const { companyId, setCompanyId } = useActiveCompany();
  const session = useAuthStore((s) => s.session);
  const clearSession = useAuthStore((s) => s.clearSession);
  const hasPermissionPrefix = useAuthStore((s) => s.hasPermissionPrefix);
  const navigate = useNavigate();
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);
  const [isPaletteOpen, setIsPaletteOpen] = useState(false);
  const unreadCount = useUnreadNotificationCount(companyId);

  const visibleModules = useMemo(
    () => MODULES.filter((module) => hasPermissionPrefix(module.name)),
    [hasPermissionPrefix],
  );

  useEffect(() => {
    function handleKeyDown(event: KeyboardEvent) {
      if ((event.metaKey || event.ctrlKey) && event.key.toLowerCase() === 'k') {
        event.preventDefault();
        setIsPaletteOpen(true);
      }
    }
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, []);

  const handleLogout = async () => {
    if (session?.refreshToken) {
      try {
        await apiClient.post('/core/auth/logout', { refreshToken: session.refreshToken });
      } catch {
        // Ignored
      }
    }
    clearSession();
    navigate('/login', { replace: true });
  };

  return (
    <div className="flex h-screen bg-surface-muted text-text font-sans antialiased">
      <a
        href="#main-content"
        className="sr-only focus:not-sr-only focus:absolute focus:left-4 focus:top-4 focus:z-50 focus:rounded-md focus:bg-primary focus:px-3 focus:py-2 focus:text-sm focus:text-primary-foreground"
      >
        Skip to main content
      </a>

      {isSidebarOpen && (
        <div
          className="fixed inset-0 z-20 bg-black/50 backdrop-blur-xs md:hidden"
          onClick={() => setIsSidebarOpen(false)}
          aria-hidden="true"
        />
      )}

      {/* Persistent Enterprise Sidebar */}
      <aside
        className={`fixed inset-y-0 left-0 z-30 w-64 shrink-0 overflow-y-auto border-r border-border bg-surface p-4 transition-transform duration-200 md:static md:translate-x-0 ${
          isSidebarOpen ? 'translate-x-0' : '-translate-x-full'
        }`}
      >
        {/* Brand Header */}
        <div className="mb-6 flex items-center justify-between px-2">
          <div className="flex items-center gap-2.5">
            <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-gradient-to-br from-primary to-purple-600 text-white font-bold text-base shadow-xs">
              F
            </div>
            <div>
              <span className="text-base font-bold tracking-tight text-text">FusionOS</span>
              <span className="ml-1.5 rounded bg-primary/10 px-1.5 py-0.5 text-[10px] font-semibold text-primary uppercase">
                Enterprise
              </span>
            </div>
          </div>
          <button
            aria-label="Close navigation"
            onClick={() => setIsSidebarOpen(false)}
            className="rounded-md p-1 text-text-muted hover:bg-surface-muted md:hidden"
          >
            <X size={18} />
          </button>
        </div>

        {/* Navigation Sections */}
        <nav className="flex flex-col gap-1 text-sm">
          <div className="mb-2">
            <p className="px-2 pb-1 text-[11px] font-semibold uppercase tracking-wider text-text-muted">Main</p>
            <NavLink
              to="/dashboard"
              onClick={() => setIsSidebarOpen(false)}
              className={({ isActive }) =>
                `flex items-center gap-2.5 rounded-lg px-2.5 py-2 transition-colors ${
                  isActive
                    ? 'bg-primary text-primary-foreground font-medium shadow-xs'
                    : 'text-text-muted hover:bg-surface-muted hover:text-text'
                }`
              }
            >
              <LayoutDashboard size={16} />
              <span>Executive Dashboard</span>
            </NavLink>

            <NavLink
              to="/core/approvals"
              onClick={() => setIsSidebarOpen(false)}
              className={({ isActive }) =>
                `flex items-center gap-2.5 rounded-lg px-2.5 py-2 transition-colors ${
                  isActive
                    ? 'bg-primary text-primary-foreground font-medium shadow-xs'
                    : 'text-text-muted hover:bg-surface-muted hover:text-text'
                }`
              }
            >
              <CheckSquare size={16} />
              <span>Pending Approvals</span>
            </NavLink>

            <NavLink
              to="/core/notifications"
              onClick={() => setIsSidebarOpen(false)}
              className={({ isActive }) =>
                `flex items-center gap-2.5 rounded-lg px-2.5 py-2 transition-colors ${
                  isActive
                    ? 'bg-primary text-primary-foreground font-medium shadow-xs'
                    : 'text-text-muted hover:bg-surface-muted hover:text-text'
                }`
              }
            >
              <Bell size={16} />
              <span>Notifications</span>
              {unreadCount > 0 && (
                <span className="ml-auto rounded-full bg-danger px-1.5 py-0.5 text-xs font-semibold text-white">
                  {unreadCount}
                </span>
              )}
            </NavLink>
          </div>

          <div>
            <p className="px-2 pb-1 pt-2 text-[11px] font-semibold uppercase tracking-wider text-text-muted">
              Business Modules
            </p>
            <div className="space-y-0.5">
              {visibleModules.map((module) => (
                <NavLink
                  key={module.name}
                  to={`/${module.name}`}
                  onClick={() => setIsSidebarOpen(false)}
                  className={({ isActive }) =>
                    `flex items-center gap-2.5 rounded-lg px-2.5 py-2 transition-colors ${
                      isActive
                        ? 'bg-primary text-primary-foreground font-medium shadow-xs'
                        : 'text-text-muted hover:bg-surface-muted hover:text-text'
                    }`
                  }
                >
                  <span className="shrink-0">{moduleIcons[module.name] || <Layers size={16} />}</span>
                  <span className="truncate">{module.label}</span>
                </NavLink>
              ))}
            </div>
          </div>
        </nav>
      </aside>

      {/* Main Workspace Frame */}
      <div className="flex flex-1 flex-col overflow-hidden">
        {/* Top Header Bar */}
        <header className="flex items-center justify-between gap-3 border-b border-border bg-surface px-4 py-2.5 sm:px-6 shadow-2xs">
          <div className="flex items-center gap-3">
            <button
              aria-label="Open navigation"
              onClick={() => setIsSidebarOpen(true)}
              className="rounded-lg border border-border p-2 text-text-muted hover:bg-surface-muted md:hidden"
            >
              <Menu size={18} />
            </button>

            {/* Quick Cmd+K Palette Trigger */}
            <button
              onClick={() => setIsPaletteOpen(true)}
              className="flex items-center gap-2 rounded-lg border border-border bg-surface-muted/50 px-3 py-1.5 text-xs text-text-muted transition-colors hover:border-primary/40 hover:bg-surface"
            >
              <Search size={14} />
              <span className="hidden sm:inline font-medium">Search modules or press</span>
              <kbd className="rounded bg-surface border border-border px-1.5 py-0.5 text-[10px] font-mono text-text">
                ⌘K
              </kbd>
            </button>
          </div>

          <div className="flex items-center gap-3">
            {/* Active Company Selector Input */}
            <div className="flex items-center gap-2 text-xs">
              <Building2 size={15} className="text-text-muted hidden sm:inline" />
              <span className="hidden text-text-muted font-medium lg:inline">Company:</span>
              <input
                value={companyId}
                onChange={(e) => setCompanyId(e.target.value)}
                placeholder="Active Company ID"
                className="w-32 rounded-lg border border-border bg-surface px-2.5 py-1 text-xs text-text focus:border-primary focus:outline-none sm:w-48"
              />
            </div>

            <div className="h-4 w-px bg-border hidden sm:block" />

            {/* AI Fusion Copilot Trigger */}
            <button
              onClick={() => navigate('/ai')}
              className="flex items-center gap-1.5 rounded-lg bg-purple-500/10 px-2.5 py-1.5 text-xs font-semibold text-purple-600 dark:text-purple-400 hover:bg-purple-500/20 transition-colors"
            >
              <Sparkles size={14} />
              <span className="hidden sm:inline">Fusion AI</span>
            </button>

            {/* Theme Toggle */}
            <button
              aria-label="Toggle dark mode"
              onClick={toggleTheme}
              className="rounded-lg border border-border p-2 text-text-muted hover:bg-surface-muted hover:text-text transition-colors"
            >
              {theme === 'dark' ? <Sun size={16} /> : <Moon size={16} />}
            </button>

            {/* User Session & Logout */}
            <div className="flex items-center gap-2 border-l border-border pl-3">
              {session && (
                <div className="hidden flex-col text-right text-xs lg:flex">
                  <span className="font-semibold text-text truncate max-w-[120px]">{session.email}</span>
                  <span className="text-[10px] text-text-muted uppercase tracking-wider">Staff</span>
                </div>
              )}
              <button
                aria-label="Sign out"
                onClick={handleLogout}
                className="flex items-center gap-1.5 rounded-lg border border-border p-2 text-text-muted hover:bg-rose-500/10 hover:text-rose-500 hover:border-rose-500/30 transition-colors"
              >
                <LogOut size={16} />
              </button>
            </div>
          </div>
        </header>

        {/* Viewport Render Area */}
        <main id="main-content" tabIndex={-1} className="flex-1 overflow-y-auto p-4 sm:p-6 focus:outline-none">
          <Outlet />
        </main>
      </div>

      <CommandPalette open={isPaletteOpen} onClose={() => setIsPaletteOpen(false)} modules={visibleModules} />
    </div>
  );
}

