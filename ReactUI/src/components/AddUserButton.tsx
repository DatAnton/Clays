import { useState } from 'react'
import { Alert, Button, Form, Input, Modal, Typography, message } from 'antd'
import { createUser } from '../services/usersApi'
import type { CreateUserPayload } from '../services/usersApi'

export default function AddUserButton({ onCreated }: { onCreated?: () => void }) {
  const [open, setOpen] = useState(false)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [form] = Form.useForm<CreateUserPayload>()
  const [messageApi, contextHolder] = message.useMessage()

  async function submit(values: CreateUserPayload) {
    setSaving(true)
    setError(null)
    try {
      await createUser(values)
      setOpen(false)
      form.resetFields()
      onCreated?.()
      void messageApi.success('User added. They can now sign in with this email.')
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'Unable to create user.')
    } finally {
      setSaving(false)
    }
  }

  return <>
    {contextHolder}
    <Button type="primary" onClick={() => { setError(null); setOpen(true) }}>Add user</Button>
    <Modal
      title="Add user"
      open={open}
      okText="Create user"
      confirmLoading={saving}
      cancelButtonProps={{ disabled: saving }}
      closable={!saving}
      maskClosable={!saving}
      onCancel={() => { if (!saving) setOpen(false) }}
      onOk={() => form.submit()}
    >
      <Typography.Paragraph>
        Register the email this person will use to sign in. They’ll need to verify it before their first visit.
        Assign their application roles from the Users page after adding them.
      </Typography.Paragraph>
      {error && <Alert type="error" title={error} showIcon />}
      <Form form={form} layout="vertical" onFinish={submit} disabled={saving}>
        <Form.Item name="email" label="Email" rules={[
          { required: true, type: 'email', message: 'Enter a valid email address.', transform: (value: string) => value?.trim() },
          { max: 320 },
        ]}>
          <Input type="email" autoComplete="off" maxLength={320} />
        </Form.Item>
        <Form.Item name="firstName" label="First name" rules={[{ required: true, whitespace: true }, { max: 200 }]}>
          <Input autoComplete="off" maxLength={200} />
        </Form.Item>
        <Form.Item name="lastName" label="Last name" rules={[{ required: true, whitespace: true }, { max: 200 }]}>
          <Input autoComplete="off" maxLength={200} />
        </Form.Item>
      </Form>
    </Modal>
  </>
}
