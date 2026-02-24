"use client"

interface ControlSidebarProps {
  children: React.ReactNode
  width?: string
}

export function ControlSidebar({ children, width = "w-[420px]" }: ControlSidebarProps) {
  return (
    <aside className={`${width} shrink-0 border-r border-border bg-background flex flex-col overflow-hidden`}>
      <div className="flex-1 overflow-y-auto px-5 py-5">
        <p className="text-[11px] font-semibold uppercase tracking-wider text-muted-foreground mb-4">
          Settings
        </p>
        {children}
      </div>
    </aside>
  )
}
