import { Checkbox, ConfigProvider, Form, Input, Modal, Select } from 'antd'
import type { CreateCompanyPayload } from '../../../services/companiesApi'

export type CompanyFormValues = CreateCompanyPayload & { status?: number | null; note?: string | null }

interface Props {
  mode: 'add' | 'update'
  open: boolean
  saving: boolean
  initialValues?: Partial<CompanyFormValues>
  onCancel: () => void
  onSubmit: (values: CompanyFormValues) => void | Promise<void>
}

export default function AddUpdateCompanyDialog({ mode, open, saving, initialValues, onCancel, onSubmit }: Props) {
  const [form] = Form.useForm<CompanyFormValues>()

  return <Modal title={mode === 'update' ? 'Edit company' : 'Add company'} open={open}
    onCancel={onCancel} onOk={() => form.submit()} confirmLoading={saving}
    cancelButtonProps={{ disabled: saving }} closable={!saving} mask={{ closable: !saving }}
    okText={mode === 'update' ? 'Save changes' : 'Add company'} destroyOnHidden>
    <Form form={form} layout="vertical" initialValues={{ isOurPartner: false, ...initialValues }} onFinish={onSubmit} disabled={saving}>
      <Form.Item name="name" label="Company name" rules={[{ required: true, whitespace: true, message: 'Enter a company name' }, { max: 200 }]}>
        <Input maxLength={200} />
      </Form.Item>
      <div className="form-two-columns">
        <Form.Item name="businessId" label="Business ID"><Input maxLength={50} /></Form.Item>
        <Form.Item name="city" label="City"><Input maxLength={200} /></Form.Item>
      </div>
      <Form.Item name="address" label="Address"><Input maxLength={500} /></Form.Item>
      <Form.Item name="mainBusinessLine" label="Business line"><Input maxLength={500} /></Form.Item>
      {mode === 'update' && <>
        <Form.Item name="status" label="Status"><Select allowClear options={['Action required', 'Nothing interesting', 'Stay in touch', 'Good future contact'].map((label, value) => ({ value, label }))} /></Form.Item>
        <Form.Item name="note" label="Notes" rules={[{ max: 4000 }]}><Input.TextArea rows={3} maxLength={4000} /></Form.Item>
      </>}
      <ConfigProvider theme={{ token: { controlInteractiveSize: 32 } }}>
        <Form.Item name="isOurPartner" valuePropName="checked">
          <Checkbox className="company-partner-checkbox">Our partner</Checkbox>
        </Form.Item>
      </ConfigProvider>
    </Form>
  </Modal>
}
