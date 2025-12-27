-- ========================================
-- Stored Procedures: Roluri si Permisiuni
-- Database: ValyanMed
-- Descriere: CRUD pentru Policy-Based Authorization
-- Generat: 2025-12-25
-- ========================================

USE [ValyanMed]
GO

-- ========================================
-- 1. sp_Roluri_GetAll
-- ========================================
IF OBJECT_ID('dbo.sp_Roluri_GetAll', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Roluri_GetAll
GO

CREATE PROCEDURE dbo.sp_Roluri_GetAll
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @SearchText NVARCHAR(100) = NULL,
    @EsteActiv BIT = NULL,
    @SortColumn NVARCHAR(50) = 'OrdineAfisare',
    @SortDirection NVARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        r.[RolID],
        r.[Denumire],
        r.[Descriere],
        r.[Este_Activ] AS EsteActiv,
        r.[Este_Sistem] AS EsteSistem,
        r.[Ordine_Afisare] AS OrdineAfisare,
        r.[Data_Crearii] AS DataCrearii,
        r.[Data_Ultimei_Modificari] AS DataUltimeiModificari,
        r.[Creat_De] AS CreatDe,
        r.[Modificat_De] AS ModificatDe,
        (SELECT COUNT(*) FROM dbo.RoluriPermisiuni rp WHERE rp.[RolID] = r.[RolID]) AS NumarPermisiuni,
        (SELECT COUNT(*) FROM dbo.Utilizatori u WHERE u.[Rol] = r.[Denumire]) AS NumarUtilizatori
    FROM dbo.Roluri r
    WHERE (@SearchText IS NULL OR r.[Denumire] LIKE '%' + @SearchText + '%' OR r.[Descriere] LIKE '%' + @SearchText + '%')
      AND (@EsteActiv IS NULL OR r.[Este_Activ] = @EsteActiv)
    ORDER BY
        CASE WHEN @SortColumn = 'Denumire' AND @SortDirection = 'ASC' THEN r.[Denumire] END ASC,
        CASE WHEN @SortColumn = 'Denumire' AND @SortDirection = 'DESC' THEN r.[Denumire] END DESC,
        CASE WHEN @SortColumn = 'OrdineAfisare' AND @SortDirection = 'ASC' THEN r.[Ordine_Afisare] END ASC,
        CASE WHEN @SortColumn = 'OrdineAfisare' AND @SortDirection = 'DESC' THEN r.[Ordine_Afisare] END DESC,
        CASE WHEN @SortColumn = 'DataCrearii' AND @SortDirection = 'ASC' THEN r.[Data_Crearii] END ASC,
        CASE WHEN @SortColumn = 'DataCrearii' AND @SortDirection = 'DESC' THEN r.[Data_Crearii] END DESC,
        r.[Ordine_Afisare] ASC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- ========================================
-- 2. sp_Roluri_GetCount
-- ========================================
IF OBJECT_ID('dbo.sp_Roluri_GetCount', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Roluri_GetCount
GO

CREATE PROCEDURE dbo.sp_Roluri_GetCount
    @SearchText NVARCHAR(100) = NULL,
    @EsteActiv BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*)
    FROM dbo.Roluri r
    WHERE (@SearchText IS NULL OR r.[Denumire] LIKE '%' + @SearchText + '%' OR r.[Descriere] LIKE '%' + @SearchText + '%')
      AND (@EsteActiv IS NULL OR r.[Este_Activ] = @EsteActiv);
END
GO

-- ========================================
-- 3. sp_Roluri_GetById
-- ========================================
IF OBJECT_ID('dbo.sp_Roluri_GetById', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Roluri_GetById
GO

CREATE PROCEDURE dbo.sp_Roluri_GetById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        r.[RolID],
        r.[Denumire],
        r.[Descriere],
        r.[Este_Activ] AS EsteActiv,
        r.[Este_Sistem] AS EsteSistem,
        r.[Ordine_Afisare] AS OrdineAfisare,
        r.[Data_Crearii] AS DataCrearii,
        r.[Data_Ultimei_Modificari] AS DataUltimeiModificari,
        r.[Creat_De] AS CreatDe,
        r.[Modificat_De] AS ModificatDe
    FROM dbo.Roluri r
    WHERE r.[RolID] = @Id;
END
GO

-- ========================================
-- 4. sp_Roluri_Create
-- ========================================
IF OBJECT_ID('dbo.sp_Roluri_Create', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Roluri_Create
GO

CREATE PROCEDURE dbo.sp_Roluri_Create
    @Denumire NVARCHAR(100),
    @Descriere NVARCHAR(500) = NULL,
    @EsteActiv BIT = 1,
    @OrdineAfisare INT = 0,
    @CreatDe NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @NewId UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO dbo.Roluri ([RolID], [Denumire], [Descriere], [Este_Activ], [Este_Sistem], [Ordine_Afisare], [Creat_De])
    VALUES (@NewId, @Denumire, @Descriere, @EsteActiv, 0, @OrdineAfisare, @CreatDe);
    
    SELECT 
        r.[RolID],
        r.[Denumire],
        r.[Descriere],
        r.[Este_Activ] AS EsteActiv,
        r.[Este_Sistem] AS EsteSistem,
        r.[Ordine_Afisare] AS OrdineAfisare,
        r.[Data_Crearii] AS DataCrearii,
        r.[Data_Ultimei_Modificari] AS DataUltimeiModificari,
        r.[Creat_De] AS CreatDe,
        r.[Modificat_De] AS ModificatDe
    FROM dbo.Roluri r
    WHERE r.[RolID] = @NewId;
END
GO

-- ========================================
-- 5. sp_Roluri_Update
-- ========================================
IF OBJECT_ID('dbo.sp_Roluri_Update', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Roluri_Update
GO

CREATE PROCEDURE dbo.sp_Roluri_Update
    @Id UNIQUEIDENTIFIER,
    @Denumire NVARCHAR(100),
    @Descriere NVARCHAR(500) = NULL,
    @EsteActiv BIT = 1,
    @OrdineAfisare INT = 0,
    @ModificatDe NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.Roluri
    SET [Denumire] = @Denumire,
        [Descriere] = @Descriere,
        [Este_Activ] = @EsteActiv,
        [Ordine_Afisare] = @OrdineAfisare,
        [Modificat_De] = @ModificatDe,
        [Data_Ultimei_Modificari] = GETDATE()
    WHERE [RolID] = @Id AND [Este_Sistem] = 0;
    
    SELECT @@ROWCOUNT AS AffectedRows;
END
GO

-- ========================================
-- 6. sp_Roluri_Delete
-- ========================================
IF OBJECT_ID('dbo.sp_Roluri_Delete', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Roluri_Delete
GO

CREATE PROCEDURE dbo.sp_Roluri_Delete
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Nu permite stergerea rolurilor de sistem
    IF EXISTS (SELECT 1 FROM dbo.Roluri WHERE [RolID] = @Id AND [Este_Sistem] = 1)
    BEGIN
        RAISERROR('Nu se poate șterge un rol de sistem.', 16, 1);
        RETURN;
    END
    
    DELETE FROM dbo.Roluri WHERE [RolID] = @Id;
    
    SELECT @@ROWCOUNT AS AffectedRows;
END
GO

-- ========================================
-- 7. sp_Roluri_GetPermisiuni
-- ========================================
IF OBJECT_ID('dbo.sp_Roluri_GetPermisiuni', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Roluri_GetPermisiuni
GO

CREATE PROCEDURE dbo.sp_Roluri_GetPermisiuni
    @RolID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        rp.[RolPermisiuneID],
        rp.[RolID],
        rp.[Permisiune],
        rp.[Este_Acordat] AS EsteAcordat,
        rp.[Data_Crearii] AS DataCrearii,
        rp.[Creat_De] AS CreatDe,
        pd.[Categorie],
        pd.[Denumire] AS PermisiuneDenumire,
        pd.[Descriere] AS PermisiuneDescriere
    FROM dbo.RoluriPermisiuni rp
    LEFT JOIN dbo.PermisiuniDefinitii pd ON rp.[Permisiune] = pd.[Cod]
    WHERE rp.[RolID] = @RolID
    ORDER BY pd.[Categorie], pd.[Ordine_Afisare];
END
GO

-- ========================================
-- 8. sp_Roluri_SetPermisiuni
-- ========================================
IF OBJECT_ID('dbo.sp_Roluri_SetPermisiuni', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Roluri_SetPermisiuni
GO

CREATE PROCEDURE dbo.sp_Roluri_SetPermisiuni
    @RolID UNIQUEIDENTIFIER,
    @Permisiuni NVARCHAR(MAX), -- Lista de coduri permisiuni separate prin virgula
    @CreatDe NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Sterge permisiunile existente
        DELETE FROM dbo.RoluriPermisiuni WHERE [RolID] = @RolID;
        
        -- Insereaza permisiunile noi
        INSERT INTO dbo.RoluriPermisiuni ([RolID], [Permisiune], [Creat_De])
        SELECT @RolID, TRIM(value), @CreatDe
        FROM STRING_SPLIT(@Permisiuni, ',')
        WHERE TRIM(value) <> '';
        
        -- Actualizeaza Data_Ultimei_Modificari pentru rol
        UPDATE dbo.Roluri
        SET [Data_Ultimei_Modificari] = GETDATE(),
            [Modificat_De] = @CreatDe
        WHERE [RolID] = @RolID;
        
        COMMIT TRANSACTION;
        
        SELECT @@ROWCOUNT AS InsertedCount;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ========================================
-- 9. sp_Roluri_GetByDenumire
-- ========================================
IF OBJECT_ID('dbo.sp_Roluri_GetByDenumire', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Roluri_GetByDenumire
GO

CREATE PROCEDURE dbo.sp_Roluri_GetByDenumire
    @Denumire NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        rp.[Permisiune]
    FROM dbo.Roluri r
    INNER JOIN dbo.RoluriPermisiuni rp ON r.[RolID] = rp.[RolID]
    WHERE r.[Denumire] = @Denumire AND r.[Este_Activ] = 1 AND rp.[Este_Acordat] = 1;
END
GO

-- ========================================
-- 10. sp_PermisiuniDefinitii_GetAll
-- ========================================
IF OBJECT_ID('dbo.sp_PermisiuniDefinitii_GetAll', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_PermisiuniDefinitii_GetAll
GO

CREATE PROCEDURE dbo.sp_PermisiuniDefinitii_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pd.[PermisiuneDefinitieID],
        pd.[Cod],
        pd.[Categorie],
        pd.[Denumire],
        pd.[Descriere],
        pd.[Ordine_Afisare] AS OrdineAfisare,
        pd.[Este_Activ] AS EsteActiv
    FROM dbo.PermisiuniDefinitii pd
    WHERE pd.[Este_Activ] = 1
    ORDER BY pd.[Categorie], pd.[Ordine_Afisare];
END
GO

-- ========================================
-- 11. sp_PermisiuniDefinitii_GetCategorii
-- ========================================
IF OBJECT_ID('dbo.sp_PermisiuniDefinitii_GetCategorii', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_PermisiuniDefinitii_GetCategorii
GO

CREATE PROCEDURE dbo.sp_PermisiuniDefinitii_GetCategorii
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT pd.[Categorie]
    FROM dbo.PermisiuniDefinitii pd
    WHERE pd.[Este_Activ] = 1
    ORDER BY pd.[Categorie];
END
GO

PRINT 'Stored procedures pentru Roluri si Permisiuni create cu succes.'
GO
