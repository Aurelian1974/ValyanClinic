-- ============================================
-- Script: Adaugare Permisiuni Granulare la Nivel de Camp
-- Data: 2025-12-25
-- Descriere: Adauga permisiuni granulare pentru controlul accesului la nivel de input
-- ============================================

USE ValyanMed;
GO

SET NOCOUNT ON;

PRINT '=== Adaugare Permisiuni Granulare la Nivel de Camp ==='
PRINT ''

-- =============================================
-- 1. PACIENT - Permisiuni granulare View
-- =============================================
PRINT 'Adaugare permisiuni Pacient.View.* ...'

INSERT INTO dbo.PermisiuniDefinitii (PermisiuneDefinitieID, Cod, Denumire, Descriere, Categorie, Este_Activ, Ordine_Afisare)
SELECT NEWID(), v.Cod, v.Denumire, v.Descriere, v.Categorie, 1, v.Ordine_Afisare
FROM (VALUES
    ('Pacient.View.CNP', 'Vizualizare CNP', 'Permite vizualizarea CNP-ului pacientului', 'Pacient.View', 110),
    ('Pacient.View.DataNasterii', 'Vizualizare Data Nasterii', 'Permite vizualizarea datei nasterii', 'Pacient.View', 111),
    ('Pacient.View.Sex', 'Vizualizare Sex', 'Permite vizualizarea sexului pacientului', 'Pacient.View', 112),
    ('Pacient.View.Telefon', 'Vizualizare Telefon', 'Permite vizualizarea numarului de telefon', 'Pacient.View', 113),
    ('Pacient.View.Email', 'Vizualizare Email', 'Permite vizualizarea adresei de email', 'Pacient.View', 114),
    ('Pacient.View.ContactUrgenta', 'Vizualizare Contact Urgenta', 'Permite vizualizarea contactului de urgenta', 'Pacient.View', 115),
    ('Pacient.View.Adresa', 'Vizualizare Adresa', 'Permite vizualizarea adresei complete', 'Pacient.View', 116),
    ('Pacient.View.Alergii', 'Vizualizare Alergii', 'Permite vizualizarea alergiilor (date sensibile)', 'Pacient.View', 117),
    ('Pacient.View.BoliCronice', 'Vizualizare Boli Cronice', 'Permite vizualizarea bolilor cronice (date sensibile)', 'Pacient.View', 118),
    ('Pacient.View.GrupaSanguina', 'Vizualizare Grupa Sanguina', 'Permite vizualizarea grupei sanguine', 'Pacient.View', 119),
    ('Pacient.View.Greutate', 'Vizualizare Greutate', 'Permite vizualizarea greutatii', 'Pacient.View', 120),
    ('Pacient.View.Inaltime', 'Vizualizare Inaltime', 'Permite vizualizarea inaltimii', 'Pacient.View', 121),
    ('Pacient.View.Observatii', 'Vizualizare Observatii', 'Permite vizualizarea observatiilor medicale', 'Pacient.View', 122),
    ('Pacient.View.Asigurare', 'Vizualizare Asigurare', 'Permite vizualizarea datelor de asigurare', 'Pacient.View', 123),
    ('Pacient.View.CAS', 'Vizualizare CAS', 'Permite vizualizarea datelor CAS', 'Pacient.View', 124)
) AS v(Cod, Denumire, Descriere, Categorie, Ordine_Afisare)
WHERE NOT EXISTS (SELECT 1 FROM dbo.PermisiuniDefinitii WHERE Cod = v.Cod);

-- =============================================
-- 2. PACIENT - Permisiuni granulare Edit
-- =============================================
PRINT 'Adaugare permisiuni Pacient.Edit.* ...'

INSERT INTO dbo.PermisiuniDefinitii (PermisiuneDefinitieID, Cod, Denumire, Descriere, Categorie, Este_Activ, Ordine_Afisare)
SELECT NEWID(), v.Cod, v.Denumire, v.Descriere, v.Categorie, 1, v.Ordine_Afisare
FROM (VALUES
    ('Pacient.Edit.CNP', 'Editare CNP', 'Permite modificarea CNP-ului pacientului', 'Pacient.Edit', 130),
    ('Pacient.Edit.Nume', 'Editare Nume', 'Permite modificarea numelui pacientului', 'Pacient.Edit', 131),
    ('Pacient.Edit.Prenume', 'Editare Prenume', 'Permite modificarea prenumelui pacientului', 'Pacient.Edit', 132),
    ('Pacient.Edit.DataNasterii', 'Editare Data Nasterii', 'Permite modificarea datei nasterii', 'Pacient.Edit', 133),
    ('Pacient.Edit.Sex', 'Editare Sex', 'Permite modificarea sexului pacientului', 'Pacient.Edit', 134),
    ('Pacient.Edit.Activ', 'Editare Status Activ', 'Permite activarea/dezactivarea pacientului', 'Pacient.Edit', 135),
    ('Pacient.Edit.Telefon', 'Editare Telefon', 'Permite modificarea telefonului principal', 'Pacient.Edit', 136),
    ('Pacient.Edit.TelefonSecundar', 'Editare Telefon Secundar', 'Permite modificarea telefonului secundar', 'Pacient.Edit', 137),
    ('Pacient.Edit.Email', 'Editare Email', 'Permite modificarea adresei de email', 'Pacient.Edit', 138),
    ('Pacient.Edit.PersoanaContact', 'Editare Persoana Contact', 'Permite modificarea persoanei de contact', 'Pacient.Edit', 139),
    ('Pacient.Edit.RelatieContact', 'Editare Relatie Contact', 'Permite modificarea relatiei cu persoana de contact', 'Pacient.Edit', 140),
    ('Pacient.Edit.TelefonUrgenta', 'Editare Telefon Urgenta', 'Permite modificarea telefonului de urgenta', 'Pacient.Edit', 141),
    ('Pacient.Edit.Adresa', 'Editare Adresa', 'Permite modificarea adresei', 'Pacient.Edit', 142),
    ('Pacient.Edit.Localitate', 'Editare Localitate', 'Permite modificarea localitatii', 'Pacient.Edit', 143),
    ('Pacient.Edit.Judet', 'Editare Judet', 'Permite modificarea judetului', 'Pacient.Edit', 144),
    ('Pacient.Edit.CodPostal', 'Editare Cod Postal', 'Permite modificarea codului postal', 'Pacient.Edit', 145),
    ('Pacient.Edit.Alergii', 'Editare Alergii', 'Permite modificarea alergiilor (date medicale)', 'Pacient.Edit', 146),
    ('Pacient.Edit.BoliCronice', 'Editare Boli Cronice', 'Permite modificarea bolilor cronice (date medicale)', 'Pacient.Edit', 147),
    ('Pacient.Edit.GrupaSanguina', 'Editare Grupa Sanguina', 'Permite modificarea grupei sanguine', 'Pacient.Edit', 148),
    ('Pacient.Edit.Greutate', 'Editare Greutate', 'Permite modificarea greutatii', 'Pacient.Edit', 149),
    ('Pacient.Edit.Inaltime', 'Editare Inaltime', 'Permite modificarea inaltimii', 'Pacient.Edit', 150),
    ('Pacient.Edit.Observatii', 'Editare Observatii', 'Permite modificarea observatiilor medicale', 'Pacient.Edit', 151),
    ('Pacient.Edit.TipAsigurare', 'Editare Tip Asigurare', 'Permite modificarea tipului de asigurare', 'Pacient.Edit', 152),
    ('Pacient.Edit.NumarAsigurare', 'Editare Numar Asigurare', 'Permite modificarea numarului de asigurare', 'Pacient.Edit', 153),
    ('Pacient.Edit.CasJudetean', 'Editare CAS Judetean', 'Permite modificarea CAS-ului judetean', 'Pacient.Edit', 154),
    ('Pacient.Edit.DataExpirareAsigurare', 'Editare Data Expirare Asigurare', 'Permite modificarea datei de expirare a asigurarii', 'Pacient.Edit', 155)
) AS v(Cod, Denumire, Descriere, Categorie, Ordine_Afisare)
WHERE NOT EXISTS (SELECT 1 FROM dbo.PermisiuniDefinitii WHERE Cod = v.Cod);

-- =============================================
-- 3. CONSULTATIE - Permisiuni granulare Edit
-- =============================================
PRINT 'Adaugare permisiuni Consultatie.Edit.* ...'

INSERT INTO dbo.PermisiuniDefinitii (PermisiuneDefinitieID, Cod, Denumire, Descriere, Categorie, Este_Activ, Ordine_Afisare)
SELECT NEWID(), v.Cod, v.Denumire, v.Descriere, v.Categorie, 1, v.Ordine_Afisare
FROM (VALUES
    ('Consultatie.Edit.Diagnostic', 'Editare Diagnostic', 'Permite modificarea diagnosticului', 'Consultatie.Edit', 210),
    ('Consultatie.Edit.Simptome', 'Editare Simptome', 'Permite modificarea simptomelor', 'Consultatie.Edit', 211),
    ('Consultatie.Edit.ExamenClinic', 'Editare Examen Clinic', 'Permite modificarea examenului clinic', 'Consultatie.Edit', 212),
    ('Consultatie.Edit.Tratament', 'Editare Tratament', 'Permite modificarea tratamentului', 'Consultatie.Edit', 213),
    ('Consultatie.Edit.Recomandari', 'Editare Recomandari', 'Permite modificarea recomandarilor', 'Consultatie.Edit', 214),
    ('Consultatie.Edit.Reteta', 'Editare Reteta', 'Permite modificarea retetei medicale', 'Consultatie.Edit', 215),
    ('Consultatie.Edit.Investigatii', 'Editare Investigatii', 'Permite modificarea investigatiilor', 'Consultatie.Edit', 216),
    ('Consultatie.Edit.DataControl', 'Editare Data Control', 'Permite modificarea datei de control', 'Consultatie.Edit', 217),
    ('Consultatie.Edit.TipConsultatie', 'Editare Tip Consultatie', 'Permite modificarea tipului de consultatie', 'Consultatie.Edit', 218)
) AS v(Cod, Denumire, Descriere, Categorie, Ordine_Afisare)
WHERE NOT EXISTS (SELECT 1 FROM dbo.PermisiuniDefinitii WHERE Cod = v.Cod);

-- =============================================
-- 4. PROGRAMARE - Permisiuni granulare Edit
-- =============================================
PRINT 'Adaugare permisiuni Programare.Edit.* ...'

INSERT INTO dbo.PermisiuniDefinitii (PermisiuneDefinitieID, Cod, Denumire, Descriere, Categorie, Este_Activ, Ordine_Afisare)
SELECT NEWID(), v.Cod, v.Denumire, v.Descriere, v.Categorie, 1, v.Ordine_Afisare
FROM (VALUES
    ('Programare.Edit.Data', 'Editare Data', 'Permite modificarea datei programarii', 'Programare.Edit', 310),
    ('Programare.Edit.Ora', 'Editare Ora', 'Permite modificarea orei programarii', 'Programare.Edit', 311),
    ('Programare.Edit.Durata', 'Editare Durata', 'Permite modificarea duratei programarii', 'Programare.Edit', 312),
    ('Programare.Edit.Doctor', 'Editare Doctor', 'Permite modificarea doctorului', 'Programare.Edit', 313),
    ('Programare.Edit.TipProgramare', 'Editare Tip Programare', 'Permite modificarea tipului de programare', 'Programare.Edit', 314),
    ('Programare.Edit.Motiv', 'Editare Motiv', 'Permite modificarea motivului programarii', 'Programare.Edit', 315),
    ('Programare.Edit.Observatii', 'Editare Observatii', 'Permite modificarea observatiilor', 'Programare.Edit', 316),
    ('Programare.Edit.Status', 'Editare Status', 'Permite modificarea statusului programarii', 'Programare.Edit', 317)
) AS v(Cod, Denumire, Descriere, Categorie, Ordine_Afisare)
WHERE NOT EXISTS (SELECT 1 FROM dbo.PermisiuniDefinitii WHERE Cod = v.Cod);

-- =============================================
-- 5. PERSONAL - Permisiuni granulare Edit
-- =============================================
PRINT 'Adaugare permisiuni Personal.Edit.* ...'

INSERT INTO dbo.PermisiuniDefinitii (PermisiuneDefinitieID, Cod, Denumire, Descriere, Categorie, Este_Activ, Ordine_Afisare)
SELECT NEWID(), v.Cod, v.Denumire, v.Descriere, v.Categorie, 1, v.Ordine_Afisare
FROM (VALUES
    ('Personal.Edit.CNP', 'Editare CNP Personal', 'Permite modificarea CNP-ului personalului', 'Personal.Edit', 410),
    ('Personal.Edit.Nume', 'Editare Nume Personal', 'Permite modificarea numelui personalului', 'Personal.Edit', 411),
    ('Personal.Edit.Prenume', 'Editare Prenume Personal', 'Permite modificarea prenumelui personalului', 'Personal.Edit', 412),
    ('Personal.Edit.Email', 'Editare Email Personal', 'Permite modificarea emailului personalului', 'Personal.Edit', 413),
    ('Personal.Edit.Telefon', 'Editare Telefon Personal', 'Permite modificarea telefonului personalului', 'Personal.Edit', 414),
    ('Personal.Edit.Specializare', 'Editare Specializare', 'Permite modificarea specializarii', 'Personal.Edit', 415),
    ('Personal.Edit.Departament', 'Editare Departament', 'Permite modificarea departamentului', 'Personal.Edit', 416),
    ('Personal.Edit.CodParafa', 'Editare Cod Parafa', 'Permite modificarea codului de parafa', 'Personal.Edit', 417),
    ('Personal.Edit.Program', 'Editare Program', 'Permite modificarea programului de lucru', 'Personal.Edit', 418),
    ('Personal.Edit.Salariu', 'Editare Salariu', 'Permite modificarea salariului (date sensibile)', 'Personal.Edit', 419)
) AS v(Cod, Denumire, Descriere, Categorie, Ordine_Afisare)
WHERE NOT EXISTS (SELECT 1 FROM dbo.PermisiuniDefinitii WHERE Cod = v.Cod);

-- =============================================
-- 6. ADMIN.ROLURI - Permisiuni pentru administrare roluri
-- =============================================
PRINT 'Adaugare permisiuni Admin.Roluri.* ...'

INSERT INTO dbo.PermisiuniDefinitii (PermisiuneDefinitieID, Cod, Denumire, Descriere, Categorie, Este_Activ, Ordine_Afisare)
SELECT NEWID(), v.Cod, v.Denumire, v.Descriere, v.Categorie, 1, v.Ordine_Afisare
FROM (VALUES
    ('Admin.Roluri.View', 'Vizualizare Roluri', 'Permite vizualizarea listei de roluri', 'Admin.Roluri', 510),
    ('Admin.Roluri.Create', 'Creare Rol', 'Permite crearea unui rol nou', 'Admin.Roluri', 511),
    ('Admin.Roluri.Edit', 'Editare Rol', 'Permite modificarea unui rol existent', 'Admin.Roluri', 512),
    ('Admin.Roluri.Delete', 'Stergere Rol', 'Permite stergerea unui rol', 'Admin.Roluri', 513),
    ('Admin.Roluri.AssignPermissions', 'Atribuire Permisiuni', 'Permite atribuirea permisiunilor la un rol', 'Admin.Roluri', 514)
) AS v(Cod, Denumire, Descriere, Categorie, Ordine_Afisare)
WHERE NOT EXISTS (SELECT 1 FROM dbo.PermisiuniDefinitii WHERE Cod = v.Cod);

-- =============================================
-- 7. Atribuire permisiuni la rolul Admin (TOATE)
-- =============================================
PRINT 'Atribuire permisiuni granulare la rolul Admin (TOATE) ...'

INSERT INTO dbo.RoluriPermisiuni (RolPermisiuneID, RolID, Permisiune, Este_Acordat, Data_Crearii, Creat_De)
SELECT NEWID(), r.RolID, pd.Cod, 1, GETDATE(), 'System'
FROM dbo.Roluri r
CROSS JOIN dbo.PermisiuniDefinitii pd
WHERE r.Denumire = 'Admin'
  AND pd.Este_Activ = 1
  AND NOT EXISTS (
      SELECT 1 FROM dbo.RoluriPermisiuni rp 
      WHERE rp.RolID = r.RolID AND rp.Permisiune = pd.Cod
  );

-- =============================================
-- 8. Atribuire permisiuni la rolul Doctor
-- =============================================
PRINT 'Atribuire permisiuni granulare la rolul Doctor ...'

INSERT INTO dbo.RoluriPermisiuni (RolPermisiuneID, RolID, Permisiune, Este_Acordat, Data_Crearii, Creat_De)
SELECT NEWID(), r.RolID, v.Permisiune, 1, GETDATE(), 'System'
FROM dbo.Roluri r
CROSS JOIN (VALUES
    -- Vizualizare toate campurile pacient
    ('Pacient.View.CNP'), ('Pacient.View.DataNasterii'), ('Pacient.View.Sex'),
    ('Pacient.View.Telefon'), ('Pacient.View.Email'), ('Pacient.View.ContactUrgenta'),
    ('Pacient.View.Adresa'), ('Pacient.View.Alergii'), ('Pacient.View.BoliCronice'),
    ('Pacient.View.GrupaSanguina'), ('Pacient.View.Greutate'), ('Pacient.View.Inaltime'),
    ('Pacient.View.Observatii'), ('Pacient.View.Asigurare'), ('Pacient.View.CAS'),
    -- Editare date medicale (nu CNP, nu date personale de baza)
    ('Pacient.Edit.Alergii'), ('Pacient.Edit.BoliCronice'), ('Pacient.Edit.GrupaSanguina'),
    ('Pacient.Edit.Greutate'), ('Pacient.Edit.Inaltime'), ('Pacient.Edit.Observatii'),
    -- Toate permisiunile de consultatie
    ('Consultatie.Edit.Diagnostic'), ('Consultatie.Edit.Simptome'), ('Consultatie.Edit.ExamenClinic'),
    ('Consultatie.Edit.Tratament'), ('Consultatie.Edit.Recomandari'), ('Consultatie.Edit.Reteta'),
    ('Consultatie.Edit.Investigatii'), ('Consultatie.Edit.DataControl'), ('Consultatie.Edit.TipConsultatie'),
    -- Programari
    ('Programare.Edit.Observatii'), ('Programare.Edit.Status')
) AS v(Permisiune)
WHERE r.Denumire = 'Doctor'
  AND NOT EXISTS (
      SELECT 1 FROM dbo.RoluriPermisiuni rp 
      WHERE rp.RolID = r.RolID AND rp.Permisiune = v.Permisiune
  );

-- =============================================
-- 9. Atribuire permisiuni la rolul Asistent
-- =============================================
PRINT 'Atribuire permisiuni granulare la rolul Asistent ...'

INSERT INTO dbo.RoluriPermisiuni (RolPermisiuneID, RolID, Permisiune, Este_Acordat, Data_Crearii, Creat_De)
SELECT NEWID(), r.RolID, v.Permisiune, 1, GETDATE(), 'System'
FROM dbo.Roluri r
CROSS JOIN (VALUES
    -- Vizualizare limitata (fara CNP, fara date financiare)
    ('Pacient.View.DataNasterii'), ('Pacient.View.Sex'),
    ('Pacient.View.Telefon'), ('Pacient.View.Email'), ('Pacient.View.ContactUrgenta'),
    ('Pacient.View.Alergii'), ('Pacient.View.BoliCronice'), ('Pacient.View.GrupaSanguina'),
    -- Editare contact urgenta doar
    ('Pacient.Edit.PersoanaContact'), ('Pacient.Edit.RelatieContact'), ('Pacient.Edit.TelefonUrgenta'),
    -- Programari
    ('Programare.Edit.Data'), ('Programare.Edit.Ora'), ('Programare.Edit.Observatii'), ('Programare.Edit.Status')
) AS v(Permisiune)
WHERE r.Denumire = 'Asistent'
  AND NOT EXISTS (
      SELECT 1 FROM dbo.RoluriPermisiuni rp 
      WHERE rp.RolID = r.RolID AND rp.Permisiune = v.Permisiune
  );

-- =============================================
-- 10. Atribuire permisiuni la rolul Receptioner
-- =============================================
PRINT 'Atribuire permisiuni granulare la rolul Receptioner ...'

INSERT INTO dbo.RoluriPermisiuni (RolPermisiuneID, RolID, Permisiune, Este_Acordat, Data_Crearii, Creat_De)
SELECT NEWID(), r.RolID, v.Permisiune, 1, GETDATE(), 'System'
FROM dbo.Roluri r
CROSS JOIN (VALUES
    -- Vizualizare date de contact
    ('Pacient.View.Telefon'), ('Pacient.View.Email'), ('Pacient.View.Adresa'),
    -- Editare date administrative
    ('Pacient.Edit.Telefon'), ('Pacient.Edit.TelefonSecundar'), ('Pacient.Edit.Email'),
    ('Pacient.Edit.Adresa'), ('Pacient.Edit.Localitate'), ('Pacient.Edit.Judet'), ('Pacient.Edit.CodPostal'),
    ('Pacient.Edit.TipAsigurare'), ('Pacient.Edit.NumarAsigurare'), ('Pacient.Edit.CasJudetean'), ('Pacient.Edit.DataExpirareAsigurare'),
    -- Programari - complete
    ('Programare.Edit.Data'), ('Programare.Edit.Ora'), ('Programare.Edit.Durata'),
    ('Programare.Edit.Doctor'), ('Programare.Edit.TipProgramare'), ('Programare.Edit.Motiv'),
    ('Programare.Edit.Observatii'), ('Programare.Edit.Status')
) AS v(Permisiune)
WHERE r.Denumire = 'Receptioner'
  AND NOT EXISTS (
      SELECT 1 FROM dbo.RoluriPermisiuni rp 
      WHERE rp.RolID = r.RolID AND rp.Permisiune = v.Permisiune
  );

-- =============================================
-- VERIFICARE FINALA
-- =============================================
PRINT ''
PRINT '=== Verificare Permisiuni Adaugate ==='

SELECT 
    Categorie,
    COUNT(*) AS NumarPermisiuni
FROM dbo.PermisiuniDefinitii
WHERE Categorie LIKE '%.View' OR Categorie LIKE '%.Edit'
GROUP BY Categorie
ORDER BY Categorie;

PRINT ''
PRINT '=== Permisiuni Granulare per Rol ==='

SELECT 
    r.Denumire AS Rol,
    COUNT(rp.Permisiune) AS PermisiuniGranulare
FROM dbo.Roluri r
LEFT JOIN dbo.RoluriPermisiuni rp ON r.RolID = rp.RolID
    AND (rp.Permisiune LIKE '%.View.%' OR rp.Permisiune LIKE '%.Edit.%')
WHERE r.Este_Activ = 1
GROUP BY r.Denumire
ORDER BY PermisiuniGranulare DESC;

PRINT ''
PRINT '=== Script completat cu succes! ==='
GO
