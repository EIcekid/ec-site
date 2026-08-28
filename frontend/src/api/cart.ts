import http from './http'
import type { CartItem } from '../types'

export const cartApi = {
  list() {
    return http.get<CartItem[]>('/cart').then((r) => r.data)
  },
  add(productId: number, quantity: number) {
    return http.post<CartItem>('/cart', { productId, quantity }).then((r) => r.data)
  },
  update(id: number, quantity: number) {
    return http.put(`/cart/${id}`, { quantity })
  },
  remove(id: number) {
    return http.delete(`/cart/${id}`)
  },
}
