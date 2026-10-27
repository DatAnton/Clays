import { ConfigProvider, Tabs, Typography } from 'antd'
import { Navigate, Route, Routes, useLocation, useNavigate, useParams } from 'react-router-dom'
import CompaniesView from './views/Companies/CompaniesView'
import CompanyDetailsView from './views/Companies/CompanyDetailsView'
import MachinesView from './views/Machines/MachinesView'
import invMachLogo from './assets/invmachlogo.jpg'
import './App.css'

function App() {
  const navigate = useNavigate()
  const location = useLocation()
  const activeTab = location.pathname.startsWith('/machines')
    ? 'machines'
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
                  { key: 'machines', label: 'Machines' },
                ]}
                onChange={(key) => navigate(key === 'machines' ? '/machines' : '/')}
              />
            </nav>
          </div>
        </header>
        <main className="app-main">
          <Routes>
            <Route path="/" element={<CompaniesView onSelectCompany={(id) => navigate(`/company/${id}`)} />} />
            <Route path="/machines" element={<MachinesView onSelectCompany={(id) => navigate(`/company/${id}`)} />} />
            <Route path="/company/:id" element={<CompanyDetailsRoute />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </main>
        <footer className="app-footer">Invest Machinery <span>·</span> Company intelligence</footer>
      </div>
    </ConfigProvider>
  )
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
