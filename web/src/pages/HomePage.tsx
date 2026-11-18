import { useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { api, ApiError, session } from '../lib/api'
import { th } from '../i18n/th'

export default function HomePage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const user = session.user
  const schoolId = user?.schoolId ?? null

  const levels = useQuery({
    queryKey: ['classLevels', schoolId],
    queryFn: () => api.classLevels(schoolId!),
    enabled: schoolId !== null,
  })

  function logout() {
    session.clear()
    queryClient.clear()
    navigate('/login', { replace: true })
  }

  const unauthorized = levels.error instanceof ApiError && levels.error.status === 401
  useEffect(() => {
    if (unauthorized) logout()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [unauthorized])

  const error = schoolId === null
    ? th.classLevels.noSchool
    : levels.error ? (levels.error instanceof Error ? levels.error.message : th.errors.loadFailed) : null

  return (
    <div className="min-h-screen bg-slate-100">
      <header className="bg-white shadow-sm">
        <div className="mx-auto flex max-w-4xl items-center justify-between px-4 py-3">
          <h1 className="text-lg font-semibold text-slate-800">{th.appName}</h1>
          <div className="flex items-center gap-3 text-sm">
            <span className="text-slate-600">{user?.displayName} ({user ? th.roles[user.role] : ''})</span>
            <button onClick={logout} className="rounded-lg border border-slate-300 px-3 py-1 hover:bg-slate-50">{th.header.logout}</button>
          </div>
        </div>
      </header>

      <main className="mx-auto max-w-4xl px-4 py-6">
        <h2 className="mb-4 text-xl font-semibold text-slate-800">{th.classLevels.title}</h2>
        {error && <p role="alert" className="rounded-lg bg-red-50 px-3 py-2 text-red-700">{error}</p>}
        {!error && levels.isLoading && <p className="text-slate-500">{th.classLevels.loading}</p>}
        {levels.data && levels.data.length === 0 && <p className="text-slate-500">{th.classLevels.empty}</p>}
        {levels.data && levels.data.length > 0 && (
          <ul className="grid gap-3 sm:grid-cols-3" aria-label={th.classLevels.listLabel}>
            {levels.data.map(l => (
              <li key={l.id} className="rounded-xl bg-white p-4 shadow-sm" data-testid="class-level">
                <div className="text-lg font-semibold text-indigo-700">{l.name}</div>
                <div className="text-sm text-slate-500">
                  {th.classLevels.sections(l.sections.length, l.sections.map(s => s.name).join(', '))}
                </div>
              </li>
            ))}
          </ul>
        )}
      </main>
    </div>
  )
}
