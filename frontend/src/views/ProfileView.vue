<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { ElMessage } from 'element-plus'
import { usersApi } from '../api/users'
import { useAuthStore } from '../stores/auth'
import type { Me } from '../types'

const auth = useAuthStore()

const me = ref<Me | null>(null)
const loading = ref(true)

const nameForm = ref({ name: '' })
const savingName = ref(false)

const passwordForm = ref({ currentPassword: '', newPassword: '', confirmPassword: '' })
const savingPassword = ref(false)

onMounted(async () => {
  loading.value = true
  try {
    me.value = await usersApi.me()
    nameForm.value.name = me.value.name
  } finally {
    loading.value = false
  }
})

async function saveName() {
  if (!nameForm.value.name.trim()) {
    ElMessage.warning('ニックネームを入力してください')
    return
  }
  savingName.value = true
  try {
    const updated = await usersApi.updateProfile(nameForm.value.name.trim())
    me.value = updated
    auth.updateName(updated.name)
    ElMessage.success('プロフィールを更新しました')
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message ?? '更新に失敗しました')
  } finally {
    savingName.value = false
  }
}

async function savePassword() {
  if (passwordForm.value.newPassword.length < 6) {
    ElMessage.warning('新しいパスワードは6文字以上にしてください')
    return
  }
  if (passwordForm.value.newPassword !== passwordForm.value.confirmPassword) {
    ElMessage.warning('新しいパスワードが一致しません')
    return
  }
  savingPassword.value = true
  try {
    await usersApi.changePassword(passwordForm.value.currentPassword, passwordForm.value.newPassword)
    passwordForm.value = { currentPassword: '', newPassword: '', confirmPassword: '' }
    ElMessage.success('パスワードを変更しました')
  } catch (e: any) {
    ElMessage.error(e.response?.data?.message ?? 'パスワードの変更に失敗しました')
  } finally {
    savingPassword.value = false
  }
}
</script>

<template>
  <div v-loading="loading" class="container profile-page">
    <h1>マイページ</h1>

    <section v-if="me" class="block">
      <h2>アカウント情報</h2>
      <p class="info-row"><span class="label">メールアドレス</span>{{ me.email }}</p>
      <p class="info-row"><span class="label">保有ポイント</span>{{ me.points }} pt</p>
    </section>

    <section class="block">
      <h2>ニックネームの変更</h2>
      <el-form label-width="100px" class="form">
        <el-form-item label="ニックネーム">
          <el-input v-model="nameForm.name" style="max-width: 300px" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" :loading="savingName" @click="saveName">保存</el-button>
        </el-form-item>
      </el-form>
    </section>

    <section class="block">
      <h2>パスワードの変更</h2>
      <el-form label-width="140px" class="form">
        <el-form-item label="現在のパスワード">
          <el-input v-model="passwordForm.currentPassword" type="password" show-password style="max-width: 300px" />
        </el-form-item>
        <el-form-item label="新しいパスワード">
          <el-input v-model="passwordForm.newPassword" type="password" show-password placeholder="6文字以上" style="max-width: 300px" />
        </el-form-item>
        <el-form-item label="新しいパスワード（確認）">
          <el-input v-model="passwordForm.confirmPassword" type="password" show-password style="max-width: 300px" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" :loading="savingPassword" @click="savePassword">変更する</el-button>
        </el-form-item>
      </el-form>
    </section>
  </div>
</template>

<style scoped>
.profile-page {
  padding: 24px 20px 60px;
  max-width: 700px;
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
.info-row {
  margin: 0 0 8px;
  font-size: 14px;
  color: #303133;
}
.info-row .label {
  display: inline-block;
  width: 120px;
  color: #909399;
}
.form {
  max-width: 500px;
}
</style>
