import { useCallback, useEffect, useState } from 'react'
import { Alert, Checkbox, Input, Table, Typography, message } from 'antd'
import type { TableColumnsType, TablePaginationConfig } from 'antd'
import type { ChangeEvent } from 'react'
import { getUsers } from '../../services/companiesApi'
import type { UserDirectoryItem, UserDirectoryPage } from '../../services/companiesApi'
import { updateUserActiveStatus } from '../../services/companiesApi'

const PAGE_SIZE = 100

interface Props { isAdmin: boolean }

const columns: TableColumnsType<UserDirectoryItem> = [
  {
    title: 'Name',
    key: 'name',
    width: 280,
    render: (_, user) => [user.firstName, user.lastName].filter(Boolean).join(' ') || 'Unnamed user',
  },
  { title: 'Email', dataIndex: 'email', key: 'email', render: (email: string | null) => email || '—' },
  {
    title: 'Telegram nickname',
    dataIndex: 'telegramNickname',
    key: 'telegramNickname',
    render: (nickname: string | null) => nickname ? `@${nickname.replace(/^@/, '')}` : '—',
  },
  {
    title: 'Active',
    dataIndex: 'isActive',
    key: 'isActive',
    width: 100,
    render: (isActive: boolean, user) => <Checkbox checked={isActive} aria-label={`Active: ${user.email || user.id}`} />,
  },
]

function UsersView({ isAdmin }: Props) {
  const [page, setPage] = useState<UserDirectoryPage>({ items: [], page: 1, pageSize: PAGE_SIZE, totalCount: 0 })
  const [currentPage, setCurrentPage] = useState(1)
  const [search, setSearch] = useState('')
  const [draftSearch, setDraftSearch] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [updatingUserId, setUpdatingUserId] = useState<number | null>(null)
  const [messageApi, contextHolder] = message.useMessage()

  const loadUsers = useCallback(async (pageNumber: number, activeSearch: string, signal: AbortSignal) => {
    try {
      const result = await getUsers(pageNumber, activeSearch, signal)
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
      if (!controller.signal.aborted) void loadUsers(currentPage, search, controller.signal)
    })
    return () => controller.abort()
  }, [currentPage, loadUsers, search])

  const applySearch = () => {
    setLoading(true)
    setCurrentPage(1)
    setSearch(draftSearch.trim())
  }

  const handleTableChange = (pagination: TablePaginationConfig) => {
    setLoading(true)
    setCurrentPage(pagination.current ?? 1)
  }

  const changeUserActiveStatus = async (user: UserDirectoryItem, isActive: boolean) => {
    setUpdatingUserId(user.id)
    try {
      const updatedUser = await updateUserActiveStatus(user.id, isActive)
      setPage((current) => ({
        ...current,
        items: current.items.map((item) => item.id === updatedUser.id ? updatedUser : item),
      }))
      messageApi.success(isActive ? 'User activated' : 'User deactivated')
    } catch (updateError) {
      messageApi.error(updateError instanceof Error ? updateError.message : 'Could not update user access.')
    } finally {
      setUpdatingUserId(null)
    }
  }

  return (
    <section className="companies-view">
      {contextHolder}
      <div className="view-heading">
        <div>
          <Typography.Title level={2}>Users</Typography.Title>
          <Typography.Paragraph type="secondary">View people with access to the application.</Typography.Paragraph>
        </div>
        <div className="company-count">
          <span className="count-value">{page.totalCount.toLocaleString()}</span>
          <span className="count-label">users</span>
        </div>
      </div>

      <div className="filter-card users-filter-card">
        <label className="filter-field">
          <span>Search users</span>
          <Input
            allowClear
            placeholder="Name, email, or Telegram nickname..."
            value={draftSearch}
            onChange={(event: ChangeEvent<HTMLInputElement>) => setDraftSearch(event.target.value)}
            onPressEnter={applySearch}
          />
        </label>
      </div>

      {error && <Alert className="companies-error" type="error" showIcon message="Could not load users" description={error} />}

      <div className="table-card">
        <Table<UserDirectoryItem>
          rowKey="id"
          columns={columns.map((column) => column.key === 'isActive'
            ? {
              ...column,
              render: (isActive: boolean, user: UserDirectoryItem) => <Checkbox
                checked={isActive}
                disabled={!isAdmin || updatingUserId === user.id}
                aria-label={`Set ${user.email || `user ${user.id}`} active`}
                onChange={(event) => void changeUserActiveStatus(user, event.target.checked)}
              />,
            }
            : column)}
          dataSource={page.items}
          loading={loading}
          onChange={handleTableChange}
          pagination={{
            current: currentPage,
            pageSize: PAGE_SIZE,
            total: page.totalCount,
            showSizeChanger: false,
            showTotal: (total: number, range: [number, number]) => `${range[0]}–${range[1]} of ${total} users`,
            position: ['bottomRight'],
          }}
          scroll={{ x: 760 }}
          locale={{ emptyText: loading ? 'Loading users…' : 'No users match this search' }}
        />
      </div>
    </section>
  )
}

export default UsersView