-- ========================================
-- ICD-10 Data Insert: CODURI SUPLIMENTARE COMUNE
-- Database: ValyanMed
-- Descriere: Coduri ICD-10 frecvent utilizate în medicina primar?
-- Total: ~200 coduri suplimentare
-- ========================================

USE [ValyanMed]
GO

PRINT '?? Inserare coduri ICD-10 suplimentare...'
PRINT ''

-- ========================================
-- BOLI DIGESTIVE (K00-K93)
-- ========================================
PRINT '??? Categoria: DIGESTIV...'

INSERT INTO ICD10_Codes (Code, FullCode, Category, ShortDescription, LongDescription, ParentCode, IsLeafNode, IsCommon, Severity, SearchTerms)
VALUES
-- GERD ?i esofag
('K21', 'K21', 'Digestiv', 'Boala de reflux gastroesofagian', 'GERD - reflux gastroesofagian', NULL, 0, 1, 'Mild', 'gerd,reflux,arsuri stomac,pirozis'),
('K21.0', 'K21.0', 'Digestiv', 'GERD cu esofagit?', 'Boala de reflux gastroesofagian cu esofagit?', 'K21', 1, 1, 'Moderate', 'gerd esofagita,reflux esofagita'),
('K21.9', 'K21.9', 'Digestiv', 'GERD f?r? esofagit?', 'Boala de reflux gastroesofagian f?r? esofagit?', 'K21', 1, 1, 'Mild', 'gerd fara esofagita'),

-- Gastrite
('K29', 'K29', 'Digestiv', 'Gastrit? ?i duodenit?', 'Inflama?ia mucoasei gastrice ?i duodenale', NULL, 0, 1, 'Mild', 'gastrita,duodenita,stomac'),
('K29.0', 'K29.0', 'Digestiv', 'Gastrit? acut? hemoragic?', 'Gastrit? acut? cu hemoragie', 'K29', 1, 0, 'Severe', 'gastrita acuta,hemoragie gastrica'),
('K29.3', 'K29.3', 'Digestiv', 'Gastrit? cronic? superficial?', 'Gastrit? cronic? superficial?', 'K29', 1, 1, 'Mild', 'gastrita cronica'),
('K29.5', 'K29.5', 'Digestiv', 'Gastrit? cronic? nespecificat?', 'Gastrit? cronic?, nespecificat?', 'K29', 1, 1, 'Mild', 'gastrita cronica nespecificata'),
('K29.7', 'K29.7', 'Digestiv', 'Gastrit? nespecificat?', 'Gastrit?, nespecificat?', 'K29', 1, 1, 'Mild', 'gastrita'),

-- Ulcer
('K25', 'K25', 'Digestiv', 'Ulcer gastric', 'Ulcer al stomacului', NULL, 0, 1, 'Moderate', 'ulcer gastric,ulcer stomac'),
('K25.9', 'K25.9', 'Digestiv', 'Ulcer gastric nespecificat', 'Ulcer gastric, nespecificat, f?r? hemoragie sau perfora?ie', 'K25', 1, 1, 'Moderate', 'ulcer gastric'),
('K26', 'K26', 'Digestiv', 'Ulcer duodenal', 'Ulcer al duodenului', NULL, 0, 1, 'Moderate', 'ulcer duodenal,ulcer duoden'),
('K26.9', 'K26.9', 'Digestiv', 'Ulcer duodenal nespecificat', 'Ulcer duodenal, nespecificat', 'K26', 1, 1, 'Moderate', 'ulcer duodenal'),

-- Colon iritabil ?i constipa?ie
('K58', 'K58', 'Digestiv', 'Sindrom de colon iritabil', 'Colon iritabil - IBS', NULL, 0, 1, 'Mild', 'ibs,colon iritabil,intestin iritabil'),
('K58.0', 'K58.0', 'Digestiv', 'Sindrom colon iritabil cu diaree', 'IBS cu predominan?? de diaree', 'K58', 1, 1, 'Mild', 'ibs diaree'),
('K58.9', 'K58.9', 'Digestiv', 'Sindrom colon iritabil nespecificat', 'IBS f?r? specificare', 'K58', 1, 1, 'Mild', 'ibs nespecificat'),
('K59.0', 'K59.0', 'Digestiv', 'Constipa?ie', 'Constipa?ie func?ional?', NULL, 1, 1, 'Mild', 'constipatie,tranzit lent'),

-- Ficat ?i biliar
('K70', 'K70', 'Digestiv', 'Boal? hepatic? alcoolic?', 'Afectare hepatic? datorat? alcoolului', NULL, 0, 1, 'Severe', 'ficat alcoolic,hepatopatie alcoolica'),
('K70.3', 'K70.3', 'Digestiv', 'Ciroz? hepatic? alcoolic?', 'Ciroz? hepatic? datorat? alcoolului', 'K70', 1, 1, 'Critical', 'ciroza alcoolica'),
('K74', 'K74', 'Digestiv', 'Fibroz? ?i ciroz? hepatic?', 'Fibroz? ?i ciroz? hepatic?', NULL, 0, 1, 'Severe', 'ciroza,fibroza hepatica'),
('K74.6', 'K74.6', 'Digestiv', 'Ciroz? hepatic? nespecificat?', 'Ciroz? hepatic?, alte ?i nespecificate', 'K74', 1, 1, 'Severe', 'ciroza hepatica'),
('K76.0', 'K76.0', 'Digestiv', 'Steatoz? hepatic?', 'Ficat gras - steatoz? hepatic? non-alcoolic?', NULL, 1, 1, 'Mild', 'ficat gras,steatoza,nafld'),
('K80', 'K80', 'Digestiv', 'Litiaz? biliar?', 'Calculi biliari - colelitiaz?', NULL, 0, 1, 'Moderate', 'calculi biliari,pietre fiere,colecist'),
('K80.2', 'K80.2', 'Digestiv', 'Litiaz? vezicular? f?r? colecistit?', 'Calculi veziculari f?r? colecistit?', 'K80', 1, 1, 'Moderate', 'calculi veziculari'),
('K81', 'K81', 'Digestiv', 'Colecistit?', 'Inflama?ia vezicii biliare', NULL, 0, 1, 'Moderate', 'colecistita,inflamatie vezica biliara'),
('K81.0', 'K81.0', 'Digestiv', 'Colecistit? acut?', 'Colecistit? acut?', 'K81', 1, 1, 'Severe', 'colecistita acuta')
GO

-- ========================================
-- BOLI GENITO-URINARE (N00-N99)
-- ========================================
PRINT '?? Categoria: GENITO-URINAR...'

INSERT INTO ICD10_Codes (Code, FullCode, Category, ShortDescription, LongDescription, ParentCode, IsLeafNode, IsCommon, Severity, SearchTerms)
VALUES
-- Infec?ii urinare
('N39.0', 'N39.0', 'Genito-urinar', 'Infec?ie de tract urinar', 'ITU - Infec?ie urinar? nespecificat?', NULL, 1, 1, 'Mild', 'itu,infectie urinara,cistita'),
('N30', 'N30', 'Genito-urinar', 'Cistit?', 'Inflama?ia vezicii urinare', NULL, 0, 1, 'Mild', 'cistita,infectie vezica'),
('N30.0', 'N30.0', 'Genito-urinar', 'Cistit? acut?', 'Cistit? acut?', 'N30', 1, 1, 'Mild', 'cistita acuta'),
('N30.1', 'N30.1', 'Genito-urinar', 'Cistit? intersti?ial? cronic?', 'Cistit? intersti?ial? cronic?', 'N30', 1, 0, 'Moderate', 'cistita cronica'),
('N10', 'N10', 'Genito-urinar', 'Pielonefrit? acut?', 'Nefrit? tubulo-intersti?ial? acut?', NULL, 1, 1, 'Severe', 'pielonefrita,infectie rinichi'),
('N11', 'N11', 'Genito-urinar', 'Pielonefrit? cronic?', 'Nefrit? tubulo-intersti?ial? cronic?', NULL, 1, 0, 'Moderate', 'pielonefrita cronica'),

-- Litiaz? renal?
('N20', 'N20', 'Genito-urinar', 'Litiaz? renal? ?i ureteral?', 'Calculi renali ?i ureterali', NULL, 0, 1, 'Moderate', 'calculi renali,pietre rinichi,litiaza'),
('N20.0', 'N20.0', 'Genito-urinar', 'Calcul renal', 'Calcul al rinichiului', 'N20', 1, 1, 'Moderate', 'calcul renal,piatra rinichi'),
('N20.1', 'N20.1', 'Genito-urinar', 'Calcul ureteral', 'Calcul al ureterului', 'N20', 1, 1, 'Moderate', 'calcul ureteral'),
('N23', 'N23', 'Genito-urinar', 'Colic? renal? nespecificat?', 'Colic? renal?, nespecificat?', NULL, 1, 1, 'Moderate', 'colica renala'),

-- Afec?iuni renale cronice
('N18', 'N18', 'Genito-urinar', 'Boal? cronic? de rinichi', 'Insuficien?? renal? cronic?', NULL, 0, 1, 'Severe', 'brc,irc,boala cronica rinichi,insuficienta renala'),
('N18.1', 'N18.1', 'Genito-urinar', 'BCR stadiul 1', 'Boal? cronic? de rinichi stadiul 1', 'N18', 1, 0, 'Mild', 'brc stadiu 1'),
('N18.2', 'N18.2', 'Genito-urinar', 'BCR stadiul 2', 'Boal? cronic? de rinichi stadiul 2 (u?oar?)', 'N18', 1, 1, 'Mild', 'brc stadiu 2'),
('N18.3', 'N18.3', 'Genito-urinar', 'BCR stadiul 3', 'Boal? cronic? de rinichi stadiul 3 (moderat?)', 'N18', 1, 1, 'Moderate', 'brc stadiu 3'),
('N18.4', 'N18.4', 'Genito-urinar', 'BCR stadiul 4', 'Boal? cronic? de rinichi stadiul 4 (sever?)', 'N18', 1, 1, 'Severe', 'brc stadiu 4'),
('N18.5', 'N18.5', 'Genito-urinar', 'BCR stadiul 5', 'Boal? cronic? de rinichi stadiul 5 (terminal?)', 'N18', 1, 1, 'Critical', 'brc stadiu 5,dializa'),

-- Prostat?
('N40', 'N40', 'Genito-urinar', 'Hiperplazie de prostat?', 'Hiperplazia benign? de prostat? (HBP)', NULL, 1, 1, 'Mild', 'hbp,prostata marita,adenom prostata'),
('N41', 'N41', 'Genito-urinar', 'Boli inflamatorii ale prostatei', 'Prostatit?', NULL, 0, 1, 'Moderate', 'prostatita'),
('N41.0', 'N41.0', 'Genito-urinar', 'Prostatit? acut?', 'Prostatit? acut?', 'N41', 1, 1, 'Moderate', 'prostatita acuta'),
('N41.1', 'N41.1', 'Genito-urinar', 'Prostatit? cronic?', 'Prostatit? cronic?', 'N41', 1, 1, 'Mild', 'prostatita cronica')
GO

-- ========================================
-- BOLI NEUROLOGICE (G00-G99)
-- ========================================
PRINT '?? Categoria: NERVOS...'

INSERT INTO ICD10_Codes (Code, FullCode, Category, ShortDescription, LongDescription, ParentCode, IsLeafNode, IsCommon, Severity, SearchTerms)
VALUES
-- Cefalee ?i migren?
('G43', 'G43', 'Nervos', 'Migren?', 'Migren? - cefalee primar?', NULL, 0, 1, 'Moderate', 'migrena,durere cap'),
('G43.0', 'G43.0', 'Nervos', 'Migren? f?r? aur?', 'Migren? f?r? aur? (comun?)', 'G43', 1, 1, 'Moderate', 'migrena fara aura,migrena comuna'),
('G43.1', 'G43.1', 'Nervos', 'Migren? cu aur?', 'Migren? cu aur? (clasic?)', 'G43', 1, 1, 'Moderate', 'migrena cu aura,migrena clasica'),
('G43.9', 'G43.9', 'Nervos', 'Migren? nespecificat?', 'Migren?, nespecificat?', 'G43', 1, 1, 'Moderate', 'migrena nespecificata'),
('G44.2', 'G44.2', 'Nervos', 'Cefalee de tip tensional', 'Cefalee de tensiune', NULL, 1, 1, 'Mild', 'cefalee tensiune,durere cap tensiune'),

-- Epilepsie
('G40', 'G40', 'Nervos', 'Epilepsie', 'Epilepsie ?i crize recurente', NULL, 0, 1, 'Severe', 'epilepsie,crize,convulsii'),
('G40.3', 'G40.3', 'Nervos', 'Epilepsie generalizat? idiopatic?', 'Epilepsie generalizat? idiopatic?', 'G40', 1, 1, 'Severe', 'epilepsie generalizata'),
('G40.9', 'G40.9', 'Nervos', 'Epilepsie nespecificat?', 'Epilepsie, nespecificat?', 'G40', 1, 1, 'Severe', 'epilepsie nespecificata'),

-- Boli vasculare cerebrale
('G45', 'G45', 'Nervos', 'Atacuri ischemice tranzitorii', 'AIT - accidente ischemice tranzitorii', NULL, 0, 1, 'Severe', 'ait,atac ischemic tranzitor,tia'),
('G45.9', 'G45.9', 'Nervos', 'AIT nespecificat', 'Atac ischemic cerebral tranzitor, nespecificat', 'G45', 1, 1, 'Severe', 'ait nespecificat'),

-- Parkinson ?i demen??
('G20', 'G20', 'Nervos', 'Boala Parkinson', 'Boala Parkinson - parkinsonism primar', NULL, 1, 1, 'Severe', 'parkinson,tremor'),
('G30', 'G30', 'Nervos', 'Boala Alzheimer', 'Demen?? Alzheimer', NULL, 0, 1, 'Severe', 'alzheimer,dementa'),
('G30.9', 'G30.9', 'Nervos', 'Boala Alzheimer nespecificat?', 'Boala Alzheimer, nespecificat?', 'G30', 1, 1, 'Severe', 'alzheimer nespecificat'),

-- Neuropatii
('G62', 'G62', 'Nervos', 'Alte polineuropatii', 'Polineuropatii', NULL, 0, 1, 'Moderate', 'polineuropatie,neuropatie'),
('G62.9', 'G62.9', 'Nervos', 'Polineuropatie nespecificat?', 'Polineuropatie, nespecificat?', 'G62', 1, 1, 'Moderate', 'polineuropatie nespecificata'),
('G63.2', 'G63.2', 'Nervos', 'Polineuropatie diabetic?', 'Polineuropatie în diabet zaharat', NULL, 1, 1, 'Moderate', 'neuropatie diabetica,polineuropatie diabet')
GO

-- ========================================
-- BOLI MUSCULO-SCHELETICE (M00-M99)
-- ========================================
PRINT '?? Categoria: MUSCULO-SCHELETIC...'

INSERT INTO ICD10_Codes (Code, FullCode, Category, ShortDescription, LongDescription, ParentCode, IsLeafNode, IsCommon, Severity, SearchTerms)
VALUES
-- Artroze
('M15', 'M15', 'Musculo-scheletic', 'Poliartroza', 'Poliartroza - artroz? în multiple articula?ii', NULL, 1, 1, 'Moderate', 'poliartroza,artroza multipla'),
('M16', 'M16', 'Musculo-scheletic', 'Coxartroza', 'Artroza ?oldului', NULL, 0, 1, 'Moderate', 'coxartroza,artroza sold'),
('M16.9', 'M16.9', 'Musculo-scheletic', 'Coxartroza nespecificat?', 'Coxartroza, nespecificat?', 'M16', 1, 1, 'Moderate', 'coxartroza nespecificata'),
('M17', 'M17', 'Musculo-scheletic', 'Gonartroza', 'Artroza genunchiului', NULL, 0, 1, 'Moderate', 'gonartroza,artroza genunchi'),
('M17.9', 'M17.9', 'Musculo-scheletic', 'Gonartroza nespecificat?', 'Gonartroza, nespecificat?', 'M17', 1, 1, 'Moderate', 'gonartroza nespecificata'),
('M19', 'M19', 'Musculo-scheletic', 'Alte artroze', 'Alte artroze', NULL, 0, 1, 'Moderate', 'artroza'),
('M19.9', 'M19.9', 'Musculo-scheletic', 'Artroza nespecificat?', 'Artroza, nespecificat?', 'M19', 1, 1, 'Moderate', 'artroza nespecificata'),

-- Dorsalgii
('M54', 'M54', 'Musculo-scheletic', 'Dorsalgie', 'Dureri de spate', NULL, 0, 1, 'Mild', 'dorsalgie,durere spate'),
('M54.2', 'M54.2', 'Musculo-scheletic', 'Cervicalgie', 'Durere cervical? - durere de gât', 'M54', 1, 1, 'Mild', 'cervicalgie,durere gat,durere cervicala'),
('M54.4', 'M54.4', 'Musculo-scheletic', 'Lumbago cu sciatic?', 'Lumbago cu sciatic?', 'M54', 1, 1, 'Moderate', 'lumbago sciatica'),
('M54.5', 'M54.5', 'Musculo-scheletic', 'Lombalgie', 'Durere lombar? joas?', 'M54', 1, 1, 'Mild', 'lombalgie,durere lombara,durere spate jos'),
('M54.9', 'M54.9', 'Musculo-scheletic', 'Dorsalgie nespecificat?', 'Dorsalgie, nespecificat?', 'M54', 1, 1, 'Mild', 'dorsalgie nespecificata'),

-- Discopatii
('M51', 'M51', 'Musculo-scheletic', 'Afec?iuni ale discurilor intervertebrale', 'Discopatii lombare', NULL, 0, 1, 'Moderate', 'discopatie,hernie disc'),
('M51.1', 'M51.1', 'Musculo-scheletic', 'Hernie de disc lombar? cu radiculopatie', 'Hernie de disc lombar? cu afectare nervoas?', 'M51', 1, 1, 'Moderate', 'hernie disc lombara,radiculopatie'),
('M51.2', 'M51.2', 'Musculo-scheletic', 'Degenerare disc intervertebral', 'Degenerarea discului intervertebral', 'M51', 1, 1, 'Mild', 'degenerare disc'),

-- Osteoporoza
('M81', 'M81', 'Musculo-scheletic', 'Osteoporoz?', 'Osteoporoz? f?r? fractura patologic?', NULL, 0, 1, 'Moderate', 'osteoporoza'),
('M81.0', 'M81.0', 'Musculo-scheletic', 'Osteoporoz? postmenopauzal?', 'Osteoporoz? postmenopauzal?', 'M81', 1, 1, 'Moderate', 'osteoporoza postmenopauza'),
('M81.8', 'M81.8', 'Musculo-scheletic', 'Alte osteoporoze', 'Alte osteoporoze', 'M81', 1, 1, 'Moderate', 'osteoporoza alta')
GO

-- ========================================
-- SIMPTOME ?I SEMNE (R00-R99)
-- ========================================
PRINT '?? Categoria: SIMPTOME...'

INSERT INTO ICD10_Codes (Code, FullCode, Category, ShortDescription, LongDescription, ParentCode, IsLeafNode, IsCommon, Severity, SearchTerms)
VALUES
-- Simptome cardiace
('R00', 'R00', 'Simptome', 'Anomalii ale b?t?ilor inimii', 'Anomalii ale b?t?ilor cardiace', NULL, 0, 1, 'Mild', 'palpitatie,batai neregulate'),
('R00.0', 'R00.0', 'Simptome', 'Tahicardie nespecificat?', 'Tahicardie, nespecificat?', 'R00', 1, 1, 'Mild', 'tahicardie'),
('R00.1', 'R00.1', 'Simptome', 'Bradicardie nespecificat?', 'Bradicardie, nespecificat?', 'R00', 1, 1, 'Mild', 'bradicardie'),
('R00.2', 'R00.2', 'Simptome', 'Palpita?ii', 'Palpita?ii', 'R00', 1, 1, 'Mild', 'palpitatii'),

-- Simptome respiratorii
('R05', 'R05', 'Simptome', 'Tuse', 'Tuse', NULL, 1, 1, 'Mild', 'tuse'),
('R06.0', 'R06.0', 'Simptome', 'Dispnee', 'Dispnee - dificultate de respira?ie', NULL, 1, 1, 'Moderate', 'dispnee,greu respir,sufocare'),

-- Durere
('R10', 'R10', 'Simptome', 'Durere abdominal? ?i pelvin?', 'Durere abdominal? ?i pelvin?', NULL, 0, 1, 'Mild', 'durere abdomen,durere burta'),
('R10.1', 'R10.1', 'Simptome', 'Durere epigastric?', 'Durere în regiunea superioar? a abdomenului', 'R10', 1, 1, 'Mild', 'durere epigastrica,durere stomac'),
('R10.4', 'R10.4', 'Simptome', 'Durere abdominal? nespecificat?', 'Durere abdominal?, nespecificat?', 'R10', 1, 1, 'Mild', 'durere abdominala'),

-- Febr?
('R50', 'R50', 'Simptome', 'Febr? de cauz? necunoscut?', 'Febr? de origine necunoscut?', NULL, 0, 1, 'Mild', 'febra,temperatura'),
('R50.9', 'R50.9', 'Simptome', 'Febr? nespecificat?', 'Febr?, nespecificat?', 'R50', 1, 1, 'Mild', 'febra nespecificata'),

-- Cefalee
('R51', 'R51', 'Simptome', 'Cefalee', 'Cefalee - durere de cap', NULL, 1, 1, 'Mild', 'cefalee,durere cap'),

-- Vertij
('R42', 'R42', 'Simptome', 'Ame?eal? ?i vertij', 'Ame?eal? ?i vertij', NULL, 1, 1, 'Mild', 'vertij,ameteala'),

-- Oboseal?
('R53', 'R53', 'Simptome', 'Stare de r?u ?i oboseal?', 'Stare de r?u ?i oboseal?', NULL, 1, 1, 'Mild', 'oboseala,astenie,slabiciune'),

-- Sincopa
('R55', 'R55', 'Simptome', 'Sincop? ?i colaps', 'Le?in - pierdere temporar? a cuno?tin?ei', NULL, 1, 1, 'Moderate', 'sincopa,lesin,colaps')
GO

-- ========================================
-- BOLI INFEC?IOASE COMUNE (A00-B99)
-- ========================================
PRINT '?? Categoria: INFECTIOASE...'

INSERT INTO ICD10_Codes (Code, FullCode, Category, ShortDescription, LongDescription, ParentCode, IsLeafNode, IsCommon, Severity, SearchTerms)
VALUES
-- Infec?ii respiratorii
('J00', 'J00', 'Infectioase', 'Rinofaringit? acut? (R?ceal?)', 'R?ceal? comun? - rinofaringit? acut?', NULL, 1, 1, 'Mild', 'raceala,guturai,nas infundat'),
('J02', 'J02', 'Infectioase', 'Faringit? acut?', 'Durere în gât - faringit?', NULL, 0, 1, 'Mild', 'faringita,durere gat'),
('J02.9', 'J02.9', 'Infectioase', 'Faringit? acut? nespecificat?', 'Faringit? acut?, nespecificat?', 'J02', 1, 1, 'Mild', 'faringita acuta'),
('J03', 'J03', 'Infectioase', 'Amigdalit? acut?', 'Angin? - amigdalit? acut?', NULL, 0, 1, 'Mild', 'amigdalita,angina'),
('J03.9', 'J03.9', 'Infectioase', 'Amigdalit? acut? nespecificat?', 'Amigdalit? acut?, nespecificat?', 'J03', 1, 1, 'Mild', 'amigdalita acuta'),
('J06', 'J06', 'Infectioase', 'Infec?ii acute ale c?ilor respiratorii superioare', 'IACRS - infec?ie respiratorie superioar?', NULL, 0, 1, 'Mild', 'iacrs,infectie respiratorie'),
('J06.9', 'J06.9', 'Infectioase', 'Infec?ie respiratorie superioar? nespecificat?', 'Infec?ie respiratorie superioar?, nespecificat?', 'J06', 1, 1, 'Mild', 'iacrs nespecificata'),

-- Bron?it? ?i pneumonie
('J20', 'J20', 'Infectioase', 'Bron?it? acut?', 'Bron?it? acut?', NULL, 0, 1, 'Mild', 'bronsita acuta'),
('J20.9', 'J20.9', 'Infectioase', 'Bron?it? acut? nespecificat?', 'Bron?it? acut?, nespecificat?', 'J20', 1, 1, 'Mild', 'bronsita acuta nespecificata'),
('J18', 'J18', 'Infectioase', 'Pneumonie', 'Pneumonie - infec?ie pulmonar?', NULL, 0, 1, 'Moderate', 'pneumonie'),
('J18.9', 'J18.9', 'Infectioase', 'Pneumonie nespecificat?', 'Pneumonie, nespecificat?', 'J18', 1, 1, 'Moderate', 'pneumonie nespecificata'),

-- Gastroenterite
('A09', 'A09', 'Infectioase', 'Gastroenterit? infec?ioas?', 'Diaree ?i gastroenterit? de origine infec?ioas?', NULL, 1, 1, 'Mild', 'gastroenterita,diaree,varsaturi'),

-- COVID-19
('U07.1', 'U07.1', 'Infectioase', 'COVID-19 confirmat', 'COVID-19 confirmat prin test', NULL, 1, 1, 'Moderate', 'covid,covid19,coronavirus,sarscov2'),
('U07.2', 'U07.2', 'Infectioase', 'COVID-19 suspectat', 'COVID-19, virus neidentificat', NULL, 1, 0, 'Moderate', 'covid suspectat')
GO

-- ========================================
-- BOLI MENTALE ?I COMPORTAMENTALE (F00-F99)
-- ========================================
PRINT '?? Categoria: MENTAL...'

INSERT INTO ICD10_Codes (Code, FullCode, Category, ShortDescription, LongDescription, ParentCode, IsLeafNode, IsCommon, Severity, SearchTerms)
VALUES
-- Anxietate
('F41', 'F41', 'Mental', 'Alte tulbur?ri anxioase', 'Tulbur?ri anxioase', NULL, 0, 1, 'Moderate', 'anxietate,neliniste'),
('F41.0', 'F41.0', 'Mental', 'Tulburare de panic?', 'Tulburare de panic? (anxietate paroxistic? episodic?)', 'F41', 1, 1, 'Moderate', 'panica,atac panica'),
('F41.1', 'F41.1', 'Mental', 'Tulburare anxioas? generalizat?', 'Anxietate generalizat?', 'F41', 1, 1, 'Moderate', 'anxietate generalizata,tag'),
('F41.9', 'F41.9', 'Mental', 'Tulburare anxioas? nespecificat?', 'Anxietate, nespecificat?', 'F41', 1, 1, 'Mild', 'anxietate nespecificata'),

-- Depresie
('F32', 'F32', 'Mental', 'Episod depresiv', 'Episod depresiv', NULL, 0, 1, 'Moderate', 'depresie,tristete'),
('F32.0', 'F32.0', 'Mental', 'Episod depresiv u?or', 'Episod depresiv u?or', 'F32', 1, 1, 'Mild', 'depresie usoara'),
('F32.1', 'F32.1', 'Mental', 'Episod depresiv moderat', 'Episod depresiv moderat', 'F32', 1, 1, 'Moderate', 'depresie moderata'),
('F32.2', 'F32.2', 'Mental', 'Episod depresiv sever f?r? simptome psihotice', 'Episod depresiv major', 'F32', 1, 1, 'Severe', 'depresie severa,depresie majora'),
('F32.9', 'F32.9', 'Mental', 'Episod depresiv nespecificat', 'Depresie, nespecificat?', 'F32', 1, 1, 'Moderate', 'depresie nespecificata'),
('F33', 'F33', 'Mental', 'Tulburare depresiv? recurent?', 'Depresie recurent?', NULL, 0, 1, 'Moderate', 'depresie recurenta'),
('F33.9', 'F33.9', 'Mental', 'Tulburare depresiv? recurent? nespecificat?', 'Depresie recurent?, nespecificat?', 'F33', 1, 1, 'Moderate', 'depresie recurenta nespecificata'),

-- Tulbur?ri de somn
('F51', 'F51', 'Mental', 'Tulbur?ri de somn neorganice', 'Insomnie ?i alte tulbur?ri de somn', NULL, 0, 1, 'Mild', 'insomnie,somn'),
('F51.0', 'F51.0', 'Mental', 'Insomnie neorganic?', 'Insomnie', 'F51', 1, 1, 'Mild', 'insomnie'),

-- Stres
('F43', 'F43', 'Mental', 'Reac?ie la stres sever ?i tulbur?ri de adaptare', 'Reac?ie la stres', NULL, 0, 1, 'Moderate', 'stres,trauma'),
('F43.0', 'F43.0', 'Mental', 'Reac?ie acut? la stres', 'Reac?ie acut? la stres', 'F43', 1, 1, 'Moderate', 'stres acut'),
('F43.2', 'F43.2', 'Mental', 'Tulbur?ri de adaptare', 'Tulbur?ri de adaptare', 'F43', 1, 1, 'Mild', 'tulburare adaptare')
GO

-- ========================================
-- STATISTICI FINALE
-- ========================================
PRINT ''
PRINT '=========================================='
PRINT '? INSERT COMPLET!'
PRINT '=========================================='

SELECT 
    'TOTAL' AS Tip,
    COUNT(*) AS NumarCoduri
FROM ICD10_Codes

UNION ALL

SELECT 
    Category AS Tip,
    COUNT(*) AS NumarCoduri
FROM ICD10_Codes
GROUP BY Category
ORDER BY NumarCoduri DESC

PRINT ''
PRINT '?? Statistici pe categorii:'
GO
