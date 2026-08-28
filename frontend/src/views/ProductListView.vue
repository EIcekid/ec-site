<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { productsApi } from '../api/products'
import type { Category, ProductListItem } from '../types'
import ProductCard from '../components/ProductCard.vue'

const route = useRoute()
const router = useRouter()

const products = ref<ProductListItem[]>([])
const categories = ref<Category[]>([])
const total = ref(0)
const loading = ref(true)

const keyword = ref((route.query.keyword as string) ?? '')
const categoryId = ref<number | undefined>(route.query.categoryId ? Number(route.query.categoryId) : undefined)
const page = ref(Number(route.query.page ?? 1))
const pageSize = 12

async function load() {
  loading.value = true
  try {
    const result = await productsApi.list({
      keyword: keyword.value || undefined,
      categoryId: categoryId.value,
      page: page.value,
      pageSize,
    })
    products.value = result.items
    total.value = result.totalCount
  } finally {
    loading.value = false
  }
}

function flattenCategories(list: Category[]): Category[] {
  return list.flatMap((c) => [c, ...flattenCategories(c.children)])
}
const flatCategories = ref<Category[]>([])

onMounted(async () => {
  categories.value = await productsApi.categories()
  flatCategories.value = flattenCategories(categories.value)
  await load()
})

watch([keyword, categoryId, page], () => {
  router.replace({
    query: {
      ...(keyword.value ? { keyword: keyword.value } : {}),
      ...(categoryId.value ? { categoryId: String(categoryId.value) } : {}),
      ...(page.value > 1 ? { page: String(page.value) } : {}),
    },
  })
  load()
})

function selectCategory(id: number | undefined) {
  categoryId.value = id
  page.value = 1
}
</script>

<template>
  <div class="container list-page">
    <aside class="sidebar">
      <h3>商品分类</h3>
      <ul class="category-list">
        <li :class="{ active: !categoryId }" @click="selectCategory(undefined)">全部商品</li>
        <li
          v-for="c in flatCategories"
          :key="c.id"
          :class="{ active: categoryId === c.id }"
          @click="selectCategory(c.id)"
        >
          {{ c.name }}
        </li>
      </ul>
    </aside>

    <div class="content">
      <div class="toolbar">
        <el-input v-model="keyword" placeholder="搜索商品..." clearable style="max-width: 300px" @keyup.enter="page = 1" />
      </div>

      <div v-loading="loading" class="grid">
        <ProductCard v-for="p in products" :key="p.id" :product="p" />
        <el-empty v-if="!loading && products.length === 0" description="暂无商品" />
      </div>

      <el-pagination
        v-if="total > pageSize"
        v-model:current-page="page"
        :page-size="pageSize"
        :total="total"
        layout="prev, pager, next"
        class="pagination"
      />
    </div>
  </div>
</template>

<style scoped>
.list-page {
  display: flex;
  gap: 24px;
  padding-top: 24px;
  padding-bottom: 40px;
  align-items: flex-start;
}
.sidebar {
  width: 200px;
  flex-shrink: 0;
  background: #fff;
  border-radius: 8px;
  padding: 16px;
}
.sidebar h3 {
  margin: 0 0 12px;
  font-size: 15px;
}
.category-list {
  list-style: none;
  margin: 0;
  padding: 0;
}
.category-list li {
  padding: 8px 0;
  font-size: 14px;
  color: #606266;
  cursor: pointer;
}
.category-list li.active {
  color: #409eff;
  font-weight: 600;
}
.content {
  flex: 1;
  min-width: 0;
}
.toolbar {
  margin-bottom: 16px;
}
.grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 16px;
  min-height: 200px;
}
.pagination {
  margin-top: 24px;
  justify-content: center;
}
</style>
