-- ========================================
-- Tabel: RoluriSistem
-- Database: ValyanMed
-- Generat: 2025-10-08 16:36:43
-- Coloane: 5
-- Primary Keys: 1
-- Foreign Keys: 0
-- Indexes: 1
-- ========================================

USE [ValyanMed]
GO

-- Drop table if exists
IF OBJECT_ID('dbo.RoluriSistem', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.RoluriSistem
    PRINT 'Tabel RoluriSistem sters.'
END
GO

-- Create table
CREATE TABLE dbo.RoluriSistem (    [RolID] UNIQUEIDENTIFIER  NOT NULL,
    [NumeRol] NVARCHAR(50)  NOT NULL,
    [Descriere] NVARCHAR(200)  NULL,
    [EsteActiv] BIT  NULL,
    [DataCreare] DATETIME2  NULL,
    ,CONSTRAINT [PK_RoluriSistem] PRIMARY KEY ([RolID])
)
GO

-- Indexes
CREATE UNIQUE NONCLUSTERED INDEX [UQ__RoluriSi__F5C1C75DF8764415] 
ON dbo.RoluriSistem (NumeRol ASC)
GO
PRINT 'Tabel RoluriSistem creat cu succes cu 5 coloane.'
