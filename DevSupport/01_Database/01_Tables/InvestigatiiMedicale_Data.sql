/*
==============================================================================
DATE INIȚIALE NOMENCLATOARE INVESTIGAȚII MEDICALE
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
-- 1. INVESTIGAȚII IMAGISTICE
-- ============================================================================
PRINT 'Populating NomenclatorInvestigatiiImagistice...'

-- Radiografii
INSERT INTO [dbo].[NomenclatorInvestigatiiImagistice] ([Cod], [Denumire], [Categorie], [OrdineAfisare]) VALUES
('RX-TOR', 'Radiografie toracică', 'Radiografie', 1),
('RX-ABD', 'Radiografie abdomen', 'Radiografie', 2),
('RX-CERV', 'Radiografie coloană cervicală', 'Radiografie', 3),
('RX-DORS', 'Radiografie coloană dorsală', 'Radiografie', 4),
('RX-LOMB', 'Radiografie coloană lombară', 'Radiografie', 5),
('RX-UMAR', 'Radiografie umăr', 'Radiografie', 6),
('RX-COT', 'Radiografie cot', 'Radiografie', 7),
('RX-MANA', 'Radiografie mână', 'Radiografie', 8),
('RX-SOLD', 'Radiografie șold', 'Radiografie', 9),
('RX-GEN', 'Radiografie genunchi', 'Radiografie', 10),
('RX-GLEZNA', 'Radiografie gleznă', 'Radiografie', 11),
('RX-SIN', 'Radiografie sinusuri', 'Radiografie', 12),
('RX-OASE', 'Radiografie oase lungi', 'Radiografie', 13),
('OPG', 'Ortopantomogramă (OPG)', 'Radiografie', 14);

-- Ecografii
INSERT INTO [dbo].[NomenclatorInvestigatiiImagistice] ([Cod], [Denumire], [Categorie], [OrdineAfisare]) VALUES
('ECO-ABD', 'Ecografie abdominală', 'Ecografie', 20),
('ECO-TIR', 'Ecografie tiroidiană', 'Ecografie', 21),
('ECO-MAM', 'Ecografie mamară', 'Ecografie', 22),
('ECO-PELV', 'Ecografie pelviană', 'Ecografie', 23),
('ECO-DOPP', 'Ecografie Doppler vascular', 'Ecografie', 24);

-- CT
INSERT INTO [dbo].[NomenclatorInvestigatiiImagistice] ([Cod], [Denumire], [Categorie], [RequiresContrast], [OrdineAfisare]) VALUES
('CT-NAT', 'CT nativ', 'CT', 0, 30),
('CT-CONTR', 'CT cu substanță de contrast', 'CT', 1, 31),
('ANGIO-CT', 'Angio-CT', 'CT', 1, 32);

-- RMN
INSERT INTO [dbo].[NomenclatorInvestigatiiImagistice] ([Cod], [Denumire], [Categorie], [OrdineAfisare]) VALUES
('RMN', 'RMN', 'RMN', 40),
('ANGIO-RMN', 'Angio-RMN', 'RMN', 41);

-- Mamografie
INSERT INTO [dbo].[NomenclatorInvestigatiiImagistice] ([Cod], [Denumire], [Categorie], [OrdineAfisare]) VALUES
('MAMOGR', 'Mamografie', 'Mamografie', 50);

-- Densitometrie
INSERT INTO [dbo].[NomenclatorInvestigatiiImagistice] ([Cod], [Denumire], [Categorie], [OrdineAfisare]) VALUES
('DEXA', 'Densitometrie osoasă (DEXA)', 'Densitometrie', 55);

-- Angiografie
INSERT INTO [dbo].[NomenclatorInvestigatiiImagistice] ([Cod], [Denumire], [Categorie], [RequiresContrast], [OrdineAfisare]) VALUES
('ANGIO-COR', 'Angiografie coronariană', 'Angiografie', 1, 60),
('ANGIO-CER', 'Angiografie cerebrală', 'Angiografie', 1, 61),
('ANGIO-PER', 'Angiografie periferică', 'Angiografie', 1, 62),
('FLEBOGR', 'Flebografie', 'Angiografie', 1, 63);

-- Limfoscintigrafie
INSERT INTO [dbo].[NomenclatorInvestigatiiImagistice] ([Cod], [Denumire], [Categorie], [OrdineAfisare]) VALUES
('LIMFOSC', 'Limfoscintigrafie', 'Limfoscintigrafie', 70);

-- PET-CT și Scintigrafii
INSERT INTO [dbo].[NomenclatorInvestigatiiImagistice] ([Cod], [Denumire], [Categorie], [OrdineAfisare]) VALUES
('PET-CT', 'PET-CT', 'Medicină nucleară', 80),
('SCINT-OS', 'Scintigrafie osoasă', 'Medicină nucleară', 81),
('SCINT-TIR', 'Scintigrafie tiroidiană', 'Medicină nucleară', 82),
('SCINT-PULM-V', 'Scintigrafie pulmonară ventilație', 'Medicină nucleară', 83),
('SCINT-PULM-P', 'Scintigrafie pulmonară perfuzie', 'Medicină nucleară', 84),
('SCINT-MIOC', 'Scintigrafie miocardică', 'Medicină nucleară', 85),
('SCINT-REN', 'Scintigrafie renală', 'Medicină nucleară', 86);

-- Altele
INSERT INTO [dbo].[NomenclatorInvestigatiiImagistice] ([Cod], [Denumire], [Categorie], [RequiresContrast], [OrdineAfisare]) VALUES
('HSG', 'Histerosalpingografie', 'Radiologie intervențională', 1, 90),
('UIV', 'Urografie intravenoasă', 'Radiologie intervențională', 1, 91);

PRINT '✓ NomenclatorInvestigatiiImagistice populated with ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' records'
GO

-- ============================================================================
-- 2. EXPLORĂRI FUNCȚIONALE
-- ============================================================================
PRINT 'Populating NomenclatorExplorariFunc...'

-- Cardiologie
INSERT INTO [dbo].[NomenclatorExplorariFunc] ([Cod], [Denumire], [Categorie], [EstimatedDuration], [OrdineAfisare]) VALUES
('EKG', 'EKG', 'Cardiologie', 15, 1),
('ECOCARD', 'Ecocardiografie', 'Cardiologie', 30, 2),
('PROBA-EF', 'Proba de efort', 'Cardiologie', 45, 3),
('HOLTER-EKG', 'Holter EKG', 'Cardiologie', 1440, 4), -- 24h
('HOLTER-TA', 'Holter tensional', 'Cardiologie', 1440, 5),
('TILT-TEST', 'Tilt test', 'Cardiologie', 60, 6),
('ERGOSPIRO', 'Ergospiometrie', 'Cardiologie', 45, 7);

-- Pneumologie
INSERT INTO [dbo].[NomenclatorExplorariFunc] ([Cod], [Denumire], [Categorie], [EstimatedDuration], [OrdineAfisare]) VALUES
('SPIRO', 'Spirometrie', 'Pneumologie', 20, 20),
('PLETISMO', 'Pletismografie', 'Pneumologie', 30, 21),
('GAZO', 'Gazometrie', 'Pneumologie', 10, 22),
('TEST-REV', 'Test de reversibilitate bronșică', 'Pneumologie', 30, 23),
('TEST-PROV', 'Test de provocare bronșică', 'Pneumologie', 45, 24),
('DLCO', 'Difuzie pulmonară (DLCO)', 'Pneumologie', 20, 25),
('CPET', 'Test de efort cardiopulmonar (CPET)', 'Pneumologie', 60, 26),
('IOS', 'Oscilometrie cu impulsuri (IOS)', 'Pneumologie', 15, 27);

-- Neurologie
INSERT INTO [dbo].[NomenclatorExplorariFunc] ([Cod], [Denumire], [Categorie], [EstimatedDuration], [OrdineAfisare]) VALUES
('EEG', 'EEG', 'Neurologie', 30, 40),
('EMG', 'EMG', 'Neurologie', 45, 41),
('ENG', 'Electroneurografie (ENG)', 'Neurologie', 45, 42),
('PEV', 'Potențiale evocate vizuale', 'Neurologie', 30, 43),
('PEA', 'Potențiale evocate auditive', 'Neurologie', 30, 44),
('PES', 'Potențiale evocate somatosenzoriale', 'Neurologie', 45, 45),
('POLISOMNO', 'Polisomnografie', 'Neurologie', 480, 46); -- 8h

-- ORL
INSERT INTO [dbo].[NomenclatorExplorariFunc] ([Cod], [Denumire], [Categorie], [EstimatedDuration], [OrdineAfisare]) VALUES
('AUDIO-TON', 'Audiogramă tonală', 'ORL', 20, 60),
('AUDIO-VOC', 'Audiometrie vocală', 'ORL', 15, 61),
('TIMPANO', 'Timpanometrie', 'ORL', 10, 62),
('REFL-STAP', 'Reflexe stapediene', 'ORL', 10, 63);

-- Gastroenterologie
INSERT INTO [dbo].[NomenclatorExplorariFunc] ([Cod], [Denumire], [Categorie], [EstimatedDuration], [OrdineAfisare]) VALUES
('MANO-ESOF', 'Manometrie esofagiană', 'Gastroenterologie', 30, 80),
('PH-ESOF', 'pH-metrie esofagiană 24h', 'Gastroenterologie', 1440, 81),
('IMPED-PH', 'Impedanță-pH-metrie', 'Gastroenterologie', 1440, 82),
('MANO-ANO', 'Manometrie anorectală', 'Gastroenterologie', 30, 83),
('TEST-H2', 'Test respirator hidrogen', 'Gastroenterologie', 180, 84);

-- Urologie
INSERT INTO [dbo].[NomenclatorExplorariFunc] ([Cod], [Denumire], [Categorie], [EstimatedDuration], [OrdineAfisare]) VALUES
('UROFLOW', 'Uroflowmetrie', 'Urologie', 15, 100),
('CISTOM', 'Cistometrie', 'Urologie', 30, 101),
('URODIN', 'Studiu urodinamic complet', 'Urologie', 60, 102);

PRINT '✓ NomenclatorExplorariFunc populated'
GO

-- ============================================================================
-- 3. ENDOSCOPII
-- ============================================================================
PRINT 'Populating NomenclatorEndoscopii...'

-- Digestiv
INSERT INTO [dbo].[NomenclatorEndoscopii] ([Cod], [Denumire], [Categorie], [RequiresSedation], [EstimatedDuration], [OrdineAfisare]) VALUES
('GASTRO', 'Gastroscopie', 'Digestiv', 1, 20, 1),
('COLONO', 'Colonoscopie', 'Digestiv', 1, 45, 2),
('ERCP', 'ERCP', 'Digestiv', 1, 60, 3),
('ENTERO', 'Enteroscopie', 'Digestiv', 1, 60, 4),
('CAPS-ENDO', 'Capsulă endoscopică', 'Digestiv', 0, 480, 5),
('ECOENDO', 'Ecoendoscopie', 'Digestiv', 1, 45, 6);

-- ORL/Respirator
INSERT INTO [dbo].[NomenclatorEndoscopii] ([Cod], [Denumire], [Categorie], [RequiresSedation], [EstimatedDuration], [OrdineAfisare]) VALUES
('LARING-DIR', 'Laringoscopie directă', 'Respirator', 1, 15, 20),
('LARING-IND', 'Laringoscopie indirectă', 'Respirator', 0, 10, 21),
('RINOSCOP', 'Rinoscopie', 'Respirator', 0, 10, 22),
('FARINGOS', 'Faringoscopie', 'Respirator', 0, 10, 23),
('BRONHO', 'Bronhoscopie', 'Respirator', 1, 30, 24);

-- Urologic
INSERT INTO [dbo].[NomenclatorEndoscopii] ([Cod], [Denumire], [Categorie], [RequiresSedation], [EstimatedDuration], [OrdineAfisare]) VALUES
('CISTO', 'Cistoscopie', 'Urologic', 0, 15, 40);

-- Ginecologic
INSERT INTO [dbo].[NomenclatorEndoscopii] ([Cod], [Denumire], [Categorie], [RequiresSedation], [EstimatedDuration], [OrdineAfisare]) VALUES
('COLPO', 'Colposcopie', 'Ginecologic', 0, 15, 50),
('HISTERO', 'Histeroscopie', 'Ginecologic', 1, 30, 51);

-- Articular
INSERT INTO [dbo].[NomenclatorEndoscopii] ([Cod], [Denumire], [Categorie], [RequiresSedation], [EstimatedDuration], [OrdineAfisare]) VALUES
('ARTRO', 'Artroscopie', 'Articular', 1, 60, 60);

-- Chirurgical
INSERT INTO [dbo].[NomenclatorEndoscopii] ([Cod], [Denumire], [Categorie], [RequiresSedation], [EstimatedDuration], [OrdineAfisare]) VALUES
('LAPARO-DG', 'Laparoscopie diagnostică', 'Chirurgical', 1, 45, 70),
('VATS', 'Toracoscopie (VATS)', 'Chirurgical', 1, 60, 71),
('MEDIAST', 'Mediastinoscopie', 'Chirurgical', 1, 60, 72);

PRINT '✓ NomenclatorEndoscopii populated'
GO

-- ============================================================================
-- VERIFICARE FINALĂ
-- ============================================================================
PRINT ''
PRINT '============================================================================'
PRINT 'SUMAR DATE ÎNCĂRCATE:'
PRINT '============================================================================'
SELECT 'NomenclatorInvestigatiiImagistice' AS Tabel, COUNT(*) AS NrInregistrari FROM [dbo].[NomenclatorInvestigatiiImagistice]
UNION ALL
SELECT 'NomenclatorExplorariFunc', COUNT(*) FROM [dbo].[NomenclatorExplorariFunc]
UNION ALL
SELECT 'NomenclatorEndoscopii', COUNT(*) FROM [dbo].[NomenclatorEndoscopii];
GO

PRINT '============================================================================'
PRINT 'NOMENCLATOARE POPULATE CU SUCCES!'
PRINT '============================================================================'
GO
