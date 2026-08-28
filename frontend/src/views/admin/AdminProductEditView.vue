<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { adminApi } from '../../api/admin'
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
    }
  }
})

async function handleUpload(file: File) {
  uploading.value = true
  try {
    const { url } = await adminApi.uploadImage(file)
    form.value.images.push(url)
  } catch {
    ElMessage.error('上传失败')
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
    ElMessage.warning('请填写商品名称并选择分类')
    return
  }
  saving.value = true
  try {
    if (isEdit.value) {
      await adminApi.updateProduct(Number(props.id), { ...form.value, categoryId: form.value.categoryId!, isActive: true })
    } else {
      await adminApi.createProduct({ ...form.value, categoryId: form.value.categoryId! })
    }
    ElMessage.success('保存成功')
    router.push('/admin/products')
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message ?? '保存失败')
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <div class="edit-page">
    <h1 class="title">{{ isEdit ? '编辑商品' : '新增商品' }}</h1>

    <el-form label-width="100px" class="form">
      <el-form-item label="商品名称">
        <el-input v-model="form.name" />
      </el-form-item>
      <el-form-item label="商品描述">
        <el-input v-model="form.description" type="textarea" :rows="4" />
      </el-form-item>
      <el-form-item label="价格">
        <el-input-number v-model="form.price" :min="0" :precision="2" />
      </el-form-item>
      <el-form-item label="库存">
        <el-input-number v-model="form.stock" :min="0" />
      </el-form-item>
      <el-form-item label="分类">
        <el-select v-model="form.categoryId" placeholder="请选择分类" style="width: 240px">
          <el-option v-for="c in flatCategories" :key="c.id" :label="c.name" :value="c.id" />
        </el-select>
        <el-button link @click="showNewCategory = !showNewCategory">+ 新建分类</el-button>
        <div v-if="showNewCategory" class="new-category">
          <el-input v-model="newCategoryName" placeholder="分类名称" style="width: 180px" />
          <el-button size="small" type="primary" @click="addCategory">添加</el-button>
        </div>
      </el-form-item>
      <el-form-item label="商品图片">
        <div class="image-list">
          <div v-for="(img, idx) in form.images" :key="img" class="image-item">
            <img :src="img" />
            <el-button size="small" type="danger" circle :icon="'Close'" class="remove-btn" @click="removeImage(idx)" />
          </div>
          <el-upload :show-file-list="false" :before-upload="beforeUpload" accept="image/*">
            <div class="upload-trigger" :class="{ loading: uploading }">
              <span v-if="!uploading">+ 上传</span>
              <span v-else>上传中...</span>
            </div>
          </el-upload>
        </div>
      </el-form-item>

      <el-form-item>
        <el-button type="primary" :loading="saving" @click="submit">保存</el-button>
        <el-button @click="router.push('/admin/products')">取消</el-button>
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
