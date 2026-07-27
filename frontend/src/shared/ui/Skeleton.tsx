interface SkeletonProps {
  className?: string;
  rows?: number;
}

export function Skeleton({ className = 'h-4 w-full', rows = 1 }: SkeletonProps) {
  if (rows > 1) {
    return (
      <div className="space-y-2">
        {Array.from({ length: rows }).map((_, i) => (
          <div key={i} className={`animate-skeleton rounded-md bg-border/60 ${className}`} />
        ))}
      </div>
    );
  }
  return <div className={`animate-skeleton rounded-md bg-border/60 ${className}`} />;
}

export function TableSkeleton({ rows = 5, cols = 4 }: { rows?: number; cols?: number }) {
  return (
    <div role="status" aria-label="Loading" className="w-full space-y-3">
      <span className="sr-only">Loading...</span>
      <div className="flex gap-4 border-b border-border pb-2">
        {Array.from({ length: cols }).map((_, i) => (
          <Skeleton key={i} className="h-4 flex-1" />
        ))}
      </div>
      {Array.from({ length: rows }).map((_, r) => (
        <div key={r} className="flex gap-4 py-2 border-b border-border/40">
          {Array.from({ length: cols }).map((_, c) => (
            <Skeleton key={c} className="h-4 flex-1" />
          ))}
        </div>
      ))}
    </div>
  );
}
