<script setup lang="ts">
import { useRouter } from 'vue-router'
import type { ProductListItem } from '../types'
import { useAuthStore } from '../stores/auth'
import { useWishlistStore } from '../stores/wishlist'

const props = defineProps<{ product: ProductListItem }>()
const router = useRouter()
const auth = useAuthStore()
const wishlist = useWishlistStore()

async function toggleFavorite(e: MouseEvent) {
  e.preventDefault()
  e.stopPropagation()
  if (!auth.isLoggedIn) {
    router.push('/login')
    return
  }
  await wishlist.toggle(props.product.id)
}
</script>

<template>
  <router-link :to="`/products/${product.id}`" class="card">
    <div class="image-wrap">
      <img :src="product.imageUrl ?? '/placeholder.png'" :alt="product.name" />
      <button class="favorite-btn" :class="{ active: wishlist.isFavorited(product.id) }" @click="toggleFavorite">
        <el-icon><Star /></el-icon>
      </button>
    </div>
    <div class="info">
      <p class="name">{{ product.name }}</p>
      <p class="category">{{ product.categoryName }}</p>
      <div class="bottom">
        <span class="price">¥{{ product.price.toFixed(2) }}</span>
        <span v-if="product.stock === 0" class="sold-out">売り切れ</span>
      </div>
    </div>
  </router-link>
</template>

<style scoped>
.card {
  display: block;
  background: #fff;
  border-radius: 8px;
  overflow: hidden;
  transition: box-shadow 0.2s;
}
.card:hover {
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
}
.image-wrap {
  position: relative;
  width: 100%;
  aspect-ratio: 1 / 1;
  overflow: hidden;
  background: #f5f5f5;
}
.image-wrap img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}
.favorite-btn {
  position: absolute;
  top: 8px;
  right: 8px;
  width: 28px;
  height: 28px;
  border-radius: 50%;
  border: none;
  background: rgba(255, 255, 255, 0.9);
  color: #c0c4cc;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  font-size: 15px;
}
.favorite-btn.active {
  color: #f56c6c;
}
.info {
  padding: 12px;
}
.name {
  margin: 0 0 4px;
  font-size: 14px;
  font-weight: 500;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.category {
  margin: 0 0 8px;
  font-size: 12px;
  color: #909399;
}
.bottom {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.price {
  color: #f56c6c;
  font-weight: 700;
  font-size: 16px;
}
.sold-out {
  font-size: 12px;
  color: #c0c4cc;
}
</style>
