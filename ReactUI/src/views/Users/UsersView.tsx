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
  const [version, setVersion] = useState(0)
  const [editing, setEditing] = useState<ManagedUser | null>(null)
  const [roleUser, setRoleUser] = useState<ManagedUser | null>(null)
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
    try {
      await updateUser(editing.id, {
        email: values.email,
        firstName: values.firstName,
        lastName: values.lastName,
        userName: values.userName,
      })
      setEditing(null)
      refresh()
      void messageApi.success('User details saved.')
    } catch (cause) {
      setSaveError(cause instanceof Error ? cause.message : 'Unable to save user.')
    } finally { setSaving(false) }
  }

  async function saveRoles() {
    if (!roleUser) return
    setSaving(true)
    setSaveError(null)
    try {
      await assignUserRoles(roleUser, selectedRoles)
      setRoleUser(null)
      void messageApi.success('Roles saved. Access will use the updated roles on the next request.')
    } catch (cause) {
      setSaveError(cause instanceof Error ? cause.message : 'Unable to save roles.')
      setRolesNeedRefresh(true)
    } finally {
      refresh()
      setSaving(false)
    }
  }

  const columns: ColumnsType<ManagedUser> = [
    { title: 'ID', dataIndex: 'id', width: 70 },
    { title: 'User', key: 'name', width: 240, render: (_, user) => <div className="company-name-cell">
      <Typography.Text strong>{[user.firstName, user.lastName].filter(Boolean).join(' ') || 'Unnamed user'}</Typography.Text>
      <Typography.Text type="secondary">{user.email}</Typography.Text>
    </div> },
    { title: 'Username', dataIndex: 'userName', width: 150, render: value => value || '—' },
    { title: 'Roles', key: 'roles', width: 220, render: (_, user) => <Space wrap size={[0, 4]}>
      {user.roles.length ? user.roles.map(role => <Tag color={role.name === 'Admin' ? 'blue' : 'default'} key={role.id}>{role.name}</Tag>)
        : <Typography.Text type="secondary">No roles</Typography.Text>}
    </Space> },
    { title: 'Auth0 account', key: 'account', width: 220, render: (_, user) => <div className="company-name-cell">
      <Tag color={user.auth0Subject ? 'green' : 'default'}>
        {user.auth0Subject ? 'Linked' : 'Awaiting verified sign-in'}
      </Tag>
      {user.auth0Subject && <Typography.Text type="secondary" className="user-account-id">{user.auth0Subject}</Typography.Text>}
    </div> },
    { title: 'Chat ID', dataIndex: 'chatId', width: 160, render: value => value || '—' },
    { title: 'Actions', key: 'actions', width: 190, fixed: 'right', render: (_, user) => <Space>
      <Button onClick={() => {
        form.resetFields()
        form.setFieldsValue({ email: user.email, firstName: user.firstName,
          lastName: user.lastName, userName: user.userName })
        setSaveError(null)
        setEditing(user)
      }}>Edit</Button>
        <Button onClick={() => {
          setRoleUser(user)
          setSelectedRoles(user.roles.map(role => role.id))
          setSaveError(null)
          setRolesNeedRefresh(false)
        }}>Roles</Button>
    </Space> },
  ]

  return <section>
    {contextHolder}
    <div className="view-heading">
      <div>
        <Typography.Title level={2}>Users</Typography.Title>
        <Typography.Paragraph type="secondary">Manage users and their application roles.</Typography.Paragraph>
      </div>
      <Space wrap>
        <Button onClick={refresh} loading={loading}>Refresh</Button>
        <AddUserButton onCreated={() => { setPage(1); setSearch(''); refresh() }} />
      </Space>
    </div>
    <div className="filter-card users-filter-card">
      <label className="filter-field">Search users
        <Input.Search placeholder="Name, email or username" allowClear enterButton="Search" key={search}
          defaultValue={search} onSearch={value => { setPage(1); setSearch(value.trim()) }} />
      </label>
    </div>
    {error && <Alert className="companies-error" type="error" showIcon title={error}
      action={<Button onClick={refresh}>Retry</Button>} />}
    <div className="table-card">
      <Table<ManagedUser> rowKey="id" columns={columns} dataSource={data?.items ?? []} loading={loading}
        scroll={{ x: 1180 }} locale={{ emptyText: error ? 'Users could not be loaded.' : 'No users found.' }}
        pagination={{ current: page, pageSize: 25, total: data?.totalCount ?? 0, showSizeChanger: false,
          showTotal: total => `${total} users`, onChange: setPage }} />
    </div>
    <Modal title="Edit user" open={editing !== null} width={640} okText="Save details"
      confirmLoading={saving} cancelButtonProps={{ disabled: saving }} closable={!saving} maskClosable={!saving}
      onCancel={() => { if (!saving) setEditing(null) }} onOk={() => form.submit()}>
      <Typography.Paragraph type="secondary">
        Changes update the application profile. Auth0 login credentials are managed separately.
        The Auth0 account link stays attached when a linked user’s application email changes.
      </Typography.Paragraph>
      {saveError && <Alert type="error" showIcon title={saveError} />}
      <Form form={form} layout="vertical" onFinish={saveProfile} disabled={saving}>
        <Form.Item name="email" label="Email" rules={[{ required: true, type: 'email',
          transform: (value: string) => value?.trim() }, { max: 320 }]}>
          <Input type="email" maxLength={320} />
        </Form.Item>
        <div className="form-two-columns">
          <Form.Item name="firstName" label="First name" rules={[{ max: 200 }]}><Input maxLength={200} /></Form.Item>
          <Form.Item name="lastName" label="Last name" rules={[{ max: 200 }]}><Input maxLength={200} /></Form.Item>
          <Form.Item name="userName" label="Username" rules={[{ max: 200 }]}><Input maxLength={200} /></Form.Item>
        </div>
        <Form.Item label="Auth0 subject"><Input value={editing?.auth0Subject ?? 'Not linked yet'} disabled /></Form.Item>
      </Form>
    </Modal>
    <Modal title="Assign roles" open={roleUser !== null} okText="Save roles" confirmLoading={saving}
      okButtonProps={{ disabled: rolesNeedRefresh }} cancelButtonProps={{ disabled: saving }}
      closable={!saving} maskClosable={!saving} onCancel={() => { if (!saving) setRoleUser(null) }} onOk={() => void saveRoles()}>
      <Typography.Paragraph>Roles for <strong>{roleUser?.email}</strong></Typography.Paragraph>
      {saveError && <Alert type="error" showIcon title={saveError}
        description="Close this dialog and refresh the users before trying again." />}
      <Typography.Paragraph type="secondary">
        Assign Admin, Staff or ProjectWorker. Changes apply on the user’s next request.
      </Typography.Paragraph>
      <label className="filter-field">Assigned roles
        <Select mode="multiple" aria-label="Assigned roles" placeholder="Select roles" value={selectedRoles}
          onChange={setSelectedRoles} disabled={saving || rolesNeedRefresh} optionFilterProp="label"
          options={roles.map(role => ({ value: role.id, label: role.name }))} />
      </label>
      {roles.length === 0 && <Typography.Paragraph type="secondary">No application roles are available.</Typography.Paragraph>}
    </Modal>
  </section>
}
