import type { ReactNode } from 'react';

interface PageHeaderProps {
  title: string;
  description?: string;
  actions?: ReactNode;
}

/** Consistent page-title treatment across every module — 06_UI_UX_DESIGN_SYSTEM.md §2. */
export function PageHeader({ title, description, actions }: PageHeaderProps) {
  return (
    <div className="mb-6 flex items-start justify-between gap-4">
      <div>
        <h1 className="text-xl font-semibold text-text">{title}</h1>
        {description && <p className="mt-1 text-sm text-text-muted">{description}</p>}
      </div>
      {actions}
    </div>
  );
}
