<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useCartStore } from '../stores/cart'
import { useWishlistStore } from '../stores/wishlist'

const router = useRouter()
const auth = useAuthStore()
const cart = useCartStore()
const wishlist = useWishlistStore()
const keyword = ref('')

onMounted(() => {
  cart.fetch()
  wishlist.fetch()
})

function search() {
  router.push({ name: 'products', query: keyword.value ? { keyword: keyword.value } : {} })
}

function logout() {
  auth.logout()
  cart.clear()
  wishlist.clear()
  router.push('/')
}
</script>

<template>
  <header class="header">
    <div class="container header-inner">
      <router-link to="/" class="logo">EC Site</router-link>

      <div class="search">
        <el-input v-model="keyword" placeholder="商品を検索..." @keyup.enter="search">
          <template #append>
            <el-button :icon="'Search'" @click="search">検索</el-button>
          </template>
        </el-input>
      </div>

      <nav class="nav">
        <router-link to="/products">全商品</router-link>
        <template v-if="auth.isLoggedIn">
          <router-link to="/wishlist" class="cart-link">
            お気に入り
            <el-badge v-if="wishlist.totalCount > 0" :value="wishlist.totalCount" class="cart-badge" />
          </router-link>
        </template>
        <router-link to="/cart" class="cart-link">
          カート
          <el-badge v-if="cart.totalCount > 0" :value="cart.totalCount" class="cart-badge" />
        </router-link>

        <template v-if="auth.isLoggedIn">
          <router-link to="/orders">注文履歴</router-link>
          <router-link v-if="auth.isAdmin" to="/admin">管理画面</router-link>
          <span class="user-name">{{ auth.name }}</span>
          <a href="#" @click.prevent="logout">ログアウト</a>
        </template>
        <template v-else>
          <router-link to="/login">ログイン</router-link>
          <router-link to="/register">新規登録</router-link>
        </template>
      </nav>
    </div>
  </header>
</template>

<style scoped>
.header {
  background: #fff;
  border-bottom: 1px solid #ebeef5;
  position: sticky;
  top: 0;
  z-index: 100;
}
.header-inner {
  display: flex;
  align-items: center;
  gap: 24px;
  height: 64px;
}
.logo {
  font-size: 20px;
  font-weight: 700;
  color: #409eff;
  flex-shrink: 0;
}
.search {
  flex: 1;
  max-width: 420px;
}
.nav {
  display: flex;
  align-items: center;
  gap: 18px;
  font-size: 14px;
  white-space: nowrap;
}
.cart-link {
  position: relative;
}
.cart-badge {
  margin-left: 4px;
}
.user-name {
  color: #909399;
}
</style>
