import { Routes, Route, Navigate } from 'react-router-dom';
import { useEffect } from 'react';
import { useCurrentUser, useChats } from '@/hooks';
import { FullPageLoader } from '@/components/ui';
import { AppShell } from '@/components/chat/AppShell';
import { ChatWindow, EmptyState } from '@/components/chat/ChatWindow';
import LoginPage, { RegisterPage } from '@/pages/LoginPage';
import JoinViaLinkPage from '@/pages/JoinViaLinkPage';

function ProtectedRoute({ children }) {
  const { isAuthenticated, isLoading } = useCurrentUser();
  if (isLoading) return <FullPageLoader text="Checking session…" />;
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  return children;
}

function useNotificationPermission() {
  useEffect(() => {
    if ('Notification' in window && Notification.permission === 'default')
      Notification.requestPermission();
  }, []);
}

function useUnreadTitle() {
  const { chats } = useChats();
  useEffect(() => {
    const total = chats.reduce((sum, c) => sum + (c.unreadCount ?? 0), 0);
    document.title = total > 0 ? `(${total}) ChatApp` : 'ChatApp';
  }, [chats]);
}

function AppRoutes() {
  useNotificationPermission();
  useUnreadTitle();

  return (
    <Routes>
      <Route path="/login"    element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/" element={
        <ProtectedRoute><AppShell /></ProtectedRoute>
      }>
        <Route index              element={<EmptyState />} />
        <Route path="chat/:chatId" element={<ChatWindow />} />
        <Route path="join/:token"  element={<JoinViaLinkPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default AppRoutes;
