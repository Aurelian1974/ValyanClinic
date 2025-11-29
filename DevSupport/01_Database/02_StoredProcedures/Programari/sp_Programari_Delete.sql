-- ============================================================================
-- Stored Procedure: sp_Programari_Delete
-- Database: ValyanMed
-- Descriere: Soft delete (anulare) programare
-- Creat: 2025-01-15
-- Versiune: 1.0
-- ============================================================================

USE [ValyanMed]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Programari_Delete')
    DROP PROCEDURE sp_Programari_Delete
GO

CREATE PROCEDURE sp_Programari_Delete
    @ProgramareID UNIQUEIDENTIFIER,
    @ModificatDe UNIQUEIDENTIFIER,
    @MotivAnulare NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
    -- Validare: Programarea exista
        IF NOT EXISTS (SELECT 1 FROM Programari WHERE ProgramareID = @ProgramareID)
     BEGIN
      THROW 50006, 'Programarea specificata nu exista.', 1;
        END
        
        -- Verificare: Programarea nu este deja anulata
 DECLARE @CurrentStatus NVARCHAR(50);
   SELECT @CurrentStatus = Status FROM Programari WHERE ProgramareID = @ProgramareID;
   
      IF @CurrentStatus = 'Anulata'
        BEGIN
THROW 50007, 'Programarea este deja anulata.', 1;
        END
     
        DECLARE @CurrentDate DATETIME2 = GETDATE();
      DECLARE @Observatii NVARCHAR(1000);
        
-- Construieste observatii cu motivul anularii
 SELECT @Observatii = Observatii FROM Programari WHERE ProgramareID = @ProgramareID;
     
     IF @MotivAnulare IS NOT NULL
     BEGIN
    SET @Observatii = ISNULL(@Observatii, '') + 
           CHAR(13) + CHAR(10) + 
      '[ANULAT ' + FORMAT(@CurrentDate, 'dd.MM.yyyy HH:mm') + ']: ' + 
  @MotivAnulare;
   END
        
        -- Soft delete - marcare ca anulata
        UPDATE Programari SET
     Status = 'Anulata',
     Observatii = @Observatii,
            DataUltimeiModificari = @CurrentDate,
      ModificatDe = @ModificatDe
        WHERE ProgramareID = @ProgramareID;
        
    COMMIT TRANSACTION;
        
 SELECT 1 AS Success, 'Programarea a fost anulata cu succes.' AS Message;
        
    END TRY
    BEGIN CATCH
      IF @@TRANCOUNT > 0
       ROLLBACK TRANSACTION;
      THROW;
    END CATCH
END
GO

PRINT '? sp_Programari_Delete creat cu succes';
GO
