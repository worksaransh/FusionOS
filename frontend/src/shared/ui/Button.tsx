import { type ButtonHTMLAttributes, forwardRef } from 'react';
import clsx from 'clsx';

type Variant = 'primary' | 'secondary' | 'danger';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant;
}

const variantClasses: Record<Variant, string> = {
  primary: 'bg-primary text-primary-foreground hover:opacity-90',
  secondary: 'bg-surface-muted text-text border border-border hover:bg-border/40',
  danger: 'bg-danger text-white hover:opacity-90',
};

/**
 * Base button from the shared @fusionos/ui component set (06_UI_UX_DESIGN_SYSTEM.md §2).
 * Feature code composes this rather than styling raw <button> elements.
 */
export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ variant = 'primary', className, ...props }, ref) => (
    <button
      ref={ref}
      className={clsx(
        'inline-flex items-center justify-center rounded-md px-3 py-2 text-sm font-medium transition-colors disabled:opacity-50 disabled:pointer-events-none',
        variantClasses[variant],
        className,
      )}
      {...props}
    />
  ),
);
Button.displayName = 'Button';
