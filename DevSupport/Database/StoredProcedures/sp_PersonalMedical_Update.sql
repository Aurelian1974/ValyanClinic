-- ========================================
-- Stored Procedure: sp_PersonalMedical_Update
-- Database: ValyanMed
-- Created: 09/22/2025 20:34:16
-- Modified: 09/22/2025 20:34:16
-- Generat: 2025-10-18 08:40:46
-- ========================================

USE [ValyanMed]
GO

-- =============================================
-- SP pentru actualizarea unui personal medical
-- =============================================
CREATE   PROCEDURE [dbo].[sp_PersonalMedical_Update]
    @PersonalID UNIQUEIDENTIFIER,
    @Nume NVARCHAR(100),
    @Prenume NVARCHAR(100),
    @Specializare NVARCHAR(100) = NULL,
    @NumarLicenta NVARCHAR(50) = NULL,
    @Telefon NVARCHAR(20) = NULL,
    @Email NVARCHAR(100) = NULL,
    @Departament NVARCHAR(100) = NULL,
    @Pozitie NVARCHAR(50) = NULL,
    @EsteActiv BIT = 1,
    @CategorieID UNIQUEIDENTIFIER = NULL,
    @SpecializareID UNIQUEIDENTIFIER = NULL,
    @SubspecializareID UNIQUEIDENTIFIER = NULL
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
        
        -- Verificare unicitate Email ?i NumarLicenta (exclude ID-ul curent)
        IF @Email IS NOT NULL AND EXISTS (SELECT 1 FROM PersonalMedical WHERE Email = @Email AND PersonalID != @PersonalID)
        BEGIN
            THROW 50001, 'Email-ul exist? deja în baza de date.', 1;
        END
        
        IF @NumarLicenta IS NOT NULL AND EXISTS (SELECT 1 FROM PersonalMedical WHERE NumarLicenta = @NumarLicenta AND PersonalID != @PersonalID)
        BEGIN
            THROW 50002, 'Num?rul de licen?? exist? deja în baza de date.', 1;
        END
        
        UPDATE PersonalMedical SET
            Nume = @Nume,
            Prenume = @Prenume,
            Specializare = @Specializare,
            NumarLicenta = @NumarLicenta,
            Telefon = @Telefon,
            Email = @Email,
            Departament = @Departament,
            Pozitie = @Pozitie,
            EsteActiv = @EsteActiv,
            CategorieID = @CategorieID,
            SpecializareID = @SpecializareID,
            SubspecializareID = @SubspecializareID
        WHERE PersonalID = @PersonalID;
        
        COMMIT TRANSACTION;
        
        -- Returnare personal medical actualizat cu lookup-uri
        EXEC sp_PersonalMedical_GetById @PersonalID;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
