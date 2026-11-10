import { LogoutOutlined } from '@ant-design/icons'
import { Button, Modal } from 'antd'

interface Props {
  className?: string
  onConfirm: () => Promise<void>
}

export default function SignOutButton({ className, onConfirm }: Props) {
  const [modal, contextHolder] = Modal.useModal()

  return <>
    {contextHolder}
    <Button className={className} type="text" aria-label="Sign out" title="Sign out"
      icon={<LogoutOutlined />} onClick={() => modal.confirm({
        title: 'Sign out?',
        content: 'Are you sure you want to sign out?',
        okText: 'Sign out',
        cancelText: 'Cancel',
        autoFocusButton: 'cancel',
        onOk: onConfirm,
      })} />
  </>
}
