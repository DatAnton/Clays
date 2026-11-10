import { Select } from 'antd'
import type { Machinery } from '../../../services/companiesApi'
import { machineryLabel, machineryStatuses } from '../../Machinery/machineryOptions'

interface Props {
  value?: number | null
  onChange?: (value: number | null) => void
  machinery: Machinery[]
  disabled?: boolean
  id?: string
}

export default function MachinerySelect({ value, onChange, machinery, disabled, id }: Props) {
  return <Select id={id} aria-label="Machinery" value={value ?? undefined} disabled={disabled}
    allowClear showSearch={{ optionFilterProp: 'searchLabel' }} placeholder="Select this company's machinery"
    style={{ width: '100%' }} notFoundContent="No matching machinery for this company"
    labelRender={({ label }) => label ?? 'Machinery unavailable'}
    onChange={(selected: number | undefined) => onChange?.(selected ?? null)}
    options={machinery.map(item => {
      const name = machineryLabel(item)
      const status = machineryStatuses.find(status => status.value === item.status)?.label
      return {
        value: item.id,
        searchLabel: [name, status].filter(Boolean).join(' · '),
        label: <>{name}{status && <> · <em>{status}</em></>}</>,
      }
    })} />
}
