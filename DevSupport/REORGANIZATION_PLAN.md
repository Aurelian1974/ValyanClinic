# 📁 DevSupport - Plan de Reorganizare

## 🎯 Obiectiv
Reorganizarea completă a folderului DevSupport pentru o structură clară și logică.

---

## 📊 Structura Curentă (Problemă)

```
DevSupport/
├── Fișiere loose în root (SQL, PS1, HTML, PDF)
├── Database/ (bine organizat - OK)
├── Documentation/ (parțial organizat)
├── Scripts/ (mixt PowerShell + SQL)
└── Refactoring/ (nou creat - trebuie mutat)
```

**Probleme:**
- ❌ Fișiere loose în root (11 fișiere)
- ❌ Documentație împrăștiată
- ❌ Scripts mixt PowerShell + SQL
- ❌ Refactoring docs în root

---

## ✅ Structura Nouă (Propusă)

```
DevSupport/
│
├── 📁 01_Database/                    ← SQL-related
│   ├── 01_Tables/
│   ├── 02_StoredProcedures/
│   │   ├── Consultatie/
│   │   ├── ICD10/
│   │   ├── ISCO/
│   │   └── Programari/
│   ├── 03_Functions/
│   ├── 04_Views/
│   ├── 05_Triggers/
│   ├── 06_Migrations/
│   ├── 07_ICD10_Data/
│   ├── 08_Verification/
│   └── 09_Debug/
│
├── 📁 02_Scripts/                     ← Automation scripts
│   ├── PowerShell/
│   │   ├── Database/
│   │   ├── Deployment/
│   │   └── Utilities/
│   └── Bash/
│
├── 📁 03_Documentation/               ← All docs organized
│   ├── 01_Setup/
│   ├── 02_Development/
│   ├── 03_Database/
│   ├── 04_Features/
│   │   ├── Consultatie/
│   │   ├── Programari/
│   │   └── Settings/
│   ├── 05_Refactoring/
│   │   ├── ConsultatieModal/        ← Move current Refactoring docs here
│   │   ├── EventHandlers/
│   │   ├── MemoryLeaks/
│   │   └── CodeCleanup/
│   ├── 06_Fixes/
│   ├── 07_Security/
│   ├── 08_Deployment/
│   ├── 09_Patterns/
│   └── 10_Changes/
│
├── 📁 04_Tools/                       ← Utilities & tools
│   ├── PasswordFix/
│   │   ├── FixPasswordTool.html
│   │   ├── AdminPasswordHashFix.csproj
│   │   └── TestValeriaHash.csx
│   └── PopUser/
│       ├── CheckPopUser.ps1
│       ├── InvestigatePopUser.sql
│       ├── QuickCheckPopUser.sql
│       └── VerifyPopUser.sql
│
├── 📁 05_Resources/                   ← Assets & templates
│   ├── PDFs/
│   │   └── SCRISOARE-MEDICALA-2024.pdf
│   ├── Templates/
│   └── Images/
│
├── 📁 bin/                           ← Build outputs (ignore)
├── 📁 obj/                           ← Build objects (ignore)
│
├── DevSupport.csproj                 ← Project file
├── README.md                         ← Main readme
└── .gitignore                        ← Git ignore file
```

---

## 🔄 Mapping Fișiere Vechi → Noi

### **Root Files → 04_Tools/**

| Fișier Vechi | Locație Nouă |
|--------------|--------------|
| `FixPasswordTool.html` | `04_Tools/PasswordFix/` |
| `AdminPasswordHashFix.csproj` | `04_Tools/PasswordFix/` |
| `TestValeriaHash.csx` | `04_Tools/PasswordFix/` |
| `CheckPopUser.ps1` | `04_Tools/PopUser/` |
| `InvestigatePopUser.sql` | `04_Tools/PopUser/` |
| `QuickCheckPopUser.sql` | `04_Tools/PopUser/` |
| `VerifyPopUser.sql` | `04_Tools/PopUser/` |

### **Root PDFs → 05_Resources/**

| Fișier Vechi | Locație Nouă |
|--------------|--------------|
| `SCRISOARE-MEDICALA-2024.pdf` | `05_Resources/PDFs/` |

### **Database/ → 01_Database/**

| Folder Vechi | Folder Nou |
|--------------|------------|
| `Database/ICD10/` | `01_Database/07_ICD10_Data/` |
| `Database/Migrations/` | `01_Database/06_Migrations/` |
| `Database/StoredProcedures/` | `01_Database/02_StoredProcedures/` |
| `Database/Functions/` | `01_Database/03_Functions/` |
| `Database/Views/` | `01_Database/04_Views/` |
| `Database/Verification/` | `01_Database/08_Verification/` |
| `Database/Debug/` | `01_Database/09_Debug/` |
| `Database/TableStructure/` | `01_Database/01_Tables/` |

### **Scripts/ → 02_Scripts/**

| Folder Vechi | Folder Nou |
|--------------|------------|
| `Scripts/PowerShellScripts/` | `02_Scripts/PowerShell/` |
| `Scripts/SQLScripts/` | `02_Scripts/PowerShell/Database/` (dacă e deployment) |

### **Documentation/ → 03_Documentation/**

| Folder Vechi | Folder Nou |
|--------------|------------|
| `Documentation/Setup/` | `03_Documentation/01_Setup/` |
| `Documentation/Development/` | `03_Documentation/02_Development/` |
| `Documentation/Database/` | `03_Documentation/03_Database/` |
| `Documentation/Features/` | `03_Documentation/04_Features/` |
| `Documentation/Refactoring/` | `03_Documentation/05_Refactoring/` |
| `Documentation/Fixes/` | `03_Documentation/06_Fixes/` |
| `Documentation/Security/` | `03_Documentation/07_Security/` |
| `Documentation/Deployment/` | `03_Documentation/08_Deployment/` |
| `Documentation/Patterns/` | `03_Documentation/09_Patterns/` |
| `Documentation/Changes/` | `03_Documentation/10_Changes/` |

### **Refactoring/ (root) → 03_Documentation/05_Refactoring/ConsultatieModal/**

| Fișier | Locație Nouă |
|--------|--------------|
| Toate fișierele `.md` | `03_Documentation/05_Refactoring/ConsultatieModal/` |

---

## 📋 Pași de Implementare

1. ✅ Creează structura de foldere nouă
2. ✅ Mută fișierele din root
3. ✅ Redenumește folderele existente
4. ✅ Actualizează README.md
5. ✅ Creează .gitignore pentru bin/obj
6. ✅ Testează că proiectul compilează
7. ✅ Commit changes

---

## 🎯 Beneficii

1. **Claritate** - Structură logică pe categorii
2. **Navigabilitate** - Foldere numerotate (01_, 02_, etc.)
3. **Scalabilitate** - Ușor de extins cu noi categorii
4. **Profesionalism** - Structură enterprise-grade
5. **Documentație** - Toate docs într-un singur loc

---

**Status:** 📝 PLAN READY  
**Next:** Implementare automată cu PowerShell
