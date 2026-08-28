<template>
  <div class="not-found">
    <el-empty description="ページが見つかりません">
      <router-link to="/">
        <el-button type="primary">トップページに戻る</el-button>
      </router-link>
    </el-empty>
  </div>
</template>

<style scoped>
.not-found {
  display: flex;
  justify-content: center;
  padding: 100px 20px;
}
</style>
