-- ========================================
-- ICD-10 Data: TOP 100 CODURI COMUNE (ROMÂN?)
-- Database: ValyanMed
-- Descriere: Cele mai folosite coduri în medicina primar?
-- Sursa: OMS ICD-10 + Practic? medical? RO
-- ========================================

USE [ValyanMed]
GO

PRINT '?? Actualizare traduceri ROMÂN? pentru coduri COMUNE...'

-- Update existing codes cu traduceri RO
UPDATE ICD10_Codes SET 
    ShortDescription = 'Hipertensiune arterial? esen?ial? (primar?)',
    LongDescription = 'Hipertensiune arterial? idiopatic?, f?r? cauz? identificabil?',
    SearchTerms = 'hta,hipertensiune,tensiune mare,presiune arteriala crescuta'
WHERE Code = 'I10'

UPDATE ICD10_Codes SET
    ShortDescription = 'Diabet zaharat tip 2',
    LongDescription = 'Diabet zaharat non-insulino-dependent, de obicei la adul?i',
    SearchTerms = 'dz2,diabet,zahar mare,glicemie crescuta,diabet adult'
WHERE Code = 'E11'

UPDATE ICD10_Codes SET
    ShortDescription = 'Obezitate datorat? excesului caloric',
    LongDescription = 'Obezitate primar? cauzat? de aport caloric excesiv',
    SearchTerms = 'obezitate,supraponderal,greutate mare,imc crescut'
WHERE Code = 'E66.0'

UPDATE ICD10_Codes SET
    ShortDescription = 'Hipercolesterolemie pur?',
    LongDescription = 'Colesterol total crescut, f?r? cre?tere trigliceridelor',
    SearchTerms = 'colesterol mare,hipercolesterolemie,ldl crescut'
WHERE Code = 'E78.0'

UPDATE ICD10_Codes SET
    ShortDescription = 'Hiperlipemie mixt?',
    LongDescription = 'Colesterol ?i trigliceride crescute concomitent',
    SearchTerms = 'dislipidemie,colesterol si trigliceride,hiperlipemie'
WHERE Code = 'E78.2'

-- Adaug? mai multe coduri comune
INSERT INTO ICD10_Codes (Code, FullCode, Category, ShortDescription, LongDescription, ParentCode, IsLeafNode, IsCommon, Severity, SearchTerms)
VALUES
-- Digestiv
('K21', 'K21', 'Digestiv', 'Reflux gastro-esofagian (GERD)', 'Boala de reflux gastro-esofagian cu sau f?r? esofagit?', NULL, 0, 1, 'Moderate', 'gerd,reflux,arsuri,piroza,regurgitare'),
('K21.9', 'K21.9', 'Digestiv', 'Reflux gastro-esofagian f?r? esofagit?', 'GERD simplu, f?r? leziuni esofagiene', 'K21', 1, 1, 'Mild', 'gerd simplu,reflux necomplicat'),
('K29', 'K29', 'Digestiv', 'Gastrit? ?i duodenit?', 'Inflama?ia mucoasei gastrice ?i/sau duodenale', NULL, 0, 1, 'Moderate', 'gastrita,stomac,durere stomac,dispepsie'),
('K29.7', 'K29.7', 'Digestiv', 'Gastrit? cronic?, nespecificat?', 'Gastrit? cronic? f?r? alte specifica?ii', 'K29', 1, 1, 'Moderate', 'gastrita cronica'),
('K80', 'K80', 'Digestiv', 'Litiaz? biliar? (Colecistolitiaz?)', 'Calculi (pietre) în vezica biliar?', NULL, 0, 1, 'Moderate', 'litiaza,pietre bil?,colici biliare,calculi vezica'),
('K80.2', 'K80.2', 'Digestiv', 'Litiaz? biliar? f?r? colecistit?', 'Calculi în vezica biliar?, f?r? inflama?ie', 'K80', 1, 1, 'Moderate', 'pietre vezica fara infectie'),

-- Neurologic
('G43', 'G43', 'Neurologic', 'Migren?', 'Cefalee vascular? recurent?, adesea unilateral?', NULL, 0, 1, 'Moderate', 'migrena,durere cap,cefalee,hemicranee'),
('G43.0', 'G43.0', 'Neurologic', 'Migren? f?r? aur?', 'Migren? comun?, f?r? simptome neurologice premonitorii', 'G43', 1, 1, 'Moderate', 'migrena simpla,migrena comuna'),
('G43.1', 'G43.1', 'Neurologic', 'Migren? cu aur?', 'Migren? clasic?, cu simptome neurologice premonitorii', 'G43', 1, 1, 'Moderate', 'migrena cu aura,migrena clasica'),
('G43.9', 'G43.9', 'Neurologic', 'Migren?, nespecificat?', 'Migren? f?r? alte specifica?ii', 'G43', 1, 1, 'Moderate', 'migrena nespecificata'),
('G47', 'G47', 'Neurologic', 'Tulbur?ri de somn', 'Deregl?ri ale ciclului somn-veghe', NULL, 0, 1, 'Moderate', 'insomnie,tulburari somn,somnolenta'),
('G47.0', 'G47.0', 'Neurologic', 'Insomnie', 'Dificultate de ini?iere sau men?inere a somnului', 'G47', 1, 1, 'Moderate', 'insomnie,nu pot dormi'),

-- Genito-urinar
('N39', 'N39', 'Genito-urinar', 'Alte tulbur?ri ale tractului urinar', 'Afec?iuni ale tractului urinar', NULL, 0, 1, 'Moderate', 'urinar,vezica,mictiune'),
('N39.0', 'N39.0', 'Genito-urinar', 'Infec?ie a tractului urinar, localizare nespecificat?', 'Infec?ie urinar? f?r? localizare specificat?', 'N39', 1, 1, 'Moderate', 'infectie urinara,cistita,itu'),

-- Musculo-scheletic
('M54', 'M54', 'Musculo-scheletic', 'Dorsalgie (Dureri de spate)', 'Dureri localizate în diferite regiuni ale coloanei', NULL, 0, 1, 'Moderate', 'dureri spate,lombalgie,dorsalgie'),
('M54.5', 'M54.5', 'Musculo-scheletic', 'Lombalgie', 'Durere în regiunea lombar? (partea inferioar? a spatelui)', 'M54', 1, 1, 'Moderate', 'durere jos spate,lombalgie,lumbago'),
('M54.2', 'M54.2', 'Musculo-scheletic', 'Cervicalgie', 'Durere în regiunea cervical? (gât)', 'M54', 1, 1, 'Moderate', 'durere gat,cervicalgie,durere ceafa'),
('M79', 'M79', 'Musculo-scheletic', 'Alte tulbur?ri ale ?esuturilor moi', 'Afec?iuni ale mu?chilor ?i ?esuturilor moi', NULL, 0, 1, 'Mild', 'muschi,tendoane,dureri'),
('M79.1', 'M79.1', 'Musculo-scheletic', 'Mialgii', 'Dureri musculare difuze', 'M79', 1, 1, 'Mild', 'dureri musculare,mialgii'),

-- Piele
('L20', 'L20', 'Piele', 'Dermatit? atopic? (Eczem?)', 'Inflama?ie cronic? a pielii, de natur? alergic?', NULL, 0, 1, 'Moderate', 'eczema,dermatita,mancarime,alergii piele'),
('L20.9', 'L20.9', 'Piele', 'Dermatit? atopic?, nespecificat?', 'Eczem? atopic? f?r? alte specifica?ii', 'L20', 1, 1, 'Moderate', 'eczema nespecificata'),

-- Ochi ?i Urechi
('H10', 'H10', 'Ochi', 'Conjunctivit?', 'Inflama?ia conjunctivei oculare', NULL, 0, 1, 'Mild', 'conjunctivita,ochi rosii,infectie ochi'),
('H10.9', 'H10.9', 'Ochi', 'Conjunctivit?, nespecificat?', 'Conjunctivit? f?r? agent specificat', 'H10', 1, 1, 'Mild', 'conjunctivita nespecificata'),
('H66', 'H66', 'Ureche', 'Otit? medie supurativ? ?i nespecificat?', 'Infec?ie a urechii medii', NULL, 0, 1, 'Moderate', 'otita,durere ureche,infectie ureche'),
('H66.9', 'H66.9', 'Ureche', 'Otit? medie, nespecificat?', 'Otit? medie f?r? alte specifica?ii', 'H66', 1, 1, 'Moderate', 'otita nespecificata'),

-- Simptome generale
('R50', 'R50', 'Simptome', 'Febr? de cauz? necunoscut?', 'Temperatur? corporal? crescut? f?r? cauz? identificat?', NULL, 0, 1, 'Moderate', 'febra,temperatura,stare febrila'),
('R50.9', 'R50.9', 'Simptome', 'Febr?, nespecificat?', 'Febr? f?r? alte specifica?ii', 'R50', 1, 1, 'Moderate', 'febra nespecificata'),
('R51', 'R51', 'Simptome', 'Cefalee', 'Durere de cap (nespecificat? ca migren? sau cefalee tensional?)', NULL, 1, 1, 'Mild', 'durere cap,cefalee,cap greu'),
('R05', 'R05', 'Simptome', 'Tuse', 'Tuse (acut? sau cronic?)', NULL, 1, 1, 'Mild', 'tuse,tuse seaca,tuse productiva'),
('R06', 'R06', 'Simptome', 'Anomalii ale respira?iei', 'Dificult??i respiratorii', NULL, 0, 1, 'Moderate', 'dispnee,dificultate respiratie,respiratie greoaie'),
('R06.0', 'R06.0', 'Simptome', 'Dispnee', 'Dificultate de respira?ie, senza?ie de lips? de aer', 'R06', 1, 1, 'Moderate', 'dispnee,lipsa aer,greu de respirat')

PRINT '? Actualizare traduceri ROMÂN? completat?!'
PRINT '   Total coduri COMUNE actualizate: ~40 coduri'
PRINT '   Acestea sunt cele mai frecvente în medicina primar?'
GO
