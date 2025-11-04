# 🐛 BugFix: Tip Departament nu se afișa în View Modal

## 📅 Data: 2025-10-18

---

## ❌ Problema

În **DepartamentViewModal**, câmpul "Tip Departament" se afișa ca **"Nu este specificat"** chiar dacă în grid-ul principal se vedea valoarea corectă (ex: "Medical").

### Screenshot Problema

**Grid (corect):**
```
Denumire: Pneumologie
Tip: Medical ✓
```

**View Modal (incorect):**
```
Tip Departament: 🚫 Nu este specificat
```

---

## 🔍 Cauza Rădăcină

Problema era în **`DepartamentRepository.GetByIdAsync()`**:

### ❌ Cod Incorect (ÎNAINTE)
```csharp
public async Task<Departament?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
{
    var parameters = new { IdDepartament = id };
    return await QueryFirstOrDefaultAsync<Departament>("sp_Departamente_GetById", parameters, cancellationToken);
}
```

**Problema:** 
- Mapare directă la entitatea `Departament`
- Dapper nu poate popula proprietatea de navigare `TipDepartament`
- `DenumireTipDepartament` returnat de SP era ignorat

---

## ✅ Soluția

Am schimbat `GetByIdAsync()` să folosească același pattern ca `GetAllAsync()` - mapare prin DTO intermediar.

### ✅ Cod Corect (DUPĂ)
```csharp
public async Task<Departament?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
{
    var parameters = new { IdDepartament = id };
    var dto = await QueryFirstOrDefaultAsync<DepartamentDto>("sp_Departamente_GetById", parameters, cancellationToken);
    
    if (dto == null)
        return null;
    
    return new Departament
    {
        IdDepartament = dto.IdDepartament,
        IdTipDepartament = dto.IdTipDepartament,
        DenumireDepartament = dto.DenumireDepartament,
        DescriereDepartament = dto.DescriereDepartament,
        TipDepartament = dto.DenumireTipDepartament != null ? new TipDepartament 
        { 
            IdTipDepartament = dto.IdTipDepartament ?? Guid.Empty,
            DenumireTipDepartament = dto.DenumireTipDepartament 
        } : null
    };
}
```

**Beneficii:**
- ✅ Mapare explicită prin `DepartamentDto`
- ✅ `DenumireTipDepartament` din SP este captat
- ✅ Proprietate `TipDepartament` populată corect
- ✅ Consistent cu `GetAllAsync()`

---

## 🔧 Detalii Tehnice

### Stored Procedure (corect, nu a fost modificat)

```sql
CREATE PROCEDURE dbo.sp_Departamente_GetById
    @IdDepartament UNIQUEIDENTIFIER
AS
BEGIN
    SELECT 
        d.IdDepartament,
        d.IdTipDepartament,
        d.DenumireDepartament,
        d.DescriereDepartament,
        td.DenumireTipDepartament  -- ✓ Este returnat
    FROM Departamente d
    LEFT JOIN TipDepartament td ON d.IdTipDepartament = td.IdTipDepartament
    WHERE d.IdDepartament = @IdDepartament;
END
```

**Observație:** SP-ul era corect! Făcea LEFT JOIN și returna `DenumireTipDepartament`.

### DepartamentDto (existent, nu a fost modificat)

```csharp
private class DepartamentDto
{
    public Guid IdDepartament { get; set; }
    public Guid? IdTipDepartament { get; set; }
    public string DenumireDepartament { get; set; } = string.Empty;
    public string? DescriereDepartament { get; set; }
    public string? DenumireTipDepartament { get; set; }  // ✓ Definit
}
```

### Flow-ul Datelor

#### ❌ ÎNAINTE (broken)
```
SP: sp_Departamente_GetById
  → Returns: DenumireTipDepartament = "Medical"
    → Dapper maps direct to Departament entity
      → TipDepartament property = NULL (nu poate fi populat)
        → Handler: departament.TipDepartament?.DenumireTipDepartament = null
          → DTO: DenumireTipDepartament = null
            → View Modal: "Nu este specificat" ❌
```

#### ✅ DUPĂ (fixed)
```
SP: sp_Departamente_GetById
  → Returns: DenumireTipDepartament = "Medical"
    → Dapper maps to DepartamentDto
      → dto.DenumireTipDepartament = "Medical" ✓
        → Manual mapping to Departament
          → TipDepartament = new TipDepartament { DenumireTipDepartament = "Medical" }
            → Handler: departament.TipDepartament?.DenumireTipDepartament = "Medical"
              → DTO: DenumireTipDepartament = "Medical"
                → View Modal: Badge cu "Medical" ✓
```

---

## 🧪 Testing

### Test Case 1: Departament cu Tip
```
Given: Departament "Pneumologie" cu Tip = "Medical"
When: Click "Vizualizeaza"
Then: Modal afișează badge "Medical" ✓
```

### Test Case 2: Departament fără Tip
```
Given: Departament "Administrare" cu Tip = NULL
When: Click "Vizualizeaza"
Then: Modal afișează "Nu este specificat" ✓
```

### Test Case 3: Grid vs Modal
```
Given: Orice departament
When: Vezi în grid și în modal
Then: Tip Departament identic în ambele locuri ✓
```

---

## 📊 Impact

### Files Modified
- ✅ `DepartamentRepository.cs` - Metoda `GetByIdAsync()` (1 metodă)

### Files NOT Modified
- ✓ `sp_Departamente_GetById.sql` - Era deja corect
- ✓ `DepartamentDto` - Era deja definit corect
- ✓ `GetDepartamentByIdQueryHandler.cs` - Era corect
- ✓ `DepartamentViewModal.razor` - Era corect

**Total modificări: 1 fișier, 1 metodă**

---

## ✅ Verification

### Build Status
```
Build successful
Zero errors
Zero warnings
```

### Test Results
- ✅ Grid afișează "Medical" pentru Pneumologie
- ✅ View Modal afișează badge "Medical" pentru Pneumologie
- ✅ Departamente fără tip afișează "Nu este specificat" (correct empty state)
- ✅ GetAllAsync (grid) și GetByIdAsync (view) returnează aceleași date

---

## 🎓 Lecții Învățate

### ⚠️ Pitfall: Direct Entity Mapping cu Dapper
```csharp
// ❌ BAD: Direct mapping nu populează navigation properties
return await QueryFirstOrDefaultAsync<Departament>("sp_...", params);

// ✅ GOOD: DTO intermediar capturează toate coloanele din SP
var dto = await QueryFirstOrDefaultAsync<DepartamentDto>("sp_...", params);
return MapToEntity(dto);
```

### ✅ Best Practice: Consistency
```csharp
// Dacă GetAllAsync folosește DTO intermediar,
// atunci și GetByIdAsync ar trebui să folosească același pattern!
```

### 🔍 Debugging Tip
```
Dacă vezi discrepanță între grid și modal:
1. Verifică SP-ul (returnează coloana?)
2. Verifică DTO-ul (are proprietatea?)
3. Verifică mapping-ul (populează corect?)
```

---

## 📝 Pattern Recomandat

Pentru metode repository care încarcă entități cu relații:

```csharp
// 1. Definește DTO care match-ește exact coloanele din SP
private class EntityDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid? RelatedId { get; set; }
    public string? RelatedName { get; set; }  // ← Din JOIN
}

// 2. Query la DTO
var dto = await QueryFirstOrDefaultAsync<EntityDto>("sp_...", params);

// 3. Map manual la Entity cu navigation properties
return new Entity
{
    Id = dto.Id,
    Name = dto.Name,
    RelatedEntity = dto.RelatedName != null 
        ? new RelatedEntity { Id = dto.RelatedId!.Value, Name = dto.RelatedName }
        : null
};
```

---

## 🎉 Status Final

✅ **Bug FIXED!**

- ✅ Tip Departament se afișează corect în View Modal
- ✅ Consistent între Grid și Modal
- ✅ Empty states funcționează corect
- ✅ Build successful
- ✅ Zero side effects

---

*Bug identificat și rezolvat: 2025-10-18*  
*Timp de rezolvare: ~10 minute*  
*Root cause: Incorrect Dapper mapping strategy*
