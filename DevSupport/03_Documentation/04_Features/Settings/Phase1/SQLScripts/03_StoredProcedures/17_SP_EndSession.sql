-- =============================================
-- Author:      DevSupport Team
-- Create date: 2025-11-30
-- Description: Ends a user session (forced logout)
-- 
-- Purpose:
--   Closes an active session by setting EsteActiva = 0
--   Updates last activity timestamp for audit trail
--   Does NOT delete session (preserves history)
--
-- Parameters:
--   @SessionID - Unique session identifier (GUID)
--
-- Returns:
--   Number of rows affected (1 if success, 0 if session not found)
--
-- Use Cases:
--   - User logout: Close current session
--   - Admin action: Force logout user from specific session
--   - Security: Close suspicious session
--
-- Security:
--   - Parameterized query prevents SQL injection
--   - Updates only specified session
--   - Preserves audit trail (no DELETE)
--
-- Performance:
--   - Indexed on SessionID (primary key)
--   - Single UPDATE statement
--   - Fast execution (~1ms)
--
-- Example:
--   DECLARE @SessionID UNIQUEIDENTIFIER = '...';
--   EXEC SP_EndSession @SessionID = @SessionID;
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[SP_EndSession]
    @SessionID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate input
    IF @SessionID IS NULL
    BEGIN
        RAISERROR('SessionID cannot be NULL', 16, 1);
        RETURN;
    END
    
    -- Update session status
    UPDATE UserSessions 
    SET 
        EsteActiva = 0,
        DataUltimaActivitate = GETDATE()
    WHERE SessionID = @SessionID;
    
    -- Return number of rows affected
    -- Calling code can check if session was found (@@ROWCOUNT = 1)
    RETURN @@ROWCOUNT;
END
GO

-- Grant execute permissions
GRANT EXECUTE ON [dbo].[SP_EndSession] TO PUBLIC;
GO
