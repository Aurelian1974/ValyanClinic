/*
==============================================================================
STORED PROCEDURE: Consultatie_GetByMedic
==============================================================================
Description: Get all consultations for a specific medic
Author: System
Date: 2026-01-02
Version: 1.0
==============================================================================
*/

USE [ValyanClinicDB]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Consultatie_GetByMedic')
    DROP PROCEDURE [dbo].[Consultatie_GetByMedic]
GO

CREATE PROCEDURE [dbo].[Consultatie_GetByMedic]
    @MedicID UNIQUEIDENTIFIER
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
    WHERE [MedicID] = @MedicID
    ORDER BY [DataConsultatie] DESC, [OraConsultatie] DESC
END
GO
