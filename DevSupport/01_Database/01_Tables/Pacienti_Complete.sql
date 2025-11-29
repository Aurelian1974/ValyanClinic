-- ========================================
-- Tabel: Pacienti
-- Database: ValyanMed
-- Descriere: Pacienti clinica medicala
-- Generat: 2025-01-23
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.Pacienti', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Pacienti
    PRINT 'Tabel Pacienti sters.'
END
GO

-- Create table
CREATE TABLE dbo.Pacienti (
    -- Identificare primar?
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [Cod_Pacient] NVARCHAR(20) NOT NULL,
    
    -- Date personale
    [CNP] NVARCHAR(13) NULL,
    [Nume] NVARCHAR(100) NOT NULL,
    [Prenume] NVARCHAR(100) NOT NULL,
    [Data_Nasterii] DATE NOT NULL,
    [Sex] NVARCHAR(1) NOT NULL, -- 'M' sau 'F'
    
    -- Date de contact
    [Telefon] NVARCHAR(15) NULL,
    [Telefon_Secundar] NVARCHAR(15) NULL,
    [Email] NVARCHAR(100) NULL,
    
    -- Adres?
    [Judet] NVARCHAR(50) NULL,
    [Localitate] NVARCHAR(100) NULL,
    [Adresa] NVARCHAR(255) NULL,
    [Cod_Postal] NVARCHAR(10) NULL,
    
    -- Informa?ii asigurare
    [Asigurat] BIT NOT NULL DEFAULT 0,
    [CNP_Asigurat] NVARCHAR(13) NULL, -- poate fi diferit de CNP (copii)
    [Nr_Card_Sanatate] NVARCHAR(20) NULL,
    [Casa_Asigurari] NVARCHAR(100) NULL,
    
    -- Date medicale de baz?
    [Alergii] NVARCHAR(MAX) NULL,
    [Boli_Cronice] NVARCHAR(MAX) NULL,
    [Medic_Familie] NVARCHAR(150) NULL,
    
    -- Contact urgen??
    [Persoana_Contact] NVARCHAR(150) NULL,
    [Telefon_Urgenta] NVARCHAR(15) NULL,
    [Relatie_Contact] NVARCHAR(50) NULL,
    
    -- Informa?ii administrative
    [Data_Inregistrare] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [Ultima_Vizita] DATE NULL,
    [Nr_Total_Vizite] INT NOT NULL DEFAULT 0,
    [Activ] BIT NOT NULL DEFAULT 1,
    [Observatii] NVARCHAR(MAX) NULL,
    
    -- Audit
    [Data_Crearii] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [Data_Ultimei_Modificari] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [Creat_De] NVARCHAR(100) NULL DEFAULT SYSTEM_USER,
    [Modificat_De] NVARCHAR(100) NULL DEFAULT SYSTEM_USER,
    
    CONSTRAINT [PK_Pacienti] PRIMARY KEY ([Id]),
    CONSTRAINT [UK_Pacienti_Cod_Pacient] UNIQUE ([Cod_Pacient]),
    CONSTRAINT [UK_Pacienti_CNP] UNIQUE ([CNP]),
    CONSTRAINT [CK_Pacienti_Sex] CHECK ([Sex] IN ('M', 'F'))
)
GO

-- Index pentru performanta
CREATE NONCLUSTERED INDEX [IX_Pacienti_Nume_Prenume] 
ON dbo.Pacienti ([Nume] ASC, [Prenume] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Pacienti_CNP] 
ON dbo.Pacienti ([CNP] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Pacienti_Cod_Pacient] 
ON dbo.Pacienti ([Cod_Pacient] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Pacienti_Data_Nasterii] 
ON dbo.Pacienti ([Data_Nasterii] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Pacienti_Telefon] 
ON dbo.Pacienti ([Telefon] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Pacienti_Email] 
ON dbo.Pacienti ([Email] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Pacienti_Activ] 
ON dbo.Pacienti ([Activ] ASC)
GO

-- Trigger pentru actualizarea automata a datei de modificare
CREATE TRIGGER [TR_Pacienti_UpdateTimestamp]
ON dbo.Pacienti
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.Pacienti
    SET [Data_Ultimei_Modificari] = GETDATE(),
        [Modificat_De] = SYSTEM_USER
    FROM dbo.Pacienti p
    INNER JOIN inserted i ON p.[Id] = i.[Id]
END
GO

PRINT 'Tabel Pacienti creat cu succes.'
GO

-- ========================================
-- POPULARE INITIALA - Pacient de test (optional)
-- ========================================

-- Comentariu: Deocamdat? nu popul?m cu date test
-- Datele vor fi ad?ugate prin aplica?ie sau manual

PRINT 'Tabel Pacienti gata pentru utilizare.'
GO

-- Comentarii pentru documentatie
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Tabel pentru stocarea pacientilor clinicii medicale.', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Pacienti'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Identificator unic UNIQUEIDENTIFIER cu NEWSEQUENTIALID() pentru performanta optimizata', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Pacienti',
    @level2type = N'COLUMN', @level2name = N'Id'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Cod intern clinic? format PACIENT00000001, generat automat', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Pacienti',
    @level2type = N'COLUMN', @level2name = N'Cod_Pacient'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Cod numeric personal - unic', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Pacienti',
    @level2type = N'COLUMN', @level2name = N'CNP'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Numele pacientului', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Pacienti',
    @level2type = N'COLUMN', @level2name = N'Nume'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Prenumele pacientului', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Pacienti',
    @level2type = N'COLUMN', @level2name = N'Prenume'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Data nasterii pacientului', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Pacienti',
    @level2type = N'COLUMN', @level2name = N'Data_Nasterii'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Sex: M (Masculin) sau F (Feminin)', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Pacienti',
    @level2type = N'COLUMN', @level2name = N'Sex'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'CNP al asiguratului (poate diferi pentru copii)', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Pacienti',
    @level2type = N'COLUMN', @level2name = N'CNP_Asigurat'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Numar total vizite - calculat automat', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Pacienti',
    @level2type = N'COLUMN', @level2name = N'Nr_Total_Vizite'
GO

PRINT 'Tabel Pacienti creat si documentat cu succes!'
GO
