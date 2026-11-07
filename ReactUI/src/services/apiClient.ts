let accessTokenProvider: (() => Promise<string | undefined>) | null = null
let unauthorizedHandler: ((message: string) => Promise<void>) | null = null
let handlingUnauthorized = false

export function setAccessTokenProvider(provider: (() => Promise<string | undefined>) | null): void {
  accessTokenProvider = provider
  if (!provider) handlingUnauthorized = false
}

export function setUnauthorizedHandler(handler: ((message: string) => Promise<void>) | null): void {
  unauthorizedHandler = handler
}

export async function apiFetch(input: RequestInfo | URL, init: RequestInit = {}): Promise<Response> {
  const headers = new Headers(init.headers)
  if (accessTokenProvider) {
    const accessToken = await accessTokenProvider()
    if (accessToken) headers.set('Authorization', `Bearer ${accessToken}`)
  }

  const response = await fetch(input, {
    ...init,
    headers,
    credentials: 'omit',
  })

  if (response.status === 401 && unauthorizedHandler && !handlingUnauthorized) {
    handlingUnauthorized = true
    const problem = await response.clone().json().catch(() => null) as { title?: string; detail?: string } | null
    const message = [problem?.title, problem?.detail]
      .filter((value): value is string => typeof value === 'string' && value.length > 0)
      .join(' ') || 'Your session has expired. Please sign in again.'
    try {
      await unauthorizedHandler(message)
    } finally {
      handlingUnauthorized = false
    }
  }

  return response
}
