<script setup lang="ts">
import type { ProductListItem } from '../types'

defineProps<{ product: ProductListItem }>()
</script>

<template>
  <router-link :to="`/products/${product.id}`" class="card">
    <div class="image-wrap">
      <img :src="product.imageUrl ?? '/placeholder.png'" :alt="product.name" />
    </div>
    <div class="info">
      <p class="name">{{ product.name }}</p>
      <p class="category">{{ product.categoryName }}</p>
      <div class="bottom">
        <span class="price">¥{{ product.price.toFixed(2) }}</span>
        <span v-if="product.stock === 0" class="sold-out">已售罄</span>
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
