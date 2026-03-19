import { useEffect, useRef, useCallback } from 'react'
import { useAppStore } from '../../store/useAppStore'
import { useCanvas } from '../../hooks/useCanvas'
import s from './DrawingCanvas.module.css'

const W = 1400
const H = 900

export default function DrawingCanvas({
  onStrokeComplete,
  onStrokeInProgress,
  onCursorMove,
  remoteStroke,
  remoteCursor,
  clearSignal,
  replayStrokes,
  onSnapshotReady,
}) {
  const activeTool = useAppStore((st) => st.activeTool)
  const color      = useAppStore((st) => st.color)
  const lineWidth  = useAppStore((st) => st.lineWidth)
  const textInputRef      = useRef(null)
  const pendingTextPt     = useRef(null)

  const {
    canvasRef, overlayRef,
    handlePointerDown, handlePointerMove, handlePointerUp,
    applyRemoteStroke, replayStrokes: replayFn, clearCanvas, updateCursor, getSnapshot,
  } = useCanvas({ onStrokeComplete, onStrokeInProgress, onCursorMove })

  /* pass snapshot getter up */
  useEffect(() => { onSnapshotReady?.(getSnapshot) }, [getSnapshot, onSnapshotReady])

  /* remote stroke */
  useEffect(() => { if (remoteStroke) applyRemoteStroke(remoteStroke) }, [remoteStroke])

  /* remote cursor */
  useEffect(() => { if (remoteCursor) updateCursor(remoteCursor) }, [remoteCursor])

  /* clear signal */
  useEffect(() => { if (clearSignal > 0) clearCanvas() }, [clearSignal])

  /* replay on join */
  useEffect(() => { if (replayStrokes?.length) replayFn(replayStrokes) }, [replayStrokes])

  /* attach pointer events */
  useEffect(() => {
    const canvas = canvasRef.current
    if (!canvas) return

    const onDown = (e) => {
      if (useAppStore.getState().activeTool === 'text') {
        const rect = canvas.getBoundingClientRect()
        const scaleX = canvas.width / rect.width
        const scaleY = canvas.height / rect.height
        pendingTextPt.current = {
          x: (e.clientX - rect.left) * scaleX,
          y: (e.clientY - rect.top)  * scaleY,
        }
        const inp = textInputRef.current
        if (inp) {
          inp.style.left    = `${e.clientX - rect.left}px`
          inp.style.top     = `${e.clientY - rect.top - 24}px`
          inp.style.display = 'block'
          inp.value = ''
          inp.focus()
        }
        return
      }
      handlePointerDown(e)
    }

    canvas.addEventListener('pointerdown',  onDown)
    canvas.addEventListener('pointermove',  handlePointerMove)
    canvas.addEventListener('pointerup',    handlePointerUp)
    canvas.addEventListener('pointerleave', handlePointerUp)
    return () => {
      canvas.removeEventListener('pointerdown',  onDown)
      canvas.removeEventListener('pointermove',  handlePointerMove)
      canvas.removeEventListener('pointerup',    handlePointerUp)
      canvas.removeEventListener('pointerleave', handlePointerUp)
    }
  }, [activeTool, handlePointerDown, handlePointerMove, handlePointerUp])

  /* keyboard shortcuts */
  useEffect(() => {
    const map = { p:'pen', e:'eraser', l:'line', r:'rectangle', o:'ellipse', f:'fill', t:'text' }
    const onKey = (e) => {
      if (e.target.tagName === 'INPUT') return
      const tool = map[e.key.toLowerCase()]
      if (tool) useAppStore.getState().setTool(tool)
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [])

  const commitText = useCallback(() => {
    const inp = textInputRef.current
    if (!inp || !pendingTextPt.current) return
    const text = inp.value.trim()
    if (text) {
      const c = canvasRef.current?.getContext('2d')
      if (c) {
        const lw = useAppStore.getState().lineWidth
        const col = useAppStore.getState().color
        c.font = `${lw * 4 + 10}px 'DM Sans',sans-serif`
        c.fillStyle = col
        c.fillText(text, pendingTextPt.current.x, pendingTextPt.current.y)
      }
      onStrokeComplete?.({
        tool: 'text', color, lineWidth, text,
        points: [pendingTextPt.current],
        timestamp: new Date().toISOString(),
      })
    }
    inp.value = ''
    inp.style.display = 'none'
    pendingTextPt.current = null
  }, [color, lineWidth, onStrokeComplete])

  const cursorStyle = { pen:'crosshair', eraser:'cell', fill:'crosshair', text:'text' }[activeTool] || 'crosshair'

  return (
    <div className={s.wrapper} style={{ cursor: cursorStyle }}>
      <canvas ref={canvasRef}  width={W} height={H} className={s.canvas}/>
      <canvas ref={overlayRef} width={W} height={H} className={s.overlay} style={{ pointerEvents:'none' }}/>
      <input
        ref={textInputRef}
        className={s.textInput}
        placeholder="Type, press Enter"
        style={{ display:'none' }}
        onKeyDown={(e) => { if (e.key === 'Enter') commitText(); if (e.key === 'Escape') { textInputRef.current.style.display='none'; pendingTextPt.current=null } }}
        onBlur={commitText}
      />
    </div>
  )
}
