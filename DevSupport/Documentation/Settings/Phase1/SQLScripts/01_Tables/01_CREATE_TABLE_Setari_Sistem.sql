-- =============================================
-- Autor: ValyanClinic Development Team
-- Data: 2025-01-15
-- Versiune: 1.0
-- Descriere: Tabel setări sistem (Key-Value Pattern - EAV)
-- Dependențe: Nicio
-- =============================================



-- Verificare și creare tabel
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Setari_Sistem]') AND type in (N'U'))
BEGIN
    PRINT '=== CREARE TABEL Setari_Sistem ===';
    
    CREATE TABLE [dbo].[Setari_Sistem] (
        [SetareID] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [Categorie] NVARCHAR(100) NOT NULL,
        [Cheie] NVARCHAR(200) NOT NULL,
        [Valoare] NVARCHAR(MAX) NOT NULL,
        [TipDate] NVARCHAR(50) NOT NULL,
        [Descriere] NVARCHAR(500) NULL,
        [ValoareDefault] NVARCHAR(MAX) NULL,
        [EsteEditabil] BIT NOT NULL DEFAULT 1,
        [DataCrearii] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
        [DataModificarii] DATETIME2(7) NULL,
        [ModificatDe] NVARCHAR(100) NULL,
        
        CONSTRAINT [PK_Setari_Sistem] PRIMARY KEY CLUSTERED ([SetareID] ASC),
        CONSTRAINT [UQ_Setari_Sistem_Categorie_Cheie] UNIQUE NONCLUSTERED ([Categorie], [Cheie])
    );
    
    -- Index pentru performanță
    CREATE NONCLUSTERED INDEX [IX_Setari_Sistem_Categorie_Cheie] 
    ON [dbo].[Setari_Sistem] ([Categorie] ASC, [Cheie] ASC);
    
    PRINT '✓ Tabel Setari_Sistem creat cu succes.';
    PRINT '✓ Primary Key: PK_Setari_Sistem';
    PRINT '✓ Unique Constraint: UQ_Setari_Sistem_Categorie_Cheie';
    PRINT '✓ Index: IX_Setari_Sistem_Categorie_Cheie';
END
ELSE
BEGIN
  PRINT '! Tabelul Setari_Sistem există deja. Skip creation.';
END
GO

-- Verificare finală
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Setari_Sistem]') AND type in (N'U'))
BEGIN
    PRINT '=== VERIFICARE FINALĂ ===';
    PRINT '✓ Tabelul Setari_Sistem este gata de utilizare.';
    
  -- Afișare structură tabel
    SELECT 
        COLUMN_NAME AS [Coloană],
        DATA_TYPE AS [Tip Date],
        CHARACTER_MAXIMUM_LENGTH AS [Lungime Max],
        IS_NULLABLE AS [Nullable]
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Setari_Sistem'
    ORDER BY ORDINAL_POSITION;
END
GO
