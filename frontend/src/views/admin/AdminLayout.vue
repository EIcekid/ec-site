<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'

const auth = useAuthStore()
const router = useRouter()

function logout() {
  auth.logout()
  router.push('/login')
}
</script>

<template>
  <div class="admin-shell">
    <aside class="sidebar">
      <div class="brand">EC Site 管理画面</div>
      <nav>
        <router-link to="/admin" exact-active-class="active">概要</router-link>
        <router-link to="/admin/products" active-class="active">商品管理</router-link>
        <router-link to="/admin/orders" active-class="active">注文管理</router-link>
        <router-link to="/admin/coupons" active-class="active">クーポン管理</router-link>
      </nav>
    </aside>
    <div class="main">
      <header class="topbar">
        <router-link to="/" class="back-link">ショップに戻る</router-link>
        <div class="right">
          <span>{{ auth.name }}</span>
          <a href="#" @click.prevent="logout">ログアウト</a>
        </div>
      </header>
      <div class="content">
        <router-view />
      </div>
    </div>
  </div>
</template>

<style scoped>
.admin-shell {
  display: flex;
  min-height: 100vh;
}
.sidebar {
  width: 200px;
  background: #1f2937;
  color: #fff;
  flex-shrink: 0;
}
.brand {
  padding: 20px 16px;
  font-weight: 700;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}
.sidebar nav {
  display: flex;
  flex-direction: column;
  padding: 12px 0;
}
.sidebar nav a {
  padding: 12px 20px;
  color: #cbd5e1;
  font-size: 14px;
}
.sidebar nav a.active {
  background: rgba(255, 255, 255, 0.1);
  color: #fff;
}
.main {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
}
.topbar {
  height: 56px;
  background: #fff;
  border-bottom: 1px solid #ebeef5;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px;
  font-size: 13px;
}
.right {
  display: flex;
  gap: 16px;
  color: #909399;
}
.content {
  flex: 1;
  padding: 24px;
}
</style>
