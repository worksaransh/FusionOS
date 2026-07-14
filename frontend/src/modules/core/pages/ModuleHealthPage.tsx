import { useQuery } from '@tanstack/react-query';
import { useParams } from 'react-router-dom';
import { apiClient } from '../../../shared/api/client';
import { Card } from '../../../shared/ui/Card';
import { PageHeader } from '../../../shared/ui/PageHeader';
import { MODULES } from '../../../app/modules';

interface HealthResponse {
  module: string;
  status: string;
  roadmapPhase: string;
}

/**
 * Generic page every not-yet-implemented module resolves to. Proves the
 * module is registered end-to-end (frontend route -> backend health endpoint
 * -> module's own DbContext/schema) per 03_SYSTEM_ARCHITECTURE.md, without
 * pretending any business functionality exists yet.
 */
export function ModuleHealthPage() {
  const { moduleName } = useParams<{ moduleName: string }>();
  const entry = MODULES.find((m) => m.name === moduleName);

  const { data, isLoading, isError } = useQuery({
    queryKey: ['module-health', entry?.schema],
    queryFn: () => apiClient.get<HealthResponse>(`/${entry?.schema}/health`),
    enabled: Boolean(entry),
  });

  if (!entry) return <p className="text-text-muted">Unknown module.</p>;

  return (
    <div>
      <PageHeader title={entry.label} description={entry.phase} />
      <Card>
        {isLoading && <p className="text-text-muted">Checking module health…</p>}
        {isError && <p className="text-danger">Could not reach the {entry.label} API. Is the backend running?</p>}
        {data && (
          <dl className="grid grid-cols-1 gap-2 text-sm sm:grid-cols-2">
            <dt className="text-text-muted">Module</dt>
            <dd>{data.module}</dd>
            <dt className="text-text-muted">Status</dt>
            <dd>{data.status}</dd>
            <dt className="text-text-muted">Roadmap phase</dt>
            <dd>{data.roadmapPhase}</dd>
          </dl>
        )}
      </Card>
    </div>
  );
}
