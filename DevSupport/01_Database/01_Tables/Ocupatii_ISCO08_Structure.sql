-- ========================================
-- Tabel: Ocupatii_ISCO08 (VERSIUNEA FINALA CU GUID)
-- Descriere: Clasificarea Internationala Standard a Ocupatiilor (ISCO-08)
-- Sursa: data.gov.ro - Lista alfabetica ocupatii 2024
-- ID Type: UNIQUEIDENTIFIER cu NEWSEQUENTIALID()
-- Generat: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.Ocupatii_ISCO08', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Ocupatii_ISCO08
    PRINT 'Tabel Ocupatii_ISCO08 sters.'
END
GO

-- Create table pentru ocupatii ISCO-08 cu UNIQUEIDENTIFIER
CREATE TABLE dbo.Ocupatii_ISCO08 (
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [Cod_ISCO] NVARCHAR(10) NOT NULL,
    [Denumire_Ocupatie] NVARCHAR(500) NOT NULL,
    [Denumire_Ocupatie_EN] NVARCHAR(500) NULL,
    [Nivel_Ierarhic] TINYINT NOT NULL, -- 1=Grupa Major, 2=Subgrupa, 3=Grupa Minor, 4=Ocupatie
    [Cod_Parinte] NVARCHAR(10) NULL, -- Referinta la codul ISCO parinte
    [Grupa_Majora] NVARCHAR(10) NULL, -- Cod grupa majora (1 cifra)
    [Grupa_Majora_Denumire] NVARCHAR(300) NULL,
    [Subgrupa] NVARCHAR(10) NULL, -- Cod subgrupa (2 cifre)
    [Subgrupa_Denumire] NVARCHAR(300) NULL,
    [Grupa_Minora] NVARCHAR(10) NULL, -- Cod grupa minora (3 cifre)
    [Grupa_Minora_Denumire] NVARCHAR(300) NULL,
    [Descriere] NVARCHAR(MAX) NULL,
    [Observatii] NVARCHAR(1000) NULL,
    [Este_Activ] BIT NOT NULL DEFAULT 1,
    [Data_Crearii] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [Data_Ultimei_Modificari] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [Creat_De] NVARCHAR(100) NULL DEFAULT SYSTEM_USER,
    [Modificat_De] NVARCHAR(100) NULL DEFAULT SYSTEM_USER,
    
    -- Constrangeri
    CONSTRAINT [PK_Ocupatii_ISCO08] PRIMARY KEY ([Id]),
    CONSTRAINT [UK_Ocupatii_ISCO08_Cod] UNIQUE ([Cod_ISCO]),
    CONSTRAINT [CK_Ocupatii_ISCO08_Nivel] CHECK ([Nivel_Ierarhic] IN (1, 2, 3, 4)),
    CONSTRAINT [CK_Ocupatii_ISCO08_CodFormat] CHECK (
        ([Nivel_Ierarhic] = 1 AND LEN([Cod_ISCO]) = 1) OR
        ([Nivel_Ierarhic] = 2 AND LEN([Cod_ISCO]) = 2) OR
        ([Nivel_Ierarhic] = 3 AND LEN([Cod_ISCO]) = 3) OR
        ([Nivel_Ierarhic] = 4 AND LEN([Cod_ISCO]) = 4)
    )
)
GO

-- Indexes pentru performanta
CREATE NONCLUSTERED INDEX [IX_Ocupatii_ISCO08_Cod_ISCO] 
ON dbo.Ocupatii_ISCO08 ([Cod_ISCO] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Ocupatii_ISCO08_Nivel_Ierarhic] 
ON dbo.Ocupatii_ISCO08 ([Nivel_Ierarhic] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Ocupatii_ISCO08_Grupa_Majora] 
ON dbo.Ocupatii_ISCO08 ([Grupa_Majora] ASC)
WHERE [Grupa_Majora] IS NOT NULL
GO

CREATE NONCLUSTERED INDEX [IX_Ocupatii_ISCO08_Parinte] 
ON dbo.Ocupatii_ISCO08 ([Cod_Parinte] ASC)
WHERE [Cod_Parinte] IS NOT NULL
GO

CREATE NONCLUSTERED INDEX [IX_Ocupatii_ISCO08_Denumire] 
ON dbo.Ocupatii_ISCO08 ([Denumire_Ocupatie] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Ocupatii_ISCO08_Activ] 
ON dbo.Ocupatii_ISCO08 ([Este_Activ] ASC)
GO

-- Foreign key pentru referinta ierarhica
ALTER TABLE dbo.Ocupatii_ISCO08
ADD CONSTRAINT [FK_Ocupatii_ISCO08_Parinte] 
FOREIGN KEY ([Cod_Parinte]) REFERENCES dbo.Ocupatii_ISCO08 ([Cod_ISCO])
GO

-- Trigger pentru actualizarea automata a datei de modificare
CREATE TRIGGER [TR_Ocupatii_ISCO08_UpdateTimestamp]
ON dbo.Ocupatii_ISCO08
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.Ocupatii_ISCO08
    SET [Data_Ultimei_Modificari] = GETDATE(),
        [Modificat_De] = SYSTEM_USER
    FROM dbo.Ocupatii_ISCO08 o
    INNER JOIN inserted i ON o.[Id] = i.[Id]
END
GO

PRINT 'Tabel Ocupatii_ISCO08 creat cu succes cu structura UNIQUEIDENTIFIER + NEWSEQUENTIALID().'
GO

-- Comentarii pentru documentatie
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Tabel pentru stocarea clasificarii internationale standard a ocupatiilor ISCO-08. Contine structura ierarhica completa cu ID-uri UNIQUEIDENTIFIER generate secvential pentru performanta.', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Ocupatii_ISCO08'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Identificator unic UNIQUEIDENTIFIER cu NEWSEQUENTIALID() pentru performanta optimizata', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Ocupatii_ISCO08',
    @level2type = N'COLUMN', @level2name = N'Id'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Codul ISCO unic (1-4 cifre)', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Ocupatii_ISCO08',
    @level2type = N'COLUMN', @level2name = N'Cod_ISCO'
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Nivelul in ierarhia ISCO: 1=Grupa Major, 2=Subgrupa, 3=Grupa Minor, 4=Ocupatie', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Ocupatii_ISCO08',
    @level2type = N'COLUMN', @level2name = N'Nivel_Ierarhic'
GO