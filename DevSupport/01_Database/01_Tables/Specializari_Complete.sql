-- ========================================
-- Tabel: Specializari
-- Database: ValyanMed
-- Descriere: Specializari medicale pentru personal medical
-- Generat: 2025-01-20
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.Specializari', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Specializari
    PRINT 'Tabel Specializari sters.'
END
GO

-- Create table
CREATE TABLE dbo.Specializari (
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [Denumire] NVARCHAR(200) NOT NULL,
    [Categorie] NVARCHAR(100) NULL, -- Medicala, Chirurgicala, Dentara, Farmaceutica
    [Descriere] NVARCHAR(MAX) NULL,
    [Este_Activ] BIT NOT NULL DEFAULT 1,
    [Data_Crearii] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [Data_Ultimei_Modificari] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [Creat_De] NVARCHAR(100) NULL DEFAULT SYSTEM_USER,
    [Modificat_De] NVARCHAR(100) NULL DEFAULT SYSTEM_USER,
    
    CONSTRAINT [PK_Specializari] PRIMARY KEY ([Id]),
    CONSTRAINT [UK_Specializari_Denumire] UNIQUE ([Denumire])
)
GO

-- Index pentru performanta
CREATE NONCLUSTERED INDEX [IX_Specializari_Denumire] 
ON dbo.Specializari ([Denumire] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Specializari_Categorie] 
ON dbo.Specializari ([Categorie] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Specializari_Activ] 
ON dbo.Specializari ([Este_Activ] ASC)
GO

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
GO

PRINT 'Tabel Specializari creat cu succes.'
GO

-- ========================================
-- POPULARE INITIALA - Specializari Medicale
-- ========================================

PRINT 'Incepere populare date...'
GO

-- Specializari Medicale (Medicina Intern?)
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

GO

DECLARE @NumarSpecializari INT = (SELECT COUNT(*) FROM Specializari);
PRINT 'Date inserate cu succes: ' + CAST(@NumarSpecializari AS VARCHAR) + ' specializari';
GO

-- Verificare date inserate
SELECT 
    Categorie,
    COUNT(*) AS NumarSpecializari
FROM dbo.Specializari
GROUP BY Categorie
ORDER BY Categorie
GO

PRINT 'Tabel Specializari creat si populat cu succes!'
GO

-- Comentarii pentru documentatie
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Tabel pentru stocarea specializarilor medicale pentru personalul medical din cadrul institutiei.', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Specializari'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Identificator unic UNIQUEIDENTIFIER cu NEWSEQUENTIALID() pentru performanta optimizata', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Specializari',
    @level2type = N'COLUMN', @level2name = N'Id'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Denumirea specializarii medicale', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Specializari',
    @level2type = N'COLUMN', @level2name = N'Denumire'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Categoria specializarii: Medical?, Chirurgical?, Laborator ?i Diagnostic, Stomatologie, Farmaceutic?', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Specializari',
    @level2type = N'COLUMN', @level2name = N'Categorie'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Descriere detaliata a specializarii (optional)', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Specializari',
    @level2type = N'COLUMN', @level2name = N'Descriere'
GO
