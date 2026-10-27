/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly REACT_APP_AUTH0_DOMAIN: string
  readonly REACT_APP_AUTH0_CLIENT_ID: string
  readonly REACT_APP_AUTH0_AUDIENCE: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
