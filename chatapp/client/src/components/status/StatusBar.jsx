import { useState } from 'react';
import { Plus } from 'lucide-react';
import { useStatuses, useCurrentUser } from '@/hooks';
import { StatusRing } from './StatusRing';
import { StatusViewer } from './StatusViewer';
import { PostStatusDialog } from './PostStatusDialog';
import { Skeleton } from '@/components/ui';

function AvatarCircle({ src, name, size = 52 }) {
  return (
    <div
      className="rounded-full bg-muted overflow-hidden flex items-center justify-center text-sm font-bold text-muted-foreground shrink-0"
      style={{ width: size, height: size }}
    >
      {src
        ? <img src={src} className="w-full h-full object-cover" alt={name} />
        : <span>{name?.[0]?.toUpperCase() ?? '?'}</span>
      }
    </div>
  );
}

export function StatusBar() {
  const { statusGroups, isLoading, mutate } = useStatuses();
  const { user: me }                         = useCurrentUser();
  const [viewing, setViewing]                = useState(null); // { statusIds, index }

  return (
    <div className="border-b border-border shrink-0">
      <div className="flex gap-3 px-3 py-3 overflow-x-auto scrollbar-hide">

        {/* ── My status / add button ───────────────────────────────────── */}
        <PostStatusDialog onPosted={mutate}>
          <button className="flex flex-col items-center gap-1.5 shrink-0 group">
            <div className="relative">
              <StatusRing noRing size={48}>
                <AvatarCircle src={me?.avatarUrl} name={me?.displayName ?? 'Me'} size={48} />
              </StatusRing>
              <span className="absolute bottom-0 right-0 h-5 w-5 rounded-full bg-primary flex items-center justify-center border-2 border-background shadow-sm group-hover:scale-110 transition-transform">
                <Plus className="h-3 w-3 text-primary-foreground" />
              </span>
            </div>
            <span className="text-[10px] text-muted-foreground font-medium w-14 text-center truncate">
              My Status
            </span>
          </button>
        </PostStatusDialog>

        {/* ── Loading skeletons ───────────────────────────────────────── */}
        {isLoading && Array.from({ length: 4 }).map((_, i) => (
          <div key={i} className="flex flex-col items-center gap-1.5 shrink-0">
            <Skeleton className="h-[57px] w-[57px] rounded-full" />
            <Skeleton className="h-2.5 w-12" />
          </div>
        ))}

        {/* ── Contact status rings ─────────────────────────────────────── */}
        {statusGroups.map((group) => (
          <button
            key={group.userId}
            onClick={() => setViewing({ statusIds: group.statusIds, index: 0 })}
            className="flex flex-col items-center gap-1.5 shrink-0 group"
          >
            <StatusRing hasUnviewed={group.hasUnviewed} size={48}>
              <AvatarCircle src={group.avatarUrl} name={group.displayName} size={48} />
            </StatusRing>
            <span className="text-[10px] text-muted-foreground w-14 text-center truncate font-medium">
              {group.displayName}
            </span>
          </button>
        ))}

        {/* Empty state when no contacts have statuses */}
        {!isLoading && statusGroups.length === 0 && (
          <div className="flex items-center self-center ml-2">
            <p className="text-xs text-muted-foreground">No recent statuses</p>
          </div>
        )}
      </div>

      {viewing && (
        <StatusViewer
          statusIds={viewing.statusIds}
          initialIndex={viewing.index}
          onClose={() => setViewing(null)}
          onRefresh={mutate}
        />
      )}
    </div>
  );
}
