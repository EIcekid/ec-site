<script setup lang="ts">
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { authApi } from '../api/auth'
import { useAuthStore } from '../stores/auth'
import { useCartStore } from '../stores/cart'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const cart = useCartStore()

const email = ref('')
const password = ref('')
const loading = ref(false)

async function submit() {
  loading.value = true
  try {
    const result = await authApi.login(email.value, password.value)
    auth.setAuth(result)
    await cart.fetch()
    const redirect = (route.query.redirect as string) || (result.role === 'Admin' ? '/admin' : '/')
    router.push(redirect)
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message ?? 'ログインに失敗しました')
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="auth-page">
    <el-card class="auth-card">
      <h1>ログイン</h1>
      <el-form label-position="top" @submit.prevent="submit">
        <el-form-item label="メールアドレス">
          <el-input v-model="email" type="email" placeholder="you@example.com" />
        </el-form-item>
        <el-form-item label="パスワード">
          <el-input v-model="password" type="password" show-password @keyup.enter="submit" />
        </el-form-item>
        <el-button type="primary" class="submit-btn" :loading="loading" @click="submit">ログイン</el-button>
      </el-form>
      <p class="switch">
        アカウントをお持ちでない方は<router-link to="/register">今すぐ登録</router-link>
      </p>
      <p class="hint">管理者テストアカウント：admin@ec-site.local / Admin@123</p>
    </el-card>
  </div>
</template>

<style scoped>
.auth-page {
  display: flex;
  justify-content: center;
  padding: 80px 20px;
}
.auth-card {
  width: 380px;
}
.auth-card h1 {
  text-align: center;
  font-size: 20px;
  margin: 0 0 20px;
}
.submit-btn {
  width: 100%;
}
.switch {
  text-align: center;
  margin-top: 16px;
  font-size: 13px;
  color: #909399;
}
.switch a {
  color: #409eff;
}
.hint {
  text-align: center;
  margin-top: 8px;
  font-size: 12px;
  color: #c0c4cc;
}
</style>
