/*
==============================================================================
MIGRATION: Consultatie Normalization - Phase 1: Drop Old Structure
==============================================================================
Description: Drop existing Consultatii table to prepare for normalized structure
Author: System
Date: 2026-01-02
Version: 1.0

IMPORTANT: This will DELETE all existing consultation data!
Only run this script if you have confirmed data backup or starting fresh.
==============================================================================
*/

USE [ValyanClinicDB]
GO

PRINT '========================================='
PRINT 'Starting Consultatie Normalization Phase 1'
PRINT 'Dropping old monolithic structure...'
PRINT '========================================='
PRINT ''

-- Drop existing stored procedures for Consultatii (if any)
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Consultatie_GetById')
BEGIN
    PRINT 'Dropping stored procedure: Consultatie_GetById'
    DROP PROCEDURE [dbo].[Consultatie_GetById]
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Consultatie_Create')
BEGIN
    PRINT 'Dropping stored procedure: Consultatie_Create'
    DROP PROCEDURE [dbo].[Consultatie_Create]
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Consultatie_Update')
BEGIN
    PRINT 'Dropping stored procedure: Consultatie_Update'
    DROP PROCEDURE [dbo].[Consultatie_Update]
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Consultatie_Delete')
BEGIN
    PRINT 'Dropping stored procedure: Consultatie_Delete'
    DROP PROCEDURE [dbo].[Consultatie_Delete]
END
GO

-- Drop existing Consultatii table
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Consultatii')
BEGIN
    PRINT 'Dropping table: Consultatii (old monolithic structure)'
    DROP TABLE [dbo].[Consultatii]
    PRINT 'Table dropped successfully!'
END
ELSE
BEGIN
    PRINT 'Table Consultatii does not exist - nothing to drop'
END
GO

PRINT ''
PRINT '========================================='
PRINT 'Phase 1 Complete!'
PRINT 'Old structure removed successfully.'
PRINT 'Ready for normalized structure creation.'
PRINT '========================================='
GO
