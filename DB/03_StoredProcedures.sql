/*
    金融商品喜好紀錄系統 - SQL Server Stored Procedures
    請依序執行 01_DDL.sql、02_DML_ProductSeed.sql、03_StoredProcedures.sql。
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_ExistsByUserNameOrEmail
    @UserName NVARCHAR(50),
    @Email    NVARCHAR(254)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CONVERT(BIT, CASE WHEN EXISTS
        (
            SELECT 1 FROM dbo.[User] WHERE UserName = @UserName
        ) THEN 1 ELSE 0 END) AS UserNameExists,
        CONVERT(BIT, CASE WHEN EXISTS
        (
            SELECT 1 FROM dbo.[User] WHERE Email = @Email
        ) THEN 1 ELSE 0 END) AS EmailExists;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_Create
    @UserName       NVARCHAR(50),
    @Name           NVARCHAR(100),
    @Email          NVARCHAR(254),
    @DebitAccount   VARCHAR(20),
    @PasswordHash   NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        INSERT INTO dbo.[User]
            (UserName, [Name], Email, DebitAccount, PasswordHash)
        VALUES
            (@UserName, @Name, @Email, @DebitAccount, @PasswordHash);

        DECLARE @UserID BIGINT = CONVERT(BIGINT, SCOPE_IDENTITY());

        SELECT UserID, UserName, [Name], Email, DebitAccount
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

CREATE OR ALTER PROCEDURE dbo.usp_User_GetByUserName
    @UserName NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- 僅供登入驗證使用，因此會回傳 PasswordHash。
    SELECT TOP (1)
        UserID, UserName, [Name], Email, DebitAccount, PasswordHash
    FROM dbo.[User]
    WHERE UserName = @UserName;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_GetById
    @UserID BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT UserID, UserName, [Name], Email, DebitAccount
    FROM dbo.[User]
    WHERE UserID = @UserID;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_User_UpdateDebitAccount
    @UserID         BIGINT,
    @DebitAccount   VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.[User]
    SET DebitAccount = @DebitAccount
    WHERE UserID = @UserID;

    IF @@ROWCOUNT = 0
        THROW 51002, N'找不到可更新的使用者。', 1;

    SELECT UserID, UserName, [Name], Email, DebitAccount
    FROM dbo.[User]
    WHERE UserID = @UserID;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Product_GetList
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ProductID, ProductName, Price, FeeRate
    FROM dbo.Product
    ORDER BY ProductName, ProductID;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Product_GetById
    @ProductID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ProductID, ProductName, Price, FeeRate
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
        product.ProductName,
        product.Price,
        product.FeeRate,
        wish.Quantity,
        wish.DebitAccount,
        member.Email
    FROM dbo.LikeList AS wish
    INNER JOIN dbo.Product AS product
        ON product.ProductID = wish.ProductID
    INNER JOIN dbo.[User] AS member
        ON member.UserID = wish.UserID
    WHERE wish.UserID = @UserID
    ORDER BY wish.LikeListID DESC;
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
        product.ProductName,
        product.Price,
        product.FeeRate,
        wish.Quantity,
        wish.DebitAccount,
        member.Email
    FROM dbo.LikeList AS wish
    INNER JOIN dbo.Product AS product
        ON product.ProductID = wish.ProductID
    INNER JOIN dbo.[User] AS member
        ON member.UserID = wish.UserID
    WHERE wish.LikeListID = @LikeListID
      AND wish.UserID = @UserID;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_LikeList_Create
    @UserID         BIGINT,
    @ProductID      INT,
    @DebitAccount   VARCHAR(20),
    @Quantity       INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Quantity <= 0
        THROW 51010, N'購買數量必須大於 0。', 1;

    IF @DebitAccount IS NULL
       OR LEN(@DebitAccount) NOT BETWEEN 10 AND 20
       OR @DebitAccount LIKE '%[^0-9]%'
        THROW 51014, N'扣款帳號須為 10 至 20 位數字。', 1;

    BEGIN TRY
        INSERT INTO dbo.LikeList
        (
            UserID,
            ProductID,
            DebitAccount,
            Quantity
        )
        VALUES
        (
            @UserID,
            @ProductID,
            @DebitAccount,
            @Quantity
        );

        DECLARE @LikeListID BIGINT = CONVERT(BIGINT, SCOPE_IDENTITY());

        EXEC dbo.usp_LikeList_GetByIdAndUserId
            @LikeListID = @LikeListID,
            @UserID = @UserID;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() = 547
            THROW 51012, N'使用者或金融商品不存在。', 1;

        THROW;
    END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_LikeList_Update
    @LikeListID     BIGINT,
    @UserID         BIGINT,
    @ProductID      INT,
    @DebitAccount   VARCHAR(20),
    @Quantity       INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Quantity <= 0
        THROW 51010, N'購買數量必須大於 0。', 1;

    IF @DebitAccount IS NULL
       OR LEN(@DebitAccount) NOT BETWEEN 10 AND 20
       OR @DebitAccount LIKE '%[^0-9]%'
        THROW 51014, N'扣款帳號須為 10 至 20 位數字。', 1;

    BEGIN TRY
        UPDATE dbo.LikeList
        SET ProductID = @ProductID,
            DebitAccount = @DebitAccount,
            Quantity = @Quantity
        WHERE LikeListID = @LikeListID
          AND UserID = @UserID;

        IF @@ROWCOUNT = 0
            THROW 51013, N'找不到可修改的喜好項目。', 1;

        EXEC dbo.usp_LikeList_GetByIdAndUserId
            @LikeListID = @LikeListID,
            @UserID = @UserID;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() = 547
            THROW 51012, N'金融商品不存在。', 1;

        THROW;
    END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_LikeList_Delete
    @LikeListID BIGINT,
    @UserID     BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.LikeList
    WHERE LikeListID = @LikeListID
      AND UserID = @UserID;

    IF @@ROWCOUNT = 0
        THROW 51013, N'找不到可刪除的喜好項目。', 1;

    SELECT CONVERT(BIT, 1) AS IsDeleted;
END;
GO

