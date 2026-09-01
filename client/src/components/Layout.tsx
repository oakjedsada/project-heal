import type { ReactNode } from 'react'

export function Layout({ children }: { children: ReactNode }) {
  return (
    <div className="flex min-h-screen flex-col">
      <div className="border-b border-amber-300 bg-amber-100 px-4 py-2 text-center text-sm text-amber-900">
        โปรเจกต์นี้เป็น<strong>โปรเจกต์สาธิต (demo)</strong>
        เพื่อแสดงทักษะการออกแบบระบบเท่านั้น ไม่ใช่เครื่องมือวินิจฉัยทางการแพทย์
      </div>
      <main className="mx-auto w-full max-w-md flex-1 px-4 py-6">{children}</main>
    </div>
  )
}
