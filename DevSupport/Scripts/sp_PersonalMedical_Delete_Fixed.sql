-- =============================================
-- SP pentru ?tergerea unui personal medical (soft delete)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PersonalMedical_Delete]
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