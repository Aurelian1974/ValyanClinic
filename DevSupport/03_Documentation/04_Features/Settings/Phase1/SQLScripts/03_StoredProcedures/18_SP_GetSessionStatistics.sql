-- =============================================
-- Author:      DevSupport Team
-- Create date: 2025-11-30
-- Description: Gets user session statistics for dashboard and monitoring
-- 
-- Purpose:
--   Retrieves real-time session statistics:
--   - Total active sessions
--   - Sessions expiring soon (within 15 minutes)
--   - Sessions that became inactive today
--
-- Returns:
--   Single row with three columns:
--   - TotalActive: COUNT of active sessions (EsteActiva = 1)
--   - ExpiraInCurand: COUNT of sessions expiring within 15 minutes
--   - InactiviAzi: COUNT of sessions that expired today
--
-- Use Cases:
--   - Admin dashboard: Display session statistics
--   - Monitoring: Alert if TotalActive unusually high (potential attack)
--   - Capacity planning: Track usage patterns
--   - Health checks: Verify system activity
--
-- Performance:
--   - Three separate COUNT queries (subqueries)
--   - Indexed on EsteActiva, DataExpirare, DataUltimaActivitate
--   - Fast execution (~5-10ms on typical data)
--   - Consider caching result for 1-5 minutes
--
-- Security:
--   - No parameters (safe by design)
--   - Read-only operation
--   - No sensitive data exposed
--
-- Example:
--   EXEC SP_GetSessionStatistics;
--
--   Result:
--   TotalActive | ExpiraInCurand | InactiviAzi
--   ------------|----------------|------------
--          145 |             23 |          67
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[SP_GetSessionStatistics]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        -- Total active sessions (EsteActiva = 1)
        (SELECT COUNT(*) 
         FROM UserSessions 
         WHERE EsteActiva = 1) AS TotalActive,
        
        -- Sessions expiring soon (within 15 minutes)
        (SELECT COUNT(*) 
         FROM UserSessions 
         WHERE EsteActiva = 1 
         AND DATEDIFF(MINUTE, GETDATE(), DataExpirare) < 15) AS ExpiraInCurand,
        
        -- Sessions that became inactive today
        (SELECT COUNT(*) 
         FROM UserSessions 
         WHERE EsteActiva = 0 
         AND CAST(DataUltimaActivitate AS DATE) = CAST(GETDATE() AS DATE)) AS InactiviAzi;
END
GO

-- Grant execute permissions
GRANT EXECUTE ON [dbo].[SP_GetSessionStatistics] TO PUBLIC;
GO
