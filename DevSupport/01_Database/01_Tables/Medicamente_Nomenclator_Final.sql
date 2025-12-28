-- =============================================
-- Script: Recreare tabela Medicamente_Nomenclator cu coloane exacte din Excel
-- Data: 2025-12-27
-- Descriere: Coloanele respecta exact structura nomenclatorului ANM (fara diacritice)
-- =============================================

USE [ValyanMed]
GO

SET QUOTED_IDENTIFIER ON
GO

-- Sterge tabela veche daca exista
IF OBJECT_ID('dbo.Medicamente_Nomenclator', 'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[Medicamente_Nomenclator];
    PRINT 'Tabela veche stearsa.';
END
GO

-- Creare tabela noua cu coloane exacte din Excel ANM
-- Excel columns: 1-Cod CIM, 2-Denumire comerciala, 3-DCI, 4-Forma farmaceutica, 
-- 5-Concentratie, 6-Firma/tara producatoare APP, 7-Firma/tara detinatoare APP,
-- 8-Cod ATC, 9-Actiune terapeutica, 10-Prescriptie, 11-Nr/data ambalaj APP,
-- 12-Ambalaj, 13-Volum ambalaj, 14-Valabilitate ambalaj, 15-Bulina,
-- 16-Diez, 17-Stea, 18-Triunghi, 19-Dreptunghi, 20-Data actualizare

CREATE TABLE [dbo].[Medicamente_Nomenclator] (
    -- Cheie primara
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    
    -- Col 1: Cod CIM (codul unic ANM)
    [CodCIM] NVARCHAR(20) NOT NULL,
    
    -- Col 2: Denumire comerciala
    [DenumireComerciala] NVARCHAR(500) NOT NULL,
    
    -- Col 3: DCI (Denumire Comuna Internationala)
    [DCI] NVARCHAR(500) NULL,
    
    -- Col 4: Forma farmaceutica
    [FormaFarmaceutica] NVARCHAR(200) NULL,
    
    -- Col 5: Concentratie
    [Concentratie] NVARCHAR(200) NULL,
    
    -- Col 6: Firma / tara producatoare APP
    [FirmaTaraProducatoareAPP] NVARCHAR(500) NULL,
    
    -- Col 7: Firma / tara detinatoare APP
    [FirmaTaraDetinatoareAPP] NVARCHAR(500) NULL,
    
    -- Col 8: Cod ATC
    [CodATC] NVARCHAR(20) NULL,
    
    -- Col 9: Actiune terapeutica
    [ActiuneTerapeutica] NVARCHAR(500) NULL,
    
    -- Col 10: Prescriptie (P, PRF, OTC, etc.)
    [Prescriptie] NVARCHAR(50) NULL,
    
    -- Col 11: Nr / data ambalaj APP
    [NrDataAmbalajAPP] NVARCHAR(200) NULL,
    
    -- Col 12: Ambalaj
    [Ambalaj] NVARCHAR(500) NULL,
    
    -- Col 13: Volum ambalaj
    [VolumAmbalaj] NVARCHAR(100) NULL,
    
    -- Col 14: Valabilitate ambalaj
    [ValabilitateAmbalaj] NVARCHAR(100) NULL,
    
    -- Col 15: Bulina (simbol)
    [Bulina] NVARCHAR(10) NULL,
    
    -- Col 16: Diez (simbol #)
    [Diez] NVARCHAR(10) NULL,
    
    -- Col 17: Stea (simbol)
    [Stea] NVARCHAR(10) NULL,
    
    -- Col 18: Triunghi (simbol)
    [Triunghi] NVARCHAR(10) NULL,
    
    -- Col 19: Dreptunghi (simbol)
    [Dreptunghi] NVARCHAR(10) NULL,
    
    -- Col 20: Data actualizare
    [DataActualizare] NVARCHAR(50) NULL,
    
    -- Coloane de audit
    [DataImport] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [DataUltimaActualizare] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [SursaFisier] NVARCHAR(500) NULL,
    [Activ] BIT NOT NULL DEFAULT 1,
    
    CONSTRAINT [PK_Medicamente_Nomenclator] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Index unic pe CodCIM
CREATE UNIQUE INDEX [IX_Medicamente_Nomenclator_CodCIM] 
ON [dbo].[Medicamente_Nomenclator] ([CodCIM]);
GO

-- Index pentru cautare
CREATE NONCLUSTERED INDEX [IX_Medicamente_Nomenclator_Search] 
ON [dbo].[Medicamente_Nomenclator] ([DenumireComerciala], [DCI], [CodATC])
WHERE [Activ] = 1;
GO

PRINT 'Tabela Medicamente_Nomenclator recreata cu coloane exacte din Excel.';
GO

-- Actualizare stored procedures
CREATE OR ALTER PROCEDURE [dbo].[Medicamente_Upsert]
    @CodCIM NVARCHAR(20),
    @DenumireComerciala NVARCHAR(500),
    @DCI NVARCHAR(500) = NULL,
    @FormaFarmaceutica NVARCHAR(200) = NULL,
    @Concentratie NVARCHAR(200) = NULL,
    @FirmaTaraProducatoareAPP NVARCHAR(500) = NULL,
    @FirmaTaraDetinatoareAPP NVARCHAR(500) = NULL,
    @CodATC NVARCHAR(20) = NULL,
    @ActiuneTerapeutica NVARCHAR(500) = NULL,
    @Prescriptie NVARCHAR(50) = NULL,
    @NrDataAmbalajAPP NVARCHAR(200) = NULL,
    @Ambalaj NVARCHAR(500) = NULL,
    @VolumAmbalaj NVARCHAR(100) = NULL,
    @ValabilitateAmbalaj NVARCHAR(100) = NULL,
    @Bulina NVARCHAR(10) = NULL,
    @Diez NVARCHAR(10) = NULL,
    @Stea NVARCHAR(10) = NULL,
    @Triunghi NVARCHAR(10) = NULL,
    @Dreptunghi NVARCHAR(10) = NULL,
    @DataActualizare NVARCHAR(50) = NULL,
    @SursaFisier NVARCHAR(500) = NULL,
    @IsNew BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM [dbo].[Medicamente_Nomenclator] WHERE [CodCIM] = @CodCIM)
    BEGIN
        UPDATE [dbo].[Medicamente_Nomenclator]
        SET [DenumireComerciala] = @DenumireComerciala,
            [DCI] = @DCI,
            [FormaFarmaceutica] = @FormaFarmaceutica,
            [Concentratie] = @Concentratie,
            [FirmaTaraProducatoareAPP] = @FirmaTaraProducatoareAPP,
            [FirmaTaraDetinatoareAPP] = @FirmaTaraDetinatoareAPP,
            [CodATC] = @CodATC,
            [ActiuneTerapeutica] = @ActiuneTerapeutica,
            [Prescriptie] = @Prescriptie,
            [NrDataAmbalajAPP] = @NrDataAmbalajAPP,
            [Ambalaj] = @Ambalaj,
            [VolumAmbalaj] = @VolumAmbalaj,
            [ValabilitateAmbalaj] = @ValabilitateAmbalaj,
            [Bulina] = @Bulina,
            [Diez] = @Diez,
            [Stea] = @Stea,
            [Triunghi] = @Triunghi,
            [Dreptunghi] = @Dreptunghi,
            [DataActualizare] = @DataActualizare,
            [DataUltimaActualizare] = GETDATE(),
            [SursaFisier] = @SursaFisier,
            [Activ] = 1
        WHERE [CodCIM] = @CodCIM;
        
        SET @IsNew = 0;
    END
    ELSE
    BEGIN
        INSERT INTO [dbo].[Medicamente_Nomenclator] (
            [CodCIM], [DenumireComerciala], [DCI], [FormaFarmaceutica], [Concentratie],
            [FirmaTaraProducatoareAPP], [FirmaTaraDetinatoareAPP], [CodATC], [ActiuneTerapeutica],
            [Prescriptie], [NrDataAmbalajAPP], [Ambalaj], [VolumAmbalaj], [ValabilitateAmbalaj],
            [Bulina], [Diez], [Stea], [Triunghi], [Dreptunghi], [DataActualizare],
            [SursaFisier], [Activ]
        )
        VALUES (
            @CodCIM, @DenumireComerciala, @DCI, @FormaFarmaceutica, @Concentratie,
            @FirmaTaraProducatoareAPP, @FirmaTaraDetinatoareAPP, @CodATC, @ActiuneTerapeutica,
            @Prescriptie, @NrDataAmbalajAPP, @Ambalaj, @VolumAmbalaj, @ValabilitateAmbalaj,
            @Bulina, @Diez, @Stea, @Triunghi, @Dreptunghi, @DataActualizare,
            @SursaFisier, 1
        );
        
        SET @IsNew = 1;
    END
END
GO

CREATE OR ALTER PROCEDURE [dbo].[Medicamente_Search]
    @SearchTerm NVARCHAR(100),
    @MaxResults INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@MaxResults)
        [Id], [CodCIM], [DenumireComerciala], [DCI], [FormaFarmaceutica], [Concentratie],
        [FirmaTaraProducatoareAPP], [FirmaTaraDetinatoareAPP], [CodATC], [ActiuneTerapeutica],
        [Prescriptie], [NrDataAmbalajAPP], [Ambalaj], [VolumAmbalaj], [ValabilitateAmbalaj],
        [Bulina], [Diez], [Stea], [Triunghi], [Dreptunghi], [DataActualizare],
        [DataImport], [DataUltimaActualizare], [SursaFisier], [Activ]
    FROM [dbo].[Medicamente_Nomenclator]
    WHERE [Activ] = 1
      AND (
          [DenumireComerciala] LIKE '%' + @SearchTerm + '%'
          OR [DCI] LIKE '%' + @SearchTerm + '%'
          OR [CodCIM] LIKE '%' + @SearchTerm + '%'
          OR [CodATC] LIKE '%' + @SearchTerm + '%'
      )
    ORDER BY 
        CASE WHEN [DenumireComerciala] LIKE @SearchTerm + '%' THEN 0 ELSE 1 END,
        [DenumireComerciala];
END
GO

CREATE OR ALTER PROCEDURE [dbo].[Medicamente_GetByCod]
    @CodCIM NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id], [CodCIM], [DenumireComerciala], [DCI], [FormaFarmaceutica], [Concentratie],
        [FirmaTaraProducatoareAPP], [FirmaTaraDetinatoareAPP], [CodATC], [ActiuneTerapeutica],
        [Prescriptie], [NrDataAmbalajAPP], [Ambalaj], [VolumAmbalaj], [ValabilitateAmbalaj],
        [Bulina], [Diez], [Stea], [Triunghi], [Dreptunghi], [DataActualizare],
        [DataImport], [DataUltimaActualizare], [SursaFisier], [Activ]
    FROM [dbo].[Medicamente_Nomenclator]
    WHERE [CodCIM] = @CodCIM;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[Medicamente_DeactivateOld]
    @SursaFisier NVARCHAR(500),
    @DeactivatedCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[Medicamente_Nomenclator]
    SET [Activ] = 0,
        [DataUltimaActualizare] = GETDATE()
    WHERE [SursaFisier] <> @SursaFisier
      AND [Activ] = 1;
    
    SET @DeactivatedCount = @@ROWCOUNT;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[Medicamente_GetStats]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        (SELECT COUNT(*) FROM [dbo].[Medicamente_Nomenclator] WHERE [Activ] = 1) AS TotalActive,
        (SELECT COUNT(*) FROM [dbo].[Medicamente_Nomenclator] WHERE [Activ] = 0) AS TotalInactive,
        (SELECT COUNT(DISTINCT [DCI]) FROM [dbo].[Medicamente_Nomenclator] WHERE [Activ] = 1 AND [DCI] IS NOT NULL) AS TotalDCI,
        (SELECT COUNT(DISTINCT [FirmaTaraProducatoareAPP]) FROM [dbo].[Medicamente_Nomenclator] WHERE [Activ] = 1 AND [FirmaTaraProducatoareAPP] IS NOT NULL) AS TotalProducatori,
        (SELECT MAX([DataUltimaActualizare]) FROM [dbo].[Medicamente_Nomenclator]) AS UltimaActualizare,
        (SELECT MAX([DataEnd]) FROM [dbo].[Medicamente_SyncLog] WHERE [Status] = 'Success') AS UltimaSincronizareReusita;
END
GO

PRINT 'Stored procedures actualizate.';
GO
