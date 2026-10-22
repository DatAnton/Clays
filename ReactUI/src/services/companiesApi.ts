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

export interface UserSummary {
  id: number
  firstName: string | null
  lastName: string | null
  userName: string | null
}

export interface Machine {
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
  createdById: number
}

export interface Contact {
  id: number
  firstName: string | null
  lastName: string | null
  phoneNumber: string | null
  email: string | null
  companyId: number
}

export interface CompanyComment {
  id: number
  companyId: number
  text: string
  author: UserSummary
  createdAtUtc: string
}

export interface CompanyAction {
  id: number
  authorId: number
  author: UserSummary
  assignedUserId: number | null
  assignedUser: UserSummary | null
  type: number
  status: number
  note: string | null
  createdDate: string
  actionWeek: number
  companyId: number
  machineId: number | null
  machine: Machine | null
}

export interface CompanyDetails extends Company {
  status: number | null
  note: string | null
  createdBy: UserSummary | null
  contacts: Contact[]
  comments: CompanyComment[]
  machines: Machine[]
  ownerHistory: { id: number; machine: Machine }[]
  actions: CompanyAction[]
}

export interface CreateCommentPayload { text: string; authorId: number }
export interface CreateContactPayload {
  firstName?: string
  lastName?: string
  phoneNumber?: string
  email?: string
  companyId: number
}
export interface CreateMachinePayload {
  manufacturerId: number
  model: string
  companyId?: number
  year?: number
  vinNumber?: string
  registrationNumber?: string
  countryOfImport?: string
  purchaseYear?: number
  status?: number
  createdById: number
}
export interface CreateActionPayload {
  authorId: number
  assignedUserId?: number
  type: number
  status: number
  note?: string
  actionWeek: number
  companyId: number
  machineId?: number
}

async function requestJson<T>(url: string, init?: RequestInit): Promise<T> {
  const response = await fetch(url, {
    ...init,
    headers: { 'Content-Type': 'application/json', ...init?.headers },
  })
  if (!response.ok) {
    const body = await response.text()
    throw new Error(body || `Request failed (${response.status})`)
  }
  return response.json() as Promise<T>
}

export function getCompany(id: number, signal?: AbortSignal): Promise<CompanyDetails> {
  return requestJson<CompanyDetails>(`/api/companies/${id}`, { signal })
}

export function createCompanyComment(companyId: number, payload: CreateCommentPayload): Promise<CompanyComment> {
  return requestJson<CompanyComment>(`/api/companies/${companyId}/comments`, {
    method: 'POST', body: JSON.stringify(payload),
  })
}

export function createContact(payload: CreateContactPayload): Promise<Contact> {
  return requestJson<Contact>('/api/contacts', { method: 'POST', body: JSON.stringify(payload) })
}

export function createMachine(payload: CreateMachinePayload): Promise<Machine> {
  return requestJson<Machine>('/api/machines', { method: 'POST', body: JSON.stringify(payload) })
}

export function createAction(payload: CreateActionPayload): Promise<CompanyAction> {
  return requestJson<CompanyAction>('/api/actions', { method: 'POST', body: JSON.stringify(payload) })
}

export function updateActionStatus(id: number, status: number): Promise<CompanyAction> {
  return requestJson<CompanyAction>(`/api/actions/${id}/status`, {
    method: 'PATCH', body: JSON.stringify({ status }),
  })
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
