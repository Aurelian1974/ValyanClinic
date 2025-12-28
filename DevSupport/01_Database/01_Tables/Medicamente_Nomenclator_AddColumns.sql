-- =============================================
-- Script: Adăugare coloane noi în Medicamente_Nomenclator
-- Data: 2025-12-27
-- Descriere: Adaugă toate coloanele din nomenclatorul ANM
-- =============================================

USE [ValyanMed]
GO

-- Adaugă coloanele noi dacă nu există
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Medicamente_Nomenclator') AND name = 'FirmaDetinatoare')
BEGIN
    ALTER TABLE [dbo].[Medicamente_Nomenclator] ADD [FirmaDetinatoare] NVARCHAR(500) NULL;
    PRINT 'Coloana FirmaDetinatoare adăugată.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Medicamente_Nomenclator') AND name = 'TaraDetinatoare')
BEGIN
    ALTER TABLE [dbo].[Medicamente_Nomenclator] ADD [TaraDetinatoare] NVARCHAR(100) NULL;
    PRINT 'Coloana TaraDetinatoare adăugată.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Medicamente_Nomenclator') AND name = 'ActiuneTerapeutica')
BEGIN
    ALTER TABLE [dbo].[Medicamente_Nomenclator] ADD [ActiuneTerapeutica] NVARCHAR(500) NULL;
    PRINT 'Coloana ActiuneTerapeutica adăugată.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Medicamente_Nomenclator') AND name = 'NrDataAmbalajAPP')
BEGIN
    ALTER TABLE [dbo].[Medicamente_Nomenclator] ADD [NrDataAmbalajAPP] NVARCHAR(200) NULL;
    PRINT 'Coloana NrDataAmbalajAPP adăugată.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Medicamente_Nomenclator') AND name = 'Ambalaj')
BEGIN
    ALTER TABLE [dbo].[Medicamente_Nomenclator] ADD [Ambalaj] NVARCHAR(500) NULL;
    PRINT 'Coloana Ambalaj adăugată.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Medicamente_Nomenclator') AND name = 'VolumAmbalaj')
BEGIN
    ALTER TABLE [dbo].[Medicamente_Nomenclator] ADD [VolumAmbalaj] NVARCHAR(100) NULL;
    PRINT 'Coloana VolumAmbalaj adăugată.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Medicamente_Nomenclator') AND name = 'ValabilitateAmbalaj')
BEGIN
    ALTER TABLE [dbo].[Medicamente_Nomenclator] ADD [ValabilitateAmbalaj] NVARCHAR(100) NULL;
    PRINT 'Coloana ValabilitateAmbalaj adăugată.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Medicamente_Nomenclator') AND name = 'Bulina')
BEGIN
    ALTER TABLE [dbo].[Medicamente_Nomenclator] ADD [Bulina] BIT NULL DEFAULT 0;
    PRINT 'Coloana Bulina adăugată.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Medicamente_Nomenclator') AND name = 'Diez')
BEGIN
    ALTER TABLE [dbo].[Medicamente_Nomenclator] ADD [Diez] BIT NULL DEFAULT 0;
    PRINT 'Coloana Diez adăugată.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Medicamente_Nomenclator') AND name = 'Stea')
BEGIN
    ALTER TABLE [dbo].[Medicamente_Nomenclator] ADD [Stea] BIT NULL DEFAULT 0;
    PRINT 'Coloana Stea adăugată.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Medicamente_Nomenclator') AND name = 'Triunghi')
BEGIN
    ALTER TABLE [dbo].[Medicamente_Nomenclator] ADD [Triunghi] BIT NULL DEFAULT 0;
    PRINT 'Coloana Triunghi adăugată.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Medicamente_Nomenclator') AND name = 'Dreptunghi')
BEGIN
    ALTER TABLE [dbo].[Medicamente_Nomenclator] ADD [Dreptunghi] BIT NULL DEFAULT 0;
    PRINT 'Coloana Dreptunghi adăugată.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Medicamente_Nomenclator') AND name = 'DataActualizareANM')
BEGIN
    ALTER TABLE [dbo].[Medicamente_Nomenclator] ADD [DataActualizareANM] DATE NULL;
    PRINT 'Coloana DataActualizareANM adăugată.';
END

PRINT 'Toate coloanele au fost adăugate.';
GO

-- Actualizează stored procedure Medicamente_Upsert
CREATE OR ALTER PROCEDURE [dbo].[Medicamente_Upsert]
    @CodANM NVARCHAR(20),
    @NumeComercial NVARCHAR(500),
    @DCI NVARCHAR(500) = NULL,
    @FormaFarmaceutica NVARCHAR(200) = NULL,
    @Concentratie NVARCHAR(200) = NULL,
    @Producator NVARCHAR(500) = NULL,
    @TaraProducator NVARCHAR(100) = NULL,
    @FirmaDetinatoare NVARCHAR(500) = NULL,
    @TaraDetinatoare NVARCHAR(100) = NULL,
    @CodATC NVARCHAR(20) = NULL,
    @ActiuneTerapeutica NVARCHAR(500) = NULL,
    @PrescriptieMedicala BIT = 1,
    @NrDataAmbalajAPP NVARCHAR(200) = NULL,
    @Ambalaj NVARCHAR(500) = NULL,
    @VolumAmbalaj NVARCHAR(100) = NULL,
    @ValabilitateAmbalaj NVARCHAR(100) = NULL,
    @Bulina BIT = 0,
    @Diez BIT = 0,
    @Stea BIT = 0,
    @Triunghi BIT = 0,
    @Dreptunghi BIT = 0,
    @DataActualizareANM DATE = NULL,
    @SursaFisier NVARCHAR(500) = NULL,
    @IsNew BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM [dbo].[Medicamente_Nomenclator] WHERE [CodANM] = @CodANM)
    BEGIN
        -- Update
        UPDATE [dbo].[Medicamente_Nomenclator]
        SET [NumeComercial] = @NumeComercial,
            [DCI] = @DCI,
            [FormaFarmaceutica] = @FormaFarmaceutica,
            [Concentratie] = @Concentratie,
            [Producator] = @Producator,
            [TaraProducator] = @TaraProducator,
            [FirmaDetinatoare] = @FirmaDetinatoare,
            [TaraDetinatoare] = @TaraDetinatoare,
            [CodATC] = @CodATC,
            [ActiuneTerapeutica] = @ActiuneTerapeutica,
            [PrescriptieMedicala] = @PrescriptieMedicala,
            [NrDataAmbalajAPP] = @NrDataAmbalajAPP,
            [Ambalaj] = @Ambalaj,
            [VolumAmbalaj] = @VolumAmbalaj,
            [ValabilitateAmbalaj] = @ValabilitateAmbalaj,
            [Bulina] = @Bulina,
            [Diez] = @Diez,
            [Stea] = @Stea,
            [Triunghi] = @Triunghi,
            [Dreptunghi] = @Dreptunghi,
            [DataActualizareANM] = @DataActualizareANM,
            [DataUltimaActualizare] = GETDATE(),
            [SursaFisier] = @SursaFisier,
            [Activ] = 1
        WHERE [CodANM] = @CodANM;
        
        SET @IsNew = 0;
    END
    ELSE
    BEGIN
        -- Insert
        INSERT INTO [dbo].[Medicamente_Nomenclator] (
            [CodANM], [NumeComercial], [DCI], [FormaFarmaceutica], [Concentratie],
            [Producator], [TaraProducator], [FirmaDetinatoare], [TaraDetinatoare],
            [CodATC], [ActiuneTerapeutica], [PrescriptieMedicala],
            [NrDataAmbalajAPP], [Ambalaj], [VolumAmbalaj], [ValabilitateAmbalaj],
            [Bulina], [Diez], [Stea], [Triunghi], [Dreptunghi],
            [DataActualizareANM], [SursaFisier], [Activ]
        )
        VALUES (
            @CodANM, @NumeComercial, @DCI, @FormaFarmaceutica, @Concentratie,
            @Producator, @TaraProducator, @FirmaDetinatoare, @TaraDetinatoare,
            @CodATC, @ActiuneTerapeutica, @PrescriptieMedicala,
            @NrDataAmbalajAPP, @Ambalaj, @VolumAmbalaj, @ValabilitateAmbalaj,
            @Bulina, @Diez, @Stea, @Triunghi, @Dreptunghi,
            @DataActualizareANM, @SursaFisier, 1
        );
        
        SET @IsNew = 1;
    END
END
GO

-- Actualizează stored procedure Medicamente_Search pentru a returna toate coloanele
CREATE OR ALTER PROCEDURE [dbo].[Medicamente_Search]
    @SearchTerm NVARCHAR(100),
    @MaxResults INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@MaxResults)
        [Id], [CodANM], [NumeComercial], [DCI], [FormaFarmaceutica], [Concentratie],
        [Producator], [TaraProducator], [FirmaDetinatoare], [TaraDetinatoare],
        [CodATC], [ActiuneTerapeutica], [PrescriptieMedicala],
        [NrDataAmbalajAPP], [Ambalaj], [VolumAmbalaj], [ValabilitateAmbalaj],
        [Bulina], [Diez], [Stea], [Triunghi], [Dreptunghi],
        [DataActualizareANM], [Compensat], [ProcentCompensare],
        [DataImport], [DataUltimaActualizare], [SursaFisier], [Activ]
    FROM [dbo].[Medicamente_Nomenclator]
    WHERE [Activ] = 1
      AND (
          [NumeComercial] LIKE '%' + @SearchTerm + '%'
          OR [DCI] LIKE '%' + @SearchTerm + '%'
          OR [CodANM] LIKE '%' + @SearchTerm + '%'
          OR [CodATC] LIKE '%' + @SearchTerm + '%'
      )
    ORDER BY 
        CASE WHEN [NumeComercial] LIKE @SearchTerm + '%' THEN 0 ELSE 1 END,
        [NumeComercial];
END
GO

-- Actualizează stored procedure Medicamente_GetByCod
CREATE OR ALTER PROCEDURE [dbo].[Medicamente_GetByCod]
    @CodANM NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id], [CodANM], [NumeComercial], [DCI], [FormaFarmaceutica], [Concentratie],
        [Producator], [TaraProducator], [FirmaDetinatoare], [TaraDetinatoare],
        [CodATC], [ActiuneTerapeutica], [PrescriptieMedicala],
        [NrDataAmbalajAPP], [Ambalaj], [VolumAmbalaj], [ValabilitateAmbalaj],
        [Bulina], [Diez], [Stea], [Triunghi], [Dreptunghi],
        [DataActualizareANM], [Compensat], [ProcentCompensare],
        [DataImport], [DataUltimaActualizare], [SursaFisier], [Activ]
    FROM [dbo].[Medicamente_Nomenclator]
    WHERE [CodANM] = @CodANM;
END
GO

PRINT 'Stored procedures actualizate.';
GO
