import { useCallback, useEffect, useState } from 'react'
import { Alert, Button, Checkbox, ConfigProvider, Form, Input, Modal, Select, Space, Table, Tag, Typography, message } from 'antd'
import type { TableColumnsType, TablePaginationConfig } from 'antd'
import type { ChangeEvent } from 'react'
import { createCompany, getCompanies } from '../../services/companiesApi'
import type { Company, CompanyFilters, CompanyPage, CreateCompanyPayload } from '../../services/companiesApi'

interface Props {
  onSelectCompany: (companyId: number) => void
}

const PAGE_SIZE = 100
const initialFilters: CompanyFilters = {
  search: '',
  city: '',
  businessLine: '',
  isOurPartner: undefined,
}

const columns: TableColumnsType<Company> = [
  {
    title: 'Company',
    dataIndex: 'name',
    key: 'name',
    width: 260,
    render: (name: string, company) => (
      <div className="company-name-cell">
        <Typography.Text strong>{name}</Typography.Text>
        <Typography.Text type="secondary" className="business-id">{company.businessId || 'Business ID unavailable'}</Typography.Text>
      </div>
    ),
  },
  { title: 'City', dataIndex: 'city', key: 'city', width: 160, render: (value: string | null) => value || '—' },
  { title: 'Address', dataIndex: 'address', key: 'address', width: 220, render: (value: string | null) => value || '—' },
  {
    title: 'Business line',
    dataIndex: 'mainBusinessLine',
    key: 'mainBusinessLine',
    width: 260,
    render: (value: string | null) => value || '—',
  },
  {
    title: 'Partner',
    dataIndex: 'isOurPartner',
    key: 'isOurPartner',
    width: 120,
    render: (isPartner: boolean) => isPartner
      ? <Tag color="blue">Partner</Tag>
      : <Typography.Text type="secondary">—</Typography.Text>,
  },
]

function CompaniesView({ onSelectCompany }: Props) {
  const [page, setPage] = useState<CompanyPage>({ items: [], page: 1, pageSize: PAGE_SIZE, totalCount: 0 })
  const [currentPage, setCurrentPage] = useState(1)
  const [filters, setFilters] = useState<CompanyFilters>(initialFilters)
  const [draftFilters, setDraftFilters] = useState<CompanyFilters>(initialFilters)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [addCompanyOpen, setAddCompanyOpen] = useState(false)
  const [savingCompany, setSavingCompany] = useState(false)
  const [reloadKey, setReloadKey] = useState(0)
  const [form] = Form.useForm<CreateCompanyPayload>()
  const [messageApi, contextHolder] = message.useMessage()

  const loadCompanies = useCallback(async (pageNumber: number, activeFilters: CompanyFilters, signal: AbortSignal) => {
    try {
      const result = await getCompanies(pageNumber, activeFilters, signal)
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
      if (!controller.signal.aborted) {
        void loadCompanies(currentPage, filters, controller.signal)
      }
    })
    return () => controller.abort()
  }, [currentPage, filters, loadCompanies, reloadKey])

  const openAddCompany = () => {
    form.resetFields()
    setAddCompanyOpen(true)
  }

  const closeAddCompany = () => {
    if (savingCompany) return
    setAddCompanyOpen(false)
  }

  const submitNewCompany = async (values: CreateCompanyPayload) => {
    if (savingCompany) return
    setSavingCompany(true)
    try {
      await createCompany(values)
      messageApi.success('Company added successfully')
      setAddCompanyOpen(false)
      setCurrentPage(1)
      setLoading(true)
      setReloadKey((key) => key + 1)
    } catch (saveError) {
      messageApi.error(saveError instanceof Error ? saveError.message : 'Could not add company')
    } finally {
      setSavingCompany(false)
    }
  }

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

  return (
    <section className="companies-view">
      {contextHolder}
      <div className="view-heading">
        <div>
          <Typography.Title level={2}>Companies</Typography.Title>
          <Typography.Paragraph type="secondary">
            Browse and filter registered companies in your directory.
          </Typography.Paragraph>
        </div>
        <div className="companies-heading-actions">
          <Button type="primary" onClick={openAddCompany}>Add company</Button>
          <div className="company-count">
            <span className="count-value">{page.totalCount.toLocaleString()}</span>
            <span className="count-label">companies</span>
          </div>
        </div>
      </div>

      <div className="filter-card">
        <div className="filter-grid">
          <label className="filter-field filter-search">
            <span>Search companies</span>
            <Input
              allowClear
              placeholder="Name, business ID, address..."
              value={draftFilters.search}
              onChange={(event: ChangeEvent<HTMLInputElement>) => setDraftFilters({ ...draftFilters, search: event.target.value })}
              onPressEnter={applyFilters}
            />
          </label>
          <label className="filter-field">
            <span>City</span>
            <Input
              allowClear
              placeholder="All cities"
              value={draftFilters.city}
              onChange={(event: ChangeEvent<HTMLInputElement>) => setDraftFilters({ ...draftFilters, city: event.target.value })}
              onPressEnter={applyFilters}
            />
          </label>
          <label className="filter-field">
            <span>Business line</span>
            <Input
              allowClear
              placeholder="Any business line"
              value={draftFilters.businessLine}
              onChange={(event: ChangeEvent<HTMLInputElement>) => setDraftFilters({ ...draftFilters, businessLine: event.target.value })}
              onPressEnter={applyFilters}
            />
          </label>
          <label className="filter-field partner-filter">
            <span>Partner status</span>
            <Select
              value={draftFilters.isOurPartner === undefined ? 'all' : String(draftFilters.isOurPartner)}
              onChange={(value: string) => setDraftFilters({
                ...draftFilters,
                isOurPartner: value === 'all' ? undefined : value === 'true',
              })}
              options={[
                { label: 'All companies', value: 'all' },
                { label: 'Partners only', value: 'true' },
                { label: 'Non-partners', value: 'false' },
              ]}
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

      {error && <Alert className="companies-error" type="error" showIcon message="Could not load companies" description={error} />}

      <div className="table-card">
        <Table<Company>
          rowKey="id"
          columns={columns}
          dataSource={page.items}
          loading={loading}
          onChange={handleTableChange}
          onRow={(company) => ({
            onClick: () => onSelectCompany(company.id),
            className: 'company-table-row',
            'aria-label': `Open ${company.name}`,
          })}
          pagination={{
            current: currentPage,
            pageSize: PAGE_SIZE,
            total: page.totalCount,
            showSizeChanger: false,
            showTotal: (total: number, range: [number, number]) => `${range[0]}–${range[1]} of ${total} companies`,
            position: ['bottomRight'],
          }}
          scroll={{ x: 900 }}
          locale={{ emptyText: loading ? 'Loading companies…' : 'No companies match these filters' }}
        />
      </div>
      <Modal
        title="Add company"
        open={addCompanyOpen}
        onCancel={closeAddCompany}
        onOk={() => form.submit()}
        confirmLoading={savingCompany}
        cancelButtonProps={{ disabled: savingCompany }}
        okText="Add company"
        destroyOnHidden
      >
        <Form form={form} layout="vertical" initialValues={{ isOurPartner: false }} onFinish={submitNewCompany} disabled={savingCompany}>
          <Form.Item name="name" label="Company name" rules={[{ required: true, whitespace: true, message: 'Enter a company name' }, { max: 200 }]}>
            <Input maxLength={200} />
          </Form.Item>
          <div className="form-two-columns">
            <Form.Item name="businessId" label="Business ID"><Input maxLength={50} /></Form.Item>
            <Form.Item name="city" label="City"><Input maxLength={200} /></Form.Item>
          </div>
          <Form.Item name="address" label="Address"><Input maxLength={500} /></Form.Item>
          <ConfigProvider theme={{ token: { controlInteractiveSize: 32 } }}>
            <Form.Item name="isOurPartner" valuePropName="checked">
              <Checkbox className="company-partner-checkbox">Our partner</Checkbox>
            </Form.Item>
          </ConfigProvider>
        </Form>
      </Modal>
    </section>
  )
}

export default CompaniesView
