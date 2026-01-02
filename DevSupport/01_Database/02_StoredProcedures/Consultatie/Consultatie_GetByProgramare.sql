/*
==============================================================================
STORED PROCEDURE: Consultatie_GetByProgramare
==============================================================================
Description: Get consultation for a specific programare
Author: System
Date: 2026-01-02
Version: 1.0
==============================================================================
*/

USE [ValyanClinicDB]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Consultatie_GetByProgramare')
    DROP PROCEDURE [dbo].[Consultatie_GetByProgramare]
GO

CREATE PROCEDURE [dbo].[Consultatie_GetByProgramare]
    @ProgramareID UNIQUEIDENTIFIER
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
    WHERE [ProgramareID] = @ProgramareID
END
GO
