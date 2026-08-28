<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { adminApi } from '../../api/admin'
import type { DashboardStats } from '../../types'

const stats = ref<DashboardStats | null>(null)

onMounted(async () => {
  stats.value = await adminApi.dashboard()
})
</script>

<template>
  <div>
    <h1 class="title">データ概要</h1>
    <div v-if="stats" class="stat-grid">
      <div class="stat-card">
        <p class="label">公開中の商品数</p>
        <p class="value">{{ stats.totalProducts }}</p>
      </div>
      <div class="stat-card">
        <p class="label">注文総数</p>
        <p class="value">{{ stats.totalOrders }}</p>
      </div>
      <div class="stat-card">
        <p class="label">登録ユーザー数</p>
        <p class="value">{{ stats.totalUsers }}</p>
      </div>
      <div class="stat-card">
        <p class="label">累計売上</p>
        <p class="value">¥{{ stats.totalRevenue.toFixed(2) }}</p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.title {
  margin: 0 0 20px;
  font-size: 20px;
}
.stat-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 16px;
}
.stat-card {
  background: #fff;
  border-radius: 8px;
  padding: 20px;
}
.label {
  margin: 0 0 8px;
  color: #909399;
  font-size: 13px;
}
.value {
  margin: 0;
  font-size: 26px;
  font-weight: 700;
}
</style>
