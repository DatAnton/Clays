import CompaniesGrid from './CompaniesGrid'
import { useCallback, useEffect, useState } from 'react'
import { Alert, Button, Checkbox, ConfigProvider, Form, Input, Modal, Select, Space, Typography, message } from 'antd'
import type { TablePaginationConfig } from 'antd'
import type { ChangeEvent } from 'react'
import { createCompany, getCompanies, getCompany, updateCompany } from '../../services/companiesApi'
import type { Company, CompanyDetails, CompanyFilters, CompanyPage, CreateCompanyPayload } from '../../services/companiesApi'

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

function CompaniesView({ onSelectCompany }: Props) {
  const [page, setPage] = useState<CompanyPage>({ items: [], page: 1, pageSize: PAGE_SIZE, totalCount: 0 })
  const [currentPage, setCurrentPage] = useState(1)
  const [filters, setFilters] = useState<CompanyFilters>(initialFilters)
  const [draftFilters, setDraftFilters] = useState<CompanyFilters>(initialFilters)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [companyDialogOpen, setCompanyDialogOpen] = useState(false)
  const [editingCompany, setEditingCompany] = useState<CompanyDetails | null>(null)
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
    setEditingCompany(null)
    setCompanyDialogOpen(true)
  }

  const openEditCompany = async (company: Company) => {
    try {
      const details = await getCompany(company.id)
      form.resetFields()
      form.setFieldsValue({
        name: details.name,
        businessId: details.businessId ?? undefined,
        city: details.city ?? undefined,
        address: details.address ?? undefined,
        mainBusinessLine: details.mainBusinessLine ?? undefined,
        isOurPartner: details.isOurPartner,
      })
      setEditingCompany(details)
      setCompanyDialogOpen(true)
    } catch (loadError) {
      messageApi.error(loadError instanceof Error ? loadError.message : 'Could not load company for editing')
    }
  }

  const closeCompanyDialog = () => {
    if (savingCompany) return
    setCompanyDialogOpen(false)
  }

  const submitCompany = async (values: CreateCompanyPayload) => {
    if (savingCompany) return
    setSavingCompany(true)
    try {
      if (editingCompany) {
        await updateCompany(editingCompany.id, {
          name: values.name,
          businessId: values.businessId ?? null,
          city: values.city ?? null,
          address: values.address ?? null,
          mainBusinessLine: values.mainBusinessLine ?? null,
          isOurPartner: values.isOurPartner,
          status: editingCompany.status,
          note: editingCompany.note,
        })
      } else {
        await createCompany(values)
      }
      messageApi.success(editingCompany ? 'Company updated successfully' : 'Company added successfully')
      setCompanyDialogOpen(false)
      if (!editingCompany) setCurrentPage(1)
      setLoading(true)
      setReloadKey((key) => key + 1)
    } catch (saveError) {
      messageApi.error(saveError instanceof Error ? saveError.message : 'Could not save company')
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

      <CompaniesGrid onEdit={openEditCompany}
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
            placement: ['bottomEnd'],
          }}
          scroll={{ x: 900 }}
          locale={{ emptyText: loading ? 'Loading companies…' : 'No companies match these filters' }}
        />
      <Modal
        title={editingCompany ? 'Edit company' : 'Add company'}
        open={companyDialogOpen}
        onCancel={closeCompanyDialog}
        onOk={() => form.submit()}
        confirmLoading={savingCompany}
        cancelButtonProps={{ disabled: savingCompany }}
        okText={editingCompany ? 'Save changes' : 'Add company'}
        closable={!savingCompany}
        mask={{ closable: !savingCompany }}
        destroyOnHidden
      >
        <Form form={form} layout="vertical" initialValues={{ isOurPartner: false }} onFinish={submitCompany} disabled={savingCompany}>
          <Form.Item name="name" label="Company name" rules={[{ required: true, whitespace: true, message: 'Enter a company name' }, { max: 200 }]}>
            <Input maxLength={200} />
          </Form.Item>
          <div className="form-two-columns">
            <Form.Item name="businessId" label="Business ID"><Input maxLength={50} /></Form.Item>
            <Form.Item name="city" label="City"><Input maxLength={200} /></Form.Item>
          </div>
          <Form.Item name="address" label="Address"><Input maxLength={500} /></Form.Item>
          <Form.Item name="mainBusinessLine" label="Business line"><Input maxLength={500} /></Form.Item>
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
