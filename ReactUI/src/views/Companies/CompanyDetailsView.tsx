import { useCallback, useEffect, useState } from 'react'
import { Alert, Button, Card, Descriptions, Form, Input, InputNumber, Modal, Popover, Select, Space, Spin, Table, Tag, Typography, message } from 'antd'
import {
  createAction,
  createCompanyComment,
  createContact,
  createMachine,
  getCompany,
  updateActionStatus,
} from '../../services/companiesApi'
import type {
  CompanyAction,
  CompanyComment,
  CompanyDetails,
  Contact,
  CreateActionPayload,
  CreateCommentPayload,
  CreateContactPayload,
  CreateMachinePayload,
  Machine,
} from '../../services/companiesApi'

interface Props {
  companyId: number
  onBack: () => void
}

type DialogKind = 'comment' | 'contact' | 'machine' | 'action' | null

const actionStatuses = [
  { value: 0, label: 'To do' },
  { value: 1, label: 'In progress' },
  { value: 2, label: 'Done' },
]
const actionTypes = [
  { value: 0, label: 'Call' },
  { value: 1, label: 'Visit' },
  { value: 2, label: 'Other' },
]
const machineStatuses = [
  { value: 0, label: 'Sold' },
  { value: 1, label: 'Commissioned' },
  { value: 2, label: 'Decommissioned' },
  { value: 3, label: 'Candidate' },
  { value: 4, label: 'Bought' },
]
const manufacturers = [
  { value: 1, label: 'Volvo' },
  { value: 2, label: 'Hyundai' },
  { value: 3, label: 'Doosan' },
  { value: 4, label: 'Develon' },
  { value: 5, label: 'Hitachi' },
  { value: 6, label: 'Caterpillar' },
]

const displayName = (user: { firstName: string | null; lastName: string | null; userName: string | null }) =>
  [user.firstName, user.lastName].filter(Boolean).join(' ') || user.userName || 'Unknown user'

function CompanyDetailsView({ companyId, onBack }: Props) {
  const [company, setCompany] = useState<CompanyDetails | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [dialog, setDialog] = useState<DialogKind>(null)
  const [saving, setSaving] = useState(false)
  const [busyActionId, setBusyActionId] = useState<number | null>(null)
  const [form] = Form.useForm()
  const [messageApi, contextHolder] = message.useMessage()

  const loadCompany = useCallback(async (signal?: AbortSignal) => {
    try {
      const result = await getCompany(companyId, signal)
      setCompany(result)
      setError(null)
    } catch (loadError) {
      if (loadError instanceof Error && loadError.name === 'AbortError') return
      setError(loadError instanceof Error ? loadError.message : 'Could not load company details.')
    } finally {
      if (!signal?.aborted) setLoading(false)
    }
  }, [companyId])

  useEffect(() => {
    const controller = new AbortController()
    void Promise.resolve().then(() => {
      if (!controller.signal.aborted) void loadCompany(controller.signal)
    })
    return () => controller.abort()
  }, [loadCompany])

  const openDialog = (kind: Exclude<DialogKind, null>) => {
    form.resetFields()
    form.setFieldsValue({ status: 0, type: 0, actionWeek: getCurrentWeek() })
    setDialog(kind)
  }

  const closeDialog = () => {
    setDialog(null)
    form.resetFields()
  }

  const submitDialog = async (values: Record<string, string | number | undefined>) => {
    setSaving(true)
    try {
      if (dialog === 'comment') {
        await createCompanyComment(companyId, values as unknown as CreateCommentPayload)
      } else if (dialog === 'contact') {
        await createContact({ ...values, companyId } as CreateContactPayload)
      } else if (dialog === 'machine') {
        await createMachine({ ...values, companyId } as unknown as CreateMachinePayload)
      } else if (dialog === 'action') {
        await createAction({ ...values, companyId } as CreateActionPayload)
      }
      messageApi.success('Saved successfully')
      closeDialog()
      await loadCompany()
    } catch (saveError) {
      messageApi.error(saveError instanceof Error ? saveError.message : 'Could not save changes')
    } finally {
      setSaving(false)
    }
  }

  const changeActionStatus = async (action: CompanyAction, status: number) => {
    setBusyActionId(action.id)
    try {
      await updateActionStatus(action.id, status)
      messageApi.success('Action status updated')
      await loadCompany()
    } catch (updateError) {
      messageApi.error(updateError instanceof Error ? updateError.message : 'Could not update action')
    } finally {
      setBusyActionId(null)
    }
  }

  if (loading) return <div className="company-detail-loading"><Spin size="large" /></div>
  if (error || !company) {
    return <section className="company-detail-view">
      <Button type="link" onClick={onBack}>← Back to companies</Button>
      <Alert type="error" showIcon message="Could not load company" description={error || 'Company not found'} />
    </section>
  }

  const machines = new Map<number, Machine>()
  company.machines.forEach((machine) => machines.set(machine.id, machine))
  company.ownerHistory.forEach((entry) => machines.set(entry.machine.id, entry.machine))
  company.actions.forEach((action) => {
    if (action.machine) machines.set(action.machine.id, action.machine)
  })

  const titleForDialog: Record<Exclude<DialogKind, null>, string> = {
    comment: 'Add a comment',
    contact: 'Add a contact',
    machine: 'Add a machine',
    action: 'Create an action',
  }

  return (
    <section className="company-detail-view">
      {contextHolder}
      <div className="detail-back-row"><Button type="link" onClick={onBack}>← Companies</Button><Typography.Text type="secondary">Company profile</Typography.Text></div>
      <div className="detail-hero">
        <div>
          <Popover
            trigger={['hover', 'focus']}
            placement="bottomLeft"
            content={<Descriptions size="small" column={1}>
              <Descriptions.Item label="Business ID">{company.businessId || '—'}</Descriptions.Item>
              <Descriptions.Item label="Address">{company.address || '—'}</Descriptions.Item>
              <Descriptions.Item label="Business line">{company.mainBusinessLine || '—'}</Descriptions.Item>
              <Descriptions.Item label="Created by">{company.createdBy ? displayName(company.createdBy) : '—'}</Descriptions.Item>
              <Descriptions.Item label="Notes">{company.note || '—'}</Descriptions.Item>
              <Descriptions.Item label="Partner">{company.isOurPartner ? 'Yes' : 'No'}</Descriptions.Item>
              <Descriptions.Item label="Status">{company.status === null ? '—' : companyStatusLabel(company.status)}</Descriptions.Item>
            </Descriptions>}
          >
            <div className="company-summary-trigger" tabIndex={0} aria-label="Company details; hover to view more">
              <Typography.Title level={2}>{company.name}</Typography.Title>
              <Typography.Paragraph type="secondary">{company.city || 'City unavailable'}</Typography.Paragraph>
            </div>
          </Popover>
        </div>
      </div>

      <div className="company-detail-grid">
        <Card title={`Actions · ${company.actions.length}`} extra={<Button size="small" type="primary" onClick={() => openDialog('action')}>New action</Button>} className="detail-card">
          {company.actions.length ? <div className="action-card-list">{[...company.actions].sort((a, b) => b.createdDate.localeCompare(a.createdDate)).map((action) => <Card
            key={action.id}
            size="small"
            className="action-item-card"
            title={<Space wrap><Tag color={action.status === 2 ? 'green' : action.status === 1 ? 'blue' : action.status === 0 ? 'orange' : 'default'}>{actionStatuses[action.status]?.label ?? 'Unknown'}</Tag><Tag>{actionTypes[action.type]?.label ?? 'Action'}</Tag><Typography.Text strong>{displayName(action.author)}</Typography.Text></Space>}
            extra={<Select aria-label={`Update action ${action.id} status`} value={action.status} loading={busyActionId === action.id} options={actionStatuses} onChange={(status: number) => void changeActionStatus(action, status)} style={{ minWidth: 135 }} />}
          >
            <Typography.Paragraph className="action-note">{action.note || 'No details provided'}</Typography.Paragraph>
            <Typography.Text type="secondary">{new Date(action.createdDate).toLocaleDateString()} · Week {action.actionWeek}{action.assignedUser ? ` · Assigned to ${displayName(action.assignedUser)}` : ''}{action.machine ? ` · ${action.machine.manufacturerName} ${action.machine.model}` : ''}</Typography.Text>
          </Card>)}</div> : <Typography.Text type="secondary">No actions recorded yet.</Typography.Text>}
        </Card>

        <Card title={`Comments · ${company.comments.length}`} extra={<Button size="small" onClick={() => openDialog('comment')}>Add comment</Button>} className="detail-card">
          {company.comments.length ? <div className="comment-card-list">{[...company.comments].sort((a, b) => b.createdAtUtc.localeCompare(a.createdAtUtc)).map((comment: CompanyComment) => <article className="comment-item-card" key={comment.id}>
            <Typography.Paragraph>{comment.text}</Typography.Paragraph>
            <div className="comment-meta"><Typography.Text strong>{displayName(comment.author)}</Typography.Text><time dateTime={comment.createdAtUtc}>{new Date(comment.createdAtUtc).toLocaleString()}</time></div>
          </article>)}</div> : <Typography.Text type="secondary">No comments yet.</Typography.Text>}
        </Card>

        <Card title={`Machines · ${machines.size}`} extra={<Button size="small" onClick={() => openDialog('machine')}>Add machine</Button>} className="detail-card detail-card-wide">
          {machines.size ? <div className="machine-card-list">{[...machines.values()].map((machine) => {
            const history = company.ownerHistory
              .filter((entry) => entry.machine.id === machine.id)
              .map(({ id }) => ({ id, owner: company.name, city: company.city || '—' }))
            return <Card key={machine.id} size="small" className="machine-item-card" title={<span>{machine.manufacturerName} {machine.model}</span>}>
              <Descriptions size="small" column={{ xs: 1, sm: 2, lg: 3 }}>
                <Descriptions.Item label="Year">{machine.year ?? '—'}</Descriptions.Item>
                <Descriptions.Item label="VIN">{machine.vinNumber || '—'}</Descriptions.Item>
                <Descriptions.Item label="Registration">{machine.registrationNumber || '—'}</Descriptions.Item>
                <Descriptions.Item label="Status">{machine.status === null ? '—' : machineStatuses[machine.status]?.label ?? '—'}</Descriptions.Item>
              </Descriptions>
              <div className="machine-history">
                <Typography.Text className="machine-history-title">Owner history</Typography.Text>
                {history.length ? <Table
                  size="small"
                  pagination={false}
                  rowKey="id"
                  dataSource={history}
                  columns={[
                    { title: 'Owner', dataIndex: 'owner' },
                    { title: 'City', dataIndex: 'city' },
                  ]}
                /> : <Typography.Text type="secondary">No owner history recorded.</Typography.Text>}
              </div>
            </Card>
          })}</div> : <Typography.Text type="secondary">No machines linked to this company yet.</Typography.Text>}
        </Card>

        <Card title={`Contacts · ${company.contacts.length}`} extra={<Button size="small" onClick={() => openDialog('contact')}>Add contact</Button>} className="detail-card detail-card-wide">
          {company.contacts.length ? <div className="detail-list">{company.contacts.map((contact: Contact) => <div className="detail-list-item" key={contact.id}><Typography.Text strong>{[contact.firstName, contact.lastName].filter(Boolean).join(' ') || 'Unnamed contact'}</Typography.Text><Typography.Text type="secondary">{contact.email || contact.phoneNumber || 'No contact details'}</Typography.Text>{contact.email && contact.phoneNumber && <Typography.Text type="secondary">{contact.phoneNumber}</Typography.Text>}</div>)}</div> : <Typography.Text type="secondary">No contacts have been added.</Typography.Text>}
        </Card>
      </div>

      <Modal title={dialog ? titleForDialog[dialog] : ''} open={dialog !== null} onCancel={closeDialog} onOk={() => form.submit()} confirmLoading={saving} okText="Save" destroyOnHidden>
        <Form form={form} layout="vertical" onFinish={(values: Record<string, string | number | undefined>) => void submitDialog(values)}>
          {dialog === 'comment' && <>
            <Form.Item name="text" label="Comment" rules={[{ required: true, whitespace: true, message: 'Enter a comment' }, { max: 4000 }]}><Input.TextArea rows={4} maxLength={4000} showCount /></Form.Item>
            <Form.Item name="authorId" label="Your user ID" rules={[{ required: true, type: 'number', min: 1 }]}><InputNumber min={1} style={{ width: '100%' }} placeholder="Enter your user ID" /></Form.Item>
          </>}
          {dialog === 'contact' && <>
            <div className="form-two-columns"><Form.Item name="firstName" label="First name"><Input maxLength={200} /></Form.Item><Form.Item name="lastName" label="Last name"><Input maxLength={200} /></Form.Item></div>
            <div className="form-two-columns"><Form.Item name="email" label="Email" rules={[{ type: 'email' }]}><Input maxLength={320} /></Form.Item><Form.Item name="phoneNumber" label="Phone"><Input maxLength={50} /></Form.Item></div>
          </>}
          {dialog === 'machine' && <>
            <Form.Item name="manufacturerId" label="Manufacturer" rules={[{ required: true }]}><Select options={manufacturers} placeholder="Select manufacturer" /></Form.Item>
            <Form.Item name="model" label="Model" rules={[{ required: true, whitespace: true }, { max: 200 }]}><Input maxLength={200} /></Form.Item>
            <div className="form-two-columns"><Form.Item name="year" label="Year"><InputNumber min={1800} max={2200} style={{ width: '100%' }} /></Form.Item><Form.Item name="purchaseYear" label="Purchase year"><InputNumber min={1800} max={2200} style={{ width: '100%' }} /></Form.Item></div>
            <div className="form-two-columns"><Form.Item name="vinNumber" label="VIN"><Input maxLength={100} /></Form.Item><Form.Item name="registrationNumber" label="Registration number"><Input maxLength={50} /></Form.Item></div>
            <div className="form-two-columns"><Form.Item name="countryOfImport" label="Country of import"><Input maxLength={100} /></Form.Item><Form.Item name="status" label="Status"><Select allowClear options={machineStatuses} /></Form.Item></div>
            <Form.Item name="createdById" label="Created by user ID" rules={[{ required: true, type: 'number', min: 1 }]}><InputNumber min={1} style={{ width: '100%' }} /></Form.Item>
          </>}
          {dialog === 'action' && <>
            <div className="form-two-columns"><Form.Item name="type" label="Type" rules={[{ required: true }]}><Select options={actionTypes} /></Form.Item><Form.Item name="status" label="Status" rules={[{ required: true }]}><Select options={actionStatuses} /></Form.Item></div>
            <Form.Item name="note" label="Details"><Input.TextArea rows={3} maxLength={4000} /></Form.Item>
            <div className="form-two-columns"><Form.Item name="authorId" label="Author user ID" rules={[{ required: true, type: 'number', min: 1 }]}><InputNumber min={1} style={{ width: '100%' }} /></Form.Item><Form.Item name="assignedUserId" label="Assigned user ID"><InputNumber min={1} style={{ width: '100%' }} /></Form.Item></div>
            <div className="form-two-columns"><Form.Item name="actionWeek" label="Action week" rules={[{ required: true, type: 'number', min: 1, max: 53 }]}><InputNumber min={1} max={53} style={{ width: '100%' }} /></Form.Item><Form.Item name="machineId" label="Machine ID"><InputNumber min={1} style={{ width: '100%' }} /></Form.Item></div>
          </>}
        </Form>
      </Modal>
    </section>
  )
}

function companyStatusLabel(status: number) {
  return ['Action required', 'Nothing interesting', 'Stay in touch', 'Good future contact'][status] ?? 'Status'
}

function getCurrentWeek() {
  const date = new Date()
  const firstDay = new Date(date.getFullYear(), 0, 1)
  return Math.ceil((((date.getTime() - firstDay.getTime()) / 86400000) + firstDay.getDay() + 1) / 7)
}

export default CompanyDetailsView
