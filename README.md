# EC Site

一个用于研修练手的电商网站，技术栈：C# (ASP.NET Core Web API) + SQL Server + Vue 3 (TypeScript)。

## 技术栈

- 后端：ASP.NET Core 10 Web API、Entity Framework Core、JWT 鉴权、BCrypt 密码哈希、MailKit（邮件通知）
- 前端：Vue 3 + TypeScript + Vite、Pinia、Vue Router、Element Plus
- 数据库：SQL Server 2022
- 邮件测试：MailHog（拦截所有发出的邮件，不会真的发送）
- 容器化：Docker Compose 一条命令启动全部服务

## 功能

- 顾客端：商品浏览/搜索/分类筛选、商品详情与评价、购物车、收货地址管理、下单结算、优惠券、模拟支付、订单查询/取消
- 后台管理：数据概览、商品管理（含图片上传、分类管理）、订单管理（发货/完成状态流转）、优惠券管理
- 下单成功、订单状态变更时会通过 MailHog 发送通知邮件（可在 http://localhost:8025 查看）

## 快速启动（Docker，推荐）

```bash
docker compose up -d --build
```

首次启动需要几分钟（构建镜像 + SQL Server 初始化 + 自动执行数据库迁移与种子数据）。

启动完成后：

| 地址 | 说明 |
|---|---|
| http://localhost | 前端网站 |
| http://localhost/swagger | 后端 API 文档 |
| http://localhost:8025 | MailHog 邮件测试界面 |

管理员测试账号：`admin@ec-site.local` / `Admin@123`

停止服务：

```bash
docker compose down
```

清空数据库重新开始：

```bash
docker compose down -v
```

## 本地开发（不使用 Docker）

### 后端

```bash
cd backend
dotnet run
```

需要本地有 SQL Server（可用 `docker run` 单独起一个），并根据实际情况修改 `appsettings.Development.json` 中的连接字符串。

### 前端

```bash
cd frontend
npm install
npm run dev
```

默认通过 Vite 的 dev proxy 将 `/api` 转发到 `http://localhost:5000`（对应后端 `dotnet run` 的默认端口）。

## 项目结构

```
ec-site/
├── docker-compose.yml
├── backend/            # ASP.NET Core Web API
│   ├── Controllers/
│   ├── Controllers/Admin/
│   ├── Models/
│   ├── Data/           # DbContext + Migrations + 种子数据
│   ├── Services/       # JWT / 邮件 / 下单业务逻辑
│   └── DTOs/
└── frontend/           # Vue 3
    └── src/
        ├── views/
        ├── views/admin/
        ├── stores/      # Pinia：auth、cart
        ├── api/
        └── components/
```
