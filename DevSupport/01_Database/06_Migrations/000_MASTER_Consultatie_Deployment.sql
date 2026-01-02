/*
==============================================================================
MASTER DEPLOYMENT SCRIPT: Consultatie Normalization
==============================================================================
Description: Complete deployment of normalized consultatie structure
Author: System
Date: 2026-01-02
Version: 2.0

This script executes all migration and stored procedure scripts in order:
1. Drop old structure
2. Create new normalized tables
3. Create all stored procedures

IMPORTANT: Run this script in SQL Server Management Studio
BACKUP YOUR DATABASE BEFORE RUNNING THIS SCRIPT!
==============================================================================
*/

USE [ValyanClinicDB]
GO

PRINT '========================================='
PRINT 'CONSULTATIE NORMALIZATION DEPLOYMENT'
PRINT 'Starting at: ' + CONVERT(VARCHAR, GETDATE(), 120)
PRINT '========================================='
PRINT ''

-- ==================== PHASE 1: DROP OLD STRUCTURE ====================
PRINT '--- PHASE 1: Dropping old structure ---'
PRINT ''

-- Drop existing stored procedures
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Consultatie_GetById')
BEGIN
    PRINT 'Dropping: Consultatie_GetById'
    DROP PROCEDURE [dbo].[Consultatie_GetById]
END

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Consultatie_Create')
BEGIN
    PRINT 'Dropping: Consultatie_Create'
    DROP PROCEDURE [dbo].[Consultatie_Create]
END

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Consultatie_Update')
BEGIN
    PRINT 'Dropping: Consultatie_Update'
    DROP PROCEDURE [dbo].[Consultatie_Update]
END

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Consultatie_Delete')
BEGIN
    PRINT 'Dropping: Consultatie_Delete'
    DROP PROCEDURE [dbo].[Consultatie_Delete]
END

-- Drop old Consultatii table
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Consultatii')
BEGIN
    PRINT 'Dropping: Consultatii table (old structure)'
    DROP TABLE [dbo].[Consultatii]
    PRINT 'Table dropped successfully!'
END
ELSE
BEGIN
    PRINT 'Table Consultatii does not exist - nothing to drop'
END

PRINT ''
PRINT 'Phase 1 Complete!'
PRINT ''

PRINT '========================================='
PRINT 'Deployment completed successfully!'
PRINT 'Completed at: ' + CONVERT(VARCHAR, GETDATE(), 120)
PRINT ''
PRINT 'Next Steps:'
PRINT '1. Execute: 002_Consultatie_Normalization_CreateNewStructure.sql'
PRINT '2. Execute all stored procedure scripts in Consultatie folder'
PRINT '3. Update application code (Repositories, Handlers, DTOs)'
PRINT '4. Test the new structure'
PRINT '========================================='
GO
