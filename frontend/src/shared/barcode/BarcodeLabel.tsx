import { useMemo } from 'react';
import { CODE39_CHARSET, encodeCode39, normalizeCode39Input, validateCode39 } from './code39';

interface BarcodeLabelProps {
  /** Raw value to encode — uppercased/trimmed internally (Code 39 has no lowercase). */
  value: string;
  className?: string;
  /** Bar height in SVG user units. Defaults to a size legible on a small product-edit panel. */
  height?: number;
  /** Width of one narrow module in SVG user units; a wide module is 3x this. */
  narrowWidth?: number;
  /** Whether to print the human-readable value under the bars. Defaults to true. */
  showValue?: boolean;
}

/**
 * Renders a Code 39 linear barcode as an inline SVG — see shared/barcode/code39.ts for why Code
 * 39 (and not QR) is the one symbology this codebase renders itself. Deliberately fails loud
 * with a text message instead of drawing anything when the value doesn't fit the Code 39
 * character set (uppercase A-Z, 0-9, and - . $ / + % space) — a barcode image that looks right
 * but silently doesn't scan is worse than no image at all.
 */
export function BarcodeLabel({ value, className, height = 60, narrowWidth = 2, showValue = true }: BarcodeLabelProps) {
  const normalized = normalizeCode39Input(value);
  const { valid, invalidChars } = useMemo(() => validateCode39(normalized), [normalized]);

  if (!normalized) return null;

  if (!valid) {
    return (
      <p role="alert" className={className ? `${className} text-xs text-danger` : 'text-xs text-danger'}>
        Can&apos;t render a Code 39 barcode for &quot;{value}&quot; — unsupported character
        {invalidChars.length > 1 ? 's' : ''} {invalidChars.join(', ')}. Code 39 only supports {CODE39_CHARSET}.
      </p>
    );
  }

  const { bars, totalWidth } = encodeCode39(normalized, narrowWidth);
  const quietZone = narrowWidth * 10; // quiet zone margin either side, per Code 39 convention
  const viewWidth = totalWidth + quietZone * 2;

  return (
    <div className={className}>
      <svg
        viewBox={`0 0 ${viewWidth} ${height}`}
        width={viewWidth}
        height={height}
        role="img"
        aria-label={`Code 39 barcode for ${normalized}`}
      >
        <rect x={0} y={0} width={viewWidth} height={height} fill="#ffffff" />
        {bars.map((bar) => (
          <rect key={bar.x} x={bar.x + quietZone} y={0} width={bar.width} height={height} fill="#000000" />
        ))}
      </svg>
      {showValue && <p className="text-center text-xs tracking-widest text-text-muted">{normalized}</p>}
    </div>
  );
}
