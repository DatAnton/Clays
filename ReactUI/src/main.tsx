import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { Auth0Provider } from '@auth0/auth0-react'
import './index.css'
import App from './App.tsx'

const auth0Domain = import.meta.env.REACT_APP_AUTH0_DOMAIN
const auth0ClientId = import.meta.env.REACT_APP_AUTH0_CLIENT_ID
const auth0Audience = import.meta.env.REACT_APP_AUTH0_AUDIENCE

if (!auth0Domain || !auth0ClientId || !auth0Audience) {
  throw new Error('REACT_APP_AUTH0_DOMAIN, REACT_APP_AUTH0_CLIENT_ID, and REACT_APP_AUTH0_AUDIENCE must be configured.')
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <Auth0Provider
      domain={auth0Domain}
      clientId={auth0ClientId}
      cacheLocation="localstorage"
      authorizationParams={{
        redirect_uri: window.location.origin,
        audience: auth0Audience,
        scope: 'openid profile email',
      }}
    >
      <BrowserRouter>
        <App />
      </BrowserRouter>
    </Auth0Provider>
  </StrictMode>,
)
