import type { Machinery } from '../../services/companiesApi'

export function machineryLabel(item: Pick<Machinery, 'manufacturerName' | 'model' | 'registrationNumber' | 'vinNumber'>) {
  return [`${item.manufacturerName} ${item.model}`, item.vinNumber, item.registrationNumber].filter(Boolean).join(' · ')
}

export const manufacturers = [
  { value: 1, label: 'Volvo' },
  { value: 2, label: 'Hyundai' },
  { value: 3, label: 'Doosan' },
  { value: 4, label: 'Develon' },
  { value: 5, label: 'Hitachi' },
  { value: 6, label: 'Caterpillar' },
  { value: 7, label: 'Other' },
]

export const machineryStatuses = [
  { value: 0, label: 'Sold' },
  { value: 1, label: 'Commissioned' },
  { value: 2, label: 'Decommissioned' },
  { value: 3, label: 'Candidate' },
  { value: 4, label: 'Bought' },
]

