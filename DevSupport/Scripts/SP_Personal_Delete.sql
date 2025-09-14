-- SP pentru stergerea unei persoane (soft delete)
CREATE PROCEDURE [dbo].[sp_Personal_Delete]
    @Id_Personal UNIQUEIDENTIFIER,
    @Modificat_De NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificare existenta
        IF NOT EXISTS (SELECT 1 FROM Personal WHERE Id_Personal = @Id_Personal)
        BEGIN
            THROW 50003, 'Persoana nu a fost gasita.', 1;
        END
        
        -- Soft delete - setare status Inactiv
        UPDATE Personal SET
            Status_Angajat = 'Inactiv',
            Data_Ultimei_Modificari = GETUTCDATE(),
            Modificat_De = @Modificat_De,
            Observatii = ISNULL(Observatii + ' | ', '') + 'Dezactivat pe ' + CONVERT(NVARCHAR, GETUTCDATE(), 120) + ' de ' + @Modificat_De
        WHERE Id_Personal = @Id_Personal;
        
        COMMIT TRANSACTION;
        
        SELECT 1 as Success;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;