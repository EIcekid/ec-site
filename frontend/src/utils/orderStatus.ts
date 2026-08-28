import type { OrderStatus } from '../types'

export const statusLabels: Record<OrderStatus, string> = {
  PendingPayment: '待支付',
  Paid: '待发货',
  Shipped: '已发货',
  Completed: '已完成',
  Cancelled: '已取消',
}

export const statusTagType: Record<OrderStatus, 'warning' | 'primary' | 'success' | 'info' | 'danger'> = {
  PendingPayment: 'warning',
  Paid: 'primary',
  Shipped: 'primary',
  Completed: 'success',
  Cancelled: 'info',
}
