-- =============================================
-- Stored Procedures pentru Medicamente Nomenclator
-- =============================================

-- SP: Căutare medicamente pentru autocomplete cu filtrare avansată
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Medicamente_Search')
    DROP PROCEDURE [dbo].[Medicamente_Search]
GO

CREATE PROCEDURE [dbo].[Medicamente_Search]
    @SearchTerm NVARCHAR(100),
    @MaxResults INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Case-insensitive search cu UPPER pentru siguranță
    DECLARE @SearchTermUpper NVARCHAR(100) = UPPER(LTRIM(RTRIM(@SearchTerm)));
    
    -- Split search term în cuvinte pentru căutare inteligentă
    -- Ex: "sortis 10mg" -> caută "sortis" în denumire și "10mg" în concentrație
    DECLARE @Words TABLE (Word NVARCHAR(50));
    
    -- Simple word split (funcționează pentru majoritatea cazurilor)
    INSERT INTO @Words (Word)
    SELECT UPPER(LTRIM(RTRIM(value))) 
    FROM STRING_SPLIT(@SearchTermUpper, ' ')
    WHERE LTRIM(RTRIM(value)) <> '';
    
    -- Numără cuvinte pentru prioritizare
    DECLARE @WordCount INT = (SELECT COUNT(*) FROM @Words);
    
    SELECT TOP (@MaxResults)
        [Id],
        [CodCIM],
        [DenumireComerciala],
        [DCI],
        [FormaFarmaceutica],
        [Concentratie],
        [FirmaTaraProducatoareAPP],
        [FirmaTaraDetinatoareAPP],
        [CodATC],
        [ActiuneTerapeutica],
        [Prescriptie],
        [NrDataAmbalajAPP],
        [Ambalaj],
        [VolumAmbalaj],
        [ValabilitateAmbalaj],
        [Bulina],
        [Diez],
        [Stea],
        [Triunghi],
        [Dreptunghi],
        [DataActualizare],
        [DataImport],
        [DataUltimaActualizare],
        [Activ],
        -- Scoring pentru prioritizare rezultate
        (
            -- Match exact în denumire (prioritate maximă)
            CASE WHEN UPPER([DenumireComerciala]) LIKE @SearchTermUpper + '%' THEN 1000 ELSE 0 END +
            CASE WHEN UPPER([DenumireComerciala]) = @SearchTermUpper THEN 500 ELSE 0 END +
            
            -- Match în DCI
            CASE WHEN UPPER([DCI]) LIKE @SearchTermUpper + '%' THEN 100 ELSE 0 END +
            
            -- Match multi-cuvânt (ex: "sortis 10mg")
            CASE WHEN @WordCount > 1 THEN
                (SELECT COUNT(*) * 200 FROM @Words w 
                 WHERE UPPER([DenumireComerciala]) LIKE '%' + w.Word + '%' 
                    OR UPPER([Concentratie]) LIKE '%' + w.Word + '%'
                    OR UPPER([FormaFarmaceutica]) LIKE '%' + w.Word + '%')
            ELSE 0 END +
            
            -- Match parțial în denumire
            CASE WHEN UPPER([DenumireComerciala]) LIKE '%' + @SearchTermUpper + '%' THEN 50 ELSE 0 END
        ) AS Score
    FROM [dbo].[Medicamente_Nomenclator]
    WHERE [Activ] = 1
      AND (
          -- Căutare de bază (un singur cuvânt)
          @WordCount = 1 AND (
              UPPER([DenumireComerciala]) LIKE '%' + @SearchTermUpper + '%'
              OR UPPER([DCI]) LIKE '%' + @SearchTermUpper + '%'
              OR UPPER([CodCIM]) LIKE '%' + @SearchTermUpper + '%'
          )
          
          -- Căutare avansată multi-cuvânt (TOATE cuvintele trebuie prezente)
          OR (
              @WordCount > 1 
              AND (
                  SELECT COUNT(DISTINCT w.Word) 
                  FROM @Words w
                  WHERE UPPER([DenumireComerciala]) LIKE '%' + w.Word + '%'
                     OR UPPER([Concentratie]) LIKE '%' + w.Word + '%'
                     OR UPPER([FormaFarmaceutica]) LIKE '%' + w.Word + '%'
                     OR UPPER([DCI]) LIKE '%' + w.Word + '%'
              ) = @WordCount
          )
      )
    ORDER BY 
        Score DESC,
        [DenumireComerciala]
END
GO

-- SP: Obține medicament după cod ANM
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Medicamente_GetByCod')
    DROP PROCEDURE [dbo].[Medicamente_GetByCod]
GO

CREATE PROCEDURE [dbo].[Medicamente_GetByCod]
    @CodCIM NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [CodCIM],
        [DenumireComerciala],
        [DCI],
        [FormaFarmaceutica],
        [Concentratie],
        [FirmaTaraProducatoareAPP],
        [FirmaTaraDetinatoareAPP],
        [CodATC],
        [ActiuneTerapeutica],
        [Prescriptie],
        [NrDataAmbalajAPP],
        [Ambalaj],
        [VolumAmbalaj],
        [ValabilitateAmbalaj],
        [Bulina],
        [Diez],
        [Stea],
        [Triunghi],
        [Dreptunghi],
        [DataActualizare],
        [DataImport],
        [DataUltimaActualizare],
        CIM NVARCHAR(20),
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
        -- Update
        UPDATE [dbo].[Medicamente_Nomenclator]
        SET 
            [DenumireComerciala] = @DenumireComerciala,
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
        -- Insert
        INSERT INTO [dbo].[Medicamente_Nomenclator]
            ([CodCIM], [DenumireComerciala], [DCI], [FormaFarmaceutica], [Concentratie],
             [FirmaTaraProducatoareAPP], [FirmaTaraDetinatoareAPP], [CodATC], [ActiuneTerapeutica],
             [Prescriptie], [NrDataAmbalajAPP], [Ambalaj], [VolumAmbalaj], [ValabilitateAmbalaj],
             [Bulina], [Diez], [Stea], [Triunghi], [Dreptunghi], [DataActualizare], [SursaFisier])
        VALUES
            (@CodCIM, @DenumireComerciala, @DCI, @FormaFarmaceutica, @Concentratie,
             @FirmaTaraProducatoareAPP, @FirmaTaraDetinatoareAPP, @CodATC, @ActiuneTerapeutica,
             @Prescriptie, @NrDataAmbalajAPP, @Ambalaj, @VolumAmbalaj, @ValabilitateAmbalaj,
             @Bulina, @Diez, @Stea, @Triunghi, @Dreptunghi, @DataActualizare
        
        SET @IsNew = 0;
    END
    ELSE
    BEGIN
        -- Insert
        INSERT INTO [dbo].[Medicamente_Nomenclator]
            ([CodANM], [NumeComercial], [DCI], [FormaFarmaceutica], [CodATC], 
             [Concentratie], [Producator], [TaraProducator], [SursaFisier])
        VALUES
            (@CodANM, @NumeComercial, @DCI, @FormaFarmaceutica, @CodATC,
             @Concentratie, @Producator, @TaraProducator, @SursaFisier);
        
        SET @IsNew = 1;
    END
END
GO

-- SP: Dezactivează medicamentele care nu mai sunt în nomenclator
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Medicamente_DeactivateOld')
    DROP PROCEDURE [dbo].[Medicamente_DeactivateOld]
GO

CREATE PROCEDURE [dbo].[Medicamente_DeactivateOld]
    @SursaFisier NVARCHAR(500),
    @DeactivatedCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Dezactivează medicamentele care nu au fost actualizate în acest import
    UPDATE [dbo].[Medicamente_Nomenclator]
    SET [Activ] = 0,
        [DataUltimaActualizare] = GETDATE()
    WHERE [SursaFisier] != @SursaFisier
      AND [Activ] = 1
      AND [DataUltimaActualizare] < DATEADD(DAY, -1, GETDATE());
    
    SET @DeactivatedCount = @@ROWCOUNT;
END
GO

-- SP: Log sincronizare - Start
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Medicamente_SyncLog_Start')
    DROP PROCEDURE [dbo].[Medicamente_SyncLog_Start]
GO

CREATE PROCEDURE [dbo].[Medicamente_SyncLog_Start]
    @SursaURL NVARCHAR(500),
    @LogId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    SET @LogId = NEWID();
    
    INSERT INTO [dbo].[Medicamente_SyncLog]
        ([Id], [DataStart], [Status], [SursaURL])
    VALUES
        (@LogId, GETDATE(), 'InProgress', @SursaURL);
END
GO

-- SP: Log sincronizare - Complete
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Medicamente_SyncLog_Complete')
    DROP PROCEDURE [dbo].[Medicamente_SyncLog_Complete]
GO

CREATE PROCEDURE [dbo].[Medicamente_SyncLog_Complete]
    @LogId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @TotalRecords INT = NULL,
    @RecordsAdded INT = NULL,
    @RecordsUpdated INT = NULL,
    @RecordsDeactivated INT = NULL,
    @ErrorMessage NVARCHAR(MAX) = NULL,
    @NumeFisier NVARCHAR(200) = NULL,
    @DimensiuneFisier BIGINT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[Medicamente_SyncLog]
    SET 
        [DataEnd] = GETDATE(),
        [Status] = @Status,
        [TotalRecords] = @TotalRecords,
        [RecordsAdded] = @RecordsAdded,
        [RecordsUpdated] = @RecordsUpdated,
        [RecordsDeactivated] = @RecordsDeactivated,
        [ErrorMessage] = @ErrorMessage,
        [NumeFisier] = @NumeFisier,
        [DimensiuneFisier] = @DimensiuneFisier
    WHERE [Id] = @LogId;
END
GO

-- SP: Statistici nomenclator
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Medicamente_GetStats')
    DROP PROCEDURE [dbo].[Medicamente_GetStats]
GO

CREATE PROCEDURE [dbo].[Medicamente_GetStats]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        (SELECT COUNT(*) FROM [dbo].[Medicamente_Nomenclator] WHERE [Activ] = 1) AS TotalActive,
        (SELECT COUNT(*) FROM [dbo].[Medicamente_Nomenclator] WHERE [Activ] = 0) AS TotalInactive,
        (SELECT COUNT(DISTINCT [DCI]) FROM [dbo].[Medicamente_Nomenclator] WHERE [Activ] = 1) AS TotalDCI,
        (SELECT COUNT(DISTINCT [FirmaTaraProducatoareAPP]) FROM [dbo].[Medicamente_Nomenclator] WHERE [Activ] = 1) AS TotalProducatori,
        (SELECT MAX([DataUltimaActualizare]) FROM [dbo].[Medicamente_Nomenclator]) AS UltimaActualizare,
        (SELECT TOP 1 [DataEnd] FROM [dbo].[Medicamente_SyncLog] WHERE [Status] = 'Success' ORDER BY [Id] DESC) AS UltimaSincronizareReusita
END
GO

PRINT 'Stored procedures pentru Medicamente au fost create.'
GO
