import { CloseOutlined } from '@ant-design/icons'
import SaveButton from '../../../components/SaveButton'
import { Form, Input, InputNumber, Modal, Select } from 'antd'
import type { CreateActionPayload } from '../../../services/companiesApi'
import { actionStatuses, actionTypes } from '../actionOptions'

interface Props {
  open: boolean
  saving: boolean
  initialValues?: Partial<CreateActionPayload>
  onCancel: () => void
  onSubmit: (values: CreateActionPayload) => void | Promise<void>
}

export default function AddActionDialog({ open, saving, initialValues, onCancel, onSubmit }: Props) {
  const [form] = Form.useForm<CreateActionPayload>()

  return <Modal title="Create an action" open={open} onCancel={onCancel} onOk={() => form.submit()}
    confirmLoading={saving} cancelButtonProps={{ icon: <CloseOutlined />, disabled: saving }} closable={!saving}
    mask={{ closable: !saving }} footer={(_, { CancelBtn }) => <><CancelBtn /><SaveButton aria-label="Save action" loading={saving} onClick={() => form.submit()} /></>} destroyOnHidden>
    <Form form={form} initialValues={initialValues} layout="vertical" disabled={saving} onFinish={onSubmit}>
      <div className="form-two-columns"><Form.Item name="type" label="Type" rules={[{ required: true }]}><Select options={actionTypes} /></Form.Item><Form.Item name="status" label="Status" rules={[{ required: true }]}><Select options={actionStatuses} /></Form.Item></div>
      <Form.Item name="note" label="Details"><Input.TextArea rows={3} maxLength={4000} /></Form.Item>
      <Form.Item name="assignedUserId" label="Assigned user ID"><InputNumber min={1} style={{ width: '100%' }} /></Form.Item>
      <div className="form-two-columns"><Form.Item name="actionWeek" label="Action week" rules={[{ required: true, type: 'number', min: 1, max: 53 }]}><InputNumber min={1} max={53} style={{ width: '100%' }} /></Form.Item><Form.Item name="machineryId" label="Machinery ID"><InputNumber min={1} style={{ width: '100%' }} /></Form.Item></div>
    </Form>
  </Modal>
}
