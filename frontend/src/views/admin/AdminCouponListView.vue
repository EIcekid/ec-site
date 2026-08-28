<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { ElMessage } from 'element-plus'
import { adminApi } from '../../api/admin'
import type { Coupon } from '../../types'

const coupons = ref<Coupon[]>([])
const loading = ref(true)
const showDialog = ref(false)
const saving = ref(false)

const form = ref({
  code: '',
  type: 'FixedAmount' as 'FixedAmount' | 'Percentage',
  value: 0,
  minOrderAmount: 0,
  expiresAt: '',
})

async function load() {
  loading.value = true
  try {
    coupons.value = await adminApi.coupons()
  } finally {
    loading.value = false
  }
}

onMounted(load)

function openCreate() {
  form.value = { code: '', type: 'FixedAmount', value: 0, minOrderAmount: 0, expiresAt: '' }
  showDialog.value = true
}

async function save() {
  if (!form.value.code || !form.value.expiresAt) {
    ElMessage.warning('すべての項目を入力してください')
    return
  }
  saving.value = true
  try {
    await adminApi.createCoupon({ ...form.value, expiresAt: new Date(form.value.expiresAt).toISOString() })
    showDialog.value = false
    ElMessage.success('作成しました')
    await load()
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message ?? '作成に失敗しました')
  } finally {
    saving.value = false
  }
}

async function deactivate(id: number) {
  await adminApi.deactivateCoupon(id)
  await load()
}
</script>

<template>
  <div>
    <div class="header-row">
      <h1 class="title">クーポン管理</h1>
      <el-button type="primary" @click="openCreate">クーポンを追加</el-button>
    </div>

    <el-table v-loading="loading" :data="coupons" class="table">
      <el-table-column prop="code" label="コード" width="140" />
      <el-table-column label="タイプ" width="100">
        <template #default="{ row }">{{ row.type === 'FixedAmount' ? '定額割引' : '割引率' }}</template>
      </el-table-column>
      <el-table-column label="割引内容" width="100">
        <template #default="{ row }">{{ row.type === 'FixedAmount' ? `¥${row.value}` : `${row.value}%` }}</template>
      </el-table-column>
      <el-table-column label="最低利用金額" width="100">
        <template #default="{ row }">¥{{ row.minOrderAmount.toFixed(2) }}</template>
      </el-table-column>
      <el-table-column label="有効期限" width="180">
        <template #default="{ row }">{{ new Date(row.expiresAt).toLocaleDateString() }}</template>
      </el-table-column>
      <el-table-column label="状態" width="100">
        <template #default="{ row }">
          <el-tag :type="row.isActive ? 'success' : 'info'">{{ row.isActive ? '有効' : '停止中' }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="100">
        <template #default="{ row }">
          <el-button v-if="row.isActive" link type="danger" @click="deactivate(row.id)">停止</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="showDialog" title="クーポンを追加" width="420px">
      <el-form label-width="90px">
        <el-form-item label="クーポンコード"><el-input v-model="form.code" placeholder="例：WELCOME10" /></el-form-item>
        <el-form-item label="タイプ">
          <el-select v-model="form.type">
            <el-option label="定額割引" value="FixedAmount" />
            <el-option label="割引率（％）" value="Percentage" />
          </el-select>
        </el-form-item>
        <el-form-item :label="form.type === 'FixedAmount' ? '割引額' : '割引率'">
          <el-input-number v-model="form.value" :min="0" :precision="2" />
        </el-form-item>
        <el-form-item label="最低利用金額"><el-input-number v-model="form.minOrderAmount" :min="0" :precision="2" /></el-form-item>
        <el-form-item label="有効期限"><el-date-picker v-model="form.expiresAt" type="date" style="width: 100%" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showDialog = false">キャンセル</el-button>
        <el-button type="primary" :loading="saving" @click="save">保存</el-button>
      </template>
    </el-dialog>
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
.table {
  background: #fff;
}
</style>
