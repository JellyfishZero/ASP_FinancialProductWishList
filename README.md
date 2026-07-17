# ASP_FinancialProductWishList

ASP_FinancialProductWishList 是使用 ASP.NET Core MVC 製作的金融商品喜好紀錄系統，支援會員註冊與登入、瀏覽金融商品，以及喜好清單的新增、查詢、修改與刪除。

專案採用 SQL Server，所有資料庫存取皆透過 Stored Procedure 執行，並以 Controller、Service、Repository 分層管理責任。

## 開發環境

- .NET 10 SDK
- ASP.NET Core MVC
- SQL Server
- Bootstrap
- Visual Studio 2026、Visual Studio 2022（需支援 .NET 10）或其他可執行 .NET 10 的開發工具

## 建立資料庫

本專案預設使用的資料庫名稱為：

```text
ASP_FinancialProductWishList
```

可以使用 SQL Server Management Studio（SSMS）或其他 SQL Server 管理工具建立資料庫。

### 1. 建立資料庫

先連線至 SQL Server，再執行：

```sql
IF DB_ID(N'ASP_FinancialProductWishList') IS NULL
BEGIN
    CREATE DATABASE ASP_FinancialProductWishList;
END;
GO
```

### 2. 切換至專案資料庫

```sql
USE ASP_FinancialProductWishList;
GO
```

### 3. 執行資料庫腳本

請在 `ASP_FinancialProductWishList` 資料庫中，依照以下順序執行：

1. `DB/01_DDL.sql`
2. `DB/02_DML_ProductSeed.sql`
3. `DB/03_StoredProcedures.sql`

各腳本用途如下：

| 腳本 | 用途 |
| --- | --- |
| `01_DDL.sql` | 建立 `User`、`Product`、`LikeList` 資料表、外鍵、唯一約束、檢查約束及索引 |
| `02_DML_ProductSeed.sql` | 建立 8 筆虛構金融商品種子資料 |
| `03_StoredProcedures.sql` | 建立會員、商品查詢及喜好清單 CRUD 所需的 Stored Procedure |

`01_DDL.sql` 會以 Transaction 建立資料表、約束及索引；若執行失敗，會 Rollback。

> SQL 腳本本身不包含 `USE ASP_FinancialProductWishList`，執行前請確認查詢視窗目前選取的資料庫是 `ASP_FinancialProductWishList`，避免將資料表建立到其他資料庫。

> `01_DDL.sql` 與 `02_DML_ProductSeed.sql` 用於初始化空白資料庫，不應在已完成初始化的資料庫中重複執行。`03_StoredProcedures.sql` 使用 `CREATE OR ALTER PROCEDURE`，需要更新 Stored Procedure 時可以重新執行。

## 設定資料庫連線

預設連線字串位於：

- `appsettings.json`
- `appsettings.Development.json`

目前預設設定：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1;Database=ASP_FinancialProductWishList;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

如果 SQL Server 使用 Windows 驗證，只要依實際環境修改 `Server`：

```text
Server=伺服器名稱;Database=ASP_FinancialProductWishList;Trusted_Connection=True;TrustServerCertificate=True;
```

如果使用 SQL Server 帳號及密碼，請勿將帳號或密碼提交至 Git，應改用 User Secrets 或環境變數保存。

## 執行專案

確認資料庫、商品種子資料及 Stored Procedure 建立完成後，在專案根目錄執行：

```powershell
dotnet restore
dotnet run --launch-profile https
```

預設開發網址為 `https://localhost:7207`。第一次使用時，先進入註冊頁建立帳號，再使用使用者代號與密碼登入。

## 主要功能

- 使用者代號、姓名、Email、預設扣款帳號及密碼註冊
- 使用 Cookie Authentication 與 Claims 保存登入狀態
- 修改使用者的預設扣款帳號
- 瀏覽共用金融商品主檔
- 從商品頁面將商品加入自己的喜好清單
- 新增喜好項目時自動帶入使用者預設扣款帳號
- 每筆喜好項目可以使用不同的預計扣款帳號
- 同一位使用者可以針對同一商品建立多筆喜好紀錄
- 修改喜好項目的商品、數量及預計扣款帳號
- 刪除自己的喜好項目
- 顯示商品金額、手續費及預計扣款總金額
- 使用 Bootstrap 支援響應式頁面
- 使用 Anti-forgery Token 防止 CSRF
- Razor 預設 HTML Encoding 防止輸入內容直接形成 HTML
- 所有 SQL 呼叫皆使用參數及 Stored Procedure

## 專案資料夾結構

```text
ASP_FinancialProductWishList/
├─ Common/
│  ├─ Exceptions/                 共用的業務例外
│  └─ Extensions/                 ClaimsPrincipal 等共用擴充方法
├─ Controllers/                   接收 HTTP Request、驗證頁面輸入及回傳 View
├─ DB/                            SQL Server DDL、DML 與 Stored Procedure 腳本
├─ Models/
│  └─ Entities/                   對應核心資料的 Entity
├─ Repositories/
│  ├─ Implementations/            執行 Stored Procedure 與資料映射
│  └─ Interfaces/                 Repository 介面
├─ Services/
│  ├─ DTOs/                       Service 輸入及輸出資料
│  ├─ Implementations/            業務規則、權限與金額計算
│  └─ Interfaces/                 Service 介面
├─ ViewModels/                    Razor 頁面的輸入及顯示模型
├─ Views/                         Razor Views
├─ wwwroot/
│  ├─ css/                        網站樣式
│  ├─ js/                         前端 JavaScript
│  └─ lib/                        Bootstrap、jQuery 等前端套件
├─ Program.cs                     DI、Cookie Authentication 與 Middleware 設定
├─ appsettings.json               共用應用程式設定
└─ appsettings.Development.json   開發環境設定
```

## 程式分層

主要請求流程如下：

```text
Browser
  → Controller / View
  → Service
  → Repository
  → Stored Procedure
  → SQL Server
```

### Controller 與 View

Controller 負責接收請求、檢查 ModelState、取得登入使用者 Claim，並將結果傳給 View。View 使用 ViewModel 顯示資料，不直接執行資料庫操作或計算金融商品金額。

### Service

Service 負責主要業務規則，例如：

- 建立及驗證會員帳號
- 確認商品是否存在
- 限制使用者只能操作自己的喜好項目
- 驗證購買數量及扣款帳號
- 計算商品金額、手續費與預計扣款總金額
- 將資料庫錯誤轉換成業務例外

金額計算規則如下：

```text
商品金額 = 商品價格 × 購買數量
手續費 = 商品金額 × 手續費率
預計扣款總金額 = 四捨五入後的商品金額 + 四捨五入後的手續費
```

金額以 TWD 顯示至小數點後 2 位，並使用 `MidpointRounding.AwayFromZero` 四捨五入。

### Repository

Repository 負責：

- 建立 SQL Server 連線
- 使用參數呼叫 Stored Procedure
- 將查詢結果映射為 Entity

Repository 不負責頁面顯示，也不包含主要業務規則或金額計算。

### Stored Procedure

資料表的查詢與異動由 `DB/03_StoredProcedures.sql` 中的 Stored Procedure 執行。喜好清單的查詢、修改及刪除會同時使用 `LikeListID` 與登入使用者的 `UserID`，在資料庫層再次限制使用者只能存取自己的資料。

目前各項新增、修改及刪除流程只異動單一資料表；未來若單一業務操作需要同時異動多張資料表，應使用 Transaction 確保資料一致性。

## 主要資料表

| 資料表 | 用途 |
| --- | --- |
| `User` | 使用者代號、姓名、Email、預設扣款帳號及密碼雜湊 |
| `Product` | 共用金融商品名稱、價格及手續費率 |
| `LikeList` | 使用者、商品、購買數量及該筆預計扣款帳號 |

`User.UserName` 與 `User.Email` 具有唯一約束，避免重複註冊。`Product.ProductName` 也具有唯一約束，避免建立重複商品。

`User.DebitAccount` 是使用者的預設扣款帳號；新增喜好項目時會自動帶入，但使用者仍可修改 `LikeList.DebitAccount`，讓不同喜好項目使用不同的預計扣款帳號。
