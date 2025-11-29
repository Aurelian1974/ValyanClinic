-- =============================================
-- Autor: ValyanClinic Development Team
-- Data: 2025-01-15
-- Versiune: 1.0
-- Descriere: Tabel audit log pentru tracking acțiuni
-- Dependențe: Nicio (poate funcționa independent)
-- =============================================

PRINT '=== CREARE TABEL Audit_Log ===';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Audit_Log]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Audit_Log] (
        [AuditID] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [UtilizatorID] UNIQUEIDENTIFIER NULL,
        [UserName] NVARCHAR(256) NULL,
        [Actiune] NVARCHAR(200) NOT NULL,
        [Entitate] NVARCHAR(200) NULL,
        [EntitateID] NVARCHAR(100) NULL,
        [ValoareVeche] NVARCHAR(MAX) NULL,
        [ValoareNoua] NVARCHAR(MAX) NULL,
        [AdresaIP] NVARCHAR(50) NULL,
        [UserAgent] NVARCHAR(500) NULL,
        [Dispozitiv] NVARCHAR(200) NULL,
        [StatusActiune] NVARCHAR(50) NULL,
        [DetaliiEroare] NVARCHAR(MAX) NULL,
        [DataActiune] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
        
 CONSTRAINT [PK_Audit_Log] PRIMARY KEY CLUSTERED ([AuditID] ASC)
 );
    
    -- Indexuri pentru performanță
    CREATE NONCLUSTERED INDEX [IX_AuditLog_UtilizatorID] 
  ON [dbo].[Audit_Log] ([UtilizatorID] ASC);
  
    CREATE NONCLUSTERED INDEX [IX_AuditLog_DataActiune] 
   ON [dbo].[Audit_Log] ([DataActiune] DESC);
    
    CREATE NONCLUSTERED INDEX [IX_AuditLog_Actiune] 
    ON [dbo].[Audit_Log] ([Actiune] ASC);
    
  CREATE NONCLUSTERED INDEX [IX_AuditLog_StatusActiune] 
    ON [dbo].[Audit_Log] ([StatusActiune] ASC);
    
    PRINT '✓ Tabel Audit_Log creat cu succes.';
    PRINT '✓ Primary Key: PK_Audit_Log';
    PRINT '✓ 4 indexuri create pentru performanță.';
    PRINT '! NOTĂ: Pentru volume mari (>10M înregistrări) consideră table partitioning pe DataActiune.';
END
ELSE
BEGIN
    PRINT '! Tabelul Audit_Log există deja. Skip creation.';
END
GO

PRINT '=== VERIFICARE FINALĂ ===';
PRINT '✓ Tabelul Audit_Log este gata de utilizare.';
GO
