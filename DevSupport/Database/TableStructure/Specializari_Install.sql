-- ========================================
-- Script Master de Instalare - Tabela Specializari
-- Database: ValyanMed
-- Descriere: Script pentru instalarea completa automata
-- ========================================

USE [ValyanMed]
GO

PRINT '========================================';
PRINT 'INSTALARE TABELA SPECIALIZARI - START';
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
IF OBJECT_ID('dbo.Specializari', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Specializari
    PRINT '   ? Tabel Specializari existent sters.'
END

-- Create table
CREATE TABLE dbo.Specializari (
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [Denumire] NVARCHAR(200) NOT NULL,
    [Categorie] NVARCHAR(100) NULL,
    [Descriere] NVARCHAR(MAX) NULL,
    [Este_Activ] BIT NOT NULL DEFAULT 1,
    [Data_Crearii] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [Data_Ultimei_Modificari] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [Creat_De] NVARCHAR(100) NULL DEFAULT SYSTEM_USER,
    [Modificat_De] NVARCHAR(100) NULL DEFAULT SYSTEM_USER,
    
    CONSTRAINT [PK_Specializari] PRIMARY KEY ([Id]),
    CONSTRAINT [UK_Specializari_Denumire] UNIQUE ([Denumire])
)
PRINT '   ? Tabel Specializari creat cu succes.';

-- Index pentru performanta
CREATE NONCLUSTERED INDEX [IX_Specializari_Denumire] 
ON dbo.Specializari ([Denumire] ASC)

CREATE NONCLUSTERED INDEX [IX_Specializari_Categorie] 
ON dbo.Specializari ([Categorie] ASC)

CREATE NONCLUSTERED INDEX [IX_Specializari_Activ] 
ON dbo.Specializari ([Este_Activ] ASC)
PRINT '   ? Indexuri create cu succes.';

-- Trigger pentru actualizarea automata a datei de modificare
CREATE TRIGGER [TR_Specializari_UpdateTimestamp]
ON dbo.Specializari
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.Specializari
    SET [Data_Ultimei_Modificari] = GETDATE(),
        [Modificat_De] = SYSTEM_USER
    FROM dbo.Specializari s
    INNER JOIN inserted i ON s.[Id] = i.[Id]
END
PRINT '   ? Trigger creat cu succes.';

-- Populare date initiale - Specializari Medicale
INSERT INTO dbo.Specializari ([Denumire], [Categorie], [Descriere], [Este_Activ])
VALUES 
    (N'Alergologie ?i imunologie clinic?', N'Medical?', NULL, 1),
    (N'Anestezie ?i terapie intensiv?', N'Medical?', NULL, 1),
    (N'Boli infec?ioase', N'Medical?', NULL, 1),
    (N'Cardiologie', N'Medical?', NULL, 1),
    (N'Cardiologie pediatric?', N'Medical?', NULL, 1),
    (N'Dermatovenerologie', N'Medical?', NULL, 1),
    (N'Diabet zaharat, nutri?ie ?i boli metabolice', N'Medical?', NULL, 1),
    (N'Endocrinologie', N'Medical?', NULL, 1),
    (N'Expertiza medical? a capacit??ii de munc?', N'Medical?', NULL, 1),
    (N'Farmacologie clinic?', N'Medical?', NULL, 1),
    (N'Gastroenterologie', N'Medical?', NULL, 1),
    (N'Gastroenterologie pediatric?', N'Medical?', NULL, 1),
    (N'Genetic? medical?', N'Medical?', NULL, 1),
    (N'Geriatrie ?i gerontologie', N'Medical?', NULL, 1),
    (N'Hematologie', N'Medical?', NULL, 1),
    (N'Medicin? de familie', N'Medical?', NULL, 1),
    (N'Medicin? de urgen??', N'Medical?', NULL, 1),
    (N'Medicin? intern?', N'Medical?', NULL, 1),
    (N'Medicin? fizic? ?i de reabilitare', N'Medical?', NULL, 1),
    (N'Medicina muncii', N'Medical?', NULL, 1),
    (N'Medicin? sportiv?', N'Medical?', NULL, 1),
    (N'Nefrologie', N'Medical?', NULL, 1),
    (N'Nefrologie pediatric?', N'Medical?', NULL, 1),
    (N'Neonatologie', N'Medical?', NULL, 1),
    (N'Neurologie', N'Medical?', NULL, 1),
    (N'Neurologie pediatric?', N'Medical?', NULL, 1),
    (N'Oncologie medical?', N'Medical?', NULL, 1),
    (N'Oncologie ?i hematologie pediatric?', N'Medical?', NULL, 1),
    (N'Pediatrie', N'Medical?', NULL, 1),
    (N'Pneumologie', N'Medical?', NULL, 1),
    (N'Pneumologie pediatric?', N'Medical?', NULL, 1),
    (N'Psihiatrie', N'Medical?', NULL, 1),
    (N'Psihiatrie pediatric?', N'Medical?', NULL, 1),
    (N'Radioterapie', N'Medical?', NULL, 1),
    (N'Reumatologie', N'Medical?', NULL, 1);

-- Specializari Chirurgicale
INSERT INTO dbo.Specializari ([Denumire], [Categorie], [Descriere], [Este_Activ])
VALUES 
    (N'Chirurgie cardiovascular?', N'Chirurgical?', NULL, 1),
    (N'Chirurgie general?', N'Chirurgical?', NULL, 1),
    (N'Chirurgie oral? ?i maxilo-facial?', N'Chirurgical?', NULL, 1),
    (N'Chirurgie pediatric?', N'Chirurgical?', NULL, 1),
    (N'Chirurgie plastic?, estetic? ?i microchirurgie reconstructiv?', N'Chirurgical?', NULL, 1),
    (N'Chirurgie toracic?', N'Chirurgical?', NULL, 1),
    (N'Chirurgie vascular?', N'Chirurgical?', NULL, 1),
    (N'Neurochirurgie', N'Chirurgical?', NULL, 1),
    (N'Obstetric?-ginecologie', N'Chirurgical?', NULL, 1),
    (N'Oftalmologie', N'Chirurgical?', NULL, 1),
    (N'Ortopedie pediatric?', N'Chirurgical?', NULL, 1),
    (N'Ortopedie ?i traumatologie', N'Chirurgical?', NULL, 1),
    (N'Otorinolaringologie', N'Chirurgical?', NULL, 1),
    (N'Urologie', N'Chirurgical?', NULL, 1);

-- Specializari de Laborator ?i Diagnostic
INSERT INTO dbo.Specializari ([Denumire], [Categorie], [Descriere], [Este_Activ])
VALUES 
    (N'Anatomie patologic?', N'Laborator ?i Diagnostic', NULL, 1),
    (N'Epidemiologie', N'Laborator ?i Diagnostic', NULL, 1),
    (N'Igien?', N'Laborator ?i Diagnostic', NULL, 1),
    (N'Medicin? de laborator', N'Laborator ?i Diagnostic', NULL, 1),
    (N'Medicin? legal?', N'Laborator ?i Diagnostic', NULL, 1),
    (N'Medicin? nuclear?', N'Laborator ?i Diagnostic', NULL, 1),
    (N'Microbiologie medical?', N'Laborator ?i Diagnostic', NULL, 1),
    (N'Radiologie-imagistic? medical?', N'Laborator ?i Diagnostic', NULL, 1),
    (N'S?n?tate public? ?i management', N'Laborator ?i Diagnostic', NULL, 1);

-- Specializari Stomatologice
INSERT INTO dbo.Specializari ([Denumire], [Categorie], [Descriere], [Este_Activ])
VALUES 
    (N'Chirurgie dento-alveolar?', N'Stomatologie', NULL, 1),
    (N'Ortodon?ie ?i ortopedie dento-facial?', N'Stomatologie', NULL, 1),
    (N'Endodon?ie', N'Stomatologie', NULL, 1),
    (N'Parodontologie', N'Stomatologie', NULL, 1),
    (N'Pedodon?ie', N'Stomatologie', NULL, 1),
    (N'Protetic? dentar?', N'Stomatologie', NULL, 1),
    (N'Chirurgie stomatologic? ?i maxilo-facial?', N'Stomatologie', NULL, 1),
    (N'Stomatologie general?', N'Stomatologie', NULL, 1);

-- Specializari Farmaceutice
INSERT INTO dbo.Specializari ([Denumire], [Categorie], [Descriere], [Este_Activ])
VALUES 
    (N'Farmacie clinic?', N'Farmaceutic?', NULL, 1),
    (N'Analize medico-farmaceutice de laborator', N'Farmaceutic?', NULL, 1),
    (N'Farmacie general?', N'Farmaceutic?', NULL, 1),
    (N'Industrie farmaceutic? ?i cosmetic?', N'Farmaceutic?', NULL, 1);

DECLARE @NumarSpecializari INT = (SELECT COUNT(*) FROM Specializari);
PRINT '   ? Date inserate: ' + CAST(@NumarSpecializari AS VARCHAR) + ' specializari';
PRINT '';

-- ============================================================================
-- STEP 2: Creare Stored Procedures (Principalele)
-- ============================================================================
PRINT 'STEP 2: Creare stored procedures...';
PRINT '';

-- Pentru instalare rapida, cream doar SP-urile esentiale
-- SP-urile complete sunt in sp_Specializari.sql

-- 1. sp_Specializari_GetAll
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Specializari_GetAll')
    DROP PROCEDURE sp_Specializari_GetAll

EXEC('
CREATE PROCEDURE sp_Specializari_GetAll
    @PageNumber INT = 1,
    @PageSize INT = 100,
    @SearchText NVARCHAR(255) = NULL,
    @Categorie NVARCHAR(100) = NULL,
    @EsteActiv BIT = NULL,
    @SortColumn NVARCHAR(50) = ''Denumire'',
    @SortDirection NVARCHAR(4) = ''ASC''
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @SortDirection NOT IN (''ASC'', ''DESC'')
        SET @SortDirection = ''ASC'';
    
    IF @SortColumn NOT IN (''Denumire'', ''Categorie'', ''Data_Crearii'')
        SET @SortColumn = ''Denumire'';
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        Id, Denumire, Categorie, Descriere, Este_Activ,
        Data_Crearii, Data_Ultimei_Modificari,
        Creat_De, Modificat_De
    FROM Specializari
    WHERE 1=1
        AND (@SearchText IS NULL OR Denumire LIKE ''%'' + @SearchText + ''%'')
        AND (@Categorie IS NULL OR Categorie = @Categorie)
        AND (@EsteActiv IS NULL OR Este_Activ = @EsteActiv)
    ORDER BY 
        CASE WHEN @SortColumn = ''Denumire'' AND @SortDirection = ''ASC'' THEN Denumire END ASC,
        CASE WHEN @SortColumn = ''Denumire'' AND @SortDirection = ''DESC'' THEN Denumire END DESC,
        CASE WHEN @SortColumn = ''Categorie'' AND @SortDirection = ''ASC'' THEN Categorie END ASC,
        CASE WHEN @SortColumn = ''Categorie'' AND @SortDirection = ''DESC'' THEN Categorie END DESC
    OFFSET @Offset ROWS 
    FETCH NEXT @PageSize ROWS ONLY;
END
')
PRINT '   ? sp_Specializari_GetAll';

-- 2. sp_Specializari_GetById
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Specializari_GetById')
    DROP PROCEDURE sp_Specializari_GetById

EXEC('
CREATE PROCEDURE sp_Specializari_GetById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, Denumire, Categorie, Descriere, Este_Activ,
        Data_Crearii, Data_Ultimei_Modificari,
        Creat_De, Modificat_De
    FROM Specializari 
    WHERE Id = @Id;
END
')
PRINT '   ? sp_Specializari_GetById';

-- 3. sp_Specializari_GetDropdownOptions
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Specializari_GetDropdownOptions')
    DROP PROCEDURE sp_Specializari_GetDropdownOptions

EXEC('
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
')
PRINT '   ? sp_Specializari_GetDropdownOptions';

-- 4. sp_Specializari_GetCategorii
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Specializari_GetCategorii')
    DROP PROCEDURE sp_Specializari_GetCategorii

EXEC('
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
')
PRINT '   ? sp_Specializari_GetCategorii';

DECLARE @NumarSP INT = (SELECT COUNT(*) FROM sys.procedures WHERE name LIKE 'sp_Specializari_%');
PRINT '   ? Total SP-uri create: ' + CAST(@NumarSP AS VARCHAR);
PRINT '   ? Pentru SP-uri complete, rulati: sp_Specializari.sql';
PRINT '';

-- ============================================================================
-- STEP 3: Verificare Finala
-- ============================================================================
PRINT 'STEP 3: Verificare finala...';
PRINT '';

DECLARE @Verificari TABLE (Test VARCHAR(100), Status VARCHAR(10));

-- Verificare tabel
INSERT INTO @Verificari
SELECT 'Tabel Specializari', CASE WHEN OBJECT_ID('dbo.Specializari', 'U') IS NOT NULL THEN '? OK' ELSE '? EROARE' END;

-- Verificare date
INSERT INTO @Verificari
SELECT 'Date populate', CASE WHEN (SELECT COUNT(*) FROM Specializari) >= 66 THEN '? OK' ELSE '? EROARE' END;

-- Verificare SP-uri
INSERT INTO @Verificari
SELECT 'Stored Procedures', CASE WHEN (SELECT COUNT(*) FROM sys.procedures WHERE name LIKE 'sp_Specializari_%') >= 4 THEN '? OK' ELSE '? EROARE' END;

-- Verificare indexuri
INSERT INTO @Verificari
SELECT 'Indexuri', CASE WHEN (SELECT COUNT(*) FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Specializari') AND type > 0) >= 4 THEN '? OK' ELSE '? EROARE' END;

-- Verificare trigger
INSERT INTO @Verificari
SELECT 'Trigger', CASE WHEN EXISTS (SELECT 1 FROM sys.triggers WHERE parent_id = OBJECT_ID('dbo.Specializari')) THEN '? OK' ELSE '? EROARE' END;

-- Afisare rezultate
SELECT * FROM @Verificari;

-- Afisare statistici pe categorii
PRINT '';
PRINT 'Statistici specializari pe categorii:';
SELECT 
    Categorie,
    COUNT(*) AS Numar
FROM Specializari
GROUP BY Categorie
ORDER BY Categorie;

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
PRINT 'INSTALARE TABELA SPECIALIZARI - FINALIZAT';
PRINT '========================================';
GO
