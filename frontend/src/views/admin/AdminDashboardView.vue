<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { adminApi } from '../../api/admin'
import type { CategorySales, DashboardStats, OrderStatus, OrderStatusCount, RevenuePoint } from '../../types'
import { statusLabels } from '../../utils/orderStatus'

const stats = ref<DashboardStats | null>(null)
const revenueTrend = ref<RevenuePoint[]>([])
const orderStatus = ref<OrderStatusCount[]>([])
const categorySales = ref<CategorySales[]>([])
const loading = ref(true)

onMounted(async () => {
  loading.value = true
  try {
    ;[stats.value, revenueTrend.value, orderStatus.value, categorySales.value] = await Promise.all([
      adminApi.dashboard(),
      adminApi.revenueTrend(14),
      adminApi.orderStatusDistribution(),
      adminApi.categorySales(),
    ])
  } finally {
    loading.value = false
  }
})

const revenueChartOption = computed(() => ({
  tooltip: { trigger: 'axis', valueFormatter: (v: number) => `¥${v.toFixed(2)}` },
  grid: { left: 48, right: 16, top: 24, bottom: 32 },
  xAxis: { type: 'category', data: revenueTrend.value.map((p) => p.date) },
  yAxis: { type: 'value' },
  series: [
    {
      type: 'line',
      data: revenueTrend.value.map((p) => p.amount),
      smooth: true,
      areaStyle: {},
      color: '#409eff',
    },
  ],
}))

const orderStatusChartOption = computed(() => ({
  tooltip: { trigger: 'item' },
  legend: { bottom: 0 },
  series: [
    {
      type: 'pie',
      radius: ['40%', '70%'],
      data: orderStatus.value.map((s) => ({ name: statusLabels[s.status as OrderStatus], value: s.count })),
    },
  ],
}))

const categorySalesChartOption = computed(() => ({
  tooltip: { trigger: 'axis', valueFormatter: (v: number) => `¥${v.toFixed(2)}` },
  grid: { left: 100, right: 24, top: 16, bottom: 24 },
  xAxis: { type: 'value' },
  yAxis: { type: 'category', data: categorySales.value.map((c) => c.categoryName) },
  series: [
    {
      type: 'bar',
      data: categorySales.value.map((c) => c.amount),
      color: '#67c23a',
    },
  ],
}))
</script>

<template>
  <div v-loading="loading">
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

    <div class="chart-grid">
      <div class="chart-card wide">
        <h2>売上推移（直近14日間）</h2>
        <v-chart class="chart" :option="revenueChartOption" autoresize />
      </div>
      <div class="chart-card">
        <h2>注文ステータス内訳</h2>
        <v-chart class="chart" :option="orderStatusChartOption" autoresize />
      </div>
      <div class="chart-card">
        <h2>カテゴリー別売上</h2>
        <v-chart class="chart" :option="categorySalesChartOption" autoresize />
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
  margin-bottom: 24px;
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
.chart-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
}
.chart-card {
  background: #fff;
  border-radius: 8px;
  padding: 20px;
}
.chart-card.wide {
  grid-column: 1 / -1;
}
.chart-card h2 {
  margin: 0 0 12px;
  font-size: 15px;
  color: #303133;
}
.chart {
  height: 280px;
}
</style>
