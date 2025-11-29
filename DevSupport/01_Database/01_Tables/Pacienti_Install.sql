-- ========================================
-- Instalare Tabela Pacienti
-- Database: ValyanMed
-- Descriere: Script complet pentru instalarea tabelei Pacienti
-- ========================================

USE [ValyanMed]
GO

PRINT '========================================'
PRINT 'INSTALARE TABEL PACIENTI'
PRINT '========================================'
PRINT ''

-- Verificare daca tabela exista deja
IF OBJECT_ID('dbo.Pacienti', 'U') IS NOT NULL
BEGIN
    PRINT 'ATENTIE: Tabela Pacienti exista deja!'
    PRINT 'Pentru a reinstala, rulati mai intai Pacienti_Complete.sql care va sterge tabela existenta.'
    PRINT ''
    
    -- Afisare numar inregistrari existente
    DECLARE @Count INT
    SELECT @Count = COUNT(*) FROM dbo.Pacienti
    PRINT 'Numar inregistrari existente: ' + CAST(@Count AS VARCHAR(10))
    PRINT ''
    PRINT 'Instalare anulata.'
END
ELSE
BEGIN
    PRINT 'Incepere instalare tabela Pacienti...'
    PRINT ''
    
    -- Rulare script creare tabela
    PRINT 'Executare script: Pacienti_Complete.sql'
    PRINT ''
    
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
        [Sex] NVARCHAR(1) NOT NULL,
        
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
        [CNP_Asigurat] NVARCHAR(13) NULL,
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
    
    PRINT '? Tabela Pacienti creata'
    PRINT ''
    
    -- Creare indexuri
    PRINT 'Creare indexuri...'
    
    CREATE NONCLUSTERED INDEX [IX_Pacienti_Nume_Prenume] 
    ON dbo.Pacienti ([Nume] ASC, [Prenume] ASC)
    
    CREATE NONCLUSTERED INDEX [IX_Pacienti_CNP] 
    ON dbo.Pacienti ([CNP] ASC)
    
    CREATE NONCLUSTERED INDEX [IX_Pacienti_Cod_Pacient] 
    ON dbo.Pacienti ([Cod_Pacient] ASC)
    
    CREATE NONCLUSTERED INDEX [IX_Pacienti_Data_Nasterii] 
    ON dbo.Pacienti ([Data_Nasterii] ASC)
    
    CREATE NONCLUSTERED INDEX [IX_Pacienti_Telefon] 
    ON dbo.Pacienti ([Telefon] ASC)
    
    CREATE NONCLUSTERED INDEX [IX_Pacienti_Email] 
    ON dbo.Pacienti ([Email] ASC)
    
    CREATE NONCLUSTERED INDEX [IX_Pacienti_Activ] 
    ON dbo.Pacienti ([Activ] ASC)
    
    PRINT '? Indexuri create: 7'
    PRINT ''
    
    -- Creare trigger pentru update timestamp
    PRINT 'Creare trigger pentru actualizare timestamp...'
    
    EXEC('
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
    ')
    
    PRINT '? Trigger creat'
    PRINT ''
    
    -- Adaugare comentarii pentru documentatie
    PRINT 'Adaugare comentarii pentru documentatie...'
    
    EXEC sp_addextendedproperty 
        @name = N'MS_Description', 
        @value = N'Tabel pentru stocarea pacientilor clinicii medicale.', 
        @level0type = N'SCHEMA', @level0name = N'dbo', 
        @level1type = N'TABLE', @level1name = N'Pacienti'
    
    EXEC sp_addextendedproperty 
        @name = N'MS_Description', 
        @value = N'Identificator unic UNIQUEIDENTIFIER cu NEWSEQUENTIALID() pentru performanta optimizata', 
        @level0type = N'SCHEMA', @level0name = N'dbo', 
        @level1type = N'TABLE', @level1name = N'Pacienti',
        @level2type = N'COLUMN', @level2name = N'Id'
    
    EXEC sp_addextendedproperty 
        @name = N'MS_Description', 
        @value = N'Cod intern clinic? format PACIENT00000001, generat automat', 
        @level0type = N'SCHEMA', @level0name = N'dbo', 
        @level1type = N'TABLE', @level1name = N'Pacienti',
        @level2type = N'COLUMN', @level2name = N'Cod_Pacient'
    
    PRINT '? Comentarii adaugate'
    PRINT ''
    
    PRINT '========================================'
    PRINT 'INSTALARE FINALIZATA CU SUCCES!'
    PRINT '========================================'
    PRINT ''
    PRINT 'Tabel: Pacienti'
    PRINT 'Coloane: 36'
    PRINT 'Indexuri: 7'
    PRINT 'Triggere: 1'
    PRINT ''
    PRINT 'Pasii urmatori:'
    PRINT '1. Rulati sp_Pacienti.sql pentru a crea procedurile stocate'
    PRINT '2. Rulati Pacienti_Verify.sql pentru a verifica instalarea'
    PRINT ''
END
GO
