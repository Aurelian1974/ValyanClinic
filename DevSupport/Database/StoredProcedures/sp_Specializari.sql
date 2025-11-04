-- ========================================
-- Stored Procedures pentru Specializari
-- Database: ValyanMed
-- Creat: 2025-01-20
-- ========================================

USE [ValyanMed]
GO

-- ============================================================================
-- 1. sp_Specializari_GetAll - Obtinere lista completa cu filtrare si sortare
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Specializari_GetAll')
    DROP PROCEDURE sp_Specializari_GetAll
GO

CREATE PROCEDURE sp_Specializari_GetAll
    @PageNumber INT = 1,
    @PageSize INT = 100,
    @SearchText NVARCHAR(255) = NULL,
    @Categorie NVARCHAR(100) = NULL,
    @EsteActiv BIT = NULL,
    @SortColumn NVARCHAR(50) = 'Denumire',
    @SortDirection NVARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate sort direction
    IF @SortDirection NOT IN ('ASC', 'DESC')
        SET @SortDirection = 'ASC';
    
    -- Whitelist for sort columns
    IF @SortColumn NOT IN ('Denumire', 'Categorie', 'Data_Crearii', 'Data_Ultimei_Modificari')
        SET @SortColumn = 'Denumire';
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        Id,
        Denumire,
        Categorie,
        Descriere,
        Este_Activ,
        Data_Crearii,
        Data_Ultimei_Modificari,
        Creat_De,
        Modificat_De
    FROM Specializari
    WHERE 1=1
        AND (@SearchText IS NULL OR Denumire LIKE '%' + @SearchText + '%')
        AND (@Categorie IS NULL OR Categorie = @Categorie)
        AND (@EsteActiv IS NULL OR Este_Activ = @EsteActiv)
    ORDER BY 
        CASE WHEN @SortColumn = 'Denumire' AND @SortDirection = 'ASC' THEN Denumire END ASC,
        CASE WHEN @SortColumn = 'Denumire' AND @SortDirection = 'DESC' THEN Denumire END DESC,
        CASE WHEN @SortColumn = 'Categorie' AND @SortDirection = 'ASC' THEN Categorie END ASC,
        CASE WHEN @SortColumn = 'Categorie' AND @SortDirection = 'DESC' THEN Categorie END DESC,
        CASE WHEN @SortColumn = 'Data_Crearii' AND @SortDirection = 'ASC' THEN Data_Crearii END ASC,
        CASE WHEN @SortColumn = 'Data_Crearii' AND @SortDirection = 'DESC' THEN Data_Crearii END DESC,
        CASE WHEN @SortColumn = 'Data_Ultimei_Modificari' AND @SortDirection = 'ASC' THEN Data_Ultimei_Modificari END ASC,
        CASE WHEN @SortColumn = 'Data_Ultimei_Modificari' AND @SortDirection = 'DESC' THEN Data_Ultimei_Modificari END DESC
    OFFSET @Offset ROWS 
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- ============================================================================
-- 2. sp_Specializari_GetCount - Obtinere numar total cu filtrare
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Specializari_GetCount')
    DROP PROCEDURE sp_Specializari_GetCount
GO

CREATE PROCEDURE sp_Specializari_GetCount
    @SearchText NVARCHAR(255) = NULL,
    @Categorie NVARCHAR(100) = NULL,
    @EsteActiv BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*) AS TotalCount
    FROM Specializari
    WHERE 1=1
        AND (@SearchText IS NULL OR Denumire LIKE '%' + @SearchText + '%')
        AND (@Categorie IS NULL OR Categorie = @Categorie)
        AND (@EsteActiv IS NULL OR Este_Activ = @EsteActiv);
END
GO

-- ============================================================================
-- 3. sp_Specializari_GetById - Obtinere specializare dupa ID
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Specializari_GetById')
    DROP PROCEDURE sp_Specializari_GetById
GO

CREATE PROCEDURE sp_Specializari_GetById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        Denumire,
        Categorie,
        Descriere,
        Este_Activ,
        Data_Crearii,
        Data_Ultimei_Modificari,
        Creat_De,
        Modificat_De
    FROM Specializari 
    WHERE Id = @Id;
END
GO

-- ============================================================================
-- 4. sp_Specializari_GetByDenumire - Obtinere specializare dupa denumire
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Specializari_GetByDenumire')
    DROP PROCEDURE sp_Specializari_GetByDenumire
GO

CREATE PROCEDURE sp_Specializari_GetByDenumire
    @Denumire NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        Denumire,
        Categorie,
        Descriere,
        Este_Activ,
        Data_Crearii,
        Data_Ultimei_Modificari,
        Creat_De,
        Modificat_De
    FROM Specializari 
    WHERE Denumire = @Denumire;
END
GO

-- ============================================================================
-- 5. sp_Specializari_GetByCategorie - Obtinere specializari dupa categorie
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Specializari_GetByCategorie')
    DROP PROCEDURE sp_Specializari_GetByCategorie
GO

CREATE PROCEDURE sp_Specializari_GetByCategorie
    @Categorie NVARCHAR(100),
    @EsteActiv BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        Denumire,
        Categorie,
        Descriere,
        Este_Activ,
        Data_Crearii,
        Data_Ultimei_Modificari,
        Creat_De,
        Modificat_De
    FROM Specializari 
    WHERE Categorie = @Categorie
      AND Este_Activ = @EsteActiv
    ORDER BY Denumire ASC;
END
GO

-- ============================================================================
-- 6. sp_Specializari_GetCategorii - Obtinere lista categorii unice
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Specializari_GetCategorii')
    DROP PROCEDURE sp_Specializari_GetCategorii
GO

CREATE PROCEDURE sp_Specializari_GetCategorii
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT
        Categorie AS Value,
        Categorie AS Text,
        COUNT(*) AS NumarSpecializari
    FROM Specializari
    WHERE Categorie IS NOT NULL
      AND Este_Activ = 1
    GROUP BY Categorie
    ORDER BY Categorie ASC;
END
GO

-- ============================================================================
-- 7. sp_Specializari_GetDropdownOptions - Optiuni pentru dropdown-uri
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Specializari_GetDropdownOptions')
    DROP PROCEDURE sp_Specializari_GetDropdownOptions
GO

CREATE PROCEDURE sp_Specializari_GetDropdownOptions
    @Categorie NVARCHAR(100) = NULL,
    @EsteActiv BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        CAST(Id AS NVARCHAR(36)) AS Value,
        Denumire AS Text,
        Categorie
    FROM Specializari 
    WHERE Este_Activ = @EsteActiv
      AND (@Categorie IS NULL OR Categorie = @Categorie)
    ORDER BY Categorie ASC, Denumire ASC;
END
GO

-- ============================================================================
-- 8. sp_Specializari_Create - Creare specializare noua
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Specializari_Create')
    DROP PROCEDURE sp_Specializari_Create
GO

CREATE PROCEDURE sp_Specializari_Create
    @Denumire NVARCHAR(200),
    @Categorie NVARCHAR(100) = NULL,
    @Descriere NVARCHAR(MAX) = NULL,
    @EsteActiv BIT = 1,
    @CreatDe NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificare duplicat denumire
        IF EXISTS (SELECT 1 FROM Specializari WHERE Denumire = @Denumire)
        BEGIN
            THROW 50001, 'O specializare cu aceasta denumire exista deja.', 1;
        END
        
        DECLARE @NewId UNIQUEIDENTIFIER;
        DECLARE @CurrentDate DATETIME2 = GETDATE();
        DECLARE @OutputTable TABLE (Id UNIQUEIDENTIFIER);
        
        -- Folosim OUTPUT cu table variable pentru a captura UNIQUEIDENTIFIER-ul generat
        INSERT INTO Specializari (
            Denumire,
            Categorie,
            Descriere,
            Este_Activ,
            Data_Crearii,
            Data_Ultimei_Modificari,
            Creat_De,
            Modificat_De
        )
        OUTPUT INSERTED.Id INTO @OutputTable(Id)
        VALUES (
            @Denumire,
            @Categorie,
            @Descriere,
            @EsteActiv,
            @CurrentDate,
            @CurrentDate,
            @CreatDe,
            @CreatDe
        );
        
        -- Preluare ID din table variable
        SELECT @NewId = Id FROM @OutputTable;
        
        COMMIT TRANSACTION;
        
        -- Returnare specializare creata
        EXEC sp_Specializari_GetById @NewId;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ============================================================================
-- 9. sp_Specializari_Update - Actualizare specializare existenta
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Specializari_Update')
    DROP PROCEDURE sp_Specializari_Update
GO

CREATE PROCEDURE sp_Specializari_Update
    @Id UNIQUEIDENTIFIER,
    @Denumire NVARCHAR(200),
    @Categorie NVARCHAR(100) = NULL,
    @Descriere NVARCHAR(MAX) = NULL,
    @EsteActiv BIT,
    @ModificatDe NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificare existenta
        IF NOT EXISTS (SELECT 1 FROM Specializari WHERE Id = @Id)
        BEGIN
            THROW 50002, 'Specializarea specificata nu exista.', 1;
        END
        
        -- Verificare duplicat denumire (exclude current ID)
        IF EXISTS (SELECT 1 FROM Specializari WHERE Denumire = @Denumire AND Id != @Id)
        BEGIN
            THROW 50001, 'O specializare cu aceasta denumire exista deja.', 1;
        END
        
        DECLARE @CurrentDate DATETIME2 = GETDATE();
        
        UPDATE Specializari SET
            Denumire = @Denumire,
            Categorie = @Categorie,
            Descriere = @Descriere,
            Este_Activ = @EsteActiv,
            Data_Ultimei_Modificari = @CurrentDate,
            Modificat_De = @ModificatDe
        WHERE Id = @Id;
        
        COMMIT TRANSACTION;
        
        -- Returnare specializare actualizata
        EXEC sp_Specializari_GetById @Id;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ============================================================================
-- 10. sp_Specializari_Delete - Soft delete pentru specializare
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Specializari_Delete')
    DROP PROCEDURE sp_Specializari_Delete
GO

CREATE PROCEDURE sp_Specializari_Delete
    @Id UNIQUEIDENTIFIER,
    @ModificatDe NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificare existenta
        IF NOT EXISTS (SELECT 1 FROM Specializari WHERE Id = @Id)
        BEGIN
            THROW 50002, 'Specializarea specificata nu exista.', 1;
        END
        
        DECLARE @CurrentDate DATETIME2 = GETDATE();
        
        -- Soft delete - marcare ca inactiv
        UPDATE Specializari SET
            Este_Activ = 0,
            Data_Ultimei_Modificari = @CurrentDate,
            Modificat_De = @ModificatDe
        WHERE Id = @Id;
        
        COMMIT TRANSACTION;
        
        SELECT 1 AS Success, 'Specializarea a fost dezactivata cu succes.' AS Message;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ============================================================================
-- 11. sp_Specializari_HardDelete - Stergere fizica (folosire cu precautie)
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Specializari_HardDelete')
    DROP PROCEDURE sp_Specializari_HardDelete
GO

CREATE PROCEDURE sp_Specializari_HardDelete
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificare existenta
        IF NOT EXISTS (SELECT 1 FROM Specializari WHERE Id = @Id)
        BEGIN
            THROW 50002, 'Specializarea specificata nu exista.', 1;
        END
        
        -- TODO: Verificare referinte in alte tabele (ex: PersonalMedical)
        
        DELETE FROM Specializari WHERE Id = @Id;
        
        COMMIT TRANSACTION;
        
        SELECT 1 AS Success, 'Specializarea a fost stearsa definitiv.' AS Message;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ============================================================================
-- 12. sp_Specializari_CheckUnique - Verificare unicitate denumire
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Specializari_CheckUnique')
    DROP PROCEDURE sp_Specializari_CheckUnique
GO

CREATE PROCEDURE sp_Specializari_CheckUnique
    @Denumire NVARCHAR(200),
    @ExcludeId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT CASE 
        WHEN EXISTS (
            SELECT 1 
            FROM Specializari 
            WHERE Denumire = @Denumire 
            AND (@ExcludeId IS NULL OR Id != @ExcludeId)
        ) THEN 1 
        ELSE 0 
    END AS Denumire_Exists;
END
GO

-- ============================================================================
-- 13. sp_Specializari_GetStatistics - Statistici pentru dashboard
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Specializari_GetStatistics')
    DROP PROCEDURE sp_Specializari_GetStatistics
GO

CREATE PROCEDURE sp_Specializari_GetStatistics
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Statistici generale
    SELECT 
        'Total Specializari' AS Categorie,
        COUNT(*) AS Numar,
        SUM(CASE WHEN Este_Activ = 1 THEN 1 ELSE 0 END) AS Active
    FROM Specializari
    
    UNION ALL
    
    -- Statistici pe categorii
    SELECT 
        Categorie,
        COUNT(*) AS Numar,
        SUM(CASE WHEN Este_Activ = 1 THEN 1 ELSE 0 END) AS Active
    FROM Specializari
    WHERE Categorie IS NOT NULL
    GROUP BY Categorie
    ORDER BY Categorie;
END
GO

-- ============================================================================
-- VERIFICARE CREARE PROCEDURI
-- ============================================================================
PRINT 'Verificare proceduri create:';
SELECT 
    name AS 'Procedura Creata',
    create_date AS 'Data Creare'
FROM sys.procedures 
WHERE name LIKE 'sp_Specializari_%'
ORDER BY name;

PRINT '';
PRINT 'Script executat cu succes! Toate procedurile stocate pentru Specializari au fost create.';
GO
