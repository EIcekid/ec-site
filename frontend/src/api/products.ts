import http from './http'
import type { Category, PagedResult, ProductDetail, ProductListItem, Review } from '../types'

export const productsApi = {
  list(params: {
    keyword?: string
    categoryId?: number
    minPrice?: number
    maxPrice?: number
    sort?: string
    page?: number
    pageSize?: number
  }) {
    return http.get<PagedResult<ProductListItem>>('/products', { params }).then((r) => r.data)
  },
  get(id: number) {
    return http.get<ProductDetail>(`/products/${id}`).then((r) => r.data)
  },
  related(id: number) {
    return http.get<ProductListItem[]>(`/products/${id}/related`).then((r) => r.data)
  },
  categories() {
    return http.get<Category[]>('/categories').then((r) => r.data)
  },
  reviews(productId: number) {
    return http.get<Review[]>(`/products/${productId}/reviews`).then((r) => r.data)
  },
  addReview(productId: number, rating: number, content: string) {
    return http.post<Review>(`/products/${productId}/reviews`, { rating, content }).then((r) => r.data)
  },
}
