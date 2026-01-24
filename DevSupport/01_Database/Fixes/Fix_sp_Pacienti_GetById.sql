-- =============================================
-- FIX: sp_Pacienti_GetById - Adaugare NumeComplet si Varsta
-- Database: ValyanMed
-- Issue: SP nu returneaza NumeComplet si Varsta (coloane calculate)
-- Fix: Adaugare coloane calculate la SELECT
-- Date: 2026-01-07
-- =============================================

USE [ValyanMed]
GO

PRINT '========================================';
PRINT 'FIX: sp_Pacienti_GetById - NumeComplet, Varsta';
PRINT '========================================';
PRINT '';

-- Drop existing SP if exists
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_GetById')
BEGIN
    DROP PROCEDURE sp_Pacienti_GetById;
    PRINT '✓ Stored Procedure existent sters';
END
GO

-- Create fixed SP
CREATE PROCEDURE [dbo].[sp_Pacienti_GetById]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        p.Id,
        p.Cod_Pacient,
        p.CNP,
        p.Nume,
        p.Prenume,
        -- ✅ Computed columns - CRITICAL pentru afișare în UI
        CONCAT(p.Nume, ' ', p.Prenume) AS NumeComplet,
        dbo.fn_CalculateAgeFromCNP(p.CNP) AS Varsta,  -- ✅ Calculat din CNP pentru consistență
        -- Contact info
        p.Telefon,
        p.Telefon_Secundar,
        p.Email,
        -- Address
        p.Judet,
        p.Localitate,
        p.Adresa,
        p.Cod_Postal,
        CONCAT(ISNULL(p.Adresa, ''), 
               CASE WHEN p.Localitate IS NOT NULL THEN ', ' + p.Localitate ELSE '' END,
               CASE WHEN p.Judet IS NOT NULL THEN ', ' + p.Judet ELSE '' END) AS AdresaCompleta,
        -- Personal info
        p.Data_Nasterii,
        p.Sex,
        -- Insurance
        p.Asigurat,
        p.CNP_Asigurat,
        p.Nr_Card_Sanatate,
        p.Casa_Asigurari,
        -- Medical
        p.Alergii,
        p.Boli_Cronice,
        p.Medic_Familie,
        -- Emergency contact
        p.Persoana_Contact,
        p.Telefon_Urgenta,
        p.Relatie_Contact,
        -- Administrative
        p.Data_Inregistrare,
        p.Ultima_Vizita,
        p.Nr_Total_Vizite,
        p.Activ,
        p.Observatii,
        -- Audit
        p.Data_Crearii,
        p.Data_Ultimei_Modificari,
        p.Creat_De,
        p.Modificat_De
    FROM dbo.Pacienti p
    WHERE p.Id = @Id;
END
GO

PRINT '✓ sp_Pacienti_GetById creat cu NumeComplet si Varsta';
PRINT '';

-- Verificare
PRINT 'Verificare...';
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Pacienti_GetById')
    PRINT '✓ sp_Pacienti_GetById EXISTS';
ELSE
    PRINT '✗ sp_Pacienti_GetById NU EXISTA';

PRINT '';
PRINT '========================================';
PRINT 'FIX COMPLET!';
PRINT 'Reporneste aplicatia pentru a testa.';
PRINT '========================================';
GO
