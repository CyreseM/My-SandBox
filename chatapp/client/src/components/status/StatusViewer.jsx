import { useState, useEffect, useRef, useCallback } from 'react';
import { formatDistanceToNow, differenceInSeconds } from 'date-fns';
import { X, Eye, ChevronLeft, ChevronRight } from 'lucide-react';
import { statusApi } from '@/lib/mutations';
import { Spinner } from '@/components/ui';

const SLIDE_DURATION_MS = 5000;

function formatCountdown(seconds) {
  if (seconds <= 0)    return 'Expiring soon';
  const h = Math.floor(seconds / 3600);
  const m = Math.floor((seconds % 3600) / 60);
  if (h > 0) return `${h}h ${m}m left`;
  if (m > 0) return `${m}m left`;
  return `${seconds}s left`;
}

export function StatusViewer({ statusIds, initialIndex = 0, onClose, onRefresh }) {
  const [index,       setIndex]    = useState(initialIndex);
  const [status,      setStatus]   = useState(null);
  const [loading,     setLoading]  = useState(true);
  const [progress,    setProgress] = useState(0);
  const [viewers,     setViewers]  = useState([]);
  const [showViewers, setShowViewers] = useState(false);
  const [paused,      setPaused]   = useState(false);

  const rafRef    = useRef(null);
  const startRef  = useRef(null);
  const pausedAt  = useRef(null);
  const elapsed   = useRef(0);

  const advance = useCallback(() => {
    if (index < statusIds.length - 1) setIndex(i => i + 1);
    else onClose();
  }, [index, statusIds.length, onClose]);

  const retreat = () => {
    if (index > 0) setIndex(i => i - 1);
  };

  // Load status on index change
  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setProgress(0);
    elapsed.current = 0;
    cancelAnimationFrame(rafRef.current);

    statusApi.getById(statusIds[index])
      .then(data => {
        if (cancelled) return;
        setStatus(data);
        setLoading(false);
        statusApi.markViewed(statusIds[index]).catch(() => {});
        onRefresh?.();
      })
      .catch(() => { if (!cancelled) advance(); });

    return () => { cancelled = true; cancelAnimationFrame(rafRef.current); };
  }, [index]);

  // Progress bar animation
  useEffect(() => {
    if (loading || paused) return;

    startRef.current = Date.now() - elapsed.current;

    const tick = () => {
      const pct = Math.min(((Date.now() - startRef.current) / SLIDE_DURATION_MS) * 100, 100);
      setProgress(pct);
      if (pct < 100) rafRef.current = requestAnimationFrame(tick);
      else advance();
    };

    rafRef.current = requestAnimationFrame(tick);
    return () => cancelAnimationFrame(rafRef.current);
  }, [loading, paused, index]);

  // Pause / resume
  const handlePause = () => {
    elapsed.current = Date.now() - startRef.current;
    cancelAnimationFrame(rafRef.current);
    setPaused(true);
  };
  const handleResume = () => setPaused(false);

  // Countdown
  const timeLeft = status
    ? Math.max(0, differenceInSeconds(new Date(status.expiresAt), new Date()))
    : null;

  const loadViewers = async () => {
    const data = await statusApi.getViewers(statusIds[index]);
    setViewers(data);
    setShowViewers(true);
  };

  // Keyboard nav
  useEffect(() => {
    const handler = (e) => {
      if (e.key === 'ArrowRight') advance();
      if (e.key === 'ArrowLeft')  retreat();
      if (e.key === 'Escape')     onClose();
    };
    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, [advance]);

  return (
    <div className="fixed inset-0 z-50 bg-black flex items-center justify-center select-none"
         onMouseDown={handlePause} onMouseUp={handleResume}
         onTouchStart={handlePause} onTouchEnd={handleResume}>

      {/* ── Progress segments ───────────────────────────────────────────── */}
      <div className="absolute top-3 left-3 right-3 flex gap-1 z-10">
        {statusIds.map((_, i) => (
          <div key={i} className="flex-1 h-0.5 bg-white/25 rounded-full overflow-hidden">
            <div
              className="h-full bg-white rounded-full"
              style={{
                width: i < index ? '100%' : i === index ? `${progress}%` : '0%',
                transition: i === index ? 'none' : undefined,
              }}
            />
          </div>
        ))}
      </div>

      {/* ── Header ─────────────────────────────────────────────────────── */}
      <div className="absolute top-7 left-3 right-3 z-10 flex items-center gap-3">
        {status?.senderAvatarUrl
          ? <img src={status.senderAvatarUrl} className="h-9 w-9 rounded-full object-cover ring-2 ring-white/30" alt="" />
          : <div className="h-9 w-9 rounded-full bg-white/20 flex items-center justify-center text-white font-bold">
              {status?.senderDisplayName?.[0]?.toUpperCase() ?? '?'}
            </div>
        }
        <div className="flex-1 min-w-0">
          <p className="text-white text-sm font-semibold truncate">{status?.senderDisplayName}</p>
          <p className="text-white/60 text-xs">
            {status ? formatDistanceToNow(new Date(status.createdAt), { addSuffix: true }) : ''}
            {timeLeft !== null ? ` · ${formatCountdown(timeLeft)}` : ''}
          </p>
        </div>
        <button onClick={onClose} className="text-white/80 hover:text-white p-1.5 rounded-full hover:bg-white/10 transition-colors">
          <X className="h-5 w-5" />
        </button>
      </div>

      {/* ── Main content ────────────────────────────────────────────────── */}
      {loading ? (
        <Spinner size="lg" className="text-white" />
      ) : (
        <div className="w-full h-full flex items-center justify-center">
          {/* Image */}
          {status?.mediaUrl && status.mediaType === 'Image' && (
            <img src={status.mediaUrl} className="max-h-full max-w-full object-contain" alt="" />
          )}
          {/* Video */}
          {status?.mediaUrl && status.mediaType === 'Video' && (
            <video src={status.mediaUrl} autoPlay loop muted className="max-h-full max-w-full" />
          )}
          {/* Text card */}
          {status?.content && (
            <div
              className="absolute bottom-28 left-8 right-8 text-white text-2xl font-bold text-center drop-shadow-2xl rounded-2xl p-5"
              style={{ backgroundColor: status.backgroundColor ?? 'rgba(0,0,0,0.4)', backdropFilter: 'blur(4px)' }}
            >
              {status.content}
            </div>
          )}
        </div>
      )}

      {/* ── Tap zones ───────────────────────────────────────────────────── */}
      <button className="absolute left-0 top-16 bottom-28 w-1/3 z-10 flex items-center justify-start pl-2"
              onClick={(e) => { e.stopPropagation(); retreat(); }}>
        {index > 0 && <ChevronLeft className="h-8 w-8 text-white/50" />}
      </button>
      <button className="absolute right-0 top-16 bottom-28 w-1/3 z-10 flex items-center justify-end pr-2"
              onClick={(e) => { e.stopPropagation(); advance(); }}>
        <ChevronRight className="h-8 w-8 text-white/50" />
      </button>

      {/* ── Viewer count (own statuses only) ────────────────────────────── */}
      {status?.isOwn && !showViewers && (
        <button
          onClick={(e) => { e.stopPropagation(); loadViewers(); }}
          className="absolute bottom-8 left-1/2 -translate-x-1/2 flex items-center gap-2 text-white/70 hover:text-white text-sm z-10 bg-black/30 rounded-full px-4 py-2"
        >
          <Eye className="h-4 w-4" />
          {status.viewCount} {status.viewCount === 1 ? 'viewer' : 'viewers'}
        </button>
      )}

      {/* ── Viewers drawer ──────────────────────────────────────────────── */}
      {showViewers && (
        <div className="absolute bottom-0 left-0 right-0 bg-card rounded-t-3xl p-5 max-h-80 overflow-y-auto z-20 animate-slide-in">
          <div className="flex items-center justify-between mb-4">
            <h3 className="font-semibold text-foreground">Viewed by</h3>
            <button onClick={() => setShowViewers(false)} className="text-muted-foreground hover:text-foreground p-1">
              <X className="h-4 w-4" />
            </button>
          </div>
          {viewers.length === 0
            ? <p className="text-sm text-muted-foreground text-center py-4">No views yet</p>
            : viewers.map((v) => (
                <div key={v.id} className="flex items-center gap-3 py-2.5">
                  <div className="h-9 w-9 rounded-full bg-muted flex items-center justify-center text-sm font-bold shrink-0">
                    {v.displayName?.[0]?.toUpperCase()}
                  </div>
                  <p className="text-sm font-medium text-foreground">{v.displayName}</p>
                </div>
              ))
          }
        </div>
      )}
    </div>
  );
}
