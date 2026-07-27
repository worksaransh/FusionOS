import { type ReactNode } from 'react';
import { ArrowUpRight, ArrowDownRight, Minus } from 'lucide-react';
import { Card } from './Card';

interface StatsCardProps {
  title: string;
  value: string | number;
  change?: string | number;
  changeType?: 'positive' | 'negative' | 'neutral';
  description?: string;
  icon?: ReactNode;
}

export function StatsCard({ title, value, change, changeType = 'neutral', description, icon }: StatsCardProps) {
  return (
    <Card className="relative overflow-hidden transition-all hover:border-primary/40 hover:shadow-md">
      <div className="flex items-start justify-between">
        <div>
          <p className="text-xs font-medium uppercase tracking-wider text-text-muted">{title}</p>
          <p className="mt-2 text-2xl font-bold tracking-tight text-text sm:text-3xl">{value}</p>
        </div>
        {icon && <div className="rounded-lg bg-primary/10 p-2.5 text-primary">{icon}</div>}
      </div>

      {(change !== undefined || description) && (
        <div className="mt-4 flex items-center justify-between text-xs border-t border-border/50 pt-2.5">
          {change !== undefined && (
            <span
              className={`inline-flex items-center font-medium ${
                changeType === 'positive'
                  ? 'text-emerald-600 dark:text-emerald-400'
                  : changeType === 'negative'
                  ? 'text-rose-600 dark:text-rose-400'
                  : 'text-text-muted'
              }`}
            >
              {changeType === 'positive' && <ArrowUpRight size={14} className="mr-0.5" />}
              {changeType === 'negative' && <ArrowDownRight size={14} className="mr-0.5" />}
              {changeType === 'neutral' && <Minus size={14} className="mr-0.5" />}
              {change}
            </span>
          )}
          {description && <span className="text-text-muted">{description}</span>}
        </div>
      )}
    </Card>
  );
}
