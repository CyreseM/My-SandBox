import { useState } from 'react';
import { X } from 'lucide-react';
import { useUserSearch } from '@/hooks';
import { Input, Spinner, Badge } from '@/components/ui';
import { OnlineAvatar } from './OnlineAvatar';

export function MemberPicker({ selectedIds, onToggle, excludeIds = [] }) {
  const [query, setQuery] = useState('');
  const { results, isLoading } = useUserSearch(query);
  const filtered = results.filter((u) => !excludeIds.includes(u.id));

  return (
    <div className="space-y-2">
      <Input value={query} onChange={(e) => setQuery(e.target.value)} placeholder="Search people…" />

      {selectedIds.length > 0 && (
        <div className="flex flex-wrap gap-1.5">
          {selectedIds.map((id) => {
            const user = results.find((u) => u.id === id);
            return (
              <Badge key={id} variant="secondary" className="gap-1 pl-2 pr-1">
                {user?.displayName ?? id.slice(0, 8)}
                <button onClick={() => onToggle(id)} className="hover:text-destructive ml-0.5">
                  <X className="h-3 w-3" />
                </button>
              </Badge>
            );
          })}
        </div>
      )}

      {query.length >= 2 && (
        <div className="rounded-xl border border-border bg-muted/30 max-h-48 overflow-y-auto">
          {isLoading && <div className="flex justify-center p-3"><Spinner size="sm" /></div>}
          {!isLoading && filtered.length === 0 && (
            <p className="p-3 text-sm text-center text-muted-foreground">No users found</p>
          )}
          {filtered.map((user) => {
            const selected = selectedIds.includes(user.id);
            return (
              <button key={user.id} type="button" onClick={() => onToggle(user.id)}
                className={`w-full flex items-center gap-3 px-3 py-2.5 text-left transition-colors hover:bg-accent ${selected ? 'bg-accent/50' : ''}`}>
                <OnlineAvatar user={user} size="sm" />
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium truncate">{user.displayName}</p>
                  <p className="text-xs text-muted-foreground">@{user.username}</p>
                </div>
                {selected && <span className="text-xs text-primary font-semibold">✓</span>}
              </button>
            );
          })}
        </div>
      )}
    </div>
  );
}
