<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { adminApi } from '../../api/admin'
import type { ProductListItem } from '../../types'

const products = ref<ProductListItem[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = 20
const keyword = ref('')
const loading = ref(true)

async function load() {
  loading.value = true
  try {
    const result = await adminApi.products({ keyword: keyword.value || undefined, page: page.value, pageSize })
    products.value = result.items
    total.value = result.totalCount
  } finally {
    loading.value = false
  }
}

onMounted(load)

async function remove(id: number) {
  await ElMessageBox.confirm('确定要下架该商品吗？', '提示', { type: 'warning' })
  await adminApi.deleteProduct(id)
  ElMessage.success('已下架')
  await load()
}
</script>

<template>
  <div>
    <div class="header-row">
      <h1 class="title">商品管理</h1>
      <router-link to="/admin/products/new">
        <el-button type="primary">新增商品</el-button>
      </router-link>
    </div>

    <div class="toolbar">
      <el-input v-model="keyword" placeholder="搜索商品名..." style="max-width: 260px" @keyup.enter="load" />
      <el-button @click="load">搜索</el-button>
    </div>

    <el-table v-loading="loading" :data="products" class="table">
      <el-table-column label="图片" width="80">
        <template #default="{ row }">
          <img :src="row.imageUrl ?? '/placeholder.png'" class="thumb" />
        </template>
      </el-table-column>
      <el-table-column prop="name" label="名称" min-width="200" />
      <el-table-column prop="categoryName" label="分类" width="120" />
      <el-table-column label="价格" width="100">
        <template #default="{ row }">¥{{ row.price.toFixed(2) }}</template>
      </el-table-column>
      <el-table-column prop="stock" label="库存" width="80" />
      <el-table-column label="操作" width="140">
        <template #default="{ row }">
          <router-link :to="`/admin/products/${row.id}/edit`">
            <el-button link>编辑</el-button>
          </router-link>
          <el-button link type="danger" @click="remove(row.id)">下架</el-button>
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
.header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}
.title {
  margin: 0;
  font-size: 20px;
}
.toolbar {
  display: flex;
  gap: 8px;
  margin-bottom: 16px;
}
.table {
  background: #fff;
}
.thumb {
  width: 48px;
  height: 48px;
  object-fit: cover;
  border-radius: 4px;
}
.pagination {
  margin-top: 16px;
  justify-content: center;
}
</style>
