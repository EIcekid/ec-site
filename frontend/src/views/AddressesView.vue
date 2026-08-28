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
    ElMessage.warning('请填写完整地址信息')
    return
  }
  if (editingId.value) {
    await addressesApi.update(editingId.value, form.value)
  } else {
    await addressesApi.create(form.value)
  }
  showDialog.value = false
  await load()
  ElMessage.success('保存成功')
}

async function remove(id: number) {
  await ElMessageBox.confirm('确定删除该地址吗？', '提示', { type: 'warning' })
  await addressesApi.remove(id)
  await load()
}
</script>

<template>
  <div class="container addresses-page">
    <div class="header-row">
      <h1>收货地址</h1>
      <el-button type="primary" @click="openCreate">新增地址</el-button>
    </div>

    <div v-loading="loading" class="address-list">
      <div v-for="a in addresses" :key="a.id" class="address-card">
        <p><strong>{{ a.recipient }}</strong> {{ a.phone }} <el-tag v-if="a.isDefault" size="small">默认</el-tag></p>
        <p class="addr-detail">{{ a.province }}{{ a.city }}{{ a.detail }}</p>
        <div class="card-actions">
          <el-button link @click="openEdit(a)">编辑</el-button>
          <el-button link type="danger" @click="remove(a.id)">删除</el-button>
        </div>
      </div>
      <el-empty v-if="!loading && addresses.length === 0" description="暂无收货地址" />
    </div>

    <el-dialog v-model="showDialog" :title="editingId ? '编辑地址' : '新增地址'" width="420px">
      <el-form label-width="80px">
        <el-form-item label="收货人"><el-input v-model="form.recipient" /></el-form-item>
        <el-form-item label="手机号"><el-input v-model="form.phone" /></el-form-item>
        <el-form-item label="省份"><el-input v-model="form.province" /></el-form-item>
        <el-form-item label="城市"><el-input v-model="form.city" /></el-form-item>
        <el-form-item label="详细地址"><el-input v-model="form.detail" type="textarea" /></el-form-item>
        <el-form-item label="设为默认"><el-switch v-model="form.isDefault" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showDialog = false">取消</el-button>
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
