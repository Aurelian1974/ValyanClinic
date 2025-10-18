-- ========================================
-- Stored Procedure: sp_Departamente_GetCount
-- Database: ValyanMed
-- Created: 10/13/2025 14:54:02
-- Modified: 10/13/2025 14:54:02
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
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
