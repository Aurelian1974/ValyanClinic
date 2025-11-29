-- =============================================
-- SP pentru crearea unui nou personal medical
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PersonalMedical_Create]
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
        
        DECLARE @NewId UNIQUEIDENTIFIER = NEWID();
        DECLARE @CurrentDate DATETIME2 = GETDATE();
        
        -- Verificare unicitate Email ?i NumarLicenta
        IF @Email IS NOT NULL AND EXISTS (SELECT 1 FROM PersonalMedical WHERE Email = @Email)
        BEGIN
            THROW 50001, 'Email-ul exist? deja in baza de date.', 1;
        END
        
        IF @NumarLicenta IS NOT NULL AND EXISTS (SELECT 1 FROM PersonalMedical WHERE NumarLicenta = @NumarLicenta)
        BEGIN
            THROW 50002, 'Numrul de licen?? exist? deja in baza de date.', 1;
        END
        
        INSERT INTO PersonalMedical (
            PersonalID, Nume, Prenume, Specializare, NumarLicenta,
            Telefon, Email, Departament, Pozitie, EsteActiv, DataCreare,
            CategorieID, SpecializareID, SubspecializareID
        ) VALUES (
            @NewId, @Nume, @Prenume, @Specializare, @NumarLicenta,
            @Telefon, @Email, @Departament, @Pozitie, @EsteActiv, @CurrentDate,
            @CategorieID, @SpecializareID, @SubspecializareID
        );
        
        COMMIT TRANSACTION;
        
        -- Returnare personal medical creat cu lookup-uri
        EXEC sp_PersonalMedical_GetById @NewId;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;