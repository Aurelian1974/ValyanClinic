/*
==============================================================================
DATE INIȚIALE PENTRU NOMENCLATOARE INVESTIGAȚII MEDICALE
==============================================================================
Description: Populare nomenclatoare cu investigații imagistice, explorări 
             funcționale și endoscopii
Author: AI Agent
Date: 2026-01-06
Version: 1.0
==============================================================================
*/

USE [ValyanMed]
GO

-- ============================================================================
-- 1. NOMENCLATOR INVESTIGAȚII IMAGISTICE
-- ============================================================================
PRINT 'Populare NomenclatorInvestigatiiImagistice...'

-- Clear existing data (optional - comment out if you want to keep existing)
-- DELETE FROM dbo.NomenclatorInvestigatiiImagistice

IF NOT EXISTS (SELECT 1 FROM dbo.NomenclatorInvestigatiiImagistice)
BEGIN
    INSERT INTO dbo.NomenclatorInvestigatiiImagistice 
        (Cod, Denumire, Descriere, Categorie, RequiresContrast, PreparationInstructions, EstimatedDuration, IsActive, OrdineAfisare)
    VALUES
    -- RADIOGRAFIE
    ('RAD-001', 'Radiografie toracică PA', 'Radiografie torace proiecție postero-anterioară', 'Radiografie', 0, NULL, 10, 1, 1),
    ('RAD-002', 'Radiografie toracică profil', 'Radiografie torace proiecție de profil', 'Radiografie', 0, NULL, 10, 1, 2),
    ('RAD-003', 'Radiografie abdominală simplă', 'Radiografie abdomen pe gol', 'Radiografie', 0, NULL, 10, 1, 3),
    ('RAD-004', 'Radiografie coloană cervicală', 'Radiografie coloană cervicală în 2 incidențe', 'Radiografie', 0, NULL, 15, 1, 4),
    ('RAD-005', 'Radiografie coloană dorsală', 'Radiografie coloană toracică în 2 incidențe', 'Radiografie', 0, NULL, 15, 1, 5),
    ('RAD-006', 'Radiografie coloană lombară', 'Radiografie coloană lombară în 2 incidențe', 'Radiografie', 0, NULL, 15, 1, 6),
    ('RAD-007', 'Radiografie bazin', 'Radiografie bazin față', 'Radiografie', 0, NULL, 10, 1, 7),
    ('RAD-008', 'Radiografie genunchi', 'Radiografie genunchi în 2 incidențe', 'Radiografie', 0, NULL, 15, 1, 8),
    ('RAD-009', 'Radiografie gleznă', 'Radiografie gleznă în 2 incidențe', 'Radiografie', 0, NULL, 15, 1, 9),
    ('RAD-010', 'Radiografie mână', 'Radiografie mână în 2 incidențe', 'Radiografie', 0, NULL, 15, 1, 10),
    ('RAD-011', 'Radiografie umăr', 'Radiografie umăr în 2 incidențe', 'Radiografie', 0, NULL, 15, 1, 11),
    ('RAD-012', 'Radiografie craniu', 'Radiografie craniu în 2 incidențe', 'Radiografie', 0, NULL, 15, 1, 12),
    ('RAD-013', 'Radiografie sinusuri', 'Radiografie sinusuri paranazale', 'Radiografie', 0, NULL, 10, 1, 13),
    ('RAD-014', 'Ortopantomogramă', 'Radiografie panoramică dentară', 'Radiografie', 0, NULL, 10, 1, 14),
    
    -- ECOGRAFIE
    ('ECO-001', 'Ecografie abdominală completă', 'Ecografie abdomen superior și inferior', 'Ecografie', 0, 'A jeun minim 6 ore', 30, 1, 20),
    ('ECO-002', 'Ecografie hepato-biliară', 'Ecografie ficat, vezică biliară, căi biliare', 'Ecografie', 0, 'A jeun minim 6 ore', 20, 1, 21),
    ('ECO-003', 'Ecografie renală', 'Ecografie rinichi și căi urinare', 'Ecografie', 0, 'Vezică plină', 20, 1, 22),
    ('ECO-004', 'Ecografie pelvină', 'Ecografie bazin', 'Ecografie', 0, 'Vezică plină', 20, 1, 23),
    ('ECO-005', 'Ecografie tiroidiană', 'Ecografie glandă tiroidă', 'Ecografie', 0, NULL, 15, 1, 24),
    ('ECO-006', 'Ecografie mamară', 'Ecografie sân bilateral', 'Ecografie', 0, NULL, 20, 1, 25),
    ('ECO-007', 'Ecografie testiculară', 'Ecografie scrotală', 'Ecografie', 0, NULL, 15, 1, 26),
    ('ECO-008', 'Ecografie prostatică transabdominală', 'Ecografie prostată abordaj abdominal', 'Ecografie', 0, 'Vezică plină', 20, 1, 27),
    ('ECO-009', 'Ecografie Doppler carotidian', 'Eco Doppler artere carotide', 'Ecografie', 0, NULL, 30, 1, 28),
    ('ECO-010', 'Ecografie Doppler venos membre inferioare', 'Eco Doppler vene membre inferioare', 'Ecografie', 0, NULL, 30, 1, 29),
    ('ECO-011', 'Ecografie Doppler arterial membre inferioare', 'Eco Doppler artere membre inferioare', 'Ecografie', 0, NULL, 30, 1, 30),
    ('ECO-012', 'Ecografie părți moi', 'Ecografie țesuturi moi superficiale', 'Ecografie', 0, NULL, 15, 1, 31),
    ('ECO-013', 'Ecografie articulară', 'Ecografie articulație', 'Ecografie', 0, NULL, 20, 1, 32),
    ('ECO-014', 'Ecocardiografie transtoracică', 'Ecografie cardiacă transtoracică', 'Ecografie', 0, NULL, 30, 1, 33),
    
    -- CT (COMPUTER TOMOGRAFIE)
    ('CT-001', 'CT craniu nativ', 'Computer tomografie craniu fără substanță de contrast', 'CT', 0, NULL, 15, 1, 40),
    ('CT-002', 'CT craniu cu contrast', 'Computer tomografie craniu cu substanță de contrast', 'CT', 1, 'Creatinină recentă', 30, 1, 41),
    ('CT-003', 'CT torace nativ', 'CT torace fără contrast', 'CT', 0, NULL, 15, 1, 42),
    ('CT-004', 'CT torace cu contrast', 'CT torace cu substanță de contrast', 'CT', 1, 'Creatinină recentă', 30, 1, 43),
    ('CT-005', 'CT abdomen-pelvis nativ', 'CT abdomen și pelvis fără contrast', 'CT', 0, NULL, 20, 1, 44),
    ('CT-006', 'CT abdomen-pelvis cu contrast', 'CT abdomen și pelvis cu contrast', 'CT', 1, 'Creatinină recentă', 40, 1, 45),
    ('CT-007', 'CT coloană cervicală', 'CT coloană cervicală', 'CT', 0, NULL, 15, 1, 46),
    ('CT-008', 'CT coloană lombară', 'CT coloană lombară', 'CT', 0, NULL, 15, 1, 47),
    ('CT-009', 'Angio-CT cerebral', 'Angio CT artere cerebrale', 'CT', 1, 'Creatinină recentă', 30, 1, 48),
    ('CT-010', 'Angio-CT toracic', 'Angio CT artere pulmonare și aortă', 'CT', 1, 'Creatinină recentă', 30, 1, 49),
    ('CT-011', 'Angio-CT abdominal', 'Angio CT aortă abdominală și ramuri', 'CT', 1, 'Creatinină recentă', 30, 1, 50),
    ('CT-012', 'CT cardiac (Calcium Score)', 'CT cardiac pentru scor calciu coronarian', 'CT', 0, NULL, 20, 1, 51),
    ('CT-013', 'Coronarografie CT', 'CT coronariană cu contrast', 'CT', 1, 'Creatinină, frecvență cardiacă <65/min', 45, 1, 52),
    
    -- RMN (REZONANȚĂ MAGNETICĂ)
    ('RMN-001', 'RMN cerebral nativ', 'Rezonanță magnetică cerebrală fără contrast', 'RMN', 0, NULL, 30, 1, 60),
    ('RMN-002', 'RMN cerebral cu contrast', 'Rezonanță magnetică cerebrală cu contrast', 'RMN', 1, 'Creatinină recentă', 45, 1, 61),
    ('RMN-003', 'RMN coloană cervicală', 'RMN coloană cervicală', 'RMN', 0, NULL, 30, 1, 62),
    ('RMN-004', 'RMN coloană dorsală', 'RMN coloană toracică', 'RMN', 0, NULL, 30, 1, 63),
    ('RMN-005', 'RMN coloană lombară', 'RMN coloană lombară', 'RMN', 0, NULL, 30, 1, 64),
    ('RMN-006', 'RMN genunchi', 'RMN articulație genunchi', 'RMN', 0, NULL, 30, 1, 65),
    ('RMN-007', 'RMN umăr', 'RMN articulație umăr', 'RMN', 0, NULL, 30, 1, 66),
    ('RMN-008', 'RMN șold', 'RMN articulație șold', 'RMN', 0, NULL, 30, 1, 67),
    ('RMN-009', 'RMN abdomen', 'RMN abdominal', 'RMN', 0, NULL, 40, 1, 68),
    ('RMN-010', 'RMN cardiac', 'RMN cardiac', 'RMN', 0, NULL, 45, 1, 69),
    ('RMN-011', 'RMN pelvis', 'RMN pelvis', 'RMN', 0, NULL, 40, 1, 70),
    ('RMN-012', 'RMN prostată multiparametrică', 'RMN prostată mpMRI', 'RMN', 0, NULL, 45, 1, 71),
    ('RMN-013', 'RMN mamar', 'RMN sân bilateral', 'RMN', 1, NULL, 45, 1, 72),
    
    -- MAMOGRAFIE
    ('MAM-001', 'Mamografie bilaterală', 'Mamografie diagnostic ambii sâni', 'Mamografie', 0, NULL, 15, 1, 80),
    ('MAM-002', 'Mamografie screening', 'Mamografie de screening', 'Mamografie', 0, NULL, 10, 1, 81),
    
    -- SCINTIGRAFIE
    ('SCINT-001', 'Scintigrafie osoasă', 'Scintigrafie osoasă corp întreg', 'Scintigrafie', 0, 'Hidratare adecvată', 180, 1, 90),
    ('SCINT-002', 'Scintigrafie tiroidiană', 'Scintigrafie glandă tiroidă', 'Scintigrafie', 0, NULL, 30, 1, 91),
    ('SCINT-003', 'Scintigrafie renală', 'Scintigrafie renală (DTPA/MAG3)', 'Scintigrafie', 0, 'Hidratare', 60, 1, 92),
    ('SCINT-004', 'Scintigrafie miocardică (SPECT)', 'Scintigrafie perfuzie miocardică', 'Scintigrafie', 0, 'A jeun, fără cafea', 240, 1, 93),
    
    -- PET-CT
    ('PET-001', 'PET-CT corp întreg', 'PET-CT FDG corp întreg', 'PET-CT', 0, 'A jeun 6h, glicemie<150', 120, 1, 100),
    ('PET-002', 'PET-CT cerebral', 'PET-CT cerebral', 'PET-CT', 0, 'A jeun 6h', 90, 1, 101);
    
    PRINT '✓ Inserate ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistrări în NomenclatorInvestigatiiImagistice'
END
ELSE
    PRINT '→ NomenclatorInvestigatiiImagistice deja populat'
GO

-- ============================================================================
-- 2. NOMENCLATOR EXPLORĂRI FUNCȚIONALE
-- ============================================================================
PRINT 'Populare NomenclatorExplorariFunc...'

IF NOT EXISTS (SELECT 1 FROM dbo.NomenclatorExplorariFunc)
BEGIN
    INSERT INTO dbo.NomenclatorExplorariFunc 
        (Cod, Denumire, Descriere, Categorie, PreparationInstructions, EstimatedDuration, IsActive, OrdineAfisare)
    VALUES
    -- CARDIOLOGIE
    ('CARD-001', 'Electrocardiogramă (EKG)', 'EKG standard 12 derivații', 'Cardiologie', NULL, 10, 1, 1),
    ('CARD-002', 'EKG de efort', 'Test de efort pe cicloergometru sau covor rulant', 'Cardiologie', 'Fără betablocante 24h', 45, 1, 2),
    ('CARD-003', 'Holter EKG 24h', 'Monitorizare EKG 24 ore', 'Cardiologie', NULL, 30, 1, 3),
    ('CARD-004', 'Holter EKG 48h', 'Monitorizare EKG 48 ore', 'Cardiologie', NULL, 30, 1, 4),
    ('CARD-005', 'Holter EKG 7 zile', 'Monitorizare EKG extinsă', 'Cardiologie', NULL, 30, 1, 5),
    ('CARD-006', 'Holter TA 24h', 'Monitorizare tensiune arterială 24h', 'Cardiologie', NULL, 20, 1, 6),
    ('CARD-007', 'Ecocardiografie transesofagiană', 'Echo cord transesofagian', 'Cardiologie', 'A jeun 6h', 45, 1, 7),
    ('CARD-008', 'Ecocardiografie de stres', 'Echo stress cu dobutamină sau efort', 'Cardiologie', 'A jeun, fără cafea', 60, 1, 8),
    ('CARD-009', 'Tilt test', 'Test de înclinare ortostatică', 'Cardiologie', 'A jeun', 60, 1, 9),
    
    -- PNEUMOLOGIE
    ('PNEU-001', 'Spirometrie simplă', 'Spirometrie bazală', 'Pneumologie', 'Fără bronhodilatator 6h', 20, 1, 20),
    ('PNEU-002', 'Spirometrie cu test bronhodilatator', 'Spirometrie pre și post bronhodilatator', 'Pneumologie', 'Fără bronhodilatator 6h', 30, 1, 21),
    ('PNEU-003', 'Pletismografie', 'Pletismografie corporală', 'Pneumologie', NULL, 30, 1, 22),
    ('PNEU-004', 'Test difuziune CO (DLCO)', 'Capacitate de difuziune a CO', 'Pneumologie', NULL, 20, 1, 23),
    ('PNEU-005', 'Test de provocare bronșică', 'Test cu metacolină', 'Pneumologie', 'Fără antihistaminice 7 zile', 60, 1, 24),
    ('PNEU-006', 'Polisomnografie', 'Studiu de somn complet', 'Pneumologie', NULL, 480, 1, 25),
    ('PNEU-007', 'Poligrafie respiratorie', 'Studiu simplificat al somnului', 'Pneumologie', NULL, 480, 1, 26),
    ('PNEU-008', 'Oximetrie nocturnă', 'Monitorizare saturație O2 nocturnă', 'Pneumologie', NULL, 480, 1, 27),
    ('PNEU-009', 'Test mers 6 minute', 'Test de efort submaximal', 'Pneumologie', 'Îmbrăcăminte comodă', 15, 1, 28),
    ('PNEU-010', 'FeNO', 'Oxid nitric expirat fracționat', 'Pneumologie', 'A jeun 1h', 10, 1, 29),
    
    -- NEUROLOGIE
    ('NEUR-001', 'Electroencefalogramă (EEG)', 'EEG standard', 'Neurologie', 'Păr curat, fără lac', 45, 1, 40),
    ('NEUR-002', 'EEG cu privare de somn', 'EEG după privare de somn', 'Neurologie', 'Privare somn conform indicație', 60, 1, 41),
    ('NEUR-003', 'Video-EEG monitorizare', 'Monitorizare video-EEG continuă', 'Neurologie', NULL, 1440, 1, 42),
    ('NEUR-004', 'Electromiografie (EMG)', 'Electromiografie cu ac', 'Neurologie', NULL, 45, 1, 43),
    ('NEUR-005', 'Viteze de conducere nervoasă', 'Electroneurografie', 'Neurologie', NULL, 30, 1, 44),
    ('NEUR-006', 'Potențiale evocate vizuale', 'PEV', 'Neurologie', NULL, 30, 1, 45),
    ('NEUR-007', 'Potențiale evocate auditive', 'PEATC', 'Neurologie', NULL, 30, 1, 46),
    ('NEUR-008', 'Potențiale evocate somatosenzoriale', 'PESS', 'Neurologie', NULL, 45, 1, 47),
    ('NEUR-009', 'Doppler transcranean', 'Ultrasonografie Doppler transcraniană', 'Neurologie', NULL, 30, 1, 48),
    
    -- GASTROENTEROLOGIE
    ('GAST-001', 'Manometrie esofagiană', 'Manometrie esofag de înaltă rezoluție', 'Gastroenterologie', 'A jeun 8h', 45, 1, 60),
    ('GAST-002', 'pH-metrie esofagiană 24h', 'Monitorizare pH esofagian', 'Gastroenterologie', 'Fără IPP 7 zile', 30, 1, 61),
    ('GAST-003', 'Impedanță-pH metrie', 'Monitorizare impedanță și pH', 'Gastroenterologie', 'Fără IPP 7 zile', 30, 1, 62),
    ('GAST-004', 'Test respirator Helicobacter', 'Test ureează expirat H. pylori', 'Gastroenterologie', 'A jeun, fără IPP/antibiotice 2 săpt', 20, 1, 63),
    ('GAST-005', 'Test respirator lactoză', 'Test respirator intoleranță lactoză', 'Gastroenterologie', 'A jeun 12h', 180, 1, 64),
    ('GAST-006', 'Test respirator fructoză', 'Test respirator intoleranță fructoză', 'Gastroenterologie', 'A jeun 12h', 180, 1, 65),
    ('GAST-007', 'Manometrie anorectală', 'Manometrie anorectală', 'Gastroenterologie', 'Clismă', 45, 1, 66),
    ('GAST-008', 'FibroScan (elastografie hepatică)', 'Elastografie hepatică tranzitorie', 'Gastroenterologie', 'A jeun 2h', 15, 1, 67),
    
    -- ORL
    ('ORL-001', 'Audiometrie tonală', 'Audiometrie tonală liminară', 'ORL', NULL, 20, 1, 80),
    ('ORL-002', 'Audiometrie vocală', 'Audiometrie vocală', 'ORL', NULL, 20, 1, 81),
    ('ORL-003', 'Timpanometrie', 'Impedancemetrie', 'ORL', NULL, 10, 1, 82),
    ('ORL-004', 'Potențiale evocate auditive', 'BERA/ABR', 'ORL', NULL, 45, 1, 83),
    ('ORL-005', 'Electronistagmografie', 'ENG', 'ORL', 'Fără sedative', 60, 1, 84),
    ('ORL-006', 'Videonistagmografie', 'VNG', 'ORL', 'Fără sedative', 60, 1, 85),
    ('ORL-007', 'Rinomanometrie', 'Rinomanometrie anterioară', 'ORL', 'Fără decongestionant', 20, 1, 86),
    
    -- OFTALMOLOGIE
    ('OFT-001', 'Campimetrie computerizată', 'Câmp vizual computerizat', 'Oftalmologie', NULL, 30, 1, 100),
    ('OFT-002', 'OCT retinian', 'Tomografie în coerență optică retină', 'Oftalmologie', NULL, 20, 1, 101),
    ('OFT-003', 'OCT nerv optic', 'OCT disc optic și fibre nervoase', 'Oftalmologie', NULL, 20, 1, 102),
    ('OFT-004', 'Angiografie retiniană', 'Angiografie fluoresceină', 'Oftalmologie', NULL, 30, 1, 103),
    ('OFT-005', 'Electroretinogramă', 'ERG', 'Oftalmologie', 'Adaptare întuneric', 45, 1, 104),
    ('OFT-006', 'Pachimetrie corneană', 'Măsurare grosime cornee', 'Oftalmologie', 'Fără lentile contact 24h', 15, 1, 105),
    ('OFT-007', 'Topografie corneană', 'Keratometrie computerizată', 'Oftalmologie', 'Fără lentile contact 24h', 20, 1, 106),
    
    -- UROLOGIE
    ('URO-001', 'Uroflowmetrie', 'Debitmetrie urinară', 'Urologie', 'Vezică plină', 15, 1, 120),
    ('URO-002', 'Cistometrie', 'Studiu urodinamic', 'Urologie', NULL, 45, 1, 121),
    ('URO-003', 'Studiu presiune-flux', 'Studiu urodinamic complet', 'Urologie', NULL, 60, 1, 122),
    ('URO-004', 'Electromiografie sfincterică', 'EMG sfincter urinar', 'Urologie', NULL, 30, 1, 123);
    
    PRINT '✓ Inserate ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistrări în NomenclatorExplorariFunc'
END
ELSE
    PRINT '→ NomenclatorExplorariFunc deja populat'
GO

-- ============================================================================
-- 3. NOMENCLATOR ENDOSCOPII
-- ============================================================================
PRINT 'Populare NomenclatorEndoscopii...'

IF NOT EXISTS (SELECT 1 FROM dbo.NomenclatorEndoscopii)
BEGIN
    INSERT INTO dbo.NomenclatorEndoscopii 
        (Cod, Denumire, Descriere, Categorie, RequiresSedation, PreparationInstructions, EstimatedDuration, IsActive, OrdineAfisare)
    VALUES
    -- DIGESTIV
    ('DIG-001', 'Esofagogastroduodenoscopie (EDS)', 'Endoscopie digestivă superioară', 'Digestiv', 0, 'A jeun 8h', 15, 1, 1),
    ('DIG-002', 'EDS cu biopsie', 'EDS cu prelevare biopsii', 'Digestiv', 0, 'A jeun 8h', 20, 1, 2),
    ('DIG-003', 'Colonoscopie totală', 'Colonoscopie completă', 'Digestiv', 1, 'Pregătire colon conform protocol', 45, 1, 3),
    ('DIG-004', 'Colonoscopie cu polipectomie', 'Colonoscopie cu excizie polipi', 'Digestiv', 1, 'Pregătire colon conform protocol', 60, 1, 4),
    ('DIG-005', 'Rectosigmoidoscopie', 'Endoscopie rect și sigmoid', 'Digestiv', 0, 'Clismă evacuatorie', 20, 1, 5),
    ('DIG-006', 'Anuscopie', 'Examen endoscopic anus', 'Digestiv', 0, NULL, 10, 1, 6),
    ('DIG-007', 'ERCP', 'Colangiopancreatografie retrogradă endoscopică', 'Digestiv', 1, 'A jeun 8h, Antibiotice profilactic', 60, 1, 7),
    ('DIG-008', 'Ecoendoscopie digestivă', 'Ultrasonografie endoscopică', 'Digestiv', 1, 'A jeun 8h', 45, 1, 8),
    ('DIG-009', 'Enteroscopie', 'Enteroscopie cu dublu balon', 'Digestiv', 1, 'A jeun 8h', 90, 1, 9),
    ('DIG-010', 'Capsula endoscopică', 'Endoscopie capsulă intestin subțire', 'Digestiv', 0, 'A jeun 12h', 480, 1, 10),
    ('DIG-011', 'Ligatura varice esofagiene', 'Tratament endoscopic varice esofag', 'Digestiv', 1, 'A jeun 8h', 30, 1, 11),
    ('DIG-012', 'PEG (gastrostomă percutană)', 'Gastrostomă endoscopică percutană', 'Digestiv', 1, 'A jeun 8h', 45, 1, 12),
    
    -- RESPIRATOR
    ('RESP-001', 'Bronhoscopie flexibilă', 'Fibrobronhoscopie', 'Respirator', 0, 'A jeun 6h', 30, 1, 20),
    ('RESP-002', 'Bronhoscopie cu biopsie', 'Bronhoscopie cu biopsii bronșice', 'Respirator', 0, 'A jeun 6h, coagulogramă', 45, 1, 21),
    ('RESP-003', 'Bronhoscopie cu BAL', 'Bronhoscopie cu lavaj bronhoalveolar', 'Respirator', 0, 'A jeun 6h', 45, 1, 22),
    ('RESP-004', 'Bronhoscopie rigidă', 'Bronhoscopie rigidă terapeutică', 'Respirator', 1, 'A jeun 8h, AG', 60, 1, 23),
    ('RESP-005', 'EBUS', 'Bronhoscopie cu ultrasonografie endobronșică', 'Respirator', 1, 'A jeun 6h', 60, 1, 24),
    ('RESP-006', 'Toracoscopie medicală', 'Pleuroscopie', 'Respirator', 1, 'A jeun 8h', 60, 1, 25),
    
    -- UROLOGIC
    ('URO-001', 'Cistoscopie', 'Endoscopie vezică urinară', 'Urologic', 0, NULL, 15, 1, 40),
    ('URO-002', 'Cistoscopie cu biopsie', 'Cistoscopie cu biopsii vezicale', 'Urologic', 0, NULL, 30, 1, 41),
    ('URO-003', 'Ureteroscopie', 'Endoscopie ureter', 'Urologic', 1, 'A jeun 8h', 60, 1, 42),
    ('URO-004', 'Ureteroscopie cu litotriție', 'Ureteroscopie cu fragmentare calcul', 'Urologic', 1, 'A jeun 8h', 90, 1, 43),
    ('URO-005', 'Rezecție transuretrală prostată (TURP)', 'Rezecție endoscopică prostată', 'Urologic', 1, 'A jeun 8h', 90, 1, 44),
    ('URO-006', 'Rezecție transuretrală vezică (TURV)', 'Rezecție endoscopică tumori vezicale', 'Urologic', 1, 'A jeun 8h', 60, 1, 45),
    
    -- GINECOLOGIC
    ('GIN-001', 'Histeroscopie diagnostică', 'Endoscopie cavitate uterină diagnostic', 'Ginecologic', 0, 'Post-menstrual', 20, 1, 60),
    ('GIN-002', 'Histeroscopie operatorie', 'Histeroscopie cu intervenție', 'Ginecologic', 1, 'Post-menstrual', 45, 1, 61),
    ('GIN-003', 'Colposcopie', 'Examen col uterin cu magnificație', 'Ginecologic', 0, NULL, 20, 1, 62),
    ('GIN-004', 'Colposcopie cu biopsie', 'Colposcopie cu biopsie cervicală', 'Ginecologic', 0, NULL, 30, 1, 63),
    ('GIN-005', 'Laparoscopie diagnostică', 'Laparoscopie pelviană diagnostică', 'Ginecologic', 1, 'A jeun 8h', 45, 1, 64),
    
    -- ARTICULAR
    ('ART-001', 'Artroscopie genunchi diagnostică', 'Artroscopie genunchi exploratore', 'Articular', 1, 'A jeun 8h', 30, 1, 80),
    ('ART-002', 'Artroscopie genunchi terapeutică', 'Artroscopie genunchi cu intervenție', 'Articular', 1, 'A jeun 8h', 60, 1, 81),
    ('ART-003', 'Artroscopie umăr', 'Artroscopie umăr', 'Articular', 1, 'A jeun 8h', 60, 1, 82),
    ('ART-004', 'Artroscopie gleznă', 'Artroscopie gleznă', 'Articular', 1, 'A jeun 8h', 45, 1, 83),
    ('ART-005', 'Artroscopie șold', 'Artroscopie șold', 'Articular', 1, 'A jeun 8h', 90, 1, 84);
    
    PRINT '✓ Inserate ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' înregistrări în NomenclatorEndoscopii'
END
ELSE
    PRINT '→ NomenclatorEndoscopii deja populat'
GO

PRINT ''
PRINT '============================================================================'
PRINT 'NOMENCLATOARE INVESTIGAȚII POPULATE CU SUCCES!'
PRINT '============================================================================'
GO
