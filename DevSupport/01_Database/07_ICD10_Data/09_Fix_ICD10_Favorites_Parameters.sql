-- ========================================
-- ICD-10 FAVORITES - FIX STORED PROCEDURES
-- Database: ValyanMed
-- Descriere: Corectare parametri @UserID -> @PersonalID
-- ========================================

USE [ValyanMed]
GO

PRINT '========================================';
PRINT 'ICD-10 FAVORITES - FIX PARAMETRI';
PRINT '========================================';
PRINT '';

-- ==================== STEP 1: FIX sp_ICD10_AddFavorite ====================
PRINT '1. Corectare sp_ICD10_AddFavorite (UserID -> PersonalID)...';
GO

IF OBJECT_ID('dbo.sp_ICD10_AddFavorite', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ICD10_AddFavorite
GO

CREATE PROCEDURE dbo.sp_ICD10_AddFavorite
    @PersonalID UNIQUEIDENTIFIER,  -- ? FIXED: PersonalID în loc de UserID
    @ICD10_ID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if already exists
    IF EXISTS (SELECT 1 FROM dbo.ICD10_Favorites 
               WHERE PersonalID = @PersonalID AND ICD10_ID = @ICD10_ID)
    BEGIN
        PRINT 'ICD-10 cod este deja în favorite';
        RETURN 0; -- Already favorite
    END
    
    -- Insert new favorite
    INSERT INTO dbo.ICD10_Favorites (PersonalID, ICD10_ID, CreatedAt)
    VALUES (@PersonalID, @ICD10_ID, GETDATE());
    
    RETURN 1; -- Success
END
GO

PRINT '   ? sp_ICD10_AddFavorite corectat';
GO

-- ==================== STEP 2: FIX sp_ICD10_RemoveFavorite ====================
PRINT '2. Corectare sp_ICD10_RemoveFavorite (UserID -> PersonalID)...';
GO

IF OBJECT_ID('dbo.sp_ICD10_RemoveFavorite', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ICD10_RemoveFavorite
GO

CREATE PROCEDURE dbo.sp_ICD10_RemoveFavorite
    @PersonalID UNIQUEIDENTIFIER,  -- ? FIXED: PersonalID în loc de UserID
    @ICD10_ID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM dbo.ICD10_Favorites
    WHERE PersonalID = @PersonalID AND ICD10_ID = @ICD10_ID;
    
    IF @@ROWCOUNT > 0
        RETURN 1; -- Success
    ELSE
        RETURN 0; -- Not found
END
GO

PRINT '   ? sp_ICD10_RemoveFavorite corectat';
GO

-- ==================== STEP 3: FIX sp_ICD10_GetFavorites ====================
PRINT '3. Corectare sp_ICD10_GetFavorites (UserID -> PersonalID)...';
GO

IF OBJECT_ID('dbo.sp_ICD10_GetFavorites', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ICD10_GetFavorites
GO

CREATE PROCEDURE dbo.sp_ICD10_GetFavorites
    @PersonalID UNIQUEIDENTIFIER  -- ? FIXED: PersonalID în loc de UserID
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT
        icd.ICD10_ID,
        icd.ChapterId,
        icd.SectionId,
        icd.ParentId,
        icd.Code,
        icd.FullCode,
        icd.ShortDescriptionRo,
        icd.LongDescriptionRo,
        icd.ShortDescriptionEn,
        icd.LongDescriptionEn,
        icd.ParentCode,
        icd.HierarchyLevel,
        icd.IsLeafNode,
        icd.IsBillable,
        icd.Category,
        icd.IsCommon,
        icd.Severity,
        icd.SearchTermsRo,
        icd.SearchTermsEn,
        icd.IsTranslated,
        icd.TranslatedAt,
        icd.TranslatedBy,
        icd.Version,
        icd.SourceFile,
        icd.IsActive,
        icd.CreatedAt,
        icd.UpdatedAt,
        fav.CreatedAt AS FavoriteAddedAt
    FROM dbo.ICD10_Favorites fav
    INNER JOIN dbo.ICD10_Codes icd ON fav.ICD10_ID = icd.ICD10_ID
    WHERE fav.PersonalID = @PersonalID
    ORDER BY fav.CreatedAt DESC;
END
GO

PRINT '   ? sp_ICD10_GetFavorites corectat';
GO

-- ==================== STEP 4: FIX sp_ICD10_IsFavorite ====================
PRINT '4. Corectare sp_ICD10_IsFavorite (UserID -> PersonalID)...';
GO

IF OBJECT_ID('dbo.sp_ICD10_IsFavorite', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ICD10_IsFavorite
GO

CREATE PROCEDURE dbo.sp_ICD10_IsFavorite
    @PersonalID UNIQUEIDENTIFIER,  -- ? FIXED: PersonalID în loc de UserID
    @ICD10_ID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM dbo.ICD10_Favorites 
               WHERE PersonalID = @PersonalID AND ICD10_ID = @ICD10_ID)
        SELECT 1 AS IsFavorite;
    ELSE
        SELECT 0 AS IsFavorite;
END
GO

PRINT '   ? sp_ICD10_IsFavorite corectat';
GO

-- ==================== VERIFICATION ====================
PRINT '';
PRINT '========================================';
PRINT 'VERIFICARE FINAL?';
PRINT '========================================';

-- Check all procedures exist
SELECT 
    ROUTINE_NAME as ProcedureName,
    CREATED as DateCreated,
    LAST_ALTERED as LastModified
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_TYPE = 'PROCEDURE'
AND ROUTINE_SCHEMA = 'dbo'
AND ROUTINE_NAME LIKE 'sp_ICD10_%Favorite%'
ORDER BY ROUTINE_NAME;

PRINT '';
PRINT '? Corectare complet?! Toate SP-urile folosesc acum @PersonalID.';
PRINT '';
GO
