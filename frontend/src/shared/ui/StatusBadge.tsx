export type StatusVariant = 'success' | 'danger' | 'warning' | 'info' | 'purple' | 'neutral';

interface StatusBadgeProps {
  status: string;
  variant?: StatusVariant;
  size?: 'sm' | 'md';
}

const variantStyles: Record<StatusVariant, string> = {
  success: 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-500/20',
  danger: 'bg-rose-500/10 text-rose-600 dark:text-rose-400 border-rose-500/20',
  warning: 'bg-amber-500/10 text-amber-600 dark:text-amber-400 border-amber-500/20',
  info: 'bg-sky-500/10 text-sky-600 dark:text-sky-400 border-sky-500/20',
  purple: 'bg-violet-500/10 text-violet-600 dark:text-violet-400 border-violet-500/20',
  neutral: 'bg-slate-500/10 text-slate-600 dark:text-slate-400 border-slate-500/20',
};

export function StatusBadge({ status, variant, size = 'sm' }: StatusBadgeProps) {
  let computedVariant = variant;
  if (!computedVariant) {
    const s = status.toLowerCase();
    if (s.includes('active') || s.includes('approved') || s.includes('success') || s.includes('completed') || s.includes('received')) {
      computedVariant = 'success';
    } else if (s.includes('inactive') || s.includes('rejected') || s.includes('failed') || s.includes('canceled') || s.includes('deactivated')) {
      computedVariant = 'danger';
    } else if (s.includes('pending') || s.includes('draft') || s.includes('warning') || s.includes('in progress')) {
      computedVariant = 'warning';
    } else if (s.includes('open') || s.includes('submitted') || s.includes('info')) {
      computedVariant = 'info';
    } else {
      computedVariant = 'neutral';
    }
  }

  const sizeClass = size === 'sm' ? 'px-2 py-0.5 text-xs font-medium' : 'px-2.5 py-1 text-sm font-medium';

  return (
    <span className={`inline-flex items-center rounded-full border ${variantStyles[computedVariant]} ${sizeClass}`}>
      <span className="mr-1.5 h-1.5 w-1.5 rounded-full bg-current" />
      {status}
    </span>
  );
}
