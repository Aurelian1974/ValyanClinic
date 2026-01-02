-- Test what Dapper receives from sp_Consultatie_GetDraftByPacient
-- This should match exactly what the Repository calls

USE ValyanMed;
GO

EXEC sp_Consultatie_GetDraftByPacient 
    @ProgramareID = '329DE6EB-F2C1-F011-B33E-94BB437E849C',
    @PacientID = '00000000-0000-0000-0000-000000000000'; -- Guid.Empty as Repository sends
GO
