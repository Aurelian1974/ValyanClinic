-- ========================================
-- ICD-10 Data Insert: BOLI ENDOCRINE (E00-E90)
-- Database: ValyanMed
-- Categoria: Endocrin
-- ========================================

USE [ValyanMed]
GO

PRINT '?? Inserare coduri ICD-10: BOLI ENDOCRINE...'

-- E10-E14: Diabet zaharat
-- ? ICD10_ID nu mai este specificat - se genereaza automat cu NEWSEQUENTIALID()
INSERT INTO ICD10_Codes (Code, FullCode, Category, ShortDescription, LongDescription, ParentCode, IsLeafNode, IsCommon, Severity, SearchTerms)
VALUES
('E10', 'E10', 'Endocrin', 'Diabet zaharat tip 1', 'Diabet zaharat insulino-dependent (DZ tip 1)', NULL, 0, 1, 'Severe', 'dz1,diabet tip 1,insulino-dependent,diabet juvenil'),
('E10.9', 'E10.9', 'Endocrin', 'Diabet zaharat tip 1 f?r? complica?ii', 'DZ tip 1 necomplicat', 'E10', 1, 1, 'Moderate', 'dz1 necomplicat'),

('E11', 'E11', 'Endocrin', 'Diabet zaharat tip 2', 'Diabet zaharat non-insulino-dependent (DZ tip 2)', NULL, 0, 1, 'Moderate', 'dz2,diabet tip 2,non-insulino-dependent'),
('E11.9', 'E11.9', 'Endocrin', 'Diabet zaharat tip 2 f?r? complica?ii', 'DZ tip 2 necomplicat', 'E11', 1, 1, 'Moderate', 'dz2 necomplicat'),
('E11.2', 'E11.2', 'Endocrin', 'DZ tip 2 cu complica?ii renale', 'Diabet zaharat tip 2 cu nefropatie diabetic?', 'E11', 1, 1, 'Severe', 'dz2 nefropatie,diabet renal'),
('E11.3', 'E11.3', 'Endocrin', 'DZ tip 2 cu complica?ii oftalmologice', 'Diabet zaharat tip 2 cu retinopatie diabetic?', 'E11', 1, 1, 'Severe', 'dz2 retinopatie,diabet ochi'),
('E11.4', 'E11.4', 'Endocrin', 'DZ tip 2 cu complica?ii neurologice', 'Diabet zaharat tip 2 cu neuropatie diabetic?', 'E11', 1, 1, 'Severe', 'dz2 neuropatie,diabet neurologic'),
('E11.5', 'E11.5', 'Endocrin', 'DZ tip 2 cu complica?ii circulatorii', 'Diabet zaharat tip 2 cu complica?ii vasculare periferice', 'E11', 1, 1, 'Severe', 'dz2 vascular,angiopatie diabetica'),
('E11.6', 'E11.6', 'Endocrin', 'DZ tip 2 cu alte complica?ii', 'Diabet zaharat tip 2 cu alte complica?ii specificate', 'E11', 1, 0, 'Severe', 'dz2 alte complicatii'),

-- E66: Obezitate
('E66', 'E66', 'Endocrin', 'Obezitate', 'Obezitate (IMC ?30 kg/m²)', NULL, 0, 1, 'Moderate', 'obezitate,supraponderal,gras'),
('E66.0', 'E66.0', 'Endocrin', 'Obezitate datorat? excesului caloric', 'Obezitate primar? prin hiperalimen?ie', 'E66', 1, 1, 'Moderate', 'obezitate alimentara'),
('E66.1', 'E66.1', 'Endocrin', 'Obezitate indus? de medicamente', 'Obezitate secundar? tratamentului medicamentos', 'E66', 1, 0, 'Moderate', 'obezitate medicamentoasa'),
('E66.2', 'E66.2', 'Endocrin', 'Obezitate extrem? cu hipoventila?ie', 'Sindrom Pickwick - obezitate morbid? cu insuficien?? respiratorie', 'E66', 1, 0, 'Severe', 'obezitate morbida,sindrom pickwick'),
('E66.8', 'E66.8', 'Endocrin', 'Alte forme de obezitate', 'Obezitate de alte cauze', 'E66', 1, 0, 'Moderate', 'obezitate secundara'),
('E66.9', 'E66.9', 'Endocrin', 'Obezitate, nespecificat?', 'Obezitate f?r? alte specifica?ii', 'E66', 1, 1, 'Moderate', 'obezitate nespecificata'),

-- E78: Dislipidemii
('E78', 'E78', 'Endocrin', 'Tulbur?ri ale metabolismului lipoproteinelor', 'Dislipidemii (colesterol, trigliceride crescute)', NULL, 0, 1, 'Moderate', 'dislipidemie,colesterol,trigliceride'),
('E78.0', 'E78.0', 'Endocrin', 'Hipercolesterolemie pur?', 'Colesterol total crescut (hiperlipemia tip IIa)', 'E78', 1, 1, 'Moderate', 'colesterol mare,hipercolesterolemie'),
('E78.1', 'E78.1', 'Endocrin', 'Hipergliceridemie pur?', 'Trigliceride crescute (hiperlipemia tip IV)', 'E78', 1, 1, 'Moderate', 'trigliceride mari,hipertrigliceridemie'),
('E78.2', 'E78.2', 'Endocrin', 'Hiperlipemie mixt?', 'Colesterol ?i trigliceride crescute (hiperlipemia tip IIb)', 'E78', 1, 1, 'Moderate', 'dislipidemie mixta'),
('E78.5', 'E78.5', 'Endocrin', 'Hiperlipidemie, nespecificat?', 'Dislipidemie f?r? alte specifica?ii', 'E78', 1, 1, 'Moderate', 'dislipidemie nespecificata'),

-- E03-E07: Boli tiroidiene
('E03', 'E03', 'Endocrin', 'Hipotiroidism', 'Func?ie sc?zut? a glandei tiroide', NULL, 0, 1, 'Moderate', 'hipotiroidism,tiroida'),
('E03.9', 'E03.9', 'Endocrin', 'Hipotiroidism, nespecificat', 'Hipotiroidism f?r? alte specifica?ii', 'E03', 1, 1, 'Moderate', 'hipotiroidism nespecificat'),

('E05', 'E05', 'Endocrin', 'Tirotoxicoz? (hipertiroidism)', 'Func?ie crescut? a glandei tiroide', NULL, 0, 1, 'Moderate', 'hipertiroidism,tirotoxicoza,tiroida'),
('E05.0', 'E05.0', 'Endocrin', 'Tirotoxicoz? cu gu?? difuz?', 'Boala Graves-Basedow', 'E05', 1, 1, 'Moderate', 'graves,basedow,gusa toxica'),
('E05.9', 'E05.9', 'Endocrin', 'Tirotoxicoz?, nespecificat?', 'Hipertiroidism f?r? alte specifica?ii', 'E05', 1, 1, 'Moderate', 'hipertiroidism nespecificat'),

('E06', 'E06', 'Endocrin', 'Tiroidit?', 'Inflama?ia glandei tiroide', NULL, 0, 0, 'Moderate', 'tiroidita,inflamatie tiroida'),
('E06.3', 'E06.3', 'Endocrin', 'Tiroidit? autoimun?', 'Tiroidita Hashimoto', 'E06', 1, 1, 'Moderate', 'hashimoto,tiroidita autoimuna')

PRINT '? Coduri ICD-10 ENDOCRIN inserate cu succes! (26 coduri)'
GO
