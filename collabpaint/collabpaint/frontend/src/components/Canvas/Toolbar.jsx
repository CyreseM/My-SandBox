import { useAppStore } from '../../store/useAppStore'
import s from './Toolbar.module.css'

const TOOLS = [
  { id:'pen',       label:'Pen',       key:'P', icon:<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M12 19l7-7 3 3-7 7-3-3z"/><path d="M18 13l-1.5-7.5L2 2l3.5 14.5L13 18l5-5z"/><circle cx="11" cy="11" r="2"/></svg> },
  { id:'eraser',    label:'Eraser',    key:'E', icon:<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M20 20H7L3 16l10-10 7 7-2.5 2.5"/><path d="M6 17l4-4"/></svg> },
  { id:'line',      label:'Line',      key:'L', icon:<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><line x1="5" y1="19" x2="19" y2="5"/></svg> },
  { id:'rectangle', label:'Rect',      key:'R', icon:<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><rect x="3" y="3" width="18" height="18" rx="2"/></svg> },
  { id:'ellipse',   label:'Ellipse',   key:'O', icon:<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><ellipse cx="12" cy="12" rx="10" ry="7"/></svg> },
  { id:'fill',      label:'Fill',      key:'F', icon:<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M19 11l-8-8-8.5 8.5a5.5 5.5 0 0 0 7.78 7.78L19 11z"/><path d="M20 16.2l.45 2.13A2 2 0 0 1 16.7 20a2.26 2.26 0 0 1 .33-1.06L18 17l1-1 1 .2z"/></svg> },
  { id:'text',      label:'Text',      key:'T', icon:<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><polyline points="4 7 4 4 20 4 20 7"/><line x1="9" y1="20" x2="15" y2="20"/><line x1="12" y1="4" x2="12" y2="20"/></svg> },
]

const PALETTE = [
  '#1a1a2e','#ffffff','#e63946','#f4a261','#e9c46a',
  '#2a9d8f','#264653','#5b5bd6','#7c7ce8','#6a4c93',
  '#ff595e','#ffca3a','#6a994e','#1982c4','#bc4749',
  '#c77dff','#48cae4','#f9c74f','#90be6d','#577590',
]

const SIZES = [2, 4, 8, 14, 22]

export default function Toolbar({ onClear }) {
  const { activeTool, color, lineWidth, fillShape, setTool, setColor, setLineWidth, setFillShape } = useAppStore()

  return (
    <aside className={s.toolbar}>
      {/* Tools */}
      <div className={s.section}>
        <span className={s.sectionLabel}>Tools</span>
        <div className={s.toolGrid}>
          {TOOLS.map((t) => (
            <button
              key={t.id}
              className={`${s.toolBtn} ${activeTool === t.id ? s.active : ''}`}
              onClick={() => setTool(t.id)}
              title={`${t.label} (${t.key})`}
            >
              <span className={s.icon}>{t.icon}</span>
              <span className={s.key}>{t.key}</span>
            </button>
          ))}
        </div>
      </div>

      <div className={s.divider}/>

      {/* Fill toggle for shapes */}
      {(activeTool === 'rectangle' || activeTool === 'ellipse') && <>
        <div className={s.section}>
          <span className={s.sectionLabel}>Shape style</span>
          <div className={s.fillRow}>
            <button className={`${s.fillBtn} ${!fillShape ? s.active : ''}`} onClick={() => setFillShape(false)}>Outline</button>
            <button className={`${s.fillBtn} ${fillShape  ? s.active : ''}`} onClick={() => setFillShape(true)}>Filled</button>
          </div>
        </div>
        <div className={s.divider}/>
      </>}

      {/* Size */}
      <div className={s.section}>
        <span className={s.sectionLabel}>Size — {lineWidth}px</span>
        <div className={s.sizeRow}>
          {SIZES.map((sz) => (
            <button key={sz} className={`${s.sizeBtn} ${lineWidth === sz ? s.active : ''}`} onClick={() => setLineWidth(sz)} title={`${sz}px`}>
              <span className={s.sizeDot} style={{ width: Math.min(sz+4, 22), height: Math.min(sz+4, 22), background: color }}/>
            </button>
          ))}
        </div>
        <input type="range" min="1" max="40" value={lineWidth} onChange={(e) => setLineWidth(Number(e.target.value))} className={s.slider}/>
      </div>

      <div className={s.divider}/>

      {/* Palette */}
      <div className={s.section}>
        <span className={s.sectionLabel}>Color</span>
        <div className={s.palette}>
          {PALETTE.map((c) => (
            <button
              key={c}
              className={`${s.swatch} ${color === c ? s.activeSwatch : ''}`}
              style={{ background: c }}
              onClick={() => setColor(c)}
              title={c}
            />
          ))}
        </div>
        <div className={s.customRow}>
          <div className={s.colorPreview} style={{ background: color }}/>
          <label className={s.colorLabel}>
            <input type="color" value={color} onChange={(e) => setColor(e.target.value)} className={s.colorPicker}/>
            <span className={s.colorHex}>{color}</span>
          </label>
        </div>
      </div>

      <div className={s.divider}/>

      {/* Actions */}
      <div className={s.section}>
        <span className={s.sectionLabel}>Actions</span>
        <button className={`${s.actionBtn} ${s.clearBtn}`} onClick={onClear}>
          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14H6L5 6"/></svg>
          Clear canvas
        </button>
      </div>
    </aside>
  )
}
