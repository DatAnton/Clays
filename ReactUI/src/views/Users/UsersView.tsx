import UsersGrid from './UsersGrid'
import { useEffect, useState } from 'react'
import { Alert, Button, Form, Input, Modal, Select, Space, Typography, message } from 'antd'
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

  const openEditUser = (user: ManagedUser) => {
    form.resetFields()
    form.setFieldsValue({ firstName: user.firstName, lastName: user.lastName })
    setSelectedRoles(user.roles.map(role => role.id))
    setSaveError(null)
    setRolesNeedRefresh(false)
    setEditing(user)
  }

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
    <UsersGrid onEdit={openEditUser} dataSource={data?.items ?? []} loading={loading}
        scroll={{ x: 700 }} locale={{ emptyText: error ? 'Users could not be loaded.' : 'No users found.' }}
        pagination={{ current: page, pageSize: 25, total: data?.totalCount ?? 0, showSizeChanger: false,
          showTotal: total => `${total} users`, onChange: setPage }} />
    <Modal title="Edit user" open={editing !== null} width={640} okText="Save changes"
      okButtonProps={{ disabled: rolesNeedRefresh }}
      confirmLoading={saving} cancelButtonProps={{ disabled: saving }} closable={!saving} mask={{ closable: !saving }}
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
