import { useRef, useCallback, useEffect } from 'react'
import { useAppStore } from '../store/useAppStore'

export const PARTICIPANT_COLORS = [
  '#e63946','#2a9d8f','#e9c46a','#264653','#f4a261',
  '#6a4c93','#1982c4','#ff595e','#6a994e','#bc4749',
]

export function useCanvas({ onStrokeComplete, onStrokeInProgress, onCursorMove }) {
  const canvasRef  = useRef(null)
  const overlayRef = useRef(null)
  const isDrawing  = useRef(false)
  const stroke     = useRef(null)
  const lastBcast  = useRef(0)
  const cursors    = useRef(new Map())
  const cursorTimers = useRef(new Map())

  const getStore = () => useAppStore.getState()
  const ctx    = useCallback(() => canvasRef.current?.getContext('2d'),  [])
  const octx   = useCallback(() => overlayRef.current?.getContext('2d'), [])

  const canvasPoint = useCallback((e) => {
    const c = canvasRef.current
    if (!c) return { x: 0, y: 0 }
    const r = c.getBoundingClientRect()
    return {
      x: (e.clientX - r.left) * (c.width  / r.width),
      y: (e.clientY - r.top)  * (c.height / r.height),
    }
  }, [])

  // ── Render remote cursors ─────────────────────────────────────────────────
  const renderCursors = useCallback(() => {
    const oc = octx()
    const ol = overlayRef.current
    if (!oc || !ol) return
    oc.clearRect(0, 0, ol.width, ol.height)
    const me = getStore().userId
    cursors.current.forEach((cur) => {
      if (cur.userId === me) return
      oc.save()
      oc.fillStyle = cur.color || '#e63946'
      oc.beginPath(); oc.arc(cur.x, cur.y, 6, 0, Math.PI * 2); oc.fill()
      oc.font = "bold 11px 'DM Sans',sans-serif"
      oc.fillText(cur.username, cur.x + 10, cur.y - 4)
      oc.restore()
    })
  }, [octx])

  // ── Draw a stroke onto a context ──────────────────────────────────────────
  const drawStroke = useCallback((c, s) => {
    if (!s.points?.length) return
    c.save()
    if (s.tool === 'eraser') {
      c.globalCompositeOperation = 'destination-out'
      c.strokeStyle = 'rgba(0,0,0,1)'
    } else {
      c.globalCompositeOperation = 'source-over'
      c.strokeStyle = s.color
      c.fillStyle   = s.color
    }
    c.lineWidth  = s.lineWidth
    c.lineCap    = 'round'
    c.lineJoin   = 'round'
    const pts = s.points

    if (s.tool === 'pen' || s.tool === 'eraser') {
      c.beginPath(); c.moveTo(pts[0].x, pts[0].y)
      for (let i = 1; i < pts.length; i++) {
        if (i < pts.length - 1) {
          const mx = (pts[i].x + pts[i+1].x) / 2
          const my = (pts[i].y + pts[i+1].y) / 2
          c.quadraticCurveTo(pts[i].x, pts[i].y, mx, my)
        } else { c.lineTo(pts[i].x, pts[i].y) }
      }
      c.stroke()
    } else if (s.tool === 'line' && pts.length >= 2) {
      const last = pts[pts.length - 1]
      c.beginPath(); c.moveTo(pts[0].x, pts[0].y); c.lineTo(last.x, last.y); c.stroke()
    } else if (s.tool === 'rectangle' && pts.length >= 2) {
      const last = pts[pts.length - 1]
      const x = Math.min(pts[0].x, last.x), y = Math.min(pts[0].y, last.y)
      const w = Math.abs(last.x - pts[0].x),  h = Math.abs(last.y - pts[0].y)
      s.fillShape ? c.fillRect(x, y, w, h) : c.strokeRect(x, y, w, h)
    } else if (s.tool === 'ellipse' && pts.length >= 2) {
      const last = pts[pts.length - 1]
      const cx = (pts[0].x + last.x) / 2, cy = (pts[0].y + last.y) / 2
      const rx = Math.abs(last.x - pts[0].x) / 2, ry = Math.abs(last.y - pts[0].y) / 2
      c.beginPath(); c.ellipse(cx, cy, rx, ry, 0, 0, Math.PI * 2)
      s.fillShape ? c.fill() : c.stroke()
    } else if (s.tool === 'text' && s.text && pts.length) {
      c.globalCompositeOperation = 'source-over'
      c.font = `${s.lineWidth * 4 + 10}px 'DM Sans',sans-serif`
      c.fillStyle = s.color
      c.fillText(s.text, pts[0].x, pts[0].y)
    }
    c.restore()
  }, [])

  // ── Flood fill ────────────────────────────────────────────────────────────
  const floodFill = useCallback((sx, sy, fillColor) => {
    const canvas = canvasRef.current
    const c = ctx()
    if (!canvas || !c) return
    const imgData = c.getImageData(0, 0, canvas.width, canvas.height)
    const d = imgData.data, W = canvas.width, H = canvas.height
    const getP = (x, y) => { const i=(y*W+x)*4; return [d[i],d[i+1],d[i+2],d[i+3]] }
    const setP = (x, y, r, g, b, a) => { const i=(y*W+x)*4; d[i]=r;d[i+1]=g;d[i+2]=b;d[i+3]=a }
    const hex = fillColor.replace('#','')
    const fr=parseInt(hex.slice(0,2),16), fg=parseInt(hex.slice(2,4),16), fb=parseInt(hex.slice(4,6),16)
    const target = getP(Math.round(sx), Math.round(sy))
    if (target[0]===fr&&target[1]===fg&&target[2]===fb) return
    const match = (x, y) => {
      const c = getP(x,y)
      return Math.abs(c[0]-target[0])<30 && Math.abs(c[1]-target[1])<30 &&
             Math.abs(c[2]-target[2])<30 && Math.abs(c[3]-target[3])<30
    }
    const stack = [[Math.round(sx), Math.round(sy)]]
    const visited = new Set()
    while (stack.length) {
      const [x, y] = stack.pop()
      const key = `${x},${y}`
      if (visited.has(key)||x<0||x>=W||y<0||y>=H) continue
      if (!match(x,y)) continue
      visited.add(key)
      setP(x, y, fr, fg, fb, 255)
      stack.push([x+1,y],[x-1,y],[x,y+1],[x,y-1])
    }
    c.putImageData(imgData, 0, 0)
  }, [ctx])

  // ── Apply a remote stroke ─────────────────────────────────────────────────
  const applyRemoteStroke = useCallback((s) => {
    const c = ctx()
    if (!c) return
    if (s.tool === 'fill') floodFill(s.points[0].x, s.points[0].y, s.color)
    else drawStroke(c, s)
  }, [ctx, drawStroke, floodFill])

  // ── Replay full history ───────────────────────────────────────────────────
  const replayStrokes = useCallback((strokes) => {
    const c = ctx(); const canvas = canvasRef.current
    if (!c || !canvas) return
    c.clearRect(0, 0, canvas.width, canvas.height)
    c.fillStyle = '#ffffff'; c.fillRect(0, 0, canvas.width, canvas.height)
    for (const s of strokes) {
      if (s.tool === 'fill') floodFill(s.points[0].x, s.points[0].y, s.color)
      else drawStroke(c, s)
    }
  }, [ctx, drawStroke, floodFill])

  // ── Clear canvas ──────────────────────────────────────────────────────────
  const clearCanvas = useCallback(() => {
    const c = ctx(); const canvas = canvasRef.current
    if (!c || !canvas) return
    c.clearRect(0, 0, canvas.width, canvas.height)
    c.fillStyle = '#ffffff'; c.fillRect(0, 0, canvas.width, canvas.height)
  }, [ctx])

  // ── Update a remote cursor ────────────────────────────────────────────────
  const updateCursor = useCallback((cursor) => {
    cursors.current.set(cursor.userId, cursor)
    const old = cursorTimers.current.get(cursor.userId)
    if (old) clearTimeout(old)
    cursorTimers.current.set(cursor.userId, setTimeout(() => {
      cursors.current.delete(cursor.userId); renderCursors()
    }, 3000))
    renderCursors()
  }, [renderCursors])

  // ── Pointer handlers ──────────────────────────────────────────────────────
  const handlePointerDown = useCallback((e) => {
    const canvas = canvasRef.current; if (!canvas) return
    canvas.setPointerCapture(e.pointerId)
    const pt = canvasPoint(e)
    const { activeTool, color, lineWidth, fillShape } = getStore()

    if (activeTool === 'fill') {
      floodFill(pt.x, pt.y, color)
      onStrokeComplete?.({ tool:'fill', color, lineWidth, points:[pt], timestamp: new Date().toISOString() })
      return
    }
    isDrawing.current = true
    stroke.current = { tool: activeTool, color, lineWidth, fillShape, points: [pt], timestamp: new Date().toISOString() }
  }, [canvasPoint, floodFill, onStrokeComplete])

  const handlePointerMove = useCallback((e) => {
    const pt  = canvasPoint(e)
    const now = Date.now()
    if (now - lastBcast.current > 50) { onCursorMove?.(pt.x, pt.y); lastBcast.current = now }
    if (!isDrawing.current || !stroke.current) return

    stroke.current.points.push(pt)
    const c = ctx(); if (!c) return

    if (['line','rectangle','ellipse'].includes(stroke.current.tool)) {
      const oc = octx(); const ol = overlayRef.current
      if (oc && ol) { oc.clearRect(0,0,ol.width,ol.height); renderCursors(); drawStroke(oc, stroke.current) }
    } else {
      const pts = stroke.current.points
      c.save()
      if (stroke.current.tool === 'eraser') c.globalCompositeOperation = 'destination-out'
      c.strokeStyle = stroke.current.color; c.lineWidth = stroke.current.lineWidth
      c.lineCap = 'round'; c.lineJoin = 'round'
      c.beginPath()
      if (pts.length >= 3) {
        const l = pts.slice(-3)
        c.moveTo((l[0].x+l[1].x)/2,(l[0].y+l[1].y)/2)
        c.quadraticCurveTo(l[1].x,l[1].y,(l[1].x+l[2].x)/2,(l[1].y+l[2].y)/2)
      } else { c.moveTo(pts[0].x,pts[0].y); c.lineTo(pt.x,pt.y) }
      c.stroke(); c.restore()
      if (now - lastBcast.current > 16) { onStrokeInProgress?.(stroke.current); lastBcast.current = now }
    }
  }, [canvasPoint, ctx, octx, drawStroke, renderCursors, onCursorMove, onStrokeInProgress])

  const handlePointerUp = useCallback(() => {
    if (!isDrawing.current || !stroke.current) return
    isDrawing.current = false
    const s = stroke.current; stroke.current = null
    if (['line','rectangle','ellipse'].includes(s.tool)) {
      const c = ctx(); const oc = octx(); const ol = overlayRef.current
      if (c) drawStroke(c, s)
      if (oc && ol) { oc.clearRect(0,0,ol.width,ol.height); renderCursors() }
    }
    onStrokeComplete?.(s)
  }, [ctx, octx, drawStroke, renderCursors, onStrokeComplete])

  // White background on mount
  useEffect(() => {
    const c = ctx(); const canvas = canvasRef.current
    if (!c || !canvas) return
    c.fillStyle = '#ffffff'; c.fillRect(0,0,canvas.width,canvas.height)
  }, [ctx])

  const getSnapshot = useCallback(() => canvasRef.current?.toDataURL('image/png') ?? '', [])

  return {
    canvasRef, overlayRef,
    handlePointerDown, handlePointerMove, handlePointerUp,
    applyRemoteStroke, replayStrokes, clearCanvas, updateCursor, getSnapshot,
  }
}
