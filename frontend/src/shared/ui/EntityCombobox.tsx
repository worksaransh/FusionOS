import { useEffect, useId, useMemo, useRef, useState } from 'react';
import { ChevronDown, Loader2, X } from 'lucide-react';
import clsx from 'clsx';

export interface EntityOption {
  id: string;
  label: string;
}

interface EntityComboboxProps {
  value: string;
  onChange: (id: string) => void;
  options: EntityOption[];
  isLoading?: boolean;
  placeholder?: string;
  className?: string;
  disabled?: boolean;
  /**
   * When provided, every keystroke is forwarded here instead of filtering
   * `options` client-side — the caller (shared/api/entityOptions.ts) is
   * expected to debounce it and re-fetch from a server-side search endpoint
   * (08_API_STANDARDS.md). Omit it for entities whose list endpoint has no
   * `search` param yet; the component then falls back to filtering the
   * options it was given, same as before.
   */
  onSearchChange?: (search: string) => void;
}

/**
 * Searchable id picker backing every cross-module reference field in this app
 * (Product, Supplier, Customer, Warehouse, Zone, Account, Sales Order,
 * Purchase Order) — replaces raw pasted GUIDs with a type-to-filter dropdown
 * over the entity's own list endpoint (06_UI_UX_DESIGN_SYSTEM.md §2).
 * Options are fetched by the caller (see shared/api/entityOptions.ts) and
 * passed in; this component only owns search/open state and emits the
 * selected id, so it works the same way for any entity type without needing
 * per-entity picker components.
 *
 * Implements the WAI-ARIA 1.2 "combobox with listbox popup" pattern
 * (role=combobox on the input, role=listbox on the popup, aria-activedescendant
 * tracking a keyboard-highlighted option) plus Up/Down/Home/End/Enter/Escape
 * keyboard support — flagged as missing entirely in the audit's Frontend/UX
 * pass, and worth fixing once here since every reference-data picker in the
 * app is this one component (09_CODING_STANDARDS.md accessibility gap).
 */
export function EntityCombobox({ value, onChange, options, isLoading, placeholder, className, disabled, onSearchChange }: EntityComboboxProps) {
  const [query, setQuery] = useState('');
  const [isOpen, setIsOpen] = useState(false);
  const [highlightedIndex, setHighlightedIndex] = useState(-1);
  const containerRef = useRef<HTMLDivElement>(null);
  const listboxId = useId();

  const selected = useMemo(() => options.find((o) => o.id === value), [options, value]);

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  // With server-side search, `options` is already the filtered result set —
  // filtering it again client-side would just re-narrow an already-narrow list.
  const filtered = useMemo(() => {
    if (onSearchChange) return options;
    const q = query.trim().toLowerCase();
    if (!q) return options;
    return options.filter((o) => o.label.toLowerCase().includes(q));
  }, [options, query, onSearchChange]);

  // Keep the highlighted option in range whenever the visible list changes
  // (new search results, options finish loading, dropdown re-opens, etc.).
  useEffect(() => {
    setHighlightedIndex(filtered.length === 0 ? -1 : 0);
  }, [filtered]);

  useEffect(() => {
    if (!isOpen || highlightedIndex < 0) return;
    document.getElementById(optionId(listboxId, highlightedIndex))?.scrollIntoView({ block: 'nearest' });
  }, [isOpen, highlightedIndex, listboxId]);

  const displayValue = isOpen ? query : (selected?.label ?? '');

  function selectOption(option: EntityOption) {
    onChange(option.id);
    setQuery('');
    setIsOpen(false);
  }

  function handleKeyDown(event: React.KeyboardEvent<HTMLInputElement>) {
    if (disabled) return;

    if (event.key === 'ArrowDown') {
      event.preventDefault();
      if (!isOpen) {
        setIsOpen(true);
        return;
      }
      setHighlightedIndex((i) => Math.min(i + 1, filtered.length - 1));
    } else if (event.key === 'ArrowUp') {
      event.preventDefault();
      if (!isOpen) {
        setIsOpen(true);
        return;
      }
      setHighlightedIndex((i) => Math.max(i - 1, 0));
    } else if (event.key === 'Home') {
      if (isOpen && filtered.length > 0) {
        event.preventDefault();
        setHighlightedIndex(0);
      }
    } else if (event.key === 'End') {
      if (isOpen && filtered.length > 0) {
        event.preventDefault();
        setHighlightedIndex(filtered.length - 1);
      }
    } else if (event.key === 'Enter') {
      if (isOpen && highlightedIndex >= 0 && filtered[highlightedIndex]) {
        event.preventDefault();
        selectOption(filtered[highlightedIndex]);
      }
    } else if (event.key === 'Escape') {
      if (isOpen) {
        event.preventDefault();
        setIsOpen(false);
        setQuery('');
      }
    }
  }

  return (
    <div ref={containerRef} className={clsx('relative', className)}>
      <div className="relative">
        <input
          type="text"
          role="combobox"
          aria-expanded={isOpen}
          aria-haspopup="listbox"
          aria-controls={listboxId}
          aria-autocomplete="list"
          aria-activedescendant={isOpen && highlightedIndex >= 0 ? optionId(listboxId, highlightedIndex) : undefined}
          className="w-full rounded-md border border-border bg-surface px-2 py-1.5 pr-14 text-sm"
          placeholder={placeholder}
          value={displayValue}
          disabled={disabled}
          onFocus={() => {
            setQuery('');
            setIsOpen(true);
            onSearchChange?.('');
          }}
          onChange={(e) => {
            setQuery(e.target.value);
            setIsOpen(true);
            onSearchChange?.(e.target.value);
          }}
          onKeyDown={handleKeyDown}
        />
        <div className="absolute right-2 top-1/2 flex -translate-y-1/2 items-center gap-1">
          {isLoading && <Loader2 size={14} className="animate-spin text-text-muted" aria-hidden="true" />}
          {value && !disabled && (
            <button
              type="button"
              tabIndex={-1}
              aria-label="Clear selection"
              className="text-text-muted hover:text-text"
              onClick={() => {
                onChange('');
                setQuery('');
              }}
            >
              <X size={14} aria-hidden="true" />
            </button>
          )}
          <ChevronDown size={14} className="text-text-muted" aria-hidden="true" />
        </div>
      </div>

      {isOpen && (
        <div role="listbox" id={listboxId} className="absolute z-20 mt-1 max-h-56 w-full overflow-auto rounded-md border border-border bg-surface shadow-lg">
          {filtered.length === 0 && (
            <p className="px-3 py-2 text-xs text-text-muted">{isLoading ? 'Loading…' : 'No matches.'}</p>
          )}
          {filtered.map((option, index) => (
            <button
              key={option.id}
              id={optionId(listboxId, index)}
              role="option"
              aria-selected={option.id === value}
              type="button"
              className={clsx(
                'block w-full truncate px-3 py-1.5 text-left text-sm hover:bg-surface-muted',
                (option.id === value || index === highlightedIndex) && 'bg-surface-muted font-medium',
              )}
              onMouseEnter={() => setHighlightedIndex(index)}
              onClick={() => selectOption(option)}
            >
              {option.label}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}

function optionId(listboxId: string, index: number): string {
  return `${listboxId}-option-${index}`;
}
