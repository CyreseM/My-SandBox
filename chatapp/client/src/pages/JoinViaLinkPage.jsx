import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { chatApi } from '@/lib/mutations';
import { getConnection } from '@/lib/signalr';
import { FullPageLoader } from '@/components/ui';
import { toast } from 'sonner';

export default function JoinViaLinkPage() {
  const { token } = useParams();
  const navigate  = useNavigate();
  const [error, setError] = useState(null);

  useEffect(() => {
    chatApi.joinViaLink(token)
      .then(async ({ data: chat }) => {
        await getConnection().invoke('JoinChatGroup', chat.id);
        toast.success(`Joined "${chat.name}"!`);
        navigate(`/chat/${chat.id}`, { replace: true });
      })
      .catch(() => setError('Invalid or expired invite link.'));
  }, [token]);

  if (error) return (
    <div className="min-h-screen flex items-center justify-center">
      <p className="text-destructive font-medium text-center px-4">{error}</p>
    </div>
  );
  return <FullPageLoader text="Joining chat…" />;
}
