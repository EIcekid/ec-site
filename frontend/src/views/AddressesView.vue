<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { addressesApi } from '../api/orders'
import type { Address } from '../types'

const addresses = ref<Address[]>([])
const loading = ref(true)
const showDialog = ref(false)
const editingId = ref<number | null>(null)
const form = ref({ recipient: '', phone: '', province: '', city: '', detail: '', isDefault: false })

async function load() {
  loading.value = true
  try {
    addresses.value = await addressesApi.list()
  } finally {
    loading.value = false
  }
}

onMounted(load)

function openCreate() {
  editingId.value = null
  form.value = { recipient: '', phone: '', province: '', city: '', detail: '', isDefault: false }
  showDialog.value = true
}

function openEdit(a: Address) {
  editingId.value = a.id
  form.value = { recipient: a.recipient, phone: a.phone, province: a.province, city: a.city, detail: a.detail, isDefault: a.isDefault }
  showDialog.value = true
}

async function save() {
  if (!form.value.recipient || !form.value.phone || !form.value.detail) {
    ElMessage.warning('住所情報をすべて入力してください')
    return
  }
  if (editingId.value) {
    await addressesApi.update(editingId.value, form.value)
  } else {
    await addressesApi.create(form.value)
  }
  showDialog.value = false
  await load()
  ElMessage.success('保存しました')
}

async function remove(id: number) {
  await ElMessageBox.confirm('この住所を削除しますか？', '確認', { type: 'warning' })
  await addressesApi.remove(id)
  await load()
}
</script>

<template>
  <div class="container addresses-page">
    <div class="header-row">
      <h1>配送先住所</h1>
      <el-button type="primary" @click="openCreate">住所を追加</el-button>
    </div>

    <div v-loading="loading" class="address-list">
      <div v-for="a in addresses" :key="a.id" class="address-card">
        <p><strong>{{ a.recipient }}</strong> {{ a.phone }} <el-tag v-if="a.isDefault" size="small">デフォルト</el-tag></p>
        <p class="addr-detail">{{ a.province }}{{ a.city }}{{ a.detail }}</p>
        <div class="card-actions">
          <el-button link @click="openEdit(a)">編集</el-button>
          <el-button link type="danger" @click="remove(a.id)">削除</el-button>
        </div>
      </div>
      <el-empty v-if="!loading && addresses.length === 0" description="配送先住所がありません" />
    </div>

    <el-dialog v-model="showDialog" :title="editingId ? '住所を編集' : '住所を追加'" width="420px">
      <el-form label-width="80px">
        <el-form-item label="お名前"><el-input v-model="form.recipient" /></el-form-item>
        <el-form-item label="電話番号"><el-input v-model="form.phone" /></el-form-item>
        <el-form-item label="都道府県"><el-input v-model="form.province" /></el-form-item>
        <el-form-item label="市区町村"><el-input v-model="form.city" /></el-form-item>
        <el-form-item label="番地・建物名"><el-input v-model="form.detail" type="textarea" /></el-form-item>
        <el-form-item label="デフォルトに設定"><el-switch v-model="form.isDefault" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showDialog = false">キャンセル</el-button>
        <el-button type="primary" @click="save">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.addresses-page {
  padding: 24px 20px 60px;
}
.header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}
.header-row h1 {
  margin: 0;
  font-size: 20px;
}
.address-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 12px;
}
.address-card {
  background: #fff;
  border-radius: 8px;
  padding: 16px;
  font-size: 14px;
}
.addr-detail {
  color: #909399;
  margin: 6px 0;
}
.card-actions {
  display: flex;
  gap: 8px;
}
</style>
