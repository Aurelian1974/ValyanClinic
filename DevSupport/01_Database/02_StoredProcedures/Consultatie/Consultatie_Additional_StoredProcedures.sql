-- =============================================
-- Additional Stored Procedures pentru Consultatii Medicale
-- Versiune: 1.0
-- Data: 2025-01-08
-- Database: ValyanMed
-- Procedures: UPDATE, SaveDraft, Finalize
-- =============================================

USE ValyanMed;
GO

PRINT '========================================';
PRINT 'Creare Stored Procedures Aditionale';
PRINT 'Database: ValyanMed';
PRINT '========================================';
PRINT '';

-- =============================================
-- 1. sp_Consultatie_Update
-- Actualizeaza consultatie existenta (pentru auto-save)
-- =============================================
PRINT '1. Creare sp_Consultatie_Update...';

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_Consultatie_Update')
    DROP PROCEDURE sp_Consultatie_Update;
GO

CREATE PROCEDURE sp_Consultatie_Update
    @ConsultatieID UNIQUEIDENTIFIER,
    @ProgramareID UNIQUEIDENTIFIER = NULL,
    @PacientID UNIQUEIDENTIFIER = NULL,
    @MedicID UNIQUEIDENTIFIER = NULL,
    @DataConsultatie DATE = NULL,
    @OraConsultatie TIME = NULL,
    @TipConsultatie NVARCHAR(50) = NULL,
    
    -- Motive
    @MotivPrezentare NVARCHAR(MAX) = NULL,
    @IstoricBoalaActuala NVARCHAR(MAX) = NULL,
    
    -- Antecedente Heredo-Colaterale
    @AHC_Mama NVARCHAR(500) = NULL,
    @AHC_Tata NVARCHAR(500) = NULL,
    @AHC_Frati NVARCHAR(500) = NULL,
    @AHC_Bunici NVARCHAR(500) = NULL,
    @AHC_Altele NVARCHAR(500) = NULL,
    
    -- Antecedente Fiziologice
    @AF_Nastere NVARCHAR(200) = NULL,
    @AF_Dezvoltare NVARCHAR(200) = NULL,
    @AF_Menstruatie NVARCHAR(200) = NULL,
    @AF_Sarcini NVARCHAR(200) = NULL,
    @AF_Alaptare NVARCHAR(200) = NULL,
    
    -- Antecedente Personale Patologice
    @APP_BoliCopilarieAdolescenta NVARCHAR(500) = NULL,
    @APP_BoliAdult NVARCHAR(500) = NULL,
    @APP_Interventii NVARCHAR(500) = NULL,
    @APP_Traumatisme NVARCHAR(500) = NULL,
    @APP_Transfuzii NVARCHAR(500) = NULL,
    @APP_Alergii NVARCHAR(500) = NULL,
    @APP_Medicatie NVARCHAR(MAX) = NULL,
    
    -- Conditii Socio-Economice
    @Profesie NVARCHAR(200) = NULL,
    @ConditiiLocuinta NVARCHAR(300) = NULL,
    @ConditiiMunca NVARCHAR(300) = NULL,
    @ObiceiuriAlimentare NVARCHAR(300) = NULL,
    @Toxice NVARCHAR(300) = NULL,
    
    -- Examen General
    @StareGenerala NVARCHAR(50) = NULL,
    @Constitutie NVARCHAR(50) = NULL,
    @Atitudine NVARCHAR(50) = NULL,
    @Facies NVARCHAR(200) = NULL,
    @Tegumente NVARCHAR(200) = NULL,
    @Mucoase NVARCHAR(200) = NULL,
    @GangliniLimfatici NVARCHAR(200) = NULL,
    
    -- Semne Vitale
    @Greutate DECIMAL(5,2) = NULL,
    @Inaltime DECIMAL(5,2) = NULL,
    @IMC DECIMAL(5,2) = NULL,
    @Temperatura DECIMAL(4,2) = NULL,
    @TensiuneArteriala NVARCHAR(20) = NULL,
    @Puls INT = NULL,
    @FreccventaRespiratorie INT = NULL,
    @SaturatieO2 INT = NULL,
    @Glicemie DECIMAL(5,2) = NULL,
    
    -- Examen pe Aparate
    @ExamenCardiovascular NVARCHAR(MAX) = NULL,
    @ExamenRespiratoriu NVARCHAR(MAX) = NULL,
    @ExamenDigestiv NVARCHAR(MAX) = NULL,
    @ExamenUrinar NVARCHAR(MAX) = NULL,
    @ExamenNervos NVARCHAR(MAX) = NULL,
    @ExamenLocomotor NVARCHAR(MAX) = NULL,
    @ExamenEndocrin NVARCHAR(MAX) = NULL,
    @ExamenORL NVARCHAR(MAX) = NULL,
    @ExamenOftalmologic NVARCHAR(MAX) = NULL,
    @ExamenDermatologic NVARCHAR(MAX) = NULL,
    
    -- Investigatii
    @InvestigatiiLaborator NVARCHAR(MAX) = NULL,
    @InvestigatiiImagistice NVARCHAR(MAX) = NULL,
    @InvestigatiiEKG NVARCHAR(MAX) = NULL,
    @AlteInvestigatii NVARCHAR(MAX) = NULL,
    
    -- Diagnostic
    @DiagnosticPozitiv NVARCHAR(MAX) = NULL,
    @DiagnosticDiferential NVARCHAR(MAX) = NULL,
    @DiagnosticEtiologic NVARCHAR(MAX) = NULL,
    @CoduriICD10 NVARCHAR(200) = NULL,
    
    -- Tratament
    @TratamentMedicamentos NVARCHAR(MAX) = NULL,
    @TratamentNemedicamentos NVARCHAR(MAX) = NULL,
    @RecomandariDietetice NVARCHAR(MAX) = NULL,
    @RecomandariRegimViata NVARCHAR(MAX) = NULL,
    
    -- Recomandari
    @InvestigatiiRecomandate NVARCHAR(MAX) = NULL,
    @ConsulturiSpecialitate NVARCHAR(MAX) = NULL,
    @DataUrmatoareiProgramari NVARCHAR(100) = NULL,
    @RecomandariSupraveghere NVARCHAR(MAX) = NULL,
    
    -- Prognostic & Concluzie
    @Prognostic NVARCHAR(50) = NULL,
    @Concluzie NVARCHAR(MAX) = NULL,
    
    -- Observatii
    @ObservatiiMedic NVARCHAR(MAX) = NULL,
    @NotePacient NVARCHAR(MAX) = NULL,
    
    -- Status
    @Status NVARCHAR(50) = NULL,
    @DataFinalizare DATETIME = NULL,
    @DurataMinute INT = NULL,
    
    -- Audit
    @ModificatDe UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Verificare existenta consultatie
        IF NOT EXISTS (SELECT 1 FROM Consultatii WHERE ConsultatieID = @ConsultatieID)
        BEGIN
            RAISERROR('Consultatia cu ID-ul specificat nu exista!', 16, 1);
            RETURN;
        END
        
        -- UPDATE cu toate campurile (NULL values sunt pastrate din baza de date)
        UPDATE Consultatii
        SET 
            ProgramareID = ISNULL(@ProgramareID, ProgramareID),
            PacientID = ISNULL(@PacientID, PacientID),
            MedicID = ISNULL(@MedicID, MedicID),
            DataConsultatie = ISNULL(@DataConsultatie, DataConsultatie),
            OraConsultatie = ISNULL(@OraConsultatie, OraConsultatie),
            TipConsultatie = ISNULL(@TipConsultatie, TipConsultatie),
            
            -- Motive
            MotivPrezentare = ISNULL(@MotivPrezentare, MotivPrezentare),
            IstoricBoalaActuala = ISNULL(@IstoricBoalaActuala, IstoricBoalaActuala),
            
            -- Antecedente Heredo-Colaterale
            AHC_Mama = ISNULL(@AHC_Mama, AHC_Mama),
            AHC_Tata = ISNULL(@AHC_Tata, AHC_Tata),
            AHC_Frati = ISNULL(@AHC_Frati, AHC_Frati),
            AHC_Bunici = ISNULL(@AHC_Bunici, AHC_Bunici),
            AHC_Altele = ISNULL(@AHC_Altele, AHC_Altele),
            
            -- Antecedente Fiziologice
            AF_Nastere = ISNULL(@AF_Nastere, AF_Nastere),
            AF_Dezvoltare = ISNULL(@AF_Dezvoltare, AF_Dezvoltare),
            AF_Menstruatie = ISNULL(@AF_Menstruatie, AF_Menstruatie),
            AF_Sarcini = ISNULL(@AF_Sarcini, AF_Sarcini),
            AF_Alaptare = ISNULL(@AF_Alaptare, AF_Alaptare),
            
            -- Antecedente Personale Patologice
            APP_BoliCopilarieAdolescenta = ISNULL(@APP_BoliCopilarieAdolescenta, APP_BoliCopilarieAdolescenta),
            APP_BoliAdult = ISNULL(@APP_BoliAdult, APP_BoliAdult),
            APP_Interventii = ISNULL(@APP_Interventii, APP_Interventii),
            APP_Traumatisme = ISNULL(@APP_Traumatisme, APP_Traumatisme),
            APP_Transfuzii = ISNULL(@APP_Transfuzii, APP_Transfuzii),
            APP_Alergii = ISNULL(@APP_Alergii, APP_Alergii),
            APP_Medicatie = ISNULL(@APP_Medicatie, APP_Medicatie),
            
            -- Conditii Socio-Economice
            Profesie = ISNULL(@Profesie, Profesie),
            ConditiiLocuinta = ISNULL(@ConditiiLocuinta, ConditiiLocuinta),
            ConditiiMunca = ISNULL(@ConditiiMunca, ConditiiMunca),
            ObiceiuriAlimentare = ISNULL(@ObiceiuriAlimentare, ObiceiuriAlimentare),
            Toxice = ISNULL(@Toxice, Toxice),
            
            -- Examen General
            StareGenerala = ISNULL(@StareGenerala, StareGenerala),
            Constitutie = ISNULL(@Constitutie, Constitutie),
            Atitudine = ISNULL(@Atitudine, Atitudine),
            Facies = ISNULL(@Facies, Facies),
            Tegumente = ISNULL(@Tegumente, Tegumente),
            Mucoase = ISNULL(@Mucoase, Mucoase),
            GangliniLimfatici = ISNULL(@GangliniLimfatici, GangliniLimfatici),
            
            -- Semne Vitale
            Greutate = ISNULL(@Greutate, Greutate),
            Inaltime = ISNULL(@Inaltime, Inaltime),
            IMC = ISNULL(@IMC, IMC),
            Temperatura = ISNULL(@Temperatura, Temperatura),
            TensiuneArteriala = ISNULL(@TensiuneArteriala, TensiuneArteriala),
            Puls = ISNULL(@Puls, Puls),
            FreccventaRespiratorie = ISNULL(@FreccventaRespiratorie, FreccventaRespiratorie),
            SaturatieO2 = ISNULL(@SaturatieO2, SaturatieO2),
            Glicemie = ISNULL(@Glicemie, Glicemie),
            
            -- Examen pe Aparate
            ExamenCardiovascular = ISNULL(@ExamenCardiovascular, ExamenCardiovascular),
            ExamenRespiratoriu = ISNULL(@ExamenRespiratoriu, ExamenRespiratoriu),
            ExamenDigestiv = ISNULL(@ExamenDigestiv, ExamenDigestiv),
            ExamenUrinar = ISNULL(@ExamenUrinar, ExamenUrinar),
            ExamenNervos = ISNULL(@ExamenNervos, ExamenNervos),
            ExamenLocomotor = ISNULL(@ExamenLocomotor, ExamenLocomotor),
            ExamenEndocrin = ISNULL(@ExamenEndocrin, ExamenEndocrin),
            ExamenORL = ISNULL(@ExamenORL, ExamenORL),
            ExamenOftalmologic = ISNULL(@ExamenOftalmologic, ExamenOftalmologic),
            ExamenDermatologic = ISNULL(@ExamenDermatologic, ExamenDermatologic),
            
            -- Investigatii
            InvestigatiiLaborator = ISNULL(@InvestigatiiLaborator, InvestigatiiLaborator),
            InvestigatiiImagistice = ISNULL(@InvestigatiiImagistice, InvestigatiiImagistice),
            InvestigatiiEKG = ISNULL(@InvestigatiiEKG, InvestigatiiEKG),
            AlteInvestigatii = ISNULL(@AlteInvestigatii, AlteInvestigatii),
            
            -- Diagnostic
            DiagnosticPozitiv = ISNULL(@DiagnosticPozitiv, DiagnosticPozitiv),
            DiagnosticDiferential = ISNULL(@DiagnosticDiferential, DiagnosticDiferential),
            DiagnosticEtiologic = ISNULL(@DiagnosticEtiologic, DiagnosticEtiologic),
            CoduriICD10 = ISNULL(@CoduriICD10, CoduriICD10),
            
            -- Tratament
            TratamentMedicamentos = ISNULL(@TratamentMedicamentos, TratamentMedicamentos),
            TratamentNemedicamentos = ISNULL(@TratamentNemedicamentos, TratamentNemedicamentos),
            RecomandariDietetice = ISNULL(@RecomandariDietetice, RecomandariDietetice),
            RecomandariRegimViata = ISNULL(@RecomandariRegimViata, RecomandariRegimViata),
            
            -- Recomandari
            InvestigatiiRecomandate = ISNULL(@InvestigatiiRecomandate, InvestigatiiRecomandate),
            ConsulturiSpecialitate = ISNULL(@ConsulturiSpecialitate, ConsulturiSpecialitate),
            DataUrmatoareiProgramari = ISNULL(@DataUrmatoareiProgramari, DataUrmatoareiProgramari),
            RecomandariSupraveghere = ISNULL(@RecomandariSupraveghere, RecomandariSupraveghere),
            
            -- Prognostic & Concluzie
            Prognostic = ISNULL(@Prognostic, Prognostic),
            Concluzie = ISNULL(@Concluzie, Concluzie),
            
            -- Observatii
            ObservatiiMedic = ISNULL(@ObservatiiMedic, ObservatiiMedic),
            NotePacient = ISNULL(@NotePacient, NotePacient),
            
            -- Status
            [Status] = ISNULL(@Status, [Status]),
            DataFinalizare = ISNULL(@DataFinalizare, DataFinalizare),
            DurataMinute = ISNULL(@DurataMinute, DurataMinute),
            
            -- Audit
            DataUltimeiModificari = GETDATE(),
            ModificatDe = @ModificatDe
            
        WHERE ConsultatieID = @ConsultatieID;
        
        -- Return success indicator
        SELECT @@ROWCOUNT AS RowsAffected;
        
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

PRINT '   ? sp_Consultatie_Update creat cu succes';
PRINT '';

-- =============================================
-- 2. sp_Consultatie_SaveDraft
-- Salveaza consultatie ca draft (auto-save) fara finalizare
-- =============================================
PRINT '2. Creare sp_Consultatie_SaveDraft...';

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_Consultatie_SaveDraft')
    DROP PROCEDURE sp_Consultatie_SaveDraft;
GO

CREATE PROCEDURE sp_Consultatie_SaveDraft
    @ConsultatieID UNIQUEIDENTIFIER OUTPUT,
    @ProgramareID UNIQUEIDENTIFIER,
    @PacientID UNIQUEIDENTIFIER,
    @MedicID UNIQUEIDENTIFIER,
    @DataConsultatie DATE = NULL,
    @OraConsultatie TIME = NULL,
    @TipConsultatie NVARCHAR(50) = 'Prima consultatie',
    
    -- Motive
    @MotivPrezentare NVARCHAR(MAX) = NULL,
    @IstoricBoalaActuala NVARCHAR(MAX) = NULL,
    
    -- Semne Vitale (cele mai frecvent completate)
    @Greutate DECIMAL(5,2) = NULL,
    @Inaltime DECIMAL(5,2) = NULL,
    @IMC DECIMAL(5,2) = NULL,
    @Temperatura DECIMAL(4,2) = NULL,
    @TensiuneArteriala NVARCHAR(20) = NULL,
    @Puls INT = NULL,
    
    -- Diagnostic Primar
    @DiagnosticPozitiv NVARCHAR(MAX) = NULL,
    @CoduriICD10 NVARCHAR(200) = NULL,
    
    -- Tratament Primar
    @TratamentMedicamentos NVARCHAR(MAX) = NULL,
    
    -- Observatii
    @ObservatiiMedic NVARCHAR(MAX) = NULL,
    
    -- Audit
    @CreatDeSauModificatDe UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Verificare daca consultatia exista deja
        IF EXISTS (SELECT 1 FROM Consultatii WHERE ConsultatieID = @ConsultatieID)
        BEGIN
            -- UPDATE draft existent
            UPDATE Consultatii
            SET 
                MotivPrezentare = ISNULL(@MotivPrezentare, MotivPrezentare),
                IstoricBoalaActuala = ISNULL(@IstoricBoalaActuala, IstoricBoalaActuala),
                Greutate = ISNULL(@Greutate, Greutate),
                Inaltime = ISNULL(@Inaltime, Inaltime),
                IMC = ISNULL(@IMC, IMC),
                Temperatura = ISNULL(@Temperatura, Temperatura),
                TensiuneArteriala = ISNULL(@TensiuneArteriala, TensiuneArteriala),
                Puls = ISNULL(@Puls, Puls),
                DiagnosticPozitiv = ISNULL(@DiagnosticPozitiv, DiagnosticPozitiv),
                CoduriICD10 = ISNULL(@CoduriICD10, CoduriICD10),
                TratamentMedicamentos = ISNULL(@TratamentMedicamentos, TratamentMedicamentos),
                ObservatiiMedic = ISNULL(@ObservatiiMedic, ObservatiiMedic),
                DataUltimeiModificari = GETDATE(),
                ModificatDe = @CreatDeSauModificatDe,
                [Status] = 'In desfasurare'
            WHERE ConsultatieID = @ConsultatieID;
        END
        ELSE
        BEGIN
            -- INSERT draft nou
            IF @ConsultatieID IS NULL OR @ConsultatieID = '00000000-0000-0000-0000-000000000000'
                SET @ConsultatieID = NEWID();
            
            IF @DataConsultatie IS NULL
                SET @DataConsultatie = CAST(GETDATE() AS DATE);
            
            IF @OraConsultatie IS NULL
                SET @OraConsultatie = CAST(GETDATE() AS TIME);
            
            INSERT INTO Consultatii (
                ConsultatieID, ProgramareID, PacientID, MedicID,
                DataConsultatie, OraConsultatie, TipConsultatie,
                MotivPrezentare, IstoricBoalaActuala,
                Greutate, Inaltime, IMC, Temperatura, TensiuneArteriala, Puls,
                DiagnosticPozitiv, CoduriICD10,
                TratamentMedicamentos,
                ObservatiiMedic,
                [Status], DataCreare, CreatDe
            )
            VALUES (
                @ConsultatieID, @ProgramareID, @PacientID, @MedicID,
                @DataConsultatie, @OraConsultatie, @TipConsultatie,
                @MotivPrezentare, @IstoricBoalaActuala,
                @Greutate, @Inaltime, @IMC, @Temperatura, @TensiuneArteriala, @Puls,
                @DiagnosticPozitiv, @CoduriICD10,
                @TratamentMedicamentos,
                @ObservatiiMedic,
                'In desfasurare', GETDATE(), @CreatDeSauModificatDe
            );
        END
        
        -- Return ConsultatieID
        SELECT @ConsultatieID AS ConsultatieID, @@ROWCOUNT AS RowsAffected;
        
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

PRINT '   ? sp_Consultatie_SaveDraft creat cu succes';
PRINT '';

-- =============================================
-- 3. sp_Consultatie_Finalize
-- Finalizeaza consultatie si schimba status programare
-- =============================================
PRINT '3. Creare sp_Consultatie_Finalize...';

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_Consultatie_Finalize')
    DROP PROCEDURE sp_Consultatie_Finalize;
GO

CREATE PROCEDURE sp_Consultatie_Finalize
    @ConsultatieID UNIQUEIDENTIFIER,
    @DurataMinute INT = 0,
    @ModificatDe UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificare existenta consultatie
        IF NOT EXISTS (SELECT 1 FROM Consultatii WHERE ConsultatieID = @ConsultatieID)
        BEGIN
            RAISERROR('Consultatia cu ID-ul specificat nu exista!', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Verificare daca consultatie este deja finalizata
        DECLARE @StatusActual NVARCHAR(50);
        SELECT @StatusActual = [Status] FROM Consultatii WHERE ConsultatieID = @ConsultatieID;
        
        IF @StatusActual = 'Finalizata'
        BEGIN
            RAISERROR('Consultatia este deja finalizata!', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validare campuri obligatorii pentru finalizare
        DECLARE @ProgramareID UNIQUEIDENTIFIER;
        DECLARE @MotivPrezentare NVARCHAR(MAX);
        DECLARE @DiagnosticPozitiv NVARCHAR(MAX);
        
        SELECT 
            @ProgramareID = ProgramareID,
            @MotivPrezentare = MotivPrezentare,
            @DiagnosticPozitiv = DiagnosticPozitiv
        FROM Consultatii 
        WHERE ConsultatieID = @ConsultatieID;
        
        -- Verificare campuri obligatorii
        IF @MotivPrezentare IS NULL OR LEN(LTRIM(RTRIM(@MotivPrezentare))) = 0
        BEGIN
            RAISERROR('Motivul prezentarii este obligatoriu pentru finalizare!', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        IF @DiagnosticPozitiv IS NULL OR LEN(LTRIM(RTRIM(@DiagnosticPozitiv))) = 0
        BEGIN
            RAISERROR('Diagnosticul este obligatoriu pentru finalizare!', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Finalizare consultatie
        UPDATE Consultatii
        SET 
            [Status] = 'Finalizata',
            DataFinalizare = GETDATE(),
            DurataMinute = @DurataMinute,
            DataUltimeiModificari = GETDATE(),
            ModificatDe = @ModificatDe
        WHERE ConsultatieID = @ConsultatieID;
        
        -- Update status programare
        UPDATE Programari
        SET [Status] = 'Finalizata'
        WHERE ProgramareID = @ProgramareID;
        
        COMMIT TRANSACTION;
        
        -- Return success
        SELECT 
            @ConsultatieID AS ConsultatieID, 
            'Finalizata' AS Status,
            GETDATE() AS DataFinalizare,
            1 AS Success;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT '   ? sp_Consultatie_Finalize creat cu succes';
PRINT '';

-- =============================================
-- 4. VERIFICARE FINALA
-- =============================================
PRINT '========================================';
PRINT 'VERIFICARE STORED PROCEDURES';
PRINT '========================================';
PRINT '';

SELECT 
    p.name AS ProcedureName,
    p.create_date AS DateCreated,
    p.modify_date AS DateModified
FROM sys.procedures p
WHERE p.schema_id = SCHEMA_ID('dbo')
AND p.name LIKE 'sp_Consultatie%'
ORDER BY p.name;

PRINT '';
PRINT '========================================';
PRINT '? TOATE STORED PROCEDURES CREATE!';
PRINT '? Total: 8 procedures';
PRINT '  - sp_Consultatie_Create (INSERT)';
PRINT '  - sp_Consultatie_Update (UPDATE toate campurile)';
PRINT '  - sp_Consultatie_SaveDraft (Auto-save draft)';
PRINT '  - sp_Consultatie_Finalize (Finalizare + validari)';
PRINT '  - sp_Consultatie_GetById (SELECT by ID)';
PRINT '  - sp_Consultatie_GetByPacient (SELECT lista)';
PRINT '  - sp_Consultatie_GetByMedic (SELECT lista)';
PRINT '  - sp_Consultatie_GetByProgramare (SELECT by FK)';
PRINT '========================================';
GO
