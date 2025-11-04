-- ========================================
-- Stored Procedures pentru Pozitii
-- Database: ValyanMed
-- Creat: 2025-01-20
-- ========================================

USE [ValyanMed]
GO

-- ============================================================================
-- 1. sp_Pozitii_GetAll - Obtinere lista completa cu filtrare si sortare
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pozitii_GetAll')
    DROP PROCEDURE sp_Pozitii_GetAll
GO

CREATE PROCEDURE sp_Pozitii_GetAll
    @PageNumber INT = 1,
    @PageSize INT = 50,
    @SearchText NVARCHAR(255) = NULL,
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
    IF @SortColumn NOT IN ('Denumire', 'Data_Crearii', 'Data_Ultimei_Modificari')
        SET @SortColumn = 'Denumire';
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        Id,
        Denumire,
        Descriere,
        Este_Activ,
        Data_Crearii,
        Data_Ultimei_Modificari,
        Creat_De,
        Modificat_De
    FROM Pozitii
    WHERE 1=1
        AND (@SearchText IS NULL OR Denumire LIKE '%' + @SearchText + '%')
        AND (@EsteActiv IS NULL OR Este_Activ = @EsteActiv)
    ORDER BY 
        CASE WHEN @SortColumn = 'Denumire' AND @SortDirection = 'ASC' THEN Denumire END ASC,
        CASE WHEN @SortColumn = 'Denumire' AND @SortDirection = 'DESC' THEN Denumire END DESC,
        CASE WHEN @SortColumn = 'Data_Crearii' AND @SortDirection = 'ASC' THEN Data_Crearii END ASC,
        CASE WHEN @SortColumn = 'Data_Crearii' AND @SortDirection = 'DESC' THEN Data_Crearii END DESC,
        CASE WHEN @SortColumn = 'Data_Ultimei_Modificari' AND @SortDirection = 'ASC' THEN Data_Ultimei_Modificari END ASC,
        CASE WHEN @SortColumn = 'Data_Ultimei_Modificari' AND @SortDirection = 'DESC' THEN Data_Ultimei_Modificari END DESC
    OFFSET @Offset ROWS 
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- ============================================================================
-- 2. sp_Pozitii_GetCount - Obtinere numar total cu filtrare
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pozitii_GetCount')
    DROP PROCEDURE sp_Pozitii_GetCount
GO

CREATE PROCEDURE sp_Pozitii_GetCount
    @SearchText NVARCHAR(255) = NULL,
    @EsteActiv BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*) AS TotalCount
    FROM Pozitii
    WHERE 1=1
        AND (@SearchText IS NULL OR Denumire LIKE '%' + @SearchText + '%')
        AND (@EsteActiv IS NULL OR Este_Activ = @EsteActiv);
END
GO

-- ============================================================================
-- 3. sp_Pozitii_GetById - Obtinere pozitie dupa ID
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pozitii_GetById')
    DROP PROCEDURE sp_Pozitii_GetById
GO

CREATE PROCEDURE sp_Pozitii_GetById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        Denumire,
        Descriere,
        Este_Activ,
        Data_Crearii,
        Data_Ultimei_Modificari,
        Creat_De,
        Modificat_De
    FROM Pozitii 
    WHERE Id = @Id;
END
GO

-- ============================================================================
-- 4. sp_Pozitii_GetByDenumire - Obtinere pozitie dupa denumire
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pozitii_GetByDenumire')
    DROP PROCEDURE sp_Pozitii_GetByDenumire
GO

CREATE PROCEDURE sp_Pozitii_GetByDenumire
    @Denumire NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        Denumire,
        Descriere,
        Este_Activ,
        Data_Crearii,
        Data_Ultimei_Modificari,
        Creat_De,
        Modificat_De
    FROM Pozitii 
    WHERE Denumire = @Denumire;
END
GO

-- ============================================================================
-- 5. sp_Pozitii_GetDropdownOptions - Optiuni pentru dropdown-uri
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pozitii_GetDropdownOptions')
    DROP PROCEDURE sp_Pozitii_GetDropdownOptions
GO

CREATE PROCEDURE sp_Pozitii_GetDropdownOptions
    @EsteActiv BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        CAST(Id AS NVARCHAR(36)) AS Value,
        Denumire AS Text
    FROM Pozitii 
    WHERE Este_Activ = @EsteActiv
    ORDER BY Denumire ASC;
END
GO

-- ============================================================================
-- 6. sp_Pozitii_Create - Creare pozitie noua
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pozitii_Create')
    DROP PROCEDURE sp_Pozitii_Create
GO

CREATE PROCEDURE sp_Pozitii_Create
    @Denumire NVARCHAR(200),
    @Descriere NVARCHAR(MAX) = NULL,
    @EsteActiv BIT = 1,
    @CreatDe NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificare duplicat denumire
        IF EXISTS (SELECT 1 FROM Pozitii WHERE Denumire = @Denumire)
        BEGIN
            THROW 50001, 'O pozitie cu aceasta denumire exista deja.', 1;
        END
        
        DECLARE @NewId UNIQUEIDENTIFIER;
        DECLARE @CurrentDate DATETIME2 = GETDATE();
        DECLARE @OutputTable TABLE (Id UNIQUEIDENTIFIER);
        
        -- Folosim OUTPUT cu table variable pentru a captura UNIQUEIDENTIFIER-ul generat
        INSERT INTO Pozitii (
            Denumire,
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
        
        -- Returnare pozitie creata
        EXEC sp_Pozitii_GetById @NewId;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ============================================================================
-- 7. sp_Pozitii_Update - Actualizare pozitie existenta
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pozitii_Update')
    DROP PROCEDURE sp_Pozitii_Update
GO

CREATE PROCEDURE sp_Pozitii_Update
    @Id UNIQUEIDENTIFIER,
    @Denumire NVARCHAR(200),
    @Descriere NVARCHAR(MAX) = NULL,
    @EsteActiv BIT,
    @ModificatDe NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificare existenta
        IF NOT EXISTS (SELECT 1 FROM Pozitii WHERE Id = @Id)
        BEGIN
            THROW 50002, 'Pozitia specificata nu exista.', 1;
        END
        
        -- Verificare duplicat denumire (exclude current ID)
        IF EXISTS (SELECT 1 FROM Pozitii WHERE Denumire = @Denumire AND Id != @Id)
        BEGIN
            THROW 50001, 'O pozitie cu aceasta denumire exista deja.', 1;
        END
        
        DECLARE @CurrentDate DATETIME2 = GETDATE();
        
        UPDATE Pozitii SET
            Denumire = @Denumire,
            Descriere = @Descriere,
            Este_Activ = @EsteActiv,
            Data_Ultimei_Modificari = @CurrentDate,
            Modificat_De = @ModificatDe
        WHERE Id = @Id;
        
        COMMIT TRANSACTION;
        
        -- Returnare pozitie actualizata
        EXEC sp_Pozitii_GetById @Id;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ============================================================================
-- 8. sp_Pozitii_Delete - Soft delete pentru pozitie
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pozitii_Delete')
    DROP PROCEDURE sp_Pozitii_Delete
GO

CREATE PROCEDURE sp_Pozitii_Delete
    @Id UNIQUEIDENTIFIER,
    @ModificatDe NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificare existenta
        IF NOT EXISTS (SELECT 1 FROM Pozitii WHERE Id = @Id)
        BEGIN
            THROW 50002, 'Pozitia specificata nu existe.', 1;
        END
        
        DECLARE @CurrentDate DATETIME2 = GETDATE();
        
        -- Soft delete - marcare ca inactiv
        UPDATE Pozitii SET
            Este_Activ = 0,
            Data_Ultimei_Modificari = @CurrentDate,
            Modificat_De = @ModificatDe
        WHERE Id = @Id;
        
        COMMIT TRANSACTION;
        
        SELECT 1 AS Success, 'Pozitia a fost dezactivata cu succes.' AS Message;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ============================================================================
-- 9. sp_Pozitii_HardDelete - Stergere fizica (folosire cu precautie)
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pozitii_HardDelete')
    DROP PROCEDURE sp_Pozitii_HardDelete
GO

CREATE PROCEDURE sp_Pozitii_HardDelete
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificare existenta
        IF NOT EXISTS (SELECT 1 FROM Pozitii WHERE Id = @Id)
        BEGIN
            THROW 50002, 'Pozitia specificata nu exista.', 1;
        END
        
        -- TODO: Verificare referinte in alte tabele (ex: Personal, PersonalMedical)
        -- Exemplu: IF EXISTS (SELECT 1 FROM Personal WHERE Id_Pozitie = @Id)
        
        DELETE FROM Pozitii WHERE Id = @Id;
        
        COMMIT TRANSACTION;
        
        SELECT 1 AS Success, 'Pozitia a fost stearsa definitiv.' AS Message;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ============================================================================
-- 10. sp_Pozitii_CheckUnique - Verificare unicitate denumire
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pozitii_CheckUnique')
    DROP PROCEDURE sp_Pozitii_CheckUnique
GO

CREATE PROCEDURE sp_Pozitii_CheckUnique
    @Denumire NVARCHAR(200),
    @ExcludeId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT CASE 
        WHEN EXISTS (
            SELECT 1 
            FROM Pozitii 
            WHERE Denumire = @Denumire 
            AND (@ExcludeId IS NULL OR Id != @ExcludeId)
        ) THEN 1 
        ELSE 0 
    END AS Denumire_Exists;
END
GO

-- ============================================================================
-- 11. sp_Pozitii_GetStatistics - Statistici pentru dashboard
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pozitii_GetStatistics')
    DROP PROCEDURE sp_Pozitii_GetStatistics
GO

CREATE PROCEDURE sp_Pozitii_GetStatistics
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        'Total Pozitii' AS Categorie,
        COUNT(*) AS Numar,
        SUM(CASE WHEN Este_Activ = 1 THEN 1 ELSE 0 END) AS Active
    FROM Pozitii;
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
WHERE name LIKE 'sp_Pozitii_%'
ORDER BY name;

PRINT '';
PRINT 'Script executat cu succes! Toate procedurile stocate pentru Pozitii au fost create.';
GO
