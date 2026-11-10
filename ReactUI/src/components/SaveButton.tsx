import { SaveOutlined } from '@ant-design/icons'
import { Button } from 'antd'
import type { ButtonProps } from 'antd'

export type SaveButtonProps = Omit<ButtonProps, 'children' | 'icon' | 'iconPlacement'>

export default function SaveButton(props: SaveButtonProps) {
  return <Button type="primary" {...props} icon={<SaveOutlined />}>Save</Button>
}
