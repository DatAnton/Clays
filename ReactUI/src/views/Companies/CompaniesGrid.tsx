import { Typography } from 'antd'
import type { TableColumnsType, TableProps } from 'antd'
import type { Company } from '../../services/companiesApi'
import DataGrid, { GridCell, GridTag, GridEditButton } from '../../components/grid/DataGrid'

type Props = Omit<TableProps<Company>, 'columns' | 'rowKey'> & { onEdit: (record: Company) => void }

export default function CompaniesGrid({ onEdit, ...props }: Props) {
  const columns: TableColumnsType<Company> = [
    {
      title: 'Company',
      dataIndex: 'name',
      key: 'name',
      width: 260,
      render: (name: string, company) => (
        <GridCell title={name}>
          <Typography.Text type="secondary">{company.businessId || 'Business ID unavailable'}</Typography.Text>
        </GridCell>
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
        ? <GridTag color="blue">Partner</GridTag>
        : <Typography.Text type="secondary">—</Typography.Text>,
    },
    { title: 'Actions', key: 'actions', width: 110, fixed: 'right',
      render: (_, company) => <GridEditButton label={`Edit ${company.name}`} onClick={() => onEdit(company)} /> },
  ]

  return <DataGrid<Company> {...props} columns={columns} />
}
