import { th } from '../i18n/th'

export type Role = 'SuperAdmin' | 'SchoolAdmin' | 'Viewer'
export type User = { id: number; username: string; displayName: string; role: Role; schoolId: number | null }
export type LoginResponse = { token: string; expiresAt: string; user: User }
export type ClassSection = { id: number; sectionNo: number; name: string }
export type ClassLevel = { id: number; code: number; name: string; startPeriod: number; endPeriod: number; sections: ClassSection[] }

const TOKEN_KEY = 'es.token'
const USER_KEY = 'es.user'

export class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message)
  }
}

export const session = {
  get token(): string | null {
    return localStorage.getItem(TOKEN_KEY)
  },
  get user(): User | null {
    const raw = localStorage.getItem(USER_KEY)
    return raw ? (JSON.parse(raw) as User) : null
  },
  save(res: LoginResponse): void {
    localStorage.setItem(TOKEN_KEY, res.token)
    localStorage.setItem(USER_KEY, JSON.stringify(res.user))
  },
  clear(): void {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
  },
}

type ProblemDetails = { title?: string; detail?: string }

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers)
  headers.set('Accept', 'application/json')
  if (init.body) headers.set('Content-Type', 'application/json; charset=utf-8')
  if (session.token) headers.set('Authorization', `Bearer ${session.token}`)

  const res = await fetch(path, { ...init, headers })
  if (!res.ok) {
    let message: string = th.errors.generic(res.status)
    try {
      const body = (await res.json()) as ProblemDetails
      message = body.detail ?? body.title ?? message
    } catch {
      // ไม่มี body
    }
    if (res.status === 403) message = th.errors.forbidden
    throw new ApiError(res.status, message)
  }
  return (await res.json()) as T
}

export const api = {
  login: (username: string, password: string) =>
    request<LoginResponse>('/api/auth/login', { method: 'POST', body: JSON.stringify({ username, password }) }),
  classLevels: (schoolId: number) => request<ClassLevel[]>(`/api/schools/${schoolId}/classlevels`),
}
