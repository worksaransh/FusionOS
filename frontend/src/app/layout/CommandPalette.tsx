import { useEffect, useId, useMemo, useRef, useState } from 'react';
import type { KeyboardEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import clsx from 'clsx';
import { Loader2 } from 'lucide-react';
import type { ModuleNavEntry } from '../modules';
import { apiClient } from '../../shared/api/client';
import type { PagedResult } from '../../shared/api/types';
import { useActiveCompany } from '../../shared/company/useActiveCompany';
import { useDebouncedValue } from '../../shared/hooks/useDebouncedValue';

interface CommandPaletteProps {
  open: boolean;
  onClose: () => void;
  /** Already RBAC-filtered by the caller (AppShell) — see hasPermissionPrefix. */
  modules: ModuleNavEntry[];
}

/** One row inside an entity result group — just enough to render and navigate. */
interface EntitySearchResult {
  id: string;
  label: string;
}

/**
 * Describes one curated, search-capable list endpoint this palette fans out
 * to. `moduleName` does double duty: it's checked against the RBAC-filtered
 * `modules` prop to decide whether this entity is even offered to the
 * signed-in user, and it's the navigation target on click (there's no
 * per-record detail route anywhere in the app — see AppRoutes.tsx, every
 * module resolves to one list page — so "go to the module's list page" is
 * the correct, and only sensible, destination).
 */
interface EntitySearchDescriptor {
  key: string;
  /** Plural, human-facing group heading, e.g. "Products". */
  groupLabel: string;
  moduleName: string;
  path: string;
  mapToResult: (item: unknown) => EntitySearchResult;
}

/** Ties a descriptor's `mapToResult` to a concrete DTO shape at the definition site, then erases it back to `unknown` for the shared array below — keeps every call site type-checked without needing an `any` in the descriptor list itself. */
function defineEntitySearch<T>(descriptor: {
  key: string;
  groupLabel: string;
  moduleName: string;
  path: string;
  mapToResult: (item: T) => EntitySearchResult;
}): EntitySearchDescriptor {
  return { ...descriptor, mapToResult: descriptor.mapToResult as (item: unknown) => EntitySearchResult };
}

/**
 * Curated set of high-value, search-capable list endpoints (confirmed via
 * their controllers — ProductsController.List, CustomersController.List,
 * etc. — all accept `?search=` and return the standard `PagedResult<T>`
 * envelope, 08_API_STANDARDS.md §4) — see shared/api/entityOptions.ts for the
 * full inventory of which entities support `search` today; this is a
 * deliberate subset (one per module, ~9 of the ~15 modules) rather than
 * every searchable entity, to keep a keystroke from firing a couple dozen
 * requests. Add another `defineEntitySearch` entry here if a future entity
 * earns a slot.
 */
const ENTITY_SEARCH_DESCRIPTORS: EntitySearchDescriptor[] = [
  defineEntitySearch<{ id: string; sku: string; name: string }>({
    key: 'products',
    groupLabel: 'Products',
    moduleName: 'inventory',
    path: '/inventory/products',
    mapToResult: (p) => ({ id: p.id, label: `${p.sku} — ${p.name}` }),
  }),
  defineEntitySearch<{ id: string; code: string; name: string }>({
    key: 'customers',
    groupLabel: 'Customers',
    moduleName: 'sales',
    path: '/sales/customers',
    mapToResult: (c) => ({ id: c.id, label: `${c.code} — ${c.name}` }),
  }),
  defineEntitySearch<{ id: string; code: string; name: string }>({
    key: 'suppliers',
    groupLabel: 'Suppliers',
    moduleName: 'procurement',
    path: '/procurement/suppliers',
    mapToResult: (s) => ({ id: s.id, label: `${s.code} — ${s.name}` }),
  }),
  defineEntitySearch<{ id: string; code: string; name: string }>({
    key: 'warehouses',
    groupLabel: 'Warehouses',
    moduleName: 'warehouse',
    path: '/warehouse/warehouses',
    mapToResult: (w) => ({ id: w.id, label: `${w.code} — ${w.name}` }),
  }),
  defineEntitySearch<{ id: string; code: string; name: string }>({
    key: 'accounts',
    groupLabel: 'Accounts',
    moduleName: 'finance',
    path: '/finance/accounts',
    mapToResult: (a) => ({ id: a.id, label: `${a.code} — ${a.name}` }),
  }),
  defineEntitySearch<{ id: string; code: string; name: string }>({
    key: 'bills-of-materials',
    groupLabel: 'Bills of Materials',
    moduleName: 'manufacturing',
    path: '/manufacturing/bills-of-materials',
    mapToResult: (b) => ({ id: b.id, label: `${b.code} — ${b.name}` }),
  }),
  defineEntitySearch<{ id: string; name: string; status: string }>({
    key: 'leads',
    groupLabel: 'Leads',
    moduleName: 'crm',
    path: '/crm/leads',
    mapToResult: (l) => ({ id: l.id, label: `${l.name} (${l.status})` }),
  }),
  defineEntitySearch<{ id: string; code: string; fullName: string }>({
    key: 'employees',
    groupLabel: 'Employees',
    moduleName: 'hrms',
    path: '/hrms/employees',
    mapToResult: (e) => ({ id: e.id, label: `${e.code} — ${e.fullName}` }),
  }),
  defineEntitySearch<{ id: string; code: string; name: string }>({
    key: 'assets',
    groupLabel: 'Assets',
    moduleName: 'maintenance',
    path: '/maintenance/assets',
    mapToResult: (a) => ({ id: a.id, label: `${a.code} — ${a.name}` }),
  }),
];

/** Debounce delay before an entity fan-out fires — long enough that a fast typist doesn't fire one request per keystroke, short enough to still feel instant. */
const SEARCH_DEBOUNCE_MS = 300;
/** A query shorter than this is too likely to match everything to be worth ~9 parallel requests over. */
const MIN_QUERY_LENGTH = 2;
/** Small per-group cap — this is a "jump to the right place" palette, not a full results page. */
const RESULTS_PER_ENTITY = 5;

type EntityGroupState =
  | { status: 'loading'; results: EntitySearchResult[] }
  | { status: 'loaded'; results: EntitySearchResult[] }
  | { status: 'error'; results: EntitySearchResult[] };

/** One flattened, keyboard-navigable row — either a module jump or an entity hit — so Up/Down/Enter walks across both sections with a single highlighted index. */
type SelectableItem =
  | { type: 'module'; module: ModuleNavEntry }
  | { type: 'entity'; descriptor: EntitySearchDescriptor; result: EntitySearchResult };

/**
 * Cmd/Ctrl+K command palette — jumps to a module route (06_UI_UX_DESIGN_SYSTEM.md
 * §3's "global search trigger" placeholder, wired up for real) AND, once the
 * query is 2+ characters, fans out debounced parallel requests to a curated
 * set of entity list endpoints so a search can land on a record, not just a
 * module.
 *
 * ARCHITECTURAL NOTE — why this is a frontend fan-out and not one backend
 * "global search" endpoint: FusionOS enforces a hard module-boundary rule
 * (backend/tests/FusionOS.Architecture.Tests/ModuleBoundaryTests.cs) — no
 * module's backend code may take a compile-time dependency on another
 * module's types. A single cross-module search endpoint would either violate
 * that rule (some module would have to know about Products, Customers,
 * Suppliers, etc. all at once) or require a shared search index/read-model
 * (Elasticsearch/OpenSearch or similar) — infrastructure that doesn't exist
 * today (confirmed absent from docker-compose.yml) and is out of scope for
 * this pass. Since most list endpoints already accept a `?search=` query
 * param (ILIKE-based server-side filtering, 08_API_STANDARDS.md), the
 * pragmatic option is to let the already-authenticated, already-RBAC-aware
 * frontend do the fanning out: no new backend surface, no boundary
 * violation, and it degrades gracefully (per-group errors don't take down
 * the others — see the Promise-per-request handling below).
 *
 * Follows the same combobox-with-listbox keyboard convention as
 * shared/ui/EntityCombobox.tsx (role=combobox/listbox/option,
 * aria-activedescendant tracking a highlighted index, Up/Down/Enter/Escape)
 * rather than inventing a new interaction pattern.
 */
export function CommandPalette({ open, onClose, modules }: CommandPaletteProps) {
  const [query, setQuery] = useState('');
  const [highlightedIndex, setHighlightedIndex] = useState(0);
  const [entityGroups, setEntityGroups] = useState<Record<string, EntityGroupState>>({});
  const navigate = useNavigate();
  const listboxId = useId();
  const { companyId } = useActiveCompany();
  const debouncedQuery = useDebouncedValue(query, SEARCH_DEBOUNCE_MS);
  // Bumped on every search attempt so a slow response from an earlier
  // keystroke can't clobber state set by a newer one that already resolved.
  const requestGenerationRef = useRef(0);

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase();
    if (!q) return modules;
    return modules.filter((m) => m.label.toLowerCase().includes(q) || m.name.toLowerCase().includes(q));
  }, [modules, query]);

  // Only offer entities whose owning module is in the caller's RBAC-filtered
  // `modules` list (AppShell's hasPermissionPrefix/visibleModules gating) —
  // a user with no Manufacturing permission shouldn't fan a request out to
  // /manufacturing/bills-of-materials just because they typed a query.
  const searchableEntities = useMemo(
    () => ENTITY_SEARCH_DESCRIPTORS.filter((d) => modules.some((m) => m.name === d.moduleName)),
    [modules],
  );

  useEffect(() => {
    if (!open) return;

    const trimmed = debouncedQuery.trim();
    const generation = ++requestGenerationRef.current;

    if (trimmed.length < MIN_QUERY_LENGTH || !companyId || searchableEntities.length === 0) {
      setEntityGroups({});
      return;
    }

    setEntityGroups(
      Object.fromEntries(searchableEntities.map((d) => [d.key, { status: 'loading', results: [] } as const])),
    );

    // Independent per-entity fetches (not a single Promise.all) so one
    // endpoint erroring doesn't blank out results that already came back
    // from the others — each group renders whatever state it's actually in.
    for (const descriptor of searchableEntities) {
      const params = new URLSearchParams({
        companyId,
        search: trimmed,
        page: '1',
        pageSize: String(RESULTS_PER_ENTITY),
      });

      apiClient
        .get<PagedResult<unknown>>(`${descriptor.path}?${params.toString()}`)
        .then((page) => {
          if (requestGenerationRef.current !== generation) return; // a newer query already superseded this one
          setEntityGroups((prev) => ({
            ...prev,
            [descriptor.key]: { status: 'loaded', results: page.data.map(descriptor.mapToResult) },
          }));
        })
        .catch(() => {
          if (requestGenerationRef.current !== generation) return;
          setEntityGroups((prev) => ({ ...prev, [descriptor.key]: { status: 'error', results: [] } }));
        });
    }
  }, [open, debouncedQuery, companyId, searchableEntities]);

  // Every entity hit currently in view, in the same order the groups render
  // in — this (appended after the module matches) is what the keyboard
  // highlight actually walks over.
  const entityItems = useMemo(
    () =>
      searchableEntities.flatMap((descriptor) => {
        const group = entityGroups[descriptor.key];
        if (!group || group.results.length === 0) return [];
        return group.results.map((result) => ({ type: 'entity' as const, descriptor, result }));
      }),
    [searchableEntities, entityGroups],
  );

  const selectableItems: SelectableItem[] = useMemo(
    () => [...filtered.map((module) => ({ type: 'module' as const, module })), ...entityItems],
    [filtered, entityItems],
  );

  // First index of each entity group's rows within `selectableItems`/the
  // rendered listbox, so each result button gets the right global index for
  // aria-activedescendant/highlighting without recomputing it inline.
  const groupStartIndex = useMemo(() => {
    const starts: Record<string, number> = {};
    let cursor = filtered.length;
    for (const descriptor of searchableEntities) {
      starts[descriptor.key] = cursor;
      cursor += entityGroups[descriptor.key]?.results.length ?? 0;
    }
    return starts;
  }, [filtered.length, searchableEntities, entityGroups]);

  useEffect(() => {
    setHighlightedIndex(selectableItems.length === 0 ? -1 : 0);
  }, [selectableItems]);

  // Reset to a clean slate every time the palette opens, so it never reopens
  // showing a stale query/highlight/results from the last time it was used.
  useEffect(() => {
    if (open) {
      setQuery('');
      setHighlightedIndex(0);
      setEntityGroups({});
    }
  }, [open]);

  if (!open) return null;

  function goTo(module: ModuleNavEntry) {
    navigate(`/${module.name}`);
    onClose();
  }

  function goToEntityModule(descriptor: EntitySearchDescriptor) {
    navigate(`/${descriptor.moduleName}`);
    onClose();
  }

  function selectItem(item: SelectableItem) {
    if (item.type === 'module') {
      goTo(item.module);
    } else {
      goToEntityModule(item.descriptor);
    }
  }

  function handleKeyDown(event: KeyboardEvent<HTMLInputElement>) {
    if (event.key === 'ArrowDown') {
      event.preventDefault();
      setHighlightedIndex((i) => Math.min(i + 1, selectableItems.length - 1));
    } else if (event.key === 'ArrowUp') {
      event.preventDefault();
      setHighlightedIndex((i) => Math.max(i - 1, 0));
    } else if (event.key === 'Enter') {
      event.preventDefault();
      if (highlightedIndex >= 0 && selectableItems[highlightedIndex]) {
        selectItem(selectableItems[highlightedIndex]);
      }
    } else if (event.key === 'Escape') {
      event.preventDefault();
      onClose();
    }
  }

  return (
    <div
      className="fixed inset-0 z-40 flex items-start justify-center bg-black/40 p-4 pt-24"
      onClick={onClose}
      role="presentation"
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-label="Jump to module or record"
        className="w-full max-w-md rounded-lg border border-border bg-surface p-2 shadow-xl"
        onClick={(event) => event.stopPropagation()}
      >
        <input
          // Opening the palette should focus it immediately, same as any modal dialog.
          autoFocus
          type="text"
          role="combobox"
          aria-expanded="true"
          aria-haspopup="listbox"
          aria-controls={listboxId}
          aria-autocomplete="list"
          aria-activedescendant={highlightedIndex >= 0 ? optionId(listboxId, highlightedIndex) : undefined}
          value={query}
          onChange={(event) => setQuery(event.target.value)}
          onKeyDown={handleKeyDown}
          placeholder="Jump to a module or search records…"
          className="w-full rounded-md border border-border bg-surface px-3 py-2 text-sm text-text outline-none"
        />
        <div role="listbox" id={listboxId} aria-label="Modules and records" className="mt-2 max-h-96 overflow-auto">
          {filtered.length === 0 && <p className="px-3 py-2 text-xs text-text-muted">No matching modules.</p>}
          {filtered.map((module, index) => (
            <button
              key={module.name}
              id={optionId(listboxId, index)}
              role="option"
              aria-selected={index === highlightedIndex}
              type="button"
              className={clsx(
                'block w-full truncate rounded-md px-3 py-2 text-left text-sm',
                index === highlightedIndex ? 'bg-surface-muted font-medium' : 'hover:bg-surface-muted',
              )}
              onMouseEnter={() => setHighlightedIndex(index)}
              onClick={() => goTo(module)}
            >
              {module.label}
            </button>
          ))}

          {searchableEntities.map((descriptor) => {
            const group = entityGroups[descriptor.key];
            if (!group) return null; // haven't searched yet (query too short, no company, etc.)

            if (group.status === 'loading') {
              return (
                <p key={descriptor.key} className="flex items-center gap-2 px-3 py-1.5 text-xs text-text-muted">
                  <Loader2 size={12} className="animate-spin" aria-hidden="true" />
                  Searching {descriptor.groupLabel}…
                </p>
              );
            }

            if (group.status === 'error') {
              return (
                <p key={descriptor.key} className="px-3 py-1.5 text-xs text-danger">
                  {descriptor.groupLabel} search failed.
                </p>
              );
            }

            if (group.results.length === 0) return null; // loaded, nothing found — skip silently rather than clutter the list

            return (
              <div key={descriptor.key}>
                <p className="px-3 pt-2 text-xs font-semibold uppercase tracking-wide text-text-muted">
                  {descriptor.groupLabel} ({group.results.length})
                </p>
                {group.results.map((result, resultIndex) => {
                  const globalIndex = groupStartIndex[descriptor.key] + resultIndex;
                  return (
                    <button
                      key={result.id}
                      id={optionId(listboxId, globalIndex)}
                      role="option"
                      aria-selected={globalIndex === highlightedIndex}
                      type="button"
                      className={clsx(
                        'block w-full truncate rounded-md px-3 py-2 text-left text-sm',
                        globalIndex === highlightedIndex ? 'bg-surface-muted font-medium' : 'hover:bg-surface-muted',
                      )}
                      onMouseEnter={() => setHighlightedIndex(globalIndex)}
                      onClick={() => goToEntityModule(descriptor)}
                    >
                      {result.label}
                    </button>
                  );
                })}
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}

function optionId(listboxId: string, index: number): string {
  return `${listboxId}-option-${index}`;
}
