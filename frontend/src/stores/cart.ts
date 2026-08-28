import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { cartApi } from '../api/cart'
import type { CartItem } from '../types'
import { useAuthStore } from './auth'

export const useCartStore = defineStore('cart', () => {
  const items = ref<CartItem[]>([])
  const loading = ref(false)

  const totalCount = computed(() => items.value.reduce((sum, i) => sum + i.quantity, 0))
  const totalAmount = computed(() => items.value.reduce((sum, i) => sum + i.price * i.quantity, 0))

  async function fetch() {
    const auth = useAuthStore()
    if (!auth.isLoggedIn) {
      items.value = []
      return
    }
    loading.value = true
    try {
      items.value = await cartApi.list()
    } finally {
      loading.value = false
    }
  }

  async function add(productId: number, quantity: number) {
    await cartApi.add(productId, quantity)
    await fetch()
  }

  async function update(id: number, quantity: number) {
    await cartApi.update(id, quantity)
    await fetch()
  }

  async function remove(id: number) {
    await cartApi.remove(id)
    await fetch()
  }

  function clear() {
    items.value = []
  }

  return { items, loading, totalCount, totalAmount, fetch, add, update, remove, clear }
})
