-- ========================================
-- PARTEA 1: CREARE ?I POPULARE TABELE ÎN HelperValyanMed
-- Ruleaz? DOAR pe HelperValyanMed
-- ========================================
USE [HelperValyanMed]
GO

PRINT '========================================';
PRINT 'PARTEA 1: CREARE TABELE CU GUID';
PRINT 'Database: HelperValyanMed';
PRINT '========================================';
GO

-- Drop tabele _New dac? exist?
IF OBJECT_ID('dbo.AnalizeMedicale_New', 'U') IS NOT NULL DROP TABLE dbo.AnalizeMedicale_New;
IF OBJECT_ID('dbo.AnalizeMedicaleCategorii_New', 'U') IS NOT NULL DROP TABLE dbo.AnalizeMedicaleCategorii_New;
IF OBJECT_ID('dbo.AnalizeMedicaleLaboratoare_New', 'U') IS NOT NULL DROP TABLE dbo.AnalizeMedicaleLaboratoare_New;
GO

PRINT '[1/3] Creare tabel Laboratoare_New...';
GO

-- ========================================
-- TABEL: AnalizeMedicaleLaboratoare_New
-- ========================================
CREATE TABLE dbo.AnalizeMedicaleLaboratoare_New (
    [LaboratorID] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [OldLaboratorID] INT NOT NULL,
    [NumeLaborator] NVARCHAR(200) NOT NULL,
    [Acronim] NVARCHAR(50) NULL,
    [Adresa] NVARCHAR(500) NULL,
    [Judet] NVARCHAR(50) NULL,
    [Localitate] NVARCHAR(100) NULL,
    [Telefon] NVARCHAR(20) NULL,
    [Email] NVARCHAR(100) NULL,
    [Website] NVARCHAR(200) NULL,
    [Acreditare] NVARCHAR(100) NULL,
    [EsteActiv] BIT NOT NULL DEFAULT 1,
    [DataCreare] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [DataUltimaActualizare] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_AnalizeMedicaleLaboratoare_New] PRIMARY KEY ([LaboratorID])
);
GO

PRINT '  Tabel creat. Migrare date...';

INSERT INTO dbo.AnalizeMedicaleLaboratoare_New (
    LaboratorID, OldLaboratorID, NumeLaborator, Acronim, Adresa, Judet, Localitate,
    Telefon, Email, Website, Acreditare, EsteActiv
)
SELECT 
    NEWID(),
    LaboratorID,
    NumeLaborator,
    NumeScurt AS Acronim, -- ? FIX: Mapare NumeScurt ? Acronim
    Adresa,
    NULL AS Judet, -- ? FIX: Coloan? lips? în surs?
    Localitate,
    Telefon,
    Email,
    Website,
    NULL AS Acreditare, -- ? FIX: Coloan? lips? în surs?
    ISNULL(Activ, 1)
FROM dbo.Laboratoare;

PRINT '  ? Laboratoare migrate: ' + CAST(@@ROWCOUNT AS VARCHAR);
GO

PRINT '[2/3] Creare tabel Categorii_New...';
GO

-- ========================================
-- TABEL: AnalizeMedicaleCategorii_New
-- ========================================
CREATE TABLE dbo.AnalizeMedicaleCategorii_New (
    [CategorieID] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [OldCategorieID] INT NOT NULL,
    [NumeCategorie] NVARCHAR(100) NOT NULL,
    [Descriere] NVARCHAR(500) NULL,
    [Icon] NVARCHAR(50) NULL,
    [OrdineAfisare] INT NOT NULL DEFAULT 0,
    [EsteActiv] BIT NOT NULL DEFAULT 1,
    [DataCreare] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_AnalizeMedicaleCategorii_New] PRIMARY KEY ([CategorieID])
);
GO

PRINT '  Tabel creat. Migrare date...';

INSERT INTO dbo.AnalizeMedicaleCategorii_New (
    CategorieID, OldCategorieID, NumeCategorie, Descriere, Icon, OrdineAfisare, EsteActiv
)
SELECT 
    NEWID(),
    CategorieID,
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
        ELSE 'fa-vial'
    END,
    ISNULL(OrdineAfisare, 0),
    ISNULL(Activ, 1)
FROM dbo.Categorii;

PRINT '  ? Categorii migrate: ' + CAST(@@ROWCOUNT AS VARCHAR);
GO

PRINT '[3/3] Creare tabel Analize_New...';
GO

-- ========================================
-- TABEL: AnalizeMedicale_New
-- ========================================
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
GO

PRINT '  Tabel creat. Migrare date (2649 analize)...';
PRINT '  Acest pas poate dura 10-30 secunde...';

INSERT INTO dbo.AnalizeMedicale_New (
    AnalizaID, LaboratorID, CategorieID, CodAnaliza, NumeAnaliza, NumeScurt,
    Acronime, Descriere, PreparareaTestului, Material, TermenProcesare,
    Pret, Moneda, PretActualizatLa, URLSursa, EsteActiv, DataScraping
)
SELECT 
    NEWID(),
    ln.LaboratorID,
    cn.CategorieID,
    a.CodAnaliza,
    a.NumeAnaliza,
    a.NumeScurt,
    a.Acronime,
    a.Descriere,
    a.PreparareaTestului,
    a.Material,
    a.TermenProcesare,
    a.Pret,
    ISNULL(a.Moneda, 'RON'),
    a.PretActualizatLa,
    a.URLSursa,
    ISNULL(a.Activ, 1),
    a.DataScraping
FROM dbo.AnalizeLaborator a
INNER JOIN dbo.AnalizeMedicaleLaboratoare_New ln ON a.LaboratorID = ln.OldLaboratorID
INNER JOIN dbo.AnalizeMedicaleCategorii_New cn ON a.CategorieID = cn.OldCategorieID
WHERE ISNULL(a.Activ, 1) = 1;

PRINT '  ? Analize migrate: ' + CAST(@@ROWCOUNT AS VARCHAR);
GO

-- ========================================
-- VERIFICARE FINAL?
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

-- Sample date
PRINT '';
PRINT 'Sample analize migrate (5 random):';
SELECT TOP 5
    a.NumeAnaliza,
    c.NumeCategorie,
    l.NumeLaborator
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
PRINT 'NEXT: Ruleaz? scriptul PARTEA 2 pe ValyanMed';
PRINT '';
GO
