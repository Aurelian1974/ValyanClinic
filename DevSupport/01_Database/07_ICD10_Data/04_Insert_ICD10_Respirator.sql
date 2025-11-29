-- ========================================
-- ICD-10 Data Insert: BOLI RESPIRATORII (J00-J99)
-- Database: ValyanMed
-- Categoria: Respirator
-- ========================================

USE [ValyanMed]
GO

PRINT '?? Inserare coduri ICD-10: BOLI RESPIRATORII...'

-- ? ICD10_ID nu mai este specificat - se genereaza automat cu NEWSEQUENTIALID()
INSERT INTO ICD10_Codes (Code, FullCode, Category, ShortDescription, LongDescription, ParentCode, IsLeafNode, IsCommon, Severity, SearchTerms)
VALUES
-- J00-J06: Infectii acute
('J00', 'J00', 'Respirator', 'Rinofaringit? acut? (guturai comun)', 'Infec?ie acut? a c?ilor respiratorii superioare', NULL, 1, 1, 'Mild', 'raceala,guturai,rinofaringita'),
('J02', 'J02', 'Respirator', 'Faringit? acut?', 'Inflama?ia acut? a faringelui', NULL, 0, 1, 'Mild', 'faringita,durere gat,amigdalita'),
('J02.9', 'J02.9', 'Respirator', 'Faringit? acut?, nespecificat?', 'Faringit? acut? f?r? agent cauzal specificat', 'J02', 1, 1, 'Mild', 'faringita nespecificata'),
('J03', 'J03', 'Respirator', 'Amigdalit? acut?', 'Inflama?ia acut? a amigdalelor palatine', NULL, 0, 1, 'Mild', 'amigdalita,angina'),
('J03.9', 'J03.9', 'Respirator', 'Amigdalit? acut?, nespecificat?', 'Amigdalit? acut? f?r? agent specificat', 'J03', 1, 1, 'Mild', 'amigdalita nespecificata'),
('J06', 'J06', 'Respirator', 'Infec?ie acut? VRS multiple', 'Infec?ii acute ale mai multor zone ale c?ilor respiratorii superioare', NULL, 0, 1, 'Mild', 'infectie respiratorie,vrs'),
('J06.9', 'J06.9', 'Respirator', 'Infec?ie VRS, nespecificat?', 'Infec?ie acut? VRS f?r? localizare specificat?', 'J06', 1, 1, 'Mild', 'infectie vrs nespecificata'),

-- J18-J22: Pneumonii
('J18', 'J18', 'Respirator', 'Pneumonie', 'Inflama?ia acut? a parenchimului pulmonar', NULL, 0, 1, 'Severe', 'pneumonie,infectie pulmonara'),
('J18.0', 'J18.0', 'Respirator', 'Bronhopneumonie, nespecificat?', 'Pneumonie cu afectare bron?ic? asociat?', 'J18', 1, 1, 'Severe', 'bronhopneumonie'),
('J18.1', 'J18.1', 'Respirator', 'Pneumonie lobar?, nespecificat?', 'Pneumonie care afecteaz? un lob pulmonar', 'J18', 1, 0, 'Severe', 'pneumonie lobara'),
('J18.9', 'J18.9', 'Respirator', 'Pneumonie, nespecificat?', 'Pneumonie f?r? agent cauzal specificat', 'J18', 1, 1, 'Severe', 'pneumonie nespecificata'),

-- J40-J47: Boli pulmonare obstructive cronice
('J40', 'J40', 'Respirator', 'Bron?it?', 'Inflama?ia bronhiilor (acut? sau cronic? nespecificat?)', NULL, 1, 1, 'Moderate', 'bronsita,tuse'),
('J44', 'J44', 'Respirator', 'Boal? pulmonar? obstructiv? cronic? (BPOC)', 'BPOC - obstruc?ie cronic? a c?ilor respiratorii', NULL, 0, 1, 'Severe', 'bpoc,emfizem,bronsita cronica'),
('J44.0', 'J44.0', 'Respirator', 'BPOC cu infec?ie acut? VRI', 'BPOC cu exacerbare infec?ioas? acut?', 'J44', 1, 1, 'Severe', 'bpoc exacerbat'),
('J44.1', 'J44.1', 'Respirator', 'BPOC cu exacerbare acut?', 'BPOC cu exacerbare acut? (f?r? infec?ie specificat?)', 'J44', 1, 1, 'Severe', 'bpoc acut'),
('J44.9', 'J44.9', 'Respirator', 'BPOC, nespecificat', 'BPOC f?r? alte specifica?ii', 'J44', 1, 1, 'Severe', 'bpoc nespecificat'),

('J45', 'J45', 'Respirator', 'Astm bron?ic', 'Boal? inflamatorie cronic? a c?ilor respiratorii cu hiperreactivitate', NULL, 0, 1, 'Moderate', 'astm,dispnee,wheezing'),
('J45.0', 'J45.0', 'Respirator', 'Astm predominant alergic', 'Astm extrinsec - declan?at de alergeni', 'J45', 1, 1, 'Moderate', 'astm alergic'),
('J45.1', 'J45.1', 'Respirator', 'Astm non-alergic', 'Astm intrinsec - f?r? cauz? alergic? identificat?', 'J45', 1, 0, 'Moderate', 'astm intrinsec'),
('J45.9', 'J45.9', 'Respirator', 'Astm, nespecificat', 'Astm bron?ic f?r? alte specifica?ii', 'J45', 1, 1, 'Moderate', 'astm nespecificat'),

-- J80-J84: Boli intersitiale
('J84', 'J84', 'Respirator', 'Boli pulmonare intersti?iale', 'Afec?iuni ale ?esutului intersti?ial pulmonar', NULL, 0, 0, 'Severe', 'fibroza pulmonara,interstitiala'),
('J84.1', 'J84.1', 'Respirator', 'Alte boli pulmonare intersti?iale cu fibroz?', 'Fibroz? pulmonar? cronic?', 'J84', 1, 0, 'Severe', 'fibroza pulmonara'),

-- J90-J94: Pleura
('J90', 'J90', 'Respirator', 'Rev?rsat pleural', 'Acumulare de lichid în spa?iul pleural', NULL, 1, 1, 'Moderate', 'pleurezie,revarsat'),
('J94', 'J94', 'Respirator', 'Alte afec?iuni pleurale', 'Boli ale pleurei, altele decât rev?rsat', NULL, 0, 0, 'Moderate', 'pleura'),
('J94.2', 'J94.2', 'Respirator', 'Hemotorax', 'Acumulare de sânge în cavitatea pleural?', 'J94', 1, 0, 'Severe', 'hemotorax,sange pleural')

PRINT '? Coduri ICD-10 RESPIRATOR inserate cu succes! (25 coduri)'
GO
