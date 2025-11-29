-- ========================================
-- Script Master de Instalare - Tabela Pozitii
-- Database: ValyanMed
-- Descriere: Script pentru instalarea completa automata
-- ========================================

USE [ValyanMed]
GO

PRINT '========================================';
PRINT 'INSTALARE TABELA POZITII - START';
PRINT 'Database: ValyanMed';
PRINT 'Data: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
PRINT '';

-- ============================================================================
-- STEP 1: Creare Tabel si Populare Date
-- ============================================================================
PRINT 'STEP 1: Creare tabel si populare date initiale...';
PRINT '';

-- Drop table if exists
IF OBJECT_ID('dbo.Pozitii', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Pozitii
    PRINT '   ? Tabel Pozitii existent sters.'
END

-- Create table
CREATE TABLE dbo.Pozitii (
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [Denumire] NVARCHAR(200) NOT NULL,
    [Descriere] NVARCHAR(MAX) NULL,
    [Este_Activ] BIT NOT NULL DEFAULT 1,
    [Data_Crearii] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [Data_Ultimei_Modificari] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [Creat_De] NVARCHAR(100) NULL DEFAULT SYSTEM_USER,
    [Modificat_De] NVARCHAR(100) NULL DEFAULT SYSTEM_USER,
    
    CONSTRAINT [PK_Pozitii] PRIMARY KEY ([Id]),
    CONSTRAINT [UK_Pozitii_Denumire] UNIQUE ([Denumire])
)
PRINT '   ? Tabel Pozitii creat cu succes.';

-- Index pentru performanta
CREATE NONCLUSTERED INDEX [IX_Pozitii_Denumire] 
ON dbo.Pozitii ([Denumire] ASC)

CREATE NONCLUSTERED INDEX [IX_Pozitii_Activ] 
ON dbo.Pozitii ([Este_Activ] ASC)
PRINT '   ? Indexuri create cu succes.';

-- Trigger pentru actualizarea automata a datei de modificare
CREATE TRIGGER [TR_Pozitii_UpdateTimestamp]
ON dbo.Pozitii
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.Pozitii
    SET [Data_Ultimei_Modificari] = GETDATE(),
        [Modificat_De] = SYSTEM_USER
    FROM dbo.Pozitii p
    INNER JOIN inserted i ON p.[Id] = i.[Id]
END
PRINT '   ? Trigger creat cu succes.';

-- Populare date initiale
INSERT INTO dbo.Pozitii ([Denumire], [Descriere], [Este_Activ])
VALUES 
    (N'Medic primar', NULL, 1),
    (N'Medic specialist', NULL, 1),
    (N'Medic rezident', NULL, 1),
    (N'Medic stomatolog', NULL, 1),
    (N'Farmacist', NULL, 1),
    (N'Biolog', NULL, 1),
    (N'Biochimist', NULL, 1),
    (N'Chimist', NULL, 1),
    (N'?ef de sec?ie', NULL, 1),
    (N'?ef de laborator', NULL, 1),
    (N'?ef de compartiment', NULL, 1),
    (N'Farmacist-?ef', NULL, 1),
    (N'Asistent medical generalist', NULL, 1),
    (N'Asistent medical cu studii superioare specialitatea medicina general?', NULL, 1),
    (N'Asistent medical cu studii postliceale medicina general?', NULL, 1),
    (N'Moa??', NULL, 1),
    (N'Infirmier? (debutant? ?i cu vechime)', NULL, 1),
    (N'Îngrijitoare', NULL, 1),
    (N'Brancardier', NULL, 1),
    (N'Kinetoterapeut', NULL, 1);
    
DECLARE @NumarPozitii INT = (SELECT COUNT(*) FROM Pozitii);
PRINT '   ? Date inserate: ' + CAST(@NumarPozitii AS VARCHAR) + ' pozitii';
PRINT '';

-- ============================================================================
-- STEP 2: Creare Stored Procedures
-- ============================================================================
PRINT 'STEP 2: Creare stored procedures...';
PRINT '';

-- 1. sp_Pozitii_GetAll
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pozitii_GetAll')
    DROP PROCEDURE sp_Pozitii_GetAll

EXEC('
CREATE PROCEDURE sp_Pozitii_GetAll
    @PageNumber INT = 1,
    @PageSize INT = 50,
    @SearchText NVARCHAR(255) = NULL,
    @EsteActiv BIT = NULL,
    @SortColumn NVARCHAR(50) = ''Denumire'',
    @SortDirection NVARCHAR(4) = ''ASC''
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @SortDirection NOT IN (''ASC'', ''DESC'')
        SET @SortDirection = ''ASC'';
    
    IF @SortColumn NOT IN (''Denumire'', ''Data_Crearii'', ''Data_Ultimei_Modificari'')
        SET @SortColumn = ''Denumire'';
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        Id, Denumire, Descriere, Este_Activ,
        Data_Crearii, Data_Ultimei_Modificari,
        Creat_De, Modificat_De
    FROM Pozitii
    WHERE 1=1
        AND (@SearchText IS NULL OR Denumire LIKE ''%'' + @SearchText + ''%'')
        AND (@EsteActiv IS NULL OR Este_Activ = @EsteActiv)
    ORDER BY 
        CASE WHEN @SortColumn = ''Denumire'' AND @SortDirection = ''ASC'' THEN Denumire END ASC,
        CASE WHEN @SortColumn = ''Denumire'' AND @SortDirection = ''DESC'' THEN Denumire END DESC,
        CASE WHEN @SortColumn = ''Data_Crearii'' AND @SortDirection = ''ASC'' THEN Data_Crearii END ASC,
        CASE WHEN @SortColumn = ''Data_Crearii'' AND @SortDirection = ''DESC'' THEN Data_Crearii END DESC
    OFFSET @Offset ROWS 
    FETCH NEXT @PageSize ROWS ONLY;
END
')
PRINT '   ? sp_Pozitii_GetAll';

-- 2. sp_Pozitii_GetCount
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pozitii_GetCount')
    DROP PROCEDURE sp_Pozitii_GetCount

EXEC('
CREATE PROCEDURE sp_Pozitii_GetCount
    @SearchText NVARCHAR(255) = NULL,
    @EsteActiv BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*) AS TotalCount
    FROM Pozitii
    WHERE 1=1
        AND (@SearchText IS NULL OR Denumire LIKE ''%'' + @SearchText + ''%'')
        AND (@EsteActiv IS NULL OR Este_Activ = @EsteActiv);
END
')
PRINT '   ? sp_Pozitii_GetCount';

-- 3. sp_Pozitii_GetById
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pozitii_GetById')
    DROP PROCEDURE sp_Pozitii_GetById

EXEC('
CREATE PROCEDURE sp_Pozitii_GetById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, Denumire, Descriere, Este_Activ,
        Data_Crearii, Data_Ultimei_Modificari,
        Creat_De, Modificat_De
    FROM Pozitii 
    WHERE Id = @Id;
END
')
PRINT '   ? sp_Pozitii_GetById';

-- 4. sp_Pozitii_GetDropdownOptions
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pozitii_GetDropdownOptions')
    DROP PROCEDURE sp_Pozitii_GetDropdownOptions

EXEC('
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
')
PRINT '   ? sp_Pozitii_GetDropdownOptions';

-- 5. sp_Pozitii_CheckUnique
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pozitii_CheckUnique')
    DROP PROCEDURE sp_Pozitii_CheckUnique

EXEC('
CREATE PROCEDURE sp_Pozitii_CheckUnique
    @Denumire NVARCHAR(200),
    @ExcludeId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT CASE 
        WHEN EXISTS (
            SELECT 1 FROM Pozitii 
            WHERE Denumire = @Denumire 
            AND (@ExcludeId IS NULL OR Id != @ExcludeId)
        ) THEN 1 ELSE 0 
    END AS Denumire_Exists;
END
')
PRINT '   ? sp_Pozitii_CheckUnique';

-- 6. sp_Pozitii_GetStatistics
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pozitii_GetStatistics')
    DROP PROCEDURE sp_Pozitii_GetStatistics

EXEC('
CREATE PROCEDURE sp_Pozitii_GetStatistics
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ''Total Pozitii'' AS Categorie,
        COUNT(*) AS Numar,
        SUM(CASE WHEN Este_Activ = 1 THEN 1 ELSE 0 END) AS Active
    FROM Pozitii;
END
')
PRINT '   ? sp_Pozitii_GetStatistics';

DECLARE @NumarSP INT = (SELECT COUNT(*) FROM sys.procedures WHERE name LIKE 'sp_Pozitii_%');
PRINT '   ? Total SP-uri create: ' + CAST(@NumarSP AS VARCHAR);
PRINT '';

-- ============================================================================
-- STEP 3: Verificare Finala
-- ============================================================================
PRINT 'STEP 3: Verificare finala...';
PRINT '';

DECLARE @Verificari TABLE (Test VARCHAR(100), Status VARCHAR(10));

-- Verificare tabel
INSERT INTO @Verificari
SELECT 'Tabel Pozitii', CASE WHEN OBJECT_ID('dbo.Pozitii', 'U') IS NOT NULL THEN '? OK' ELSE '? EROARE' END;

-- Verificare date
INSERT INTO @Verificari
SELECT 'Date populate', CASE WHEN (SELECT COUNT(*) FROM Pozitii) >= 20 THEN '? OK' ELSE '? EROARE' END;

-- Verificare SP-uri
INSERT INTO @Verificari
SELECT 'Stored Procedures', CASE WHEN (SELECT COUNT(*) FROM sys.procedures WHERE name LIKE 'sp_Pozitii_%') >= 6 THEN '? OK' ELSE '? EROARE' END;

-- Verificare indexuri
INSERT INTO @Verificari
SELECT 'Indexuri', CASE WHEN (SELECT COUNT(*) FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Pozitii') AND type > 0) >= 3 THEN '? OK' ELSE '? EROARE' END;

-- Verificare trigger
INSERT INTO @Verificari
SELECT 'Trigger', CASE WHEN EXISTS (SELECT 1 FROM sys.triggers WHERE parent_id = OBJECT_ID('dbo.Pozitii')) THEN '? OK' ELSE '? EROARE' END;

-- Afisare rezultate
SELECT * FROM @Verificari;

PRINT '';
PRINT '========================================';
IF NOT EXISTS (SELECT 1 FROM @Verificari WHERE Status LIKE '%EROARE%')
BEGIN
    PRINT '??? INSTALARE COMPLETA SI FUNCTIONALA ???';
END
ELSE
BEGIN
    PRINT '??? INSTALARE CU ERORI - Verificati mai sus ???';
END
PRINT '========================================';
PRINT '';
PRINT 'INSTALARE TABELA POZITII - FINALIZAT';
PRINT '========================================';
GO
