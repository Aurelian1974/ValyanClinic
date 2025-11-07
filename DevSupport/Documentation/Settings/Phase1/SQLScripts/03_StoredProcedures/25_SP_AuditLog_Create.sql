-- ========================================
-- Stored Procedure: SP_AuditLog_Create
-- Description: Creaza o noua inregistrare in audit log
-- ========================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_AuditLog_Create]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[SP_AuditLog_Create]
GO

CREATE PROCEDURE [dbo].[SP_AuditLog_Create]
    @UserName NVARCHAR(255),
    @Actiune NVARCHAR(100),
    @Entitate NVARCHAR(100) = NULL,
    @EntitateID NVARCHAR(100) = NULL,
 @ValoareVeche NVARCHAR(MAX) = NULL,
    @ValoareNoua NVARCHAR(MAX) = NULL,
    @AdresaIP NVARCHAR(50) = NULL,
    @UserAgent NVARCHAR(500) = NULL,
  @Dispozitiv NVARCHAR(255) = NULL,
@StatusActiune NVARCHAR(50) = 'Success',
    @DetaliiEroare NVARCHAR(MAX) = NULL,
    @UtilizatorID UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

DECLARE @AuditID UNIQUEIDENTIFIER = NEWID();

    INSERT INTO Audit_Log (
AuditID, UtilizatorID, UserName, Actiune, DataActiune,
      Entitate, EntitateID, ValoareVeche, ValoareNoua,
AdresaIP, UserAgent, Dispozitiv, StatusActiune, DetaliiEroare
    )
    VALUES (
    @AuditID, @UtilizatorID, @UserName, @Actiune, GETUTCDATE(),
  @Entitate, @EntitateID, @ValoareVeche, @ValoareNoua,
     @AdresaIP, @UserAgent, @Dispozitiv, @StatusActiune, @DetaliiEroare
    );

    -- Returneaza inregistrarea creata
    SELECT 
   AuditID, UtilizatorID, UserName, Actiune, DataActiune,
     Entitate, EntitateID, ValoareVeche, ValoareNoua,
     AdresaIP, UserAgent, Dispozitiv, StatusActiune, DetaliiEroare
    FROM Audit_Log
    WHERE AuditID = @AuditID;
END
GO
