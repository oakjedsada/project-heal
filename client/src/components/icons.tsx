// Tiny inline SVG icon primitives shared across the app — kept here instead
// of an icon library so the client bundle doesn't grow for a handful of shapes.

export function SparkleIcon({ className }: { className?: string }) {
  return (
    <svg viewBox="0 0 24 24" fill="currentColor" className={className} aria-hidden="true">
      <path d="M12 2.5c.3 3.3 1 5.6 2.2 6.8 1.2 1.2 3.5 1.9 6.8 2.2-3.3.3-5.6 1-6.8 2.2-1.2 1.2-1.9 3.5-2.2 6.8-.3-3.3-1-5.6-2.2-6.8-1.2-1.2-3.5-1.9-6.8-2.2 3.3-.3 5.6-1 6.8-2.2 1.2-1.2 1.9-3.5 2.2-6.8Z" />
    </svg>
  )
}

export function CheckIcon({ className }: { className?: string }) {
  return (
    <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" strokeWidth={2.5} strokeLinecap="round" strokeLinejoin="round" className={className} aria-hidden="true">
      <path d="M4 10.5 8 14.5 16 6" />
    </svg>
  )
}

export function HeartIcon({ className }: { className?: string }) {
  return (
    <svg viewBox="0 0 24 24" fill="currentColor" className={className} aria-hidden="true">
      <path d="M12 21s-7.2-4.5-10-9.1C.4 8.6 1.6 5 5 4.2c2-.5 3.9.4 5 2 .9-1.4 2.7-2.5 5-2 3.4.8 4.6 4.4 3 7.7C19.2 16.5 12 21 12 21Z" />
    </svg>
  )
}
