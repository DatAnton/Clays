import { useId, useState } from 'react'
import type { FormEventHandler, ReactNode } from 'react'
import { Button, Tag } from 'antd'
import './GridFilters.css'

interface Props {
  values: { label: string; value: string | number | undefined }[]
  children: ReactNode
  onSubmit?: FormEventHandler<HTMLFormElement>
  actions?: ReactNode
  filterActions?: ReactNode
}

export default function GridFilters({ values, children, onSubmit, actions, filterActions }: Props) {
  const [expanded, setExpanded] = useState(true)
  const contentId = useId()
  const activeValues = values.filter(({ value }) => value !== undefined && String(value).trim() !== '')

  return <form className="filter-card grid-filters" onSubmit={onSubmit ?? (event => event.preventDefault())}>
    <div className="grid-filters-header">
      <Button type="text" htmlType="button" aria-expanded={expanded} aria-controls={contentId}
        onClick={() => setExpanded(value => !value)}
        icon={<svg width="16" height="16" viewBox="0 0 16 16" fill="none" stroke="currentColor"
          strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
          <path d={expanded ? 'm4 10 4-4 4 4' : 'm4 6 4 4 4-4'} />
        </svg>}>
        {expanded ? 'Hide filters' : 'Show filters'}
      </Button>
      {!expanded && <div className="grid-filters-summary" aria-label="Active filters">
        {activeValues.length ? activeValues.map(({ label, value }) =>
          <Tag key={label} className="grid-filter-value" title={`${label}: ${value}`}>{label}: {value}</Tag>)
          : <span className="grid-filters-empty">No active filters</span>}
      </div>}
      {(actions || (expanded && filterActions)) && <div className="grid-filters-header-actions">
        {expanded && filterActions}
        {actions}
      </div>}
    </div>
    <div id={contentId} hidden={!expanded} className="grid-filters-content">{children}</div>
  </form>
}
