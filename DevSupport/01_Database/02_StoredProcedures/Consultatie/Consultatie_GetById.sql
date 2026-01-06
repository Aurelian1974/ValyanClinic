/*
==============================================================================
STORED PROCEDURE: Consultatie_GetById
==============================================================================
Description: Retrieves a complete consultatie with all related details
Author: System
Date: 2026-01-02
Version: 2.0 (Normalized Structure)

Returns multiple result sets:
    1. Master consultatie data
    2. Motive prezentare
    3. Antecedente
    4. Examen obiectiv
    5. Investigatii
    6. Analize medicale
    7. Diagnostic
    8. Tratament
    9. Concluzii

Usage:
    EXEC Consultatie_GetById @ConsultatieID = '...'
==============================================================================
*/

USE [ValyanClinicDB]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Consultatie_GetById')
    DROP PROCEDURE [dbo].[Consultatie_GetById]
GO

CREATE PROCEDURE [dbo].[Consultatie_GetById]
    @ConsultatieID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Result Set 1: Master data
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
    WHERE [ConsultatieID] = @ConsultatieID;
    
    -- Result Set 2: Motive Prezentare
    SELECT 
        [Id],
        [ConsultatieID],
        [MotivPrezentare],
        [IstoricBoalaActuala],
        [DataCreare],
        [CreatDe],
        [DataUltimeiModificari],
        [ModificatDe]
    FROM [dbo].[ConsultatieMotivePrezentare]
    WHERE [ConsultatieID] = @ConsultatieID;
    
    -- Result Set 3: Antecedente
    SELECT 
        [Id],
        [ConsultatieID],
        [AHC_Mama],
        [AHC_Tata],
        [AHC_Frati],
        [AHC_Bunici],
        [AHC_Altele],
        [AF_Nastere],
        [AF_Dezvoltare],
        [AF_Menstruatie],
        [AF_Sarcini],
        [AF_Alaptare],
        [APP_BoliCopilarieAdolescenta],
        [APP_BoliAdult],
        [APP_Interventii],
        [APP_Traumatisme],
        [APP_Transfuzii],
        [APP_Alergii],
        [APP_Medicatie],
        [Profesie],
        [ConditiiLocuinta],
        [ConditiiMunca],
        [ObiceiuriAlimentare],
        [Toxice],
        [DataCreare],
        [CreatDe],
        [DataUltimeiModificari],
        [ModificatDe]
    FROM [dbo].[ConsultatieAntecedente]
    WHERE [ConsultatieID] = @ConsultatieID;
    
    -- Result Set 4: Examen Obiectiv
    SELECT 
        [Id],
        [ConsultatieID],
        [StareGenerala],
        [Constitutie],
        [Atitudine],
        [Facies],
        [Tegumente],
        [Mucoase],
        [GanglioniLimfatici],
        [Edeme],
        [Greutate],
        [Inaltime],
        [IMC],
        [Temperatura],
        [TensiuneArteriala],
        [Puls],
        [FreccventaRespiratorie],
        [SaturatieO2],
        [Glicemie],
        [ExamenCardiovascular],
        [ExamenRespiratoriu],
        [ExamenDigestiv],
        [ExamenUrinar],
        [ExamenNervos],
        [ExamenLocomotor],
        [ExamenEndocrin],
        [ExamenORL],
        [ExamenOftalmologic],
        [ExamenDermatologic],
        [ExamenObiectivDetaliat],
        [AlteObservatiiClinice],
        [DataCreare],
        [CreatDe],
        [DataUltimeiModificari],
        [ModificatDe]
    FROM [dbo].[ConsultatieExamenObiectiv]
    WHERE [ConsultatieID] = @ConsultatieID;
    
    -- Result Set 5: Investigatii
    SELECT 
        [Id],
        [ConsultatieID],
        [InvestigatiiLaborator],
        [InvestigatiiImagistice],
        [InvestigatiiEKG],
        [AlteInvestigatii],
        [DataCreare],
        [CreatDe],
        [DataUltimeiModificari],
        [ModificatDe]
    FROM [dbo].[ConsultatieInvestigatii]
    WHERE [ConsultatieID] = @ConsultatieID;
    
    -- Result Set 6: Analize Medicale (with details)
    SELECT 
        am.[Id],
        am.[ConsultatieID],
        am.[TipAnaliza],
        am.[NumeAnaliza],
        am.[CodAnaliza],
        am.[StatusAnaliza],
        am.[DataRecomandare],
        am.[DataProgramata],
        am.[DataEfectuare],
        am.[LocEfectuare],
        am.[Prioritate],
        am.[EsteCito],
        am.[IndicatiiClinice],
        am.[ObservatiiRecomandare],
        am.[AreRezultate],
        am.[DataRezultate],
        am.[ValoareRezultat],
        am.[UnitatiMasura],
        am.[ValoareNormalaMin],
        am.[ValoareNormalaMax],
        am.[EsteInAfaraLimitelor],
        am.[InterpretareMedic],
        am.[ConclusiiAnaliza],
        am.[CaleFisierRezultat],
        am.[TipFisier],
        am.[DimensiuneFisier],
        am.[Pret],
        am.[Decontat],
        am.[LaboratorID],
        am.[MedicInterpretareID],
        am.[DataCreare],
        am.[CreatDe],
        am.[DataUltimeiModificari],
        am.[ModificatDe]
    FROM [dbo].[ConsultatieAnalizeMedicale] am
    WHERE am.[ConsultatieID] = @ConsultatieID
    ORDER BY am.[DataRecomandare] DESC;
    
    -- Result Set 7: Analize Detalii (for all analize)
    SELECT 
        ad.[Id],
        ad.[AnalizaMedicalaID],
        ad.[NumeParametru],
        ad.[CodParametru],
        ad.[Valoare],
        ad.[UnitatiMasura],
        ad.[TipValoare],
        ad.[ValoareNormalaMin],
        ad.[ValoareNormalaMax],
        ad.[ValoareNormalaText],
        ad.[EsteAnormal],
        ad.[NivelGravitate],
        ad.[Observatii],
        ad.[DataCreare]
    FROM [dbo].[ConsultatieAnalizaDetalii] ad
    INNER JOIN [dbo].[ConsultatieAnalizeMedicale] am ON ad.[AnalizaMedicalaID] = am.[Id]
    WHERE am.[ConsultatieID] = @ConsultatieID
    ORDER BY ad.[AnalizaMedicalaID], ad.[NumeParametru];
    
    -- Result Set 8: Diagnostic
    SELECT 
        [Id],
        [ConsultatieID],
        [DiagnosticPozitiv],
        [DiagnosticDiferential],
        [DiagnosticEtiologic],
        [CoduriICD10],
        [CoduriICD10Secundare],
        [DataCreare],
        [CreatDe],
        [DataUltimeiModificari],
        [ModificatDe]
    FROM [dbo].[ConsultatieDiagnostic]
    WHERE [ConsultatieID] = @ConsultatieID;
    
    -- Result Set 9: Tratament
    SELECT 
        [Id],
        [ConsultatieID],
        [TratamentMedicamentos],
        [TratamentNemedicamentos],
        [RecomandariDietetice],
        [RecomandariRegimViata],
        [InvestigatiiRecomandate],
        [ConsulturiSpecialitate],
        [DataUrmatoareiProgramari],
        [RecomandariSupraveghere],
        [DataCreare],
        [CreatDe],
        [DataUltimeiModificari],
        [ModificatDe]
    FROM [dbo].[ConsultatieTratament]
    WHERE [ConsultatieID] = @ConsultatieID;
    
    -- Result Set 10: Concluzii
    SELECT 
        [Id],
        [ConsultatieID],
        [Prognostic],
        [Concluzie],
        [ObservatiiMedic],
        [NotePacient],
        [DocumenteAtatate],
        -- Scrisoare Medicala - Anexa 43
        [EsteAfectiuneOncologica],
        [DetaliiAfectiuneOncologica],
        [AreIndicatieInternare],
        [TermenInternare],
        [SaEliberatPrescriptie],
        [SeriePrescriptie],
        [SaEliberatConcediuMedical],
        [SerieConcediuMedical],
        [SaEliberatIngrijiriDomiciliu],
        [SaEliberatDispozitiveMedicale],
        [TransmiterePrinEmail],
        [EmailTransmitere],
        [DataCreare],
        [CreatDe],
        [DataUltimeiModificari],
        [ModificatDe]
    FROM [dbo].[ConsultatieConcluzii]
    WHERE [ConsultatieID] = @ConsultatieID;
END
GO
