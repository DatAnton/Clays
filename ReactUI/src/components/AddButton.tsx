import { Button } from 'antd'
import type { ButtonProps } from 'antd'

export type AddButtonProps = Omit<ButtonProps, 'children' | 'icon' | 'iconPlacement'>

export default function AddButton(props: AddButtonProps) {
  return <Button type="primary" {...props}
    icon={<svg width="16" height="16" viewBox="0 0 16 16" fill="none" stroke="currentColor"
      strokeWidth="1.5" strokeLinecap="round" aria-hidden="true">
      <path d="M8 3v10M3 8h10" />
    </svg>}>Add</Button>
}
