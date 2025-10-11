-- ========================================
-- Stored Procedure: sp_PersonalMedical_Delete
-- Database: ValyanMed
-- Created: 09/22/2025 20:34:29
-- Modified: 09/22/2025 20:34:29
-- Generat: 2025-10-08 16:36:44
-- ========================================

USE [ValyanMed]
GO

-- =============================================
-- SP pentru ?tergerea unui personal medical (soft delete)
-- =============================================
CREATE   PROCEDURE [dbo].[sp_PersonalMedical_Delete]
    @PersonalID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificare existen?a
        IF NOT EXISTS (SELECT 1 FROM PersonalMedical WHERE PersonalID = @PersonalID)
        BEGIN
            THROW 50003, 'Personalul medical nu a fost g?sit.', 1;
        END
        
        -- Soft delete - setare EsteActiv pe false
        UPDATE PersonalMedical SET
            EsteActiv = 0
        WHERE PersonalID = @PersonalID;
        
        COMMIT TRANSACTION;
        
        SELECT 1 as Success;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
