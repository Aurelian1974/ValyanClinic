-- ========================================
-- Tabel: Utilizatori
-- Database: ValyanMed
-- Descriere: Tabela pentru utilizatori aplicatie - ASOCIAT CU PersonalMedical
-- Created: 2025-01-24
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.Utilizatori', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Utilizatori
    PRINT 'Tabel Utilizatori sters.'
END
GO

-- Create table
CREATE TABLE dbo.Utilizatori (
    [UtilizatorID] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [PersonalMedicalID] UNIQUEIDENTIFIER NOT NULL,
    [Username] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(100) NOT NULL,
    [PasswordHash] NVARCHAR(256) NOT NULL,
    [Salt] NVARCHAR(100) NOT NULL,
[Rol] NVARCHAR(50) NOT NULL,
  [EsteActiv] BIT NOT NULL DEFAULT 1,
    [DataCreare] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [DataUltimaAutentificare] DATETIME2 NULL,
    [NumarIncercariEsuate] INT NOT NULL DEFAULT 0,
    [DataBlocare] DATETIME2 NULL,
    [TokenResetareParola] NVARCHAR(256) NULL,
    [DataExpirareToken] DATETIME2 NULL,
    [CreatDe] NVARCHAR(100) NULL,
    [DataCrearii] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [ModificatDe] NVARCHAR(100) NULL,
    [DataUltimeiModificari] DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT [PK_Utilizatori] PRIMARY KEY ([UtilizatorID]),
    CONSTRAINT [FK_Utilizatori_PersonalMedical] FOREIGN KEY ([PersonalMedicalID]) 
        REFERENCES dbo.[PersonalMedical] ([PersonalID]) ON DELETE CASCADE,
    CONSTRAINT [UQ_Utilizatori_Username] UNIQUE ([Username]),
    CONSTRAINT [UQ_Utilizatori_Email] UNIQUE ([Email]),
    CONSTRAINT [UQ_Utilizatori_PersonalMedicalID] UNIQUE ([PersonalMedicalID]),
    CONSTRAINT [CK_Utilizatori_Rol] CHECK ([Rol] IN ('Administrator', 'Doctor', 'Asistent', 'Receptioner', 'Manager', 'Utilizator'))
)
GO

-- Indexes pentru performanta
CREATE NONCLUSTERED INDEX [IX_Utilizatori_Username] 
    ON dbo.Utilizatori ([Username] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Utilizatori_Email] 
    ON dbo.Utilizatori ([Email] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Utilizatori_PersonalMedicalID] 
    ON dbo.Utilizatori ([PersonalMedicalID] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Utilizatori_EsteActiv] 
    ON dbo.Utilizatori ([EsteActiv] ASC)
    INCLUDE ([Username], [Email], [Rol])
GO

CREATE NONCLUSTERED INDEX [IX_Utilizatori_Rol] 
    ON dbo.Utilizatori ([Rol] ASC)
    WHERE [EsteActiv] = 1
GO

CREATE NONCLUSTERED INDEX [IX_Utilizatori_TokenResetareParola] 
    ON dbo.Utilizatori ([TokenResetareParola] ASC)
    WHERE [TokenResetareParola] IS NOT NULL AND [DataExpirareToken] IS NOT NULL
GO

PRINT 'Tabel Utilizatori creat cu succes cu 18 coloane.'
PRINT 'Foreign Key: PersonalMedicalID -> PersonalMedical.PersonalID'
PRINT 'Indexes: 6 indexes create pentru performanta'
GO

-- Comentarii detaliate
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Tabela pentru gestionarea utilizatorilor aplicatiei. Fiecare utilizator este asociat cu o inregistrare din PersonalMedical (doctor, asistent, etc.)', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'Utilizatori'
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'ID unic pentru utilizator (GUID)', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'Utilizatori',
    @level2type = N'COLUMN', @level2name = N'UtilizatorID'
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Foreign Key catre PersonalMedical - asociaza utilizatorul cu un membru al personalului medical', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
  @level1type = N'TABLE',  @level1name = N'Utilizatori',
    @level2type = N'COLUMN', @level2name = N'PersonalMedicalID'
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Nume utilizator pentru autentificare (unic)', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'Utilizatori',
    @level2type = N'COLUMN', @level2name = N'Username'
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Rol utilizator: Administrator, Doctor, Asistent, Receptioner, Manager, Utilizator', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'Utilizatori',
    @level2type = N'COLUMN', @level2name = N'Rol'
GO
