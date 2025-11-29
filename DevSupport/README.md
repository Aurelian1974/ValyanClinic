# 📁 DevSupport - Development Support Files

This folder contains all development support materials: database scripts, documentation, tools, and resources.

---

## 📊 Folder Structure

```
DevSupport/
│
├── 📁 01_Database/                    ← SQL Scripts & Database
│   ├── 01_Tables/                     - Table creation scripts
│   ├── 02_StoredProcedures/           - Stored procedures (by feature)
│   │   ├── Consultatie/
│   │   ├── ICD10/
│   │   ├── ISCO/
│   │   └── Programari/
│   ├── 03_Functions/                  - SQL functions
│   ├── 04_Views/                      - Database views
│   ├── 05_Triggers/                   - Database triggers
│   ├── 06_Migrations/                 - Migration scripts
│   ├── 07_ICD10_Data/                 - ICD-10 medical codes
│   ├── 08_Verification/               - Verification scripts
│   └── 09_Debug/                      - Debug scripts
│
├── 📁 02_Scripts/                     ← Automation Scripts
│   └── PowerShell/
│       ├── Database/                  - DB deployment scripts
│       ├── Deployment/                - App deployment scripts
│       └── Utilities/                 - General utilities
│
├── 📁 03_Documentation/               ← All Documentation
│   ├── 01_Setup/                      - Setup guides
│   ├── 02_Development/                - Development docs
│   ├── 03_Database/                   - Database docs
│   ├── 04_Features/                   - Feature-specific docs
│   │   ├── Consultatie/
│   │   ├── Programari/
│   │   └── Settings/
│   ├── 05_Refactoring/                - Refactoring reports
│   │   ├── ConsultatieModal/          - Modal refactoring (Phase 1-6)
│   │   ├── EventHandlers/             - Event handlers cleanup
│   │   ├── MemoryLeaks/               - Memory leaks fixes
│   │   └── CodeCleanup/               - Code cleanup reports
│   ├── 06_Fixes/                      - Bug fixes documentation
│   ├── 07_Security/                   - Security docs
│   ├── 08_Deployment/                 - Deployment guides
│   ├── 09_Patterns/                   - Design patterns
│   └── 10_Changes/                    - Change logs
│
├── 📁 04_Tools/                       ← Utilities & Tools
│   ├── PasswordFix/                   - Password hashing tools
│   └── PopUser/                       - PopUser investigation
│
├── 📁 05_Resources/                   ← Assets & Templates
│   ├── PDFs/                          - PDF documents
│   ├── Templates/                     - Document templates
│   └── Images/                        - Graphics
│
├── DevSupport.csproj                  - Project file
├── README.md                          - This file
└── REORGANIZATION_PLAN.md             - Reorganization details
```

---

## 🎯 Quick Navigation

### **Working with Database**
```powershell
# Navigate to database scripts
cd 01_Database

# Execute table creation
sqlcmd -S server -d database -i 01_Tables\*.sql

# Deploy stored procedures
cd 02_StoredProcedures
# Run scripts by feature folder
```

### **Reading Documentation**
```powershell
# Start with setup
cd 03_Documentation\01_Setup

# View feature docs
cd 03_Documentation\04_Features\Consultatie

# Check refactoring reports
cd 03_Documentation\05_Refactoring\ConsultatieModal
```

### **Using Tools**
```powershell
# Password fix tool
cd 04_Tools\PasswordFix

# PopUser investigation
cd 04_Tools\PopUser
```

---

## 📚 Key Documentation

### **Setup & Development**
- [Setup Guide](03_Documentation/01_Setup/) - Initial setup instructions
- [Development Guide](03_Documentation/02_Development/) - Development practices

### **Refactoring Reports**
- [ConsultatieModal Refactoring](03_Documentation/05_Refactoring/ConsultatieModal/SESSION_COMPLETE_FINAL.md) - Complete refactoring (Phases 1-6)
- [EventHandlers Cleanup](03_Documentation/05_Refactoring/EventHandlers/) - Event handlers refactoring
- [Memory Leaks Fix](03_Documentation/05_Refactoring/MemoryLeaks/) - Memory management fixes

### **Database**
- [ICD-10 Setup](01_Database/07_ICD10_Data/) - Medical codes installation
- [Migrations](01_Database/06_Migrations/) - Database migrations

---

## 🔄 Reorganization History

This folder was reorganized on **19 decembrie 2024** from a flat structure to a logical, numbered hierarchy.

**Benefits:**
- ✅ Clear separation by category
- ✅ Numbered folders for logical order
- ✅ Easy navigation
- ✅ Scalable structure
- ✅ Professional organization

**Details:** See [REORGANIZATION_PLAN.md](REORGANIZATION_PLAN.md)

---

## 🚀 Quick Start

### **For Developers**
1. Read [Setup Guide](03_Documentation/01_Setup/)
2. Review [Development Guide](03_Documentation/02_Development/)
3. Check [Feature Documentation](03_Documentation/04_Features/)

### **For Database Admins**
1. Review [Database Structure](01_Database/)
2. Execute scripts in numerical order
3. Use [Verification Scripts](01_Database/08_Verification/)

### **For DevOps**
1. Check [Deployment Guide](03_Documentation/08_Deployment/)
2. Use [Automation Scripts](02_Scripts/PowerShell/)

---

## 📊 Statistics

| Category | Folders | Approximate Files |
|----------|---------|-------------------|
| Database | 13 | 200+ SQL scripts |
| Documentation | 20+ | 150+ MD files |
| Scripts | 5 | 50+ PowerShell |
| Tools | 2 | 10+ utilities |
| Resources | 3 | Various |

---

## 🔧 Maintenance

### **Adding New Content**

**New Database Script:**
```powershell
# Add to appropriate subfolder
01_Database/02_StoredProcedures/[Feature]/SP_NewProcedure.sql
```

**New Documentation:**
```powershell
# Add to relevant category
03_Documentation/04_Features/[Feature]/NewFeature.md
```

**New Tool:**
```powershell
# Create subfolder in Tools
04_Tools/[ToolName]/
```

### **Updating Existing Content**
- Keep original folder structure
- Update README in subfolder if needed
- Maintain numerical prefixes

---

## 📝 Notes

- Folders are numbered (`01_`, `02_`, etc.) for logical ordering
- Each major folder has its own README
- Old folders (Database, Scripts, Documentation, Refactoring) preserved for safety
- Delete old folders after verifying new structure works

---

## ✅ Verification Checklist

Before deleting old folders, verify:
- [ ] All files copied to new location
- [ ] Project compiles successfully
- [ ] Scripts run from new locations
- [ ] Documentation accessible
- [ ] Tools work correctly

---

## 🆘 Support

If you need to revert:
1. Old folders are preserved
2. Files were copied (not moved)
3. Can restore from Git history

---

**Last Updated:** 19 decembrie 2024  
**Version:** 2.0 (Reorganized Structure)  
**Status:** ✅ Production Ready

---

*ValyanClinic Development Support - Clean, Organized, Professional* 🚀

