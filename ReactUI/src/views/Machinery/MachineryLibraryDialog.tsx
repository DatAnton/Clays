import { useEffect, useState } from 'react'
import { Alert, Button, Form, Input, Modal, Select, message } from 'antd'
import type { TableColumnsType } from 'antd'
import DataGrid, { GridEditButton } from '../../components/grid/DataGrid'
import { createMachineryLibrary, getMachineryLibrary, updateMachineryLibrary } from '../../services/machineryApi'
import type { MachineryLibraryItem, MachineryLibraryPayload } from '../../services/machineryApi'
import { manufacturers } from './machineryOptions'

interface Props {
  isAdmin: boolean
  onCancel: () => void
  onChanged: () => void
}

export default function MachineryLibraryDialog({ isAdmin, onCancel, onChanged }: Props) {
  const [items, setItems] = useState<MachineryLibraryItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [reloadKey, setReloadKey] = useState(0)
  const [search, setSearch] = useState('')
  const [manufacturerId, setManufacturerId] = useState<number | undefined>(undefined)
  const [editing, setEditing] = useState<MachineryLibraryItem | null>(null)
  const [editorOpen, setEditorOpen] = useState(false)
  const [saving, setSaving] = useState(false)
  const [form] = Form.useForm<MachineryLibraryPayload>()
  const [messageApi, contextHolder] = message.useMessage()

  useEffect(() => {
    const controller = new AbortController()
    void getMachineryLibrary(controller.signal).then(entries => {
      if (!controller.signal.aborted) {
        setItems(entries)
        setError(null)
      }
    }).catch((cause: unknown) => {
      if (!controller.signal.aborted) setError(cause instanceof Error ? cause.message : 'Could not load machinery library')
    }).finally(() => {
      if (!controller.signal.aborted) setLoading(false)
    })
    return () => controller.abort()
  }, [reloadKey])

  const openEditor = (entry: MachineryLibraryItem | null) => {
    if (!isAdmin) return
    setEditing(entry)
    form.resetFields()
    if (entry) form.setFieldsValue({ manufacturerId: entry.manufacturerId, model: entry.model })
    setEditorOpen(true)
  }

  const save = async (values: MachineryLibraryPayload) => {
    if (!isAdmin || saving) return
    setSaving(true)
    try {
      const saved = editing
        ? await updateMachineryLibrary(editing.id, values)
        : await createMachineryLibrary(values)
      setItems(current => [...current.filter(item => item.id !== saved.id), saved]
        .sort((a, b) => a.manufacturerName.localeCompare(b.manufacturerName) || a.model.localeCompare(b.model) || a.id - b.id))
      setEditorOpen(false)
      messageApi.success(editing ? 'Library entry updated successfully' : 'Library entry created successfully')
      onChanged()
    } catch (cause) {
      messageApi.error(cause instanceof Error ? cause.message : 'Could not save library entry')
    } finally {
      setSaving(false)
    }
  }

  const columns: TableColumnsType<MachineryLibraryItem> = [
    { title: 'Manufacturer', dataIndex: 'manufacturerName', key: 'manufacturer', width: 220 },
    { title: 'Model', dataIndex: 'model', key: 'model' },
    ...(isAdmin ? [{
      title: 'Actions', key: 'actions', width: 90,
      render: (_: unknown, entry: MachineryLibraryItem) => <GridEditButton
        label={`Edit ${entry.manufacturerName} ${entry.model}`} onClick={() => openEditor(entry)} />,
    }] : []),
  ]
  const query = search.trim().toLocaleLowerCase()
  const filteredItems = items.filter(item => (manufacturerId === undefined || item.manufacturerId === manufacturerId)
    && `${item.manufacturerName} ${item.model}`.toLocaleLowerCase().includes(query))

  return <>
    {contextHolder}
    <Modal title="Library" open width={850} onCancel={onCancel} footer={<Button onClick={onCancel}>Close</Button>}>
      <div className="machinery-library-toolbar">
        <Input.Search className="machinery-library-search" allowClear placeholder="Search manufacturer or model" value={search}
          onChange={event => setSearch(event.target.value)} />
        <Select className="machinery-library-manufacturer" aria-label="Filter by manufacturer"
          allowClear placeholder="All manufacturers" options={manufacturers} value={manufacturerId}
          onChange={(value: number | undefined) => setManufacturerId(value)} />
        {isAdmin && <Button className="machinery-library-add" type="primary" onClick={() => openEditor(null)}
          icon={<svg width="16" height="16" viewBox="0 0 16 16" fill="none" stroke="currentColor"
            strokeWidth="1.5" strokeLinecap="round" aria-hidden="true"><path d="M8 3v10M3 8h10" /></svg>}>Add</Button>}
      </div>
      {error && <Alert type="error" showIcon title="Could not load machinery library" description={error}
        action={<Button onClick={() => { setLoading(true); setReloadKey(key => key + 1) }}>Retry</Button>} />}
      <DataGrid<MachineryLibraryItem> columns={columns} dataSource={filteredItems} loading={loading}
        pagination={{ pageSize: 10, showSizeChanger: false, hideOnSinglePage: true }}
        locale={{ emptyText: loading ? 'Loading machinery library…' : 'No machinery library entries found' }} />
    </Modal>
    <Modal title={editing ? 'Edit library machinery' : 'Add library machinery'} open={editorOpen && isAdmin}
      onCancel={() => { if (!saving) setEditorOpen(false) }} onOk={() => form.submit()}
      okText="Save machinery" confirmLoading={saving} cancelButtonProps={{ disabled: saving }}
      closable={!saving} mask={{ closable: !saving }}>
      <Form form={form} layout="vertical" disabled={saving} onFinish={save}>
        <Form.Item name="manufacturerId" label="Manufacturer" rules={[{ required: true, message: 'Select a manufacturer' }]}>
          <Select options={manufacturers} showSearch={{ optionFilterProp: 'label' }} placeholder="Select manufacturer" />
        </Form.Item>
        <Form.Item name="model" label="Model" rules={[
          { required: true, whitespace: true, message: 'Enter a model' },
          { max: 200, message: 'Model must be 200 characters or fewer' },
        ]}>
          <Input maxLength={200} placeholder="Model" />
        </Form.Item>
      </Form>
    </Modal>
  </>
}
