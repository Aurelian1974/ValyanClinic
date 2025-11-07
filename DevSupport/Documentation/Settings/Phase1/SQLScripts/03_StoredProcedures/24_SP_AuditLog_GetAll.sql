-- ========================================
-- Stored Procedure: SP_AuditLog_GetAll
-- Description: Returneaza audit logs cu filtre si paginare
-- ========================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_AuditLog_GetAll]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[SP_AuditLog_GetAll]
GO

CREATE PROCEDURE [dbo].[SP_AuditLog_GetAll]
    @PageNumber INT = 1,
    @PageSize INT = 50,
  @SearchText NVARCHAR(255) = NULL,
    @UtilizatorID UNIQUEIDENTIFIER = NULL,
    @Actiune NVARCHAR(100) = NULL,
    @DataStart DATETIME2 = NULL,
    @DataEnd DATETIME2 = NULL,
    @SortColumn NVARCHAR(50) = 'DataActiune',
    @SortDirection NVARCHAR(4) = 'DESC'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Returneaza datele paginate
    SELECT 
 AuditID,
        UtilizatorID,
        UserName,
        Actiune,
        DataActiune,
        Entitate,
      EntitateID,
     ValoareVeche,
        ValoareNoua,
      AdresaIP,
     UserAgent,
        Dispozitiv,
        StatusActiune,
        DetaliiEroare
    FROM Audit_Log
    WHERE (@SearchText IS NULL OR UserName LIKE '%' + @SearchText + '%' OR Actiune LIKE '%' + @SearchText + '%')
      AND (@UtilizatorID IS NULL OR UtilizatorID = @UtilizatorID)
      AND (@Actiune IS NULL OR Actiune = @Actiune)
      AND (@DataStart IS NULL OR DataActiune >= @DataStart)
      AND (@DataEnd IS NULL OR DataActiune <= @DataEnd)
  ORDER BY 
        CASE WHEN @SortDirection = 'ASC' THEN DataActiune END ASC,
        CASE WHEN @SortDirection = 'DESC' THEN DataActiune END DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    -- Returneaza total count
    SELECT COUNT(*) AS TotalCount
    FROM Audit_Log
    WHERE (@SearchText IS NULL OR UserName LIKE '%' + @SearchText + '%' OR Actiune LIKE '%' + @SearchText + '%')
      AND (@UtilizatorID IS NULL OR UtilizatorID = @UtilizatorID)
      AND (@Actiune IS NULL OR Actiune = @Actiune)
      AND (@DataStart IS NULL OR DataActiune >= @DataStart)
      AND (@DataEnd IS NULL OR DataActiune <= @DataEnd);
END
GO
