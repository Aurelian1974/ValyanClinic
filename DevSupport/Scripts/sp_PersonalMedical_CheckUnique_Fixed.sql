-- =============================================
-- SP pentru verificarea unicit??ii Email ?i NumarLicenta
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PersonalMedical_CheckUnique]
    @Email VARCHAR(100) = NULL,
    @NumarLicenta VARCHAR(50) = NULL,
    @ExcludeId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        CASE WHEN EXISTS (
            SELECT 1 FROM PersonalMedical 
            WHERE Email = @Email 
            AND (@ExcludeId IS NULL OR PersonalID != @ExcludeId)
        ) THEN 1 ELSE 0 END as Email_Exists,
        
        CASE WHEN EXISTS (
            SELECT 1 FROM PersonalMedical 
            WHERE NumarLicenta = @NumarLicenta 
            AND (@ExcludeId IS NULL OR PersonalID != @ExcludeId)
        ) THEN 1 ELSE 0 END as NumarLicenta_Exists;
END;