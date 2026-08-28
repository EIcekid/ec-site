<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { productsApi } from '../api/products'
import type { ProductListItem } from '../types'
import ProductCard from '../components/ProductCard.vue'

const products = ref<ProductListItem[]>([])
const loading = ref(true)

onMounted(async () => {
  try {
    const result = await productsApi.list({ page: 1, pageSize: 8 })
    products.value = result.items
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="home">
    <div class="banner container">
      <h1>EC Site へようこそ</h1>
      <p>厳選アイテムをまとめてお届け</p>
      <router-link to="/products">
        <el-button type="primary" size="large">今すぐ購入する</el-button>
      </router-link>
    </div>

    <div class="container">
      <h2 class="section-title">人気商品</h2>
      <div v-loading="loading" class="grid">
        <ProductCard v-for="p in products" :key="p.id" :product="p" />
      </div>
    </div>
  </div>
</template>

<style scoped>
.banner {
  padding: 60px 20px;
  text-align: center;
  background: linear-gradient(135deg, #409eff, #79bbff);
  color: #fff;
  border-radius: 0 0 16px 16px;
}
.banner h1 {
  margin: 0 0 8px;
  font-size: 32px;
}
.banner p {
  margin: 0 0 24px;
  opacity: 0.9;
}
.section-title {
  margin: 32px 0 16px;
  font-size: 20px;
}
.grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 16px;
  min-height: 200px;
  padding-bottom: 40px;
}
</style>
