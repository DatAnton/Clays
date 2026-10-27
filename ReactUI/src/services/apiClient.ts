let accessTokenProvider: (() => Promise<string | undefined>) | null = null

export function setAccessTokenProvider(provider: (() => Promise<string | undefined>) | null): void {
  accessTokenProvider = provider
}

export async function apiFetch(input: RequestInfo | URL, init: RequestInit = {}): Promise<Response> {
  const headers = new Headers(init.headers)
  if (accessTokenProvider) {
    const accessToken = await accessTokenProvider()
    if (accessToken) headers.set('Authorization', `Bearer ${accessToken}`)
  }

  return fetch(input, {
    ...init,
    headers,
    credentials: 'omit',
  })
}