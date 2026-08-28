<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { useCartStore } from '../stores/cart'
import { addressesApi, ordersApi } from '../api/orders'
import type { Address } from '../types'

const cart = useCartStore()
const router = useRouter()

const addresses = ref<Address[]>([])
const selectedAddressId = ref<number | null>(null)
const couponCode = ref('')
const submitting = ref(false)

const showAddressDialog = ref(false)
const form = ref({ recipient: '', phone: '', province: '', city: '', detail: '', isDefault: false })

async function loadAddresses() {
  addresses.value = await addressesApi.list()
  const def = addresses.value.find((a) => a.isDefault) ?? addresses.value[0]
  selectedAddressId.value = def?.id ?? null
}

onMounted(async () => {
  await cart.fetch()
  if (cart.items.length === 0) {
    router.replace('/cart')
    return
  }
  await loadAddresses()
})

async function saveAddress() {
  if (!form.value.recipient || !form.value.phone || !form.value.detail) {
    ElMessage.warning('请填写完整地址信息')
    return
  }
  const created = await addressesApi.create(form.value)
  await loadAddresses()
  selectedAddressId.value = created.id
  showAddressDialog.value = false
  form.value = { recipient: '', phone: '', province: '', city: '', detail: '', isDefault: false }
}

async function submitOrder() {
  if (!selectedAddressId.value) {
    ElMessage.warning('请选择收货地址')
    return
  }
  submitting.value = true
  try {
    const order = await ordersApi.create(selectedAddressId.value, couponCode.value || undefined)
    await cart.fetch()
    ElMessage.success('订单创建成功')
    router.push(`/orders/${order.id}`)
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message ?? '下单失败')
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="container checkout-page">
    <h1>确认订单</h1>

    <section class="block">
      <h2>收货地址</h2>
      <div class="address-list">
        <div
          v-for="a in addresses"
          :key="a.id"
          class="address-card"
          :class="{ active: selectedAddressId === a.id }"
          @click="selectedAddressId = a.id"
        >
          <p><strong>{{ a.recipient }}</strong> {{ a.phone }}</p>
          <p class="addr-detail">{{ a.province }}{{ a.city }}{{ a.detail }}</p>
        </div>
        <div class="address-card add-new" @click="showAddressDialog = true">+ 新增地址</div>
      </div>
    </section>

    <section class="block">
      <h2>商品清单</h2>
      <ul class="item-list">
        <li v-for="item in cart.items" :key="item.id">
          <img :src="item.imageUrl ?? '/placeholder.png'" />
          <span class="name">{{ item.productName }}</span>
          <span>x{{ item.quantity }}</span>
          <span class="subtotal">¥{{ (item.price * item.quantity).toFixed(2) }}</span>
        </li>
      </ul>
    </section>

    <section class="block">
      <h2>优惠券</h2>
      <el-input v-model="couponCode" placeholder="输入优惠码（可选）" style="max-width: 300px" />
    </section>

    <div class="summary">
      <span>合计：</span>
      <span class="total">¥{{ cart.totalAmount.toFixed(2) }}</span>
      <el-button type="primary" size="large" :loading="submitting" @click="submitOrder">提交订单</el-button>
    </div>

    <el-dialog v-model="showAddressDialog" title="新增收货地址" width="420px">
      <el-form label-width="80px">
        <el-form-item label="收货人"><el-input v-model="form.recipient" /></el-form-item>
        <el-form-item label="手机号"><el-input v-model="form.phone" /></el-form-item>
        <el-form-item label="省份"><el-input v-model="form.province" /></el-form-item>
        <el-form-item label="城市"><el-input v-model="form.city" /></el-form-item>
        <el-form-item label="详细地址"><el-input v-model="form.detail" type="textarea" /></el-form-item>
        <el-form-item label="设为默认"><el-switch v-model="form.isDefault" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showAddressDialog = false">取消</el-button>
        <el-button type="primary" @click="saveAddress">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.checkout-page {
  padding: 24px 20px 60px;
}
.block {
  background: #fff;
  border-radius: 8px;
  padding: 20px;
  margin-bottom: 16px;
}
.block h2 {
  margin: 0 0 16px;
  font-size: 16px;
}
.address-list {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}
.address-card {
  width: 220px;
  border: 1px solid #dcdfe6;
  border-radius: 6px;
  padding: 12px;
  cursor: pointer;
  font-size: 13px;
}
.address-card.active {
  border-color: #409eff;
  background: #ecf5ff;
}
.address-card.add-new {
  display: flex;
  align-items: center;
  justify-content: center;
  color: #909399;
}
.addr-detail {
  color: #909399;
  margin: 4px 0 0;
}
.item-list {
  list-style: none;
  margin: 0;
  padding: 0;
}
.item-list li {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px 0;
  border-bottom: 1px solid #f0f0f0;
}
.item-list img {
  width: 48px;
  height: 48px;
  object-fit: cover;
  border-radius: 4px;
}
.item-list .name {
  flex: 1;
}
.subtotal {
  color: #f56c6c;
  font-weight: 600;
  width: 90px;
  text-align: right;
}
.summary {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 12px;
  background: #fff;
  padding: 20px;
  border-radius: 8px;
}
.total {
  color: #f56c6c;
  font-size: 22px;
  font-weight: 700;
}
</style>
