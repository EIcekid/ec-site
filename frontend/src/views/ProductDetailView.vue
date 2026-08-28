<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { productsApi } from '../api/products'
import { useCartStore } from '../stores/cart'
import { useAuthStore } from '../stores/auth'
import type { ProductDetail, Review } from '../types'

const props = defineProps<{ id: string }>()
const router = useRouter()
const cart = useCartStore()
const auth = useAuthStore()

const product = ref<ProductDetail | null>(null)
const reviews = ref<Review[]>([])
const activeImage = ref('')
const quantity = ref(1)
const loading = ref(true)

const newRating = ref(5)
const newContent = ref('')
const submitting = ref(false)

const productId = computed(() => Number(props.id))

async function loadAll() {
  loading.value = true
  try {
    const [p, r] = await Promise.all([productsApi.get(productId.value), productsApi.reviews(productId.value)])
    product.value = p
    activeImage.value = p.images[0] ?? ''
    reviews.value = r
  } finally {
    loading.value = false
  }
}

onMounted(loadAll)

async function addToCart() {
  if (!auth.isLoggedIn) {
    router.push({ name: 'login', query: { redirect: router.currentRoute.value.fullPath } })
    return
  }
  await cart.add(productId.value, quantity.value)
  ElMessage.success('カートに追加しました')
}

async function buyNow() {
  await addToCart()
  router.push('/cart')
}

async function submitReview() {
  if (!newContent.value.trim()) {
    ElMessage.warning('レビュー内容を入力してください')
    return
  }
  submitting.value = true
  try {
    await productsApi.addReview(productId.value, newRating.value, newContent.value)
    newContent.value = ''
    const [p, r] = await Promise.all([productsApi.get(productId.value), productsApi.reviews(productId.value)])
    product.value = p
    reviews.value = r
    ElMessage.success('レビューを投稿しました')
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message ?? 'レビューの投稿に失敗しました')
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div v-loading="loading" class="container detail-page">
    <template v-if="product">
      <div class="top">
        <div class="gallery">
          <div class="main-image">
            <img :src="activeImage" :alt="product.name" />
          </div>
          <div class="thumbs">
            <img
              v-for="img in product.images"
              :key="img"
              :src="img"
              :class="{ active: img === activeImage }"
              @click="activeImage = img"
            />
          </div>
        </div>

        <div class="info">
          <h1>{{ product.name }}</h1>
          <div class="rating-row">
            <el-rate :model-value="product.averageRating" disabled allow-half />
            <span class="review-count">レビュー {{ product.reviewCount }} 件</span>
          </div>
          <p class="price">¥{{ product.price.toFixed(2) }}</p>
          <p class="stock">在庫 {{ product.stock }} 点</p>
          <p class="desc">{{ product.description }}</p>

          <div class="qty-row">
            <span>数量</span>
            <el-input-number v-model="quantity" :min="1" :max="product.stock" :disabled="product.stock === 0" />
          </div>

          <div class="actions">
            <el-button size="large" :disabled="product.stock === 0" @click="addToCart">カートに入れる</el-button>
            <el-button size="large" type="primary" :disabled="product.stock === 0" @click="buyNow">今すぐ購入</el-button>
          </div>
        </div>
      </div>

      <div class="reviews">
        <h2>商品レビュー ({{ reviews.length }})</h2>

        <div v-if="auth.isLoggedIn" class="review-form">
          <el-rate v-model="newRating" />
          <el-input v-model="newContent" type="textarea" :rows="2" placeholder="ご購入の感想をお聞かせください（購入済みの方のみ投稿できます）" />
          <el-button type="primary" :loading="submitting" @click="submitReview">レビューを投稿</el-button>
        </div>

        <ul class="review-list">
          <li v-for="r in reviews" :key="r.id">
            <div class="review-header">
              <strong>{{ r.userName }}</strong>
              <el-rate :model-value="r.rating" disabled size="small" />
              <span class="review-date">{{ new Date(r.createdAt).toLocaleDateString() }}</span>
            </div>
            <p>{{ r.content }}</p>
          </li>
        </ul>
        <el-empty v-if="reviews.length === 0" description="まだレビューがありません" />
      </div>
    </template>
  </div>
</template>

<style scoped>
.detail-page {
  padding: 24px 20px 60px;
}
.top {
  display: flex;
  gap: 40px;
  background: #fff;
  border-radius: 8px;
  padding: 24px;
}
.gallery {
  width: 400px;
  flex-shrink: 0;
}
.main-image {
  width: 100%;
  aspect-ratio: 1 / 1;
  background: #f5f5f5;
  border-radius: 8px;
  overflow: hidden;
}
.main-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}
.thumbs {
  display: flex;
  gap: 8px;
  margin-top: 8px;
}
.thumbs img {
  width: 64px;
  height: 64px;
  object-fit: cover;
  border-radius: 4px;
  cursor: pointer;
  border: 2px solid transparent;
}
.thumbs img.active {
  border-color: #409eff;
}
.info {
  flex: 1;
}
.info h1 {
  margin: 0 0 12px;
  font-size: 22px;
}
.rating-row {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
}
.review-count {
  color: #909399;
  font-size: 13px;
}
.price {
  font-size: 28px;
  color: #f56c6c;
  font-weight: 700;
  margin: 0 0 8px;
}
.stock {
  color: #909399;
  margin: 0 0 16px;
}
.desc {
  color: #606266;
  line-height: 1.6;
  margin-bottom: 24px;
}
.qty-row {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 24px;
}
.actions {
  display: flex;
  gap: 12px;
}
.reviews {
  margin-top: 32px;
  background: #fff;
  border-radius: 8px;
  padding: 24px;
}
.reviews h2 {
  margin: 0 0 16px;
  font-size: 18px;
}
.review-form {
  display: flex;
  flex-direction: column;
  gap: 12px;
  max-width: 500px;
  margin-bottom: 24px;
  padding-bottom: 24px;
  border-bottom: 1px solid #ebeef5;
}
.review-form .el-button {
  align-self: flex-start;
}
.review-list {
  list-style: none;
  margin: 0;
  padding: 0;
}
.review-list li {
  padding: 12px 0;
  border-bottom: 1px solid #f0f0f0;
}
.review-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 6px;
}
.review-date {
  color: #c0c4cc;
  font-size: 12px;
}
</style>
