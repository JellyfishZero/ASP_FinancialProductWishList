/*
    金融商品喜好紀錄系統 - SQL Server Stored Procedures

    執行順序：
      1. 01_DDL.sql
      2. 02_DML_ProductSeed.sql
      3. 03_StoredProcedures.sql

    原則：
      - 所有輸入皆使用參數，不組合動態 SQL。
      - LikeList 的單筆查詢、修改及刪除皆以 UserID 限制資料所有權。
      - 金額與手續費由應用程式 Service 計算；查詢只回傳必要原始資料。
      - 錯誤編號 51000 至 51099 保留給本系統的業務規則錯誤。
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_ExistsByUserNameOrEmail
    @NormalizedUserName NVARCHAR(50),
    @NormalizedEmail    NVARCHAR(254)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CONVERT(BIT, CASE WHEN EXISTS
        (
            SELECT 1
            FROM dbo.[User]
            WHERE NormalizedUserName = @NormalizedUserName
        ) THEN 1 ELSE 0 END) AS UserNameExists,
        CONVERT(BIT, CASE WHEN EXISTS
        (
            SELECT 1
            FROM dbo.[User]
            WHERE NormalizedEmail = @NormalizedEmail
        ) THEN 1 ELSE 0 END) AS EmailExists;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_Create
    @UserName            NVARCHAR(50),
    @NormalizedUserName  NVARCHAR(50),
    @DisplayName         NVARCHAR(100),
    @Email               NVARCHAR(254),
    @NormalizedEmail     NVARCHAR(254),
    @DebitAccount        VARCHAR(20),
    @PasswordHash        NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        INSERT INTO dbo.[User]
        (
            UserName,
            NormalizedUserName,
            DisplayName,
            Email,
            NormalizedEmail,
            DebitAccount,
            PasswordHash
        )
        VALUES
        (
            @UserName,
            @NormalizedUserName,
            @DisplayName,
            @Email,
            @NormalizedEmail,
            @DebitAccount,
            @PasswordHash
        );

        DECLARE @UserID BIGINT = CONVERT(BIGINT, SCOPE_IDENTITY());

        SELECT
            UserID,
            UserName,
            DisplayName,
            Email,
            DebitAccount,
            IsActive,
            CreatedAt,
            UpdatedAt,
            RowVersion
        FROM dbo.[User]
        WHERE UserID = @UserID;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() IN (2601, 2627)
            THROW 51001, N'使用者代號或 Email 已被使用。', 1;

        THROW;
    END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_GetByNormalizedUserName
    @NormalizedUserName NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- 僅供登入驗證使用，因此此 SP 會回傳 PasswordHash。
    SELECT TOP (1)
        UserID,
        UserName,
        NormalizedUserName,
        DisplayName,
        Email,
        DebitAccount,
        PasswordHash,
        IsActive,
        RowVersion
    FROM dbo.[User]
    WHERE NormalizedUserName = @NormalizedUserName;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_GetById
    @UserID BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        UserID,
        UserName,
        DisplayName,
        Email,
        DebitAccount,
        IsActive,
        CreatedAt,
        UpdatedAt,
        RowVersion
    FROM dbo.[User]
    WHERE UserID = @UserID;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_UpdateDebitAccount
    @UserID              BIGINT,
    @DebitAccount        VARCHAR(20),
    @ExpectedRowVersion  BINARY(8)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    UPDATE dbo.[User]
       SET DebitAccount = @DebitAccount,
           UpdatedAt    = SYSUTCDATETIME()
    WHERE UserID = @UserID
      AND IsActive = 1
      AND RowVersion = @ExpectedRowVersion;

    IF @@ROWCOUNT = 0
    BEGIN
        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.[User]
            WHERE UserID = @UserID
              AND IsActive = 1
        )
            THROW 51002, N'找不到可更新的使用者。', 1;

        THROW 51003, N'使用者資料已被其他操作更新，請重新載入後再試。', 1;
    END;

    SELECT
        UserID,
        UserName,
        DisplayName,
        Email,
        DebitAccount,
        IsActive,
        CreatedAt,
        UpdatedAt,
        RowVersion
    FROM dbo.[User]
    WHERE UserID = @UserID;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Product_GetActiveList
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ProductID,
        ProductCode,
        ProductName,
        Price,
        FeeRate,
        RowVersion
    FROM dbo.Product
    WHERE IsActive = 1
    ORDER BY ProductName, ProductID;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Product_GetById
    @ProductID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ProductID,
        ProductCode,
        ProductName,
        Price,
        FeeRate,
        IsActive,
        CreatedAt,
        UpdatedAt,
        RowVersion
    FROM dbo.Product
    WHERE ProductID = @ProductID;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_LikeList_GetByUserId
    @UserID BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        wish.LikeListID,
        wish.UserID,
        wish.ProductID,
        product.ProductCode,
        product.ProductName,
        product.Price,
        product.FeeRate,
        wish.Quantity,
        member.DebitAccount,
        member.Email,
        wish.CreatedAt,
        wish.UpdatedAt,
        wish.RowVersion
    FROM dbo.LikeList AS wish
    INNER JOIN dbo.Product AS product
        ON product.ProductID = wish.ProductID
    INNER JOIN dbo.[User] AS member
        ON member.UserID = wish.UserID
    WHERE wish.UserID = @UserID
      AND member.IsActive = 1
    ORDER BY wish.CreatedAt DESC, wish.LikeListID DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_LikeList_GetByIdAndUserId
    @LikeListID BIGINT,
    @UserID     BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        wish.LikeListID,
        wish.UserID,
        wish.ProductID,
        product.ProductCode,
        product.ProductName,
        product.Price,
        product.FeeRate,
        product.IsActive AS ProductIsActive,
        wish.Quantity,
        member.DebitAccount,
        member.Email,
        wish.CreatedAt,
        wish.UpdatedAt,
        wish.RowVersion
    FROM dbo.LikeList AS wish
    INNER JOIN dbo.Product AS product
        ON product.ProductID = wish.ProductID
    INNER JOIN dbo.[User] AS member
        ON member.UserID = wish.UserID
    WHERE wish.LikeListID = @LikeListID
      AND wish.UserID = @UserID
      AND member.IsActive = 1;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_LikeList_Create
    @UserID     BIGINT,
    @ProductID  INT,
    @Quantity   INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @Quantity <= 0
        THROW 51010, N'購買數量必須大於 0。', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.[User]
        WHERE UserID = @UserID
          AND IsActive = 1
    )
        THROW 51011, N'找不到有效的使用者。', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Product
        WHERE ProductID = @ProductID
          AND IsActive = 1
    )
        THROW 51012, N'找不到可選擇的金融商品。', 1;

    BEGIN TRY
        INSERT INTO dbo.LikeList (UserID, ProductID, Quantity)
        VALUES (@UserID, @ProductID, @Quantity);

        DECLARE @LikeListID BIGINT = CONVERT(BIGINT, SCOPE_IDENTITY());

        EXEC dbo.usp_LikeList_GetByIdAndUserId
            @LikeListID = @LikeListID,
            @UserID = @UserID;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() IN (2601, 2627)
            THROW 51013, N'此金融商品已存在於喜好清單。', 1;

        THROW;
    END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_LikeList_Update
    @LikeListID         BIGINT,
    @UserID             BIGINT,
    @ProductID          INT,
    @Quantity           INT,
    @ExpectedRowVersion BINARY(8)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @Quantity <= 0
        THROW 51010, N'購買數量必須大於 0。', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Product
        WHERE ProductID = @ProductID
          AND IsActive = 1
    )
        THROW 51012, N'找不到可選擇的金融商品。', 1;

    BEGIN TRY
        UPDATE dbo.LikeList
           SET ProductID = @ProductID,
               Quantity  = @Quantity,
               UpdatedAt = SYSUTCDATETIME()
        WHERE LikeListID = @LikeListID
          AND UserID = @UserID
          AND RowVersion = @ExpectedRowVersion;

        IF @@ROWCOUNT = 0
        BEGIN
            IF NOT EXISTS
            (
                SELECT 1
                FROM dbo.LikeList
                WHERE LikeListID = @LikeListID
                  AND UserID = @UserID
            )
                THROW 51014, N'找不到可修改的喜好項目。', 1;

            THROW 51015, N'喜好項目已被其他操作更新，請重新載入後再試。', 1;
        END;

        EXEC dbo.usp_LikeList_GetByIdAndUserId
            @LikeListID = @LikeListID,
            @UserID = @UserID;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() IN (2601, 2627)
            THROW 51013, N'此金融商品已存在於喜好清單。', 1;

        THROW;
    END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_LikeList_Delete
    @LikeListID         BIGINT,
    @UserID             BIGINT,
    @ExpectedRowVersion BINARY(8)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DELETE FROM dbo.LikeList
    WHERE LikeListID = @LikeListID
      AND UserID = @UserID
      AND RowVersion = @ExpectedRowVersion;

    IF @@ROWCOUNT = 0
    BEGIN
        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.LikeList
            WHERE LikeListID = @LikeListID
              AND UserID = @UserID
        )
            THROW 51014, N'找不到可刪除的喜好項目。', 1;

        THROW 51015, N'喜好項目已被其他操作更新，請重新載入後再試。', 1;
    END;

    SELECT CONVERT(BIT, 1) AS IsDeleted;
END;
GO

