# PowerShell Scripts pentru Administrarea Bazei de Date

Acest director contine scripturile PowerShell ESENȚIALE pentru administrarea si sincronizarea bazei de date ValyanMed.

## ✨ Scripturi Disponibile

### 🎯 **Run-DatabaseExtraction.ps1** *(SCRIPT PRINCIPAL)*
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

---

### 📊 **Extract-AllTables.ps1** ⭐ **CEL MAI COMPLET**
Extrage toate tabelele (29) si toate stored procedures (36) cu constraint-uri complete.

**Utilizare:**
```powershell
.\Extract-AllTables.ps1 [-ConfigPath "..\..\ValyanClinic\appsettings.json"] [-OutputPath "..\..\Database"]
```

**Ce extrage:**
- 29 tabele complete cu toate constraint-urile
- 36 stored procedures
- Generare rapoarte detaliate

---

### 🔍 **Extract-DatabaseSchema.ps1**
Extrage schema completa din baza de date (versiunea corrigata).

**Utilizare:**
```powershell
.\Extract-DatabaseSchema.ps1 [-ConfigPath "..."] [-OutputPath "..."]
```

---

### 📋 **Extract-Complete.ps1**
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

---

### 🔄 **Compare-SchemaWithCode.ps1**
Compara structura din baza de date cu entity models din cod si genereaza un raport detaliat.

**Utilizare:**
```powershell
.\Compare-SchemaWithCode.ps1 [-ConfigPath "..."]
```

**Output:**
- Diferente intre DB si models
- Coloane lipsa
- Tipuri de date necorespunzatoare
- Recomandari

---

### ✅ **Validate-DatabaseSchema.ps1**
Valideaza schema bazei de date si genereaza un raport de structura.

**Utilizare:**
```powershell
.\Validate-DatabaseSchema.ps1 [-ConfigPath "..."]
```

---

### 🔌 **Test-Connection.ps1**
Test rapid de conectare la baza de date.

**Utilizare:**
```powershell
.\Test-Connection.ps1
```

**Ce face:**
- Verifica connection string
- Testeaza conexiunea la DB
- Listeaza tabele disponibile

---

### 🔎 **Query-ValyanMedDatabase.ps1**
Permite executarea sigura de query-uri SELECT pe baza de date.

**Utilizare:**
```powershell
.\Query-ValyanMedDatabase.ps1 -Query "SELECT TOP 10 * FROM Personal" [-Format Text|Json|Csv]
```

**Securitate:**
- Doar query-uri SELECT permise
- Validare automata SQL injection
- Output formatat

**Exemple:**
```powershell
# Text format
.\Query-ValyanMedDatabase.ps1 -Query "SELECT * FROM Departamente"

# JSON format
.\Query-ValyanMedDatabase.ps1 -Query "SELECT Nume, Prenume FROM Personal WHERE EsteActiv = 1" -Format Json

# CSV format
.\Query-ValyanMedDatabase.ps1 -Query "SELECT * FROM Judete" -Format Csv
```

---

## 📁 Directorul de Output

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
├── Functions/                       # User-defined functions
│   └── README.md
├── Views/                          # Database views
│   └── README.md
└── *_Report.md                     # Rapoarte generate
```

---

## 🔧 Prerequisite

1. **SQL Server Client Tools** - pentru conexiunea la baza de date
2. **PowerShell 5.1+** - pentru executia script-urilor
3. **Acces la baza de date** - connection string valid in appsettings.json

---

## 🔗 Connection String

Script-urile citesc connection string-ul din:
```
ValyanClinic\appsettings.json -> ConnectionStrings.DefaultConnection
```

**Configuratie actuala:**
- Server: `TS1828\ERP`
- Database: `ValyanMed`
- Authentication: Windows Authentication (Trusted Connection)

---

## 🚀 Cum sa Utilizezi

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

---

## ⚠️ Troubleshooting

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

---

## 📈 Output Files

Dupa rularea script-urilor, verifica directorul `DevSupport\Database\` pentru:
- Script-uri SQL de recreare tabele (29 fisiere)
- Backup-uri stored procedures (36 fisiere)
- Rapoarte de comparare si analiza

---

## ✅ Rezultate Actuale

**Extractie 100% completa realizata!**
- ✓ 29/29 tabele extrase cu succes
- ✓ 36/36 stored procedures extrase
- ✓ 0 functions (nu exista in DB)
- ✓ 0 views (nu exista in DB)

---

## 🧹 Curățare și Întreținere

**Scripturi curatate:** 43 scripturi neesențiale au fost eliminate (ISCO, teste, temporare).

**Scripturi ramase:** 9 scripturi esențiale pentru operațiuni cu baza de date.

Pentru a rula din nou curățarea (ATENȚIE: șterge toate scripturile neesențiale):
```powershell
.\_CLEANUP_Scripts.ps1
```

---

## 📚 Best Practices

1. **Ruleaza extractii regulate** - pentru a mentine documentatia la zi
2. **Compara schema cu codul** - inainte de deployment
3. **Valideaza schema** - dupa modificari in DB
4. **Foloseste Query script** - pentru interogari ad-hoc sigure
5. **Backup** - mentine un backup al extractiilor

---

*Pentru suport, verifica documentatia in README.md files din fiecare subdirector.*
