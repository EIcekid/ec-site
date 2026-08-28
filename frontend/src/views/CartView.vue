<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useCartStore } from '../stores/cart'

const cart = useCartStore()
const router = useRouter()

onMounted(() => cart.fetch())

function checkout() {
  router.push('/checkout')
}
</script>

<template>
  <div class="container cart-page">
    <h1>カート</h1>

    <div v-loading="cart.loading">
      <el-table v-if="cart.items.length > 0" :data="cart.items" class="cart-table">
        <el-table-column label="商品" min-width="300">
          <template #default="{ row }">
            <router-link :to="`/products/${row.productId}`" class="product-cell">
              <img :src="row.imageUrl ?? '/placeholder.png'" />
              <span>{{ row.productName }}</span>
            </router-link>
          </template>
        </el-table-column>
        <el-table-column label="単価" width="120">
          <template #default="{ row }">¥{{ row.price.toFixed(2) }}</template>
        </el-table-column>
        <el-table-column label="数量" width="160">
          <template #default="{ row }">
            <el-input-number
              :model-value="row.quantity"
              :min="1"
              :max="row.stock"
              size="small"
              @change="(v: number) => cart.update(row.id, v)"
            />
          </template>
        </el-table-column>
        <el-table-column label="小計" width="120">
          <template #default="{ row }">¥{{ (row.price * row.quantity).toFixed(2) }}</template>
        </el-table-column>
        <el-table-column label="" width="80">
          <template #default="{ row }">
            <el-button link type="danger" @click="cart.remove(row.id)">削除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <el-empty v-else description="カートは空です">
        <router-link to="/products">
          <el-button type="primary">買い物を続ける</el-button>
        </router-link>
      </el-empty>

      <div v-if="cart.items.length > 0" class="summary">
        <span>商品 {{ cart.totalCount }} 点、合計：</span>
        <span class="total">¥{{ cart.totalAmount.toFixed(2) }}</span>
        <el-button type="primary" size="large" @click="checkout">レジに進む</el-button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.cart-page {
  padding: 24px 20px 60px;
}
.cart-table {
  background: #fff;
  border-radius: 8px;
}
.product-cell {
  display: flex;
  align-items: center;
  gap: 12px;
}
.product-cell img {
  width: 56px;
  height: 56px;
  object-fit: cover;
  border-radius: 4px;
}
.summary {
  margin-top: 24px;
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 12px;
  background: #fff;
  padding: 20px;
  border-radius: 8px;
}
.total {
  color: #f56c6c;
  font-size: 22px;
  font-weight: 700;
}
</style>
