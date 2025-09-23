-- =============================================
-- PERSONAL MEDICAL - MISSING STORED PROCEDURES FIX
-- This script creates the missing stored procedures that are causing errors
-- in the AdministrarePersonalMedical page
-- =============================================

-- Check if database has PersonalMedical table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PersonalMedical')
BEGIN
    PRINT 'WARNING: PersonalMedical table does not exist. Please create the table first.'
    RETURN
END

PRINT 'Creating missing stored procedures for PersonalMedical...'

-- =============================================
-- SP pentru ob?inerea distribu?iei personalului medical pe departamente
-- CRITICAL: This procedure was missing and causing the SqlException
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PersonalMedical_GetDistributiePerDepartament]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ISNULL(pm.Departament, 'Nedefinit') as Categorie,
        COUNT(*) as Numar
    FROM PersonalMedical pm
    WHERE pm.EsteActiv = 1  -- Doar personalul activ
    GROUP BY pm.Departament
    ORDER BY Numar DESC, Categorie ASC;
END;

PRINT 'Created: sp_PersonalMedical_GetDistributiePerDepartament'

-- =============================================
-- SP pentru ob?inerea distribu?iei personalului medical pe specializ?ri
-- CRITICAL: This procedure was also missing and would cause errors
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PersonalMedical_GetDistributiePerSpecializare]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        CASE 
            WHEN pm.Specializare IS NULL OR pm.Specializare = '' THEN 'Nespecializat'
            ELSE pm.Specializare
        END as Categorie,
        COUNT(*) as Numar
    FROM PersonalMedical pm
    WHERE pm.EsteActiv = 1  -- Doar personalul activ
    GROUP BY pm.Specializare
    ORDER BY Numar DESC, Categorie ASC;
END;

PRINT 'Created: sp_PersonalMedical_GetDistributiePerSpecializare'

-- =============================================
-- Verify the stored procedures were created successfully
-- =============================================
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_NAME = 'sp_PersonalMedical_GetDistributiePerDepartament' AND ROUTINE_TYPE = 'PROCEDURE')
    PRINT '? sp_PersonalMedical_GetDistributiePerDepartament - OK'
ELSE
    PRINT '? sp_PersonalMedical_GetDistributiePerDepartament - FAILED'

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_NAME = 'sp_PersonalMedical_GetDistributiePerSpecializare' AND ROUTINE_TYPE = 'PROCEDURE')
    PRINT '? sp_PersonalMedical_GetDistributiePerSpecializare - OK'
ELSE
    PRINT '? sp_PersonalMedical_GetDistributiePerSpecializare - FAILED'

-- =============================================
-- Test the procedures with sample data (if table has data)
-- =============================================
DECLARE @RecordCount INT;
SELECT @RecordCount = COUNT(*) FROM PersonalMedical;

IF @RecordCount > 0
BEGIN
    PRINT CHAR(13) + 'Testing procedures with existing data:'
    
    PRINT 'Testing sp_PersonalMedical_GetDistributiePerDepartament:'
    EXEC sp_PersonalMedical_GetDistributiePerDepartament;
    
    PRINT CHAR(13) + 'Testing sp_PersonalMedical_GetDistributiePerSpecializare:'
    EXEC sp_PersonalMedical_GetDistributiePerSpecializare;
END
ELSE
BEGIN
    PRINT 'No data found in PersonalMedical table - procedures created but not tested.'
END

PRINT CHAR(13) + 'Missing stored procedures fix completed successfully!'
PRINT 'The AdministrarePersonalMedical page should now load without SqlException errors.'