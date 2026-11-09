import { useState } from 'react'
import { Alert, Form, Input, Modal, Select, Typography } from 'antd'
import { assignUserRoles, createUser, updateUser } from '../../../services/usersApi'
import type { ManagedUser, Role, UpdateUserPayload } from '../../../services/usersApi'

type Props = {
  roles: Role[]
  onCancel: () => void
  onSaved: (mode: 'add' | 'update') => void
  onRefresh: () => void
} & ({ mode: 'add'; user?: never } | { mode: 'update'; user: ManagedUser })

type UserFormValues = UpdateUserPayload & { email?: string }

export default function AddUpdateUserDialog({ mode, user, roles, onCancel, onSaved, onRefresh }: Props) {
  const [saving, setSaving] = useState(false)
  const [saveError, setSaveError] = useState<string | null>(null)
  const [rolesNeedRefresh, setRolesNeedRefresh] = useState(false)
  const [selectedRoles, setSelectedRoles] = useState<number[]>(() => user?.roles.map(role => role.id) ?? [])
  const [form] = Form.useForm<UserFormValues>()

  async function saveProfile(values: UserFormValues) {
    if (saving || rolesNeedRefresh) return
    setSaving(true)
    setSaveError(null)
    let profileSaved = false
    try {
      if (mode === 'add') {
        await createUser({ email: values.email ?? '', firstName: values.firstName ?? '', lastName: values.lastName ?? '' })
        onSaved('add')
        onCancel()
        return
      }
      if (!user) return
      await updateUser(user.id, {
        firstName: values.firstName,
        lastName: values.lastName,
      })
      profileSaved = true
      const rolesChanged = selectedRoles.length !== user.roles.length
        || selectedRoles.some(id => !user.roles.some(role => role.id === id))
      if (rolesChanged) await assignUserRoles(user, selectedRoles)
      onCancel()
      onSaved('update')
    } catch (cause) {
      const detail = cause instanceof Error ? cause.message : 'Unable to save user.'
      setSaveError(profileSaved ? `User details saved, but roles could not be saved. ${detail}` : detail)
      setRolesNeedRefresh(profileSaved)
    } finally {
      if (mode === 'update') onRefresh()
      setSaving(false)
    }
  }

  return <Modal title={mode === 'add' ? 'Add user' : 'Edit user'} open width={640}
      okText={mode === 'add' ? 'Create user' : 'Save changes'}
      okButtonProps={{ disabled: rolesNeedRefresh }} confirmLoading={saving}
      cancelButtonProps={{ disabled: saving }} closable={!saving} mask={{ closable: !saving }}
      onCancel={() => { if (!saving) onCancel() }} onOk={() => form.submit()}>
      {saveError && <Alert type="error" showIcon title={saveError}
        description={rolesNeedRefresh ? 'Close this dialog and reopen the user before trying again.' : undefined} />}
      <Form form={form} layout="vertical" onFinish={saveProfile} disabled={saving}
        initialValues={{ firstName: user?.firstName, lastName: user?.lastName }}>
        {mode === 'add' ? <Form.Item name="email" label="Email" rules={[
          { required: true, type: 'email', message: 'Enter a valid email address.', transform: (value: string) => value?.trim() },
          { max: 320 },
        ]}><Input type="email" autoComplete="off" maxLength={320} /></Form.Item>
          : <Form.Item label="Email"><Input type="email" value={user.email} readOnly /></Form.Item>}
        <div className="form-two-columns">
          <Form.Item name="firstName" label="First name" rules={[{ required: mode === 'add', whitespace: true }, { max: 200 }]}><Input maxLength={200} /></Form.Item>
          <Form.Item name="lastName" label="Last name" rules={[{ required: mode === 'add', whitespace: true }, { max: 200 }]}><Input maxLength={200} /></Form.Item>
        </div>
        {mode === 'update' && <>
          <Form.Item label="Assigned roles">
            <Select mode="multiple" aria-label="Assigned roles" placeholder="Select roles" value={selectedRoles}
              onChange={setSelectedRoles} disabled={saving || rolesNeedRefresh} optionFilterProp="label"
              options={roles.map(role => ({ value: role.id, label: role.name }))} />
          </Form.Item>
          {roles.length === 0 && <Typography.Paragraph type="secondary">No application roles are available.</Typography.Paragraph>}
        </>}
      </Form>
  </Modal>
}
