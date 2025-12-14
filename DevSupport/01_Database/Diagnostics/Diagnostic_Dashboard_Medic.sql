-- =============================================================================
-- DIAGNOSTIC: Dashboard Medic - De ce nu apar programari?
-- Executati acest script in SQL Server Management Studio
-- Database: ValyanMed
-- =============================================================================

USE [ValyanMed]
GO

PRINT '=============================================='
PRINT 'DIAGNOSTIC: Programari Dashboard Medic'
PRINT '=============================================='
PRINT ''

-- 1. Toate programarile de ASTAZI
PRINT '1. PROGRAMARI ASTAZI (' + CONVERT(VARCHAR, GETDATE(), 23) + '):'
PRINT '-------------------------------------------'

SELECT 
    p.ProgramareID,
    p.DoctorID,
    p.DataProgramare,
    p.OraInceput,
    p.Status,
    pm.PersonalID as [PersonalMedical.PersonalID],
    pm.Nume + ' ' + pm.Prenume as DoctorName,
    pac.Nume + ' ' + pac.Prenume as PacientName
FROM Programari p
LEFT JOIN PersonalMedical pm ON p.DoctorID = pm.PersonalID
LEFT JOIN Pacienti pac ON p.PacientID = pac.Id
WHERE p.DataProgramare = CAST(GETDATE() AS DATE)
ORDER BY p.OraInceput

PRINT ''
PRINT '2. UTILIZATORI CU PersonalMedicalID:'
PRINT '-------------------------------------------'

SELECT 
    u.UtilizatorID,
    u.Username,
    u.PersonalMedicalID as [Utilizator.PersonalMedicalID],
    pm.PersonalID as [PersonalMedical.PersonalID],
    pm.Nume + ' ' + pm.Prenume as NumeComplet,
    CASE 
        WHEN u.PersonalMedicalID = pm.PersonalID THEN 'MATCH'
        ELSE 'MISMATCH'
    END as Status
FROM Utilizatori u
LEFT JOIN PersonalMedical pm ON u.PersonalMedicalID = pm.PersonalID
WHERE u.EsteActiv = 1

PRINT ''
PRINT '3. VERIFICARE MATCHING - DoctorID din Programari vs PersonalMedicalID din Utilizatori:'
PRINT '-------------------------------------------'

SELECT 
    'Programari cu DoctorID care NU are Utilizator asociat' as Problema,
    COUNT(*) as NumarProgramari
FROM Programari p
WHERE p.DataProgramare = CAST(GETDATE() AS DATE)
    AND NOT EXISTS (
        SELECT 1 FROM Utilizatori u WHERE u.PersonalMedicalID = p.DoctorID AND u.EsteActiv = 1
    )

UNION ALL

SELECT 
    'Programari cu DoctorID care ARE Utilizator asociat' as Problema,
    COUNT(*) as NumarProgramari
FROM Programari p
WHERE p.DataProgramare = CAST(GETDATE() AS DATE)
    AND EXISTS (
        SELECT 1 FROM Utilizatori u WHERE u.PersonalMedicalID = p.DoctorID AND u.EsteActiv = 1
    )

PRINT ''
PRINT '4. DETALII PROBLEMA (daca exista mismatch):'
PRINT '-------------------------------------------'

SELECT 
    p.ProgramareID,
    p.DoctorID as [Programare.DoctorID],
    pm.PersonalID as [PersonalMedical.PersonalID],
    pm.Nume + ' ' + pm.Prenume as DoctorNumeComplet,
    u.Username as [Utilizator.Username],
    u.PersonalMedicalID as [Utilizator.PersonalMedicalID],
    CASE 
        WHEN p.DoctorID = u.PersonalMedicalID THEN 'OK - Match'
        WHEN u.PersonalMedicalID IS NULL THEN 'EROARE: Utilizator nu are PersonalMedicalID'
        ELSE 'EROARE: DoctorID <> PersonalMedicalID'
    END as DiagnosticResult
FROM Programari p
INNER JOIN PersonalMedical pm ON p.DoctorID = pm.PersonalID
LEFT JOIN Utilizatori u ON pm.PersonalID = u.PersonalMedicalID
WHERE p.DataProgramare = CAST(GETDATE() AS DATE)

PRINT ''
PRINT '=============================================='
PRINT 'CONCLUZIE:'
PRINT '=============================================='
PRINT ''
PRINT 'Daca vezi "EROARE" in coloana DiagnosticResult,'
PRINT 'inseamna ca utilizatorul logat are un PersonalMedicalID'
PRINT 'diferit de DoctorID din programari.'
PRINT ''
PRINT 'SOLUTIE: Verifica si actualizeaza:'
PRINT '1. Utilizatori.PersonalMedicalID = PersonalMedical.PersonalID al medicului'
PRINT '2. Programari.DoctorID = PersonalMedical.PersonalID al medicului'
PRINT ''

GO
