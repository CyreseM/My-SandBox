import { cn } from '@/lib/utils';
import { Slot } from '@radix-ui/react-slot';
import { cva } from 'class-variance-authority';
import { forwardRef } from 'react';

// ── Button ────────────────────────────────────────────────────────────────────
const buttonVariants = cva(
  'inline-flex items-center justify-center whitespace-nowrap rounded-lg text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary disabled:pointer-events-none disabled:opacity-50',
  {
    variants: {
      variant: {
        default:     'bg-primary text-primary-foreground hover:bg-primary/90',
        destructive: 'bg-destructive text-destructive-foreground hover:bg-destructive/90',
        outline:     'border border-border bg-transparent hover:bg-accent hover:text-accent-foreground',
        ghost:       'hover:bg-accent hover:text-accent-foreground',
        secondary:   'bg-muted text-foreground hover:bg-muted/80',
        link:        'text-primary underline-offset-4 hover:underline',
      },
      size: {
        default: 'h-9 px-4 py-2',
        sm:      'h-8 rounded-md px-3 text-xs',
        lg:      'h-10 rounded-lg px-6',
        icon:    'h-9 w-9',
      },
    },
    defaultVariants: { variant: 'default', size: 'default' },
  }
);

export const Button = forwardRef(({ className, variant, size, asChild, ...props }, ref) => {
  const Comp = asChild ? Slot : 'button';
  return <Comp ref={ref} className={cn(buttonVariants({ variant, size, className }))} {...props} />;
});
Button.displayName = 'Button';

// ── Input ─────────────────────────────────────────────────────────────────────
export const Input = forwardRef(({ className, type, ...props }, ref) => (
  <input
    type={type ?? 'text'}
    className={cn(
      'flex h-9 w-full rounded-lg border border-border bg-muted/50 px-3 py-1 text-sm',
      'placeholder:text-muted-foreground',
      'focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent',
      'disabled:cursor-not-allowed disabled:opacity-50',
      'transition-colors',
      className
    )}
    ref={ref}
    {...props}
  />
));
Input.displayName = 'Input';

// ── Textarea ──────────────────────────────────────────────────────────────────
export const Textarea = forwardRef(({ className, ...props }, ref) => (
  <textarea
    className={cn(
      'flex w-full rounded-lg border border-border bg-muted/50 px-3 py-2 text-sm',
      'placeholder:text-muted-foreground resize-none',
      'focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent',
      'disabled:cursor-not-allowed disabled:opacity-50',
      'transition-colors',
      className
    )}
    ref={ref}
    {...props}
  />
));
Textarea.displayName = 'Textarea';

// ── Avatar ────────────────────────────────────────────────────────────────────
export function Avatar({ src, fallback, size = 'md', className }) {
  const sizes = { sm: 'h-8 w-8 text-xs', md: 'h-10 w-10 text-sm', lg: 'h-14 w-14 text-base' };
  return (
    <div className={cn('relative rounded-full overflow-hidden bg-muted shrink-0 flex items-center justify-center font-semibold text-muted-foreground', sizes[size], className)}>
      {src
        ? <img src={src} alt="" className="w-full h-full object-cover" onError={(e) => { e.target.style.display = 'none'; }} />
        : <span>{(fallback ?? '?')[0]?.toUpperCase()}</span>
      }
    </div>
  );
}

// ── Badge ─────────────────────────────────────────────────────────────────────
export function Badge({ children, variant = 'default', className }) {
  const variants = {
    default:   'bg-primary text-primary-foreground',
    secondary: 'bg-muted text-muted-foreground',
    destructive:'bg-destructive text-destructive-foreground',
  };
  return (
    <span className={cn('inline-flex items-center rounded-full px-2 py-0.5 text-xs font-semibold', variants[variant], className)}>
      {children}
    </span>
  );
}

// ── Spinner ───────────────────────────────────────────────────────────────────
export function Spinner({ size = 'md', className }) {
  const sizes = { sm: 'h-4 w-4', md: 'h-6 w-6', lg: 'h-10 w-10' };
  return (
    <svg className={cn('animate-spin text-primary', sizes[size], className)} xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
      <path  className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
    </svg>
  );
}

// ── FullPageLoader ────────────────────────────────────────────────────────────
export function FullPageLoader({ text = 'Loading...' }) {
  return (
    <div className="fixed inset-0 z-50 flex flex-col items-center justify-center gap-3 bg-background">
      <Spinner size="lg" />
      <p className="text-sm text-muted-foreground animate-pulse">{text}</p>
    </div>
  );
}

// ── LoadingButton ─────────────────────────────────────────────────────────────
export function LoadingButton({ loading, children, ...props }) {
  return (
    <Button disabled={loading || props.disabled} {...props}>
      {loading && <Spinner size="sm" className="mr-2" />}
      {children}
    </Button>
  );
}

// ── FormField ─────────────────────────────────────────────────────────────────
export function FormField({ label, error, children }) {
  return (
    <div className="space-y-1.5">
      {label && <label className="text-sm font-medium text-foreground">{label}</label>}
      {children}
      {error && <p className="text-xs text-destructive">{error.message}</p>}
    </div>
  );
}

// ── Skeleton ──────────────────────────────────────────────────────────────────
export function Skeleton({ className }) {
  return <div className={cn('animate-pulse rounded-md bg-muted', className)} />;
}

// ── Switch ────────────────────────────────────────────────────────────────────
export function Switch({ checked, onCheckedChange, className }) {
  return (
    <button
      role="switch"
      aria-checked={checked}
      onClick={() => onCheckedChange?.(!checked)}
      className={cn(
        'relative inline-flex h-6 w-11 shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors',
        checked ? 'bg-primary' : 'bg-muted',
        className
      )}
    >
      <span className={cn('pointer-events-none inline-block h-5 w-5 transform rounded-full bg-white shadow transition-transform', checked ? 'translate-x-5' : 'translate-x-0')} />
    </button>
  );
}
