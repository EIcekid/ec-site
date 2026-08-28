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
    ElMessage.warning('パスワードは6文字以上にしてください')
    return
  }
  loading.value = true
  try {
    const result = await authApi.register(email.value, password.value, name.value)
    auth.setAuth(result)
    ElMessage.success('登録が完了しました')
    router.push('/')
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message ?? '登録に失敗しました')
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="auth-page">
    <el-card class="auth-card">
      <h1>新規登録</h1>
      <el-form label-position="top" @submit.prevent="submit">
        <el-form-item label="ニックネーム">
          <el-input v-model="name" placeholder="任意入力、未入力の場合はメールアドレスの@より前を使用" />
        </el-form-item>
        <el-form-item label="メールアドレス">
          <el-input v-model="email" type="email" placeholder="you@example.com" />
        </el-form-item>
        <el-form-item label="パスワード">
          <el-input v-model="password" type="password" show-password placeholder="6文字以上" @keyup.enter="submit" />
        </el-form-item>
        <el-button type="primary" class="submit-btn" :loading="loading" @click="submit">登録</el-button>
      </el-form>
      <p class="switch">
        既にアカウントをお持ちの方は<router-link to="/login">ログイン</router-link>
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
