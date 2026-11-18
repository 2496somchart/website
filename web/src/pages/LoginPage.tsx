import { FormEvent, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api, session } from '../lib/api'

export default function LoginPage() {
  const navigate = useNavigate()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setBusy(true)
    try {
      session.save(await api.login(username, password))
      navigate('/', { replace: true })
    } catch (err) {
      setError(err instanceof Error ? err.message : 'เข้าสู่ระบบไม่สำเร็จ')
    } finally {
      setBusy(false)
    }
  }

  return (
    <main className="min-h-screen flex items-center justify-center bg-slate-100 px-4">
      <form onSubmit={onSubmit} className="w-full max-w-sm rounded-2xl bg-white p-8 shadow-md space-y-5" aria-label="เข้าสู่ระบบ">
        <div className="text-center space-y-1">
          <h1 className="text-2xl font-semibold text-slate-800">EasySchedule</h1>
          <p className="text-slate-500">ระบบจัดตารางสอน — เข้าสู่ระบบ</p>
        </div>

        <label className="block space-y-1">
          <span className="text-sm text-slate-700">ชื่อผู้ใช้</span>
          <input
            name="username" autoComplete="username" required value={username}
            onChange={e => setUsername(e.target.value)}
            className="w-full rounded-lg border border-slate-300 px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-500"
          />
        </label>

        <label className="block space-y-1">
          <span className="text-sm text-slate-700">รหัสผ่าน</span>
          <input
            name="password" type="password" autoComplete="current-password" required value={password}
            onChange={e => setPassword(e.target.value)}
            className="w-full rounded-lg border border-slate-300 px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-500"
          />
        </label>

        {error && <p role="alert" className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">{error}</p>}

        <button
          type="submit" disabled={busy}
          className="w-full rounded-lg bg-indigo-600 py-2 font-semibold text-white hover:bg-indigo-700 disabled:opacity-60"
        >
          {busy ? 'กำลังเข้าสู่ระบบ…' : 'เข้าสู่ระบบ'}
        </button>
      </form>
    </main>
  )
}
