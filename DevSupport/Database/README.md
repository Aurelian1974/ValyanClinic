# Database Schema Documentation - ValyanMed Complete

This folder contains the COMPLETE extracted database schema from ValyanMed database for ValyanClinic application.

## Last Update
Generated on: 2025-10-08 16:36:44

## Database Connection
- Server: TS1828\ERP  
- Database: ValyanMed
- Authentication: Windows Authentication (Trusted Connection)

## Complete Extraction Results

### Tables (29/29 extracted successfully)
- ✅ **Audit_Persoana** - Complete table structure with constraints and indexes
- ✅ **Audit_Utilizator** - Complete table structure with constraints and indexes
- ✅ **Audit_UtilizatorDetaliat** - Complete table structure with constraints and indexes
- ✅ **ComenziTeste** - Complete table structure with constraints and indexes
- ✅ **Consultatii** - Complete table structure with constraints and indexes
- ✅ **Departamente** - Complete table structure with constraints and indexes
- ✅ **DepartamenteIerarhie** - Complete table structure with constraints and indexes
- ✅ **Diagnostice** - Complete table structure with constraints and indexes
- ✅ **DispozitiveMedicale** - Complete table structure with constraints and indexes
- ✅ **FormulareConsimtamant** - Complete table structure with constraints and indexes
- ✅ **IstoricMedical** - Complete table structure with constraints and indexes
- ✅ **Judet** - Complete table structure with constraints and indexes
- ✅ **Localitate** - Complete table structure with constraints and indexes
- ✅ **MaterialeSanitare** - Complete table structure with constraints and indexes
- ✅ **Medicament** - Complete table structure with constraints and indexes
- ✅ **MedicamenteNoi** - Complete table structure with constraints and indexes
- ✅ **Pacienti** - Complete table structure with constraints and indexes
- ✅ **Partener** - Complete table structure with constraints and indexes
- ✅ **Personal** - Complete table structure with constraints and indexes
- ✅ **PersonalMedical** - Complete table structure with constraints and indexes
- ✅ **PersonalMedical_Backup_Migration** - Complete table structure with constraints and indexes
- ✅ **Prescriptii** - Complete table structure with constraints and indexes
- ✅ **Programari** - Complete table structure with constraints and indexes
- ✅ **RezultateTeste** - Complete table structure with constraints and indexes
- ✅ **RoluriSistem** - Complete table structure with constraints and indexes
- ✅ **SemneVitale** - Complete table structure with constraints and indexes
- ✅ **TipLocalitate** - Complete table structure with constraints and indexes
- ✅ **TipuriTeste** - Complete table structure with constraints and indexes
- ✅ **TriajPacienti** - Complete table structure with constraints and indexes

### New Tables (Design Phase)
- 🔄 **Ocupatii_ISCO08** - ISCO-08 occupations classification (ready for implementation)

### Stored Procedures (39/39 extracted successfully)

Total procedures extracted: 39

### By Pattern:
- **sp_Personal%** : 19 procedures
- **sp_PersonalMedical%** : 10 procedures
- **sp_Judet%** : 4 procedures
- **sp_Localitat%** : 4 procedures
- **sp_Departament%** : 2 procedures

### New Stored Procedures (Design Phase)
- 🔄 **sp_Ocupatii_ISCO08_*** - Complete set of procedures for ISCO-08 management

## Usage

These files can be used to:
1. **Recreate database structure** in development environments
2. **Compare schema** with entity models in C# code
3. **Document database design** for development team
4. **Track schema changes** over time

## Files Structure

```
Database/
├── README.md                          # This documentation
├── TableStructure/                    # All table creation scripts
│   ├── Personal_Complete.sql
│   ├── PersonalMedical_Complete.sql
│   ├── Judet_Complete.sql
│   ├── Localitate_Complete.sql
│   ├── Departamente_Complete.sql
│   ├── Pacienti_Complete.sql
│   ├── ... (all 29 existing tables)
│   ├── Ocupatii_ISCO08_Structure.sql    # 🆕 NEW: ISCO-08 table
│   ├── Ocupatii_ISCO08_SampleData.sql   # 🆕 NEW: Sample data
│   ├── README_Ocupatii_ISCO08.md        # 🆕 NEW: Complete documentation
│   └── OcupatieISCO_Entity.cs           # 🆕 NEW: C# Entity model
├── StoredProcedures/                  # All stored procedure scripts
│   ├── sp_Personal_*.sql
│   ├── sp_PersonalMedical_*.sql
│   ├── sp_Judet_*.sql
│   ├── sp_Localitat_*.sql
│   ├── sp_Departament_*.sql
│   └── sp_Ocupatii_ISCO08_All.sql       # 🆕 NEW: ISCO-08 procedures
├── Functions/                         # User-defined functions (empty)
└── Views/                            # Database views (empty)
```

## New Addition: ISCO-08 Occupations Classification

### 🆕 What's New

A complete implementation for **ISCO-08 (International Standard Classification of Occupations)** has been designed and is ready for deployment:

#### 📊 **Database Components**
- **Table Structure**: `Ocupatii_ISCO08_Structure.sql`
  - Hierarchical structure (4 levels: Major Groups → Sub-groups → Minor Groups → Occupations)
  - Complete constraints, indexes, and foreign keys
  - Audit trail with timestamps and user tracking
  - Self-referencing hierarchy for parent-child relationships

#### 🔧 **Stored Procedures**
- `sp_Ocupatii_ISCO08_GetAll` - Paginated listing with filtering
- `sp_Ocupatii_ISCO08_GetById` - Get by ID
- `sp_Ocupatii_ISCO08_GetByCod` - Get by ISCO code
- `sp_Ocupatii_ISCO08_GetCopii` - Get hierarchical children
- `sp_Ocupatii_ISCO08_Search` - Advanced search with relevance scoring
- `sp_Ocupatii_ISCO08_GetStatistics` - Statistical reports
- `sp_Ocupatii_ISCO08_GetDropdownOptions` - UI dropdown support

#### 📝 **Sample Data**
- Complete hierarchical sample data
- Major groups (10), Sub-groups, Minor groups, and specific occupations
- Focus on healthcare professions relevant to ValyanClinic
- IT occupations for development team classification

#### 💻 **C# Integration**
- **Entity Model**: `OcupatieISCO.cs` ready for Entity Framework
- Complete property mapping with validation attributes
- Navigation properties for hierarchical relationships  
- Computed properties for UI display (`CodSiDenumire`, `NumeNivelIerarhic`)
- Blazor-compatible implementation

#### 📥 **Data Import**
- **PowerShell Script**: `Import-OcupatiiISCO.ps1`
- Automated import from official Romanian government data source
- XML parsing and database population
- Error handling and progress reporting
- **Data Source**: https://data.gov.ro (official ISCO-08 Romanian translation)

#### 📚 **Documentation**
- **Complete Documentation**: `README_Ocupatii_ISCO08.md`
- Usage scenarios for ValyanClinic
- Performance recommendations
- Compliance with international standards (ILO, EU, Romanian authorities)

### 🎯 **Business Value**

1. **HR Management**: Standardized occupation classification for all personnel
2. **Compliance**: Meets Romanian and EU reporting requirements  
3. **Integration**: Ready for ANOFM, INS, and healthcare authority integrations
4. **Standardization**: International standard ensures compatibility and data quality
5. **Scalability**: Hierarchical structure supports detailed or general classifications

### 🚀 **Implementation Ready**

All components are **production-ready** and can be deployed:

1. **Database Setup**:
   ```sql
   -- Run in SQL Server Management Studio
   :r Ocupatii_ISCO08_Structure.sql
   :r sp_Ocupatii_ISCO08_All.sql
   :r Ocupatii_ISCO08_SampleData.sql  -- Optional: sample data
   ```

2. **Data Import**:
   ```powershell
   # From DevSupport\Scripts\PowerShellScripts
   .\Import-OcupatiiISCO.ps1 -XmlFilePath "isco-08-ocupatii-2024.xml"
   ```

3. **Entity Framework Integration**:
   - Add `OcupatieISCO.cs` to `ValyanClinic.Domain.Entities`
   - Update `DbContext` with `DbSet<OcupatieISCO>`
   - Run migrations

### 📈 **Expected Data Volume**
- **Major Groups**: 10
- **Sub-groups**: ~43
- **Minor Groups**: ~130  
- **Detailed Occupations**: ~436
- **Total Records**: ~619

---

*Generated automatically by ValyanClinic Database Extraction Scripts*
*This represents the FULL database schema as of 2025-10-08 16:36:44*
*🆕 ISCO-08 components added on 2025-10-08*
