-- ========================================
-- Tabel: Pozitii
-- Database: ValyanMed
-- Descriere: Pozitii/Functii pentru personal medical si non-medical
-- Generat: 2025-01-20
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.Pozitii', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Pozitii
    PRINT 'Tabel Pozitii sters.'
END
GO

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
GO

-- Index pentru performanta
CREATE NONCLUSTERED INDEX [IX_Pozitii_Denumire] 
ON dbo.Pozitii ([Denumire] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Pozitii_Activ] 
ON dbo.Pozitii ([Este_Activ] ASC)
GO

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
GO

PRINT 'Tabel Pozitii creat cu succes.'
GO

-- ========================================
-- POPULARE INITIALA - Pozitii Personal Medical si Non-Medical
-- ========================================

PRINT 'Incepere populare date...'
GO

-- Pozitii Personal Medical
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

GO

PRINT 'Date inserate cu succes.'
GO

-- Verificare date inserate
SELECT 
    Id,
    Denumire,
    Descriere,
    Este_Activ,
    Data_Crearii
FROM dbo.Pozitii
ORDER BY Denumire
GO

PRINT 'Tabel Pozitii creat si populat cu succes!'
GO

-- Comentarii pentru documentatie
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Tabel pentru stocarea pozitiilor/functiilor pentru personalul medical si non-medical din cadrul institutiei.', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Pozitii'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Identificator unic UNIQUEIDENTIFIER cu NEWSEQUENTIALID() pentru performanta optimizata', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Pozitii',
    @level2type = N'COLUMN', @level2name = N'Id'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Denumirea pozitiei/functiei', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Pozitii',
    @level2type = N'COLUMN', @level2name = N'Denumire'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Descriere detaliata a pozitiei (optional)', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Pozitii',
    @level2type = N'COLUMN', @level2name = N'Descriere'
GO
