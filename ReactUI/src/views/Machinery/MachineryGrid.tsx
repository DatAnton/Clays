import { Button, Typography } from 'antd'
import type { TableColumnsType, TableProps } from 'antd'
import type { MachineryGridItem } from '../../services/machineryApi'
import DataGrid, { GridCell, GridTag, GridEditButton } from '../../components/grid/DataGrid'

const statuses = [
  { value: 0, label: 'Sold' },
  { value: 1, label: 'Commissioned' },
  { value: 2, label: 'Decommissioned' },
  { value: 3, label: 'Candidate' },
  { value: 4, label: 'Bought' },
]

type Props = Omit<TableProps<MachineryGridItem>, 'columns' | 'rowKey'> & { onEdit: (record: MachineryGridItem) => void; onSelectCompany: (id: number) => void }

export default function MachineryGrid({ onEdit, onSelectCompany, ...props }: Props) {
  const columns: TableColumnsType<MachineryGridItem> = [
    {
      title: 'Machinery',
      key: 'machinery',
      width: 260,
      render: (_, machinery) => <GridCell title={`${machinery.manufacturerName} ${machinery.model}`}>
        <Typography.Text type="secondary">{machinery.year ?? 'Year unknown'} · {machinery.vinNumber || 'No VIN'}</Typography.Text>
      </GridCell>,
    },
    {
      title: 'Company',
      key: 'company',
      width: 220,
      render: (_, machinery) => machinery.companyId && machinery.companyName
        ? <Button type="link" className="machinery-company-link" onClick={() => onSelectCompany(machinery.companyId!)}>{machinery.companyName}</Button>
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
      render: (_, machinery) => <GridEditButton label={`Edit ${machinery.manufacturerName} ${machinery.model}`} onClick={() => onEdit(machinery)} />,
    },
  ]

  return <DataGrid<MachineryGridItem> {...props} columns={columns} />
}
