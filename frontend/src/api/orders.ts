import http from './http'
import type { Address, Order } from '../types'

export const addressesApi = {
  list() {
    return http.get<Address[]>('/addresses').then((r) => r.data)
  },
  create(payload: Omit<Address, 'id'>) {
    return http.post<Address>('/addresses', payload).then((r) => r.data)
  },
  update(id: number, payload: Omit<Address, 'id'>) {
    return http.put<Address>(`/addresses/${id}`, payload).then((r) => r.data)
  },
  remove(id: number) {
    return http.delete(`/addresses/${id}`)
  },
}

export const ordersApi = {
  create(addressId: number, couponCode?: string, pointsToUse = 0) {
    return http.post<Order>('/orders', { addressId, couponCode: couponCode || null, pointsToUse }).then((r) => r.data)
  },
  list() {
    return http.get<Order[]>('/orders').then((r) => r.data)
  },
  get(id: number) {
    return http.get<Order>(`/orders/${id}`).then((r) => r.data)
  },
  pay(id: number) {
    return http.post<Order>(`/orders/${id}/pay`).then((r) => r.data)
  },
  cancel(id: number) {
    return http.post<Order>(`/orders/${id}/cancel`).then((r) => r.data)
  },
}
