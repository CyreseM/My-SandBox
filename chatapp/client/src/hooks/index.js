import useSWR from 'swr';
import useSWRInfinite from 'swr/infinite';
import { useMemo, useState, useEffect } from 'react';
import { fetcher } from '@/lib/fetcher';

export function useCurrentUser() {
  const { data, error, isLoading, mutate } = useSWR('/auth/me', fetcher, {
    shouldRetryOnError: false,
  });
  return { user: data, isLoading, isAuthenticated: !!data && !error, mutate };
}

export function useChats() {
  const { data, isLoading, mutate } = useSWR('/chats', fetcher, {
    refreshInterval: 30_000,
  });
  return { chats: data ?? [], isLoading, mutate };
}

export function useChat(chatId) {
  const { data, isLoading, mutate } = useSWR(chatId ? `/chats/${chatId}` : null, fetcher);
  return { chat: data, isLoading, mutate };
}

export function useMessages(chatId) {
  const getKey = (pageIndex, prev) => {
    if (prev && !prev.hasMore) return null;
    const cursor = prev?.cursor ? `&before=${encodeURIComponent(prev.cursor)}` : '';
    return chatId ? `/chats/${chatId}/messages?limit=50${cursor}` : null;
  };
  const { data, size, setSize, isLoading } = useSWRInfinite(getKey, fetcher, {
    revalidateFirstPage: false,
  });
  const allMessages = useMemo(
    () => (data ?? []).flatMap((p) => p.messages).reverse(), [data]
  );
  return {
    allMessages,
    loadMore: () => setSize(size + 1),
    hasMore:  data?.[data.length - 1]?.hasMore ?? false,
    isLoading,
  };
}

export function useUserSearch(query) {
  const debounced = useDebounce(query, 300);
  const { data, isLoading } = useSWR(
    debounced.length >= 2
      ? `/users/search?q=${encodeURIComponent(debounced)}&limit=20`
      : null,
    fetcher
  );
  return { results: data ?? [], isLoading };
}

export function useStatuses() {
  const { data, isLoading, mutate } = useSWR('/statuses', fetcher, {
    refreshInterval: 60_000,
  });
  return { statusGroups: data ?? [], isLoading, mutate };
}

export function useDebounce(value, delay) {
  const [debounced, setDebounced] = useState(value);
  useEffect(() => {
    const t = setTimeout(() => setDebounced(value), delay);
    return () => clearTimeout(t);
  }, [value, delay]);
  return debounced;
}

export function useMembers(chatId, enabled = true) {
  const { data, isLoading, mutate } = useSWR(
    enabled && chatId ? `/chats/${chatId}/members` : null,
    fetcher
  );
  return { members: data ?? [], isLoading, mutate };
}
