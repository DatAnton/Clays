import AddUpdateMachineDialog from './AddUpdateMachineDialog/AddUpdateMachineDialog'
import { manufacturers, machineStatuses } from './machineOptions'
import MachinesGrid from './MachinesGrid'
import { useCallback, useEffect, useState } from 'react'
import { Alert, Button, Input, Select, Space, Typography, message } from 'antd'
import type { TablePaginationConfig } from 'antd'
import type { ChangeEvent } from 'react'
import { createMachine, updateMachine } from '../../services/companiesApi'
import type { CreateMachinePayload, UpdateMachinePayload } from '../../services/companiesApi'
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

function MachinesView({ onSelectCompany }: Props) {
  const [page, setPage] = useState<MachinePage>({ items: [], page: 1, pageSize: PAGE_SIZE, totalCount: 0 })
  const [currentPage, setCurrentPage] = useState(1)
  const [filters, setFilters] = useState<MachineFilters>(initialFilters)
  const [draftFilters, setDraftFilters] = useState<MachineFilters>(initialFilters)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [machineDialogOpen, setMachineDialogOpen] = useState(false)
  const [savingMachine, setSavingMachine] = useState(false)
  const [editingMachine, setEditingMachine] = useState<MachineGridItem | null>(null)
  const [reloadKey, setReloadKey] = useState(0)
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
    setEditingMachine(null)
    setMachineDialogOpen(true)
  }

  const closeMachineDialog = () => {
    if (savingMachine) return
    setMachineDialogOpen(false)
    setEditingMachine(null)
  }

  const openEditMachine = (machine: MachineGridItem) => {
    setEditingMachine(machine)
    setMachineDialogOpen(true)
  }

  const submitMachine = async (values: CreateMachinePayload | UpdateMachinePayload) => {
    if (savingMachine) return
    setSavingMachine(true)
    try {
      if (editingMachine) {
        await updateMachine(editingMachine.id, { ...values, status: values.status ?? null } as UpdateMachinePayload)
      } else {
        await createMachine(values as CreateMachinePayload)
        setCurrentPage(1)
      }
      messageApi.success(editingMachine ? 'Machine updated successfully' : 'Machine added successfully')
      setMachineDialogOpen(false)
      setEditingMachine(null)
      setLoading(true)
      setReloadKey((key) => key + 1)
    } catch (saveError) {
      messageApi.error(saveError instanceof Error ? saveError.message : 'Could not save machine')
    } finally {
      setSavingMachine(false)
    }
  }

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
              options={machineStatuses}
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

      <MachinesGrid onEdit={openEditMachine} onSelectCompany={onSelectCompany}
          dataSource={page.items}
          loading={loading}
          onChange={handleTableChange}
          pagination={{
            current: currentPage,
            pageSize: PAGE_SIZE,
            total: page.totalCount,
            showSizeChanger: false,
            showTotal: (total: number, range: [number, number]) => `${range[0]}–${range[1]} of ${total} machines`,
            placement: ['bottomEnd'],
          }}
          scroll={{ x: 1100 }}
          locale={{ emptyText: loading ? 'Loading machines…' : 'No machines match these filters' }}
        />

      {machineDialogOpen && <AddUpdateMachineDialog mode={editingMachine ? 'update' : 'add'} open
        initialValues={editingMachine ?? undefined}
        saving={savingMachine} onCancel={closeMachineDialog} onSubmit={submitMachine} />}
    </section>
  )
}

export default MachinesView
