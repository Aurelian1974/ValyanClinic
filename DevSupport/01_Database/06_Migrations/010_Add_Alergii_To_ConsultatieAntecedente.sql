/*
==============================================================================
MIGRATION: 010_Add_Alergii_To_ConsultatieAntecedente
==============================================================================
Description: Adds Alergii column to ConsultatieAntecedente table for 
             Scrisoare Medicală Anexa 43 compliance.
             Also updates stored procedures for UPSERT and GET.
Author: AI Agent
Date: 2026-01-04
Database: ValyanMed
==============================================================================
*/

USE [ValyanMed]
GO

PRINT '========================================';
PRINT 'Migration: Add Alergii column';
PRINT 'Date: 2026-01-04';
PRINT '========================================';
PRINT '';

-- =============================================
-- 1. Add Alergii column to ConsultatieAntecedente
-- =============================================
PRINT '1. Adding Alergii column to ConsultatieAntecedente...';

IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ConsultatieAntecedente' 
    AND COLUMN_NAME = 'Alergii'
)
BEGIN
    ALTER TABLE [dbo].[ConsultatieAntecedente]
    ADD [Alergii] NVARCHAR(MAX) NULL;
    
    PRINT '   ✓ Coloana Alergii adăugată cu succes';
END
ELSE
BEGIN
    PRINT '   → Coloana Alergii există deja';
END
GO

-- =============================================
-- 2. Update ConsultatieAntecedente_Upsert stored procedure
-- =============================================
PRINT '';
PRINT '2. Updating ConsultatieAntecedente_Upsert stored procedure...';

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ConsultatieAntecedente_Upsert')
    DROP PROCEDURE [dbo].[ConsultatieAntecedente_Upsert]
GO

CREATE PROCEDURE [dbo].[ConsultatieAntecedente_Upsert]
    @ConsultatieID UNIQUEIDENTIFIER,
    @IstoricMedicalPersonal NVARCHAR(MAX) = NULL,
    @IstoricFamilial NVARCHAR(MAX) = NULL,
    @TratamentAnterior NVARCHAR(MAX) = NULL,
    @FactoriDeRisc NVARCHAR(MAX) = NULL,
    @Alergii NVARCHAR(MAX) = NULL,
    @ModificatDe UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        IF EXISTS (SELECT 1 FROM [dbo].[ConsultatieAntecedente] WHERE [ConsultatieID] = @ConsultatieID)
        BEGIN
            UPDATE [dbo].[ConsultatieAntecedente]
            SET 
                [IstoricMedicalPersonal] = @IstoricMedicalPersonal,
                [IstoricFamilial] = @IstoricFamilial,
                [TratamentAnterior] = @TratamentAnterior,
                [FactoriDeRisc] = @FactoriDeRisc,
                [Alergii] = @Alergii,
                [DataUltimeiModificari] = GETDATE(),
                [ModificatDe] = @ModificatDe
            WHERE [ConsultatieID] = @ConsultatieID;
        END
        ELSE
        BEGIN
            INSERT INTO [dbo].[ConsultatieAntecedente]
            (
                [Id], [ConsultatieID],
                [IstoricMedicalPersonal], [IstoricFamilial],
                [TratamentAnterior], [FactoriDeRisc], [Alergii],
                [DataCreare], [CreatDe]
            )
            VALUES
            (
                NEWID(), @ConsultatieID,
                @IstoricMedicalPersonal, @IstoricFamilial,
                @TratamentAnterior, @FactoriDeRisc, @Alergii,
                GETDATE(), @ModificatDe
            );
        END
        
        UPDATE [dbo].[Consultatii]
        SET [DataUltimeiModificari] = GETDATE(), [ModificatDe] = @ModificatDe
        WHERE [ConsultatieID] = @ConsultatieID;
        
        COMMIT TRANSACTION;
        
        SELECT * FROM [dbo].[ConsultatieAntecedente] WHERE [ConsultatieID] = @ConsultatieID;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT '   ✓ ConsultatieAntecedente_Upsert actualizat cu succes';

-- =============================================
-- 3. Update sp_Consultatie_GetDraftByPacient stored procedure
-- =============================================
PRINT '';
PRINT '3. Updating sp_Consultatie_GetDraftByPacient stored procedure...';

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
            
            -- ConsultatieAntecedente (1:1) - SIMPLIFIED STRUCTURE with Alergii
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
            
            -- ConsultatieDiagnostic (1:1)
            diag.Id AS Diagnostic_Id,
            diag.DiagnosticPozitiv AS Diagnostic_DiagnosticPozitiv,
            diag.DiagnosticDiferential AS Diagnostic_DiagnosticDiferential,
            diag.DiagnosticEtiologic AS Diagnostic_DiagnosticEtiologic,
            diag.CoduriICD10 AS Diagnostic_CoduriICD10,
            diag.CoduriICD10Secundare AS Diagnostic_CoduriICD10Secundare,
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
        LEFT JOIN ConsultatieMotivePrezentare mp ON mp.ConsultatieID = c.ConsultatieID
        LEFT JOIN ConsultatieAntecedente ant ON ant.ConsultatieID = c.ConsultatieID
        LEFT JOIN ConsultatieExamenObiectiv exo ON exo.ConsultatieID = c.ConsultatieID
        LEFT JOIN ConsultatieInvestigatii inv ON inv.ConsultatieID = c.ConsultatieID
        LEFT JOIN ConsultatieDiagnostic diag ON diag.ConsultatieID = c.ConsultatieID
        LEFT JOIN ConsultatieTratament trat ON trat.ConsultatieID = c.ConsultatieID
        LEFT JOIN ConsultatieConcluzii conc ON conc.ConsultatieID = c.ConsultatieID
        WHERE c.PacientID = @PacientID
            AND (@MedicID IS NULL OR c.MedicID = @MedicID)
            AND (@ProgramareID IS NULL OR c.ProgramareID = @ProgramareID)
            AND c.[Status] NOT IN ('Finalizata', 'Anulata')
        ORDER BY c.DataCreare DESC;
        
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

PRINT '   ✓ sp_Consultatie_GetDraftByPacient actualizat cu succes';

-- =============================================
-- Migration Complete
-- =============================================
PRINT '';
PRINT '========================================';
PRINT 'Migration completă!';
PRINT '- Coloana Alergii adăugată în ConsultatieAntecedente';
PRINT '- Stored procedures actualizate';
PRINT '========================================';
GO
