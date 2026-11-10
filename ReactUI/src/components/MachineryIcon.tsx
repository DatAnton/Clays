import Icon from '@ant-design/icons'

function CrawlerSvg() {
  return <svg viewBox="0 0 24 24" width="1em" height="1em" fill="none"
    stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round"
    aria-hidden="true" focusable="false">
    <rect x="2" y="16" width="16" height="6" rx="3" />
    <path d="M6 19h8M4 16v-5h5V6h5l2 10M9 11h6M14 6l4-3 3 10M19 13h3v4h-4" />
  </svg>
}

export default function MachineryIcon() {
  return <Icon component={CrawlerSvg} />
}
