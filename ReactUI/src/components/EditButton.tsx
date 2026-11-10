import { EditOutlined } from '@ant-design/icons'
import { Button } from 'antd'
import type { ButtonProps } from 'antd'

export type EditButtonProps = Omit<ButtonProps, 'children' | 'icon' | 'iconPlacement'> & {
  iconOnly?: boolean
}

export default function EditButton({ iconOnly = false, ...props }: EditButtonProps) {
  return <Button {...props}
    icon={<EditOutlined />}>{iconOnly ? null : 'Edit'}</Button>
}
