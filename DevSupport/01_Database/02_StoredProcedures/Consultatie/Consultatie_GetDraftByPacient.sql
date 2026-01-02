/*
==============================================================================
STORED PROCEDURE: Consultatie_GetDraftByPacient
==============================================================================
Description: Find draft consultation for a patient on a specific date
Author: System
Date: 2026-01-02
Version: 1.0
==============================================================================
*/

USE [ValyanClinicDB]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Consultatie_GetDraftByPacient')
    DROP PROCEDURE [dbo].[Consultatie_GetDraftByPacient]
GO

CREATE PROCEDURE [dbo].[Consultatie_GetDraftByPacient]
    @PacientID UNIQUEIDENTIFIER,
    @MedicID UNIQUEIDENTIFIER = NULL,
    @DataConsultatie DATE = NULL,
    @ProgramareID UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Default to today if no date provided
    IF @DataConsultatie IS NULL
        SET @DataConsultatie = CAST(GETDATE() AS DATE)
    
    -- Find draft consultation matching criteria
    -- Priority: exact ProgramareID match > same patient/date/medic
    SELECT TOP 1
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
    WHERE 
        [PacientID] = @PacientID
        AND [Status] NOT IN ('Finalizata', 'Anulata')
        AND CAST([DataConsultatie] AS DATE) = @DataConsultatie
        AND (@MedicID IS NULL OR [MedicID] = @MedicID)
        AND (@ProgramareID IS NULL OR [ProgramareID] = @ProgramareID OR [ProgramareID] IS NULL)
    ORDER BY 
        CASE WHEN [ProgramareID] = @ProgramareID THEN 0 ELSE 1 END,  -- Prefer exact match
        [DataCreare] DESC
END
GO
