-- ============================================================================
-- Migration: Make ProgramareID Nullable in Consultatii table
-- Description: Allow consultations without a programare (walk-in patients)
-- Date: 2025-01-13
-- Author: DevSupport
-- ============================================================================

USE [ValyanMed]
GO

PRINT '============================================================================'
PRINT 'Starting migration: Make ProgramareID Nullable'
PRINT '============================================================================'

-- Step 1: Check if constraint exists and drop it
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Consultatii_Programari')
BEGIN
    PRINT 'Dropping foreign key constraint FK_Consultatii_Programari...'
    ALTER TABLE dbo.Consultatii DROP CONSTRAINT FK_Consultatii_Programari
    PRINT '? Foreign key constraint dropped'
END
ELSE
BEGIN
    PRINT '?? Foreign key constraint FK_Consultatii_Programari not found (may have different name)'
    
    -- Try to find and drop any FK on ProgramareID column
    DECLARE @ConstraintName NVARCHAR(255)
    SELECT @ConstraintName = fk.name
    FROM sys.foreign_keys fk
    INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
    INNER JOIN sys.columns c ON fkc.parent_column_id = c.column_id AND fkc.parent_object_id = c.object_id
    WHERE fk.parent_object_id = OBJECT_ID('dbo.Consultatii')
      AND c.name = 'ProgramareID'
    
    IF @ConstraintName IS NOT NULL
    BEGIN
        PRINT 'Found FK constraint: ' + @ConstraintName
        EXEC('ALTER TABLE dbo.Consultatii DROP CONSTRAINT ' + @ConstraintName)
        PRINT '? Foreign key constraint dropped'
    END
END

-- Step 2: Alter column to allow NULL
PRINT 'Altering ProgramareID column to allow NULL...'

ALTER TABLE dbo.Consultatii
ALTER COLUMN ProgramareID UNIQUEIDENTIFIER NULL

PRINT '? ProgramareID column is now NULLABLE'

-- Step 3: Re-add the foreign key constraint (optional - if needed)
-- Only add if you want to maintain referential integrity for non-null values
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Consultatii_Programari')
BEGIN
    PRINT 'Re-creating foreign key constraint FK_Consultatii_Programari (with ON DELETE SET NULL)...'
    
    ALTER TABLE dbo.Consultatii
    ADD CONSTRAINT FK_Consultatii_Programari
    FOREIGN KEY (ProgramareID) REFERENCES dbo.Programari(ProgramareID)
    ON DELETE SET NULL
    ON UPDATE CASCADE
    
    PRINT '? Foreign key constraint re-created with ON DELETE SET NULL'
END

-- Step 4: Verify the change
PRINT ''
PRINT '============================================================================'
PRINT 'Verification:'
PRINT '============================================================================'

SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.is_nullable AS IsNullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.Consultatii')
  AND c.name = 'ProgramareID'

PRINT ''
PRINT '? Migration completed successfully!'
PRINT '============================================================================'
GO
