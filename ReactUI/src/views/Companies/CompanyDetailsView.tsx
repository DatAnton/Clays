import AddUpdateCompanyDialog from './AddUpdateCompanyDialog/AddUpdateCompanyDialog'
import type { CompanyFormValues } from './AddUpdateCompanyDialog/AddUpdateCompanyDialog'
import AddUpdateMachineryDialog from '../Machinery/AddUpdateMachineryDialog/AddUpdateMachineryDialog'
import AddCommentDialog from './AddCommentDialog/AddCommentDialog'
import AddContactDialog from './AddContactDialog/AddContactDialog'
import AddActionDialog from './AddActionDialog/AddActionDialog'
import { actionStatuses, actionTypes } from './actionOptions'
import { machineryStatuses } from '../Machinery/machineryOptions'
import { useCallback, useEffect, useState } from 'react'
import { Alert, Button, Card, Descriptions, Popover, Select, Space, Spin, Table, Tag, Typography, message } from 'antd'
import {
  createAction,
  createCompanyComment,
  createContact,
  createMachinery,
  getCompany,
  updateActionStatus,
  updateCompany,
  updateMachinery,
} from '../../services/companiesApi'
import type {
  CompanyAction,
  CompanyComment,
  CompanyDetails,
  Contact,
  CreateActionPayload,
  CreateCommentPayload,
  CreateContactPayload,
  Machinery,
  UpdateMachineryPayload,
} from '../../services/companiesApi'

interface Props {
  companyId: number
  onBack: () => void
}

type DialogKind = 'comment' | 'contact' | 'machinery' | 'action' | 'editCompany' | 'editMachinery' | null

const displayName = (user: { firstName: string | null; lastName: string | null; userName: string | null }) =>
  [user.firstName, user.lastName].filter(Boolean).join(' ') || user.userName || 'Unknown user'

function CompanyDetailsView({ companyId, onBack }: Props) {
  const [company, setCompany] = useState<CompanyDetails | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [dialog, setDialog] = useState<DialogKind>(null)
  const [saving, setSaving] = useState(false)
  const [editingMachinery, setEditingMachinery] = useState<Machinery | null>(null)
  const [busyActionId, setBusyActionId] = useState<number | null>(null)
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
    setEditingMachinery(null)
    setDialog(kind)
  }

  const openEditMachinery = (machinery: Machinery) => {
    setEditingMachinery(machinery)
    setDialog('editMachinery')
  }

  const closeDialog = () => {
    if (saving) return
    setDialog(null)
    setEditingMachinery(null)
  }

  const saveChanges = async (save: () => Promise<unknown>) => {
    if (saving) return
    setSaving(true)
    try {
      await save()
      messageApi.success('Saved successfully')
      setDialog(null)
      setEditingMachinery(null)
      await loadCompany()
    } catch (saveError) {
      messageApi.error(saveError instanceof Error ? saveError.message : 'Could not save changes')
    } finally {
      setSaving(false)
    }
  }

  const submitComment = (values: CreateCommentPayload) => saveChanges(() => createCompanyComment(companyId, values))
  const submitContact = (values: CreateContactPayload) => saveChanges(() => createContact({ ...values, companyId }))
  const submitAction = (values: CreateActionPayload) => saveChanges(() => createAction({ ...values, companyId }))
  const submitCompany = (values: CompanyFormValues) => saveChanges(() => updateCompany(companyId, {
    ...values,
    businessId: values.businessId ?? null,
    city: values.city ?? null,
    address: values.address ?? null,
    mainBusinessLine: values.mainBusinessLine ?? null,
    status: values.status ?? null,
    note: values.note ?? null,
  }))
  const submitMachinery = (values: UpdateMachineryPayload) => saveChanges(() => editingMachinery
    ? updateMachinery(editingMachinery.id, { ...values, status: values.status ?? null })
    : createMachinery({
      machineryLibraryId: values.machineryLibraryId,
      companyId,
      year: values.year ?? undefined,
      purchaseYear: values.purchaseYear ?? undefined,
      vinNumber: values.vinNumber ?? undefined,
      registrationNumber: values.registrationNumber ?? undefined,
      countryOfImport: values.countryOfImport ?? undefined,
      status: values.status ?? undefined,
    }))

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

  const machinery = new Map<number, Machinery>()
  company.machinery.forEach((item) => machinery.set(item.id, item))
  company.ownerHistory.forEach((entry) => machinery.set(entry.machinery.id, entry.machinery))
  company.actions.forEach((action) => {
    if (action.machinery) machinery.set(action.machinery.id, action.machinery)
  })

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
        <Button onClick={() => openDialog('editCompany')}>Edit company</Button>
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
            <Typography.Text type="secondary">{new Date(action.createdDate).toLocaleDateString()} · Week {action.actionWeek}{action.assignedUser ? ` · Assigned to ${displayName(action.assignedUser)}` : ''}{action.machinery ? ` · ${action.machinery.manufacturerName} ${action.machinery.model}` : ''}</Typography.Text>
          </Card>)}</div> : <Typography.Text type="secondary">No actions recorded yet.</Typography.Text>}
        </Card>

        <Card title={`Comments · ${company.comments.length}`} extra={<Button size="small" onClick={() => openDialog('comment')}>Add comment</Button>} className="detail-card">
          {company.comments.length ? <div className="comment-card-list">{[...company.comments].sort((a, b) => b.createdAtUtc.localeCompare(a.createdAtUtc)).map((comment: CompanyComment) => <article className="comment-item-card" key={comment.id}>
            <Typography.Paragraph>{comment.text}</Typography.Paragraph>
            <div className="comment-meta"><Typography.Text strong>{displayName(comment.author)}</Typography.Text><time dateTime={comment.createdAtUtc}>{new Date(comment.createdAtUtc).toLocaleString()}</time></div>
          </article>)}</div> : <Typography.Text type="secondary">No comments yet.</Typography.Text>}
        </Card>

        <Card title={`Machinery · ${machinery.size}`} extra={<Button size="small" onClick={() => openDialog('machinery')}>Add machinery</Button>} className="detail-card detail-card-wide">
          {machinery.size ? <div className="machinery-card-list">{[...machinery.values()].map((machinery) => {
            const history = company.ownerHistory
              .filter((entry) => entry.machinery.id === machinery.id)
              .map(({ id }) => ({ id, owner: company.name, city: company.city || '—' }))
            return <Card key={machinery.id} size="small" className="machinery-item-card" title={<span>{machinery.manufacturerName} {machinery.model}</span>} extra={<Button size="small" onClick={() => openEditMachinery(machinery)}>Edit machinery</Button>}>
              <Descriptions size="small" column={{ xs: 1, sm: 2, lg: 3 }}>
                <Descriptions.Item label="Year">{machinery.year ?? '—'}</Descriptions.Item>
                <Descriptions.Item label="VIN">{machinery.vinNumber || '—'}</Descriptions.Item>
                <Descriptions.Item label="Registration">{machinery.registrationNumber || '—'}</Descriptions.Item>
                <Descriptions.Item label="Status">{machinery.status === null ? '—' : machineryStatuses[machinery.status]?.label ?? '—'}</Descriptions.Item>
              </Descriptions>
              <div className="machinery-history">
                <Typography.Text className="machinery-history-title">Owner history</Typography.Text>
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
          })}</div> : <Typography.Text type="secondary">No machinery linked to this company yet.</Typography.Text>}
        </Card>

        <Card title={`Contacts · ${company.contacts.length}`} extra={<Button size="small" onClick={() => openDialog('contact')}>Add contact</Button>} className="detail-card detail-card-wide">
          {company.contacts.length ? <div className="detail-list">{company.contacts.map((contact: Contact) => <div className="detail-list-item" key={contact.id}><Typography.Text strong>{[contact.firstName, contact.lastName].filter(Boolean).join(' ') || 'Unnamed contact'}</Typography.Text><Typography.Text type="secondary">{contact.email || contact.phoneNumber || 'No contact details'}</Typography.Text>{contact.email && contact.phoneNumber && <Typography.Text type="secondary">{contact.phoneNumber}</Typography.Text>}</div>)}</div> : <Typography.Text type="secondary">No contacts have been added.</Typography.Text>}
        </Card>
      </div>

      {dialog === 'editCompany' && <AddUpdateCompanyDialog mode="update" initialValues={{
        name: company.name,
        businessId: company.businessId ?? undefined,
        city: company.city ?? undefined,
        address: company.address ?? undefined,
        mainBusinessLine: company.mainBusinessLine ?? undefined,
        isOurPartner: company.isOurPartner,
        status: company.status,
        note: company.note,
      }} open saving={saving} onCancel={closeDialog} onSubmit={submitCompany} />}
      {(dialog === 'machinery' || dialog === 'editMachinery') && <AddUpdateMachineryDialog mode={dialog === 'editMachinery' ? 'update' : 'add'} initialValues={editingMachinery ?? undefined} open saving={saving} onCancel={closeDialog} onSubmit={submitMachinery} />}
      {dialog === 'comment' && <AddCommentDialog open saving={saving} onCancel={closeDialog} onSubmit={submitComment} />}
      {dialog === 'contact' && <AddContactDialog open saving={saving} onCancel={closeDialog} onSubmit={submitContact} />}
      {dialog === 'action' && <AddActionDialog initialValues={{ status: 0, type: 0, actionWeek: getCurrentWeek() }} open saving={saving} onCancel={closeDialog} onSubmit={submitAction} />}
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
