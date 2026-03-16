import { Avatar } from '@/components/ui';
import { useChatStore } from '@/store/chatStore';
import { cn } from '@/lib/utils';

export function OnlineAvatar({ user, size = 'md', className }) {
  const isOnline = useChatStore((s) => s.onlineUsers.has(user?.id));
  const dotSizes = { sm: 'h-2 w-2', md: 'h-2.5 w-2.5', lg: 'h-3.5 w-3.5' };

  return (
    <div className={cn('relative inline-block shrink-0', className)}>
      <Avatar src={user?.avatarUrl} fallback={user?.displayName} size={size} />
      {isOnline && (
        <span className={cn(
          'absolute bottom-0 right-0 rounded-full bg-green-500 border-2 border-background',
          dotSizes[size]
        )} />
      )}
    </div>
  );
}
