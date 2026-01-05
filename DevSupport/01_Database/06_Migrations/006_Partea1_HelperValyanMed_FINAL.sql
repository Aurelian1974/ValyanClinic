-- ========================================
-- MIGRARE ANALIZE MEDICALE - FINAL CORECT
-- Partea 1: HelperValyanMed (creare tabele cu GUID)
-- ========================================
USE [HelperValyanMed]
GO

PRINT '========================================';
PRINT 'MIGRARE ANALIZE MEDICALE - PARTEA 1';
PRINT 'Database: HelperValyanMed';
PRINT 'Datele sursa: INT ? Destinatie: UNIQUEIDENTIFIER';
PRINT '========================================';
GO

-- Cleanup tabele existente
IF OBJECT_ID('dbo.AnalizeMedicale_New', 'U') IS NOT NULL DROP TABLE dbo.AnalizeMedicale_New;
IF OBJECT_ID('dbo.AnalizeMedicaleCategorii_New', 'U') IS NOT NULL DROP TABLE dbo.AnalizeMedicaleCategorii_New;
IF OBJECT_ID('dbo.AnalizeMedicaleLaboratoare_New', 'U') IS NOT NULL DROP TABLE dbo.AnalizeMedicaleLaboratoare_New;
GO

-- ========================================
-- [1/3] LABORATOARE
-- ========================================
PRINT '[1/3] Creare tabel AnalizeMedicaleLaboratoare_New...';

CREATE TABLE dbo.AnalizeMedicaleLaboratoare_New (
    [LaboratorID] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [OldLaboratorID] INT NOT NULL, -- Pentru mapare cu AnalizeLaborator
    [NumeLaborator] NVARCHAR(200) NOT NULL,
    [Acronim] NVARCHAR(50) NULL,
    [Adresa] NVARCHAR(500) NULL,
    [Localitate] NVARCHAR(100) NULL,
    [Telefon] NVARCHAR(20) NULL,
    [Email] NVARCHAR(100) NULL,
    [Website] NVARCHAR(200) NULL,
    [EsteActiv] BIT NOT NULL DEFAULT 1,
    [DataCreare] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [DataUltimaActualizare] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_AnalizeMedicaleLaboratoare_New] PRIMARY KEY ([LaboratorID])
);

PRINT '  Inserare date din Laboratoare (schema: LaboratorID INT)...';

INSERT INTO dbo.AnalizeMedicaleLaboratoare_New (
    LaboratorID, 
    OldLaboratorID, 
    NumeLaborator, 
    Acronim, 
    Adresa, 
    Localitate,
    Telefon, 
    Email, 
    Website, 
    EsteActiv,
    DataCreare,
    DataUltimaActualizare
)
SELECT 
    NEWID() AS LaboratorID,
    LaboratorID AS OldLaboratorID,
    NumeLaborator,
    NumeScurt AS Acronim,
    Adresa,
    Localitate,
    Telefon,
    Email,
    Website,
    ISNULL(Activ, 1) AS EsteActiv,
    ISNULL(DataAdaugare, GETDATE()) AS DataCreare,
    ISNULL(DataActualizare, GETDATE()) AS DataUltimaActualizare
FROM dbo.Laboratoare;

DECLARE @LabCount INT = @@ROWCOUNT;
PRINT '  ? Migrate: ' + CAST(@LabCount AS VARCHAR) + ' laboratoare';
GO

-- ========================================
-- [2/3] CATEGORII
-- ========================================
PRINT '[2/3] Creare tabel AnalizeMedicaleCategorii_New...';

CREATE TABLE dbo.AnalizeMedicaleCategorii_New (
    [CategorieID] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [OldCategorieID] INT NOT NULL, -- Pentru mapare
    [NumeCategorie] NVARCHAR(100) NOT NULL,
    [Descriere] NVARCHAR(500) NULL,
    [Icon] NVARCHAR(50) NULL,
    [OrdineAfisare] INT NOT NULL DEFAULT 0,
    [EsteActiv] BIT NOT NULL DEFAULT 1,
    [DataCreare] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_AnalizeMedicaleCategorii_New] PRIMARY KEY ([CategorieID])
);

PRINT '  Inserare date din Categorii (schema: CategorieID INT)...';

INSERT INTO dbo.AnalizeMedicaleCategorii_New (
    CategorieID,
    OldCategorieID,
    NumeCategorie,
    Descriere,
    Icon,
    OrdineAfisare,
    EsteActiv,
    DataCreare
)
SELECT 
    NEWID() AS CategorieID,
    CategorieID AS OldCategorieID,
    NumeCategorie,
    Descriere,
    CASE UPPER(LTRIM(RTRIM(NumeCategorie)))
        WHEN 'HEMATOLOGIE' THEN 'fa-tint'
        WHEN 'BIOCHIMIE' THEN 'fa-flask'
        WHEN 'IMUNOLOGIE' THEN 'fa-shield-virus'
        WHEN 'SEROLOGIE' THEN 'fa-syringe'
        WHEN 'COAGULARE' THEN 'fa-droplet'
        WHEN 'HORMONI' THEN 'fa-brain'
        WHEN 'MARKERI TUMORALI' THEN 'fa-dna'
        WHEN 'URINA' THEN 'fa-toilet'
        WHEN 'GLICEMIE' THEN 'fa-candy-cane'
        WHEN 'LIPIDE' THEN 'fa-heart'
        ELSE 'fa-vial'
    END AS Icon,
    ISNULL(Ordine, 0) AS OrdineAfisare,
    ISNULL(Activ, 1) AS EsteActiv,
    ISNULL(DataAdaugare, GETDATE()) AS DataCreare
FROM dbo.Categorii;

DECLARE @CatCount INT = @@ROWCOUNT;
PRINT '  ? Migrate: ' + CAST(@CatCount AS VARCHAR) + ' categorii';
GO

-- ========================================
-- [3/3] ANALIZE
-- ========================================
PRINT '[3/3] Creare tabel AnalizeMedicale_New...';
PRINT '  Acest pas poate dura 30-60 secunde pentru 2649 analize...';

CREATE TABLE dbo.AnalizeMedicale_New (
    [AnalizaID] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [LaboratorID] UNIQUEIDENTIFIER NOT NULL,
    [CategorieID] UNIQUEIDENTIFIER NOT NULL,
    [CodAnaliza] NVARCHAR(50) NULL,
    [NumeAnaliza] NVARCHAR(400) NOT NULL,
    [NumeScurt] NVARCHAR(150) NULL,
    [Acronime] NVARCHAR(100) NULL,
    [Descriere] NVARCHAR(MAX) NULL,
    [PreparareaTestului] NVARCHAR(MAX) NULL,
    [Material] NVARCHAR(200) NULL,
    [TermenProcesare] NVARCHAR(100) NULL,
    [Pret] DECIMAL(10, 2) NULL,
    [Moneda] NVARCHAR(10) NULL DEFAULT 'RON',
    [PretActualizatLa] DATE NULL,
    [URLSursa] NVARCHAR(500) NULL,
    [EsteActiv] BIT NOT NULL DEFAULT 1,
    [DataScraping] DATETIME2 NULL,
    [DataCreare] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [DataUltimaActualizare] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_AnalizeMedicale_New] PRIMARY KEY ([AnalizaID])
);

PRINT '  Inserare date din AnalizeLaborator (JOIN cu mapare INT?GUID)...';

INSERT INTO dbo.AnalizeMedicale_New (
    AnalizaID,
    LaboratorID,
    CategorieID,
    CodAnaliza,
    NumeAnaliza,
    NumeScurt,
    Acronime,
    Descriere,
    PreparareaTestului,
    Material,
    TermenProcesare,
    Pret,
    Moneda,
    PretActualizatLa,
    URLSursa,
    EsteActiv,
    DataScraping,
    DataCreare,
    DataUltimaActualizare
)
SELECT 
    NEWID() AS AnalizaID,
    ln.LaboratorID, -- ? GUID din mapare
    cn.CategorieID, -- ? GUID din mapare
    a.CodAnaliza,
    a.NumeAnaliza,
    a.NumeScurt,
    a.Acronime,
    a.Descriere,
    a.PreparareaTestului,
    a.Material,
    a.TermenProcesare,
    a.Pret,
    ISNULL(a.Moneda, 'RON') AS Moneda,
    a.PretActualizatLa,
    a.URLSursa,
    ISNULL(a.Activ, 1) AS EsteActiv,
    a.DataScraping,
    ISNULL(a.DataActualizare, GETDATE()) AS DataCreare,
    ISNULL(a.DataActualizare, GETDATE()) AS DataUltimaActualizare
FROM dbo.AnalizeLaborator a
INNER JOIN dbo.AnalizeMedicaleLaboratoare_New ln ON a.LaboratorID = ln.OldLaboratorID
INNER JOIN dbo.AnalizeMedicaleCategorii_New cn ON a.CategorieID = cn.OldCategorieID
WHERE ISNULL(a.Activ, 1) = 1; -- Doar analize active

DECLARE @AnalCount INT = @@ROWCOUNT;
PRINT '  ? Migrate: ' + CAST(@AnalCount AS VARCHAR) + ' analize active';
GO

-- ========================================
-- VERIFICARE
-- ========================================
PRINT '';
PRINT '========================================';
PRINT 'VERIFICARE DATE ÎN HelperValyanMed:';
PRINT '========================================';

SELECT 
    'Laboratoare_New' AS Tabel, 
    COUNT(*) AS Total,
    SUM(CASE WHEN EsteActiv = 1 THEN 1 ELSE 0 END) AS Active
FROM dbo.AnalizeMedicaleLaboratoare_New
UNION ALL
SELECT 
    'Categorii_New', 
    COUNT(*),
    SUM(CASE WHEN EsteActiv = 1 THEN 1 ELSE 0 END)
FROM dbo.AnalizeMedicaleCategorii_New
UNION ALL
SELECT 
    'Analize_New', 
    COUNT(*),
    SUM(CASE WHEN EsteActiv = 1 THEN 1 ELSE 0 END)
FROM dbo.AnalizeMedicale_New;
GO

-- Sample date pentru validare
PRINT '';
PRINT 'Sample analize migrate (5 random):';
SELECT TOP 5
    a.NumeAnaliza,
    c.NumeCategorie,
    l.NumeLaborator,
    CAST(a.Pret AS VARCHAR) + ' ' + a.Moneda AS Pret
FROM dbo.AnalizeMedicale_New a
INNER JOIN dbo.AnalizeMedicaleCategorii_New c ON a.CategorieID = c.CategorieID
INNER JOIN dbo.AnalizeMedicaleLaboratoare_New l ON a.LaboratorID = l.LaboratorID
ORDER BY NEWID();
GO

PRINT '';
PRINT '========================================';
PRINT '? PARTEA 1 COMPLET?!';
PRINT '========================================';
PRINT '';
PRINT 'Date preg?tite în HelperValyanMed cu GUID.';
PRINT 'NEXT STEP: Ruleaz? Partea 2 (import în ValyanMed)';
PRINT '';
GO
