# PowerShell Scripts pentru Administrarea Bazei de Date

Acest director contine scripturi PowerShell pentru administrarea si sincronizarea bazei de date ValyanMed.

## Scripturi Principale

### ? **Run-DatabaseExtraction.ps1** *(SCRIPT PRINCIPAL)*
Script master care ofera un meniu interactiv pentru toate operatiile de extractie si analiza.

**Utilizare:**
```powershell
.\Run-DatabaseExtraction.ps1
```

**Optiuni disponibile:**
1. Extractie completa (toate obiectele din DB) - **RECOMANDAT**
2. Extractie standard (cu functii avansate)
3. Extractie specifica (doar tabele relevante)
4. Comparare schema DB vs Entity Models
5. Verificare schema
6. Iesire

### ? **Extract-AllTables.ps1** ⭐ **CEL MAI COMPLET**
Extrage toate tabelele (29) si toate stored procedures (36) cu constraint-uri complete.

**Utilizare:**
```powershell
.\Extract-AllTables.ps1 [-ConfigPath "..\..\ValyanClinic\appsettings.json"] [-OutputPath "..\..\Database"]
```

### ? **Extract-DatabaseSchema.ps1**
Extrage schema completa din baza de date (versiunea corrigata).

### ? **Extract-SpecificTables.ps1**
Extrage doar tabelele si stored procedures relevante pentru aplicatia ValyanClinic.

**Tabele tinta:**
- Personal
- PersonalMedical
- Judete
- Localitati
- Departamente
- PozitiiMedicale
- Patient (daca exista)
- User (daca exista)

### ? **Compare-SchemaWithCode.ps1**
Compara structura din baza de date cu entity models din cod si genereaza un raport detaliat.

### ? **Validate-DatabaseSchema.ps1**
Valideaza schema bazei de date si genereaza un raport de structura.

### ? **Validate-Extraction.ps1**
Valideaza ca extractia a fost realizata cu succes si afiseaza statistici.

## Scripturi Existente (Refactorizare)

### **02_DeleteOldFiles.ps1**
Script pentru stergerea fisierelor vechi in timpul refactorizarii.

### **03_InstallPackages.ps1**
Script pentru instalarea pachetelor NuGet necesare.

### **Analyze-DatabaseSync.ps1**
Script pentru analiza sincronizarii dintre cod si baza de date.

## Directorul de Output

Toate script-urile de extractie genereaza fisiere in directorul `../../Database/` (DevSupport/Database/) cu urmatoarea structura:

```
DevSupport/Database/                   # ← LOCAȚIA CORECTĂ
├── README.md                         # Documentatie generala
├── TableStructure/                   # Script-uri CREATE TABLE
│   ├── README.md
│   ├── Personal_Complete.sql
│   ├── PersonalMedical_Complete.sql
│   ├── Audit_Persoana_Complete.sql
│   ├── ... (toate cele 29 de tabele)
│   └── TriajPacienti_Complete.sql
├── StoredProcedures/                # Stored procedures
│   ├── README.md
│   ├── sp_Personal_GetAll.sql
│   ├── sp_Personal_Create.sql
│   ├── ... (toate cele 36 de SP-uri)
│   └── sp_PersonalMedical_Update.sql
├── Functions/                       # User-defined functions (gol)
│   ├── README.md
│   └── ...
├── Views/                          # Database views (gol)
│   ├── README.md
│   └── ...
└── *_Report.md                     # Rapoarte generate
```

## Prerequisite

1. **SQL Server Client Tools** - pentru conexiunea la baza de date
2. **PowerShell 5.1+** - pentru executia script-urilor
3. **Acces la baza de date** - connection string valid in appsettings.json

## Connection String

Script-urile citesc connection string-ul din:
```
ValyanClinic\appsettings.json -> ConnectionStrings.DefaultConnection
```

Configuratie actuala:
- Server: `TS1828\ERP`
- Database: `ValyanMed`
- Authentication: Windows Authentication (Trusted Connection)

## Cum sa Utilizezi

### Pas 1: Navigheaza in directorul de scripturi
```powershell
cd DevSupport\Scripts\PowerShellScripts
```

### Pas 2: Ruleaza script-ul principal
```powershell
.\Run-DatabaseExtraction.ps1
```

### Pas 3: Alege optiunea dorita din meniu
- Pentru prima data, recomand **Optiunea 1** (extractie completa)
- Pentru comparare cod vs DB, foloseste **Optiunea 4**

## Troubleshooting

### Eroare: "SQL Server Client nu este disponibil"
- Instaleaza SQL Server Management Studio (SSMS)
- Sau instaleaza SQL Server Client Tools
- Verifica ca ai .NET Framework/Core instalat

### Eroare: "Nu gasesc appsettings.json"
- Verifica ca esti in directorul `DevSupport\Scripts\PowerShellScripts`
- Verifica ca fisierul `ValyanClinic\appsettings.json` exista

### Eroare de conexiune la baza de date
- Verifica ca SQL Server-ul este pornit
- Verifica ca ai acces la server-ul `TS1828\ERP`
- Testa conexiunea cu SQL Server Management Studio

## Output Files

Dupa rularea script-urilor, verifica directorul `DevSupport\Database\` pentru:
- Script-uri SQL de recreare tabele (29 fisiere)
- Backup-uri stored procedures (36 fisiere)
- Rapoarte de comparare si analiza

## Rezultate Actuale

✅ **Extractie 100% completa realizata!**
- 29/29 tabele extrase cu succes
- 36/36 stored procedures extrase
- 0 functions (nu exista in DB)
- 0 views (nu exista in DB)

---
*Pentru suport, verifica documentatia in README.md files din fiecare subdirector.*
