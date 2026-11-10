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
