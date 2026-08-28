<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { adminApi } from '../../api/admin'
import type { AdminOrderListItem, OrderStatus } from '../../types'
import { statusLabels, statusTagType } from '../../utils/orderStatus'

const orders = ref<AdminOrderListItem[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = 20
const status = ref<OrderStatus | ''>('')
const loading = ref(true)

const statusOptions: { label: string; value: OrderStatus | '' }[] = [
  { label: 'すべて', value: '' },
  { label: '支払い待ち', value: 'PendingPayment' },
  { label: '発送準備中', value: 'Paid' },
  { label: '発送済み', value: 'Shipped' },
  { label: '完了', value: 'Completed' },
  { label: 'キャンセル済み', value: 'Cancelled' },
]

async function load() {
  loading.value = true
  try {
    const result = await adminApi.orders({ status: status.value || undefined, page: page.value, pageSize })
    orders.value = result.items
    total.value = result.totalCount
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<template>
  <div>
    <h1 class="title">注文管理</h1>

    <div class="toolbar">
      <el-select v-model="status" style="width: 140px" @change="load">
        <el-option v-for="o in statusOptions" :key="o.value" :label="o.label" :value="o.value" />
      </el-select>
    </div>

    <el-table v-loading="loading" :data="orders" class="table">
      <el-table-column prop="id" label="注文番号" width="100" />
      <el-table-column prop="customerName" label="顧客" width="140" />
      <el-table-column label="金額" width="120">
        <template #default="{ row }">¥{{ row.totalAmount.toFixed(2) }}</template>
      </el-table-column>
      <el-table-column label="状態" width="120">
        <template #default="{ row }">
          <el-tag :type="statusTagType[row.status as OrderStatus]">{{ statusLabels[row.status as OrderStatus] }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="注文日時" width="180">
        <template #default="{ row }">{{ new Date(row.createdAt).toLocaleString() }}</template>
      </el-table-column>
      <el-table-column label="操作" width="100">
        <template #default="{ row }">
          <router-link :to="`/admin/orders/${row.id}`">
            <el-button link>詳細</el-button>
          </router-link>
        </template>
      </el-table-column>
    </el-table>

    <el-pagination
      v-if="total > pageSize"
      v-model:current-page="page"
      :page-size="pageSize"
      :total="total"
      layout="prev, pager, next"
      class="pagination"
      @current-change="load"
    />
  </div>
</template>

<style scoped>
.title {
  margin: 0 0 20px;
  font-size: 20px;
}
.toolbar {
  margin-bottom: 16px;
}
.table {
  background: #fff;
}
.pagination {
  margin-top: 16px;
  justify-content: center;
}
</style>
