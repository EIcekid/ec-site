<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { ElMessage } from 'element-plus'
import { adminApi } from '../../api/admin'
import type { Order, OrderStatus } from '../../types'
import { statusLabels, statusTagType } from '../../utils/orderStatus'

const props = defineProps<{ id: string }>()

const order = ref<Order | null>(null)
const loading = ref(true)
const updating = ref(false)

async function load() {
  loading.value = true
  try {
    order.value = (await adminApi.order(Number(props.id))) as Order
  } finally {
    loading.value = false
  }
}

onMounted(load)

const nextStatusMap: Partial<Record<OrderStatus, { status: OrderStatus; label: string }>> = {
  Paid: { status: 'Shipped', label: '标记为已发货' },
  Shipped: { status: 'Completed', label: '标记为已完成' },
}

async function advance() {
  if (!order.value) return
  const next = nextStatusMap[order.value.status]
  if (!next) return
  updating.value = true
  try {
    await adminApi.updateOrderStatus(order.value.id, next.status)
    ElMessage.success('订单状态已更新')
    await load()
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message ?? '更新失败')
  } finally {
    updating.value = false
  }
}
</script>

<template>
  <div v-loading="loading" class="order-detail">
    <template v-if="order">
      <div class="header-row">
        <h1 class="title">订单 #{{ order.id }}</h1>
        <el-tag :type="statusTagType[order.status]" size="large">{{ statusLabels[order.status] }}</el-tag>
      </div>

      <section class="block">
        <h2>收货信息</h2>
        <p>{{ order.address.recipient }} {{ order.address.phone }}</p>
        <p>{{ order.address.province }}{{ order.address.city }}{{ order.address.detail }}</p>
      </section>

      <section class="block">
        <h2>商品清单</h2>
        <ul class="item-list">
          <li v-for="item in order.items" :key="item.productId">
            <img :src="item.productImageUrl ?? '/placeholder.png'" />
            <span class="name">{{ item.productName }}</span>
            <span>¥{{ item.price.toFixed(2) }} x{{ item.quantity }}</span>
          </li>
        </ul>
      </section>

      <section class="block amounts">
        <p class="grand-total"><span>实付</span><span>¥{{ order.totalAmount.toFixed(2) }}</span></p>
      </section>

      <div v-if="nextStatusMap[order.status]" class="actions">
        <el-button type="primary" :loading="updating" @click="advance">
          {{ nextStatusMap[order.status]?.label }}
        </el-button>
      </div>
    </template>
  </div>
</template>

<style scoped>
.title {
  margin: 0;
  font-size: 20px;
}
.header-row {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 16px;
}
.block {
  background: #fff;
  border-radius: 8px;
  padding: 20px;
  margin-bottom: 16px;
}
.block h2 {
  margin: 0 0 12px;
  font-size: 15px;
}
.item-list {
  list-style: none;
  margin: 0;
  padding: 0;
}
.item-list li {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px 0;
  border-bottom: 1px solid #f0f0f0;
}
.item-list img {
  width: 48px;
  height: 48px;
  object-fit: cover;
  border-radius: 4px;
}
.item-list .name {
  flex: 1;
}
.grand-total {
  display: flex;
  justify-content: space-between;
  font-weight: 700;
  font-size: 16px;
  color: #f56c6c;
  margin: 0;
}
.actions {
  display: flex;
  justify-content: flex-end;
}
</style>
