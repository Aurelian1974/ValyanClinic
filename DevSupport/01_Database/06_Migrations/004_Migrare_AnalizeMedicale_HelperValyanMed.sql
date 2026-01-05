-- ========================================
-- MIGRARE ANALIZE MEDICALE
-- Surs?: DESKTOP-3Q8HI82\ERP.HelperValyanMed
-- Destina?ie: ValyanMed
-- Data: 2026-01-05
-- Autor: AI Assistant (Claude)
-- ========================================
--
-- IMPORTANT: Acest script face mapare INT ? UNIQUEIDENTIFIER
-- Structura HelperValyanMed:
--   - AnalizeLaborator (2649 active)
--   - Categorii (7 coloane)
--   - Laboratoare (12 coloane)
--   - IstoricScraping (8 coloane)
--
-- ========================================

USE [ValyanMed]
GO

PRINT '========================================';
PRINT 'MIGRARE ANALIZE MEDICALE';
PRINT 'Începere: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
GO

-- ========================================
-- STEP 1: CREATE TABELE DESTINA?IE
-- ========================================

-- Drop tabele existente (dac? exist?)
IF OBJECT_ID('dbo.AnalizeMedicaleIstoricScraping', 'U') IS NOT NULL
    DROP TABLE dbo.AnalizeMedicaleIstoricScraping;
IF OBJECT_ID('dbo.AnalizeMedicale', 'U') IS NOT NULL
    DROP TABLE dbo.AnalizeMedicale;
IF OBJECT_ID('dbo.AnalizeMedicaleCategorii', 'U') IS NOT NULL
    DROP TABLE dbo.AnalizeMedicaleCategorii;
IF OBJECT_ID('dbo.AnalizeMedicaleLaboratoare', 'U') IS NOT NULL
    DROP TABLE dbo.AnalizeMedicaleLaboratoare;
GO

PRINT '[1/6] Creare tabele destina?ie...';
GO

-- ========================================
-- TABEL: AnalizeMedicaleLaboratoare
-- ========================================
CREATE TABLE dbo.AnalizeMedicaleLaboratoare (
    [LaboratorID] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
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

PRINT '  ? Tabel AnalizeMedicaleLaboratoare creat';
GO

-- ========================================
-- TABEL: AnalizeMedicaleCategorii
-- ========================================
CREATE TABLE dbo.AnalizeMedicaleCategorii (
    [CategorieID] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [NumeCategorie] NVARCHAR(100) NOT NULL,
    [Descriere] NVARCHAR(500) NULL,
    [Icon] NVARCHAR(50) NULL, -- Icon FontAwesome (ex: "fa-flask")
    [OrdineAfisare] INT NOT NULL DEFAULT 0,
    [EsteActiv] BIT NOT NULL DEFAULT 1,
    [DataCreare] DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT [PK_AnalizeMedicaleCategorii] PRIMARY KEY ([CategorieID]),
    CONSTRAINT [UQ_AnalizeMedicaleCategorii_Nume] UNIQUE ([NumeCategorie])
);
GO

PRINT '  ? Tabel AnalizeMedicaleCategorii creat';
GO

-- ========================================
-- TABEL: AnalizeMedicale (NOMENCLATOR)
-- ========================================
CREATE TABLE dbo.AnalizeMedicale (
    [AnalizaID] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [LaboratorID] UNIQUEIDENTIFIER NOT NULL,
    [CategorieID] UNIQUEIDENTIFIER NOT NULL,
    
    -- Informa?ii analiz?
    [CodAnaliza] NVARCHAR(50) NULL,
    [NumeAnaliza] NVARCHAR(400) NOT NULL,
    [NumeScurt] NVARCHAR(150) NULL,
    [Acronime] NVARCHAR(100) NULL,
    [Descriere] NVARCHAR(MAX) NULL,
    
    -- Instruc?iuni
    [PreparareaTestului] NVARCHAR(MAX) NULL,
    [Material] NVARCHAR(200) NULL, -- Ex: "Sânge venos", "Urin?", etc.
    [TermenProcesare] NVARCHAR(100) NULL, -- Ex: "1-2 zile lucr?toare"
    
    -- Pre?uri
    [Pret] DECIMAL(10, 2) NULL,
    [Moneda] NVARCHAR(10) NULL DEFAULT 'RON',
    [PretActualizatLa] DATE NULL,
    
    -- Metadata
    [URLSursa] NVARCHAR(500) NULL,
    [EsteActiv] BIT NOT NULL DEFAULT 1,
    [DataScraping] DATETIME2 NULL,
    [DataCreare] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [DataUltimaActualizare] DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT [PK_AnalizeMedicale] PRIMARY KEY ([AnalizaID]),
    CONSTRAINT [FK_AnalizeMedicale_Laborator] FOREIGN KEY ([LaboratorID])
        REFERENCES dbo.AnalizeMedicaleLaboratoare ([LaboratorID]) ON DELETE CASCADE,
    CONSTRAINT [FK_AnalizeMedicale_Categorie] FOREIGN KEY ([CategorieID])
        REFERENCES dbo.AnalizeMedicaleCategorii ([CategorieID])
);
GO

-- Indexes pentru performan??
CREATE NONCLUSTERED INDEX [IX_AnalizeMedicale_Laborator] ON dbo.AnalizeMedicale ([LaboratorID]);
CREATE NONCLUSTERED INDEX [IX_AnalizeMedicale_Categorie] ON dbo.AnalizeMedicale ([CategorieID]);
CREATE NONCLUSTERED INDEX [IX_AnalizeMedicale_Nume] ON dbo.AnalizeMedicale ([NumeAnaliza]);
CREATE NONCLUSTERED INDEX [IX_AnalizeMedicale_Acronime] ON dbo.AnalizeMedicale ([Acronime]);
CREATE NONCLUSTERED INDEX [IX_AnalizeMedicale_Activ] ON dbo.AnalizeMedicale ([EsteActiv])
    WHERE [EsteActiv] = 1;
GO

PRINT '  ? Tabel AnalizeMedicale creat cu 5 indexes';
GO

-- ========================================
-- TABEL: AnalizeMedicaleIstoricScraping
-- ========================================
CREATE TABLE dbo.AnalizeMedicaleIstoricScraping (
    [IstoricID] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [LaboratorID] UNIQUEIDENTIFIER NOT NULL,
    [DataScraping] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [NumarAnalizeNoi] INT NOT NULL DEFAULT 0,
    [NumarAnalizeActualizate] INT NOT NULL DEFAULT 0,
    [NumarErori] INT NOT NULL DEFAULT 0,
    [Status] NVARCHAR(50) NOT NULL, -- "Success", "Partial", "Failed"
    [Mesaj] NVARCHAR(MAX) NULL,
    [DurataScraping] INT NULL, -- în secunde
    
    CONSTRAINT [PK_AnalizeMedicaleIstoricScraping] PRIMARY KEY ([IstoricID]),
    CONSTRAINT [FK_IstoricScraping_Laborator] FOREIGN KEY ([LaboratorID])
        REFERENCES dbo.AnalizeMedicaleLaboratoare ([LaboratorID])
);
GO

PRINT '  ? Tabel AnalizeMedicaleIstoricScraping creat';
GO

-- ========================================
-- STEP 2: CREATE TABELE MAPARE TEMPORARE
-- ========================================
PRINT '[2/6] Creare tabele mapare INT ? GUID...';
GO

-- Tabel mapare Laboratoare
CREATE TABLE #LaboratorMapping (
    OldID INT NOT NULL,
    NewID UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY (OldID)
);

-- Tabel mapare Categorii
CREATE TABLE #CategorieMapping (
    OldID INT NOT NULL,
    NewID UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY (OldID)
);

-- Tabel mapare Analize
CREATE TABLE #AnalizaMapping (
    OldID INT NOT NULL,
    NewID UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY (OldID)
);

PRINT '  ? Tabele mapare temporare create';
GO

-- ========================================
-- STEP 3: MIGRARE LABORATOARE (FIXED v2 - CTE)
-- ========================================
PRINT '[3/6] Migrare Laboratoare...';
GO

-- Creare CTE pentru laboratoare surs?
;WITH SourceLaboratoare AS (
    SELECT 
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
        Activ,
        DataCreare,
        DataActualizare
    FROM [DESKTOP-3Q8HI82\ERP].[HelperValyanMed].dbo.Laboratoare
)
INSERT INTO dbo.AnalizeMedicaleLaboratoare (
    LaboratorID, NumeLaborator, Acronim, Adresa, Judet, Localitate,
    Telefon, Email, Website, Acreditare, EsteActiv, DataCreare, DataUltimaActualizare
)
SELECT 
    NEWID() AS LaboratorID,
    NumeLaborator,
    Acronim,
    Adresa,
    Judet,
    Localitate,
    Telefon,
    Email,
    Website,
    Acreditare,
    ISNULL(Activ, 1) AS EsteActiv,
    ISNULL(DataCreare, GETDATE()) AS DataCreare,
    ISNULL(DataActualizare, GETDATE()) AS DataUltimaActualizare
FROM SourceLaboratoare;

DECLARE @LaboratoareCount INT = @@ROWCOUNT;

-- Creare mapare INT ? GUID (join pe NumeLaborator - unic)
;WITH SourceLab AS (
    SELECT LaboratorID, NumeLaborator
    FROM [DESKTOP-3Q8HI82\ERP].[HelperValyanMed].dbo.Laboratoare
)
INSERT INTO #LaboratorMapping (OldID, NewID)
SELECT 
    src.LaboratorID AS OldID,
    dst.LaboratorID AS NewID
FROM SourceLab src
INNER JOIN dbo.AnalizeMedicaleLaboratoare dst ON src.NumeLaborator = dst.NumeLaborator;

PRINT '  ? Migrate ' + CAST(@LaboratoareCount AS VARCHAR) + ' laboratoare';
PRINT '  ? Mapare creat?: ' + CAST((SELECT COUNT(*) FROM #LaboratorMapping) AS VARCHAR) + ' intr?ri';
GO

-- ========================================
-- STEP 4: MIGRARE CATEGORII (FIXED v2 - CTE)
-- ========================================
PRINT '[4/6] Migrare Categorii...';
GO

-- Mapare iconi?e
DECLARE @IconMapping TABLE (NumeCategorie NVARCHAR(100), Icon NVARCHAR(50));
INSERT INTO @IconMapping VALUES
    ('HEMATOLOGIE', 'fa-tint'),
    ('BIOCHIMIE', 'fa-flask'),
    ('IMUNOLOGIE', 'fa-shield-virus'),
    ('SEROLOGIE', 'fa-syringe'),
    ('COAGULARE', 'fa-droplet'),
    ('HORMONI', 'fa-brain'),
    ('MARKERI TUMORALI', 'fa-dna'),
    ('URINA', 'fa-toilet'),
    ('GLICEMIE', 'fa-candy-cane'),
    ('LIPIDE', 'fa-heart'),
    ('HEPATIC', 'fa-liver'),
    ('RENAL', 'fa-kidneys'),
    ('TIROIDIAN', 'fa-lungs');

-- Insert categorii
;WITH SourceCategorii AS (
    SELECT 
        CategorieID,
        NumeCategorie,
        Descriere,
        OrdineAfisare,
        Activ,
        DataCreare
    FROM [DESKTOP-3Q8HI82\ERP].[HelperValyanMed].dbo.Categorii
)
INSERT INTO dbo.AnalizeMedicaleCategorii (
    CategorieID, NumeCategorie, Descriere, Icon, OrdineAfisare, EsteActiv, DataCreare
)
SELECT 
    NEWID() AS CategorieID,
    c.NumeCategorie,
    c.Descriere,
    ISNULL(im.Icon, 'fa-vial') AS Icon,
    ISNULL(c.OrdineAfisare, 0) AS OrdineAfisare,
    ISNULL(c.Activ, 1) AS EsteActiv,
    ISNULL(c.DataCreare, GETDATE()) AS DataCreare
FROM SourceCategorii c
LEFT JOIN @IconMapping im ON UPPER(LTRIM(RTRIM(c.NumeCategorie))) = im.NumeCategorie;

DECLARE @CategoriiCount INT = @@ROWCOUNT;

-- Mapare INT ? GUID
;WITH SourceCat AS (
    SELECT CategorieID, NumeCategorie
    FROM [DESKTOP-3Q8HI82\ERP].[HelperValyanMed].dbo.Categorii
)
INSERT INTO #CategorieMapping (OldID, NewID)
SELECT 
    src.CategorieID AS OldID,
    dst.CategorieID AS NewID
FROM SourceCat src
INNER JOIN dbo.AnalizeMedicaleCategorii dst ON src.NumeCategorie = dst.NumeCategorie;

PRINT '  ? Migrate ' + CAST(@CategoriiCount AS VARCHAR) + ' categorii';
PRINT '  ? Mapare creat?: ' + CAST((SELECT COUNT(*) FROM #CategorieMapping) AS VARCHAR) + ' intr?ri';
GO

-- ========================================
-- STEP 5: MIGRARE ANALIZE (FIXED v2 - CTE)
-- ========================================
PRINT '[5/6] Migrare Analize (2649 înregistr?ri)...';
PRINT '  Aceasta poate dura 30-60 secunde...';
GO

-- Insert analize cu CTE
;WITH SourceAnalize AS (
    SELECT 
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
        Activ,
        DataScraping,
        DataActualizare
    FROM [DESKTOP-3Q8HI82\ERP].[HelperValyanMed].dbo.AnalizeLaborator
    WHERE ISNULL(Activ, 1) = 1
)
INSERT INTO dbo.AnalizeMedicale (
    AnalizaID, LaboratorID, CategorieID, CodAnaliza, NumeAnaliza, NumeScurt,
    Acronime, Descriere, PreparareaTestului, Material, TermenProcesare,
    Pret, Moneda, PretActualizatLa, URLSursa, EsteActiv, DataScraping,
    DataCreare, DataUltimaActualizare
)
SELECT 
    NEWID() AS AnalizaID,
    lm.NewID AS LaboratorID,
    cm.NewID AS CategorieID,
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
    1 AS EsteActiv,
    a.DataScraping,
    ISNULL(a.DataActualizare, GETDATE()) AS DataCreare,
    ISNULL(a.DataActualizare, GETDATE()) AS DataUltimaActualizare
FROM SourceAnalize a
INNER JOIN #LaboratorMapping lm ON a.LaboratorID = lm.OldID
INNER JOIN #CategorieMapping cm ON a.CategorieID = cm.OldID;

DECLARE @AnalizeCount INT = @@ROWCOUNT;

-- Mapare analize (op?ional - pentru viitoare referin?e)
;WITH SourceAn AS (
    SELECT AnalizaID, NumeAnaliza
    FROM [DESKTOP-3Q8HI82\ERP].[HelperValyanMed].dbo.AnalizeLaborator
    WHERE ISNULL(Activ, 1) = 1
)
INSERT INTO #AnalizaMapping (OldID, NewID)
SELECT 
    src.AnalizaID AS OldID,
    dst.AnalizaID AS NewID
FROM SourceAn src
INNER JOIN dbo.AnalizeMedicale dst ON src.NumeAnaliza = dst.NumeAnaliza;

PRINT '  ? Migrate ' + CAST(@AnalizeCount AS VARCHAR) + ' analize active';
PRINT '  ? Mapare creat?: ' + CAST((SELECT COUNT(*) FROM #AnalizaMapping) AS VARCHAR) + ' intr?ri';
GO

-- ========================================
-- STEP 6: MIGRARE ISTORIC SCRAPING (SKIP - Schema difer?)
-- ========================================
PRINT '[6/6] Migrare istoric scraping...';
PRINT '  ?? SKIP - Schema IstoricScraping difer? în HelperValyanMed';
PRINT '  Aceast? tabel? va fi populat? automat la viatoarele scraping-uri';
GO

-- ========================================
-- STEP 7: VERIFICARE & STATISTICI
-- ========================================
PRINT '';
PRINT '========================================';
PRINT 'STATISTICI MIGRARE';
PRINT '========================================';
GO

SELECT 
    'Laboratoare' AS Tabel,
    COUNT(*) AS TotalInregistrari,
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
FROM dbo.AnalizeMedicale
UNION ALL
SELECT 
    'Istoric Scraping',
    COUNT(*),
    NULL
FROM dbo.AnalizeMedicaleIstoricScraping;
GO

-- Top 10 categorii cu cele mai multe analize
PRINT 'Top 10 Categorii cu cele mai multe analize:';
SELECT TOP 10
    c.NumeCategorie,
    COUNT(a.AnalizaID) AS NumarAnalize,
    c.Icon
FROM dbo.AnalizeMedicaleCategorii c
LEFT JOIN dbo.AnalizeMedicale a ON c.CategorieID = a.CategorieID AND a.EsteActiv = 1
GROUP BY c.NumeCategorie, c.Icon
ORDER BY COUNT(a.AnalizaID) DESC;
GO

-- Sample analize migrate
PRINT 'Sample analize migrate:';
SELECT TOP 5
    a.NumeAnaliza,
    c.NumeCategorie,
    l.NumeLaborator,
    a.Pret,
    a.Moneda
FROM dbo.AnalizeMedicale a
INNER JOIN dbo.AnalizeMedicaleCategorii c ON a.CategorieID = c.CategorieID
INNER JOIN dbo.AnalizeMedicaleLaboratoare l ON a.LaboratorID = l.LaboratorID
WHERE a.EsteActiv = 1
ORDER BY NEWID();
GO

PRINT '';
PRINT '========================================';
PRINT 'MIGRARE COMPLET?!';
PRINT 'Sfâr?it: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
PRINT '';
PRINT 'NEXT STEPS:';
PRINT '  1. Verific? datele în SSMS';
PRINT '  2. Ruleaz?: SELECT * FROM AnalizeMedicale WHERE EsteActiv = 1';
PRINT '  3. Continu? cu crearea entit??ilor Domain în C#';
PRINT '';
GO
