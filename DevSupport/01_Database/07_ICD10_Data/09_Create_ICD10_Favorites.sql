-- ========================================
-- ICD-10 FAVORITES - TABLE & STORED PROCEDURES
-- Database: ValyanMed
-- Descriere: Management favorite ICD-10 per utilizator
-- ========================================

USE [ValyanMed]
GO

PRINT '========================================';
PRINT 'ICD-10 FAVORITES - TABLE & SPs SETUP';
PRINT '========================================';
PRINT '';

-- ==================== STEP 1: CREATE TABLE ICD10_Favorites ====================
PRINT '1. Creare tabel ICD10_Favorites...';

IF OBJECT_ID('dbo.ICD10_Favorites', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.ICD10_Favorites;
    PRINT '   ? Tabel ICD10_Favorites ?ters (exista deja)';
END
GO

CREATE TABLE dbo.ICD10_Favorites (
    FavoriteID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    UserID UNIQUEIDENTIFIER NOT NULL,     -- FK to Utilizatori
    ICD10_ID UNIQUEIDENTIFIER NOT NULL, -- FK to ICD10_Codes
    DataAdaugare DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT PK_ICD10_Favorites PRIMARY KEY (FavoriteID),
    CONSTRAINT UQ_ICD10_Favorites_User_Code UNIQUE (UserID, ICD10_ID),
    CONSTRAINT FK_ICD10_Favorites_ICD10 FOREIGN KEY (ICD10_ID) 
REFERENCES dbo.ICD10_Codes(ICD10_ID) ON DELETE CASCADE
)
GO

CREATE NONCLUSTERED INDEX IX_ICD10_Favorites_UserID 
    ON dbo.ICD10_Favorites (UserID, DataAdaugare DESC)
GO

PRINT '   ? Tabel ICD10_Favorites creat cu succes';
PRINT '';

-- ==================== STEP 2: sp_ICD10_AddFavorite ====================
PRINT '2. Creare sp_ICD10_AddFavorite...';
GO

IF OBJECT_ID('dbo.sp_ICD10_AddFavorite', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ICD10_AddFavorite
GO

CREATE PROCEDURE dbo.sp_ICD10_AddFavorite
    @UserID UNIQUEIDENTIFIER,
  @ICD10_ID UNIQUEIDENTIFIER
AS
BEGIN
SET NOCOUNT ON;
    
    -- Check if already exists
    IF EXISTS (SELECT 1 FROM dbo.ICD10_Favorites 
   WHERE UserID = @UserID AND ICD10_ID = @ICD10_ID)
    BEGIN
        PRINT 'ICD-10 cod este deja în favorite';
        RETURN 0; -- Already favorite
    END
    
    -- Insert new favorite
    INSERT INTO dbo.ICD10_Favorites (UserID, ICD10_ID, DataAdaugare)
  VALUES (@UserID, @ICD10_ID, GETDATE());
    
    RETURN 1; -- Success
END
GO

PRINT '   ? sp_ICD10_AddFavorite creat';
GO

-- ==================== STEP 3: sp_ICD10_RemoveFavorite ====================
PRINT '3. Creare sp_ICD10_RemoveFavorite...';
GO

IF OBJECT_ID('dbo.sp_ICD10_RemoveFavorite', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ICD10_RemoveFavorite
GO

CREATE PROCEDURE dbo.sp_ICD10_RemoveFavorite
    @UserID UNIQUEIDENTIFIER,
    @ICD10_ID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM dbo.ICD10_Favorites
    WHERE UserID = @UserID AND ICD10_ID = @ICD10_ID;
    
    IF @@ROWCOUNT > 0
        RETURN 1; -- Success
    ELSE
        RETURN 0; -- Not found
END
GO

PRINT '   ? sp_ICD10_RemoveFavorite creat';
GO

-- ==================== STEP 4: sp_ICD10_GetFavorites ====================
PRINT '4. Creare sp_ICD10_GetFavorites...';
GO

IF OBJECT_ID('dbo.sp_ICD10_GetFavorites', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ICD10_GetFavorites
GO

CREATE PROCEDURE dbo.sp_ICD10_GetFavorites
    @UserID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
  SELECT
        icd.ICD10_ID,
        icd.Code,
        icd.FullCode,
        icd.Category,
        icd.ShortDescription,
     icd.LongDescription,
        icd.EnglishDescription,
        icd.ParentCode,
     icd.IsLeafNode,
        icd.IsCommon,
        icd.Severity,
        icd.SearchTerms,
    icd.Notes,
        icd.DataCreare,
        icd.DataModificare,
        fav.DataAdaugare AS FavoriteAddedAt
    FROM dbo.ICD10_Favorites fav
 INNER JOIN dbo.ICD10_Codes icd ON fav.ICD10_ID = icd.ICD10_ID
    WHERE fav.UserID = @UserID
    ORDER BY fav.DataAdaugare DESC;
END
GO

PRINT '   ? sp_ICD10_GetFavorites creat';
GO

-- ==================== STEP 5: sp_ICD10_IsFavorite ====================
PRINT '5. Creare sp_ICD10_IsFavorite (helper)...';
GO

IF OBJECT_ID('dbo.sp_ICD10_IsFavorite', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ICD10_IsFavorite
GO

CREATE PROCEDURE dbo.sp_ICD10_IsFavorite
    @UserID UNIQUEIDENTIFIER,
    @ICD10_ID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM dbo.ICD10_Favorites 
     WHERE UserID = @UserID AND ICD10_ID = @ICD10_ID)
      SELECT 1 AS IsFavorite;
    ELSE
 SELECT 0 AS IsFavorite;
END
GO

PRINT '   ? sp_ICD10_IsFavorite creat';
GO

-- ==================== STEP 6: Verification ====================
PRINT '';
PRINT '========================================';
PRINT 'VERIFICARE FINAL?';
PRINT '========================================';

-- Count procedures
SELECT COUNT(*) as TotalFavoriteProcedures
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_TYPE = 'PROCEDURE'
AND ROUTINE_SCHEMA = 'dbo'
AND ROUTINE_NAME LIKE 'sp_ICD10_%Favorite%';

-- List procedures
SELECT 
ROUTINE_NAME as ProcedureName,
    CREATED as DateCreated
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_TYPE = 'PROCEDURE'
AND ROUTINE_SCHEMA = 'dbo'
AND ROUTINE_NAME LIKE 'sp_ICD10_%Favorite%'
ORDER BY ROUTINE_NAME;

-- Check table exists
IF OBJECT_ID('dbo.ICD10_Favorites', 'U') IS NOT NULL
    PRINT '? Tabel ICD10_Favorites: EXISTS';
ELSE
    PRINT '? Tabel ICD10_Favorites: MISSING';

PRINT '';
PRINT '? Setup complet! ICD-10 Favorites tabel ?i SP-uri create.';
PRINT '';
GO
