import { useCallback, useEffect, useRef, useState } from 'react'
import { Alert, Button, ConfigProvider, Spin, Tabs, Typography, message } from 'antd'
import { Navigate, Route, Routes, useLocation, useNavigate, useParams } from 'react-router-dom'
import CompaniesView from './views/Companies/CompaniesView'
import CompanyDetailsView from './views/Companies/CompanyDetailsView'
import MachinesView from './views/Machines/MachinesView'
import UsersView from './views/Users/UsersView'
import { getAuthConfiguration, getCurrentUser, signInWithGoogle, signOut } from './services/companiesApi'
import type { AuthUser } from './services/companiesApi'
import invMachLogo from './assets/invmachlogo.jpg'
import './App.css'

interface GoogleCredentialResponse { credential: string }
interface GoogleIdentityServices {
  accounts: {
    id: {
      initialize: (options: { client_id: string; callback: (response: GoogleCredentialResponse) => void }) => void
      renderButton: (element: HTMLElement, options: { theme: string; size: string; shape: string; width: number }) => void
    }
  }
}

declare global {
  interface Window { google?: GoogleIdentityServices }
}

function App() {
  const navigate = useNavigate()
  const location = useLocation()
  const activeTab = location.pathname.startsWith('/machines')
    ? 'machines'
    : location.pathname.startsWith('/users') ? 'users' : 'companies'
  const [user, setUser] = useState<AuthUser | null>(null)
  const [authLoading, setAuthLoading] = useState(true)
  const [authError, setAuthError] = useState<string | null>(null)
  const [messageApi, contextHolder] = message.useMessage()

  useEffect(() => {
    let active = true
    getCurrentUser()
      .then((currentUser) => { if (active) setUser(currentUser) })
      .catch((error: unknown) => {
        if (active && !(error instanceof Error && error.message.includes('(401)'))) {
          setAuthError(error instanceof Error ? error.message : 'Unable to check your account.')
        }
      })
      .finally(() => { if (active) setAuthLoading(false) })
    return () => { active = false }
  }, [])

  const handleSignIn = useCallback(async (credential: string) => {
    try {
      setAuthError(null)
      setUser(await signInWithGoogle(credential))
    } catch (error) {
      setAuthError(error instanceof Error ? error.message : 'Google sign-in failed.')
    }
  }, [])

  const handleSignOut = async () => {
    try {
      await signOut()
      setUser(null)
      navigate('/')
    } catch (error) {
      messageApi.error(error instanceof Error ? error.message : 'Sign out failed.')
    }
  }

  return (
    <ConfigProvider theme={{
      token: {
        colorPrimary: '#123b70',
        colorInfo: '#123b70',
        borderRadius: 8,
        fontFamily: "Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif",
      },
    }}>
      {contextHolder}
      {authLoading ? <div className="auth-loading"><Spin size="large" /></div> : !user ? <GoogleSignIn
        error={authError}
        onCredential={handleSignIn}
      /> : <div className="app-shell">
        <header className="app-header">
          <div className="header-inner">
            <img className="brand-logo" src={invMachLogo} alt="Invest Machinery logo" />
            <Typography.Title level={3} className="brand-title">Invest Machinery</Typography.Title>
            <nav className="primary-navigation" aria-label="Main navigation">
              <Tabs
                activeKey={activeTab}
                items={[
                  { key: 'companies', label: 'Companies' },
                  { key: 'machines', label: 'Machines' },
                  { key: 'users', label: 'Users' },
                ]}
                onChange={(key) => navigate(key === 'machines' ? '/machines' : key === 'users' ? '/users' : '/')}
              />
            </nav>
            <div className="account-menu">
              <span className="account-name">
                {[user.firstName, user.lastName].filter(Boolean).join(' ') || user.userName || user.email}
              </span>
              <Button ghost size="small" onClick={() => void handleSignOut()}>Sign out</Button>
            </div>
          </div>
        </header>
        <main className="app-main">
          <Routes>
            <Route path="/" element={<CompaniesView onSelectCompany={(id) => navigate(`/company/${id}`)} />} />
            <Route path="/machines" element={<MachinesView onSelectCompany={(id) => navigate(`/company/${id}`)} />} />
            <Route path="/users" element={<UsersView isAdmin={user.isAdmin} />} />
            <Route path="/company/:id" element={<CompanyDetailsRoute />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </main>
        <footer className="app-footer">Invest Machinery <span>·</span> Company intelligence</footer>
      </div>
      }
    </ConfigProvider>
  )
}

function GoogleSignIn({ error, onCredential }: { error: string | null; onCredential: (credential: string) => void }) {
  const buttonRef = useRef<HTMLDivElement>(null)
  const [clientId, setClientId] = useState<string | null>(null)
  const [setupError, setSetupError] = useState<string | null>(null)

  useEffect(() => {
    let active = true
    getAuthConfiguration()
      .then((config) => {
        if (!active) return
        if (!config.googleClientId) setSetupError('Google sign-in is not configured. Set Google:ClientId in the server configuration.')
        else setClientId(config.googleClientId)
      })
      .catch((loadError: unknown) => {
        if (active) setSetupError(loadError instanceof Error ? loadError.message : 'Could not load sign-in configuration.')
      })
    return () => { active = false }
  }, [])

  useEffect(() => {
    if (!clientId || !buttonRef.current) return
    let active = true
    const initialize = () => {
      if (!active || !window.google || !buttonRef.current) return
      window.google.accounts.id.initialize({ client_id: clientId, callback: ({ credential }) => onCredential(credential) })
      window.google.accounts.id.renderButton(buttonRef.current, {
        theme: 'outline', size: 'large', shape: 'rectangular', width: 280,
      })
    }

    if (window.google) {
      initialize()
    } else {
      let script = document.querySelector<HTMLScriptElement>('script[data-google-identity]')
      if (!script) {
        script = document.createElement('script')
        script.src = 'https://accounts.google.com/gsi/client'
        script.async = true
        script.defer = true
        script.dataset.googleIdentity = 'true'
        document.head.append(script)
      }
      script.addEventListener('load', initialize)
      return () => {
        active = false
        script?.removeEventListener('load', initialize)
      }
    }

    return () => { active = false }
  }, [clientId, onCredential])

  return <main className="sign-in-page">
    <section className="sign-in-card">
      <img className="sign-in-logo" src={invMachLogo} alt="Invest Machinery logo" />
      <Typography.Text className="sign-in-eyebrow">INVEST MACHINERY</Typography.Text>
      <Typography.Title level={2}>Welcome</Typography.Title>
      <Typography.Paragraph type="secondary">Sign in with your Google account to access company and machine intelligence.</Typography.Paragraph>
      {setupError && <Alert type="warning" showIcon message={setupError} />}
      {error && <Alert className="sign-in-error" type="error" showIcon message="Sign-in failed" description={error} />}
      <div className="google-button-slot" ref={buttonRef} />
      <Typography.Text className="sign-in-caption">Your account is protected by Google authentication.</Typography.Text>
    </section>
  </main>
}

function CompanyDetailsRoute() {
  const { id } = useParams()
  const navigate = useNavigate()
  const companyId = Number(id)

  if (!Number.isInteger(companyId) || companyId < 1) {
    return <Navigate to="/" replace />
  }

  return <CompanyDetailsView companyId={companyId} onBack={() => navigate('/')} />
}

export default App
