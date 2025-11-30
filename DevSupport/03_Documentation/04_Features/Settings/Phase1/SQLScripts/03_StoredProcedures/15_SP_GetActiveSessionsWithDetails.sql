-- =============================================
-- Author:      DevSupport Team
-- Create date: 2025-11-30
-- Description: Gets active sessions with user details (JOIN with Utilizatori)
-- 
-- Purpose:
--   Retrieves active user sessions with full user information
--   Supports filtering by user ID, expiration status, and sorting
--   Used by admin dashboard and user profile pages
--
-- Parameters:
--   @UtilizatorID (optional) - Filter by specific user
--   @DoarExpiraInCurand - Filter sessions expiring within 15 minutes
--   @SortColumn - Column name for sorting (validated against whitelist)
--   @SortDirection - Sort direction: ASC or DESC
--
-- Security:
--   - Uses INNER JOIN to ensure data integrity
--   - Column whitelist prevents SQL injection
--   - Parameterized filters for safe queries
--
-- Performance:
--   - Indexed on EsteActiva, DataExpirare, UtilizatorID
--   - JOIN optimized with proper indexes
--   - Returns only necessary columns
--
-- Example:
--   -- Get all active sessions
--   EXEC SP_GetActiveSessionsWithDetails
--
--   -- Get sessions for specific user
--   EXEC SP_GetActiveSessionsWithDetails @UtilizatorID = '...'
--
--   -- Get sessions expiring soon
--   EXEC SP_GetActiveSessionsWithDetails @DoarExpiraInCurand = 1
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[SP_GetActiveSessionsWithDetails]
    @UtilizatorID UNIQUEIDENTIFIER = NULL,
    @DoarExpiraInCurand BIT = 0,
    @SortColumn NVARCHAR(50) = 'DataUltimaActivitate',
    @SortDirection NVARCHAR(4) = 'DESC'
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate sort column (whitelist to prevent SQL injection)
    IF @SortColumn NOT IN ('DataCreare', 'DataUltimaActivitate', 'DataExpirare', 'Username', 'AdresaIP')
        SET @SortColumn = 'DataUltimaActivitate';
    
    -- Validate sort direction
    IF @SortDirection NOT IN ('ASC', 'DESC')
        SET @SortDirection = 'DESC';
    
    -- Build dynamic SQL with validated parameters
    DECLARE @SQL NVARCHAR(MAX);
    
    SET @SQL = N'
        SELECT 
            us.SessionID,
            us.UtilizatorID,
            u.Username,
            u.Email,
            u.Rol,
            us.SessionToken,
            us.AdresaIP,
            us.UserAgent,
            us.Dispozitiv,
            us.DataCreare,
            us.DataUltimaActivitate,
            us.DataExpirare,
            us.EsteActiva
        FROM UserSessions us
        INNER JOIN Utilizatori u ON us.UtilizatorID = u.UtilizatorID
        WHERE us.EsteActiva = 1';
    
    -- Add optional filters
    IF @UtilizatorID IS NOT NULL
        SET @SQL = @SQL + N' AND us.UtilizatorID = @UtilizatorID';
    
    IF @DoarExpiraInCurand = 1
        SET @SQL = @SQL + N' AND DATEDIFF(MINUTE, GETDATE(), us.DataExpirare) < 15';
    
    -- Add ORDER BY with validated column and direction
    SET @SQL = @SQL + N' ORDER BY us.' + QUOTENAME(@SortColumn) + N' ' + @SortDirection;
    
    -- Execute dynamic SQL
    EXEC sp_executesql @SQL,
        N'@UtilizatorID UNIQUEIDENTIFIER',
        @UtilizatorID = @UtilizatorID;
END
GO

-- Grant execute permissions
GRANT EXECUTE ON [dbo].[SP_GetActiveSessionsWithDetails] TO PUBLIC;
GO
