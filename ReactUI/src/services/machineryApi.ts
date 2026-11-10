import { apiFetch } from './apiClient'

export interface MachineryGridItem {
  id: number
  machineryLibraryId: number
  manufacturerId: number
  manufacturerName: string
  model: string
  year: number | null
  vinNumber: string | null
  registrationNumber: string | null
  countryOfImport: string | null
  purchaseYear: number | null
  status: number | null
  companyId: number | null
  companyName: string | null
  companyCity: string | null
}

export interface MachineryFilters {
  search: string
  manufacturerId: number | undefined
  status: number | undefined
  city: string
}

export interface MachineryPage {
  items: MachineryGridItem[]
  page: number
  pageSize: number
  totalCount: number
}

export async function getMachinery(
  page: number,
  filters: MachineryFilters,
  signal?: AbortSignal,
): Promise<MachineryPage> {
  const params = new URLSearchParams({ page: String(page) })
  if (filters.search.trim()) params.set('search', filters.search.trim())
  if (filters.manufacturerId !== undefined) params.set('manufacturerId', String(filters.manufacturerId))
  if (filters.status !== undefined) params.set('status', String(filters.status))
  if (filters.city.trim()) params.set('city', filters.city.trim())

  const response = await apiFetch(`/api/machinery?${params.toString()}`, {
    signal,
  })
  if (!response.ok) {
    throw new Error(`Unable to load machinery (${response.status})`)
  }

  return response.json() as Promise<MachineryPage>
}

export interface MachineryLibraryItem {
  id: number
  manufacturerId: number
  manufacturerName: string
  model: string
}

export async function getMachineryLibrary(signal?: AbortSignal): Promise<MachineryLibraryItem[]> {
  const response = await apiFetch('/api/machinery-library', { signal })
  if (!response.ok) {
    throw new Error(`Unable to load machinery library (${response.status})`)
  }
  return response.json() as Promise<MachineryLibraryItem[]>
}

export interface MachineryLibraryPayload {
  manufacturerId: number
  model: string
}

async function saveMachineryLibrary(payload: MachineryLibraryPayload, id?: number): Promise<MachineryLibraryItem> {
  const response = await apiFetch(id === undefined ? '/api/machinery-library' : `/api/machinery-library/${id}`, {
    method: id === undefined ? 'POST' : 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ ...payload, model: payload.model.trim() }),
  })
  if (!response.ok) {
    const problem = await response.json().catch(() => null) as { title?: string; errors?: Record<string, string[]> } | null
    const validationErrors = problem?.errors ? Object.values(problem.errors).flat().join(' ') : ''
    throw new Error(validationErrors || problem?.title || `Unable to save machinery library entry (${response.status})`)
  }
  return response.json() as Promise<MachineryLibraryItem>
}

export function createMachineryLibrary(payload: MachineryLibraryPayload): Promise<MachineryLibraryItem> {
  return saveMachineryLibrary(payload)
}

export function updateMachineryLibrary(id: number, payload: MachineryLibraryPayload): Promise<MachineryLibraryItem> {
  return saveMachineryLibrary(payload, id)
}
