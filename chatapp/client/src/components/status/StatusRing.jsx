import { cn } from '@/lib/utils';

/**
 * Wraps an avatar with a coloured ring.
 * hasUnviewed = true  → gradient blue ring
 * hasUnviewed = false → muted grey ring
 * noRing      = true  → plain (used for "add" button)
 */
export function StatusRing({ hasUnviewed, noRing = false, size = 52, children }) {
  return (
    <div
      className={cn(
        'rounded-full p-[2.5px] shrink-0',
        noRing
          ? 'bg-transparent'
          : hasUnviewed
          ? 'status-ring-active'
          : 'status-ring-viewed'
      )}
      style={{ width: size + 5, height: size + 5 }}
    >
      <div className="rounded-full bg-background p-[2px] h-full w-full flex items-center justify-center overflow-hidden">
        {children}
      </div>
    </div>
  );
}
