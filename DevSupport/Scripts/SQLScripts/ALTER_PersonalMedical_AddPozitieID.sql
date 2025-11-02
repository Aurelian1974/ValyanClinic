-- =============================================
-- ALTER TABLE: Add PozitieID to PersonalMedical
-- Adds FK to Pozitii table
-- Database: ValyanMed
-- =============================================

USE [ValyanMed]
GO

PRINT '=============================================='
PRINT 'Adding PozitieID to PersonalMedical'
PRINT '=============================================='
PRINT ''

-- Step 1: Check if column already exists
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'PersonalMedical' AND COLUMN_NAME = 'PozitieID')
BEGIN
    PRINT 'Step 1: Adding PozitieID column...'
    
    ALTER TABLE PersonalMedical
    ADD PozitieID UNIQUEIDENTIFIER NULL;
    
    PRINT '[OK] PozitieID column added'
END
ELSE
BEGIN
    PRINT '[INFO] PozitieID column already exists'
END
GO

-- Step 2: Add Foreign Key constraint
IF NOT EXISTS (SELECT * FROM sys.foreign_keys 
               WHERE name = 'FK_PersonalMedical_Pozitie' 
               AND parent_object_id = OBJECT_ID('PersonalMedical'))
BEGIN
    PRINT 'Step 2: Adding FK constraint...'
    
    ALTER TABLE PersonalMedical
    ADD CONSTRAINT FK_PersonalMedical_Pozitie
    FOREIGN KEY (PozitieID) REFERENCES Pozitii(Id)
    ON DELETE SET NULL;
    
    PRINT '[OK] FK constraint added'
END
ELSE
BEGIN
    PRINT '[INFO] FK constraint already exists'
END
GO

-- Step 3: Create index for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes 
               WHERE name = 'IX_PersonalMedical_PozitieID' 
               AND object_id = OBJECT_ID('PersonalMedical'))
BEGIN
    PRINT 'Step 3: Creating index...'
    
    CREATE INDEX IX_PersonalMedical_PozitieID 
    ON PersonalMedical(PozitieID);
    
    PRINT '[OK] Index created'
END
ELSE
BEGIN
    PRINT '[INFO] Index already exists'
END
GO

-- Verification
PRINT ''
PRINT '=============================================='
PRINT 'VERIFICATION'
PRINT '=============================================='

-- Check column
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'PersonalMedical' AND COLUMN_NAME = 'PozitieID')
    PRINT '[OK] PozitieID column exists'
ELSE
    PRINT '[ERROR] PozitieID column NOT found'

-- Check FK
IF EXISTS (SELECT * FROM sys.foreign_keys 
           WHERE name = 'FK_PersonalMedical_Pozitie')
    PRINT '[OK] FK constraint exists'
ELSE
    PRINT '[ERROR] FK constraint NOT found'

-- Check Index
IF EXISTS (SELECT * FROM sys.indexes 
           WHERE name = 'IX_PersonalMedical_PozitieID')
    PRINT '[OK] Index exists'
ELSE
    PRINT '[ERROR] Index NOT found'

PRINT ''
PRINT '=============================================='
PRINT 'COMPLETE!'
PRINT '=============================================='
PRINT ''
PRINT 'PersonalMedical now has:'
PRINT '  - PozitieID (UNIQUEIDENTIFIER NULL)'
PRINT '  - FK to Pozitii(Id)'
PRINT '  - Index for better performance'
PRINT ''
PRINT 'READY TO USE!'
PRINT '=============================================='
