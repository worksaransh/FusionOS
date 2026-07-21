/**
 * Code 39 ("3 of 9") linear barcode encoder — pure, dependency-free.
 *
 * This exists because FusionOS has no barcode/QR rendering anywhere (2026-07-21). A real QR
 * code is a structured 2D matrix with Reed-Solomon error correction; writing a correct encoder
 * from scratch with no library is a substantial, error-prone undertaking where a subtly wrong
 * implementation produces something that LOOKS like a QR code but silently fails to scan — worse
 * than shipping nothing. Code 39 is different: it's a simple 1D symbology where every character
 * maps to one fixed, fully-published 9-element bar/space pattern (5 bars + 4 spaces, of which
 * exactly 3 elements are "wide" and 6 are "narrow" — hence "3 of 9"). That table is small,
 * closed, and has been unchanged for decades (ANSI/AIM USS-39), so it's tractable to hard-code
 * exactly and get right, unlike QR.
 *
 * Character set limitation: Code 39 supports only uppercase A-Z, digits 0-9, and the symbols
 * - . $ / + % and space. There is no lowercase and no arbitrary Unicode. Input is uppercased and
 * validated against this table before encoding; anything outside it is rejected rather than
 * silently mangled (see validateCode39 / encodeCode39).
 */

const WIDE = 1;

/**
 * One entry per element = 9 elements per character, alternating bar/space starting with a bar
 * (bar, space, bar, space, bar, space, bar, space, bar). 0 = narrow, 1 = wide. Every pattern below
 * has exactly three 1s and six 0s, matching the "3 of 9" structural invariant of this symbology —
 * a useful self-check that these 44 patterns haven't been mistyped.
 */
const CODE39_PATTERNS: Record<string, readonly number[]> = {
  '0': [0, 0, 0, 1, 1, 0, 1, 0, 0],
  '1': [1, 0, 0, 1, 0, 0, 0, 0, 1],
  '2': [0, 0, 1, 1, 0, 0, 0, 0, 1],
  '3': [1, 0, 1, 1, 0, 0, 0, 0, 0],
  '4': [0, 0, 0, 1, 1, 0, 0, 0, 1],
  '5': [1, 0, 0, 1, 1, 0, 0, 0, 0],
  '6': [0, 0, 1, 1, 1, 0, 0, 0, 0],
  '7': [0, 0, 0, 1, 0, 0, 1, 0, 1],
  '8': [1, 0, 0, 1, 0, 0, 1, 0, 0],
  '9': [0, 0, 1, 1, 0, 0, 1, 0, 0],
  A: [1, 0, 0, 0, 0, 1, 0, 0, 1],
  B: [0, 0, 1, 0, 0, 1, 0, 0, 1],
  C: [1, 0, 1, 0, 0, 1, 0, 0, 0],
  D: [0, 0, 0, 0, 1, 1, 0, 0, 1],
  E: [1, 0, 0, 0, 1, 1, 0, 0, 0],
  F: [0, 0, 1, 0, 1, 1, 0, 0, 0],
  G: [0, 0, 0, 0, 0, 1, 1, 0, 1],
  H: [1, 0, 0, 0, 0, 1, 1, 0, 0],
  I: [0, 0, 1, 0, 0, 1, 1, 0, 0],
  J: [0, 0, 0, 0, 1, 1, 1, 0, 0],
  K: [1, 0, 0, 0, 0, 0, 0, 1, 1],
  L: [0, 0, 1, 0, 0, 0, 0, 1, 1],
  M: [1, 0, 1, 0, 0, 0, 0, 1, 0],
  N: [0, 0, 0, 0, 1, 0, 0, 1, 1],
  O: [1, 0, 0, 0, 1, 0, 0, 1, 0],
  P: [0, 0, 1, 0, 1, 0, 0, 1, 0],
  Q: [0, 0, 0, 0, 0, 0, 1, 1, 1],
  R: [1, 0, 0, 0, 0, 0, 1, 1, 0],
  S: [0, 0, 1, 0, 0, 0, 1, 1, 0],
  T: [0, 0, 0, 0, 1, 0, 1, 1, 0],
  U: [1, 1, 0, 0, 0, 0, 0, 0, 1],
  V: [0, 1, 1, 0, 0, 0, 0, 0, 1],
  W: [1, 1, 1, 0, 0, 0, 0, 0, 0],
  X: [0, 1, 0, 0, 1, 0, 0, 0, 1],
  Y: [1, 1, 0, 0, 1, 0, 0, 0, 0],
  Z: [0, 1, 1, 0, 1, 0, 0, 0, 0],
  '-': [0, 1, 0, 0, 0, 0, 1, 0, 1],
  '.': [1, 1, 0, 0, 0, 0, 1, 0, 0],
  ' ': [0, 1, 1, 0, 0, 0, 1, 0, 0],
  $: [0, 1, 0, 1, 0, 1, 0, 0, 0],
  '/': [0, 1, 0, 1, 0, 0, 0, 1, 0],
  '+': [0, 1, 0, 0, 0, 1, 0, 1, 0],
  '%': [0, 0, 0, 1, 0, 1, 0, 1, 0],
  '*': [0, 1, 0, 0, 1, 0, 1, 0, 0], // start/stop sentinel — not a valid data character
};

/** The data character set this encoder accepts (excludes '*', which is reserved for start/stop). */
export const CODE39_CHARSET = '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-. $/+%';

export function normalizeCode39Input(value: string): string {
  return (value ?? '').trim().toUpperCase();
}

export function isValidCode39Char(ch: string): boolean {
  return ch !== '*' && Object.prototype.hasOwnProperty.call(CODE39_PATTERNS, ch);
}

export interface Code39ValidationResult {
  valid: boolean;
  invalidChars: string[];
}

/** Validates an (already-uppercased) value against the Code 39 character set. Empty is invalid — there's nothing to encode. */
export function validateCode39(value: string): Code39ValidationResult {
  if (!value) return { valid: false, invalidChars: [] };

  const invalidChars = Array.from(new Set(Array.from(value).filter((ch) => !isValidCode39Char(ch))));
  return { valid: invalidChars.length === 0, invalidChars };
}

export interface Code39Bar {
  x: number;
  width: number;
}

export interface Code39Encoding {
  bars: Code39Bar[];
  totalWidth: number;
}

/**
 * Encodes `value` (uppercased/trimmed internally) into a list of black bar rectangles on a white
 * background, using module widths where a wide element is 3x a narrow one (the standard Code 39
 * ratio). Throws if the value is empty or contains characters outside CODE39_CHARSET — callers
 * (e.g. BarcodeLabel) should validate first with validateCode39 and show a message instead of
 * calling this with bad input.
 */
export function encodeCode39(value: string, narrowWidth = 2): Code39Encoding {
  const normalized = normalizeCode39Input(value);
  const { valid, invalidChars } = validateCode39(normalized);
  if (!valid) {
    throw new Error(
      normalized
        ? `Value contains characters unsupported by Code 39: ${invalidChars.join(', ')}`
        : 'Value is empty — nothing to encode.',
    );
  }

  const wideWidth = narrowWidth * 3;
  const sequence = `*${normalized}*`; // start/stop sentinel on both ends, per the Code 39 spec
  const bars: Code39Bar[] = [];
  let cursor = 0;

  for (let charIndex = 0; charIndex < sequence.length; charIndex++) {
    const pattern = CODE39_PATTERNS[sequence[charIndex]];
    for (let elementIndex = 0; elementIndex < pattern.length; elementIndex++) {
      const isBar = elementIndex % 2 === 0; // elements alternate bar, space, bar, space, ... starting with a bar
      const width = pattern[elementIndex] === WIDE ? wideWidth : narrowWidth;
      if (isBar) {
        bars.push({ x: cursor, width });
      }
      cursor += width;
    }

    // One narrow inter-character gap between characters (not after the last one) — standard
    // Code 39 spacing, distinct from the 4 intra-character spaces already in each pattern.
    if (charIndex < sequence.length - 1) {
      cursor += narrowWidth;
    }
  }

  return { bars, totalWidth: cursor };
}
