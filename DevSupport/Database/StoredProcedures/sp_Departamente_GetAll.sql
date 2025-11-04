-- ========================================
-- Stored Procedure: sp_Departamente_GetAll
-- Database: ValyanMed
-- Created: 10/13/2025 14:54:02
-- Modified: 10/13/2025 14:54:02
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
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
