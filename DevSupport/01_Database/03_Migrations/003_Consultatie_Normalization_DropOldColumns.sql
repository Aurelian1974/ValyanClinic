/*
==============================================================================
MIGRATION: Drop old monolithic columns from Consultatii table
Author: AI Agent
Date: 2026-01-02
Description: Remove 68 columns that have been moved to normalized tables
==============================================================================
*/

USE [ValyanMed]
GO

PRINT '========================================='
PRINT 'Starting Migration 003: Drop old columns'
PRINT '========================================='

-- Create backup table first (safety measure)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Consultatii_Backup_BeforeDrop]') AND type in (N'U'))
BEGIN
    PRINT 'Creating backup table Consultatii_Backup_BeforeDrop...'
    SELECT * INTO [dbo].[Consultatii_Backup_BeforeDrop] FROM [dbo].[Consultatii]
    PRINT '✓ Backup created successfully'
END
ELSE
BEGIN
    PRINT '⚠ Backup table already exists, skipping backup creation'
END
GO

-- Drop columns that are now in ConsultatieMotivePrezentare
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Consultatii]') AND name = 'MotivPrezentare')
BEGIN
    PRINT 'Dropping column: MotivPrezentare'
    ALTER TABLE [dbo].[Consultatii] DROP COLUMN [MotivPrezentare]
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Consultatii]') AND name = 'IstoricBoalaActuala')
BEGIN
    PRINT 'Dropping column: IstoricBoalaActuala'
    ALTER TABLE [dbo].[Consultatii] DROP COLUMN [IstoricBoalaActuala]
END
GO

-- Drop columns that are now in ConsultatieAntecedente
DECLARE @AntecedenteCols TABLE (ColName NVARCHAR(128))
INSERT INTO @AntecedenteCols VALUES 
    ('AHC_Mama'), ('AHC_Tata'), ('AHC_Frati'), ('AHC_Bunici'), ('AHC_Altele'),
    ('AF_Nastere'), ('AF_Dezvoltare'), ('AF_Menstruatie'), ('AF_Sarcini'), ('AF_Alaptare'),
    ('APP_BoliCopilarieAdolescenta'), ('APP_BoliAdult'), ('APP_Interventii'), ('APP_Traumatisme'), ('APP_Transfuzii'),
    ('APP_Alergii'), ('APP_Medicatie'),
    ('Profesie'), ('ConditiiLocuinta'), ('ConditiiMunca'), ('ObiceiuriAlimentare'), ('Toxice')

DECLARE @ColName NVARCHAR(128)
DECLARE col_cursor CURSOR FOR SELECT ColName FROM @AntecedenteCols
OPEN col_cursor
FETCH NEXT FROM col_cursor INTO @ColName

WHILE @@FETCH_STATUS = 0
BEGIN
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Consultatii]') AND name = @ColName)
    BEGIN
        DECLARE @sql NVARCHAR(MAX) = N'ALTER TABLE [dbo].[Consultatii] DROP COLUMN [' + @ColName + N']'
        PRINT 'Dropping column: ' + @ColName
        EXEC sp_executesql @sql
    END
    FETCH NEXT FROM col_cursor INTO @ColName
END

CLOSE col_cursor
DEALLOCATE col_cursor
GO

-- Drop columns that are now in ConsultatieExamenObiectiv
DECLARE @ExamenCols TABLE (ColName NVARCHAR(128))
INSERT INTO @ExamenCols VALUES 
    ('StareGenerala'), ('Constitutie'), ('Atitudine'), ('Facies'), ('Tegumente'), ('Mucoase'), ('GangliniLimfatici'),
    ('Greutate'), ('Inaltime'), ('IMC'), ('Temperatura'), ('TensiuneArteriala'), ('Puls'),
    ('FreccventaRespiratorie'), ('SaturatieO2'), ('Glicemie'),
    ('ExamenCardiovascular'), ('ExamenRespiratoriu'), ('ExamenDigestiv'), ('ExamenUrinar'),
    ('ExamenNervos'), ('ExamenLocomotor'), ('ExamenEndocrin'), ('ExamenORL'), ('ExamenOftalmologic'), ('ExamenDermatologic')

DECLARE @ColName2 NVARCHAR(128)
DECLARE col_cursor2 CURSOR FOR SELECT ColName FROM @ExamenCols
OPEN col_cursor2
FETCH NEXT FROM col_cursor2 INTO @ColName2

WHILE @@FETCH_STATUS = 0
BEGIN
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Consultatii]') AND name = @ColName2)
    BEGIN
        DECLARE @sql2 NVARCHAR(MAX) = N'ALTER TABLE [dbo].[Consultatii] DROP COLUMN [' + @ColName2 + N']'
        PRINT 'Dropping column: ' + @ColName2
        EXEC sp_executesql @sql2
    END
    FETCH NEXT FROM col_cursor2 INTO @ColName2
END

CLOSE col_cursor2
DEALLOCATE col_cursor2
GO

-- Drop columns that are now in ConsultatieInvestigatii
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Consultatii]') AND name = 'InvestigatiiLaborator')
    ALTER TABLE [dbo].[Consultatii] DROP COLUMN [InvestigatiiLaborator]

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Consultatii]') AND name = 'InvestigatiiImagistice')
    ALTER TABLE [dbo].[Consultatii] DROP COLUMN [InvestigatiiImagistice]

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Consultatii]') AND name = 'InvestigatiiEKG')
    ALTER TABLE [dbo].[Consultatii] DROP COLUMN [InvestigatiiEKG]

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Consultatii]') AND name = 'AlteInvestigatii')
    ALTER TABLE [dbo].[Consultatii] DROP COLUMN [AlteInvestigatii]
GO

-- Drop columns that are now in ConsultatieDiagnostic
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Consultatii]') AND name = 'DiagnosticPozitiv')
    ALTER TABLE [dbo].[Consultatii] DROP COLUMN [DiagnosticPozitiv]

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Consultatii]') AND name = 'DiagnosticDiferential')
    ALTER TABLE [dbo].[Consultatii] DROP COLUMN [DiagnosticDiferential]

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Consultatii]') AND name = 'DiagnosticEtiologic')
    ALTER TABLE [dbo].[Consultatii] DROP COLUMN [DiagnosticEtiologic]

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Consultatii]') AND name = 'CoduriICD10')
    ALTER TABLE [dbo].[Consultatii] DROP COLUMN [CoduriICD10]
GO

-- Drop columns that are now in ConsultatieTratament
DECLARE @TratamentCols TABLE (ColName NVARCHAR(128))
INSERT INTO @TratamentCols VALUES 
    ('TratamentMedicamentos'), ('TratamentNemedicamentos'), ('RecomandariDietetice'), ('RecomandariRegimViata'),
    ('InvestigatiiRecomandate'), ('ConsulturiSpecialitate'), ('DataUrmatoareiProgramari'), ('RecomandariSupraveghere')

DECLARE @ColName3 NVARCHAR(128)
DECLARE col_cursor3 CURSOR FOR SELECT ColName FROM @TratamentCols
OPEN col_cursor3
FETCH NEXT FROM col_cursor3 INTO @ColName3

WHILE @@FETCH_STATUS = 0
BEGIN
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Consultatii]') AND name = @ColName3)
    BEGIN
        DECLARE @sql3 NVARCHAR(MAX) = N'ALTER TABLE [dbo].[Consultatii] DROP COLUMN [' + @ColName3 + N']'
        PRINT 'Dropping column: ' + @ColName3
        EXEC sp_executesql @sql3
    END
    FETCH NEXT FROM col_cursor3 INTO @ColName3
END

CLOSE col_cursor3
DEALLOCATE col_cursor3
GO

-- Drop columns that are now in ConsultatieConcluzii (except DocumenteAtatate which stays in Concluzii)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Consultatii]') AND name = 'Prognostic')
    ALTER TABLE [dbo].[Consultatii] DROP COLUMN [Prognostic]

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Consultatii]') AND name = 'Concluzie')
    ALTER TABLE [dbo].[Consultatii] DROP COLUMN [Concluzie]

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Consultatii]') AND name = 'ObservatiiMedic')
    ALTER TABLE [dbo].[Consultatii] DROP COLUMN [ObservatiiMedic]

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Consultatii]') AND name = 'NotePacient')
    ALTER TABLE [dbo].[Consultatii] DROP COLUMN [NotePacient]

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Consultatii]') AND name = 'DocumenteAtatate')
    ALTER TABLE [dbo].[Consultatii] DROP COLUMN [DocumenteAtatate]
GO

PRINT '========================================='
PRINT 'Verifying final structure...'
PRINT '========================================='

-- Show remaining columns (should be only 17 master columns)
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Consultatii'
ORDER BY ORDINAL_POSITION

DECLARE @ColCount INT
SELECT @ColCount = COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Consultatii'
PRINT ''
PRINT '✓ Total columns remaining: ' + CAST(@ColCount AS VARCHAR(10))
PRINT ''

IF @ColCount = 17
BEGIN
    PRINT '✓✓✓ SUCCESS! Consultatii table now has ONLY master columns (17 total)'
    PRINT ''
    PRINT 'Master columns (expected 17):'
    PRINT '  1. ConsultatieID (PK)'
    PRINT '  2. ProgramareID (FK)'
    PRINT '  3. PacientID (FK)'
    PRINT '  4. MedicID (FK)'
    PRINT '  5. DataConsultatie'
    PRINT '  6. OraConsultatie'
    PRINT '  7. TipConsultatie'
    PRINT '  8. Status'
    PRINT '  9. DataFinalizare'
    PRINT ' 10. DurataMinute'
    PRINT ' 11. DataCreare'
    PRINT ' 12. CreatDe'
    PRINT ' 13. DataUltimeiModificari'
    PRINT ' 14. ModificatDe'
    PRINT ' 15. CoduriICD10Secundare (if exists)'
    PRINT ' 16. Edeme (if exists)'
    PRINT ' 17. InterpretareIMC (if exists)'
END
ELSE
BEGIN
    PRINT '⚠ WARNING: Column count is ' + CAST(@ColCount AS VARCHAR(10)) + ' instead of expected 17'
    PRINT 'Please verify the structure manually'
END

PRINT ''
PRINT '========================================='
PRINT 'Migration 003 completed!'
PRINT '========================================='
GO
