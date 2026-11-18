import { type FormEvent, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation } from '@tanstack/react-query'
import { api, session } from '../lib/api'
import { th } from '../i18n/th'

export default function LoginPage() {
  const navigate = useNavigate()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')

  const login = useMutation({
    mutationFn: () => api.login(username, password),
    onSuccess: res => {
      session.save(res)
      navigate('/', { replace: true })
    },
  })

  function onSubmit(e: FormEvent) {
    e.preventDefault()
    login.mutate()
  }

  const error = login.error ? (login.error instanceof Error ? login.error.message : th.login.failed) : null

  return (
    <main className="min-h-screen flex items-center justify-center bg-slate-100 px-4">
      <form onSubmit={onSubmit} className="w-full max-w-sm rounded-2xl bg-white p-8 shadow-md space-y-5" aria-label={th.login.title}>
        <div className="text-center space-y-1">
          <h1 className="text-2xl font-semibold text-slate-800">{th.appName}</h1>
          <p className="text-slate-500">{th.login.subtitle}</p>
        </div>

        <label className="block space-y-1">
          <span className="text-sm text-slate-700">{th.login.username}</span>
          <input
            name="username" autoComplete="username" required value={username}
            onChange={e => setUsername(e.target.value)}
            className="w-full rounded-lg border border-slate-300 px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-500"
          />
        </label>

        <label className="block space-y-1">
          <span className="text-sm text-slate-700">{th.login.password}</span>
          <input
            name="password" type="password" autoComplete="current-password" required value={password}
            onChange={e => setPassword(e.target.value)}
            className="w-full rounded-lg border border-slate-300 px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-500"
          />
        </label>

        {error && <p role="alert" className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">{error}</p>}

        <button
          type="submit" disabled={login.isPending}
          className="w-full rounded-lg bg-indigo-600 py-2 font-semibold text-white hover:bg-indigo-700 disabled:opacity-60"
        >
          {login.isPending ? th.login.submitting : th.login.submit}
        </button>
      </form>
    </main>
  )
}
