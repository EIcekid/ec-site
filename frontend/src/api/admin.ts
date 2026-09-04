import http from './http'
import type {
  AdminOrderListItem,
  CategorySales,
  Coupon,
  DashboardStats,
  OrderStatus,
  OrderStatusCount,
  PagedResult,
  ProductDetail,
  ProductListItem,
  RevenuePoint,
} from '../types'

export interface ProductVariantForm {
  color: string | null
  size: string | null
  sku: string
  priceDelta: number
  stock: number
}

export interface ProductForm {
  name: string
  description: string
  price: number
  stock: number
  categoryId: number
  images: string[]
  variants: ProductVariantForm[]
  isActive?: boolean
}

export const adminApi = {
  dashboard() {
    return http.get<DashboardStats>('/admin/dashboard').then((r) => r.data)
  },
  revenueTrend(days = 14) {
    return http.get<RevenuePoint[]>('/admin/dashboard/revenue-trend', { params: { days } }).then((r) => r.data)
  },
  orderStatusDistribution() {
    return http.get<OrderStatusCount[]>('/admin/dashboard/order-status').then((r) => r.data)
  },
  categorySales() {
    return http.get<CategorySales[]>('/admin/dashboard/category-sales').then((r) => r.data)
  },

  products(params: { keyword?: string; page?: number; pageSize?: number }) {
    return http.get<PagedResult<ProductListItem>>('/admin/products', { params }).then((r) => r.data)
  },
  product(id: number) {
    return http.get<ProductDetail>(`/admin/products/${id}`).then((r) => r.data)
  },
  createProduct(payload: ProductForm) {
    return http.post('/admin/products', payload).then((r) => r.data)
  },
  updateProduct(id: number, payload: ProductForm) {
    return http.put(`/admin/products/${id}`, { ...payload, isActive: payload.isActive ?? true })
  },
  deleteProduct(id: number) {
    return http.delete(`/admin/products/${id}`)
  },

  createCategory(name: string, parentId: number | null) {
    return http.post('/admin/categories', { name, parentId }).then((r) => r.data)
  },
  deleteCategory(id: number) {
    return http.delete(`/admin/categories/${id}`)
  },

  uploadImage(file: File) {
    const form = new FormData()
    form.append('file', file)
    return http.post<{ url: string }>('/admin/upload/image', form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    }).then((r) => r.data)
  },

  orders(params: { status?: string; page?: number; pageSize?: number }) {
    return http.get<PagedResult<AdminOrderListItem>>('/admin/orders', { params }).then((r) => r.data)
  },
  order(id: number) {
    return http.get(`/admin/orders/${id}`).then((r) => r.data)
  },
  updateOrderStatus(id: number, status: OrderStatus) {
    return http.put(`/admin/orders/${id}/status`, { status })
  },

  coupons() {
    return http.get<Coupon[]>('/admin/coupons').then((r) => r.data)
  },
  createCoupon(payload: { code: string; type: string; value: number; minOrderAmount: number; expiresAt: string }) {
    return http.post<Coupon>('/admin/coupons', payload).then((r) => r.data)
  },
  deactivateCoupon(id: number) {
    return http.delete(`/admin/coupons/${id}`)
  },
}
