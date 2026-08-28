<template>
  <div class="not-found">
    <el-empty description="页面不存在">
      <router-link to="/">
        <el-button type="primary">返回首页</el-button>
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
