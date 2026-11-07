import { apiFetch } from './apiClient'

export type RoleName = 'Admin' | 'Staff' | 'ProjectWorker'
export interface Role { id: number; name: RoleName }
export interface ManagedUser {
  id: number
  email: string
  firstName: string | null
  lastName: string | null
  userName: string | null
  roles: Role[]
}
export interface UserPage { items: ManagedUser[]; page: number; pageSize: number; totalCount: number }
export interface UpdateUserPayload {
  firstName: string | null
  lastName: string | null
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const headers = new Headers(init?.headers)
  if (init?.body != null && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }
  const response = await apiFetch(path, {
    ...init,
    headers,
  })
  if (!response.ok) {
    const problem = await response.json().catch(() => null) as {
      title?: string; message?: string; errors?: Record<string, string[]>
    } | null
    const details = Object.values(problem?.errors ?? {}).flat().join(' ')
    throw new Error(details || problem?.title || problem?.message || `Request failed (${response.status})`)
  }
  return response.json() as Promise<T>
}

export function getCurrentUserAccess(signal?: AbortSignal): Promise<{ isAdmin: boolean }> {
  return request('/api/users/me', { signal })
}

export function getUsers(page: number, search: string, signal?: AbortSignal): Promise<UserPage> {
  const params = new URLSearchParams({ page: String(page) })
  if (search.trim()) params.set('search', search.trim())
  return request(`/api/users?${params}`, { signal })
}

export function getRoles(signal?: AbortSignal): Promise<Role[]> {
  return request('/api/users/roles', { signal })
}

export function updateUser(id: number, payload: UpdateUserPayload): Promise<{ id: number }> {
  return request(`/api/users/${id}`, { method: 'PUT', body: JSON.stringify(payload) })
}

export function assignUserRoles(user: ManagedUser, roleIds: number[]): Promise<{ roles: Role[] }> {
  return request(`/api/users/${user.id}/roles`, {
    method: 'PUT',
    body: JSON.stringify({ roleIds, expectedRoleIds: user.roles.map(role => role.id) }),
  })
}

export interface CreateUserPayload {
  email: string
  firstName: string
  lastName: string
}

export async function createUser(payload: CreateUserPayload): Promise<void> {
  const response = await apiFetch('/api/users', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  if (!response.ok) {
    const problem = await response.json().catch(() => null) as { title?: string } | null
    throw new Error(problem?.title ?? `Unable to create user (${response.status})`)
  }
}
