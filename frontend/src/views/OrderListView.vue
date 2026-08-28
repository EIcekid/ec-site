<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { ordersApi } from '../api/orders'
import type { Order } from '../types'
import { statusLabels, statusTagType } from '../utils/orderStatus'

const orders = ref<Order[]>([])
const loading = ref(true)

onMounted(async () => {
  try {
    orders.value = await ordersApi.list()
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="container orders-page">
    <h1>我的订单</h1>

    <div v-loading="loading">
      <el-empty v-if="!loading && orders.length === 0" description="暂无订单" />

      <router-link v-for="order in orders" :key="order.id" :to="`/orders/${order.id}`" class="order-card">
        <div class="order-header">
          <span>订单号 #{{ order.id }}</span>
          <span>{{ new Date(order.createdAt).toLocaleString() }}</span>
          <el-tag :type="statusTagType[order.status]">{{ statusLabels[order.status] }}</el-tag>
        </div>
        <div class="order-items">
          <img v-for="item in order.items" :key="item.productId" :src="item.productImageUrl ?? '/placeholder.png'" />
        </div>
        <div class="order-footer">
          <span>共 {{ order.items.reduce((s, i) => s + i.quantity, 0) }} 件</span>
          <span class="total">合计 ¥{{ order.totalAmount.toFixed(2) }}</span>
        </div>
      </router-link>
    </div>
  </div>
</template>

<style scoped>
.orders-page {
  padding: 24px 20px 60px;
}
.order-card {
  display: block;
  background: #fff;
  border-radius: 8px;
  padding: 16px 20px;
  margin-bottom: 12px;
}
.order-header {
  display: flex;
  gap: 16px;
  align-items: center;
  font-size: 13px;
  color: #909399;
  padding-bottom: 12px;
  border-bottom: 1px solid #f0f0f0;
  margin-bottom: 12px;
}
.order-items {
  display: flex;
  gap: 8px;
}
.order-items img {
  width: 56px;
  height: 56px;
  object-fit: cover;
  border-radius: 4px;
}
.order-footer {
  display: flex;
  justify-content: flex-end;
  gap: 16px;
  margin-top: 12px;
  font-size: 14px;
}
.total {
  color: #f56c6c;
  font-weight: 700;
}
</style>
