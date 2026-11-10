import AddButton from '../../components/AddButton'
import AddUpdateMachineryDialog from './AddUpdateMachineryDialog/AddUpdateMachineryDialog'
import { manufacturers, machineryStatuses } from './machineryOptions'
import MachineryGrid from './MachineryGrid'
import GridFilters from '../../components/grid/GridFilters'
import MachineryLibraryDialog from './MachineryLibraryDialog'
import { useCallback, useEffect, useState } from 'react'
import { Alert, Button, Input, Select, Space, Typography, message } from 'antd'
import type { TablePaginationConfig } from 'antd'
import type { ChangeEvent } from 'react'
import { createMachinery, updateMachinery } from '../../services/companiesApi'
import type { CreateMachineryPayload, UpdateMachineryPayload } from '../../services/companiesApi'
import { getMachinery, getMachineryLibrary } from '../../services/machineryApi'
import type { MachineryFilters, MachineryGridItem, MachineryLibraryItem, MachineryPage } from '../../services/machineryApi'

interface Props {
  isAdmin: boolean
  onSelectCompany: (companyId: number) => void
}

const PAGE_SIZE = 50
const initialFilters: MachineryFilters = {
  search: '',
  manufacturerId: undefined,
  machineryLibraryId: undefined,
  status: undefined,
}

function MachineryView({ isAdmin, onSelectCompany }: Props) {
  const [page, setPage] = useState<MachineryPage>({ items: [], page: 1, pageSize: PAGE_SIZE, totalCount: 0 })
  const [currentPage, setCurrentPage] = useState(1)
  const [filters, setFilters] = useState<MachineryFilters>(initialFilters)
  const [draftFilters, setDraftFilters] = useState<MachineryFilters>(initialFilters)
  const [loading, setLoading] = useState(true)
  const [library, setLibrary] = useState<MachineryLibraryItem[]>([])
  const [loadingLibrary, setLoadingLibrary] = useState(true)
  const [libraryError, setLibraryError] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [libraryDialogOpen, setLibraryDialogOpen] = useState(false)
  const [machineryDialogOpen, setMachineryDialogOpen] = useState(false)
  const [savingMachinery, setSavingMachinery] = useState(false)
  const [editingMachinery, setEditingMachinery] = useState<MachineryGridItem | null>(null)
  const [reloadKey, setReloadKey] = useState(0)
  const [messageApi, contextHolder] = message.useMessage()

  const loadMachinery = useCallback(async (pageNumber: number, activeFilters: MachineryFilters, signal: AbortSignal) => {
    try {
      const result = await getMachinery(pageNumber, activeFilters, signal)
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
      if (!controller.signal.aborted) void loadMachinery(currentPage, filters, controller.signal)
    })
    return () => controller.abort()
  }, [currentPage, filters, loadMachinery, reloadKey])

  useEffect(() => {
    const controller = new AbortController()
    void getMachineryLibrary(controller.signal)
      .then((items) => {
        if (!controller.signal.aborted) {
          setLibrary(items)
          setLibraryError(null)
        }
      })
      .catch((loadError: unknown) => {
        if (!controller.signal.aborted) {
          setLibraryError(loadError instanceof Error ? loadError.message : 'Could not load machinery library')
        }
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoadingLibrary(false)
      })
    return () => controller.abort()
  }, [reloadKey])

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

  const openAddMachinery = () => {
    setEditingMachinery(null)
    setMachineryDialogOpen(true)
  }

  const closeMachineryDialog = () => {
    if (savingMachinery) return
    setMachineryDialogOpen(false)
    setEditingMachinery(null)
  }

  const openEditMachinery = (machinery: MachineryGridItem) => {
    setEditingMachinery(machinery)
    setMachineryDialogOpen(true)
  }

  const submitMachinery = async (values: CreateMachineryPayload | UpdateMachineryPayload) => {
    if (savingMachinery) return
    setSavingMachinery(true)
    try {
      if (editingMachinery) {
        await updateMachinery(editingMachinery.id, { ...values, status: values.status ?? null } as UpdateMachineryPayload)
      } else {
        await createMachinery(values as CreateMachineryPayload)
        setCurrentPage(1)
      }
      messageApi.success(editingMachinery ? 'Machinery updated successfully' : 'Machinery added successfully')
      setMachineryDialogOpen(false)
      setEditingMachinery(null)
      setLoading(true)
      setReloadKey((key) => key + 1)
    } catch (saveError) {
      messageApi.error(saveError instanceof Error ? saveError.message : 'Could not save machinery')
    } finally {
      setSavingMachinery(false)
    }
  }

  return (
    <section className="companies-view machinery-view">
      {contextHolder}
      <div className="view-heading">
        <div>
          <Typography.Title level={2}>Machinery</Typography.Title>
          <Typography.Paragraph type="secondary">Browse and filter machinery in your directory.</Typography.Paragraph>
        </div>
        <div className="machinery-heading-actions">
          <Button onClick={() => setLibraryDialogOpen(true)}
            icon={<svg width="16" height="16" viewBox="0 0 16 16" fill="none" stroke="currentColor"
              strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
              <path d="M2 2h3v12H2zM5 2h3v12H5zM9 3l3-1 4 11-3 1z" />
            </svg>}>Library</Button>
          <AddButton aria-label="Add machinery" onClick={openAddMachinery} />
          <div className="company-count">
            <span className="count-value">{page.totalCount.toLocaleString()}</span>
            <span className="count-label">machinery</span>
          </div>
        </div>
      </div>

      <GridFilters values={[
        { label: 'Search', value: filters.search },
        { label: 'Manufacturer', value: manufacturers.find(item => item.value === filters.manufacturerId)?.label ?? filters.manufacturerId },
        { label: 'Machinery', value: (() => {
          const item = library.find(item => item.id === filters.machineryLibraryId)
          return item ? `${item.manufacturerName} ${item.model}` : filters.machineryLibraryId
        })() },
        { label: 'Status', value: machineryStatuses.find(item => item.value === filters.status)?.label ?? filters.status },
      ]} filterActions={<Space>
        <Button onClick={clearFilters}>Clear filters</Button>
        <Button type="primary" onClick={applyFilters}>Apply filters</Button>
      </Space>}>
        <div className="filter-grid machinery-filter-grid">
          <label className="filter-field filter-search">
            <span>Search machinery</span>
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
            <span>Machinery</span>
            <Select
              size="large"
              allowClear
              showSearch={{ optionFilterProp: 'label' }}
              placeholder="All machinery"
              loading={loadingLibrary}
              disabled={loadingLibrary || libraryError !== null}
              value={draftFilters.machineryLibraryId}
              onChange={(machineryLibraryId: number | undefined) => setDraftFilters({ ...draftFilters, machineryLibraryId })}
              options={library.map((item) => ({ value: item.id, label: `${item.manufacturerName} ${item.model}` }))}
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
              options={machineryStatuses}
            />
          </label>
        </div>
      </GridFilters>

      {error && <Alert className="companies-error" type="error" showIcon title="Could not load machinery" description={error} />}
      {libraryError && <Alert className="companies-error" type="error" showIcon title="Could not load machinery library" description={libraryError} />}

      <MachineryGrid onEdit={openEditMachinery} onSelectCompany={onSelectCompany}
          dataSource={page.items}
          loading={loading}
          onChange={handleTableChange}
          pagination={{
            current: currentPage,
            pageSize: PAGE_SIZE,
            total: page.totalCount,
            showSizeChanger: false,
            showTotal: (total: number, range: [number, number]) => `${range[0]}–${range[1]} of ${total} machinery`,
            placement: ['bottomEnd'],
          }}
          scroll={{ x: 1100 }}
          locale={{ emptyText: loading ? 'Loading machinery…' : 'No machinery matches these filters' }}
        />

      {libraryDialogOpen && <MachineryLibraryDialog isAdmin={isAdmin}
        onCancel={() => setLibraryDialogOpen(false)}
        onChanged={() => { setLoading(true); setReloadKey((key) => key + 1) }} />}

      {machineryDialogOpen && <AddUpdateMachineryDialog mode={editingMachinery ? 'update' : 'add'} open
        initialValues={editingMachinery ?? undefined}
        saving={savingMachinery} onCancel={closeMachineryDialog} onSubmit={submitMachinery} />}
    </section>
  )
}

export default MachineryView
