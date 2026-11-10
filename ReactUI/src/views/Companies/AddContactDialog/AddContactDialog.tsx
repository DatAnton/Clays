import AddButton from '../../../components/AddButton'
import { Form, Input, Modal } from 'antd'
import type { CreateContactPayload } from '../../../services/companiesApi'

interface Props {
  open: boolean
  saving: boolean
  initialValues?: Partial<CreateContactPayload>
  onCancel: () => void
  onSubmit: (values: CreateContactPayload) => void | Promise<void>
}

export default function AddContactDialog({ open, saving, initialValues, onCancel, onSubmit }: Props) {
  const [form] = Form.useForm<CreateContactPayload>()

  return <Modal title="Add a contact" open={open} onCancel={onCancel} onOk={() => form.submit()}
    confirmLoading={saving} cancelButtonProps={{ disabled: saving }} closable={!saving}
    mask={{ closable: !saving }} footer={(_, { CancelBtn }) => <><CancelBtn /><AddButton aria-label="Add contact" loading={saving} onClick={() => form.submit()} /></>} destroyOnHidden>
    <Form form={form} initialValues={initialValues} layout="vertical" disabled={saving} onFinish={onSubmit}>
      <div className="form-two-columns"><Form.Item name="firstName" label="First name"><Input maxLength={200} /></Form.Item><Form.Item name="lastName" label="Last name"><Input maxLength={200} /></Form.Item></div>
      <div className="form-two-columns"><Form.Item name="email" label="Email" rules={[{ type: 'email' }]}><Input maxLength={320} /></Form.Item><Form.Item name="phoneNumber" label="Phone"><Input maxLength={50} /></Form.Item></div>
    </Form>
  </Modal>
}
