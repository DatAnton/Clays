import { useAuth0 } from '@auth0/auth0-react'
import { useEffect, useState } from 'react'
import { Button, ConfigProvider, Result, Spin, Tabs, Typography } from 'antd'
import { Navigate, Route, Routes, useLocation, useNavigate, useParams } from 'react-router-dom'
import CompaniesView from './views/Companies/CompaniesView'
import CompanyDetailsView from './views/Companies/CompanyDetailsView'
import MachineryView from './views/Machinery/MachineryView'
import { setAccessTokenProvider, setUnauthorizedHandler } from './services/apiClient'
import UsersView from './views/Users/UsersView'
import { getCurrentUserAccess } from './services/usersApi'
import invMachLogo from './assets/invmachlogo.jpg'
import './App.css'

function App() {
  const { isLoading, isAuthenticated, user, loginWithRedirect, logout, getAccessTokenSilently } = useAuth0()
  const navigate = useNavigate()
  const location = useLocation()
  const [accessCheck, setAccessCheck] = useState<{
    subject: string | undefined
    isAdmin: boolean
    isStaff: boolean
    error: string | null
  } | null>(null)
  const subject = user?.sub
  const currentAccess = accessCheck?.subject === subject
    ? accessCheck : null
  setAccessTokenProvider(isAuthenticated
    ? () => getAccessTokenSilently({
      authorizationParams: { audience: import.meta.env.REACT_APP_AUTH0_AUDIENCE },
    })
    : null)

  useEffect(() => {
    if (!isAuthenticated) return
    const controller = new AbortController()
    getCurrentUserAccess(controller.signal).then(access => {
      if (!controller.signal.aborted) {
        setAccessCheck({ subject, isAdmin: access.isAdmin, isStaff: access.isStaff, error: null })
      }
    }).catch((cause: unknown) => {
      if (!controller.signal.aborted) {
        setAccessCheck({ subject, isAdmin: false, isStaff: false,
          error: cause instanceof Error ? cause.message : 'Unable to verify account access.' })
      }
    })
    return () => controller.abort()
  }, [isAuthenticated, subject])
  setUnauthorizedHandler(isAuthenticated
    ? async (message) => {
      setAccessCheck({ subject, isAdmin: false, isStaff: false, error: message })
    }
    : null)

  if (isLoading) {
    return <div className="auth-screen"><Spin size="large" /></div>
  }

  if (!isAuthenticated) {
    return <div className="auth-screen">
      <section className="welcome-card" aria-labelledby="welcome-title">
        <div className="welcome-logo-frame">
          <img className="welcome-logo" src={invMachLogo} alt="Invest Machinery logo" />
        </div>
        <Typography.Title id="welcome-title" level={2}>Invest Machinery</Typography.Title>
        <Typography.Paragraph type="secondary">
          Sign in to access the company and machinery directory.
        </Typography.Paragraph>
        <Button type="primary" size="large" onClick={() => void loginWithRedirect()}>Sign in</Button>
      </section>
    </div>
  }

  if (!currentAccess) {
    return <div className="auth-screen"><Spin size="large" /></div>
  }

  if (currentAccess.error) {
    return <div className="auth-screen">
      <Result status="403" title="Access unavailable" subTitle={currentAccess.error}
        extra={[
          <Button key="logout" type="primary" onClick={() => void logout({
            logoutParams: { returnTo: window.location.origin },
          })}>Sign out</Button>,
        ]} />
    </div>
  }

  const isAdmin = currentAccess.isAdmin
  const activeTab = location.pathname.startsWith('/users') ? 'users' : location.pathname.startsWith('/machinery')
    ? 'machinery'
    : 'companies'

  return (
    <ConfigProvider theme={{
      token: {
        colorPrimary: '#123b70',
        colorInfo: '#123b70',
        borderRadius: 8,
        fontFamily: "Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif",
      },
    }}>
      <div className="app-shell">
        <header className="app-header">
          <div className="header-inner">
            <img className="brand-logo" src={invMachLogo} alt="Invest Machinery logo" />
            <Typography.Title level={3} className="brand-title">Invest Machinery</Typography.Title>
            <nav className="primary-navigation" aria-label="Main navigation">
              <Tabs
                activeKey={activeTab}
                items={[
                  { key: 'companies', label: 'Companies' },
                  { key: 'machinery', label: 'Machinery' },
                  ...(isAdmin ? [{ key: 'users', label: 'Users' }] : []),
                ]}
                onChange={(key) => navigate(key === 'companies' ? '/' : `/${key}`)}
              />
            </nav>
            <div className="header-actions">
              {user?.email && <span className="header-user-email" title={user.email}>{user.email}</span>}
              <Button onClick={() => void logout({
                logoutParams: { returnTo: window.location.origin },
              })}>Sign out</Button>
            </div>
          </div>
        </header>
        <main className="app-main">
          <Routes>
            <Route path="/" element={<CompaniesView onSelectCompany={(id) => navigate(`/company/${id}`)} />} />
            <Route path="/machinery" element={<MachineryView isAdmin={isAdmin} onSelectCompany={(id) => navigate(`/company/${id}`)} />} />
            <Route path="/company/:id" element={<CompanyDetailsRoute canManageContacts={isAdmin || currentAccess.isStaff} />} />
            <Route path="/users" element={isAdmin ? <UsersView />
              : <Result status="403" title="Administrator access required"
                subTitle="Only users with the Admin role can manage users." />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </main>
        <footer className="app-footer">Invest Machinery <span>·</span> Company intelligence</footer>
      </div>
    </ConfigProvider>
  )
}

function CompanyDetailsRoute({ canManageContacts }: { canManageContacts: boolean }) {
  const { id } = useParams()
  const navigate = useNavigate()
  const companyId = Number(id)

  if (!Number.isInteger(companyId) || companyId < 1) {
    return <Navigate to="/" replace />
  }

  return <CompanyDetailsView key={companyId} companyId={companyId} canManageContacts={canManageContacts} onBack={() => navigate('/')} />
}

export default App
