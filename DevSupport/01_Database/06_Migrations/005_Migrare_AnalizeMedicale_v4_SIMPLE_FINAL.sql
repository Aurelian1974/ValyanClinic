-- ========================================
-- MIGRARE ANALIZE MEDICALE - v4 FINAL SIMPLE
-- Strategie: Tabele noi în HelperValyanMed cu GUID, apoi INSERT direct
-- Database: HelperValyanMed (pentru preg?tire) + ValyanMed (pentru import final)
-- ========================================

-- ========================================
-- PARTEA 1: RULEAZ? ÎN HelperValyanMed
-- ========================================
USE [HelperValyanMed]
GO

PRINT '========================================';
PRINT 'PARTEA 1: CREARE TABELE CU GUID ÎN HelperValyanMed';
PRINT '========================================';
GO

-- Drop tabele noi dac? exist?
IF OBJECT_ID('dbo.AnalizeMedicale_New', 'U') IS NOT NULL DROP TABLE dbo.AnalizeMedicale_New;
IF OBJECT_ID('dbo.AnalizeMedicaleCategorii_New', 'U') IS NOT NULL DROP TABLE dbo.AnalizeMedicaleCategorii_New;
IF OBJECT_ID('dbo.AnalizeMedicaleLaboratoare_New', 'U') IS NOT NULL DROP TABLE dbo.AnalizeMedicaleLaboratoare_New;
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

-- Migrare date laboratoare
INSERT INTO dbo.AnalizeMedicaleLaboratoare_New (
    LaboratorID, OldLaboratorID, NumeLaborator, Acronim, Adresa, Judet, Localitate,
    Telefon, Email, Website, Acreditare, EsteActiv
)
SELECT 
    NEWID(),
    LaboratorID,
    NumeLaborator,
    Acronim,
    Adresa,
    Judet,
    Localitate,
    Telefon,
    Email,
    Website,
    Acreditare,
    ISNULL(Activ, 1)
FROM dbo.Laboratoare;

PRINT '  ? Laboratoare migrate: ' + CAST(@@ROWCOUNT AS VARCHAR);
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

-- Migrare date categorii cu iconi?e
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

-- Migrare date analize cu JOIN pe OldID
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

-- Verificare date în HelperValyanMed
PRINT '';
PRINT 'VERIFICARE HelperValyanMed:';
SELECT 'Laboratoare_New' AS Tabel, COUNT(*) AS Total FROM dbo.AnalizeMedicaleLaboratoare_New
UNION ALL SELECT 'Categorii_New', COUNT(*) FROM dbo.AnalizeMedicaleCategorii_New
UNION ALL SELECT 'Analize_New', COUNT(*) FROM dbo.AnalizeMedicale_New;
GO

PRINT '';
PRINT '? PARTEA 1 COMPLET? - Date preg?tite în HelperValyanMed';
PRINT '';
PRINT '========================================';
PRINT 'ACUM RULEAZ? PARTEA 2 (SCHIMB? DATABASE LA ValyanMed)';
PRINT '========================================';
GO

-- ========================================
-- PARTEA 2: RULEAZ? ÎN ValyanMed (IMPORT FINAL)
-- ========================================
USE [ValyanMed]
GO

PRINT '';
PRINT '========================================';
PRINT 'PARTEA 2: IMPORT FINAL ÎN ValyanMed';
PRINT '========================================';
GO

-- Drop tabele existente în ValyanMed
IF OBJECT_ID('dbo.AnalizeMedicale', 'U') IS NOT NULL DROP TABLE dbo.AnalizeMedicale;
IF OBJECT_ID('dbo.AnalizeMedicaleCategorii', 'U') IS NOT NULL DROP TABLE dbo.AnalizeMedicaleCategorii;
IF OBJECT_ID('dbo.AnalizeMedicaleLaboratoare', 'U') IS NOT NULL DROP TABLE dbo.AnalizeMedicaleLaboratoare;
GO

-- ========================================
-- CREARE TABELE FINALE ÎN ValyanMed
-- ========================================

-- Tabela Laboratoare (f?r? OldID - nu mai e necesar)
CREATE TABLE dbo.AnalizeMedicaleLaboratoare (
    [LaboratorID] UNIQUEIDENTIFIER NOT NULL,
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
    CONSTRAINT [PK_AnalizeMedicaleLaboratoare] PRIMARY KEY ([LaboratorID])
);
GO

-- Tabela Categorii
CREATE TABLE dbo.AnalizeMedicaleCategorii (
    [CategorieID] UNIQUEIDENTIFIER NOT NULL,
    [NumeCategorie] NVARCHAR(100) NOT NULL,
    [Descriere] NVARCHAR(500) NULL,
    [Icon] NVARCHAR(50) NULL,
    [OrdineAfisare] INT NOT NULL DEFAULT 0,
    [EsteActiv] BIT NOT NULL DEFAULT 1,
    [DataCreare] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_AnalizeMedicaleCategorii] PRIMARY KEY ([CategorieID])
);
GO

-- Tabela Analize
CREATE TABLE dbo.AnalizeMedicale (
    [AnalizaID] UNIQUEIDENTIFIER NOT NULL,
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
    CONSTRAINT [PK_AnalizeMedicale] PRIMARY KEY ([AnalizaID]),
    CONSTRAINT [FK_AnalizeMedicale_Laborator] FOREIGN KEY ([LaboratorID])
        REFERENCES dbo.AnalizeMedicaleLaboratoare ([LaboratorID]),
    CONSTRAINT [FK_AnalizeMedicale_Categorie] FOREIGN KEY ([CategorieID])
        REFERENCES dbo.AnalizeMedicaleCategorii ([CategorieID])
);
GO

-- Indexes
CREATE NONCLUSTERED INDEX [IX_AnalizeMedicale_Laborator] ON dbo.AnalizeMedicale ([LaboratorID]);
CREATE NONCLUSTERED INDEX [IX_AnalizeMedicale_Categorie] ON dbo.AnalizeMedicale ([CategorieID]);
CREATE NONCLUSTERED INDEX [IX_AnalizeMedicale_Nume] ON dbo.AnalizeMedicale ([NumeAnaliza]);
GO

PRINT 'Tabele create în ValyanMed';
GO

-- ========================================
-- IMPORT SIMPLU CU INSERT...SELECT
-- ========================================

-- Import Laboratoare
INSERT INTO dbo.AnalizeMedicaleLaboratoare (
    LaboratorID, NumeLaborator, Acronim, Adresa, Judet, Localitate,
    Telefon, Email, Website, Acreditare, EsteActiv, DataCreare, DataUltimaActualizare
)
SELECT 
    LaboratorID, NumeLaborator, Acronim, Adresa, Judet, Localitate,
    Telefon, Email, Website, Acreditare, EsteActiv, DataCreare, DataUltimaActualizare
FROM [HelperValyanMed].dbo.AnalizeMedicaleLaboratoare_New;

PRINT '  ? Laboratoare importate: ' + CAST(@@ROWCOUNT AS VARCHAR);
GO

-- Import Categorii
INSERT INTO dbo.AnalizeMedicaleCategorii (
    CategorieID, NumeCategorie, Descriere, Icon, OrdineAfisare, EsteActiv, DataCreare
)
SELECT 
    CategorieID, NumeCategorie, Descriere, Icon, OrdineAfisare, EsteActiv, DataCreare
FROM [HelperValyanMed].dbo.AnalizeMedicaleCategorii_New;

PRINT '  ? Categorii importate: ' + CAST(@@ROWCOUNT AS VARCHAR);
GO

-- Import Analize
INSERT INTO dbo.AnalizeMedicale (
    AnalizaID, LaboratorID, CategorieID, CodAnaliza, NumeAnaliza, NumeScurt,
    Acronime, Descriere, PreparareaTestului, Material, TermenProcesare,
    Pret, Moneda, PretActualizatLa, URLSursa, EsteActiv, DataScraping,
    DataCreare, DataUltimaActualizare
)
SELECT 
    AnalizaID, LaboratorID, CategorieID, CodAnaliza, NumeAnaliza, NumeScurt,
    Acronime, Descriere, PreparareaTestului, Material, TermenProcesare,
    Pret, Moneda, PretActualizatLa, URLSursa, EsteActiv, DataScraping,
    DataCreare, DataUltimaActualizare
FROM [HelperValyanMed].dbo.AnalizeMedicale_New;

PRINT '  ? Analize importate: ' + CAST(@@ROWCOUNT AS VARCHAR);
GO

-- ========================================
-- VERIFICARE FINAL?
-- ========================================
PRINT '';
PRINT 'STATISTICI FINALE ValyanMed:';
SELECT 'Laboratoare' AS Tabel, COUNT(*) AS Total FROM dbo.AnalizeMedicaleLaboratoare
UNION ALL SELECT 'Categorii', COUNT(*) FROM dbo.AnalizeMedicaleCategorii
UNION ALL SELECT 'Analize', COUNT(*) FROM dbo.AnalizeMedicale;
GO

-- Sample date
PRINT '';
PRINT 'Sample analize migrate (5 random):';
SELECT TOP 5
    a.NumeAnaliza,
    c.NumeCategorie,
    l.NumeLaborator,
    CAST(a.Pret AS VARCHAR) + ' ' + a.Moneda AS Pret
FROM dbo.AnalizeMedicale a
INNER JOIN dbo.AnalizeMedicaleCategorii c ON a.CategorieID = c.CategorieID
INNER JOIN dbo.AnalizeMedicaleLaboratoare l ON a.LaboratorID = l.LaboratorID
WHERE a.EsteActiv = 1
ORDER BY NEWID();
GO

PRINT '';
PRINT '========================================';
PRINT '? MIGRARE COMPLET? CU SUCCES!';
PRINT '========================================';
PRINT '';
PRINT 'CLEANUP (OP?IONAL):';
PRINT '  Po?i ?terge tabelele _New din HelperValyanMed:';
PRINT '  DROP TABLE HelperValyanMed.dbo.AnalizeMedicale_New;';
PRINT '  DROP TABLE HelperValyanMed.dbo.AnalizeMedicaleCategorii_New;';
PRINT '  DROP TABLE HelperValyanMed.dbo.AnalizeMedicaleLaboratoare_New;';
PRINT '';
GO
