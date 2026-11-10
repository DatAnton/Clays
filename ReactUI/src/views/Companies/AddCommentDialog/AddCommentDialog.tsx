import { CloseOutlined } from '@ant-design/icons'
import SaveButton from '../../../components/SaveButton'
import { Form, Input, Modal } from 'antd'
import type { CreateCommentPayload } from '../../../services/companiesApi'

interface Props {
  open: boolean
  saving: boolean
  initialValues?: Partial<CreateCommentPayload>
  onCancel: () => void
  onSubmit: (values: CreateCommentPayload) => void | Promise<void>
}

export default function AddCommentDialog({ open, saving, initialValues, onCancel, onSubmit }: Props) {
  const [form] = Form.useForm<CreateCommentPayload>()

  return <Modal title="Add a comment" open={open} onCancel={onCancel} onOk={() => form.submit()}
    confirmLoading={saving} cancelButtonProps={{ icon: <CloseOutlined />, disabled: saving }} closable={!saving}
    mask={{ closable: !saving }} footer={(_, { CancelBtn }) => <><CancelBtn /><SaveButton aria-label="Save comment" loading={saving} onClick={() => form.submit()} /></>} destroyOnHidden>
    <Form form={form} initialValues={initialValues} layout="vertical" disabled={saving} onFinish={onSubmit}>
      <Form.Item name="text" label="Comment" rules={[{ required: true, whitespace: true, message: 'Enter a comment' }, { max: 4000 }]}><Input.TextArea rows={4} maxLength={4000} showCount /></Form.Item>
    </Form>
  </Modal>
}
