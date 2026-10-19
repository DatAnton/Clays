export interface Company {
  id: number
  name: string
  businessId: string | null
  city: string | null
  address: string | null
  mainBusinessLine: string | null
  isOurPartner: boolean
}

export interface CompanyPage {
  items: Company[]
  page: number
  pageSize: number
  totalCount: number
}

export interface CompanyFilters {
  search: string
  city: string
  businessLine: string
  isOurPartner: boolean | undefined
}

export async function getCompanies(
  page: number,
  filters: CompanyFilters,
  signal?: AbortSignal,
): Promise<CompanyPage> {
  const params = new URLSearchParams({ page: String(page) })
  if (filters.search.trim()) params.set('search', filters.search.trim())
  if (filters.city.trim()) params.set('city', filters.city.trim())
  if (filters.businessLine.trim()) params.set('businessLine', filters.businessLine.trim())
  if (filters.isOurPartner !== undefined) {
    params.set('isOurPartner', String(filters.isOurPartner))
  }

  const response = await fetch(`/api/companies?${params.toString()}`, { signal })
  if (!response.ok) {
    throw new Error(`Unable to load companies (${response.status})`)
  }

  return response.json() as Promise<CompanyPage>
}
