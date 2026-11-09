import { Button, Table, Tag, Typography } from 'antd'
import type { TableProps, TagProps } from 'antd'
import type { ReactNode } from 'react'
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
  return <Button className="grid-edit-button" aria-label={label} title={label}
    onClick={event => { event.stopPropagation(); onClick() }}
    icon={<svg width="16" height="16" viewBox="0 0 16 16" fill="none" stroke="currentColor"
      strokeWidth="1.5" strokeLinecap="round" aria-hidden="true">
      <path d="m10 3 3 3M3 13l3-.7 7.5-7.5a2.1 2.1 0 0 0-3-3L3 9.3 2.3 13.7Z" />
    </svg>} />
}
