import type { ReactNode } from 'react'

type NoticePanelProps = {
  action?: ReactNode
  children: ReactNode
  title?: string
  tone?: 'default' | 'error' | 'success'
}

const toneClasses = {
  default: 'border-[#24322a]',
  error: 'border-[#493229]',
  success: 'border-[#284533]',
}

export function NoticePanel({ action, children, title, tone = 'default' }: NoticePanelProps) {
  return (
    <div
      className={`mb-6 flex flex-col items-start justify-between gap-5 rounded-2xl border bg-[#111a16] p-6 sm:flex-row sm:items-center ${toneClasses[tone]}`}
      role={tone === 'error' ? 'alert' : tone === 'success' ? 'status' : undefined}
    >
      <div>
        {title && <h3 className="m-0 mb-2 text-xl font-bold text-[#f3f6f4]">{title}</h3>}
        <div className="max-w-2xl leading-6 text-[#a5afa8]">{children}</div>
      </div>
      {action && <div className="shrink-0">{action}</div>}
    </div>
  )
}
