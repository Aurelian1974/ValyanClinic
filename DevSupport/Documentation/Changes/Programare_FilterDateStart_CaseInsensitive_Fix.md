# Fix: Programare List - Data Start & Case-Insensitive Search

**Date:** 2025-01-XX  
**Status:** ✅ **IMPLEMENTED**  
**Module:** Programare Management (Lista Programări)

---

## 📋 Summary

Two improvements have been made to the **Lista Programări** (Appointments List) page:

1. **FilterDataStart default changed** from "today" to **"first day of current month"**
2. **Search made explicitly case-insensitive** using SQL Server COLLATE

---

## 🎯 Changes Made

### 1. **Default FilterDataStart → Beginning of Current Month**

#### **Before** ❌
```csharp
// C# Component (Blazor)
FilterDataStart = DateTime.Today;  // azi
FilterDataEnd = DateTime.Today.AddDays(30);  // azi + 30 zile

// SQL Stored Procedure
IF @DataStart IS NULL
    SET @DataStart = CAST(GETDATE() AS DATE);  // azi
IF @DataEnd IS NULL
    SET @DataEnd = DATEADD(DAY, 30, @DataStart);  // azi + 30 zile
```

**Problem:** Users had to manually change the start date every time they wanted to see appointments from the beginning of the month.

#### **After** ✅
```csharp
// C# Component (Blazor) - UPDATED!
var today = DateTime.Today;
FilterDataStart = new DateTime(today.Year, today.Month, 1); // Prima zi a lunii
FilterDataEnd = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month)); // Ultima zi a lunii

// SQL Stored Procedure - UPDATED!
IF @DataStart IS NULL
  SET @DataStart = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);  // prima zi luna
IF @DataEnd IS NULL
    SET @DataEnd = EOMONTH(@DataStart);  // ultima zi luna
```

**Benefit:** By default, users now see ALL appointments for the **entire current month** (from day 1 to last day), which is more practical for medical scheduling.

**Example:**
- If today is **January 15, 2025**:
  - FilterDataStart: **January 1, 2025**
  - FilterDataEnd: **January 31, 2025**

---

### 2. **Case-Insensitive Search**

#### **Before** ⚠️
```sql
-- Relied on database default collation (may vary)
pac.Nume LIKE '%' + @SearchText + '%' OR
pac.Prenume LIKE '%' + @SearchText + '%' OR
...
```

**Problem:** Search behavior was dependent on database collation settings. If database was case-sensitive, searching for "Ion" wouldn't find "ION" or "ion".

#### **After** ✅
```sql
-- Explicitly case-insensitive using COLLATE Latin1_General_CI_AI
-- CI = Case Insensitive, AI = Accent Insensitive
pac.Nume COLLATE Latin1_General_CI_AI LIKE '%' + @SearchText COLLATE Latin1_General_CI_AI + '%' OR
pac.Prenume COLLATE Latin1_General_CI_AI LIKE '%' + @SearchText COLLATE Latin1_General_CI_AI + '%' OR
pac.CNP LIKE '%' + @SearchText + '%' OR  -- CNP doesn't need COLLATE (numeric)
doc.Nume COLLATE Latin1_General_CI_AI LIKE '%' + @SearchText COLLATE Latin1_General_CI_AI + '%' OR
doc.Prenume COLLATE Latin1_General_CI_AI LIKE '%' + @SearchText COLLATE Latin1_General_CI_AI + '%' OR
p.Observatii COLLATE Latin1_General_CI_AI LIKE '%' + @SearchText COLLATE Latin1_General_CI_AI + '%'
```

**Benefit:** 
- Searching "popescu" will find "Popescu", "POPESCU", "PopEscu", etc.
- Searching "ion" will find "Ion", "ION", "ion"
- Consistent behavior regardless of database collation settings

---

## 📁 Files Modified

### 1. **GetProgramareListQuery.cs** (Documentation Update)
**Path:** `ValyanClinic.Application\Features\ProgramareManagement\Queries\GetProgramareList\GetProgramareListQuery.cs`

**Change:**
```csharp
// BEFORE:
/// <summary>
/// Data de început pentru interval (implicit: azi).
/// </summary>
public DateTime? FilterDataStart { get; set; }

/// <summary>
/// Data de sfârșit pentru interval (implicit: azi + 30 zile).
/// </summary>
public DateTime? FilterDataEnd { get; set; }

// AFTER:
/// <summary>
/// Data de început pentru interval (implicit: prima zi a lunii curente).
/// </summary>
public DateTime? FilterDataStart { get; set; }

/// <summary>
/// Data de sfârșit pentru interval (implicit: ultima zi a lunii curente).
/// </summary>
public DateTime? FilterDataEnd { get; set; }
```

**Note:** Only documentation comments were changed. The actual logic is in the stored procedures and Blazor component.

---

### 2. **ListaProgramari.razor.cs** (Blazor Component Logic) ⭐ **NEW!**
**Path:** `ValyanClinic\Components\Pages\Programari\ListaProgramari.razor.cs`

**Changes:**

#### **OnInitializedAsync() - Line ~58**
```csharp
// BEFORE ❌
FilterDataStart = DateTime.Today;  // azi
FilterDataEnd = DateTime.Today.AddDays(30);  // azi + 30 zile

// AFTER ✅
var today = DateTime.Today;
FilterDataStart = new DateTime(today.Year, today.Month, 1); // Prima zi a lunii
FilterDataEnd = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month)); // Ultima zi a lunii
```

#### **ClearAllFilters() - Line ~224**
```csharp
// BEFORE ❌
FilterDataStart = DateTime.Today;
FilterDataEnd = DateTime.Today.AddDays(30);

// AFTER ✅
var today = DateTime.Today;
FilterDataStart = new DateTime(today.Year, today.Month, 1); // Prima zi a lunii
FilterDataEnd = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month)); // Ultima zi a lunii
```

**Why this matters:**
- The Blazor component is the **UI layer** that sends the filter values to the backend
- Even though the SQL stored procedure has defaults, the Blazor component was sending explicit values (`DateTime.Today`)
- By updating both C# and SQL, we ensure consistent behavior whether filters are sent from UI or defaulted in SQL

---

### 3. **sp_Programari_GetAll.sql** (SQL Logic)
**Path:** `DevSupport\Database\StoredProcedures\Programari\sp_Programari_GetAll.sql`

**Changes:**
1. **Default date range:**
   ```sql
   -- ✅ MODIFICARE 1: Setare interval implicit - prima zi a lunii curente
   IF @DataStart IS NULL
       SET @DataStart = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
 
   -- ✅ MODIFICARE 2: Data end - ultima zi a lunii curente
   IF @DataEnd IS NULL
       SET @DataEnd = EOMONTH(@DataStart);
   ```

2. **Case-insensitive search:**
   ```sql
   -- ✅ MODIFICARE 3: Search global CASE-INSENSITIVE cu COLLATE
   AND (
       @SearchText IS NULL OR
       pac.Nume COLLATE Latin1_General_CI_AI LIKE '%' + @SearchText COLLATE Latin1_General_CI_AI + '%' OR
       pac.Prenume COLLATE Latin1_General_CI_AI LIKE '%' + @SearchText COLLATE Latin1_General_CI_AI + '%' OR
       pac.CNP LIKE '%' + @SearchText + '%' OR
       doc.Nume COLLATE Latin1_General_CI_AI LIKE '%' + @SearchText COLLATE Latin1_General_CI_AI + '%' OR
       doc.Prenume COLLATE Latin1_General_CI_AI LIKE '%' + @SearchText COLLATE Latin1_General_CI_AI + '%' OR
   p.Observatii COLLATE Latin1_General_CI_AI LIKE '%' + @SearchText COLLATE Latin1_General_CI_AI + '%'
   )
   ```

**Version:** Updated from 1.0 to **1.1**

---

### 4. **sp_Programari_GetCount.sql** (SQL Logic)
**Path:** `DevSupport\Database\StoredProcedures\Programari\sp_Programari_GetCount.sql`

**Same changes** as `sp_Programari_GetAll.sql` to ensure consistent behavior for pagination.

**Version:** Updated from 1.0 to **1.1**

---

## 🚀 How to Apply Changes

### Step 1: Run SQL Scripts

Execute the updated stored procedures in SSMS or Azure Data Studio:

```powershell
# Option 1: SQLCMD
sqlcmd -S TS1828\ERP -d ValyanMed -i "DevSupport\Database\StoredProcedures\Programari\sp_Programari_GetAll.sql"
sqlcmd -S TS1828\ERP -d ValyanMed -i "DevSupport\Database\StoredProcedures\Programari\sp_Programari_GetCount.sql"

# Option 2: In SSMS
# 1. Open sp_Programari_GetAll.sql
# 2. Execute (F5)
# 3. Open sp_Programari_GetCount.sql
# 4. Execute (F5)
```

**Expected output:**
```
✅ sp_Programari_GetAll creat cu succes (v1.1 - case-insensitive search + prima zi luna curenta)
✅ sp_Programari_GetCount creat cu succes (v1.1 - case-insensitive search + prima zi luna curenta)
```

---

### Step 2: Rebuild Application

The C# code changes are already compiled successfully:

```powershell
cd ValyanClinic
dotnet build
```

**Expected output:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

✅ **Build Status:** SUCCESS

---

### Step 3: Restart Application

```powershell
# Stop the application if running
# Then start it again
dotnet run --project ValyanClinic
```

---

### Step 4: Test

1. **Test Default Date Range:**
   - Navigate to `/programari/lista`
   - **Verify:** Date filters should default to **first day** and **last day** of current month
   - Example: If today is **January 15, 2025**, defaults should be:
     - FilterDataStart: **January 1, 2025**
     - FilterDataEnd: **January 31, 2025**
   - **Verify in UI:** The date inputs should show these values when page loads

2. **Test Clear Filters:**
   - Apply some filters (e.g., select a doctor, change dates)
   - Click **"Șterge Filtre"** button
   - **Verify:** Dates reset to **first day** and **last day** of current month (not today + 30 days)

3. **Test Case-Insensitive Search:**
   ```
   Test Case 1: Search "popescu"
   Expected: Finds "Popescu", "POPESCU", "PopEscu"
   
   Test Case 2: Search "ION"
   Expected: Finds "Ion", "ion", "ION"
   
   Test Case 3: Search "medic"
   Expected: Finds "Medic", "MEDIC", "medic" in Observatii
   ```

---

## 🧪 Testing Scenarios

### Scenario 1: Default Behavior (Page Load)
```csharp
// When user navigates to /programari/lista
// Expected:
// - FilterDataStart: First day of current month (2025-01-01)
// - FilterDataEnd: Last day of current month (2025-01-31)
// - Shows ALL appointments for January 2025
```

**Visual Check:**
```html
<!-- Date inputs should show: -->
<input type="date" value="2025-01-01" />  <!-- De la data -->
<input type="date" value="2025-01-31" />  <!-- Până la data -->
```

### Scenario 2: Clear All Filters
```csharp
// User applies filters → clicks "Șterge Filtre"
// Expected:
// - Dates reset to first/last day of current month
// - NOT reset to today + 30 days
```

### Scenario 3: Case-Insensitive Search
```csharp
// User searches "popescu" (lowercase)
var query = new GetProgramareListQuery
{
    GlobalSearchText = "popescu"
};

// Expected Result:
// - Finds pacients named: "Popescu", "POPESCU", "PopEscu", "popescu"
```

### Scenario 4: Custom Date Range (Still Supported)
```csharp
// User manually changes dates in UI
// Expected:
// - Uses custom dates (overrides defaults)
// - Works as before
```

---

## ⚙️ Technical Details

### Why Update Both C# and SQL?

**Architecture:**
```
[Blazor Component (ListaProgramari.razor.cs)]
   ↓ sends FilterDataStart, FilterDataEnd
[MediatR Query (GetProgramareListQuery)]
         ↓
[Repository (ProgramareRepository)]
      ↓ passes parameters
[SQL Stored Procedure (sp_Programari_GetAll)]
         ↓ applies defaults if NULL
[Database]
```

**Two levels of defaults:**
1. **C# Component Level:** Sets initial values when page loads
2. **SQL Procedure Level:** Applies defaults if NULL received (fallback)

**Why both?**
- **C# Component:** Provides immediate UI feedback (date pickers show correct values)
- **SQL Procedure:** Safety net if NULL values are passed (e.g., from API calls)

### DateTime Calculations in C#

```csharp
var today = DateTime.Today;

// First day of month
var firstDay = new DateTime(today.Year, today.Month, 1);
// Example: 2025-01-01

// Last day of month
var lastDay = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
// Example: 2025-01-31 (handles February correctly: 28 or 29 days)
```

### Why `DATEFROMPARTS` and `EOMONTH` in SQL?

- **`DATEFROMPARTS(YEAR, MONTH, DAY)`** = Constructs exact date (January 1, 2025)
- **`EOMONTH(date)`** = Returns last day of month (January 31, 2025)
- Both are more reliable than date arithmetic and avoid timezone issues

### Performance Impact

**Minimal to None:**
- C# DateTime calculations happen once per component initialization (< 1ms)
- SQL date functions are computed once per query execution (microseconds)
- Indexes on columns are still used efficiently

---

## 📊 Before vs. After Comparison

| Aspect | Before ❌ | After ✅ |
|--------|-----------|---------|
| **Default Start Date (C#)** | Today (azi) | First day of current month |
| **Default End Date (C#)** | Today + 30 days | Last day of current month |
| **Default Start Date (SQL)** | Today (azi) | First day of current month |
| **Default End Date (SQL)** | Today + 30 days | Last day of current month |
| **Clear Filters Reset** | Today + 30 days | First/last day of current month |
| **Search "popescu"** | May not find "Popescu" | Finds "Popescu", "POPESCU", "PopEscu" |
| **Search "ION"** | May not find "Ion" | Finds "Ion", "ion", "ION" |
| **User Experience** | Must adjust dates manually | Sees entire month by default |
| **Consistency** | C# and SQL mismatch | C# and SQL aligned |

---

## ✅ Checklist

- [x] Updated `GetProgramareListQuery.cs` (documentation comments)
- [x] Updated `ListaProgramari.razor.cs` (OnInitializedAsync + ClearAllFilters) ⭐ **NEW!**
- [x] Updated `sp_Programari_GetAll.sql` (default dates + case-insensitive)
- [x] Updated `sp_Programari_GetCount.sql` (default dates + case-insensitive)
- [x] Created this documentation
- [x] Verified build success ✅
- [ ] **TODO:** Run SQL scripts in database
- [ ] **TODO:** Restart Blazor application
- [ ] **TODO:** Test default date behavior in UI
- [ ] **TODO:** Test Clear Filters button
- [ ] **TODO:** Test case-insensitive search
- [ ] **TODO:** Deploy to production

---

## 🔮 Future Enhancements (Optional)

1. **Quick Date Filters** (UI Buttons):
   - "Azi" (Today)
   - "Săptămâna curentă" (This week)
   - "Luna curentă" (This month) ← **Now the default!**
   - "Următoarele 7 zile" (Next 7 days)
   - "Luna viitoare" (Next month)

2. **Search Highlighting** (UI):
   - Highlight matched search terms in results

3. **Fuzzy Search** (Advanced):
   - Allow typos: "popesku" → finds "Popescu"
   - Requires full-text search or external library

4. **Date Range Presets** (Dropdown):
 ```html
   <select>
   <option>Luna curentă</option>
     <option>Luna trecută</option>
     <option>Următoarele 30 zile</option>
     <option>Custom...</option>
   </select>
   ```

---

## 📞 Support

**For questions about this change:**
- Developer: GitHub Copilot
- Date: 2025-01-XX
- Module: Programare Management
- Priority: ⭐⭐ Medium (UX improvement)

**Files Changed:**
- **C# (Blazor):** `ListaProgramari.razor.cs` ⭐ **UPDATED!**
- **C# (Application):** `GetProgramareListQuery.cs`
- **SQL:** `sp_Programari_GetAll.sql`
- **SQL:** `sp_Programari_GetCount.sql`

---

*Changes implemented successfully! ✅ Users can now see the entire month by default (both in UI and SQL), and search is reliably case-insensitive.*

**Version:** 1.1  
**Last Updated:** 2025-01-XX  
**Status:** ✅ Complete  
**Build:** ✅ Success
