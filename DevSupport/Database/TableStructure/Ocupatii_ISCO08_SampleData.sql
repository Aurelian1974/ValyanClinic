-- ========================================
-- Script pentru Popularea cu Date Exemple - Ocupatii_ISCO08
-- Database: ValyanMed
-- Generat: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
-- ========================================

USE [ValyanMed]
GO

-- Populare cu grupele majore ISCO-08 (nivel 1)
INSERT INTO dbo.Ocupatii_ISCO08 (
    [Cod_ISCO], [Denumire_Ocupatie], [Denumire_Ocupatie_EN], [Nivel_Ierarhic], 
    [Cod_Parinte], [Grupa_Majora], [Grupa_Majora_Denumire], [Descriere]
) VALUES
('0', 'Ocupa?ii din domeniul militar', 'Armed forces occupations', 1, NULL, '0', 'Ocupa?ii din domeniul militar', 'Aceast? grup? major? include toate ocupa?iile din cadrul for?elor armate.'),
('1', 'Manageri', 'Managers', 1, NULL, '1', 'Manageri', 'Managerii planific?, dirijeaz?, coordoneaz? ?i evalueaz? activit??ile generale ale întreprinderilor, guvernelor ?i altor organiza?ii.'),
('2', 'Profesioni?ti', 'Professionals', 1, NULL, '2', 'Profesioni?ti', 'Profesioni?tii cresc stocul de cuno?tin?e existente, aplic? concepte ?i teorii ?tiin?ifice sau artistice.'),
('3', 'Tehnicieni ?i profesioni?ti asocia?i', 'Technicians and associate professionals', 1, NULL, '3', 'Tehnicieni ?i profesioni?ti asocia?i', 'Tehnicieni ?i profesioni?ti asocia?i efectueaz? sarcini tehnice ?i înrudite legate de cercetare ?i aplicarea conceptelor.'),
('4', 'Personal administrativ', 'Clerical support workers', 1, NULL, '4', 'Personal administrativ', 'Personalul administrativ înregistreaz?, organizeaz?, stocheaz?, calculeaz? ?i recupereaz? informa?ii.'),
('5', 'Lucr?tori în servicii ?i vânz?ri', 'Service and sales workers', 1, NULL, '5', 'Lucr?tori în servicii ?i vânz?ri', 'Lucr?torii în servicii ?i vânz?ri presteaz? servicii personale ?i de securitate, vând în magazine ?i pie?e.'),
('6', 'Lucr?tori califica?i în agricultur?, silvicultur? ?i pescuit', 'Skilled agricultural, forestry and fishery workers', 1, NULL, '6', 'Lucr?tori califica?i în agricultur?, silvicultur? ?i pescuit', 'Lucr?torii califica?i în agricultur?, silvicultur? ?i pescuit produc alimente, fibre, combustibili din lemn.'),
('7', 'Lucr?tori califica?i în meserii ?i me?te?uguri', 'Craft and related trades workers', 1, NULL, '7', 'Lucr?tori califica?i în meserii ?i me?te?uguri', 'Lucr?torii califica?i în meserii aplic? cuno?tin?e ?i abilit??i specifice pentru construc?ia ?i între?inerea cl?dirilor.'),
('8', 'Operatori de instala?ii ?i ma?ini ?i asamblatori', 'Plant and machine operators and assemblers', 1, NULL, '8', 'Operatori de instala?ii ?i ma?ini ?i asamblatori', 'Operatorii de instala?ii ?i ma?ini opereaz? ?i monitorizeaz? ma?ini industriale ?i echipamente.'),
('9', 'Ocupa?ii elementare', 'Elementary occupations', 1, NULL, '9', 'Ocupa?ii elementare', 'Ocupa?iile elementare implic? sarcini simple ?i de rutin? care necesit? în principal utilizarea unor instrumente simple.');

-- Populare cu subgrupe exemple pentru grupa 2 (Profesioni?ti)
INSERT INTO dbo.Ocupatii_ISCO08 (
    [Cod_ISCO], [Denumire_Ocupatie], [Denumire_Ocupatie_EN], [Nivel_Ierarhic], 
    [Cod_Parinte], [Grupa_Majora], [Grupa_Majora_Denumire], [Subgrupa], [Subgrupa_Denumire]
) VALUES
('21', 'Profesioni?ti în ?tiin?e ?i inginerie', 'Science and engineering professionals', 2, '2', '2', 'Profesioni?ti', '21', 'Profesioni?ti în ?tiin?e ?i inginerie'),
('22', 'Profesioni?ti în s?n?tate', 'Health professionals', 2, '2', '2', 'Profesioni?ti', '22', 'Profesioni?ti în s?n?tate'),
('23', 'Profesioni?ti în înv???mânt', 'Teaching professionals', 2, '2', '2', 'Profesioni?ti', '23', 'Profesioni?ti în înv???mânt'),
('24', 'Speciali?ti în administra?ia afacerilor ?i a administra?iei publice', 'Business and administration professionals', 2, '2', '2', 'Profesioni?ti', '24', 'Speciali?ti în administra?ia afacerilor ?i a administra?iei publice'),
('25', 'Profesioni?ti în tehnologia informa?iei ?i comunica?iilor', 'Information and communications technology professionals', 2, '2', '2', 'Profesioni?ti', '25', 'Profesioni?ti în tehnologia informa?iei ?i comunica?iilor'),
('26', 'Profesioni?ti juridici, sociali ?i culturali', 'Legal, social and cultural professionals', 2, '2', '2', 'Profesioni?ti', '26', 'Profesioni?ti juridici, sociali ?i culturali');

-- Populare cu grupe minore exemple pentru grupa 22 (Profesioni?ti în s?n?tate)
INSERT INTO dbo.Ocupatii_ISCO08 (
    [Cod_ISCO], [Denumire_Ocupatie], [Denumire_Ocupatie_EN], [Nivel_Ierarhic], 
    [Cod_Parinte], [Grupa_Majora], [Grupa_Majora_Denumire], [Subgrupa], [Subgrupa_Denumire],
    [Grupa_Minora], [Grupa_Minora_Denumire]
) VALUES
('221', 'Medici', 'Medical doctors', 3, '22', '2', 'Profesioni?ti', '22', 'Profesioni?ti în s?n?tate', '221', 'Medici'),
('222', 'Profesioni?ti din domeniul îngrijirii medicale', 'Nursing and midwifery professionals', 3, '22', '2', 'Profesioni?ti', '22', 'Profesioni?ti în s?n?tate', '222', 'Profesioni?ti din domeniul îngrijirii medicale'),
('223', 'Profesioni?ti în medicina tradi?ional? ?i complementar?', 'Traditional and complementary medicine professionals', 3, '22', '2', 'Profesioni?ti', '22', 'Profesioni?ti în s?n?tate', '223', 'Profesioni?ti în medicina tradi?ional? ?i complementar?'),
('224', 'Asisten?i farmacii, tehnicieni în medicin? ?i în medicina dentar? ?i al?i profesioni?ti în s?n?tate', 'Paramedical practitioners', 3, '22', '2', 'Profesioni?ti', '22', 'Profesioni?ti în s?n?tate', '224', 'Asisten?i farmacii, tehnicieni în medicin? ?i în medicina dentar? ?i al?i profesioni?ti în s?n?tate'),
('225', 'Medici veterinari', 'Veterinarians', 3, '22', '2', 'Profesioni?ti', '22', 'Profesioni?ti în s?n?tate', '225', 'Medici veterinari'),
('226', 'Al?i profesioni?ti în s?n?tate', 'Other health professionals', 3, '22', '2', 'Profesioni?ti', '22', 'Profesioni?ti în s?n?tate', '226', 'Al?i profesioni?ti în s?n?tate');

-- Populare cu ocupa?ii detaliate exemple pentru grupa 221 (Medici)
INSERT INTO dbo.Ocupatii_ISCO08 (
    [Cod_ISCO], [Denumire_Ocupatie], [Denumire_Ocupatie_EN], [Nivel_Ierarhic], 
    [Cod_Parinte], [Grupa_Majora], [Grupa_Majora_Denumire], [Subgrupa], [Subgrupa_Denumire],
    [Grupa_Minora], [Grupa_Minora_Denumire], [Descriere]
) VALUES
('2211', 'Medici generali?ti', 'Generalist medical practitioners', 4, '221', '2', 'Profesioni?ti', '22', 'Profesioni?ti în s?n?tate', '221', 'Medici', 'Medicii generali?ti diagnosticheaz?, trateaz? ?i previn bolile, r?nirile ?i alte deficien?e fizice ?i mentale la oameni prin aplicarea principiilor ?i procedurilor medicii moderne.'),
('2212', 'Medici speciali?ti', 'Specialist medical practitioners', 4, '221', '2', 'Profesioni?ti', '22', 'Profesioni?ti în s?n?tate', '221', 'Medici', 'Medicii speciali?ti diagnosticheaz?, trateaz? ?i previn bolile, r?nirile ?i alte deficien?e fizice ?i mentale la oameni, folosind metode specializate, tehnici ?i proceduri specifice anumitor domenii ale medicinii.'),
('2213', 'Medici în medicina animalelor', 'Veterinarians', 4, '221', '2', 'Profesioni?ti', '22', 'Profesioni?ti în s?n?tate', '221', 'Medici', 'Medicii veterinari previn, diagnosticheaz? ?i trateaz? bolile, r?nirile ?i disfunc?iile la animale.'),
('2214', 'Farmaci?ti', 'Pharmacists', 4, '221', '2', 'Profesioni?ti', '22', 'Profesioni?ti în s?n?tate', '221', 'Medici', 'Farmaci?tii stocheaz?, p?streaz?, distribuie ?i vând medicamente ?i ofer? sfaturi cu privire la utilizarea lor eficient? ?i sigur?.'),
('2215', 'Al?i profesioni?ti în domeniul s?n?t??ii', 'Other health professionals', 4, '221', '2', 'Profesioni?ti', '22', 'Profesioni?ti în s?n?tate', '221', 'Medici', 'Aceast? grup? include al?i profesioni?ti în s?n?tate care nu sunt clasifica?i în alt? parte.');

-- Populare cu ocupa?ii pentru IT (relevante pentru proiect)
INSERT INTO dbo.Ocupatii_ISCO08 (
    [Cod_ISCO], [Denumire_Ocupatie], [Denumire_Ocupatie_EN], [Nivel_Ierarhic], 
    [Cod_Parinte], [Grupa_Majora], [Grupa_Majora_Denumire], [Subgrupa], [Subgrupa_Denumire],
    [Grupa_Minora], [Grupa_Minora_Denumire], [Descriere]
) VALUES
('25', 'Profesioni?ti în tehnologia informa?iei ?i comunica?iilor', 'Information and communications technology professionals', 2, '2', '2', 'Profesioni?ti', '25', 'Profesioni?ti în tehnologia informa?iei ?i comunica?iilor', NULL, NULL, 'Profesioni?tii în TIC cerceteaz?, analizeaz?, proiecteaz?, programeaz?, modific?, testeaz? ?i men?in sisteme informatice.'),
('251', 'Analisti si designeri de software si aplicatii', 'Software and applications developers and analysts', 3, '25', '2', 'Profesioni?ti', '25', 'Profesioni?ti în tehnologia informa?iei ?i comunica?iilor', '251', 'Analisti si designeri de software si aplicatii'),
('252', 'Specialisti in baze de date si retele de calculatoare', 'Database and network professionals', 3, '25', '2', 'Profesioni?ti', '25', 'Profesioni?ti în tehnologia informa?iei ?i comunica?iilor', '252', 'Specialisti in baze de date si retele de calculatoare'),
('2511', 'Analisti de sisteme', 'Systems analysts', 4, '251', '2', 'Profesioni?ti', '25', 'Profesioni?ti în tehnologia informa?iei ?i comunica?iilor', '251', 'Analisti si designeri de software si aplicatii', 'Anali?tii de sisteme analizeaz? cerin?ele utilizatorilor, procedurile ?i problemele pentru a automatiza sau îmbun?t??i sistemele informatice existente.'),
('2512', 'Dezvoltatori de software', 'Software developers', 4, '251', '2', 'Profesioni?ti', '25', 'Profesioni?ti în tehnologia informa?iei ?i comunica?iilor', '251', 'Analisti si designeri de software si aplicatii', 'Dezvoltatorii de software cerceteaz?, analizeaz? ?i evalueaz? cerin?ele pentru software-ul existent ?i nou ?i proiecteaz?, programeaz?, testeaz? ?i men?in software-ul de sistem pentru calculatoare.'),
('2513', 'Dezvoltatori de aplicatii web si multimedia', 'Web and multimedia developers', 4, '251', '2', 'Profesioni?ti', '25', 'Profesioni?ti în tehnologia informa?iei ?i comunica?iilor', '251', 'Analisti si designeri de software si aplicatii', 'Dezvoltatorii web ?i multimedia proiecteaz?, dezvolt?, testeaz?, men?in ?i suport? aplica?ii software pentru internet, intranet ?i dispozitive mobile.');

PRINT 'Date exemple pentru Ocupatii_ISCO08 inserate cu succes.';

-- Afisare statistici dupa populare
EXEC sp_Ocupatii_ISCO08_GetStatistics;