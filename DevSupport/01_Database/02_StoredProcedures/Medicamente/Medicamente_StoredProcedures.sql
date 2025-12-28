-- =============================================
-- Stored Procedures pentru Medicamente Nomenclator
-- =============================================

-- SP: Căutare medicamente pentru autocomplete
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Medicamente_Search')
    DROP PROCEDURE [dbo].[Medicamente_Search]
GO

CREATE PROCEDURE [dbo].[Medicamente_Search]
    @SearchTerm NVARCHAR(100),
    @MaxResults INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@MaxResults)
        [Id],
        [CodANM],
        [NumeComercial],
        [DCI],
        [FormaFarmaceutica],
        [CodATC],
        [Concentratie],
        [Producator]
    FROM [dbo].[Medicamente_Nomenclator]
    WHERE [Activ] = 1
      AND (
          [NumeComercial] LIKE @SearchTerm + '%'
          OR [NumeComercial] LIKE '%' + @SearchTerm + '%'
          OR [DCI] LIKE @SearchTerm + '%'
          OR [CodANM] LIKE @SearchTerm + '%'
      )
    ORDER BY 
        CASE WHEN [NumeComercial] LIKE @SearchTerm + '%' THEN 0 ELSE 1 END,
        [NumeComercial]
END
GO

-- SP: Obține medicament după cod ANM
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Medicamente_GetByCod')
    DROP PROCEDURE [dbo].[Medicamente_GetByCod]
GO

CREATE PROCEDURE [dbo].[Medicamente_GetByCod]
    @CodANM NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [Id],
        [CodANM],
        [NumeComercial],
        [DCI],
        [FormaFarmaceutica],
        [CodATC],
        [Concentratie],
        [Producator],
        [TaraProducator],
        [PrescriptieMedicala],
        [Compensat],
        [ProcentCompensare]
    FROM [dbo].[Medicamente_Nomenclator]
    WHERE [CodANM] = @CodANM AND [Activ] = 1
END
GO

-- SP: Upsert medicament (pentru import)
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Medicamente_Upsert')
    DROP PROCEDURE [dbo].[Medicamente_Upsert]
GO

CREATE PROCEDURE [dbo].[Medicamente_Upsert]
    @CodANM NVARCHAR(20),
    @NumeComercial NVARCHAR(500),
    @DCI NVARCHAR(500) = NULL,
    @FormaFarmaceutica NVARCHAR(200) = NULL,
    @CodATC NVARCHAR(20) = NULL,
    @Concentratie NVARCHAR(200) = NULL,
    @Producator NVARCHAR(500) = NULL,
    @TaraProducator NVARCHAR(100) = NULL,
    @SursaFisier NVARCHAR(500) = NULL,
    @IsNew BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM [dbo].[Medicamente_Nomenclator] WHERE [CodANM] = @CodANM)
    BEGIN
        -- Update
        UPDATE [dbo].[Medicamente_Nomenclator]
        SET 
            [NumeComercial] = @NumeComercial,
            [DCI] = @DCI,
            [FormaFarmaceutica] = @FormaFarmaceutica,
            [CodATC] = @CodATC,
            [Concentratie] = @Concentratie,
            [Producator] = @Producator,
            [TaraProducator] = @TaraProducator,
            [DataUltimaActualizare] = GETDATE(),
            [SursaFisier] = @SursaFisier,
            [Activ] = 1
        WHERE [CodANM] = @CodANM;
        
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
        (SELECT COUNT(DISTINCT [Producator]) FROM [dbo].[Medicamente_Nomenclator] WHERE [Activ] = 1) AS TotalProducatori,
        (SELECT MAX([DataUltimaActualizare]) FROM [dbo].[Medicamente_Nomenclator]) AS UltimaActualizare,
        (SELECT TOP 1 [DataEnd] FROM [dbo].[Medicamente_SyncLog] WHERE [Status] = 'Success' ORDER BY [Id] DESC) AS UltimaSincronizareReusita
END
GO

PRINT 'Stored procedures pentru Medicamente au fost create.'
GO
