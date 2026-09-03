<script setup lang="ts">
import { onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { useWishlistStore } from '../stores/wishlist'
import { wishlistApi } from '../api/wishlist'

const wishlist = useWishlistStore()

onMounted(() => wishlist.fetch())

async function remove(productId: number) {
  await wishlistApi.remove(productId)
  await wishlist.fetch()
  ElMessage.success('お気に入りから削除しました')
}
</script>

<template>
  <div class="container wishlist-page">
    <h1>お気に入り</h1>

    <div v-loading="wishlist.loading" class="grid">
      <div v-for="item in wishlist.items" :key="item.id" class="card">
        <router-link :to="`/products/${item.productId}`" class="image-wrap">
          <img :src="item.imageUrl ?? '/placeholder.png'" :alt="item.productName" />
        </router-link>
        <div class="info">
          <router-link :to="`/products/${item.productId}`" class="name">{{ item.productName }}</router-link>
          <div class="bottom">
            <span class="price">¥{{ item.price.toFixed(2) }}</span>
            <el-button link type="danger" @click="remove(item.productId)">削除</el-button>
          </div>
        </div>
      </div>
    </div>

    <el-empty v-if="!wishlist.loading && wishlist.items.length === 0" description="お気に入りに登録した商品はまだありません">
      <router-link to="/products">
        <el-button type="primary">商品を見る</el-button>
      </router-link>
    </el-empty>
  </div>
</template>

<style scoped>
.wishlist-page {
  padding: 24px 20px 60px;
}
.grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 16px;
  min-height: 100px;
}
.card {
  background: #fff;
  border-radius: 8px;
  overflow: hidden;
}
.image-wrap {
  display: block;
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
  display: block;
  font-size: 14px;
  font-weight: 500;
  margin-bottom: 8px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
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
</style>
