export type User = { id: number; username: string; displayName: string; role: 'SuperAdmin' | 'SchoolAdmin' | 'Viewer'; schoolId: number | null }
export type LoginResponse = { token: string; expiresAt: string; user: User }
export type ClassSection = { id: number; number: number; name: string }
export type ClassLevel = { id: number; code: string; name: string; sortOrder: number; sections: ClassSection[] }

const TOKEN_KEY = 'es.token'
const USER_KEY = 'es.user'

export class ApiError extends Error {
  constructor(public status: number, message: string) { super(message) }
}

export const session = {
  get token() { return localStorage.getItem(TOKEN_KEY) },
  get user(): User | null {
    const raw = localStorage.getItem(USER_KEY)
    return raw ? (JSON.parse(raw) as User) : null
  },
  save(res: LoginResponse) {
    localStorage.setItem(TOKEN_KEY, res.token)
    localStorage.setItem(USER_KEY, JSON.stringify(res.user))
  },
  clear() {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
  },
}

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers)
  headers.set('Accept', 'application/json')
  if (init.body) headers.set('Content-Type', 'application/json; charset=utf-8')
  if (session.token) headers.set('Authorization', `Bearer ${session.token}`)

  const res = await fetch(path, { ...init, headers })
  if (!res.ok) {
    let message = `เกิดข้อผิดพลาด (${res.status})`
    try {
      const body = await res.json()
      if (body?.detail) message = body.detail
      else if (body?.title) message = body.title
    } catch { /* ไม่มี body */ }
    if (res.status === 403) message = 'ไม่มีสิทธิ์เข้าถึงข้อมูลนี้'
    throw new ApiError(res.status, message)
  }
  return (await res.json()) as T
}

export const api = {
  login: (username: string, password: string) =>
    request<LoginResponse>('/api/auth/login', { method: 'POST', body: JSON.stringify({ username, password }) }),
  classLevels: (schoolId: number) => request<ClassLevel[]>(`/api/schools/${schoolId}/classlevels`),
}

export const roleLabel: Record<User['role'], string> = {
  SuperAdmin: 'ผู้ดูแลระบบกลาง',
  SchoolAdmin: 'ผู้ดูแลโรงเรียน',
  Viewer: 'ผู้ชม',
}
