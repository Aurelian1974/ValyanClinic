-- ========================================
-- ICD-10 Data Insert: BOLI CARDIOVASCULARE (I00-I99)
-- Database: ValyanMed
-- Categoria: Cardiovascular
-- ========================================

USE [ValyanMed]
GO

PRINT '?? Inserare coduri ICD-10: BOLI CARDIOVASCULARE...'

-- I20-I25: Cardiopatie ischemica
-- ? ICD10_ID nu mai este specificat - se genereaza automat cu NEWSEQUENTIALID()
INSERT INTO ICD10_Codes (Code, FullCode, Category, ShortDescription, LongDescription, ParentCode, IsLeafNode, IsCommon, Severity, SearchTerms)
VALUES
('I20', 'I20', 'Cardiovascular', 'Angina pectoral?', 'Angina pectoral? (angor) - durere toracic? cauzat? de ischemia miocardic?', NULL, 0, 1, 'Moderate', 'angina,angor,durere toracica,ischemie'),
('I20.0', 'I20.0', 'Cardiovascular', 'Angina instabil?', 'Angina instabil? - risc crescut de infarct miocardic', 'I20', 1, 1, 'Severe', 'angina instabila,crescendo,preinfarct'),
('I20.1', 'I20.1', 'Cardiovascular', 'Angina cu spasm coronarian', 'Angina Prinzmetal - spasm al arterelor coronariene', 'I20', 1, 0, 'Moderate', 'angina prinzmetal,spasm coronarian,angina vasospastica'),
('I20.8', 'I20.8', 'Cardiovascular', 'Alte forme de angin? pectoral?', 'Alte forme specificate de angin? pectoral?', 'I20', 1, 0, 'Moderate', 'angina atipica'),
('I20.9', 'I20.9', 'Cardiovascular', 'Angin? pectoral?, nespecificat?', 'Angin? pectoral? f?r? alte specifica?ii', 'I20', 1, 1, 'Moderate', 'angina nespecificata'),

('I21', 'I21', 'Cardiovascular', 'Infarct miocardic acut', 'Infarct miocardic acut (IMA) - necroz? a mu?chiului cardiac', NULL, 0, 1, 'Critical', 'ima,infarct,atac cardiac,ischemie acuta'),
('I21.0', 'I21.0', 'Cardiovascular', 'Infarct miocardic STEMI anterior', 'Infarct miocardic acut cu supradenivelare ST (STEMI) localizare anterioar?', 'I21', 1, 1, 'Critical', 'stemi anterior,ima anterior,infarct perete anterior'),
('I21.1', 'I21.1', 'Cardiovascular', 'Infarct miocardic STEMI inferior', 'Infarct miocardic acut cu supradenivelare ST (STEMI) localizare inferioar?', 'I21', 1, 1, 'Critical', 'stemi inferior,ima inferior,infarct perete inferior'),
('I21.2', 'I21.2', 'Cardiovascular', 'Infarct miocardic STEMI posterior', 'Infarct miocardic acut cu supradenivelare ST (STEMI) localizare posterioar?', 'I21', 1, 0, 'Critical', 'stemi posterior,ima posterior'),
('I21.4', 'I21.4', 'Cardiovascular', 'Infarct miocardic NSTEMI', 'Infarct miocardic acut f?r? supradenivelare ST (NSTEMI)', 'I21', 1, 1, 'Critical', 'nstemi,ima fara st,non-stemi'),
('I21.9', 'I21.9', 'Cardiovascular', 'Infarct miocardic acut, nespecificat', 'Infarct miocardic acut f?r? alte specifica?ii', 'I21', 1, 1, 'Critical', 'ima nespecificat'),

('I25', 'I25', 'Cardiovascular', 'Cardiopatie ischemic? cronic?', 'Boli cardiace cronice de origine ischemic?', NULL, 0, 1, 'Moderate', 'cardiopatie ischemiaca,ischemie cronica'),
('I25.1', 'I25.1', 'Cardiovascular', 'Cardiopatie aterosclerotic?', 'Cardiopatie aterosclerotic? - boli ale arterelor coronariene', 'I25', 1, 1, 'Moderate', 'ateroscleroza coronariana,boala coronariana'),
('I25.2', 'I25.2', 'Cardiovascular', 'Infarct miocardic vechi', 'Infarct miocardic vechi (cicatrice post-IM)', 'I25', 1, 1, 'Moderate', 'infarct vechi,cicatrice post-im,sechelar'),
('I25.5', 'I25.5', 'Cardiovascular', 'Cardiomiopatie ischemic?', 'Cardiomiopatie cauzat? de ischemie coronarian? cronic?', 'I25', 1, 0, 'Severe', 'cardiomiopatie ischemiaca'),

-- I10-I15: Hipertensiune arteriala
('I10', 'I10', 'Cardiovascular', 'Hipertensiune arterial? esen?ial?', 'Hipertensiune arterial? (HTA) primar?, esen?ial?, idiopatic?', NULL, 1, 1, 'Moderate', 'hta,hipertensiune,tensiune mare,presiune arteriala'),
('I11', 'I11', 'Cardiovascular', 'Cardiopatie hipertensiv?', 'Boal? cardiac? cauzat? de hipertensiune arterial?', NULL, 1, 1, 'Severe', 'cardiopatie hta,cord hipertensiv'),
('I12', 'I12', 'Cardiovascular', 'Nefropatie hipertensiv?', 'Boal? renal? cauzat? de hipertensiune arterial?', NULL, 1, 0, 'Severe', 'nefropatie hta,rinichi hipertensiv'),
('I13', 'I13', 'Cardiovascular', 'Cardiopatie ?i nefropatie hipertensiv?', 'Afectare concomitent? cardio-renal? cauzat? de HTA', NULL, 1, 0, 'Severe', 'cardio-renal hipertensiv'),
('I15', 'I15', 'Cardiovascular', 'Hipertensiune arterial? secundar?', 'Hipertensiune cauzat? de alte boli (renale, endocrine)', NULL, 1, 0, 'Moderate', 'hta secundara'),

-- I48: Fibrilatie atriala
('I48', 'I48', 'Cardiovascular', 'Fibrila?ie atrial? ?i flutter atrial', 'Aritmie supraventricular? frecvent?', NULL, 0, 1, 'Moderate', 'fa,fibrilatie,flutter,aritmie'),
('I48.0', 'I48.0', 'Cardiovascular', 'Fibrila?ie atrial? paroxistic?', 'FA cu episoade autolimitate (<7 zile)', 'I48', 1, 1, 'Moderate', 'fa paroxistica,fibrilatie paroxistica'),
('I48.1', 'I48.1', 'Cardiovascular', 'Fibrila?ie atrial? persistent?', 'FA care dureaz? >7 zile', 'I48', 1, 1, 'Moderate', 'fa persistenta,fibrilatie persistenta'),
('I48.2', 'I48.2', 'Cardiovascular', 'Fibrila?ie atrial? permanent?', 'FA cronic? acceptat?', 'I48', 1, 1, 'Moderate', 'fa permanenta,fibrilatie cronica'),
('I48.3', 'I48.3', 'Cardiovascular', 'Flutter atrial tipic', 'Flutter atrial tipic (comun)', 'I48', 1, 0, 'Moderate', 'flutter tipic'),

-- I50: Insuficienta cardiaca
('I50', 'I50', 'Cardiovascular', 'Insuficien?? cardiac?', 'Incapacitatea cordului de a pompa sânge eficient', NULL, 0, 1, 'Severe', 'ic,insuficienta cardiaca,decompensare'),
('I50.0', 'I50.0', 'Cardiovascular', 'Insuficien?? cardiac? congestiv?', 'IC cu congestie pulmonar? ?i/sau sistemic?', 'I50', 1, 1, 'Severe', 'icc,insuficienta congestiva'),
('I50.1', 'I50.1', 'Cardiovascular', 'Insuficien?? cardiac? stâng?', 'IC cu predominan?? stâng? (edem pulmonar)', 'I50', 1, 1, 'Severe', 'ic stanga,edem pulmonar'),
('I50.9', 'I50.9', 'Cardiovascular', 'Insuficien?? cardiac?, nespecificat?', 'IC f?r? alte specifica?ii', 'I50', 1, 1, 'Severe', 'ic nespecificata')

PRINT '? Coduri ICD-10 CARDIOVASCULAR inserate cu succes! (30 coduri)'
GO
