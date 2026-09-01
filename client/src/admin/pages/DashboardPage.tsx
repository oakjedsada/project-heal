import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import {
  Bar,
  BarChart,
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import { apiClient } from '../../api/client'
import type { components } from '../../api/schema'
import { useAdminAuth } from '../AdminAuthContext'

type DashboardStatsDto = components['schemas']['DashboardStatsDto']

const BAR_COLORS = ['#1d4ed8', '#0891b2', '#16a34a', '#ca8a04', '#dc2626', '#7c3aed']

function pivotLevelBreakdown(stats: DashboardStatsDto) {
  const items = stats.levelBreakdown ?? []
  const instruments = Array.from(new Set(items.map((i) => i.instrumentCode!)))
  const levels = Array.from(new Set(items.map((i) => i.level!)))

  const rows = instruments.map((code) => {
    const row: Record<string, string | number> = { instrument: code }
    for (const level of levels) {
      row[level] = items.find((i) => i.instrumentCode === code && i.level === level)?.count ?? 0
    }
    return row
  })

  return { rows, levels }
}

export function DashboardPage() {
  const { authHeader } = useAdminAuth()
  const [stats, setStats] = useState<DashboardStatsDto | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    apiClient
      .GET('/api/admin/dashboard/stats', { headers: authHeader })
      .then(({ data, error: apiError }) => {
        if (apiError || !data) {
          setError('โหลดสถิติไม่สำเร็จ')
          return
        }
        setStats(data)
      })
      .finally(() => setIsLoading(false))
  }, [authHeader])

  const hasBreakdown = (stats?.levelBreakdown?.length ?? 0) > 0
  const hasTrend = (stats?.weeklyTrend?.length ?? 0) > 0
  const { rows, levels } = stats ? pivotLevelBreakdown(stats) : { rows: [], levels: [] }

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <div className="mb-4 flex items-center justify-between">
        <h1 className="text-xl font-semibold text-slate-900">สถิติรวม (ไม่ระบุตัวตน)</h1>
        <div className="flex gap-4 text-sm text-blue-700">
          <Link to="/admin/instruments/new" className="underline">
            สร้างแบบประเมิน
          </Link>
          <Link to="/admin/flow-transitions" className="underline">
            เส้นทางแบบประเมิน
          </Link>
        </div>
      </div>

      {isLoading && <p className="text-slate-600">กำลังโหลด...</p>}
      {error && <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-800">{error}</p>}

      {!isLoading && !error && (
        <div className="flex flex-col gap-6">
          <section className="rounded-lg border border-slate-200 bg-white p-4">
            <h2 className="mb-3 font-medium text-slate-900">จำนวน session แยกตามระดับผล</h2>
            {hasBreakdown ? (
              <div className="h-72 w-full">
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={rows}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="instrument" />
                    <YAxis allowDecimals={false} />
                    <Tooltip />
                    <Legend />
                    {levels.map((level, i) => (
                      <Bar key={level} dataKey={level} fill={BAR_COLORS[i % BAR_COLORS.length]} />
                    ))}
                  </BarChart>
                </ResponsiveContainer>
              </div>
            ) : (
              <p className="text-sm text-slate-500">ยังไม่มีข้อมูล session ที่ทำแบบประเมินจนจบ</p>
            )}
          </section>

          <section className="rounded-lg border border-slate-200 bg-white p-4">
            <h2 className="mb-3 font-medium text-slate-900">แนวโน้มจำนวน session รายสัปดาห์</h2>
            {hasTrend ? (
              <div className="h-72 w-full">
                <ResponsiveContainer width="100%" height="100%">
                  <LineChart data={stats!.weeklyTrend!}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="weekStart" />
                    <YAxis allowDecimals={false} />
                    <Tooltip />
                    <Line type="monotone" dataKey="sessionCount" stroke="#1d4ed8" strokeWidth={2} />
                  </LineChart>
                </ResponsiveContainer>
              </div>
            ) : (
              <p className="text-sm text-slate-500">ยังไม่มี session เริ่มต้นเลย</p>
            )}
          </section>
        </div>
      )}
    </div>
  )
}
