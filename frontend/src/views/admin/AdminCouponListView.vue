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
    ElMessage.warning('请填写完整信息')
    return
  }
  saving.value = true
  try {
    await adminApi.createCoupon({ ...form.value, expiresAt: new Date(form.value.expiresAt).toISOString() })
    showDialog.value = false
    ElMessage.success('创建成功')
    await load()
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message ?? '创建失败')
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
      <h1 class="title">优惠券管理</h1>
      <el-button type="primary" @click="openCreate">新增优惠券</el-button>
    </div>

    <el-table v-loading="loading" :data="coupons" class="table">
      <el-table-column prop="code" label="代码" width="140" />
      <el-table-column label="类型" width="100">
        <template #default="{ row }">{{ row.type === 'FixedAmount' ? '满减' : '折扣' }}</template>
      </el-table-column>
      <el-table-column label="优惠" width="100">
        <template #default="{ row }">{{ row.type === 'FixedAmount' ? `¥${row.value}` : `${row.value}%` }}</template>
      </el-table-column>
      <el-table-column label="最低消费" width="100">
        <template #default="{ row }">¥{{ row.minOrderAmount.toFixed(2) }}</template>
      </el-table-column>
      <el-table-column label="过期时间" width="180">
        <template #default="{ row }">{{ new Date(row.expiresAt).toLocaleDateString() }}</template>
      </el-table-column>
      <el-table-column label="状态" width="100">
        <template #default="{ row }">
          <el-tag :type="row.isActive ? 'success' : 'info'">{{ row.isActive ? '生效中' : '已停用' }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="100">
        <template #default="{ row }">
          <el-button v-if="row.isActive" link type="danger" @click="deactivate(row.id)">停用</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="showDialog" title="新增优惠券" width="420px">
      <el-form label-width="90px">
        <el-form-item label="优惠码"><el-input v-model="form.code" placeholder="如 WELCOME10" /></el-form-item>
        <el-form-item label="类型">
          <el-select v-model="form.type">
            <el-option label="满减（固定金额）" value="FixedAmount" />
            <el-option label="折扣（百分比）" value="Percentage" />
          </el-select>
        </el-form-item>
        <el-form-item :label="form.type === 'FixedAmount' ? '减免金额' : '折扣百分比'">
          <el-input-number v-model="form.value" :min="0" :precision="2" />
        </el-form-item>
        <el-form-item label="最低消费"><el-input-number v-model="form.minOrderAmount" :min="0" :precision="2" /></el-form-item>
        <el-form-item label="过期时间"><el-date-picker v-model="form.expiresAt" type="date" style="width: 100%" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showDialog = false">取消</el-button>
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
