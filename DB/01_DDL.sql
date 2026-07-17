/*
    金融商品喜好紀錄系統 - SQL Server DDL
    請先切換至目標資料庫，再執行本腳本。
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    CREATE TABLE dbo.[User]
    (
        UserID          BIGINT IDENTITY(1, 1) NOT NULL,
        UserName        NVARCHAR(50) NOT NULL,
        [Name]          NVARCHAR(100) NOT NULL,
        Email           NVARCHAR(254) NOT NULL,
        DebitAccount    VARCHAR(20) NOT NULL,
        PasswordHash    NVARCHAR(500) NOT NULL,

        CONSTRAINT PK_User PRIMARY KEY CLUSTERED (UserID),
        CONSTRAINT UQ_User_UserName UNIQUE (UserName),
        CONSTRAINT UQ_User_Email UNIQUE (Email),
        CONSTRAINT CK_User_UserName_NotBlank
            CHECK (LEN(LTRIM(RTRIM(UserName))) > 0),
        CONSTRAINT CK_User_Name_NotBlank
            CHECK (LEN(LTRIM(RTRIM([Name]))) > 0),
        CONSTRAINT CK_User_Email_NotBlank
            CHECK (LEN(LTRIM(RTRIM(Email))) > 0),
        CONSTRAINT CK_User_DebitAccount
            CHECK
            (
                LEN(DebitAccount) BETWEEN 10 AND 20
                AND DebitAccount NOT LIKE '%[^0-9]%'
            )
    );

    CREATE TABLE dbo.Product
    (
        ProductID      INT IDENTITY(1, 1) NOT NULL,
        ProductName    NVARCHAR(100) NOT NULL,
        Price          DECIMAL(19, 4) NOT NULL,
        FeeRate        DECIMAL(9, 6) NOT NULL,

        CONSTRAINT PK_Product PRIMARY KEY CLUSTERED (ProductID),
        CONSTRAINT UQ_Product_ProductName UNIQUE (ProductName),
        CONSTRAINT CK_Product_ProductName_NotBlank
            CHECK (LEN(LTRIM(RTRIM(ProductName))) > 0),
        CONSTRAINT CK_Product_Price CHECK (Price > 0),
        CONSTRAINT CK_Product_FeeRate CHECK (FeeRate BETWEEN 0 AND 1)
    );

    CREATE TABLE dbo.LikeList
    (
        LikeListID    BIGINT IDENTITY(1, 1) NOT NULL,
        UserID        BIGINT NOT NULL,
        ProductID     INT NOT NULL,
        Quantity      INT NOT NULL,

        CONSTRAINT PK_LikeList PRIMARY KEY CLUSTERED (LikeListID),

        CONSTRAINT FK_LikeList_User FOREIGN KEY (UserID)
            REFERENCES dbo.[User] (UserID),
        CONSTRAINT FK_LikeList_Product FOREIGN KEY (ProductID)
            REFERENCES dbo.Product (ProductID),
        CONSTRAINT CK_LikeList_Quantity CHECK (Quantity > 0)
    );

    CREATE NONCLUSTERED INDEX IX_LikeList_ProductID
        ON dbo.LikeList (ProductID);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;

