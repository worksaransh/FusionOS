import { useEffect, useState } from 'react';

/** Generic debounce for anything driving a network request as the user types (e.g. EntityCombobox search — 06_UI_UX_DESIGN_SYSTEM.md §2). */
export function useDebouncedValue<T>(value: T, delayMs: number): T {
  const [debounced, setDebounced] = useState(value);

  useEffect(() => {
    const timeout = setTimeout(() => setDebounced(value), delayMs);
    return () => clearTimeout(timeout);
  }, [value, delayMs]);

  return debounced;
}
