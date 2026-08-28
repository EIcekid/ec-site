<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { authApi } from '../api/auth'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const auth = useAuthStore()

const name = ref('')
const email = ref('')
const password = ref('')
const loading = ref(false)

async function submit() {
  if (password.value.length < 6) {
    ElMessage.warning('密码至少6位')
    return
  }
  loading.value = true
  try {
    const result = await authApi.register(email.value, password.value, name.value)
    auth.setAuth(result)
    ElMessage.success('注册成功')
    router.push('/')
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message ?? '注册失败')
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="auth-page">
    <el-card class="auth-card">
      <h1>注册</h1>
      <el-form label-position="top" @submit.prevent="submit">
        <el-form-item label="昵称">
          <el-input v-model="name" placeholder="选填，默认取邮箱前缀" />
        </el-form-item>
        <el-form-item label="邮箱">
          <el-input v-model="email" type="email" placeholder="you@example.com" />
        </el-form-item>
        <el-form-item label="密码">
          <el-input v-model="password" type="password" show-password placeholder="至少6位" @keyup.enter="submit" />
        </el-form-item>
        <el-button type="primary" class="submit-btn" :loading="loading" @click="submit">注册</el-button>
      </el-form>
      <p class="switch">
        已有账号？<router-link to="/login">去登录</router-link>
      </p>
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
</style>
