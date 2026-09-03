import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', name: 'home', component: () => import('../views/HomeView.vue') },
    { path: '/products', name: 'products', component: () => import('../views/ProductListView.vue') },
    { path: '/products/:id', name: 'product-detail', component: () => import('../views/ProductDetailView.vue'), props: true },
    { path: '/cart', name: 'cart', component: () => import('../views/CartView.vue'), meta: { requiresAuth: true } },
    { path: '/checkout', name: 'checkout', component: () => import('../views/CheckoutView.vue'), meta: { requiresAuth: true } },
    { path: '/login', name: 'login', component: () => import('../views/LoginView.vue') },
    { path: '/register', name: 'register', component: () => import('../views/RegisterView.vue') },
    { path: '/orders', name: 'orders', component: () => import('../views/OrderListView.vue'), meta: { requiresAuth: true } },
    { path: '/orders/:id', name: 'order-detail', component: () => import('../views/OrderDetailView.vue'), props: true, meta: { requiresAuth: true } },
    { path: '/addresses', name: 'addresses', component: () => import('../views/AddressesView.vue'), meta: { requiresAuth: true } },
    { path: '/wishlist', name: 'wishlist', component: () => import('../views/WishlistView.vue'), meta: { requiresAuth: true } },

    {
      path: '/admin',
      component: () => import('../views/admin/AdminLayout.vue'),
      meta: { requiresAdmin: true },
      children: [
        { path: '', name: 'admin-dashboard', component: () => import('../views/admin/AdminDashboardView.vue') },
        { path: 'products', name: 'admin-products', component: () => import('../views/admin/AdminProductListView.vue') },
        { path: 'products/new', name: 'admin-product-new', component: () => import('../views/admin/AdminProductEditView.vue') },
        { path: 'products/:id/edit', name: 'admin-product-edit', component: () => import('../views/admin/AdminProductEditView.vue'), props: true },
        { path: 'orders', name: 'admin-orders', component: () => import('../views/admin/AdminOrderListView.vue') },
        { path: 'orders/:id', name: 'admin-order-detail', component: () => import('../views/admin/AdminOrderDetailView.vue'), props: true },
        { path: 'coupons', name: 'admin-coupons', component: () => import('../views/admin/AdminCouponListView.vue') },
      ],
    },

    { path: '/:pathMatch(.*)*', name: 'not-found', component: () => import('../views/NotFoundView.vue') },
  ],
  scrollBehavior() {
    return { top: 0 }
  },
})

router.beforeEach((to) => {
  const auth = useAuthStore()

  if (to.meta.requiresAdmin && (!auth.isLoggedIn || !auth.isAdmin)) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }
  if (to.meta.requiresAuth && !auth.isLoggedIn) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }
  return true
})

export default router
