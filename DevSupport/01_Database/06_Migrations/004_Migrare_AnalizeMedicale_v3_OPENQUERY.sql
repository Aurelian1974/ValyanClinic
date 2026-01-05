-- ========================================
-- MIGRARE ANALIZE MEDICALE - v3 SIMPLIFIED
-- Strategie: OPENQUERY pentru linked server
-- ========================================

USE [ValyanMed]
GO

PRINT '========================================';
PRINT 'MIGRARE ANALIZE MEDICALE v3';
PRINT '========================================';
GO

-- Drop tabele existente
IF OBJECT_ID('AnalizeMedicaleIstoricScraping', 'U') IS NOT NULL DROP TABLE AnalizeMedicaleIstoricScraping;
IF OBJECT_ID('AnalizeMedicale', 'U') IS NOT NULL DROP TABLE AnalizeMedicale;
IF OBJECT_ID('AnalizeMedicaleCategorii', 'U') IS NOT NULL DROP TABLE AnalizeMedicaleCategorii;
IF OBJECT_ID('AnalizeMedicaleLaboratoare', 'U') IS NOT NULL DROP TABLE AnalizeMedicaleLaboratoare;
GO

-- ========================================
-- TABEL: AnalizeMedicaleLaboratoare
-- ========================================
CREATE TABLE dbo.AnalizeMedicaleLaboratoare (
    [LaboratorID] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [OldLaboratorID] INT NOT NULL, -- ? P?STR?M ID vechi pentru mapare
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
    
    CONSTRAINT [PK_AnalizeMedicaleLaboratoare] PRIMARY KEY ([LaboratorID]),
    CONSTRAINT [UQ_AnalizeMedicaleLaboratoare_OldID] UNIQUE ([OldLaboratorID])
);
GO

-- ========================================
-- TABEL: AnalizeMedicaleCategorii
-- ========================================
CREATE TABLE dbo.AnalizeMedicaleCategorii (
    [CategorieID] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [OldCategorieID] INT NOT NULL, -- ? P?STR?M ID vechi
    [NumeCategorie] NVARCHAR(100) NOT NULL,
    [Descriere] NVARCHAR(500) NULL,
    [Icon] NVARCHAR(50) NULL,
    [OrdineAfisare] INT NOT NULL DEFAULT 0,
    [EsteActiv] BIT NOT NULL DEFAULT 1,
    [DataCreare] DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT [PK_AnalizeMedicaleCategorii] PRIMARY KEY ([CategorieID]),
    CONSTRAINT [UQ_AnalizeMedicaleCategorii_OldID] UNIQUE ([OldCategorieID])
);
GO

-- ========================================
-- TABEL: AnalizeMedicale
-- ========================================
CREATE TABLE dbo.AnalizeMedicale (
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
    
    CONSTRAINT [PK_AnalizeMedicale] PRIMARY KEY ([AnalizaID]),
    CONSTRAINT [FK_AnalizeMedicale_Laborator] FOREIGN KEY ([LaboratorID])
        REFERENCES dbo.AnalizeMedicaleLaboratoare ([LaboratorID]),
    CONSTRAINT [FK_AnalizeMedicale_Categorie] FOREIGN KEY ([CategorieID])
        REFERENCES dbo.AnalizeMedicaleCategorii ([CategorieID])
);
GO

CREATE NONCLUSTERED INDEX [IX_AnalizeMedicale_Laborator] ON dbo.AnalizeMedicale ([LaboratorID]);
CREATE NONCLUSTERED INDEX [IX_AnalizeMedicale_Categorie] ON dbo.AnalizeMedicale ([CategorieID]);
CREATE NONCLUSTERED INDEX [IX_AnalizeMedicale_Nume] ON dbo.AnalizeMedicale ([NumeAnaliza]);
GO

PRINT 'Tabele create';
GO

-- ========================================
-- MIGRARE LABORATOARE (OPENQUERY)
-- ========================================
PRINT 'Migrare Laboratoare...';

INSERT INTO dbo.AnalizeMedicaleLaboratoare (
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
FROM OPENQUERY([DESKTOP-3Q8HI82\ERP], 
    'SELECT LaboratorID, NumeLaborator, Acronim, Adresa, Judet, Localitate, 
            Telefon, Email, Website, Acreditare, Activ 
     FROM HelperValyanMed.dbo.Laboratoare');

PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR) + ' laboratoare';
GO

-- ========================================
-- MIGRARE CATEGORII (OPENQUERY)
-- ========================================
PRINT 'Migrare Categorii...';

INSERT INTO dbo.AnalizeMedicaleCategorii (
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
        ELSE 'fa-vial'
    END,
    ISNULL(OrdineAfisare, 0),
    ISNULL(Activ, 1)
FROM OPENQUERY([DESKTOP-3Q8HI82\ERP],
    'SELECT CategorieID, NumeCategorie, Descriere, OrdineAfisare, Activ
     FROM HelperValyanMed.dbo.Categorii');

PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR) + ' categorii';
GO

-- ========================================
-- MIGRARE ANALIZE (OPENQUERY + JOIN local)
-- ========================================
PRINT 'Migrare Analize (2649)...';

INSERT INTO dbo.AnalizeMedicale (
    AnalizaID, LaboratorID, CategorieID, CodAnaliza, NumeAnaliza, NumeScurt,
    Acronime, Descriere, PreparareaTestului, Material, TermenProcesare,
    Pret, Moneda, PretActualizatLa, URLSursa, EsteActiv, DataScraping
)
SELECT 
    NEWID(),
    l.LaboratorID,
    c.CategorieID,
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
    1,
    a.DataScraping
FROM OPENQUERY([DESKTOP-3Q8HI82\ERP],
    'SELECT AnalizaID, LaboratorID, CategorieID, CodAnaliza, NumeAnaliza, NumeScurt,
            Acronime, Descriere, PreparareaTestului, Material, TermenProcesare,
            Pret, Moneda, PretActualizatLa, URLSursa, DataScraping, Activ
     FROM HelperValyanMed.dbo.AnalizeLaborator
     WHERE ISNULL(Activ, 1) = 1') a
INNER JOIN dbo.AnalizeMedicaleLaboratoare l ON a.LaboratorID = l.OldLaboratorID
INNER JOIN dbo.AnalizeMedicaleCategorii c ON a.CategorieID = c.OldCategorieID;

PRINT '  ? ' + CAST(@@ROWCOUNT AS VARCHAR) + ' analize';
GO

-- ========================================
-- VERIFICARE FINALE
-- ========================================
PRINT '';
PRINT 'STATISTICI:';
SELECT 'Laboratoare' AS Tabel, COUNT(*) AS Total FROM AnalizeMedicaleLaboratoare
UNION ALL SELECT 'Categorii', COUNT(*) FROM AnalizeMedicaleCategorii
UNION ALL SELECT 'Analize', COUNT(*) FROM AnalizeMedicale;
GO

PRINT '';
PRINT '? MIGRARE COMPLET?!';
GO
