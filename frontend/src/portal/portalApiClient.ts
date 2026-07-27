/**
 * A small, dedicated API client for the Customer Portal - not the staff
 * shared/api/client.ts apiClient (which hardcodes useAuthStore, the staff
 * session store, and has no way to swap in a different token source). This
 * is simpler than trying to parameterize the staff client: it calls the same
 * backend base URL, but attaches the portal's own token
 * (usePortalAuthStore) and, on a 401, clears the portal session (no refresh
 * token in this minimal slice - see backend PortalLoginCommandHandler's doc
 * comment - so a portal session that 401s is just over, back to
 * PortalLoginPage).
 */
import { usePortalAuthStore } from './portalAuthStore';

export interface PortalProblemDetails {
  type?: string;
  title: string;
  status: number;
  traceId?: string;
  errors?: Record<string, string[]>;
}

export class PortalApiError extends Error {
  problem: PortalProblemDetails;

  constructor(problem: PortalProblemDetails) {
    super(problem.title);
    this.problem = problem;
  }
}

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '/api/v1';

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const session = usePortalAuthStore.getState().session;

  const response = await fetch(`${BASE_URL}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...(session ? { Authorization: `Bearer ${session.accessToken}` } : {}),
      ...(init?.headers ?? {}),
    },
  });

  if (response.status === 401) {
    usePortalAuthStore.getState().clearSession();
  }

  if (!response.ok) {
    const problem = (await response.json().catch(() => ({
      title: response.statusText,
      status: response.status,
    }))) as PortalProblemDetails;
    throw new PortalApiError(problem);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export const portalApiClient = {
  get: <T>(path: string) => request<T>(path, { method: 'GET' }),
  post: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: 'POST', body: body !== undefined ? JSON.stringify(body) : undefined }),
};
