<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { adminApi, type ProductVariantForm } from '../../api/admin'
import { productsApi } from '../../api/products'
import type { Category } from '../../types'

const props = defineProps<{ id?: string }>()
const router = useRouter()
const isEdit = computed(() => !!props.id)

const categories = ref<Category[]>([])
const flatCategories = ref<{ id: number; name: string }[]>([])

const form = ref({
  name: '',
  description: '',
  price: 0,
  stock: 0,
  categoryId: undefined as number | undefined,
  images: [] as string[],
  variants: [] as ProductVariantForm[],
})
const saving = ref(false)
const uploading = ref(false)

const newCategoryName = ref('')
const showNewCategory = ref(false)

function flatten(list: Category[], prefix = ''): { id: number; name: string }[] {
  return list.flatMap((c) => [{ id: c.id, name: prefix + c.name }, ...flatten(c.children, prefix + c.name + ' / ')])
}

async function loadCategories() {
  categories.value = await productsApi.categories()
  flatCategories.value = flatten(categories.value)
}

onMounted(async () => {
  await loadCategories()
  if (isEdit.value) {
    const p = await adminApi.product(Number(props.id))
    form.value = {
      name: p.name,
      description: p.description,
      price: p.price,
      stock: p.stock,
      categoryId: p.categoryId,
      images: p.images,
      variants: p.variants.map((v) => ({ color: v.color, size: v.size, sku: v.sku, priceDelta: v.priceDelta, stock: v.stock })),
    }
  }
})

async function handleUpload(file: File) {
  uploading.value = true
  try {
    const { url } = await adminApi.uploadImage(file)
    form.value.images.push(url)
  } catch {
    ElMessage.error('アップロードに失敗しました')
  } finally {
    uploading.value = false
  }
}

function beforeUpload(rawFile: File) {
  handleUpload(rawFile)
  return false
}

function removeImage(idx: number) {
  form.value.images.splice(idx, 1)
}

function addVariant() {
  form.value.variants.push({ color: '', size: '', sku: '', priceDelta: 0, stock: 0 })
}

function removeVariant(idx: number) {
  form.value.variants.splice(idx, 1)
}

async function addCategory() {
  if (!newCategoryName.value.trim()) return
  const created = await adminApi.createCategory(newCategoryName.value, null)
  await loadCategories()
  form.value.categoryId = created.id
  newCategoryName.value = ''
  showNewCategory.value = false
}

async function submit() {
  if (!form.value.name || !form.value.categoryId) {
    ElMessage.warning('商品名を入力し、カテゴリーを選択してください')
    return
  }
  saving.value = true
  try {
    const variants = form.value.variants
      .filter((v) => (v.color && v.color.trim()) || (v.size && v.size.trim()))
      .map((v) => ({
        color: v.color?.trim() || null,
        size: v.size?.trim() || null,
        sku: v.sku.trim(),
        priceDelta: v.priceDelta,
        stock: v.stock,
      }))
    const payload = { ...form.value, categoryId: form.value.categoryId!, variants }
    if (isEdit.value) {
      await adminApi.updateProduct(Number(props.id), { ...payload, isActive: true })
    } else {
      await adminApi.createProduct(payload)
    }
    ElMessage.success('保存しました')
    router.push('/admin/products')
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message ?? '保存に失敗しました')
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <div class="edit-page">
    <h1 class="title">{{ isEdit ? '商品を編集' : '商品を追加' }}</h1>

    <el-form label-width="100px" class="form">
      <el-form-item label="商品名">
        <el-input v-model="form.name" />
      </el-form-item>
      <el-form-item label="商品説明">
        <el-input v-model="form.description" type="textarea" :rows="4" />
      </el-form-item>
      <el-form-item label="価格">
        <el-input-number v-model="form.price" :min="0" :precision="2" />
      </el-form-item>
      <el-form-item label="在庫数">
        <el-input-number v-model="form.stock" :min="0" :disabled="form.variants.length > 0" />
        <p v-if="form.variants.length > 0" class="field-hint">規格ごとの在庫を使用するため、この値は無視されます</p>
      </el-form-item>
      <el-form-item label="カテゴリー">
        <el-select v-model="form.categoryId" placeholder="カテゴリーを選択" style="width: 240px">
          <el-option v-for="c in flatCategories" :key="c.id" :label="c.name" :value="c.id" />
        </el-select>
        <el-button link @click="showNewCategory = !showNewCategory">+ カテゴリーを新規作成</el-button>
        <div v-if="showNewCategory" class="new-category">
          <el-input v-model="newCategoryName" placeholder="カテゴリー名" style="width: 180px" />
          <el-button size="small" type="primary" @click="addCategory">追加</el-button>
        </div>
      </el-form-item>
      <el-form-item label="商品画像">
        <div class="image-list">
          <div v-for="(img, idx) in form.images" :key="img" class="image-item">
            <img :src="img" />
            <el-button size="small" type="danger" circle :icon="'Close'" class="remove-btn" @click="removeImage(idx)" />
          </div>
          <el-upload :show-file-list="false" :before-upload="beforeUpload" accept="image/*">
            <div class="upload-trigger" :class="{ loading: uploading }">
              <span v-if="!uploading">+ アップロード</span>
              <span v-else>アップロード中...</span>
            </div>
          </el-upload>
        </div>
      </el-form-item>

      <el-form-item label="規格（任意）">
        <div class="variant-list">
          <div v-for="(v, idx) in form.variants" :key="idx" class="variant-row">
            <el-input v-model="v.color" placeholder="カラー（例：ブラック）" style="width: 140px" />
            <el-input v-model="v.size" placeholder="サイズ（例：M）" style="width: 100px" />
            <el-input v-model="v.sku" placeholder="SKU" style="width: 120px" />
            <el-input-number v-model="v.priceDelta" placeholder="価格差" :precision="2" style="width: 120px" />
            <el-input-number v-model="v.stock" :min="0" placeholder="在庫" style="width: 100px" />
            <el-button type="danger" circle :icon="'Close'" size="small" @click="removeVariant(idx)" />
          </div>
        </div>
        <el-button size="small" @click="addVariant">+ 規格を追加</el-button>
      </el-form-item>

      <el-form-item>
        <el-button type="primary" :loading="saving" @click="submit">保存</el-button>
        <el-button @click="router.push('/admin/products')">キャンセル</el-button>
      </el-form-item>
    </el-form>
  </div>
</template>

<style scoped>
.title {
  margin: 0 0 20px;
  font-size: 20px;
}
.form {
  max-width: 600px;
  background: #fff;
  padding: 24px;
  border-radius: 8px;
}
.new-category {
  display: flex;
  gap: 8px;
  margin-top: 8px;
}
.field-hint {
  margin: 4px 0 0;
  font-size: 12px;
  color: #e6a23c;
}
.variant-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 8px;
}
.variant-row {
  display: flex;
  gap: 8px;
  align-items: center;
}
.image-list {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}
.image-item {
  position: relative;
  width: 90px;
  height: 90px;
}
.image-item img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border-radius: 4px;
}
.remove-btn {
  position: absolute;
  top: -8px;
  right: -8px;
}
.upload-trigger {
  width: 90px;
  height: 90px;
  border: 1px dashed #dcdfe6;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #909399;
  font-size: 13px;
  cursor: pointer;
}
</style>
