import { Table, Tag, Typography } from 'antd'
import type { TableProps, TagProps } from 'antd'
import type { ReactNode } from 'react'
import EditButton from '../EditButton'
import './DataGrid.css'

export default function DataGrid<T extends { id: number }>(props: TableProps<T>) {
  return <div className="table-card data-grid">
    <Table<T> rowKey="id" size="middle" {...props} />
  </div>
}

export function GridCell({ title, children }: { title: ReactNode; children?: ReactNode }) {
  return <div className="grid-cell">
    <Typography.Text strong>{title}</Typography.Text>
    {children && <div className="grid-cell-details">{children}</div>}
  </div>
}

export function GridTag(props: TagProps) {
  return <Tag {...props} className="grid-tag" />
}

export function GridEditButton({ label, onClick }: { label: string; onClick: () => void }) {
  return <EditButton iconOnly size="large" className="grid-action-button" aria-label={label} title={label}
    onClick={event => { event.stopPropagation(); onClick() }} />
}
