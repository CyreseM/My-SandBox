import { useState, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Search } from 'lucide-react';
import { useUserSearch } from '@/hooks';
import { chatApi } from '@/lib/mutations';
import { withToast } from '@/lib/toast';
import { Input, Spinner } from '@/components/ui';
import { OnlineAvatar } from '@/components/shared/OnlineAvatar';

export function UserSearchInput() {
  const [query, setQuery]   = useState('');
  const [open, setOpen]     = useState(false);
  const { results, isLoading } = useUserSearch(query);
  const navigate = useNavigate();
  const containerRef = useRef();

  useEffect(() => {
    const handler = (e) => {
      if (!containerRef.current?.contains(e.target)) setOpen(false);
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  const openDirect = async (userId) => {
    setQuery(''); setOpen(false);
    const { data: chat } = await withToast(
      () => chatApi.createDirect(userId),
      { loading: 'Opening chat…', success: 'Chat ready!' }
    );
    navigate('/chat/' + chat.id);
  };

  return (
    <div ref={containerRef} className="relative px-3 py-2">
      <div className="relative">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-3.5 w-3.5 text-muted-foreground" />
        <Input
          value={query}
          onChange={(e) => { setQuery(e.target.value); setOpen(true); }}
          onFocus={() => setOpen(true)}
          placeholder="Search people…"
          className="pl-8 h-8 text-sm bg-muted/60 border-0"
        />
      </div>
      {open && query.length >= 2 && (
        <div className="absolute left-3 right-3 top-full z-50 mt-1 rounded-xl border border-border bg-card shadow-2xl overflow-hidden">
          {isLoading && <div className="flex justify-center p-4"><Spinner size="sm" /></div>}
          {!isLoading && results.length === 0 && (
            <p className="p-4 text-sm text-center text-muted-foreground">No users found</p>
          )}
          {results.map((user) => (
            <button
              key={user.id}
              onClick={() => openDirect(user.id)}
              className="w-full flex items-center gap-3 px-4 py-2.5 hover:bg-accent text-left transition-colors"
            >
              <OnlineAvatar user={user} size="sm" />
              <div>
                <p className="text-sm font-medium">{user.displayName}</p>
                <p className="text-xs text-muted-foreground">@{user.username}</p>
              </div>
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
