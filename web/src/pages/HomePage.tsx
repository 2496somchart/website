import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api, ApiError, ClassLevel, roleLabel, session } from '../lib/api'

export default function HomePage() {
  const navigate = useNavigate()
  const user = session.user
  const [levels, setLevels] = useState<ClassLevel[] | null>(null)
  const [error, setError] = useState<string | null>(null)

  function logout() {
    session.clear()
    navigate('/login', { replace: true })
  }

  useEffect(() => {
    if (!user?.schoolId) {
      setError('บัญชีนี้ไม่ได้ผูกกับโรงเรียน (SuperAdmin จะเลือกโรงเรียนได้ในเวอร์ชันถัดไป)')
      return
    }
    api.classLevels(user.schoolId)
      .then(setLevels)
      .catch(err => {
        if (err instanceof ApiError && err.status === 401) return logout()
        setError(err instanceof Error ? err.message : 'โหลดข้อมูลไม่สำเร็จ')
      })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  return (
    <div className="min-h-screen bg-slate-100">
      <header className="bg-white shadow-sm">
        <div className="mx-auto flex max-w-4xl items-center justify-between px-4 py-3">
          <h1 className="text-lg font-semibold text-slate-800">EasySchedule</h1>
          <div className="flex items-center gap-3 text-sm">
            <span className="text-slate-600">{user?.displayName} ({user ? roleLabel[user.role] : ''})</span>
            <button onClick={logout} className="rounded-lg border border-slate-300 px-3 py-1 hover:bg-slate-50">ออกจากระบบ</button>
          </div>
        </div>
      </header>

      <main className="mx-auto max-w-4xl px-4 py-6">
        <h2 className="mb-4 text-xl font-semibold text-slate-800">ระดับชั้น</h2>
        {error && <p role="alert" className="rounded-lg bg-red-50 px-3 py-2 text-red-700">{error}</p>}
        {!error && levels === null && <p className="text-slate-500">กำลังโหลด…</p>}
        {levels && levels.length === 0 && <p className="text-slate-500">ยังไม่มีระดับชั้น</p>}
        {levels && levels.length > 0 && (
          <ul className="grid gap-3 sm:grid-cols-3" aria-label="รายชื่อระดับชั้น">
            {levels.map(l => (
              <li key={l.id} className="rounded-xl bg-white p-4 shadow-sm" data-testid="class-level">
                <div className="text-lg font-semibold text-indigo-700">{l.name}</div>
                <div className="text-sm text-slate-500">{l.sections.length} ห้องเรียน: {l.sections.map(s => s.name).join(', ')}</div>
              </li>
            ))}
          </ul>
        )}
      </main>
    </div>
  )
}
