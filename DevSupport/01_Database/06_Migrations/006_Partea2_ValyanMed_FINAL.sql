-- ========================================
-- MIGRARE ANALIZE MEDICALE - PARTEA 2
-- Import final în ValyanMed
-- ========================================
USE [ValyanMed]
GO

PRINT '========================================';
PRINT 'MIGRARE ANALIZE MEDICALE - PARTEA 2';
PRINT 'Database: ValyanMed';
PRINT 'Import din HelperValyanMed (tabele _New)';
PRINT '========================================';
GO

-- Cleanup tabele existente în ValyanMed
IF OBJECT_ID('dbo.AnalizeMedicale', 'U') IS NOT NULL DROP TABLE dbo.AnalizeMedicale;
IF OBJECT_ID('dbo.AnalizeMedicaleCategorii', 'U') IS NOT NULL DROP TABLE dbo.AnalizeMedicaleCategorii;
IF OBJECT_ID('dbo.AnalizeMedicaleLaboratoare', 'U') IS NOT NULL DROP TABLE dbo.AnalizeMedicaleLaboratoare;
GO

-- ========================================
-- CREARE TABELE ÎN ValyanMed
-- ========================================
PRINT 'Creare tabele în ValyanMed...';

-- Tabela Laboratoare (f?r? OldID)
CREATE TABLE dbo.AnalizeMedicaleLaboratoare (
    [LaboratorID] UNIQUEIDENTIFIER NOT NULL,
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
    CONSTRAINT [PK_AnalizeMedicaleLaboratoare] PRIMARY KEY ([LaboratorID])
);

-- Tabela Categorii (f?r? OldID)
CREATE TABLE dbo.AnalizeMedicaleCategorii (
    [CategorieID] UNIQUEIDENTIFIER NOT NULL,
    [NumeCategorie] NVARCHAR(100) NOT NULL,
    [Descriere] NVARCHAR(500) NULL,
    [Icon] NVARCHAR(50) NULL,
    [OrdineAfisare] INT NOT NULL DEFAULT 0,
    [EsteActiv] BIT NOT NULL DEFAULT 1,
    [DataCreare] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_AnalizeMedicaleCategorii] PRIMARY KEY ([CategorieID]),
    CONSTRAINT [UQ_AnalizeMedicaleCategorii_Nume] UNIQUE ([NumeCategorie])
);

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

-- Indexes pentru performan??
CREATE NONCLUSTERED INDEX [IX_AnalizeMedicale_Laborator] ON dbo.AnalizeMedicale ([LaboratorID]);
CREATE NONCLUSTERED INDEX [IX_AnalizeMedicale_Categorie] ON dbo.AnalizeMedicale ([CategorieID]);
CREATE NONCLUSTERED INDEX [IX_AnalizeMedicale_Nume] ON dbo.AnalizeMedicale ([NumeAnaliza]);
CREATE NONCLUSTERED INDEX [IX_AnalizeMedicale_Acronime] ON dbo.AnalizeMedicale ([Acronime]);
CREATE NONCLUSTERED INDEX [IX_AnalizeMedicale_Activ] ON dbo.AnalizeMedicale ([EsteActiv])
    WHERE [EsteActiv] = 1;

PRINT '  ? Tabele create cu succes';
GO

-- ========================================
-- IMPORT DATE DIN HelperValyanMed
-- ========================================

PRINT '';
PRINT '[1/3] Import Laboratoare...';

INSERT INTO dbo.AnalizeMedicaleLaboratoare (
    LaboratorID, NumeLaborator, Acronim, Adresa, Localitate,
    Telefon, Email, Website, EsteActiv, DataCreare, DataUltimaActualizare
)
SELECT 
    LaboratorID, -- GUID deja
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
FROM [HelperValyanMed].dbo.AnalizeMedicaleLaboratoare_New;

PRINT '  ? Importate: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' laboratoare';
GO

PRINT '[2/3] Import Categorii...';

INSERT INTO dbo.AnalizeMedicaleCategorii (
    CategorieID, NumeCategorie, Descriere, Icon, OrdineAfisare, EsteActiv, DataCreare
)
SELECT 
    CategorieID, -- GUID deja
    NumeCategorie,
    Descriere,
    Icon,
    OrdineAfisare,
    EsteActiv,
    DataCreare
FROM [HelperValyanMed].dbo.AnalizeMedicaleCategorii_New;

PRINT '  ? Importate: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' categorii';
GO

PRINT '[3/3] Import Analize (2649)...';
PRINT '  Acest pas poate dura 30-60 secunde...';

INSERT INTO dbo.AnalizeMedicale (
    AnalizaID, LaboratorID, CategorieID, CodAnaliza, NumeAnaliza, NumeScurt,
    Acronime, Descriere, PreparareaTestului, Material, TermenProcesare,
    Pret, Moneda, PretActualizatLa, URLSursa, EsteActiv, DataScraping,
    DataCreare, DataUltimaActualizare
)
SELECT 
    AnalizaID, -- GUID deja
    LaboratorID, -- GUID deja
    CategorieID, -- GUID deja
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
FROM [HelperValyanMed].dbo.AnalizeMedicale_New;

PRINT '  ? Importate: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' analize';
GO

-- ========================================
-- VERIFICARE FINAL?
-- ========================================
PRINT '';
PRINT '========================================';
PRINT 'STATISTICI FINALE ValyanMed:';
PRINT '========================================';

SELECT 
    'Laboratoare' AS Tabel, 
    COUNT(*) AS Total,
    SUM(CASE WHEN EsteActiv = 1 THEN 1 ELSE 0 END) AS Active
FROM dbo.AnalizeMedicaleLaboratoare
UNION ALL
SELECT 
    'Categorii', 
    COUNT(*),
    SUM(CASE WHEN EsteActiv = 1 THEN 1 ELSE 0 END)
FROM dbo.AnalizeMedicaleCategorii
UNION ALL
SELECT 
    'Analize', 
    COUNT(*),
    SUM(CASE WHEN EsteActiv = 1 THEN 1 ELSE 0 END)
FROM dbo.AnalizeMedicale;
GO

-- Top 10 Categorii cu cele mai multe analize
PRINT '';
PRINT 'Top 10 Categorii:';
SELECT TOP 10
    c.NumeCategorie,
    c.Icon,
    COUNT(a.AnalizaID) AS NumarAnalize
FROM dbo.AnalizeMedicaleCategorii c
LEFT JOIN dbo.AnalizeMedicale a ON c.CategorieID = a.CategorieID
GROUP BY c.NumeCategorie, c.Icon, c.OrdineAfisare
ORDER BY c.OrdineAfisare, NumarAnalize DESC;
GO

-- Sample analize
PRINT '';
PRINT 'Sample analize (5 random):';
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
PRINT '?? MIGRARE COMPLET? CU SUCCES!';
PRINT '========================================';
PRINT '';
PRINT 'Date migrate în ValyanMed:';
PRINT '  - 4 laboratoare';
PRINT '  - 19 categorii';
PRINT '  - 2649 analize medicale';
PRINT '';
PRINT 'CLEANUP (OP?IONAL):';
PRINT '  Po?i ?terge tabelele _New din HelperValyanMed:';
PRINT '  USE HelperValyanMed;';
PRINT '  DROP TABLE AnalizeMedicale_New;';
PRINT '  DROP TABLE AnalizeMedicaleCategorii_New;';
PRINT '  DROP TABLE AnalizeMedicaleLaboratoare_New;';
PRINT '';
GO
