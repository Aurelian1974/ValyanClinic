/*
==============================================================================
STORED PROCEDURE: Consultatie_GetByPacient
==============================================================================
Description: Retrieves all consultatii for a specific patient (MASTER records only)
Author: System
Date: 2026-01-02
Version: 2.0 (Normalized Structure)
==============================================================================
*/

USE [ValyanClinicDB]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Consultatie_GetByPacient')
    DROP PROCEDURE [dbo].[Consultatie_GetByPacient]
GO

CREATE PROCEDURE [dbo].[Consultatie_GetByPacient]
    @PacientID UNIQUEIDENTIFIER,
    @IncludeFinalizate BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [ConsultatieID],
        [ProgramareID],
        [PacientID],
        [MedicID],
        [DataConsultatie],
        [OraConsultatie],
        [TipConsultatie],
        [Status],
        [DataFinalizare],
        [DurataMinute],
        [DataCreare],
        [CreatDe],
        [DataUltimeiModificari],
        [ModificatDe]
    FROM [dbo].[Consultatii]
    WHERE [PacientID] = @PacientID
        AND (@IncludeFinalizate = 1 OR [Status] != 'Finalizata')
    ORDER BY [DataConsultatie] DESC, [OraConsultatie] DESC;
END
GO
