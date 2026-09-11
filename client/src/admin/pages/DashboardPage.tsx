import { useEffect, useState } from 'react'
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
import { useAuth } from '../../state/AuthContext'
import { useLanguage } from '../../state/LanguageContext'
import { AdminHeader } from '../components/AdminHeader'

type DashboardStatsDto = components['schemas']['DashboardStatsDto']

const BAR_COLORS = ['#ec4899', '#f59e0b', '#8b5cf6', '#0ea5e9', '#65a30d', '#f97316']

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
  const { authHeader } = useAuth()
  const { t } = useLanguage()
  const [stats, setStats] = useState<DashboardStatsDto | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    apiClient
      .GET('/api/admin/dashboard/stats', { headers: authHeader })
      .then(({ data, error: apiError }) => {
        if (apiError || !data) {
          setError(t('admin.dashboard.loadFailed'))
          return
        }
        setStats(data)
      })
      .finally(() => setIsLoading(false))
  }, [authHeader, t])

  const hasBreakdown = (stats?.levelBreakdown?.length ?? 0) > 0
  const hasTrend = (stats?.dailyTrend?.length ?? 0) > 0
  const { rows, levels } = stats ? pivotLevelBreakdown(stats) : { rows: [], levels: [] }

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <AdminHeader title={t('admin.dashboard.title')} />

      {isLoading && <p className="text-stone-500">{t('common.loading')}</p>}
      {error && <p className="rounded-xl bg-red-50 px-3 py-2 text-sm text-red-800">{error}</p>}

      {!isLoading && !error && (
        <div className="flex flex-col gap-6">
          <section className="rounded-2xl border border-pink-100 bg-white p-4 shadow-sm shadow-pink-900/5">
            <h2 className="mb-3 font-medium text-stone-900">{t('admin.dashboard.byLevel')}</h2>
            {hasBreakdown ? (
              <div className="h-72 w-full">
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={rows}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#e7e5e4" />
                    <XAxis dataKey="instrument" stroke="#78716c" />
                    <YAxis allowDecimals={false} stroke="#78716c" />
                    <Tooltip />
                    <Legend />
                    {levels.map((level, i) => (
                      <Bar key={level} dataKey={level} fill={BAR_COLORS[i % BAR_COLORS.length]} radius={[4, 4, 0, 0]} />
                    ))}
                  </BarChart>
                </ResponsiveContainer>
              </div>
            ) : (
              <p className="text-sm text-stone-500">{t('admin.dashboard.noBreakdownData')}</p>
            )}
          </section>

          <section className="rounded-2xl border border-pink-100 bg-white p-4 shadow-sm shadow-pink-900/5">
            <h2 className="mb-3 font-medium text-stone-900">{t('admin.dashboard.dailyTrend')}</h2>
            {hasTrend ? (
              <div className="h-72 w-full">
                <ResponsiveContainer width="100%" height="100%">
                  <LineChart data={stats!.dailyTrend!} margin={{ top: 5, right: 36, bottom: 5, left: 0 }}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#e7e5e4" />
                    <XAxis dataKey="date" stroke="#78716c" interval={0} tick={{ fontSize: 11 }} />
                    <YAxis allowDecimals={false} stroke="#78716c" />
                    <Tooltip />
                    <Line type="monotone" dataKey="sessionCount" stroke="#db2777" strokeWidth={2.5} dot={{ fill: '#db2777' }} />
                  </LineChart>
                </ResponsiveContainer>
              </div>
            ) : (
              <p className="text-sm text-stone-500">{t('admin.dashboard.noTrendData')}</p>
            )}
          </section>
        </div>
      )}
    </div>
  )
}
