# EC Site

研修の実践課題として作成したECサイトです。技術スタック：C# (ASP.NET Core Web API) + SQL Server + Vue 3 (TypeScript)。

## 技術スタック

- バックエンド：ASP.NET Core 10 Web API、Entity Framework Core、JWT認証、BCryptパスワードハッシュ、MailKit（メール通知）
- フロントエンド：Vue 3 + TypeScript + Vite、Pinia、Vue Router、Element Plus
- データベース：SQL Server 2022
- メールテスト：MailHog（送信メールをすべて捕捉し、実際には送信しない）
- コンテナ化：Docker Compose で全サービスをワンコマンド起動

## 機能

- 顧客向け：商品の閲覧・検索・並び替え・価格帯絞り込み、商品詳細とレビュー、関連商品、お気に入り、商品規格（カラー・サイズ）選択、カート、配送先住所管理、注文・決済、クーポン、会員ポイント、模擬決済、注文照会・キャンセル
- 管理画面：データ概要（売上推移・注文ステータス内訳・カテゴリー別売上をECharts で可視化）、商品管理（画像アップロード・規格管理・カテゴリー管理を含む）、注文管理（発送・完了ステータス遷移、完了時にポイント自動付与）、クーポン管理
- 注文成立時・注文ステータス変更時にMailHog経由で通知メールを送信（http://localhost:8025 で確認可能）
- 会員ポイント：購入金額¥10ごとに1ポイント付与（注文完了時）、100ポイント＝¥10として注文時に利用可能

## クイックスタート（Docker、推奨）

```bash
docker compose up -d --build
```

初回起動には数分かかります（イメージのビルド + SQL Serverの初期化 + データベースマイグレーションとシードデータの自動実行）。

起動完了後：

| URL | 内容 |
|---|---|
| http://localhost | フロントエンドサイト |
| http://localhost/swagger | バックエンドAPIドキュメント |
| http://localhost:8025 | MailHog メールテスト画面 |

管理者テストアカウント：`admin@ec-site.local` / `Admin@123`
顧客テストアカウント：`customer@ec-site.local` / `Customer@123`（保有ポイント500pt）

サービスの停止：

```bash
docker compose down
```

データベースをリセットして最初からやり直す：

```bash
docker compose down -v
```

## ローカル開発（Dockerを使わない場合）

### バックエンド

```bash
cd backend
dotnet run
```

ローカルにSQL Serverが必要です（`docker run` で単体起動可能）。実際の環境に合わせて `appsettings.Development.json` の接続文字列を修正してください。

### フロントエンド

```bash
cd frontend
npm install
npm run dev
```

デフォルトではViteのdevプロキシにより `/api` が `http://localhost:5000`（バックエンドの `dotnet run` のデフォルトポート）に転送されます。

## テスト

`backend.Tests/` に xUnit のユニットテストがあります（SQLiteインメモリDBを使用し、EF Coreのトランザクション処理も含めて検証）。

```bash
dotnet test backend.Tests/EcSite.Api.Tests.csproj
```

主に注文作成のビジネスロジック（クーポン割引・ポイント利用/上限・在庫チェック・規格ごとの在庫管理）とJWTトークン発行を検証しています。

`main`/`master` への push・PR時には GitHub Actions（`.github/workflows/ci.yml`）でバックエンドのビルド・テストとフロントエンドの型チェック・ビルドが自動実行されます。

## プロジェクト構成

```
ec-site/
├── .github/workflows/  # GitHub Actions CI
├── docker-compose.yml
├── backend/            # ASP.NET Core Web API
│   ├── Controllers/
│   ├── Controllers/Admin/
│   ├── Models/
│   ├── Data/           # DbContext + マイグレーション + シードデータ
│   ├── Services/       # JWT / メール / 注文のビジネスロジック
│   └── DTOs/
├── backend.Tests/      # xUnit ユニットテスト
└── frontend/           # Vue 3
    └── src/
        ├── views/
        ├── views/admin/
        ├── stores/      # Pinia：auth、cart、wishlist
        ├── api/
        └── components/
```
