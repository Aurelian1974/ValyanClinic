/*
==============================================================================
STORED PROCEDURE: sp_Consultatie_GetDraftByPacient
==============================================================================
Description: Get draft consultation with all normalized sections by PacientID
Author: AI Agent
Date: 2026-01-02
Version: 1.0

Usage:
    EXEC sp_Consultatie_GetDraftByPacient
        @PacientID = '...',
        @MedicID = '...',
        @DataConsultatie = '2026-01-02',
        @ProgramareID = '...'
==============================================================================
*/

USE [ValyanMed]
GO

PRINT 'Creating sp_Consultatie_GetDraftByPacient...'

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_Consultatie_GetDraftByPacient')
    DROP PROCEDURE sp_Consultatie_GetDraftByPacient;
GO

CREATE PROCEDURE sp_Consultatie_GetDraftByPacient
    @PacientID UNIQUEIDENTIFIER,
    @MedicID UNIQUEIDENTIFIER = NULL,
    @DataConsultatie DATE = NULL,
    @ProgramareID UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Default to today if no date provided
        IF @DataConsultatie IS NULL
            SET @DataConsultatie = CAST(GETDATE() AS DATE);
        
        -- Find draft consultation (Status != 'Finalizata', 'Anulata')
        SELECT TOP 1
            -- Master table
            c.*,
            
            -- ConsultatieMotivePrezentare (1:1)
            mp.Id AS MotivePrezentare_Id,
            mp.MotivPrezentare AS MotivePrezentare_MotivPrezentare,
            mp.IstoricBoalaActuala AS MotivePrezentare_IstoricBoalaActuala,
            mp.DataCreare AS MotivePrezentare_DataCreare,
            mp.CreatDe AS MotivePrezentare_CreatDe,
            mp.DataUltimeiModificari AS MotivePrezentare_DataUltimeiModificari,
            mp.ModificatDe AS MotivePrezentare_ModificatDe,
            
            -- ConsultatieAntecedente (1:1) - SIMPLIFIED STRUCTURE
            ant.Id AS Antecedente_Id,
            ant.IstoricMedicalPersonal AS Antecedente_IstoricMedicalPersonal,
            ant.IstoricFamilial AS Antecedente_IstoricFamilial,
            ant.TratamentAnterior AS Antecedente_TratamentAnterior,
            ant.FactoriDeRisc AS Antecedente_FactoriDeRisc,
            ant.Alergii AS Antecedente_Alergii,
            ant.DataCreare AS Antecedente_DataCreare,
            ant.CreatDe AS Antecedente_CreatDe,
            ant.DataUltimeiModificari AS Antecedente_DataUltimeiModificari,
            ant.ModificatDe AS Antecedente_ModificatDe,
            
            -- ConsultatieExamenObiectiv (1:1)
            exo.Id AS ExamenObiectiv_Id,
            exo.StareGenerala AS ExamenObiectiv_StareGenerala,
            exo.Constitutie AS ExamenObiectiv_Constitutie,
            exo.Atitudine AS ExamenObiectiv_Atitudine,
            exo.Facies AS ExamenObiectiv_Facies,
            exo.Tegumente AS ExamenObiectiv_Tegumente,
            exo.Mucoase AS ExamenObiectiv_Mucoase,
            exo.GangliniLimfatici AS ExamenObiectiv_GangliniLimfatici,
            exo.Edeme AS ExamenObiectiv_Edeme,
            exo.Greutate AS ExamenObiectiv_Greutate,
            exo.Inaltime AS ExamenObiectiv_Inaltime,
            exo.IMC AS ExamenObiectiv_IMC,
            exo.Temperatura AS ExamenObiectiv_Temperatura,
            exo.TensiuneArteriala AS ExamenObiectiv_TensiuneArteriala,
            exo.Puls AS ExamenObiectiv_Puls,
            exo.FreccventaRespiratorie AS ExamenObiectiv_FreccventaRespiratorie,
            exo.SaturatieO2 AS ExamenObiectiv_SaturatieO2,
            exo.Glicemie AS ExamenObiectiv_Glicemie,
            exo.ExamenCardiovascular AS ExamenObiectiv_ExamenCardiovascular,
            exo.ExamenRespiratoriu AS ExamenObiectiv_ExamenRespiratoriu,
            exo.ExamenDigestiv AS ExamenObiectiv_ExamenDigestiv,
            exo.ExamenUrinar AS ExamenObiectiv_ExamenUrinar,
            exo.ExamenNervos AS ExamenObiectiv_ExamenNervos,
            exo.ExamenLocomotor AS ExamenObiectiv_ExamenLocomotor,
            exo.ExamenEndocrin AS ExamenObiectiv_ExamenEndocrin,
            exo.ExamenORL AS ExamenObiectiv_ExamenORL,
            exo.ExamenOftalmologic AS ExamenObiectiv_ExamenOftalmologic,
            exo.ExamenDermatologic AS ExamenObiectiv_ExamenDermatologic,
            exo.DataCreare AS ExamenObiectiv_DataCreare,
            exo.CreatDe AS ExamenObiectiv_CreatDe,
            exo.DataUltimeiModificari AS ExamenObiectiv_DataUltimeiModificari,
            exo.ModificatDe AS ExamenObiectiv_ModificatDe,
            
            -- ConsultatieInvestigatii (1:1)
            inv.Id AS Investigatii_Id,
            inv.InvestigatiiLaborator AS Investigatii_InvestigatiiLaborator,
            inv.InvestigatiiImagistice AS Investigatii_InvestigatiiImagistice,
            inv.InvestigatiiEKG AS Investigatii_InvestigatiiEKG,
            inv.AlteInvestigatii AS Investigatii_AlteInvestigatii,
            inv.DataCreare AS Investigatii_DataCreare,
            inv.CreatDe AS Investigatii_CreatDe,
            inv.DataUltimeiModificari AS Investigatii_DataUltimeiModificari,
            inv.ModificatDe AS Investigatii_ModificatDe,
            
            -- ConsultatieDiagnostic (1:1) - Normalized structure
            diag.Id AS Diagnostic_Id,
            diag.CodICD10Principal AS Diagnostic_CodICD10Principal,
            diag.NumeDiagnosticPrincipal AS Diagnostic_NumeDiagnosticPrincipal,
            diag.DescriereDetaliataPrincipal AS Diagnostic_DescriereDetaliataPrincipal,
            -- LEGACY fields for backwards compatibility
            diag.DiagnosticPozitiv AS Diagnostic_DiagnosticPozitiv,
            diag.CoduriICD10 AS Diagnostic_CoduriICD10,
            diag.DataCreare AS Diagnostic_DataCreare,
            diag.CreatDe AS Diagnostic_CreatDe,
            diag.DataUltimeiModificari AS Diagnostic_DataUltimeiModificari,
            diag.ModificatDe AS Diagnostic_ModificatDe,
            
            -- ConsultatieTratament (1:1)
            trat.Id AS Tratament_Id,
            trat.TratamentMedicamentos AS Tratament_TratamentMedicamentos,
            trat.TratamentNemedicamentos AS Tratament_TratamentNemedicamentos,
            trat.RecomandariDietetice AS Tratament_RecomandariDietetice,
            trat.RecomandariRegimViata AS Tratament_RecomandariRegimViata,
            trat.InvestigatiiRecomandate AS Tratament_InvestigatiiRecomandate,
            trat.ConsulturiSpecialitate AS Tratament_ConsulturiSpecialitate,
            trat.DataUrmatoareiProgramari AS Tratament_DataUrmatoareiProgramari,
            trat.RecomandariSupraveghere AS Tratament_RecomandariSupraveghere,
            trat.DataCreare AS Tratament_DataCreare,
            trat.CreatDe AS Tratament_CreatDe,
            trat.DataUltimeiModificari AS Tratament_DataUltimeiModificari,
            trat.ModificatDe AS Tratament_ModificatDe,
            
            -- ConsultatieConcluzii (1:1)
            conc.Id AS Concluzii_Id,
            conc.Prognostic AS Concluzii_Prognostic,
            conc.Concluzie AS Concluzii_Concluzie,
            conc.ObservatiiMedic AS Concluzii_ObservatiiMedic,
            conc.NotePacient AS Concluzii_NotePacient,
            conc.DocumenteAtatate AS Concluzii_DocumenteAtatate,
            conc.DataCreare AS Concluzii_DataCreare,
            conc.CreatDe AS Concluzii_CreatDe,
            conc.DataUltimeiModificari AS Concluzii_DataUltimeiModificari,
            conc.ModificatDe AS Concluzii_ModificatDe
            
        FROM Consultatii c
        LEFT JOIN ConsultatieMotivePrezentare mp ON c.ConsultatieID = mp.ConsultatieID
        LEFT JOIN ConsultatieAntecedente ant ON c.ConsultatieID = ant.ConsultatieID
        LEFT JOIN ConsultatieExamenObiectiv exo ON c.ConsultatieID = exo.ConsultatieID
        LEFT JOIN ConsultatieInvestigatii inv ON c.ConsultatieID = inv.ConsultatieID
        LEFT JOIN ConsultatieDiagnostic diag ON c.ConsultatieID = diag.ConsultatieID
        LEFT JOIN ConsultatieTratament trat ON c.ConsultatieID = trat.ConsultatieID
        LEFT JOIN ConsultatieConcluzii conc ON c.ConsultatieID = conc.ConsultatieID
        
        WHERE 
          -- When ProgramareID is provided, use it as primary filter (ignore PacientID)
          (
              (@ProgramareID IS NOT NULL AND c.ProgramareID = @ProgramareID)
              OR 
              (@ProgramareID IS NULL AND c.PacientID = @PacientID)
          )
          AND c.[Status] NOT IN ('Finalizata', 'Anulata')
          AND (@MedicID IS NULL OR c.MedicID = @MedicID)
        
        ORDER BY c.DataCreare DESC;
          
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

PRINT '✓ sp_Consultatie_GetDraftByPacient created successfully'
GO
