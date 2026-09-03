<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { useCartStore } from '../stores/cart'
import { addressesApi, ordersApi } from '../api/orders'
import { usersApi } from '../api/users'
import type { Address } from '../types'

const POINT_VALUE_YEN = 0.1

const cart = useCartStore()
const router = useRouter()

const addresses = ref<Address[]>([])
const selectedAddressId = ref<number | null>(null)
const couponCode = ref('')
const submitting = ref(false)

const myPoints = ref(0)
const pointsToUse = ref(0)

const maxUsablePoints = computed(() => {
  const capByBalance = myPoints.value
  const capByAmount = Math.floor(cart.totalAmount / POINT_VALUE_YEN)
  return Math.max(0, Math.min(capByBalance, capByAmount))
})
const pointsDiscount = computed(() => Math.min(pointsToUse.value * POINT_VALUE_YEN, cart.totalAmount))
const estimatedTotal = computed(() => cart.totalAmount - pointsDiscount.value)

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
  const me = await usersApi.me()
  myPoints.value = me.points
})

async function saveAddress() {
  if (!form.value.recipient || !form.value.phone || !form.value.detail) {
    ElMessage.warning('住所情報をすべて入力してください')
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
    ElMessage.warning('配送先住所を選択してください')
    return
  }
  submitting.value = true
  try {
    const order = await ordersApi.create(selectedAddressId.value, couponCode.value || undefined, pointsToUse.value)
    await cart.fetch()
    ElMessage.success('注文が完了しました')
    router.push(`/orders/${order.id}`)
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message ?? '注文に失敗しました')
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="container checkout-page">
    <h1>注文内容の確認</h1>

    <section class="block">
      <h2>配送先住所</h2>
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
        <div class="address-card add-new" @click="showAddressDialog = true">+ 住所を追加</div>
      </div>
    </section>

    <section class="block">
      <h2>注文商品</h2>
      <ul class="item-list">
        <li v-for="item in cart.items" :key="item.id">
          <img :src="item.imageUrl ?? '/placeholder.png'" />
          <span class="name">
            {{ item.productName }}
            <small v-if="item.variantLabel" class="variant-label">{{ item.variantLabel }}</small>
          </span>
          <span>x{{ item.quantity }}</span>
          <span class="subtotal">¥{{ (item.price * item.quantity).toFixed(2) }}</span>
        </li>
      </ul>
    </section>

    <section class="block">
      <h2>クーポン</h2>
      <el-input v-model="couponCode" placeholder="クーポンコードを入力（任意）" style="max-width: 300px" />
    </section>

    <section class="block">
      <h2>ポイント利用</h2>
      <p class="points-balance">保有ポイント：{{ myPoints }} pt（100pt = ¥10）</p>
      <el-input-number v-model="pointsToUse" :min="0" :max="maxUsablePoints" :step="100" />
      <p v-if="pointsToUse > 0" class="points-discount">-¥{{ pointsDiscount.toFixed(2) }} 割引されます</p>
    </section>

    <div class="summary">
      <span>概算合計（クーポン適用前）：</span>
      <span class="total">¥{{ estimatedTotal.toFixed(2) }}</span>
      <el-button type="primary" size="large" :loading="submitting" @click="submitOrder">注文を確定する</el-button>
    </div>

    <el-dialog v-model="showAddressDialog" title="配送先住所を追加" width="420px">
      <el-form label-width="80px">
        <el-form-item label="お名前"><el-input v-model="form.recipient" /></el-form-item>
        <el-form-item label="電話番号"><el-input v-model="form.phone" /></el-form-item>
        <el-form-item label="都道府県"><el-input v-model="form.province" /></el-form-item>
        <el-form-item label="市区町村"><el-input v-model="form.city" /></el-form-item>
        <el-form-item label="番地・建物名"><el-input v-model="form.detail" type="textarea" /></el-form-item>
        <el-form-item label="デフォルトに設定"><el-switch v-model="form.isDefault" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showAddressDialog = false">キャンセル</el-button>
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
.variant-label {
  display: block;
  color: #909399;
  font-size: 12px;
  margin-top: 2px;
}
.points-balance {
  margin: 0 0 12px;
  color: #606266;
  font-size: 14px;
}
.points-discount {
  margin: 8px 0 0;
  color: #f56c6c;
  font-size: 13px;
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
