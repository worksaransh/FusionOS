import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { History, MessageSquare, Pencil, Trash2 } from 'lucide-react';
import { apiClient, ApiError } from '../api/client';
import { useActiveCompany } from '../company/useActiveCompany';
import { useAuthStore } from '../auth/authStore';
import { Button } from './Button';
import { Card } from './Card';

interface ActivityTimelineEntryDto {
  id: string;
  kind: 'AuditEvent' | 'Comment';
  timestamp: string;
  actorUserId: string;
  description: string;
}

interface ActivityTimelinePanelProps {
  /** The polymorphic reference's EntityType (e.g. "PurchaseOrder") — must match whatever the backend command that created the target entity used, same (EntityType, EntityId) convention as ApprovalRequest. */
  entityType: string;
  entityId: string;
}

/**
 * Reusable read-only activity feed + comment composer for any
 * (entityType, entityId) target. Backed by GET /api/v1/core/activity, which
 * merges Core's system-generated AuditLog with user-authored Comments into
 * one chronological list — this component doesn't know or care which of the
 * two produced any given row beyond picking an icon for it.
 *
 * Edit/delete controls only render on the signed-in user's own Comment
 * entries (kind === 'Comment' && actorUserId === the caller's own user id).
 * This is a UX nicety, not the real authorization boundary — the backend
 * (UpdateCommentCommandHandler/DeleteCommentCommandHandler) enforces
 * author-only edit and author-or-"core.comment.delete"-holder delete
 * regardless of what this component chooses to render, same caveat as every
 * other permission-aware control in this app (06_UI_UX_DESIGN_SYSTEM.md §8).
 */
export function ActivityTimelinePanel({ entityType, entityId }: ActivityTimelinePanelProps) {
  const { companyId } = useActiveCompany();
  const session = useAuthStore((s) => s.session);
  const queryClient = useQueryClient();

  const [draft, setDraft] = useState('');
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editDraft, setEditDraft] = useState('');

  const queryKey = ['activity-timeline', companyId, entityType, entityId];

  const timelineQuery = useQuery({
    queryKey,
    queryFn: () =>
      apiClient.get<ActivityTimelineEntryDto[]>(
        `/core/activity?companyId=${companyId}&entityType=${entityType}&entityId=${entityId}`,
      ),
    enabled: Boolean(companyId && entityId),
  });

  const createComment = useMutation({
    mutationFn: (body: string) => apiClient.post('/core/comments', { companyId, entityType, entityId, body }),
    onSuccess: () => {
      setDraft('');
      queryClient.invalidateQueries({ queryKey });
    },
  });

  const updateComment = useMutation({
    mutationFn: ({ id, body }: { id: string; body: string }) => apiClient.put(`/core/comments/${id}`, { companyId, body }),
    onSuccess: () => {
      setEditingId(null);
      queryClient.invalidateQueries({ queryKey });
    },
  });

  const deleteComment = useMutation({
    mutationFn: (id: string) => apiClient.delete(`/core/comments/${id}?companyId=${companyId}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey }),
  });

  if (!companyId) return null;

  const entries = timelineQuery.data ?? [];

  return (
    <Card className="mt-4">
      <h3 className="mb-3 text-sm font-semibold text-text">Activity</h3>

      <form
        className="mb-4 flex flex-col gap-2"
        onSubmit={(event) => {
          event.preventDefault();
          const body = draft.trim();
          if (body) createComment.mutate(body);
        }}
      >
        <textarea
          className="min-h-16 w-full rounded-md border border-border bg-surface px-2 py-1.5 text-sm"
          placeholder="Add a comment…"
          value={draft}
          onChange={(event) => setDraft(event.target.value)}
          maxLength={4000}
        />
        <Button type="submit" className="w-fit" disabled={!draft.trim() || createComment.isPending}>
          {createComment.isPending ? 'Posting…' : 'Comment'}
        </Button>
        {createComment.isError && createComment.error instanceof ApiError && (
          <p role="alert" className="text-xs text-danger">{createComment.error.problem.title}</p>
        )}
      </form>

      {timelineQuery.isLoading && <p role="status" className="text-sm text-text-muted">Loading…</p>}
      {timelineQuery.isError && <p role="alert" className="text-sm text-danger">Could not load activity.</p>}

      <ul className="flex flex-col gap-3">
        {entries.map((entry) => {
          const isComment = entry.kind === 'Comment';
          const isOwnComment = isComment && entry.actorUserId === session?.userId;
          const isEditing = editingId === entry.id;

          return (
            <li key={entry.id} className="flex items-start gap-2 text-sm">
              <span className="mt-0.5 text-text-muted" aria-hidden="true">
                {isComment ? <MessageSquare size={16} /> : <History size={16} />}
              </span>
              <div className="flex-1">
                <div className="text-xs text-text-muted">
                  {new Date(entry.timestamp).toLocaleString()}
                  {!isComment && ' · system'}
                </div>
                {isEditing ? (
                  <div className="mt-1 flex flex-col gap-1">
                    <textarea
                      className="min-h-14 w-full rounded-md border border-border bg-surface px-2 py-1.5 text-sm"
                      value={editDraft}
                      onChange={(event) => setEditDraft(event.target.value)}
                      maxLength={4000}
                    />
                    <div className="flex gap-2">
                      <Button
                        variant="secondary"
                        disabled={!editDraft.trim() || updateComment.isPending}
                        onClick={() => {
                          const body = editDraft.trim();
                          if (body) updateComment.mutate({ id: entry.id, body });
                        }}
                      >
                        Save
                      </Button>
                      <Button variant="secondary" onClick={() => setEditingId(null)}>Cancel</Button>
                    </div>
                  </div>
                ) : (
                  <p className="whitespace-pre-wrap text-text">{entry.description}</p>
                )}
              </div>
              {isOwnComment && !isEditing && (
                <div className="flex gap-1">
                  <Button
                    variant="secondary"
                    aria-label="Edit comment"
                    onClick={() => {
                      setEditingId(entry.id);
                      setEditDraft(entry.description);
                    }}
                  >
                    <Pencil size={14} />
                  </Button>
                  <Button
                    variant="secondary"
                    aria-label="Delete comment"
                    disabled={deleteComment.isPending}
                    onClick={() => deleteComment.mutate(entry.id)}
                  >
                    <Trash2 size={14} />
                  </Button>
                </div>
              )}
            </li>
          );
        })}
        {entries.length === 0 && !timelineQuery.isLoading && (
          <li className="text-sm text-text-muted">No activity yet.</li>
        )}
      </ul>
    </Card>
  );
}
