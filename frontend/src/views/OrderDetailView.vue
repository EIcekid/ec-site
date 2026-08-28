<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { ordersApi } from '../api/orders'
import type { Order } from '../types'
import { statusLabels, statusTagType } from '../utils/orderStatus'

const props = defineProps<{ id: string }>()

const order = ref<Order | null>(null)
const loading = ref(true)
const acting = ref(false)

async function load() {
  loading.value = true
  try {
    order.value = await ordersApi.get(Number(props.id))
  } finally {
    loading.value = false
  }
}

onMounted(load)

async function pay() {
  acting.value = true
  try {
    order.value = await ordersApi.pay(Number(props.id))
    ElMessage.success('支付成功')
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message ?? '支付失败')
  } finally {
    acting.value = false
  }
}

async function cancel() {
  await ElMessageBox.confirm('确定要取消该订单吗？', '提示', { type: 'warning' })
  acting.value = true
  try {
    order.value = await ordersApi.cancel(Number(props.id))
    ElMessage.success('订单已取消')
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message ?? '取消失败')
  } finally {
    acting.value = false
  }
}
</script>

<template>
  <div v-loading="loading" class="container order-detail-page">
    <template v-if="order">
      <div class="header-row">
        <h1>订单 #{{ order.id }}</h1>
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
        <p><span>商品总额</span><span>¥{{ (order.totalAmount + order.discountAmount).toFixed(2) }}</span></p>
        <p v-if="order.discountAmount > 0"><span>优惠</span><span>-¥{{ order.discountAmount.toFixed(2) }}</span></p>
        <p class="grand-total"><span>实付</span><span>¥{{ order.totalAmount.toFixed(2) }}</span></p>
      </section>

      <div class="actions">
        <el-button v-if="order.status === 'PendingPayment'" :loading="acting" @click="cancel">取消订单</el-button>
        <el-button v-if="order.status === 'PendingPayment'" type="primary" :loading="acting" @click="pay">
          立即支付
        </el-button>
      </div>
    </template>
  </div>
</template>

<style scoped>
.order-detail-page {
  padding: 24px 20px 60px;
}
.header-row {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 16px;
}
.header-row h1 {
  margin: 0;
  font-size: 20px;
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
.amounts p {
  display: flex;
  justify-content: space-between;
  margin: 4px 0;
  color: #606266;
}
.grand-total {
  font-weight: 700;
  font-size: 16px;
  color: #f56c6c !important;
}
.actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}
</style>
