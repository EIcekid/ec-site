import http from './http'
import type { WishlistItem } from '../types'

export const wishlistApi = {
  list() {
    return http.get<WishlistItem[]>('/wishlist').then((r) => r.data)
  },
  add(productId: number) {
    return http.post('/wishlist', { productId })
  },
  remove(productId: number) {
    return http.delete(`/wishlist/${productId}`)
  },
}
