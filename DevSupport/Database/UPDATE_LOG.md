# Database Update Log

## Actualizare: 2025-10-18 08:40:46

### Rezumat Actualizare
Folderul `DevSupport/Database` a fost actualizat complet folosind scriptul `Extract-AllTables.ps1` cu datele actuale din baza de date **ValyanMed**.

### Conexiune Utilizată
- **Server:** DESKTOP-9H54BCS\SQLSERVER
- **Database:** ValyanMed
- **Authentication:** Windows Authentication (Trusted Connection)
- **Connection String Source:** ValyanClinic/appsettings.json

### Rezultate Extracție

#### Tabele Extrase
- **Total tabele în DB:** 30
- **Tabele extrase cu succes:** 30 (100%)
- **Erori:** 0

#### Stored Procedures Extrase
- **Total SP în DB:** 51
- **SP extrase cu succes:** 51 (100%)
- **Erori:** 0

#### Fișiere Generate
- **TableStructure:** 31 fișiere SQL complete
- **StoredProcedures:** 52 fișiere SQL complete
- **Functions:** 0 (nu există în DB)
- **Views:** 0 (nu există în DB)

### Lista Tabelelor Extrase

1. Audit_Persoana
2. Audit_Utilizator
3. Audit_UtilizatorDetaliat
4. ComenziTeste
5. Consultatii
6. Departamente
7. Diagnostice
8. DispozitiveMedicale
9. FormulareConsimtamant
10. IstoricMedical
11. Judet
12. Localitate
13. MaterialeSanitare
14. Medicament
15. MedicamenteNoi
16. Ocupatii_ISCO08
17. Pacienti
18. Partener
19. Personal ⭐
20. PersonalMedical ⭐
21. PersonalMedical_Backup_Migration
22. Prescriptii
23. Programari
24. RezultateTeste
25. RoluriSistem
26. SemneVitale
27. TipDepartament
28. TipLocalitate
29. TipuriTeste
30. TriajPacienti

⭐ = Tabele principale pentru aplicația ValyanClinic

### Categorii Stored Procedures

#### Departamente (2 SP)
- sp_Departamente_GetAll
- sp_Departamente_GetByTip

#### Judete (5 SP)
- sp_Judete_GetAll
- sp_Judete_GetByCod
- sp_Judete_GetById
- sp_Judete_GetOrderedByName
- GetAllJudete

#### Localitati (4 SP)
- sp_Localitati_GetAll
- sp_Localitati_GetById
- sp_Localitati_GetByJudetId
- sp_Localitati_GetByJudetIdOrdered
- Localitate_GetByJudet

#### Location (4 SP)
- sp_Location_GetJudete
- sp_Location_GetJudetNameById
- sp_Location_GetLocalitateNameById
- sp_Location_GetLocalitatiByJudetId

#### Lookup (1 SP)
- sp_Lookup_GetDepartamente

#### Ocupatii ISCO08 (8 SP)
- sp_Ocupatii_ISCO08_Create
- sp_Ocupatii_ISCO08_Delete
- sp_Ocupatii_ISCO08_GetAll
- sp_Ocupatii_ISCO08_GetById
- sp_Ocupatii_ISCO08_GetGrupeMajore
- sp_Ocupatii_ISCO08_GetStatistics
- sp_Ocupatii_ISCO08_Search
- sp_Ocupatii_ISCO08_Update

#### Personal (10 SP)
- sp_Personal_CheckUnique
- sp_Personal_Create
- sp_Personal_Delete
- sp_Personal_GetAll
- sp_Personal_GetById
- sp_Personal_GetCount
- sp_Personal_GetDropdownOptions
- sp_Personal_GetStatistics
- sp_Personal_Update

#### PersonalMedical (10 SP)
- sp_PersonalMedical_CheckUnique
- sp_PersonalMedical_Create
- sp_PersonalMedical_Delete
- sp_PersonalMedical_GetAll
- sp_PersonalMedical_GetById
- sp_PersonalMedical_GetDistributiePerDepartament
- sp_PersonalMedical_GetDistributiePerSpecializare
- sp_PersonalMedical_GetDropdownOptions
- sp_PersonalMedical_GetStatistics
- sp_PersonalMedical_Update

#### TipDepartament (1 SP)
- sp_TipDepartament_GetAll

### Caracteristici Extracție

Fiecare fișier de tabel include:
- ✅ Structură completă cu toate coloanele
- ✅ Tipuri de date exacte cu lungimi și precizie
- ✅ Constraint-uri NOT NULL / NULL
- ✅ Primary Keys
- ✅ Foreign Keys cu acțiuni ON DELETE/UPDATE
- ✅ Indexes (clustered și non-clustered)
- ✅ Identity columns
- ✅ Script DROP IF EXISTS
- ✅ Metadata (data generării, număr coloane, etc.)

### Script Utilizat
```powershell
.\Extract-AllTables.ps1 -ConfigPath "..\..\..\ValyanClinic\appsettings.json" -OutputPath "..\..\Database"
```

### Status
✅ **Actualizare completă și reușită - 100% Success Rate**

---
*Generat automat de Database Extraction Script*
*Pentru actualizări viitoare, rulează: `.\Run-DatabaseExtraction.ps1` și selectează opțiunea 1*
