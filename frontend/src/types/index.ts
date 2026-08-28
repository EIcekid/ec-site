export interface AuthResponse {
  token: string
  email: string
  name: string
  role: 'Customer' | 'Admin'
}

export interface Category {
  id: number
  name: string
  parentId: number | null
  children: Category[]
}

export interface ProductListItem {
  id: number
  name: string
  price: number
  imageUrl: string | null
  stock: number
  categoryName: string
}

export interface ProductDetail {
  id: number
  name: string
  description: string
  price: number
  stock: number
  categoryId: number
  categoryName: string
  images: string[]
  averageRating: number
  reviewCount: number
}

export interface Review {
  id: number
  userName: string
  rating: number
  content: string
  createdAt: string
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export interface CartItem {
  id: number
  productId: number
  productName: string
  imageUrl: string | null
  price: number
  quantity: number
  stock: number
}

export interface Address {
  id: number
  recipient: string
  phone: string
  province: string
  city: string
  detail: string
  isDefault: boolean
}

export interface OrderItem {
  productId: number
  productName: string
  productImageUrl: string | null
  price: number
  quantity: number
}

export type OrderStatus = 'PendingPayment' | 'Paid' | 'Shipped' | 'Completed' | 'Cancelled'

export interface Order {
  id: number
  status: OrderStatus
  totalAmount: number
  discountAmount: number
  createdAt: string
  paidAt: string | null
  shippedAt: string | null
  completedAt: string | null
  address: Address
  items: OrderItem[]
}

export interface AdminOrderListItem {
  id: number
  customerName: string
  status: OrderStatus
  totalAmount: number
  createdAt: string
}

export interface Coupon {
  id: number
  code: string
  type: 'FixedAmount' | 'Percentage'
  value: number
  minOrderAmount: number
  expiresAt: string
  isActive: boolean
}

export interface DashboardStats {
  totalProducts: number
  totalOrders: number
  totalUsers: number
  totalRevenue: number
}
