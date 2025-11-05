-- =============================================
-- Stored Procedure: sp_PacientiPersonalMedical_ActivateRelatie
-- Database: ValyanMed
-- Descriere: Reactiveaz? o rela?ie inactiv? dintre un pacient ?i un doctor
-- =============================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_PacientiPersonalMedical_ActivateRelatie')
  DROP PROCEDURE sp_PacientiPersonalMedical_ActivateRelatie
GO

CREATE PROCEDURE sp_PacientiPersonalMedical_ActivateRelatie
    @RelatieID UNIQUEIDENTIFIER,
    @Observatii NVARCHAR(MAX) = NULL,
    @Motiv NVARCHAR(500) = NULL,
    @ModificatDe NVARCHAR(100) = NULL
AS
BEGIN
  SET NOCOUNT ON;
 BEGIN TRY
        BEGIN TRANSACTION;
     
        -- Verificare existen?? rela?ie
        IF NOT EXISTS (SELECT 1 FROM Pacienti_PersonalMedical WHERE Id = @RelatieID)
        BEGIN
  THROW 50001, 'Rela?ia specificat? nu a fost g?sit?.', 1;
        END
        
        -- Verificare c? rela?ia este inactiv?
        IF EXISTS (SELECT 1 FROM Pacienti_PersonalMedical WHERE Id = @RelatieID AND EsteActiv = 1)
        BEGIN
      THROW 50002, 'Rela?ia este deja activ?.', 1;
        END
        
 -- Reactivare rela?ie
   UPDATE Pacienti_PersonalMedical
        SET 
        EsteActiv = 1,
    DataDezactivarii = NULL,  -- Resetare data dezactiv?rii
       Observatii = CASE 
      WHEN @Observatii IS NOT NULL 
   THEN @Observatii 
      ELSE Observatii 
            END,
        Motiv = CASE 
          WHEN @Motiv IS NOT NULL 
        THEN @Motiv 
   ELSE Motiv 
       END,
        Data_Ultimei_Modificari = GETDATE(),
        Modificat_De = ISNULL(@ModificatDe, SYSTEM_USER)
        WHERE Id = @RelatieID;
        
        COMMIT TRANSACTION;
        
        -- Returnare succes
      SELECT 
            1 AS Success,
            'Rela?ie reactivat? cu succes.' AS Message;
        
    END TRY
    BEGIN CATCH
 IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Test Script (optional - decomenteaz? pentru testare)
-- =============================================

/*
-- Test 1: Reactiveaz? o rela?ie inactiv?
DECLARE @TestRelatieID UNIQUEIDENTIFIER = 'GUID_EXISTENT_INACTIV_AICI';

EXEC sp_PacientiPersonalMedical_ActivateRelatie
    @RelatieID = @TestRelatieID,
    @Observatii = 'Rela?ie reactivat? la cererea pacientului',
    @Motiv = 'Pacientul revine pentru continuarea tratamentului',
    @ModificatDe = 'System';

-- Verificare rezultat
SELECT * FROM Pacienti_PersonalMedical WHERE Id = @TestRelatieID;
*/

PRINT 'Stored Procedure sp_PacientiPersonalMedical_ActivateRelatie created successfully!';
GO
