import AddButton from '../../components/AddButton'
import UsersGrid from './UsersGrid'
import GridFilters from '../../components/grid/GridFilters'
import { useEffect, useState } from 'react'
import { Alert, Button, Input, Space, Typography, message } from 'antd'
import AddUpdateUserDialog from './AddUpdateUserDialog/AddUpdateUserDialog'
import { getRoles, getUsers } from '../../services/usersApi'
import type { ManagedUser, Role, UserPage } from '../../services/usersApi'

export default function UsersView() {
  const [data, setData] = useState<UserPage | null>(null)
  const [roles, setRoles] = useState<Role[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [searchInput, setSearchInput] = useState('')
  const [version, setVersion] = useState(0)
  const [dialog, setDialog] = useState<{ mode: 'add' } | { mode: 'update'; user: ManagedUser } | null>(null)

  const [messageApi, contextHolder] = message.useMessage()

  function refresh() { setVersion(value => value + 1) }

  useEffect(() => {
    const controller = new AbortController()
    void Promise.resolve().then(() => {
      if (!controller.signal.aborted) { setLoading(true); setError(null); setData(null) }
    })
    Promise.all([getUsers(page, search, controller.signal), getRoles(controller.signal)])
      .then(([users, availableRoles]) => {
        if (!controller.signal.aborted) { setData(users); setRoles(availableRoles) }
      }).catch((cause: unknown) => {
        if (!controller.signal.aborted) setError(cause instanceof Error ? cause.message : 'Unable to load users.')
      }).finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [page, search, version])

  const openEditUser = (user: ManagedUser) => setDialog({ mode: 'update', user })

  return <section>
    {contextHolder}
    <div className="view-heading">
      <div>
        <Typography.Title level={2}>Users</Typography.Title>
        <Typography.Paragraph type="secondary">Manage users and their application roles.</Typography.Paragraph>
      </div>
      <div className="companies-heading-actions">
        <AddButton aria-label="Add user" onClick={() => setDialog({ mode: 'add' })} />
        <div className="company-count">
          <span className="count-value">{(data?.totalCount ?? 0).toLocaleString()}</span>
          <span className="count-label">users</span>
        </div>
      </div>
    </div>
    <GridFilters values={[{ label: 'Search', value: search }]} onSubmit={event => {
        event.preventDefault()
        setPage(1)
        setSearch(searchInput.trim())
      }} filterActions={<Space>
        <Button onClick={() => { setPage(1); setSearch(''); setSearchInput('') }}>Clear filters</Button>
        <Button type="primary" htmlType="submit">Apply filters</Button>
      </Space>}>
      <div className="filter-grid">
        <label className="filter-field filter-search" htmlFor="users-search">
          <span>Search users</span>
          <Input id="users-search" placeholder="Name, email or username" allowClear
            value={searchInput} onChange={event => setSearchInput(event.target.value)} />
        </label>
      </div>
    </GridFilters>
    {error && <Alert className="companies-error" type="error" showIcon title={error}
      action={<Button onClick={refresh}>Retry</Button>} />}
    <UsersGrid onEdit={openEditUser} dataSource={data?.items ?? []} loading={loading}
        scroll={{ x: 700 }} locale={{ emptyText: error ? 'Users could not be loaded.' : 'No users found.' }}
        pagination={{ current: page, pageSize: 25, total: data?.totalCount ?? 0, showSizeChanger: false,
          showTotal: total => `${total} users`, onChange: setPage }} />
    {dialog && <AddUpdateUserDialog {...dialog} roles={roles} onCancel={() => setDialog(null)}
      onRefresh={refresh} onSaved={mode => {
        if (mode === 'add') { setPage(1); setSearch(''); setSearchInput(''); refresh() }
        void messageApi.success(mode === 'add' ? 'User added. They can now sign in with this email.' : 'User details and roles saved.')
      }} />}

  </section>
}
