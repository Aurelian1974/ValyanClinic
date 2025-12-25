-- ========================================
-- Tabele: Roluri si Permisiuni
-- Database: ValyanMed
-- Descriere: Tabele pentru Policy-Based Authorization
-- Generat: 2025-12-25
-- ========================================

USE [ValyanMed]
GO

-- ========================================
-- 1. Tabel: PermisiuniDefinitii
-- Catalog de permisiuni disponibile in sistem
-- ========================================

IF OBJECT_ID('dbo.PermisiuniDefinitii', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.PermisiuniDefinitii
    PRINT 'Tabel PermisiuniDefinitii sters.'
END
GO

CREATE TABLE dbo.PermisiuniDefinitii (
    [PermisiuneDefinitieID] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [Cod] NVARCHAR(100) NOT NULL,                    -- ex: Pacient.View, Consultatie.Create
    [Categorie] NVARCHAR(50) NOT NULL,               -- ex: Pacient, Consultatie, Programare
    [Denumire] NVARCHAR(200) NOT NULL,               -- Denumire afisata in UI
    [Descriere] NVARCHAR(500) NULL,                  -- Descriere detaliata
    [Ordine_Afisare] INT NOT NULL DEFAULT 0,         -- Ordinea in categorie
    [Este_Activ] BIT NOT NULL DEFAULT 1,
    
    CONSTRAINT [PK_PermisiuniDefinitii] PRIMARY KEY ([PermisiuneDefinitieID]),
    CONSTRAINT [UK_PermisiuniDefinitii_Cod] UNIQUE ([Cod])
)
GO

CREATE NONCLUSTERED INDEX [IX_PermisiuniDefinitii_Categorie] 
ON dbo.PermisiuniDefinitii ([Categorie] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_PermisiuniDefinitii_Activ] 
ON dbo.PermisiuniDefinitii ([Este_Activ] ASC)
GO

PRINT 'Tabel PermisiuniDefinitii creat cu succes.'
GO

-- ========================================
-- 2. Tabel: Roluri
-- Defineste rolurile din sistem
-- ========================================

IF OBJECT_ID('dbo.RoluriPermisiuni', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.RoluriPermisiuni
    PRINT 'Tabel RoluriPermisiuni sters (dependenta).'
END
GO

IF OBJECT_ID('dbo.Roluri', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Roluri
    PRINT 'Tabel Roluri sters.'
END
GO

CREATE TABLE dbo.Roluri (
    [RolID] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [Denumire] NVARCHAR(100) NOT NULL,
    [Descriere] NVARCHAR(500) NULL,
    [Este_Activ] BIT NOT NULL DEFAULT 1,
    [Este_Sistem] BIT NOT NULL DEFAULT 0,            -- Nu poate fi sters
    [Ordine_Afisare] INT NOT NULL DEFAULT 0,
    [Data_Crearii] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [Data_Ultimei_Modificari] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [Creat_De] NVARCHAR(100) NULL DEFAULT SYSTEM_USER,
    [Modificat_De] NVARCHAR(100) NULL DEFAULT SYSTEM_USER,
    
    CONSTRAINT [PK_Roluri] PRIMARY KEY ([RolID]),
    CONSTRAINT [UK_Roluri_Denumire] UNIQUE ([Denumire])
)
GO

CREATE NONCLUSTERED INDEX [IX_Roluri_Denumire] 
ON dbo.Roluri ([Denumire] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Roluri_Activ] 
ON dbo.Roluri ([Este_Activ] ASC)
GO

-- Trigger pentru actualizarea automata a datei de modificare
CREATE TRIGGER [TR_Roluri_UpdateTimestamp]
ON dbo.Roluri
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.Roluri
    SET [Data_Ultimei_Modificari] = GETDATE(),
        [Modificat_De] = SYSTEM_USER
    FROM dbo.Roluri r
    INNER JOIN inserted i ON r.[RolID] = i.[RolID]
END
GO

PRINT 'Tabel Roluri creat cu succes.'
GO

-- ========================================
-- 3. Tabel: RoluriPermisiuni
-- Asociere many-to-many intre Roluri si Permisiuni
-- ========================================

CREATE TABLE dbo.RoluriPermisiuni (
    [RolPermisiuneID] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [RolID] UNIQUEIDENTIFIER NOT NULL,
    [Permisiune] NVARCHAR(100) NOT NULL,             -- Codul permisiunii
    [Este_Acordat] BIT NOT NULL DEFAULT 1,           -- True = acordat, False = refuzat explicit
    [Data_Crearii] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    [Creat_De] NVARCHAR(100) NULL DEFAULT SYSTEM_USER,
    
    CONSTRAINT [PK_RoluriPermisiuni] PRIMARY KEY ([RolPermisiuneID]),
    CONSTRAINT [FK_RoluriPermisiuni_Roluri] FOREIGN KEY ([RolID]) 
        REFERENCES dbo.Roluri([RolID]) ON DELETE CASCADE,
    CONSTRAINT [UK_RoluriPermisiuni_RolPermisiune] UNIQUE ([RolID], [Permisiune])
)
GO

CREATE NONCLUSTERED INDEX [IX_RoluriPermisiuni_RolID] 
ON dbo.RoluriPermisiuni ([RolID] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_RoluriPermisiuni_Permisiune] 
ON dbo.RoluriPermisiuni ([Permisiune] ASC)
GO

PRINT 'Tabel RoluriPermisiuni creat cu succes.'
GO

-- ========================================
-- POPULARE INITIALA - Permisiuni
-- ========================================

PRINT 'Incepere populare permisiuni...'
GO

-- Permisiuni Pacient
INSERT INTO dbo.PermisiuniDefinitii ([Cod], [Categorie], [Denumire], [Descriere], [Ordine_Afisare])
VALUES 
    (N'Pacient.View', N'Pacient', N'Vizualizare pacienți', N'Permite vizualizarea listei de pacienți și a detaliilor', 1),
    (N'Pacient.Create', N'Pacient', N'Creare pacient', N'Permite înregistrarea de pacienți noi', 2),
    (N'Pacient.Edit', N'Pacient', N'Editare pacient', N'Permite modificarea datelor pacienților', 3),
    (N'Pacient.Delete', N'Pacient', N'Ștergere pacient', N'Permite ștergerea pacienților din sistem', 4),
    (N'Pacient.ViewSensitiveData', N'Pacient', N'Date sensibile pacient', N'Permite vizualizarea CNP și datelor medicale sensibile', 5),
    (N'Pacient.Export', N'Pacient', N'Export pacienți', N'Permite exportul datelor pacienților', 6)
GO

-- Permisiuni Consultatie
INSERT INTO dbo.PermisiuniDefinitii ([Cod], [Categorie], [Denumire], [Descriere], [Ordine_Afisare])
VALUES 
    (N'Consultatie.View', N'Consultatie', N'Vizualizare consultații', N'Permite vizualizarea tuturor consultațiilor', 1),
    (N'Consultatie.ViewOwn', N'Consultatie', N'Consultații proprii', N'Permite vizualizarea doar a consultațiilor proprii', 2),
    (N'Consultatie.ViewDepartment', N'Consultatie', N'Consultații departament', N'Permite vizualizarea consultațiilor din departament', 3),
    (N'Consultatie.Create', N'Consultatie', N'Creare consultație', N'Permite crearea de consultații noi', 4),
    (N'Consultatie.Edit', N'Consultatie', N'Editare consultație', N'Permite editarea oricărei consultații', 5),
    (N'Consultatie.EditOwn', N'Consultatie', N'Editare consultații proprii', N'Permite editarea doar a consultațiilor proprii', 6),
    (N'Consultatie.Delete', N'Consultatie', N'Ștergere consultație', N'Permite ștergerea consultațiilor', 7),
    (N'Consultatie.Finalize', N'Consultatie', N'Finalizare consultație', N'Permite finalizarea și semnarea consultațiilor', 8),
    (N'Consultatie.Prescribe', N'Consultatie', N'Prescriere medicamente', N'Permite prescrierea de medicamente', 9)
GO

-- Permisiuni Programare
INSERT INTO dbo.PermisiuniDefinitii ([Cod], [Categorie], [Denumire], [Descriere], [Ordine_Afisare])
VALUES 
    (N'Programare.View', N'Programare', N'Vizualizare programări', N'Permite vizualizarea calendarului de programări', 1),
    (N'Programare.Create', N'Programare', N'Creare programare', N'Permite crearea de programări noi', 2),
    (N'Programare.Edit', N'Programare', N'Editare programare', N'Permite modificarea programărilor existente', 3),
    (N'Programare.Delete', N'Programare', N'Ștergere programare', N'Permite ștergerea programărilor', 4),
    (N'Programare.Cancel', N'Programare', N'Anulare programare', N'Permite anularea programărilor', 5),
    (N'Programare.Confirm', N'Programare', N'Confirmare programare', N'Permite confirmarea programărilor', 6)
GO

-- Permisiuni Personal
INSERT INTO dbo.PermisiuniDefinitii ([Cod], [Categorie], [Denumire], [Descriere], [Ordine_Afisare])
VALUES 
    (N'Personal.View', N'Personal', N'Vizualizare personal', N'Permite vizualizarea listei de personal', 1),
    (N'Personal.Create', N'Personal', N'Creare personal', N'Permite adăugarea de personal nou', 2),
    (N'Personal.Edit', N'Personal', N'Editare personal', N'Permite modificarea datelor personalului', 3),
    (N'Personal.Delete', N'Personal', N'Ștergere personal', N'Permite ștergerea din lista de personal', 4),
    (N'Personal.ManageRoles', N'Personal', N'Gestionare roluri', N'Permite atribuirea și modificarea rolurilor', 5)
GO

-- Permisiuni Admin
INSERT INTO dbo.PermisiuniDefinitii ([Cod], [Categorie], [Denumire], [Descriere], [Ordine_Afisare])
VALUES 
    (N'Admin.AccessDashboard', N'Admin', N'Acces dashboard admin', N'Permite accesul la panoul de administrare', 1),
    (N'Admin.ViewAuditLog', N'Admin', N'Vizualizare audit log', N'Permite vizualizarea jurnalului de audit', 2),
    (N'Admin.ManageSettings', N'Admin', N'Gestionare setări', N'Permite modificarea setărilor sistemului', 3),
    (N'Admin.ManageUsers', N'Admin', N'Gestionare utilizatori', N'Permite administrarea utilizatorilor', 4),
    (N'Admin.ViewReports', N'Admin', N'Vizualizare rapoarte', N'Permite accesul la rapoarte', 5),
    (N'Admin.ExportData', N'Admin', N'Export date', N'Permite exportul datelor din sistem', 6)
GO

-- Permisiuni Speciale
INSERT INTO dbo.PermisiuniDefinitii ([Cod], [Categorie], [Denumire], [Descriere], [Ordine_Afisare])
VALUES 
    (N'Special.EmergencyAccess', N'Special', N'Acces urgență', N'Permite acces de urgență (break-glass) cu logare completă', 1),
    (N'Special.FullAccess', N'Special', N'Acces complet', N'Acces nelimitat la toate funcționalitățile sistemului', 2)
GO

PRINT 'Permisiuni populate cu succes.'
GO

-- ========================================
-- POPULARE INITIALA - Roluri de sistem
-- ========================================

PRINT 'Incepere populare roluri...'
GO

DECLARE @AdminID UNIQUEIDENTIFIER = NEWID()
DECLARE @DoctorID UNIQUEIDENTIFIER = NEWID()
DECLARE @AsistentID UNIQUEIDENTIFIER = NEWID()
DECLARE @ReceptionerID UNIQUEIDENTIFIER = NEWID()
DECLARE @ManagerID UNIQUEIDENTIFIER = NEWID()

-- Roluri de sistem
INSERT INTO dbo.Roluri ([RolID], [Denumire], [Descriere], [Este_Sistem], [Ordine_Afisare])
VALUES 
    (@AdminID, N'Admin', N'Administrator de sistem cu acces complet la toate funcționalitățile', 1, 1),
    (@DoctorID, N'Doctor', N'Personal medical cu drept de consultație și prescriere', 1, 2),
    (@AsistentID, N'Asistent', N'Asistent medical care susține activitatea medicului', 1, 3),
    (@ReceptionerID, N'Receptioner', N'Personal de recepție pentru programări și înregistrare pacienți', 1, 4),
    (@ManagerID, N'Manager', N'Manager cu acces la rapoarte și administrare personal', 1, 5)
GO

PRINT 'Roluri populate cu succes.'
GO

-- ========================================
-- POPULARE - Permisiuni pentru Admin
-- ========================================

DECLARE @AdminRolID UNIQUEIDENTIFIER = (SELECT [RolID] FROM dbo.Roluri WHERE [Denumire] = N'Admin')

INSERT INTO dbo.RoluriPermisiuni ([RolID], [Permisiune])
SELECT @AdminRolID, [Cod] FROM dbo.PermisiuniDefinitii
GO

PRINT 'Permisiuni Admin populate cu succes.'
GO

-- ========================================
-- POPULARE - Permisiuni pentru Doctor
-- ========================================

DECLARE @DoctorRolID UNIQUEIDENTIFIER = (SELECT [RolID] FROM dbo.Roluri WHERE [Denumire] = N'Doctor')

INSERT INTO dbo.RoluriPermisiuni ([RolID], [Permisiune])
VALUES 
    (@DoctorRolID, N'Pacient.View'),
    (@DoctorRolID, N'Pacient.Create'),
    (@DoctorRolID, N'Pacient.Edit'),
    (@DoctorRolID, N'Pacient.ViewSensitiveData'),
    (@DoctorRolID, N'Consultatie.View'),
    (@DoctorRolID, N'Consultatie.ViewOwn'),
    (@DoctorRolID, N'Consultatie.ViewDepartment'),
    (@DoctorRolID, N'Consultatie.Create'),
    (@DoctorRolID, N'Consultatie.EditOwn'),
    (@DoctorRolID, N'Consultatie.Finalize'),
    (@DoctorRolID, N'Consultatie.Prescribe'),
    (@DoctorRolID, N'Programare.View'),
    (@DoctorRolID, N'Programare.Create'),
    (@DoctorRolID, N'Programare.Edit'),
    (@DoctorRolID, N'Programare.Confirm'),
    (@DoctorRolID, N'Personal.View'),
    (@DoctorRolID, N'Special.EmergencyAccess')
GO

PRINT 'Permisiuni Doctor populate cu succes.'
GO

-- ========================================
-- POPULARE - Permisiuni pentru Asistent
-- ========================================

DECLARE @AsistentRolID UNIQUEIDENTIFIER = (SELECT [RolID] FROM dbo.Roluri WHERE [Denumire] = N'Asistent')

INSERT INTO dbo.RoluriPermisiuni ([RolID], [Permisiune])
VALUES 
    (@AsistentRolID, N'Pacient.View'),
    (@AsistentRolID, N'Pacient.Create'),
    (@AsistentRolID, N'Pacient.Edit'),
    (@AsistentRolID, N'Consultatie.View'),
    (@AsistentRolID, N'Consultatie.ViewDepartment'),
    (@AsistentRolID, N'Programare.View'),
    (@AsistentRolID, N'Programare.Create'),
    (@AsistentRolID, N'Programare.Edit'),
    (@AsistentRolID, N'Programare.Cancel'),
    (@AsistentRolID, N'Programare.Confirm'),
    (@AsistentRolID, N'Personal.View')
GO

PRINT 'Permisiuni Asistent populate cu succes.'
GO

-- ========================================
-- POPULARE - Permisiuni pentru Receptioner
-- ========================================

DECLARE @ReceptionerRolID UNIQUEIDENTIFIER = (SELECT [RolID] FROM dbo.Roluri WHERE [Denumire] = N'Receptioner')

INSERT INTO dbo.RoluriPermisiuni ([RolID], [Permisiune])
VALUES 
    (@ReceptionerRolID, N'Pacient.View'),
    (@ReceptionerRolID, N'Pacient.Create'),
    (@ReceptionerRolID, N'Pacient.Edit'),
    (@ReceptionerRolID, N'Programare.View'),
    (@ReceptionerRolID, N'Programare.Create'),
    (@ReceptionerRolID, N'Programare.Edit'),
    (@ReceptionerRolID, N'Programare.Delete'),
    (@ReceptionerRolID, N'Programare.Cancel'),
    (@ReceptionerRolID, N'Programare.Confirm'),
    (@ReceptionerRolID, N'Personal.View')
GO

PRINT 'Permisiuni Receptioner populate cu succes.'
GO

-- ========================================
-- POPULARE - Permisiuni pentru Manager
-- ========================================

DECLARE @ManagerRolID UNIQUEIDENTIFIER = (SELECT [RolID] FROM dbo.Roluri WHERE [Denumire] = N'Manager')

INSERT INTO dbo.RoluriPermisiuni ([RolID], [Permisiune])
VALUES 
    (@ManagerRolID, N'Pacient.View'),
    (@ManagerRolID, N'Pacient.Export'),
    (@ManagerRolID, N'Programare.View'),
    (@ManagerRolID, N'Personal.View'),
    (@ManagerRolID, N'Personal.Create'),
    (@ManagerRolID, N'Personal.Edit'),
    (@ManagerRolID, N'Admin.AccessDashboard'),
    (@ManagerRolID, N'Admin.ViewReports'),
    (@ManagerRolID, N'Admin.ExportData')
GO

PRINT 'Permisiuni Manager populate cu succes.'
GO

-- ========================================
-- VERIFICARE
-- ========================================

SELECT 'Roluri' AS Entitate, COUNT(*) AS Total FROM dbo.Roluri
UNION ALL
SELECT 'PermisiuniDefinitii', COUNT(*) FROM dbo.PermisiuniDefinitii
UNION ALL
SELECT 'RoluriPermisiuni', COUNT(*) FROM dbo.RoluriPermisiuni
GO

PRINT '========================================='
PRINT 'Script completat cu succes!'
PRINT '========================================='
GO
