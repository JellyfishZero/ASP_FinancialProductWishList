/*
    金融商品喜好紀錄系統 - SQL Server DDL
    執行環境：請先切換至目標資料庫，再執行本腳本。
    本腳本不建立資料庫與登入帳號，也不包含任何正式個資。
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.[User]', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.[User]
        (
            UserID              BIGINT IDENTITY(1, 1) NOT NULL,
            UserName            NVARCHAR(50) NOT NULL,
            NormalizedUserName  NVARCHAR(50) NOT NULL,
            DisplayName         NVARCHAR(100) NOT NULL,
            Email               NVARCHAR(254) NOT NULL,
            NormalizedEmail     NVARCHAR(254) NOT NULL,
            DebitAccount        VARCHAR(20) NOT NULL,
            PasswordHash        NVARCHAR(500) NOT NULL,
            IsActive            BIT NOT NULL
                CONSTRAINT DF_User_IsActive DEFAULT (1),
            CreatedAt           DATETIME2(0) NOT NULL
                CONSTRAINT DF_User_CreatedAt DEFAULT (SYSUTCDATETIME()),
            UpdatedAt           DATETIME2(0) NOT NULL
                CONSTRAINT DF_User_UpdatedAt DEFAULT (SYSUTCDATETIME()),
            RowVersion          ROWVERSION NOT NULL,

            CONSTRAINT PK_User PRIMARY KEY CLUSTERED (UserID),
            CONSTRAINT UQ_User_NormalizedUserName UNIQUE (NormalizedUserName),
            CONSTRAINT UQ_User_NormalizedEmail UNIQUE (NormalizedEmail),
            CONSTRAINT CK_User_UserName_NotBlank
                CHECK (LEN(LTRIM(RTRIM(UserName))) > 0),
            CONSTRAINT CK_User_NormalizedUserName_NotBlank
                CHECK (LEN(LTRIM(RTRIM(NormalizedUserName))) > 0),
            CONSTRAINT CK_User_DisplayName_NotBlank
                CHECK (LEN(LTRIM(RTRIM(DisplayName))) > 0),
            CONSTRAINT CK_User_Email_NotBlank
                CHECK (LEN(LTRIM(RTRIM(Email))) > 0),
            CONSTRAINT CK_User_NormalizedEmail_NotBlank
                CHECK (LEN(LTRIM(RTRIM(NormalizedEmail))) > 0),
            CONSTRAINT CK_User_DebitAccount
                CHECK (
                    LEN(DebitAccount) BETWEEN 10 AND 20
                    AND DebitAccount NOT LIKE '%[^0-9]%'
                ),
            CONSTRAINT CK_User_UpdatedAt
                CHECK (UpdatedAt >= CreatedAt)
        );
    END;

    IF OBJECT_ID(N'dbo.Product', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Product
        (
            ProductID       INT IDENTITY(1, 1) NOT NULL,
            ProductCode     VARCHAR(30) NOT NULL,
            ProductName     NVARCHAR(100) NOT NULL,
            Price           DECIMAL(19, 4) NOT NULL,
            FeeRate         DECIMAL(9, 6) NOT NULL,
            IsActive        BIT NOT NULL
                CONSTRAINT DF_Product_IsActive DEFAULT (1),
            CreatedAt       DATETIME2(0) NOT NULL
                CONSTRAINT DF_Product_CreatedAt DEFAULT (SYSUTCDATETIME()),
            UpdatedAt       DATETIME2(0) NOT NULL
                CONSTRAINT DF_Product_UpdatedAt DEFAULT (SYSUTCDATETIME()),
            RowVersion      ROWVERSION NOT NULL,

            CONSTRAINT PK_Product PRIMARY KEY CLUSTERED (ProductID),
            CONSTRAINT UQ_Product_ProductCode UNIQUE (ProductCode),
            CONSTRAINT CK_Product_ProductCode_NotBlank
                CHECK (LEN(LTRIM(RTRIM(ProductCode))) > 0),
            CONSTRAINT CK_Product_ProductName_NotBlank
                CHECK (LEN(LTRIM(RTRIM(ProductName))) > 0),
            CONSTRAINT CK_Product_Price CHECK (Price > 0),
            CONSTRAINT CK_Product_FeeRate CHECK (FeeRate BETWEEN 0 AND 1),
            CONSTRAINT CK_Product_UpdatedAt
                CHECK (UpdatedAt >= CreatedAt)
        );
    END;

    IF OBJECT_ID(N'dbo.LikeList', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.LikeList
        (
            LikeListID      BIGINT IDENTITY(1, 1) NOT NULL,
            UserID          BIGINT NOT NULL,
            ProductID       INT NOT NULL,
            Quantity        INT NOT NULL,
            CreatedAt       DATETIME2(0) NOT NULL
                CONSTRAINT DF_LikeList_CreatedAt DEFAULT (SYSUTCDATETIME()),
            UpdatedAt       DATETIME2(0) NOT NULL
                CONSTRAINT DF_LikeList_UpdatedAt DEFAULT (SYSUTCDATETIME()),
            RowVersion      ROWVERSION NOT NULL,

            CONSTRAINT PK_LikeList PRIMARY KEY CLUSTERED (LikeListID),
            CONSTRAINT UQ_LikeList_User_Product UNIQUE (UserID, ProductID),
            CONSTRAINT FK_LikeList_User FOREIGN KEY (UserID)
                REFERENCES dbo.[User] (UserID),
            CONSTRAINT FK_LikeList_Product FOREIGN KEY (ProductID)
                REFERENCES dbo.Product (ProductID),
            CONSTRAINT CK_LikeList_Quantity CHECK (Quantity > 0),
            CONSTRAINT CK_LikeList_UpdatedAt
                CHECK (UpdatedAt >= CreatedAt)
        );

        CREATE NONCLUSTERED INDEX IX_LikeList_ProductID
            ON dbo.LikeList (ProductID)
            INCLUDE (UserID, Quantity);
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;

