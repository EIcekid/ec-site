import type { OrderStatus } from '../types'

export const statusLabels: Record<OrderStatus, string> = {
  PendingPayment: '支払い待ち',
  Paid: '発送準備中',
  Shipped: '発送済み',
  Completed: '完了',
  Cancelled: 'キャンセル済み',
}

export const statusTagType: Record<OrderStatus, 'warning' | 'primary' | 'success' | 'info' | 'danger'> = {
  PendingPayment: 'warning',
  Paid: 'primary',
  Shipped: 'primary',
  Completed: 'success',
  Cancelled: 'info',
}
