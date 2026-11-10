import { Button } from 'antd'
import type { ButtonProps } from 'antd'

export type EditButtonProps = Omit<ButtonProps, 'children' | 'icon' | 'iconPlacement'> & {
  iconOnly?: boolean
}

export default function EditButton({ iconOnly = false, ...props }: EditButtonProps) {
  return <Button {...props}
    icon={<svg width="16" height="16" viewBox="0 0 16 16" fill="none" stroke="currentColor"
      strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
      <path d="m10.5 2.5 3 3M3 10l8-8a1.06 1.06 0 0 1 1.5 0L14 3.5a1.06 1.06 0 0 1 0 1.5l-8 8-4 1 1-4Z" />
    </svg>}>{iconOnly ? null : 'Edit'}</Button>
}
