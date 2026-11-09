import { Button, Typography } from 'antd'
import type { TableColumnsType, TableProps } from 'antd'
import type { MachineGridItem } from '../../services/machinesApi'
import DataGrid, { GridCell, GridTag, GridEditButton } from '../../components/grid/DataGrid'

const statuses = [
  { value: 0, label: 'Sold' },
  { value: 1, label: 'Commissioned' },
  { value: 2, label: 'Decommissioned' },
  { value: 3, label: 'Candidate' },
  { value: 4, label: 'Bought' },
]

type Props = Omit<TableProps<MachineGridItem>, 'columns' | 'rowKey'> & { onEdit: (record: MachineGridItem) => void; onSelectCompany: (id: number) => void }

export default function MachinesGrid({ onEdit, onSelectCompany, ...props }: Props) {
  const columns: TableColumnsType<MachineGridItem> = [
    {
      title: 'Machine',
      key: 'machine',
      width: 260,
      render: (_, machine) => <GridCell title={`${machine.manufacturerName} ${machine.model}`}>
        <Typography.Text type="secondary">{machine.year ?? 'Year unknown'} · {machine.vinNumber || 'No VIN'}</Typography.Text>
      </GridCell>,
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
        : <GridTag color={status === 1 ? 'green' : status === 0 ? 'blue' : 'default'}>{statuses.find((item) => item.value === status)?.label ?? 'Unknown'}</GridTag>,
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 90,
      fixed: 'right',
      render: (_, machine) => <GridEditButton label={`Edit ${machine.manufacturerName} ${machine.model}`} onClick={() => onEdit(machine)} />,
    },
  ]

  return <DataGrid<MachineGridItem> {...props} columns={columns} />
}
