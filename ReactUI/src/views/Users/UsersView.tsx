import { useEffect, useState } from 'react'
import { Alert, Button, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import AddUserButton from '../../components/AddUserButton'
import { assignUserRoles, getRoles, getUsers, updateUser } from '../../services/usersApi'
import type { ManagedUser, Role, UpdateUserPayload, UserPage } from '../../services/usersApi'

export default function UsersView() {
  const [data, setData] = useState<UserPage | null>(null)
  const [roles, setRoles] = useState<Role[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [searchInput, setSearchInput] = useState('')
  const [version, setVersion] = useState(0)
  const [editing, setEditing] = useState<ManagedUser | null>(null)
  const [selectedRoles, setSelectedRoles] = useState<number[]>([])
  const [saving, setSaving] = useState(false)
  const [saveError, setSaveError] = useState<string | null>(null)
  const [rolesNeedRefresh, setRolesNeedRefresh] = useState(false)
  const [form] = Form.useForm<UpdateUserPayload>()
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

  async function saveProfile(values: UpdateUserPayload) {
    if (!editing) return
    setSaving(true)
    setSaveError(null)
    let profileSaved = false
    try {
      await updateUser(editing.id, {
        firstName: values.firstName,
        lastName: values.lastName,
      })
      profileSaved = true
      const rolesChanged = selectedRoles.length !== editing.roles.length
        || selectedRoles.some(id => !editing.roles.some(role => role.id === id))
      if (rolesChanged) await assignUserRoles(editing, selectedRoles)
      setEditing(null)
      void messageApi.success('User details and roles saved.')
    } catch (cause) {
      const detail = cause instanceof Error ? cause.message : 'Unable to save user.'
      setSaveError(profileSaved ? `User details saved, but roles could not be saved. ${detail}` : detail)
      setRolesNeedRefresh(profileSaved)
    } finally {
      refresh()
      setSaving(false)
    }
  }

  const columns: ColumnsType<ManagedUser> = [
    { title: 'User', key: 'name', width: 240, render: (_, user) => <div className="company-name-cell">
      <Typography.Text strong>{[user.firstName, user.lastName].filter(Boolean).join(' ') || 'Unnamed user'}</Typography.Text>
      {user.userName && <Typography.Text type="secondary">{user.userName}</Typography.Text>}
      <Typography.Text type="secondary">{user.email}</Typography.Text>
    </div> },
    { title: 'Roles', key: 'roles', width: 320, render: (_, user) => <Space wrap size={[8, 8]}>
      {user.roles.length ? user.roles.map(role => <Tag className="user-role-tag" color={role.name === 'Admin' ? 'blue' : 'default'} key={role.id}>{role.name}</Tag>)
        : <Typography.Text type="secondary">No roles</Typography.Text>}
    </Space> },
    { title: 'Actions', key: 'actions', width: 90, fixed: 'right', render: (_, user) =>
      <Button aria-label={`Edit ${user.email}`} title="Edit user" onClick={() => {
        form.resetFields()
        form.setFieldsValue({ firstName: user.firstName, lastName: user.lastName })
        setSelectedRoles(user.roles.map(role => role.id))
        setSaveError(null)
        setRolesNeedRefresh(false)
        setEditing(user)
      }} icon={
        <svg width="16" height="16" viewBox="0 0 16 16" fill="none" stroke="currentColor"
          strokeWidth="1.5" strokeLinecap="round" aria-hidden="true">
          <path d="m10 3 3 3M3 13l3-.7 7.5-7.5a2.1 2.1 0 0 0-3-3L3 9.3 2.3 13.7Z" />
        </svg>
      } />
    },
  ]

  return <section>
    {contextHolder}
    <div className="view-heading">
      <div>
        <Typography.Title level={2}>Users</Typography.Title>
        <Typography.Paragraph type="secondary">Manage users and their application roles.</Typography.Paragraph>
      </div>
      <Space wrap>
        <AddUserButton onCreated={() => { setPage(1); setSearch(''); setSearchInput(''); refresh() }} />
      </Space>
    </div>
    <div className="filter-card users-filter-card">
      <form className="users-search-form" onSubmit={event => {
        event.preventDefault()
        setPage(1)
        setSearch(searchInput.trim())
      }}>
        <label className="filter-field" htmlFor="users-search">Search users
          <Input id="users-search" placeholder="Name, email or username" allowClear
            value={searchInput} onChange={event => setSearchInput(event.target.value)}
            onClear={() => { setPage(1); setSearch('') }} />
        </label>
        <Button type="primary" htmlType="submit">Search</Button>
      </form>
    </div>
    {error && <Alert className="companies-error" type="error" showIcon title={error}
      action={<Button onClick={refresh}>Retry</Button>} />}
    <div className="table-card">
      <Table<ManagedUser> rowKey="id" columns={columns} dataSource={data?.items ?? []} loading={loading}
        scroll={{ x: 700 }} locale={{ emptyText: error ? 'Users could not be loaded.' : 'No users found.' }}
        pagination={{ current: page, pageSize: 25, total: data?.totalCount ?? 0, showSizeChanger: false,
          showTotal: total => `${total} users`, onChange: setPage }} />
    </div>
    <Modal title="Edit user" open={editing !== null} width={640} okText="Save changes"
      okButtonProps={{ disabled: rolesNeedRefresh }}
      confirmLoading={saving} cancelButtonProps={{ disabled: saving }} closable={!saving} maskClosable={!saving}
      onCancel={() => { if (!saving) setEditing(null) }} onOk={() => form.submit()}>
      {saveError && <Alert type="error" showIcon title={saveError}
        description={rolesNeedRefresh ? 'Close this dialog and reopen the user before trying again.' : undefined} />}
      <Form form={form} layout="vertical" onFinish={saveProfile} disabled={saving}>
        <Form.Item label="Email">
          <Input type="email" value={editing?.email ?? ''} readOnly />
        </Form.Item>
        <div className="form-two-columns">
          <Form.Item name="firstName" label="First name" rules={[{ max: 200 }]}><Input maxLength={200} /></Form.Item>
          <Form.Item name="lastName" label="Last name" rules={[{ max: 200 }]}><Input maxLength={200} /></Form.Item>
        </div>
        <Form.Item label="Assigned roles">
          <Select mode="multiple" aria-label="Assigned roles" placeholder="Select roles" value={selectedRoles}
            onChange={setSelectedRoles} disabled={saving || rolesNeedRefresh} optionFilterProp="label"
            options={roles.map(role => ({ value: role.id, label: role.name }))} />
        </Form.Item>
        {roles.length === 0 && <Typography.Paragraph type="secondary">No application roles are available.</Typography.Paragraph>}
      </Form>
    </Modal>
  </section>
}
