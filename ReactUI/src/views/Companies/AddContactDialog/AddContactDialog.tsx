import { CloseOutlined } from '@ant-design/icons'
import SaveButton from '../../../components/SaveButton'
import { Form, Input, Modal } from 'antd'
import type { CreateContactPayload } from '../../../services/companiesApi'
import type { Machinery } from '../../../services/companiesApi'
import MachinerySelect from './MachinerySelect'

interface Props {
  mode: 'add' | 'update'
  machinery: Machinery[]
  open: boolean
  saving: boolean
  initialValues?: Partial<CreateContactPayload>
  onCancel: () => void
  onSubmit: (values: CreateContactPayload) => void | Promise<void>
}

export default function AddContactDialog({ mode, machinery, open, saving, initialValues, onCancel, onSubmit }: Props) {
  const [form] = Form.useForm<CreateContactPayload>()

  return <Modal title={mode === 'update' ? 'Edit contact' : 'Add a contact'} open={open} onCancel={onCancel} onOk={() => form.submit()}
    confirmLoading={saving} cancelButtonProps={{ icon: <CloseOutlined />, disabled: saving }} closable={!saving}
    mask={{ closable: !saving }} footer={(_, { CancelBtn }) => <><CancelBtn /><SaveButton aria-label="Save contact" loading={saving} onClick={() => form.submit()} /></>} destroyOnHidden>
    <Form form={form} initialValues={initialValues} layout="vertical" disabled={saving} onFinish={onSubmit}>
      <div className="form-two-columns"><Form.Item name="firstName" label="First name"><Input maxLength={200} /></Form.Item><Form.Item name="lastName" label="Last name"><Input maxLength={200} /></Form.Item></div>
      <div className="form-two-columns"><Form.Item name="email" label="Email" rules={[{ type: 'email' }]}><Input maxLength={320} /></Form.Item><Form.Item name="phoneNumber" label="Phone"><Input maxLength={50} /></Form.Item></div>
      <Form.Item name="machineryId" label="Machinery" extra="Optional. Only this company's machinery is available."
        rules={[{ validator: (_, value: number | null | undefined) => value == null || machinery.some(item => item.id === value)
          ? Promise.resolve()
          : Promise.reject(new Error('Select machinery belonging to this company or clear the link.')) }]}>
        <MachinerySelect machinery={machinery} disabled={saving} />
      </Form.Item>
      <Form.Item name="comment" label="Comment" rules={[{ max: 4000 }]}><Input.TextArea rows={3} maxLength={4000} /></Form.Item>
    </Form>
  </Modal>
}
