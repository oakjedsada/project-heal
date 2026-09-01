import type { components } from '../../api/schema'

type FlowTransitionDto = components['schemas']['FlowTransitionDto']
type InstrumentSummaryDto = components['schemas']['InstrumentSummaryDto']

interface FlowDiagramProps {
  instruments: InstrumentSummaryDto[]
  transitions: FlowTransitionDto[]
}

interface NodeLayout {
  key: string
  label: string
  x: number
  y: number
}

const COLUMN_WIDTH = 200
const ROW_HEIGHT = 90
const NODE_WIDTH = 140
const NODE_HEIGHT = 48

function conditionLabel(t: FlowTransitionDto): string {
  switch (t.conditionType) {
    case 'Always':
      return 'เสมอ'
    case 'ScoreLevelEquals':
      return `level = "${t.conditionValue}"`
    case 'QuestionScoreAtLeast':
      return `Q${t.questionId} ≥ ${t.conditionValue}`
    default:
      return t.conditionType ?? ''
  }
}

export function FlowDiagram({ instruments, transitions }: FlowDiagramProps) {
  const START_KEY = '__start__'

  const byFromKey = new Map<string, FlowTransitionDto[]>()
  for (const t of transitions) {
    const key = t.fromInstrumentId == null ? START_KEY : `i-${t.fromInstrumentId}`
    const list = byFromKey.get(key) ?? []
    list.push(t)
    byFromKey.set(key, list)
  }

  const depthByKey = new Map<string, number>();
  const rowsAtDepth: Record<number, number> = {}
  const nodes = new Map<string, NodeLayout>()

  function place(key: string, label: string, depth: number) {
    if (depthByKey.has(key)) return
    depthByKey.set(key, depth)
    const row = rowsAtDepth[depth] ?? 0
    rowsAtDepth[depth] = row + 1
    nodes.set(key, {
      key,
      label,
      x: depth * COLUMN_WIDTH + 20,
      y: row * ROW_HEIGHT + 20,
    })
  }

  place(START_KEY, 'เริ่มต้น session', 0)

  const queue = [START_KEY]
  while (queue.length > 0) {
    const currentKey = queue.shift()!
    const currentDepth = depthByKey.get(currentKey)!
    const outgoing = byFromKey.get(currentKey) ?? []
    for (const t of outgoing) {
      const toKey = `i-${t.toInstrumentId}`
      if (!depthByKey.has(toKey)) {
        const instrument = instruments.find((i) => i.instrumentId === t.toInstrumentId)
        place(toKey, instrument?.code ?? `#${t.toInstrumentId}`, currentDepth + 1)
        queue.push(toKey)
      }
    }
  }

  // Any instrument never reached by a transition still gets shown, off to
  // the side, so a newly-created-but-not-yet-wired instrument isn't invisible.
  const maxDepth = Math.max(0, ...Array.from(depthByKey.values()))
  for (const instrument of instruments) {
    const key = `i-${instrument.instrumentId}`
    if (!depthByKey.has(key)) {
      place(key, instrument.code, maxDepth + 1)
    }
  }

  const width = (maxDepth + 2) * COLUMN_WIDTH + NODE_WIDTH
  const height = Math.max(200, (Math.max(...Object.values(rowsAtDepth), 1)) * ROW_HEIGHT + 60)

  return (
    <svg
      viewBox={`0 0 ${width} ${height}`}
      className="w-full rounded-lg border border-slate-200 bg-white"
      role="img"
      aria-label="แผนภาพเส้นทางแบบประเมิน"
    >
      <defs>
        <marker id="arrow" markerWidth="8" markerHeight="8" refX="7" refY="4" orient="auto">
          <path d="M0,0 L8,4 L0,8 z" fill="#1d4ed8" />
        </marker>
      </defs>

      {transitions.map((t) => {
        const fromKey = t.fromInstrumentId == null ? START_KEY : `i-${t.fromInstrumentId}`
        const toKey = `i-${t.toInstrumentId}`
        const from = nodes.get(fromKey)
        const to = nodes.get(toKey)
        if (!from || !to) return null

        const x1 = from.x + NODE_WIDTH
        const y1 = from.y + NODE_HEIGHT / 2
        const x2 = to.x
        const y2 = to.y + NODE_HEIGHT / 2
        const midX = (x1 + x2) / 2
        const midY = (y1 + y2) / 2

        return (
          <g key={t.id}>
            <path
              d={`M ${x1} ${y1} C ${midX} ${y1}, ${midX} ${y2}, ${x2} ${y2}`}
              fill="none"
              stroke="#1d4ed8"
              strokeWidth={2}
              markerEnd="url(#arrow)"
            />
            <rect x={midX - 55} y={midY - 10} width={110} height={20} fill="white" opacity={0.9} />
            <text x={midX} y={midY + 4} textAnchor="middle" fontSize="11" fill="#1e293b">
              {conditionLabel(t)}
            </text>
          </g>
        )
      })}

      {Array.from(nodes.values()).map((node) => (
        <g key={node.key}>
          <rect
            x={node.x}
            y={node.y}
            width={NODE_WIDTH}
            height={NODE_HEIGHT}
            rx={8}
            fill={node.key === START_KEY ? '#0f172a' : '#eff6ff'}
            stroke={node.key === START_KEY ? '#0f172a' : '#1d4ed8'}
            strokeWidth={1.5}
          />
          <text
            x={node.x + NODE_WIDTH / 2}
            y={node.y + NODE_HEIGHT / 2 + 4}
            textAnchor="middle"
            fontSize="13"
            fontWeight={600}
            fill={node.key === START_KEY ? 'white' : '#1e3a8a'}
          >
            {node.label}
          </text>
        </g>
      ))}
    </svg>
  )
}
