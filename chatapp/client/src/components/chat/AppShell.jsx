import { Outlet, useNavigate } from 'react-router-dom';
import { LogOut, Users, Radio, MessageSquare } from 'lucide-react';
import { Button } from '@/components/ui';
import { ChatList } from './ChatList';
import { UserSearchInput } from './UserSearchInput';
import { CreateGroupDialog, CreateChannelDialog } from '@/components/forms/CreateDialogs';
import { StatusBar } from '@/components/status/StatusBar';
import { useSignalR } from '@/hooks/useSignalR';
import { useChats, useCurrentUser } from '@/hooks';
import { withToast } from '@/lib/toast';
import { api } from '@/lib/axios';
import { stopConnection } from '@/lib/signalr';

function Sidebar() {
  const { mutate } = useChats();
  const { user }   = useCurrentUser();
  const navigate   = useNavigate();

  const logout = async () => {
    await withToast(
      async () => { await api.post('/auth/logout'); await stopConnection(); },
      { loading: 'Signing out…', success: 'Goodbye!' }
    );
    navigate('/login', { replace: true });
  };

  return (
    <aside
      className="w-[300px] flex flex-col border-r border-border shrink-0"
      style={{ background: 'hsl(var(--sidebar))' }}
    >
      {/* ── Top bar ──────────────────────────────────────────────────────── */}
      <div className="flex items-center gap-2 px-4 py-3 border-b border-border shrink-0">
        <div className="flex items-center gap-2 flex-1 min-w-0">
          <div className="h-7 w-7 rounded-lg bg-primary/20 flex items-center justify-center shrink-0">
            <MessageSquare className="h-4 w-4 text-primary" />
          </div>
          <span className="font-bold text-sm tracking-tight">ChatApp</span>
        </div>
        <div className="flex items-center gap-0.5 shrink-0">
          <CreateGroupDialog onCreated={mutate}>
            <Button variant="ghost" size="icon" className="h-8 w-8" title="New Group">
              <Users className="h-4 w-4" />
            </Button>
          </CreateGroupDialog>
          <CreateChannelDialog onCreated={mutate}>
            <Button variant="ghost" size="icon" className="h-8 w-8" title="New Channel">
              <Radio className="h-4 w-4" />
            </Button>
          </CreateChannelDialog>
          <Button
            variant="ghost" size="icon"
            className="h-8 w-8 text-muted-foreground"
            onClick={logout} title="Sign out"
          >
            <LogOut className="h-4 w-4" />
          </Button>
        </div>
      </div>

      {/* ── Status stories tray ──────────────────────────────────────────── */}
      <StatusBar />

      {/* ── Search ───────────────────────────────────────────────────────── */}
      <UserSearchInput />

      {/* ── Chat list ────────────────────────────────────────────────────── */}
      <ChatList />

      {/* ── User info footer ─────────────────────────────────────────────── */}
      {user && (
        <div className="px-3 py-2.5 border-t border-border shrink-0">
          <div className="flex items-center gap-2.5 px-2 py-1.5 rounded-xl hover:bg-accent/60 transition-colors">
            <div className="h-8 w-8 rounded-full bg-primary/20 border border-primary/30 flex items-center justify-center text-xs font-bold text-primary shrink-0">
              {user.displayName?.[0]?.toUpperCase()}
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-xs font-semibold truncate">{user.displayName}</p>
              <p className="text-[10px] text-muted-foreground truncate">@{user.username}</p>
            </div>
            <div className="h-2 w-2 rounded-full bg-green-500 shrink-0" title="Online" />
          </div>
        </div>
      )}
    </aside>
  );
}

export function AppShell() {
  useSignalR();
  return (
    <div className="flex h-screen overflow-hidden bg-background">
      <Sidebar />
      <main className="flex-1 flex flex-col min-w-0 overflow-hidden">
        <Outlet />
      </main>
    </div>
  );
}
