import { useCallback, useEffect, useState } from 'react'
import { Alert, Button, Form, Input, InputNumber, Modal, Select, Space, Table, Tag, Typography, message } from 'antd'
import type { TableColumnsType, TablePaginationConfig } from 'antd'
import type { ChangeEvent } from 'react'
import { createMachine } from '../../services/companiesApi'
import type { CreateMachinePayload } from '../../services/companiesApi'
import { getMachines } from '../../services/machinesApi'
import type { MachineFilters, MachineGridItem, MachinePage } from '../../services/machinesApi'

interface Props {
  onSelectCompany: (companyId: number) => void
}

const PAGE_SIZE = 50
const initialFilters: MachineFilters = {
  search: '',
  manufacturerId: undefined,
  status: undefined,
  city: '',
}

const manufacturers = [
  { value: 1, label: 'Volvo' },
  { value: 2, label: 'Hyundai' },
  { value: 3, label: 'Doosan' },
  { value: 4, label: 'Develon' },
  { value: 5, label: 'Hitachi' },
  { value: 6, label: 'Caterpillar' },
]

const statuses = [
  { value: 0, label: 'Sold' },
  { value: 1, label: 'Commissioned' },
  { value: 2, label: 'Decommissioned' },
  { value: 3, label: 'Candidate' },
  { value: 4, label: 'Bought' },
]

function MachinesView({ onSelectCompany }: Props) {
  const [page, setPage] = useState<MachinePage>({ items: [], page: 1, pageSize: PAGE_SIZE, totalCount: 0 })
  const [currentPage, setCurrentPage] = useState(1)
  const [filters, setFilters] = useState<MachineFilters>(initialFilters)
  const [draftFilters, setDraftFilters] = useState<MachineFilters>(initialFilters)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [addMachineOpen, setAddMachineOpen] = useState(false)
  const [savingMachine, setSavingMachine] = useState(false)
  const [reloadKey, setReloadKey] = useState(0)
  const [form] = Form.useForm()
  const [messageApi, contextHolder] = message.useMessage()

  const loadMachines = useCallback(async (pageNumber: number, activeFilters: MachineFilters, signal: AbortSignal) => {
    try {
      const result = await getMachines(pageNumber, activeFilters, signal)
      setPage(result)
      setError(null)
    } catch (loadError) {
      if (loadError instanceof Error && loadError.name === 'AbortError') return
      setError(loadError instanceof Error ? loadError.message : 'An unexpected error occurred.')
    } finally {
      if (!signal.aborted) setLoading(false)
    }
  }, [])

  useEffect(() => {
    const controller = new AbortController()
    void Promise.resolve().then(() => {
      if (!controller.signal.aborted) void loadMachines(currentPage, filters, controller.signal)
    })
    return () => controller.abort()
  }, [currentPage, filters, loadMachines, reloadKey])

  const applyFilters = () => {
    setLoading(true)
    setCurrentPage(1)
    setFilters({ ...draftFilters })
  }

  const clearFilters = () => {
    setLoading(true)
    setDraftFilters({ ...initialFilters })
    setCurrentPage(1)
    setFilters({ ...initialFilters })
  }

  const handleTableChange = (pagination: TablePaginationConfig) => {
    setLoading(true)
    setCurrentPage(pagination.current ?? 1)
  }

  const openAddMachine = () => {
    form.resetFields()
    form.setFieldsValue({ status: 0 })
    setAddMachineOpen(true)
  }

  const closeAddMachine = () => {
    setAddMachineOpen(false)
    form.resetFields()
  }

  const submitNewMachine = async (values: CreateMachinePayload) => {
    setSavingMachine(true)
    try {
      await createMachine(values)
      messageApi.success('Machine added successfully')
      closeAddMachine()
      setCurrentPage(1)
      setLoading(true)
      setReloadKey((key) => key + 1)
    } catch (saveError) {
      messageApi.error(saveError instanceof Error ? saveError.message : 'Could not add machine')
    } finally {
      setSavingMachine(false)
    }
  }

  const columns: TableColumnsType<MachineGridItem> = [
    {
      title: 'Machine',
      key: 'machine',
      width: 260,
      render: (_, machine) => <div className="company-name-cell">
        <Typography.Text strong>{machine.manufacturerName} {machine.model}</Typography.Text>
        <Typography.Text type="secondary" className="business-id">{machine.year ?? 'Year unknown'} · {machine.vinNumber || 'No VIN'}</Typography.Text>
      </div>,
    },
    {
      title: 'Company',
      key: 'company',
      width: 220,
      render: (_, machine) => machine.companyId && machine.companyName
        ? <Button type="link" className="machine-company-link" onClick={() => onSelectCompany(machine.companyId!)}>{machine.companyName}</Button>
        : <Typography.Text type="secondary">Unassigned</Typography.Text>,
    },
    { title: 'City', dataIndex: 'companyCity', key: 'companyCity', width: 150, render: (city: string | null) => city || '—' },
    { title: 'Registration', dataIndex: 'registrationNumber', key: 'registrationNumber', width: 160, render: (value: string | null) => value || '—' },
    { title: 'Purchase year', dataIndex: 'purchaseYear', key: 'purchaseYear', width: 130, render: (value: number | null) => value ?? '—' },
    { title: 'Country of import', dataIndex: 'countryOfImport', key: 'countryOfImport', width: 170, render: (value: string | null) => value || '—' },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      width: 150,
      render: (status: number | null) => status === null
        ? '—'
        : <Tag color={status === 1 ? 'green' : status === 0 ? 'blue' : 'default'}>{statuses.find((item) => item.value === status)?.label ?? 'Unknown'}</Tag>,
    },
  ]

  return (
    <section className="companies-view machines-view">
      {contextHolder}
      <div className="view-heading">
        <div>
          <Typography.Title level={2}>Machines</Typography.Title>
          <Typography.Paragraph type="secondary">Browse and filter machines in your directory.</Typography.Paragraph>
        </div>
        <div className="machines-heading-actions">
          <Button type="primary" onClick={openAddMachine}>Add machine</Button>
          <div className="company-count">
            <span className="count-value">{page.totalCount.toLocaleString()}</span>
            <span className="count-label">machines</span>
          </div>
        </div>
      </div>

      <div className="filter-card">
        <div className="filter-grid machine-filter-grid">
          <label className="filter-field filter-search">
            <span>Search machines</span>
            <Input
              size="large"
              allowClear
              placeholder="Model, VIN, registration, company..."
              value={draftFilters.search}
              onChange={(event: ChangeEvent<HTMLInputElement>) => setDraftFilters({ ...draftFilters, search: event.target.value })}
              onPressEnter={applyFilters}
            />
          </label>
          <label className="filter-field">
            <span>Manufacturer</span>
            <Select
              size="large"
              allowClear
              placeholder="All manufacturers"
              value={draftFilters.manufacturerId}
              onChange={(manufacturerId: number | undefined) => setDraftFilters({ ...draftFilters, manufacturerId })}
              options={manufacturers}
            />
          </label>
          <label className="filter-field">
            <span>Status</span>
            <Select
              size="large"
              allowClear
              placeholder="All statuses"
              value={draftFilters.status}
              onChange={(status: number | undefined) => setDraftFilters({ ...draftFilters, status })}
              options={statuses}
            />
          </label>
          <label className="filter-field">
            <span>Company city</span>
            <Input
              size="large"
              allowClear
              placeholder="All cities"
              value={draftFilters.city}
              onChange={(event: ChangeEvent<HTMLInputElement>) => setDraftFilters({ ...draftFilters, city: event.target.value })}
              onPressEnter={applyFilters}
            />
          </label>
        </div>
        <div className="filter-actions">
          <Space>
            <Button onClick={clearFilters}>Clear filters</Button>
            <Button type="primary" onClick={applyFilters}>Apply filters</Button>
          </Space>
        </div>
      </div>

      {error && <Alert className="companies-error" type="error" showIcon message="Could not load machines" description={error} />}

      <div className="table-card">
        <div className="table-heading">
          <Typography.Title level={5}>Machine directory</Typography.Title>
          <Typography.Text type="secondary">Showing up to {PAGE_SIZE} machines per page</Typography.Text>
        </div>
        <Table<MachineGridItem>
          rowKey="id"
          columns={columns}
          dataSource={page.items}
          loading={loading}
          onChange={handleTableChange}
          pagination={{
            current: currentPage,
            pageSize: PAGE_SIZE,
            total: page.totalCount,
            showSizeChanger: false,
            showTotal: (total: number, range: [number, number]) => `${range[0]}–${range[1]} of ${total} machines`,
            position: ['bottomRight'],
          }}
          scroll={{ x: 1100 }}
          locale={{ emptyText: loading ? 'Loading machines…' : 'No machines match these filters' }}
        />
      </div>

      <Modal
        title="Add an unassigned machine"
        open={addMachineOpen}
        onCancel={closeAddMachine}
        onOk={() => form.submit()}
        confirmLoading={savingMachine}
        okText="Save machine"
        destroyOnHidden
      >
        <Form form={form} layout="vertical" onFinish={(values: CreateMachinePayload) => void submitNewMachine(values)}>
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
            <Form.Item name="status" label="Status"><Select allowClear options={statuses} /></Form.Item>
          </div>
        </Form>
      </Modal>
    </section>
  )
}

export default MachinesView