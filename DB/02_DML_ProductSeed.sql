/*
    金融商品喜好紀錄系統 - Product 開發種子資料
    商品名稱與價格均為本專案展示用途的虛構資料，不代表真實報價或投資建議。
    FeeRate 儲存為比例值，例如 0.001500 = 0.15%。
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.Product', N'U') IS NULL
    THROW 50001, N'找不到 dbo.Product，請先執行 DB/01_DDL.sql。', 1;

DECLARE @SeedProduct TABLE
(
    ProductCode VARCHAR(30) NOT NULL PRIMARY KEY,
    ProductName NVARCHAR(100) NOT NULL,
    Price       DECIMAL(19, 4) NOT NULL,
    FeeRate     DECIMAL(9, 6) NOT NULL,
    IsActive    BIT NOT NULL
);

INSERT INTO @SeedProduct (ProductCode, ProductName, Price, FeeRate, IsActive)
VALUES
    ('FUND-TW-001', N'穩健收益平衡基金',       15.6800, 0.015000, 1),
    ('FUND-GL-002', N'全球科技成長基金',       32.4500, 0.018000, 1),
    ('ETF-TW-003',  N'臺灣大型股指數 ETF',     48.7200, 0.001500, 1),
    ('ETF-GL-004',  N'全球高股息指數 ETF',     26.3500, 0.002000, 1),
    ('BOND-TW-005', N'新臺幣優質債券',     100000.0000, 0.003000, 1),
    ('BOND-US-006', N'美元投資級公司債',     32000.0000, 0.004000, 1),
    ('DEPOSIT-007', N'一年期新臺幣定期存款',  10000.0000, 0.000000, 1),
    ('FUND-ESG-008',N'永續發展多重資產基金',     21.3600, 0.012000, 1);

BEGIN TRY
    BEGIN TRANSACTION;

    UPDATE target
       SET target.ProductName = source.ProductName,
           target.Price       = source.Price,
           target.FeeRate     = source.FeeRate,
           target.IsActive    = source.IsActive,
           target.UpdatedAt   = SYSUTCDATETIME()
    FROM dbo.Product AS target
    INNER JOIN @SeedProduct AS source
        ON source.ProductCode = target.ProductCode
    WHERE target.ProductName <> source.ProductName
       OR target.Price       <> source.Price
       OR target.FeeRate     <> source.FeeRate
       OR target.IsActive    <> source.IsActive;

    INSERT INTO dbo.Product
    (
        ProductCode,
        ProductName,
        Price,
        FeeRate,
        IsActive
    )
    SELECT
        source.ProductCode,
        source.ProductName,
        source.Price,
        source.FeeRate,
        source.IsActive
    FROM @SeedProduct AS source
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Product AS target
        WHERE target.ProductCode = source.ProductCode
    );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;

