import { ConfigProvider, Tabs, Typography } from 'antd'
import CompaniesView from './views/Companies/CompaniesView'
import './App.css'

function App() {
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
            <div className="brand-mark" aria-hidden="true">IM</div>
            <Typography.Title level={3} className="brand-title">Invest Machinery</Typography.Title>
            <nav className="primary-navigation" aria-label="Main navigation">
              <Tabs
                activeKey="companies"
                items={[{ key: 'companies', label: 'Companies' }]}
                onChange={() => undefined}
              />
            </nav>
          </div>
        </header>
        <main className="app-main">
          <CompaniesView />
        </main>
        <footer className="app-footer">Invest Machinery <span>·</span> Company intelligence</footer>
      </div>
    </ConfigProvider>
  )
}

export default App
