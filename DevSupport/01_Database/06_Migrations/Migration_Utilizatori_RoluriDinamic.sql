-- ========================================
-- Script: Migrare Utilizatori la Sistem Roluri Dinamic
-- Database: ValyanMed
-- Data: 2025-12-25
-- 
-- Acest script:
-- 1. Șterge tabela RoluriSistem (nefolosită)
-- 2. Modifică Utilizatori să folosească RolID (FK către Roluri)
-- 3. Șterge utilizatorii existenți
-- 4. Creează un superAdmin
-- ========================================

USE [ValyanMed]
GO

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

PRINT '=========================================='
PRINT 'MIGRARE UTILIZATORI LA SISTEM ROLURI DINAMIC'
PRINT '=========================================='
PRINT ''
GO

-- ========================================
-- PASUL 1: Ștergere tabela RoluriSistem (nefolosită)
-- ========================================
PRINT 'PASUL 1: Stergere tabela RoluriSistem...'

IF OBJECT_ID('dbo.RoluriSistem', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.RoluriSistem
    PRINT '  + Tabela RoluriSistem a fost stearsa.'
END
ELSE
BEGIN
    PRINT '  - Tabela RoluriSistem nu exista.'
END
PRINT ''
GO

-- ========================================
-- PASUL 2: Ștergere constrângeri
-- ========================================
PRINT 'PASUL 2: Stergere constrangeri...'

-- Șterge constrângerea CHECK pentru Rol (dacă există)
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Utilizatori_Rol')
BEGIN
    ALTER TABLE dbo.Utilizatori DROP CONSTRAINT CK_Utilizatori_Rol
    PRINT '  + Constrangere CK_Utilizatori_Rol stearsa.'
END

-- Șterge UNIQUE pe PersonalMedicalID
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Utilizatori_PersonalMedicalID' AND object_id = OBJECT_ID('dbo.Utilizatori'))
BEGIN
    ALTER TABLE dbo.Utilizatori DROP CONSTRAINT UQ_Utilizatori_PersonalMedicalID
    PRINT '  + Constrangere UQ_Utilizatori_PersonalMedicalID stearsa.'
END

PRINT ''
GO

-- ========================================
-- PASUL 3: Ștergere date utilizatori
-- ========================================
PRINT 'PASUL 3: Stergere utilizatori existenti...'

DELETE FROM dbo.Utilizatori
PRINT '  + Utilizatori stersi: ' + CAST(@@ROWCOUNT AS VARCHAR)
PRINT ''
GO

-- ========================================
-- PASUL 4: Adaugă coloana RolID
-- ========================================
PRINT 'PASUL 4: Adaugare coloana RolID...'

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Utilizatori') AND name = 'RolID')
BEGIN
    ALTER TABLE dbo.Utilizatori ADD RolID UNIQUEIDENTIFIER NULL
    PRINT '  + Coloana RolID adaugata.'
END
ELSE
BEGIN
    PRINT '  - Coloana RolID exista deja.'
END
PRINT ''
GO

-- ========================================
-- PASUL 5: Șterge coloana Rol (text)
-- ========================================
PRINT 'PASUL 5: Stergere coloana Rol (text)...'

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Utilizatori') AND name = 'Rol')
BEGIN
    ALTER TABLE dbo.Utilizatori DROP COLUMN Rol
    PRINT '  + Coloana Rol (text) stearsa.'
END
ELSE
BEGIN
    PRINT '  - Coloana Rol nu exista.'
END
PRINT ''
GO

-- ========================================
-- PASUL 6: Adaugă FK către Roluri
-- ========================================
PRINT 'PASUL 6: Adaugare FK catre Roluri...'

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Utilizatori_Roluri')
BEGIN
    ALTER TABLE dbo.Utilizatori
    ADD CONSTRAINT FK_Utilizatori_Roluri 
    FOREIGN KEY (RolID) REFERENCES dbo.Roluri(RolID)
    PRINT '  + FK_Utilizatori_Roluri adaugat.'
END
ELSE
BEGIN
    PRINT '  - FK_Utilizatori_Roluri exista deja.'
END
PRINT ''
GO

-- ========================================
-- PASUL 7: Adaugă index pe RolID
-- ========================================
PRINT 'PASUL 7: Adaugare index pe RolID...'

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Utilizatori_RolID')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Utilizatori_RolID ON dbo.Utilizatori(RolID)
    PRINT '  + Index IX_Utilizatori_RolID creat.'
END
ELSE
BEGIN
    PRINT '  - Index IX_Utilizatori_RolID exista deja.'
END
PRINT ''
GO

-- ========================================
-- PASUL 8: Modifică PersonalMedicalID să fie nullable
-- ========================================
PRINT 'PASUL 8: Modificare PersonalMedicalID la nullable...'

-- Șterge FK existent
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Utilizatori_PersonalMedical')
BEGIN
    ALTER TABLE dbo.Utilizatori DROP CONSTRAINT FK_Utilizatori_PersonalMedical
    PRINT '  + FK_Utilizatori_PersonalMedical sters temporar.'
END

-- Modifică la nullable
ALTER TABLE dbo.Utilizatori ALTER COLUMN PersonalMedicalID UNIQUEIDENTIFIER NULL
PRINT '  + PersonalMedicalID modificat la nullable.'

-- Recreează FK
ALTER TABLE dbo.Utilizatori
ADD CONSTRAINT FK_Utilizatori_PersonalMedical 
FOREIGN KEY (PersonalMedicalID) REFERENCES dbo.PersonalMedical(PersonalID)
PRINT '  + FK_Utilizatori_PersonalMedical recreat.'
PRINT ''
GO

-- ========================================
-- PASUL 9: Creare SuperAdmin
-- ========================================
PRINT 'PASUL 9: Creare utilizator SuperAdmin...'

DECLARE @AdminRolID UNIQUEIDENTIFIER
DECLARE @SuperAdminID UNIQUEIDENTIFIER = NEWID()

-- Găsește RolID pentru Admin
SELECT @AdminRolID = RolID FROM Roluri WHERE Denumire = 'Admin'

IF @AdminRolID IS NULL
BEGIN
    PRINT '  X EROARE: Nu s-a gasit rolul Admin in tabela Roluri!'
    RETURN
END

-- Creează SuperAdmin
-- Password: Admin123! (hash BCrypt)
INSERT INTO dbo.Utilizatori (
    UtilizatorID,
    PersonalMedicalID,
    Username,
    Email,
    PasswordHash,
    Salt,
    RolID,
    EsteActiv,
    DataCreare,
    NumarIncercariEsuate,
    CreatDe,
    DataCrearii,
    DataUltimeiModificari,
    AccessFailedCount,
    LockoutEnabled,
    MustChangePasswordOnNextLogin
)
VALUES (
    @SuperAdminID,
    NULL,  -- SuperAdmin nu are PersonalMedical atasat
    'superadmin',
    'superadmin@valyan.clinic',
    '$2a$11$rBNpBqT5K8XwU7.3g0hFVe.Y/QKGI1hJrQvW.dOx6tE8xwMUXuU/.',  -- Admin123!
    NULL,
    @AdminRolID,
    1,  -- EsteActiv
    GETDATE(),
    0,  -- NumarIncercariEsuate
    'SYSTEM',
    GETDATE(),
    GETDATE(),
    0,  -- AccessFailedCount
    1,  -- LockoutEnabled
    0   -- MustChangePasswordOnNextLogin
)

PRINT '  + SuperAdmin creat cu succes!'
PRINT '    - Username: superadmin'
PRINT '    - Email: superadmin@valyan.clinic'
PRINT '    - Password: Admin123!'
PRINT ''
GO

-- ========================================
-- PASUL 10: Verificare finală
-- ========================================
PRINT 'PASUL 10: Verificare finala...'
PRINT ''

PRINT 'Utilizatori in sistem:'
SELECT u.Username, u.Email, r.Denumire AS Rol, u.EsteActiv
FROM Utilizatori u
LEFT JOIN Roluri r ON u.RolID = r.RolID

PRINT ''
PRINT 'Roluri disponibile:'
SELECT Denumire, Este_Activ, Este_Sistem FROM Roluri ORDER BY Ordine_Afisare

PRINT ''
PRINT '=========================================='
PRINT 'MIGRARE COMPLETA!'
PRINT '=========================================='
PRINT ''
PRINT 'IMPORTANT: Conecteaza-te cu:'
PRINT '  Username: superadmin'
PRINT '  Password: Admin123!'
PRINT ''
GO
