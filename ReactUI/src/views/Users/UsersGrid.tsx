import { Space, Typography } from 'antd'
import type { TableColumnsType, TableProps } from 'antd'
import type { ManagedUser } from '../../services/usersApi'
import DataGrid, { GridCell, GridTag, GridEditButton } from '../../components/grid/DataGrid'

type Props = Omit<TableProps<ManagedUser>, 'columns' | 'rowKey'> & { onEdit: (record: ManagedUser) => void }

export default function UsersGrid({ onEdit, ...props }: Props) {
  const columns: TableColumnsType<ManagedUser> = [
    { title: 'User', key: 'name', width: 240, render: (_, user) => <GridCell title={[user.firstName, user.lastName].filter(Boolean).join(' ') || 'Unnamed user'}>
      {user.userName && <Typography.Text type="secondary">{user.userName}</Typography.Text>}
      <Typography.Text type="secondary">{user.email}</Typography.Text>
    </GridCell> },
    { title: 'Roles', key: 'roles', width: 320, render: (_, user) => <Space wrap size={[8, 8]}>
      {user.roles.length ? user.roles.map(role => <GridTag color={role.name === 'Admin' ? 'blue' : 'default'} key={role.id}>{role.name}</GridTag>)
        : <Typography.Text type="secondary">No roles</Typography.Text>}
    </Space> },
    { title: 'Actions', key: 'actions', width: 110, fixed: 'right', render: (_, user) =>
      <GridEditButton label={`Edit ${user.email}`} onClick={() => onEdit(user)} />
    },
  ]

  return <DataGrid<ManagedUser> {...props} columns={columns} />
}
