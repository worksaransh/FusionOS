import { act, renderHook } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { useDebouncedValue } from './useDebouncedValue';

describe('useDebouncedValue', () => {
  it('returns the initial value immediately', () => {
    const { result } = renderHook(() => useDebouncedValue('a', 200));
    expect(result.current).toBe('a');
  });

  it('does not update until the delay elapses', () => {
    vi.useFakeTimers();
    const { result, rerender } = renderHook(({ value }) => useDebouncedValue(value, 250), {
      initialProps: { value: 'a' },
    });

    rerender({ value: 'b' });
    expect(result.current).toBe('a');

    act(() => {
      vi.advanceTimersByTime(100);
    });
    expect(result.current).toBe('a');

    act(() => {
      vi.advanceTimersByTime(150);
    });
    expect(result.current).toBe('b');

    vi.useRealTimers();
  });

  it('resets the timer on every keystroke instead of firing per-keystroke', () => {
    vi.useFakeTimers();
    const { result, rerender } = renderHook(({ value }) => useDebouncedValue(value, 250), {
      initialProps: { value: 'p' },
    });

    rerender({ value: 'pr' });
    act(() => {
      vi.advanceTimersByTime(200);
    });
    rerender({ value: 'pro' });
    act(() => {
      vi.advanceTimersByTime(200);
    });
    // Neither intermediate value should have been committed — only the final
    // one, after a full 250ms of silence (the whole point of debouncing a
    // search box instead of firing a request per keystroke).
    expect(result.current).toBe('p');

    act(() => {
      vi.advanceTimersByTime(60);
    });
    expect(result.current).toBe('pro');

    vi.useRealTimers();
  });
});
