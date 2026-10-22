export interface MachineGridItem {
  id: number
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

export interface MachineFilters {
  search: string
  manufacturerId: number | undefined
  status: number | undefined
  city: string
}

export interface MachinePage {
  items: MachineGridItem[]
  page: number
  pageSize: number
  totalCount: number
}

export async function getMachines(
  page: number,
  filters: MachineFilters,
  signal?: AbortSignal,
): Promise<MachinePage> {
  const params = new URLSearchParams({ page: String(page) })
  if (filters.search.trim()) params.set('search', filters.search.trim())
  if (filters.manufacturerId !== undefined) params.set('manufacturerId', String(filters.manufacturerId))
  if (filters.status !== undefined) params.set('status', String(filters.status))
  if (filters.city.trim()) params.set('city', filters.city.trim())

  const response = await fetch(`/api/machines?${params.toString()}`, { signal })
  if (!response.ok) {
    throw new Error(`Unable to load machines (${response.status})`)
  }

  return response.json() as Promise<MachinePage>
}