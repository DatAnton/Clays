import { Form, Input, InputNumber, Modal, Select } from 'antd'
import type { UpdateMachinePayload } from '../../../services/companiesApi'
import { manufacturers, machineStatuses } from '../machineOptions'

export type MachineFormValues = UpdateMachinePayload

interface Props {
  mode: 'add' | 'update'
  open: boolean
  saving: boolean
  initialValues?: Partial<MachineFormValues>
  onCancel: () => void
  onSubmit: (values: MachineFormValues) => void | Promise<void>
}

export default function AddUpdateMachineDialog({ mode, open, saving, initialValues, onCancel, onSubmit }: Props) {
  const [form] = Form.useForm<MachineFormValues>()

  return <Modal title={mode === 'update' ? 'Edit machine' : 'Add machine'} open={open}
    onCancel={onCancel} onOk={() => form.submit()} confirmLoading={saving}
    cancelButtonProps={{ disabled: saving }} closable={!saving} mask={{ closable: !saving }}
    okText="Save machine" destroyOnHidden>
    <Form form={form} initialValues={initialValues} layout="vertical" disabled={saving} onFinish={onSubmit}>
      <Form.Item name="manufacturerId" label="Manufacturer" rules={[{ required: true }]}>
        <Select options={manufacturers} placeholder="Select manufacturer" />
      </Form.Item>
      <Form.Item name="model" label="Model" rules={[{ required: true, whitespace: true }, { max: 200 }]}>
        <Input maxLength={200} />
      </Form.Item>
      <div className="form-two-columns">
        <Form.Item name="year" label="Year"><InputNumber min={1800} max={2200} style={{ width: '100%' }} /></Form.Item>
        <Form.Item name="purchaseYear" label="Purchase year"><InputNumber min={1800} max={2200} style={{ width: '100%' }} /></Form.Item>
      </div>
      <div className="form-two-columns">
        <Form.Item name="vinNumber" label="VIN"><Input maxLength={100} /></Form.Item>
        <Form.Item name="registrationNumber" label="Registration number"><Input maxLength={50} /></Form.Item>
      </div>
      <div className="form-two-columns">
        <Form.Item name="countryOfImport" label="Country of import"><Input maxLength={100} /></Form.Item>
        <Form.Item name="status" label="Status"><Select allowClear options={machineStatuses} /></Form.Item>
      </div>
    </Form>
  </Modal>
}
