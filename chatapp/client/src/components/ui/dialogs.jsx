import * as DialogPrimitive from '@radix-ui/react-dialog';
import { X } from 'lucide-react';
import { cn } from '@/lib/utils';

export const Dialog = DialogPrimitive.Root;
export const DialogTrigger = DialogPrimitive.Trigger;

export function DialogContent({ children, className }) {
  return (
    <DialogPrimitive.Portal>
      <DialogPrimitive.Overlay className="fixed inset-0 z-50 bg-black/60 backdrop-blur-sm data-[state=open]:animate-fade-in" />
      <DialogPrimitive.Content
        className={cn(
          'fixed left-1/2 top-1/2 z-50 -translate-x-1/2 -translate-y-1/2',
          'w-full max-w-md rounded-2xl border border-border bg-card p-6 shadow-2xl',
          'data-[state=open]:animate-fade-in',
          className
        )}
      >
        {children}
        <DialogPrimitive.Close className="absolute right-4 top-4 rounded-lg p-1 text-muted-foreground hover:text-foreground hover:bg-muted transition-colors">
          <X className="h-4 w-4" />
        </DialogPrimitive.Close>
      </DialogPrimitive.Content>
    </DialogPrimitive.Portal>
  );
}

export function DialogHeader({ children }) {
  return <div className="mb-4">{children}</div>;
}

export function DialogTitle({ children }) {
  return <DialogPrimitive.Title className="text-lg font-semibold">{children}</DialogPrimitive.Title>;
}

// Sheet (side panel)
export function Sheet({ open, onOpenChange, children }) {
  return (
    <DialogPrimitive.Root open={open} onOpenChange={onOpenChange}>
      {children}
    </DialogPrimitive.Root>
  );
}

export function SheetContent({ children, className, side = 'right' }) {
  return (
    <DialogPrimitive.Portal>
      <DialogPrimitive.Overlay className="fixed inset-0 z-50 bg-black/60 backdrop-blur-sm" />
      <DialogPrimitive.Content
        className={cn(
          'fixed z-50 bg-card border-l border-border h-full w-80 top-0 shadow-2xl overflow-y-auto',
          'data-[state=open]:animate-slide-in',
          side === 'right' ? 'right-0' : 'left-0',
          className
        )}
      >
        {children}
        <DialogPrimitive.Close className="absolute right-4 top-4 rounded-lg p-1 text-muted-foreground hover:text-foreground hover:bg-muted transition-colors">
          <X className="h-4 w-4" />
        </DialogPrimitive.Close>
      </DialogPrimitive.Content>
    </DialogPrimitive.Portal>
  );
}

export function SheetHeader({ children }) {
  return <div className="px-6 pt-6 pb-4 border-b border-border">{children}</div>;
}

export function SheetTitle({ children }) {
  return <DialogPrimitive.Title className="text-lg font-semibold">{children}</DialogPrimitive.Title>;
}

// Dropdown
import * as DropdownPrimitive from '@radix-ui/react-dropdown-menu';

export const DropdownMenu        = DropdownPrimitive.Root;
export const DropdownMenuTrigger = DropdownPrimitive.Trigger;

export function DropdownMenuContent({ children, align = 'start', className }) {
  return (
    <DropdownPrimitive.Portal>
      <DropdownPrimitive.Content
        align={align}
        sideOffset={4}
        className={cn(
          'z-50 min-w-40 rounded-xl border border-border bg-card p-1 shadow-xl',
          'data-[state=open]:animate-fade-in',
          className
        )}
      >
        {children}
      </DropdownPrimitive.Content>
    </DropdownPrimitive.Portal>
  );
}

export function DropdownMenuItem({ children, onSelect, className }) {
  return (
    <DropdownPrimitive.Item
      onSelect={onSelect}
      className={cn(
        'flex cursor-pointer items-center rounded-lg px-3 py-2 text-sm',
        'hover:bg-accent hover:text-accent-foreground',
        'focus:outline-none focus:bg-accent',
        'transition-colors select-none',
        className
      )}
    >
      {children}
    </DropdownPrimitive.Item>
  );
}

export function DropdownMenuSeparator() {
  return <DropdownPrimitive.Separator className="my-1 h-px bg-border" />;
}
