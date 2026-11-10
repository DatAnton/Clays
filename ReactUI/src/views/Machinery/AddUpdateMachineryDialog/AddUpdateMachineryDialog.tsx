import { CloseOutlined } from '@ant-design/icons'
import SaveButton from '../../../components/SaveButton'
import { useEffect, useState } from 'react'
import { Alert, Form, Input, InputNumber, Modal, Select } from 'antd'
import type { UpdateMachineryPayload } from '../../../services/companiesApi'
import { machineryStatuses } from '../machineryOptions'
import { getMachineryLibrary } from '../../../services/machineryApi'
import type { MachineryLibraryItem } from '../../../services/machineryApi'

export type MachineryFormValues = UpdateMachineryPayload

interface Props {
  mode: 'add' | 'update'
  open: boolean
  saving: boolean
  initialValues?: Partial<MachineryFormValues>
  onCancel: () => void
  onSubmit: (values: MachineryFormValues) => void | Promise<void>
}

export default function AddUpdateMachineryDialog({ mode, open, saving, initialValues, onCancel, onSubmit }: Props) {
  const [form] = Form.useForm<MachineryFormValues>()
  const [library, setLibrary] = useState<MachineryLibraryItem[]>([])
  const [loadingLibrary, setLoadingLibrary] = useState(true)
  const [libraryError, setLibraryError] = useState<string | null>(null)

  useEffect(() => {
    const controller = new AbortController()
    void getMachineryLibrary(controller.signal)
      .then((items) => {
        if (!controller.signal.aborted) setLibrary(items)
      })
      .catch((error: unknown) => {
        if (!controller.signal.aborted) {
          setLibraryError(error instanceof Error ? error.message : 'Could not load machinery library')
        }
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoadingLibrary(false)
      })
    return () => controller.abort()
  }, [])

  return <Modal title={mode === 'update' ? 'Edit machinery' : 'Add machinery'}
      footer={(_, { CancelBtn }) => <><CancelBtn /><SaveButton aria-label="Save machinery" loading={saving} disabled={loadingLibrary || libraryError !== null || library.length === 0} onClick={() => form.submit()} /></>} open={open}
    onCancel={onCancel} onOk={() => form.submit()} confirmLoading={saving}
    cancelButtonProps={{ icon: <CloseOutlined />, disabled: saving }} closable={!saving} mask={{ closable: !saving }} destroyOnHidden>
    <Form form={form} initialValues={initialValues} layout="vertical" disabled={saving} onFinish={onSubmit}>
      {libraryError && <Alert type="error" showIcon title="Could not load machinery library" description={libraryError} />}
      <Form.Item name="machineryLibraryId" label="Manufacturer and model" rules={[{ required: true, message: 'Select machinery from the library' }]}>
        <Select showSearch={{ optionFilterProp: 'label' }} loading={loadingLibrary}
          disabled={loadingLibrary || libraryError !== null}
          options={library.map((item) => ({ value: item.id, label: `${item.manufacturerName} ${item.model}` }))}
          placeholder="Select from machinery library"
          notFoundContent={loadingLibrary ? 'Loading machinery library…' : 'No machinery library entries available'} />
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
        <Form.Item name="status" label="Status"><Select allowClear options={machineryStatuses} /></Form.Item>
      </div>
    </Form>
  </Modal>
}
