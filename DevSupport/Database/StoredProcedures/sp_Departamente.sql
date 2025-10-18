USE [ValyanMed]
GO

-- sp_Departamente_GetAll
IF OBJECT_ID('dbo.sp_Departamente_GetAll', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Departamente_GetAll;
GO

CREATE PROCEDURE dbo.sp_Departamente_GetAll
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @SearchText NVARCHAR(200) = NULL,
    @IdTipDepartament UNIQUEIDENTIFIER = NULL,
    @SortColumn NVARCHAR(50) = 'DenumireDepartament',
    @SortDirection NVARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        d.IdDepartament,
        d.IdTipDepartament,
        d.DenumireDepartament,
        d.DescriereDepartament,
        td.DenumireTipDepartament
    FROM Departamente d
    LEFT JOIN TipDepartament td ON d.IdTipDepartament = td.IdTipDepartament
    WHERE 
        (@SearchText IS NULL OR 
         d.DenumireDepartament LIKE '%' + @SearchText + '%' OR
         d.DescriereDepartament LIKE '%' + @SearchText + '%')
        AND (@IdTipDepartament IS NULL OR d.IdTipDepartament = @IdTipDepartament)
    ORDER BY 
        CASE WHEN @SortColumn = 'DenumireDepartament' AND @SortDirection = 'ASC' THEN d.DenumireDepartament END ASC,
        CASE WHEN @SortColumn = 'DenumireDepartament' AND @SortDirection = 'DESC' THEN d.DenumireDepartament END DESC,
        CASE WHEN @SortColumn = 'DenumireTipDepartament' AND @SortDirection = 'ASC' THEN td.DenumireTipDepartament END ASC,
        CASE WHEN @SortColumn = 'DenumireTipDepartament' AND @SortDirection = 'DESC' THEN td.DenumireTipDepartament END DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- sp_Departamente_GetCount
IF OBJECT_ID('dbo.sp_Departamente_GetCount', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Departamente_GetCount;
GO

CREATE PROCEDURE dbo.sp_Departamente_GetCount
    @SearchText NVARCHAR(200) = NULL,
    @IdTipDepartament UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*)
    FROM Departamente d
    WHERE 
        (@SearchText IS NULL OR 
         d.DenumireDepartament LIKE '%' + @SearchText + '%' OR
         d.DescriereDepartament LIKE '%' + @SearchText + '%')
        AND (@IdTipDepartament IS NULL OR d.IdTipDepartament = @IdTipDepartament);
END
GO

-- sp_Departamente_GetById
IF OBJECT_ID('dbo.sp_Departamente_GetById', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Departamente_GetById;
GO

CREATE PROCEDURE dbo.sp_Departamente_GetById
    @IdDepartament UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        d.IdDepartament,
        d.IdTipDepartament,
        d.DenumireDepartament,
        d.DescriereDepartament,
        td.DenumireTipDepartament
    FROM Departamente d
    LEFT JOIN TipDepartament td ON d.IdTipDepartament = td.IdTipDepartament
    WHERE d.IdDepartament = @IdDepartament;
END
GO

-- sp_Departamente_Create
IF OBJECT_ID('dbo.sp_Departamente_Create', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Departamente_Create;
GO

CREATE PROCEDURE dbo.sp_Departamente_Create
    @IdTipDepartament UNIQUEIDENTIFIER = NULL,
    @DenumireDepartament VARCHAR(200),
    @DescriereDepartament VARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @IdDepartament UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO Departamente (IdDepartament, IdTipDepartament, DenumireDepartament, DescriereDepartament)
    VALUES (@IdDepartament, @IdTipDepartament, @DenumireDepartament, @DescriereDepartament);
    
    SELECT 
        d.IdDepartament,
        d.IdTipDepartament,
        d.DenumireDepartament,
        d.DescriereDepartament,
        td.DenumireTipDepartament
    FROM Departamente d
    LEFT JOIN TipDepartament td ON d.IdTipDepartament = td.IdTipDepartament
    WHERE d.IdDepartament = @IdDepartament;
END
GO

-- sp_Departamente_Update
IF OBJECT_ID('dbo.sp_Departamente_Update', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Departamente_Update;
GO

CREATE PROCEDURE dbo.sp_Departamente_Update
    @IdDepartament UNIQUEIDENTIFIER,
    @IdTipDepartament UNIQUEIDENTIFIER = NULL,
    @DenumireDepartament VARCHAR(200),
    @DescriereDepartament VARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Departamente
    SET 
        IdTipDepartament = @IdTipDepartament,
        DenumireDepartament = @DenumireDepartament,
        DescriereDepartament = @DescriereDepartament
    WHERE IdDepartament = @IdDepartament;
    
    SELECT 
        d.IdDepartament,
        d.IdTipDepartament,
        d.DenumireDepartament,
        d.DescriereDepartament,
        td.DenumireTipDepartament
    FROM Departamente d
    LEFT JOIN TipDepartament td ON d.IdTipDepartament = td.IdTipDepartament
    WHERE d.IdDepartament = @IdDepartament;
END
GO

-- sp_Departamente_Delete
IF OBJECT_ID('dbo.sp_Departamente_Delete', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Departamente_Delete;
GO

CREATE PROCEDURE dbo.sp_Departamente_Delete
    @IdDepartament UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM Departamente
    WHERE IdDepartament = @IdDepartament;
    
    SELECT 1 AS Success;
END
GO

-- sp_Departamente_CheckUnique
IF OBJECT_ID('dbo.sp_Departamente_CheckUnique', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Departamente_CheckUnique;
GO

CREATE PROCEDURE dbo.sp_Departamente_CheckUnique
    @DenumireDepartament VARCHAR(200),
    @ExcludeId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT CASE 
        WHEN EXISTS (
            SELECT 1 
            FROM Departamente 
            WHERE DenumireDepartament = @DenumireDepartament 
            AND (@ExcludeId IS NULL OR IdDepartament != @ExcludeId)
        ) THEN 1 
        ELSE 0 
    END AS Denumire_Exists;
END
GO

-- sp_TipDepartament_GetAll
IF OBJECT_ID('dbo.sp_TipDepartament_GetAll', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_TipDepartament_GetAll;
GO

CREATE PROCEDURE dbo.sp_TipDepartament_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        IdTipDepartament,
        DenumireTipDepartament
    FROM TipDepartament
    ORDER BY DenumireTipDepartament;
END
GO

PRINT 'Stored procedures pentru Departamente create cu succes!';
