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
const sort = ref((route.query.sort as string) ?? 'newest')
const minPrice = ref<number | undefined>(route.query.minPrice ? Number(route.query.minPrice) : undefined)
const maxPrice = ref<number | undefined>(route.query.maxPrice ? Number(route.query.maxPrice) : undefined)
const page = ref(Number(route.query.page ?? 1))
const pageSize = 12

const sortOptions = [
  { label: '新着順', value: 'newest' },
  { label: '価格が安い順', value: 'price_asc' },
  { label: '価格が高い順', value: 'price_desc' },
  { label: '評価が高い順', value: 'rating' },
  { label: '売れ筋順', value: 'sales' },
]

async function load() {
  loading.value = true
  try {
    const result = await productsApi.list({
      keyword: keyword.value || undefined,
      categoryId: categoryId.value,
      minPrice: minPrice.value,
      maxPrice: maxPrice.value,
      sort: sort.value,
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

watch([keyword, categoryId, sort, minPrice, maxPrice, page], () => {
  router.replace({
    query: {
      ...(keyword.value ? { keyword: keyword.value } : {}),
      ...(categoryId.value ? { categoryId: String(categoryId.value) } : {}),
      ...(sort.value !== 'newest' ? { sort: sort.value } : {}),
      ...(minPrice.value !== undefined ? { minPrice: String(minPrice.value) } : {}),
      ...(maxPrice.value !== undefined ? { maxPrice: String(maxPrice.value) } : {}),
      ...(page.value > 1 ? { page: String(page.value) } : {}),
    },
  })
  load()
})

function selectCategory(id: number | undefined) {
  categoryId.value = id
  page.value = 1
}

function applyPriceFilter() {
  page.value = 1
}
</script>

<template>
  <div class="container list-page">
    <aside class="sidebar">
      <h3>商品カテゴリー</h3>
      <ul class="category-list">
        <li :class="{ active: !categoryId }" @click="selectCategory(undefined)">全商品</li>
        <li
          v-for="c in flatCategories"
          :key="c.id"
          :class="{ active: categoryId === c.id }"
          @click="selectCategory(c.id)"
        >
          {{ c.name }}
        </li>
      </ul>

      <h3 class="price-title">価格帯</h3>
      <div class="price-filter">
        <el-input-number v-model="minPrice" :min="0" :controls="false" placeholder="下限" size="small" />
        <span>〜</span>
        <el-input-number v-model="maxPrice" :min="0" :controls="false" placeholder="上限" size="small" />
      </div>
      <el-button size="small" class="price-btn" @click="applyPriceFilter">絞り込む</el-button>
    </aside>

    <div class="content">
      <div class="toolbar">
        <el-input v-model="keyword" placeholder="商品を検索..." clearable style="max-width: 300px" @keyup.enter="page = 1" />
        <el-select v-model="sort" style="width: 160px">
          <el-option v-for="o in sortOptions" :key="o.value" :label="o.label" :value="o.value" />
        </el-select>
      </div>

      <div v-loading="loading" class="grid">
        <ProductCard v-for="p in products" :key="p.id" :product="p" />
        <el-empty v-if="!loading && products.length === 0" description="商品がありません" />
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
.price-title {
  margin-top: 20px;
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
.price-filter {
  display: flex;
  align-items: center;
  gap: 6px;
  color: #909399;
  font-size: 13px;
}
.price-filter :deep(.el-input-number) {
  width: 76px;
}
.price-btn {
  width: 100%;
  margin-top: 10px;
}
.content {
  flex: 1;
  min-width: 0;
}
.toolbar {
  margin-bottom: 16px;
  display: flex;
  gap: 12px;
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
