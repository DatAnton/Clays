import { apiFetch } from './apiClient'

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
