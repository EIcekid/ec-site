import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { wishlistApi } from '../api/wishlist'
import type { WishlistItem } from '../types'
import { useAuthStore } from './auth'

export const useWishlistStore = defineStore('wishlist', () => {
  const items = ref<WishlistItem[]>([])
  const loading = ref(false)

  const productIds = computed(() => new Set(items.value.map((i) => i.productId)))
  const totalCount = computed(() => items.value.length)

  async function fetch() {
    const auth = useAuthStore()
    if (!auth.isLoggedIn) {
      items.value = []
      return
    }
    loading.value = true
    try {
      items.value = await wishlistApi.list()
    } finally {
      loading.value = false
    }
  }

  function isFavorited(productId: number) {
    return productIds.value.has(productId)
  }

  async function toggle(productId: number) {
    if (isFavorited(productId)) {
      await wishlistApi.remove(productId)
    } else {
      await wishlistApi.add(productId)
    }
    await fetch()
  }

  function clear() {
    items.value = []
  }

  return { items, loading, totalCount, isFavorited, toggle, fetch, clear }
})
