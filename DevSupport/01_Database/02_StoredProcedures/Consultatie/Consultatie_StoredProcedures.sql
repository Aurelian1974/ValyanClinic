-- =============================================
-- Stored Procedures pentru Consultatii Medicale
-- Versiune: 1.3
-- Data: 2025-01-08
-- Database: ValyanMed
-- FIXED: 
--   - PacientID → Pacienti.Id
--   - MedicID → PersonalMedical.PersonalID (NU PersonalMedicalID)
-- =============================================

USE ValyanMed;
GO

PRINT '========================================';
PRINT 'Initiere recreare tabel Consultatii';
PRINT 'Database: ValyanMed';
PRINT '========================================';
PRINT '';

-- =============================================
-- 0. STERGERE FOREIGN KEYS (daca exista)
-- =============================================
PRINT '1. Verificare si stergere Foreign Keys...';

-- Sterge toate FK-urile care refera Consultatii
DECLARE @sql NVARCHAR(MAX) = '';

SELECT @sql = @sql + 'ALTER TABLE [' + OBJECT_SCHEMA_NAME(parent_object_id) + '].[' + OBJECT_NAME(parent_object_id) + 
              '] DROP CONSTRAINT [' + name + '];' + CHAR(13)
FROM sys.foreign_keys
WHERE referenced_object_id = OBJECT_ID('dbo.Consultatii')
   OR parent_object_id = OBJECT_ID('dbo.Consultatii');

IF @sql <> ''
BEGIN
    PRINT '   Stergere Foreign Keys existente...';
    EXEC sp_executesql @sql;
    PRINT '   ✓ Foreign Keys sterse cu succes';
END
ELSE
BEGIN
    PRINT '   ✓ Nu exista Foreign Keys de sters';
END
GO

-- =============================================
-- 1. DROP TABLE Consultatii (dacă există)
-- =============================================
PRINT '';
PRINT '2. Stergere tabel Consultatii (daca exista)...';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Consultatii' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    DROP TABLE dbo.Consultatii;
    PRINT '   ✓ Tabel Consultatii sters cu succes';
END
ELSE
BEGIN
    PRINT '   ✓ Tabelul nu exista (OK pentru prima rulare)';
END
GO

-- =============================================
-- 2. CREATE TABLE Consultatii (Scrisoare Medicala Completa)
-- =============================================
PRINT '';
PRINT '3. Creare tabel Consultatii cu structura noua...';

CREATE TABLE dbo.Consultatii (
    -- Primary Key
    ConsultatieID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    
    -- Foreign Keys
    ProgramareID UNIQUEIDENTIFIER NOT NULL,
    PacientID UNIQUEIDENTIFIER NOT NULL,    -- REFERENTIAZA Pacienti.Id
    MedicID UNIQUEIDENTIFIER NOT NULL,       -- REFERENTIAZA PersonalMedical.PersonalID
    
    -- Date Consultatie
    DataConsultatie DATE NOT NULL DEFAULT GETDATE(),
    OraConsultatie TIME NOT NULL DEFAULT CONVERT(TIME, GETDATE()),
    TipConsultatie NVARCHAR(50) NOT NULL DEFAULT 'Prima consultatie',
    
    -- I. Motive Prezentare
    MotivPrezentare NVARCHAR(MAX) NULL,
    IstoricBoalaActuala NVARCHAR(MAX) NULL,
    
    -- II.A. Antecedente Heredo-Colaterale
    AHC_Mama NVARCHAR(500) NULL,
    AHC_Tata NVARCHAR(500) NULL,
    AHC_Frati NVARCHAR(500) NULL,
    AHC_Bunici NVARCHAR(500) NULL,
    AHC_Altele NVARCHAR(500) NULL,
    
    -- II.B. Antecedente Fiziologice
    AF_Nastere NVARCHAR(200) NULL,
    AF_Dezvoltare NVARCHAR(200) NULL,
    AF_Menstruatie NVARCHAR(200) NULL,
    AF_Sarcini NVARCHAR(200) NULL,
    AF_Alaptare NVARCHAR(200) NULL,
    
    -- II.C. Antecedente Personale Patologice
    APP_BoliCopilarieAdolescenta NVARCHAR(500) NULL,
    APP_BoliAdult NVARCHAR(500) NULL,
    APP_Interventii NVARCHAR(500) NULL,
    APP_Traumatisme NVARCHAR(500) NULL,
    APP_Transfuzii NVARCHAR(500) NULL,
    APP_Alergii NVARCHAR(500) NULL,
    APP_Medicatie NVARCHAR(MAX) NULL,
    
    -- II.D. Conditii Socio-Economice
    Profesie NVARCHAR(200) NULL,
    ConditiiLocuinta NVARCHAR(300) NULL,
    ConditiiMunca NVARCHAR(300) NULL,
    ObiceiuriAlimentare NVARCHAR(300) NULL,
    Toxice NVARCHAR(300) NULL,
    
    -- III.A. Examen General
    StareGenerala NVARCHAR(50) NULL,
    Constitutie NVARCHAR(50) NULL,
    Atitudine NVARCHAR(50) NULL,
    Facies NVARCHAR(200) NULL,
    Tegumente NVARCHAR(200) NULL,
    Mucoase NVARCHAR(200) NULL,
    GangliniLimfatici NVARCHAR(200) NULL,
    
    -- III.B. Semne Vitale
    Greutate DECIMAL(5,2) NULL,
    Inaltime DECIMAL(5,2) NULL,
    IMC DECIMAL(5,2) NULL,
    Temperatura DECIMAL(4,2) NULL,
    TensiuneArteriala NVARCHAR(20) NULL,
    Puls INT NULL,
    FreccventaRespiratorie INT NULL,
    SaturatieO2 INT NULL,
    Glicemie DECIMAL(5,2) NULL,
    
    -- III.C. Examen pe Aparate
    ExamenCardiovascular NVARCHAR(MAX) NULL,
    ExamenRespiratoriu NVARCHAR(MAX) NULL,
    ExamenDigestiv NVARCHAR(MAX) NULL,
    ExamenUrinar NVARCHAR(MAX) NULL,
    ExamenNervos NVARCHAR(MAX) NULL,
    ExamenLocomotor NVARCHAR(MAX) NULL,
    ExamenEndocrin NVARCHAR(MAX) NULL,
    ExamenORL NVARCHAR(MAX) NULL,
    ExamenOftalmologic NVARCHAR(MAX) NULL,
    ExamenDermatologic NVARCHAR(MAX) NULL,
    
    -- IV. Investigatii
    InvestigatiiLaborator NVARCHAR(MAX) NULL,
    InvestigatiiImagistice NVARCHAR(MAX) NULL,
    InvestigatiiEKG NVARCHAR(MAX) NULL,
    AlteInvestigatii NVARCHAR(MAX) NULL,
    
    -- V. Diagnostic
    DiagnosticPozitiv NVARCHAR(MAX) NULL,
    DiagnosticDiferential NVARCHAR(MAX) NULL,
    DiagnosticEtiologic NVARCHAR(MAX) NULL,
    CoduriICD10 NVARCHAR(200) NULL,
    
    -- VI. Tratament
    TratamentMedicamentos NVARCHAR(MAX) NULL,
    TratamentNemedicamentos NVARCHAR(MAX) NULL,
    RecomandariDietetice NVARCHAR(MAX) NULL,
    RecomandariRegimViata NVARCHAR(MAX) NULL,
    
    -- VII. Recomandari
    InvestigatiiRecomandate NVARCHAR(MAX) NULL,
    ConsulturiSpecialitate NVARCHAR(MAX) NULL,
    DataUrmatoareiProgramari NVARCHAR(100) NULL,
    RecomandariSupraveghere NVARCHAR(MAX) NULL,
    
    -- VIII. Prognostic
    Prognostic NVARCHAR(50) NULL,
    
    -- IX. Concluzie
    Concluzie NVARCHAR(MAX) NULL,
    
    -- X. Observatii
    ObservatiiMedic NVARCHAR(MAX) NULL,
    NotePacient NVARCHAR(MAX) NULL,
    
    -- Status & Workflow
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'In desfasurare',
    DataFinalizare DATETIME NULL,
    DurataMinute INT NOT NULL DEFAULT 0,
    
    -- Documente
    DocumenteAtatate NVARCHAR(MAX) NULL,
    
    -- Audit
    DataCreare DATETIME NOT NULL DEFAULT GETDATE(),
    CreatDe UNIQUEIDENTIFIER NOT NULL,
    DataUltimeiModificari DATETIME NULL,
    ModificatDe UNIQUEIDENTIFIER NULL,
    
    -- Indexes
    INDEX IX_Consultatii_PacientID (PacientID),
    INDEX IX_Consultatii_MedicID (MedicID),
    INDEX IX_Consultatii_ProgramareID (ProgramareID),
    INDEX IX_Consultatii_DataConsultatie (DataConsultatie DESC)
);

PRINT '   ✓ Tabel Consultatii creat cu succes';
PRINT '   ✓ Coloane: 80+ (Scrisoare Medicala Completa)';
PRINT '   ✓ Primary Key: ConsultatieID (NEWSEQUENTIALID)';
PRINT '   ✓ Indexes: 4 (PacientID, MedicID, ProgramareID, DataConsultatie)';
GO

-- =============================================
-- 3. ADAUGARE FOREIGN KEYS
-- =============================================
PRINT '';
PRINT '4. Adaugare Foreign Keys...';

-- FK 1: Programari
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Consultatii_Programari')
BEGIN
    ALTER TABLE dbo.Consultatii
    ADD CONSTRAINT FK_Consultatii_Programari 
    FOREIGN KEY (ProgramareID) REFERENCES dbo.Programari(ProgramareID);
    PRINT '   ✓ FK_Consultatii_Programari adaugat';
END

-- FK 2: Pacienti (FIXED: Pacienti.Id nu PacientID)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Consultatii_Pacienti')
BEGIN
    ALTER TABLE dbo.Consultatii
    ADD CONSTRAINT FK_Consultatii_Pacienti 
    FOREIGN KEY (PacientID) REFERENCES dbo.Pacienti(Id);  -- FIXED: Pacienti.Id
    PRINT '   ✓ FK_Consultatii_Pacienti adaugat (→ Pacienti.Id)';
END

-- FK 3: PersonalMedical (FIXED: PersonalMedical.PersonalID nu PersonalMedicalID)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Consultatii_PersonalMedical')
BEGIN
    ALTER TABLE dbo.Consultatii
    ADD CONSTRAINT FK_Consultatii_PersonalMedical 
    FOREIGN KEY (MedicID) REFERENCES dbo.PersonalMedical(PersonalID);  -- FIXED: PersonalID
    PRINT '   ✓ FK_Consultatii_PersonalMedical adaugat (→ PersonalMedical.PersonalID)';
END
GO

-- =============================================
-- 5. sp_Consultatie_Create
-- =============================================
PRINT '';
PRINT '5. Creare Stored Procedures...';

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_Consultatie_Create')
    DROP PROCEDURE sp_Consultatie_Create;
GO

CREATE PROCEDURE sp_Consultatie_Create
    @ConsultatieID UNIQUEIDENTIFIER OUTPUT,
    @ProgramareID UNIQUEIDENTIFIER,
    @PacientID UNIQUEIDENTIFIER,
    @MedicID UNIQUEIDENTIFIER,
    @DataConsultatie DATE,
    @OraConsultatie TIME,
    @TipConsultatie NVARCHAR(50),
    
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
    @Status NVARCHAR(50) = 'Finalizata',
    @DataFinalizare DATETIME = NULL,
    
    -- Audit
    @CreatDe UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Generate new ID if not provided (NEWID pentru SP, NEWSEQUENTIALID nu functioneaza in SP)
        IF @ConsultatieID IS NULL OR @ConsultatieID = '00000000-0000-0000-0000-000000000000'
            SET @ConsultatieID = NEWID();
        
        INSERT INTO Consultatii (
            ConsultatieID, ProgramareID, PacientID, MedicID,
            DataConsultatie, OraConsultatie, TipConsultatie,
            MotivPrezentare, IstoricBoalaActuala,
            AHC_Mama, AHC_Tata, AHC_Frati, AHC_Bunici, AHC_Altele,
            AF_Nastere, AF_Dezvoltare, AF_Menstruatie, AF_Sarcini, AF_Alaptare,
            APP_BoliCopilarieAdolescenta, APP_BoliAdult, APP_Interventii, 
            APP_Traumatisme, APP_Transfuzii, APP_Alergii, APP_Medicatie,
            Profesie, ConditiiLocuinta, ConditiiMunca, ObiceiuriAlimentare, Toxice,
            StareGenerala, Constitutie, Atitudine, Facies, Tegumente, Mucoase, GangliniLimfatici,
            Greutate, Inaltime, IMC, Temperatura, TensiuneArteriala, 
            Puls, FreccventaRespiratorie, SaturatieO2, Glicemie,
            ExamenCardiovascular, ExamenRespiratoriu, ExamenDigestiv, ExamenUrinar,
            ExamenNervos, ExamenLocomotor, ExamenEndocrin, ExamenORL, 
            ExamenOftalmologic, ExamenDermatologic,
            InvestigatiiLaborator, InvestigatiiImagistice, InvestigatiiEKG, AlteInvestigatii,
            DiagnosticPozitiv, DiagnosticDiferential, DiagnosticEtiologic, CoduriICD10,
            TratamentMedicamentos, TratamentNemedicamentos, RecomandariDietetice, RecomandariRegimViata,
            InvestigatiiRecomandate, ConsulturiSpecialitate, DataUrmatoareiProgramari, RecomandariSupraveghere,
            Prognostic, Concluzie, ObservatiiMedic, NotePacient,
            [Status], DataFinalizare, DataCreare, CreatDe
        )
        VALUES (
            @ConsultatieID, @ProgramareID, @PacientID, @MedicID,
            @DataConsultatie, @OraConsultatie, @TipConsultatie,
            @MotivPrezentare, @IstoricBoalaActuala,
            @AHC_Mama, @AHC_Tata, @AHC_Frati, @AHC_Bunici, @AHC_Altele,
            @AF_Nastere, @AF_Dezvoltare, @AF_Menstruatie, @AF_Sarcini, @AF_Alaptare,
            @APP_BoliCopilarieAdolescenta, @APP_BoliAdult, @APP_Interventii,
            @APP_Traumatisme, @APP_Transfuzii, @APP_Alergii, @APP_Medicatie,
            @Profesie, @ConditiiLocuinta, @ConditiiMunca, @ObiceiuriAlimentare, @Toxice,
            @StareGenerala, @Constitutie, @Atitudine, @Facies, @Tegumente, @Mucoase, @GangliniLimfatici,
            @Greutate, @Inaltime, @IMC, @Temperatura, @TensiuneArteriala,
            @Puls, @FreccventaRespiratorie, @SaturatieO2, @Glicemie,
            @ExamenCardiovascular, @ExamenRespiratoriu, @ExamenDigestiv, @ExamenUrinar,
            @ExamenNervos, @ExamenLocomotor, @ExamenEndocrin, @ExamenORL,
            @ExamenOftalmologic, @ExamenDermatologic,
            @InvestigatiiLaborator, @InvestigatiiImagistice, @InvestigatiiEKG, @AlteInvestigatii,
            @DiagnosticPozitiv, @DiagnosticDiferential, @DiagnosticEtiologic, @CoduriICD10,
            @TratamentMedicamentos, @TratamentNemedicamentos, @RecomandariDietetice, @RecomandariRegimViata,
            @InvestigatiiRecomandate, @ConsulturiSpecialitate, @DataUrmatoareiProgramari, @RecomandariSupraveghere,
            @Prognostic, @Concluzie, @ObservatiiMedic, @NotePacient,
            @Status, @DataFinalizare, GETDATE(), @CreatDe
        );
        
        -- Update status programare
        UPDATE Programari
        SET [Status] = 'Finalizata'
        WHERE ProgramareID = @ProgramareID;
        
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

PRINT '   ✓ sp_Consultatie_Create creat';

-- =============================================
-- 6. sp_Consultatie_GetById
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_Consultatie_GetById')
    DROP PROCEDURE sp_Consultatie_GetById;
GO

CREATE PROCEDURE sp_Consultatie_GetById
    @ConsultatieID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT *
    FROM Consultatii
    WHERE ConsultatieID = @ConsultatieID;
END
GO

PRINT '   ✓ sp_Consultatie_GetById creat';

-- =============================================
-- 7. sp_Consultatie_GetByPacient
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_Consultatie_GetByPacient')
    DROP PROCEDURE sp_Consultatie_GetByPacient;
GO

CREATE PROCEDURE sp_Consultatie_GetByPacient
    @PacientID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT *
    FROM Consultatii
    WHERE PacientID = @PacientID
    ORDER BY DataConsultatie DESC, OraConsultatie DESC;
END
GO

PRINT '   ✓ sp_Consultatie_GetByPacient creat';

-- =============================================
-- 8. sp_Consultatie_GetByMedic
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_Consultatie_GetByMedic')
    DROP PROCEDURE sp_Consultatie_GetByMedic;
GO

CREATE PROCEDURE sp_Consultatie_GetByMedic
    @MedicID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT *
    FROM Consultatii
    WHERE MedicID = @MedicID
    ORDER BY DataConsultatie DESC, OraConsultatie DESC;
END
GO

PRINT '   ✓ sp_Consultatie_GetByMedic creat';

-- =============================================
-- 9. sp_Consultatie_GetByProgramare
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_Consultatie_GetByProgramare')
    DROP PROCEDURE sp_Consultatie_GetByProgramare;
GO

CREATE PROCEDURE sp_Consultatie_GetByProgramare
    @ProgramareID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT *
    FROM Consultatii
    WHERE ProgramareID = @ProgramareID;
END
GO

PRINT '   ✓ sp_Consultatie_GetByProgramare creat';

PRINT '';
PRINT '========================================';
PRINT '✓ Tabel Consultatii recreat cu succes!';
PRINT '✓ Foreign Keys TOATE CORECTE:';
PRINT '  - FK_Consultatii_Programari → Programari(ProgramareID)';
PRINT '  - FK_Consultatii_Pacienti → Pacienti(Id)';
PRINT '  - FK_Consultatii_PersonalMedical → PersonalMedical(PersonalID)';
PRINT '✓ Stored Procedures create (5 buc)';
PRINT '========================================';
GO
