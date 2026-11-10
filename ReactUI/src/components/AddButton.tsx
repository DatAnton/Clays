import { PlusOutlined } from '@ant-design/icons'
import { Button } from 'antd'
import type { ButtonProps } from 'antd'

export type AddButtonProps = Omit<ButtonProps, 'children' | 'icon' | 'iconPlacement'>

export default function AddButton(props: AddButtonProps) {
  return <Button type="primary" {...props}
    icon={<PlusOutlined />}>Add</Button>
}
