import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { useAuthStore } from '../../../shared/auth/authStore';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import type { PagedResult } from '../../../shared/api/types';

interface CopilotMessageDto {
  id: string;
  role: 'User' | 'Assistant';
  content: string;
  sentAt: string;
}

interface CopilotSessionDto {
  id: string;
  userId: string;
  title: string;
  startedAt: string;
  lastMessageAt: string | null;
  messages: CopilotMessageDto[];
}

/**
 * AI Copilot — 12_AI_PLATFORM.md §4: "the conversational surface... a client
 * of the same /api/v1/ai/... and module APIs any other client uses, plus a
 * retrieval layer... over... permitted business data scoped to the asking
 * user's RBAC permissions". A session is a persisted conversation; sending a
 * message runs a real retrieval step (reusing AI Search's intent resolution
 * server-side) and gets back a deterministic, data-grounded reply — never a
 * simulated "AI thinking" effect, since there is no real language model
 * behind this yet (see IChatResponseProvider's doc comment on the backend).
 * Kept deliberately simple: a session list, a scrollable message log, and a
 * text box — no streaming/typing-indicator polish.
 */
export function CopilotPanel() {
  const { companyId } = useActiveCompany();
  const userId = useAuthStore((s) => s.session?.userId);
  const queryClient = useQueryClient();
  const [activeSessionId, setActiveSessionId] = useState<string | null>(null);
  const [draft, setDraft] = useState('');

  const sessionsQuery = useQuery({
    queryKey: ['copilot-sessions', companyId, userId],
    queryFn: () =>
      apiClient.get<PagedResult<CopilotSessionDto>>(`/ai/copilot/sessions?companyId=${companyId}&userId=${userId}&page=1&pageSize=25`),
    enabled: Boolean(companyId && userId),
  });

  const activeSessionQuery = useQuery({
    queryKey: ['copilot-session', companyId, activeSessionId],
    queryFn: () => apiClient.get<CopilotSessionDto>(`/ai/copilot/sessions/${activeSessionId}?companyId=${companyId}`),
    enabled: Boolean(companyId && activeSessionId),
  });

  const startSession = useMutation({
    mutationFn: () => apiClient.post<CopilotSessionDto>('/ai/copilot/sessions', { companyId, userId, title: null }),
    onSuccess: (session) => {
      setActiveSessionId(session.id);
      queryClient.invalidateQueries({ queryKey: ['copilot-sessions', companyId, userId] });
    },
  });

  const sendMessage = useMutation({
    mutationFn: (message: string) =>
      apiClient.post<CopilotSessionDto>(`/ai/copilot/sessions/${activeSessionId}/messages`, { companyId, message }),
    onSuccess: (session) => {
      queryClient.setQueryData(['copilot-session', companyId, activeSessionId], session);
      queryClient.invalidateQueries({ queryKey: ['copilot-sessions', companyId, userId] });
      setDraft('');
    },
  });

  if (!companyId || !userId) {
    return null;
  }

  const sessions = sessionsQuery.data?.data ?? [];
  const activeSession = activeSessionQuery.data;

  return (
    <Card className="mt-6">
      <h2 className="mb-1 text-lg font-semibold text-text">AI Copilot</h2>
      <p className="mb-4 text-sm text-text-muted">
        Ask about FusionOS data in plain English — e.g. &ldquo;overdue invoices&rdquo;, &ldquo;low stock
        products&rdquo;, or &ldquo;open leads&rdquo;. Replies are generated deterministically from real
        retrieved data, not a live language model (no LLM API key is configured in this
        environment yet).
      </p>

      <div className="grid grid-cols-1 gap-4 md:grid-cols-[16rem_1fr]">
        <div>
          <Button type="button" variant="secondary" disabled={startSession.isPending} onClick={() => startSession.mutate()}>
            {startSession.isPending ? 'Starting…' : 'New conversation'}
          </Button>

          <ul className="mt-3 flex flex-col gap-1">
            {sessions.map((session) => (
              <li key={session.id}>
                <button
                  type="button"
                  onClick={() => setActiveSessionId(session.id)}
                  className={`w-full truncate rounded-md px-2 py-1.5 text-left text-sm ${
                    session.id === activeSessionId ? 'bg-primary/10 text-primary' : 'text-text hover:bg-surface'
                  }`}
                >
                  {session.title}
                </button>
              </li>
            ))}
            {sessions.length === 0 && !sessionsQuery.isLoading && (
              <li className="text-sm text-text-muted">No conversations yet.</li>
            )}
          </ul>
        </div>

        <div className="flex min-h-[20rem] flex-col rounded-md border border-border">
          {!activeSessionId ? (
            <p className="m-auto p-6 text-sm text-text-muted">
              Start a new conversation or pick one from the list.
            </p>
          ) : (
            <>
              <div className="flex-1 space-y-3 overflow-y-auto p-3">
                {activeSession?.messages.map((message) => (
                  <div
                    key={message.id}
                    className={`flex ${message.role === 'User' ? 'justify-end' : 'justify-start'}`}
                  >
                    <div
                      className={`max-w-[80%] rounded-lg px-3 py-2 text-sm ${
                        message.role === 'User' ? 'bg-primary text-white' : 'bg-surface text-text'
                      }`}
                    >
                      {message.content}
                    </div>
                  </div>
                ))}
                {activeSessionQuery.isLoading && <p className="text-sm text-text-muted">Loading…</p>}
              </div>

              <form noValidate
                onSubmit={(e) => {
                  e.preventDefault();
                  if (draft.trim()) sendMessage.mutate(draft.trim());
                }}
                className="flex gap-2 border-t border-border p-3"
              >
                <input
                  className="flex-1 rounded-md border border-border bg-surface px-2 py-1.5 text-sm"
                  placeholder="Ask about overdue invoices, low stock, open leads…"
                  value={draft}
                  onChange={(e) => setDraft(e.target.value)}
                  disabled={sendMessage.isPending}
                />
                <Button type="submit" disabled={sendMessage.isPending || !draft.trim()}>
                  {sendMessage.isPending ? 'Sending…' : 'Send'}
                </Button>
              </form>
            </>
          )}
        </div>
      </div>

      {(startSession.isError || sendMessage.isError) && (
        <p role="alert" className="mt-2 text-sm text-danger">
          {startSession.error instanceof ApiError
            ? startSession.error.problem.title
            : sendMessage.error instanceof ApiError
              ? sendMessage.error.problem.title
              : 'Something went wrong talking to the Copilot.'}
        </p>
      )}
    </Card>
  );
}
