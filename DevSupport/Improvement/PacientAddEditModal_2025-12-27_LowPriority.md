# PacientAddEditModal - Low Priority Improvements

> **Data:** 27 Decembrie 2025  
> **Component:** `ValyanClinic/Components/Pages/Pacienti/Modals/PacientAddEditModal.razor`  
> **Status:** Backlog - Low Priority

---

## 📋 Lista de Îmbunătățiri Low Priority

### 1. 🎨 Inline Styles → CSS Classes

**Descriere:** Tab-ul Doctori conține ~50 inline styles care ar trebui mutate în fișierul `.razor.css` pentru o mai bună mentenanță.

**Locații afectate:**
- Secțiunea "Doctori Activi" - `.doctor-card` styles
- Secțiunea "Istoric Relații Inactive" - `.doctor-card-inactive` styles
- Butoanele de acțiune (Dezactivează, Reactivează, Contactează)
- Badge-urile pentru tip relație

**Exemplu de refactorizare:**
```css
/* În PacientAddEditModal.razor.css */
.doctor-card {
    background: white;
    border: 2px solid #e5e7eb;
    border-radius: 12px;
    padding: 1.25rem;
    margin-bottom: 1rem;
}

.doctor-card-inactive {
    background: #f9fafb;
    border: 2px solid #e5e7eb;
    border-radius: 12px;
    padding: 1.25rem;
    margin-bottom: 1rem;
}

.btn-deactivate {
    padding: 0.5rem 1rem;
    background: linear-gradient(135deg, #fca5a5, #ef4444);
    color: white;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    box-shadow: 0 2px 6px rgba(239, 68, 68, 0.3);
}

.btn-reactivate {
    padding: 0.5rem 1rem;
    background: linear-gradient(135deg, #10b981, #059669);
    color: white;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    box-shadow: 0 2px 6px rgba(16, 185, 129, 0.3);
}
```

**Efort estimat:** 2-3 ore

---

### 2. 💬 Tooltip-uri pentru Câmpuri Complexe

**Descriere:** Adaugă tooltip-uri informative pentru câmpurile care necesită explicații suplimentare.

**Câmpuri țintă:**
- **CNP** - "Codul Numeric Personal format din 13 cifre. Prima cifră indică sexul (1/2 = M/F născut 1900-1999, 5/6 = M/F născut 2000+)"
- **Nr. Card Sănătate** - "Numărul de pe cardul european de asigurări de sănătate"
- **CNP Asigurat** - "Poate fi diferit de CNP pacient (ex: copii asigurați prin părinți)"
- **Cod Pacient** - "Cod unic generat automat de sistem"

**Implementare sugerată:**
```razor
<SfTooltip Content="Codul Numeric Personal - 13 cifre" Position="Position.Top">
    <SfTextBox ID="cnp" @bind-Value="FormModel.CNP" ... />
</SfTooltip>
```

**Efort estimat:** 1-2 ore

---

### 3. 🖼️ Avatar Preview cu Inițiale

**Descriere:** Afișează un avatar circular cu inițialele pacientului în header-ul modalului când editezi.

**Design propus:**
```
┌─────────────────────────────────────────┐
│ [IP] Editare Pacient - Ion Popescu      │
│ ^^^^                                    │
│ Avatar cu inițiale                      │
└─────────────────────────────────────────┘
```

**Implementare sugerată:**
```razor
<div class="modal-header">
    <div class="modal-title">
        @if (IsEditMode && !string.IsNullOrEmpty(FormModel.Nume))
        {
            <div class="patient-avatar">
                @GetInitials(FormModel.Nume, FormModel.Prenume)
            </div>
        }
        <i class="fas fa-@(IsEditMode ? "user-edit" : "user-plus")"></i>
        <h2>@(IsEditMode ? $"Editare: {FormModel.Nume} {FormModel.Prenume}" : "Adăugare Pacient Nou")</h2>
    </div>
    ...
</div>
```

**CSS pentru avatar:**
```css
.patient-avatar {
    width: 40px;
    height: 40px;
    border-radius: 50%;
    background: linear-gradient(135deg, #60a5fa, #3b82f6);
    color: white;
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: 600;
    font-size: 14px;
    margin-right: 12px;
}
```

**Efort estimat:** 1 oră

---

### 4. 📝 Audit Trail pentru Modificări

**Descriere:** La editare, logează automat ce câmpuri s-au modificat pentru audit.

**Implementare sugerată:**
```csharp
private Dictionary<string, (string OldValue, string NewValue)> GetChangedFields(PacientFormModel original, PacientFormModel current)
{
    var changes = new Dictionary<string, (string, string)>();
    
    if (original.Nume != current.Nume)
        changes["Nume"] = (original.Nume, current.Nume);
    if (original.Telefon != current.Telefon)
        changes["Telefon"] = (original.Telefon ?? "", current.Telefon ?? "");
    // ... alte câmpuri
    
    return changes;
}

private async Task LogAuditTrail(Guid pacientId, Dictionary<string, (string, string)> changes)
{
    foreach (var change in changes)
    {
        Logger.LogInformation(
            "[AUDIT] Pacient {PacientId}: {Field} changed from '{Old}' to '{New}'",
            pacientId, change.Key, change.Value.OldValue, change.Value.NewValue);
    }
}
```

**Integrare cu AuditLogRepository:**
- Folosește `IAuditLogRepository` existent pentru persistență
- Creează entry-uri de tip "PacientModified" cu detalii JSON

**Efort estimat:** 3-4 ore

---

### 5. ⚡ Optimistic UI Update

**Descriere:** Afișează modificările imediat în UI și revert dacă salvarea eșuează.

**Flux propus:**
1. User apasă "Salvează"
2. UI se actualizează instant (modal se închide, grid se refreshează)
3. Request-ul merge către server în background
4. Dacă eșuează → Toast error + reopen modal cu datele
5. Dacă reușește → Toast success

**Implementare sugerată:**
```csharp
private async Task HandleOptimisticSubmit()
{
    // 1. Salvează starea curentă pentru rollback
    var backupModel = FormModel.Clone();
    
    // 2. Închide modalul optimist
    await IsVisibleChanged.InvokeAsync(false);
    await OnSaved.InvokeAsync();
    
    // 3. Trimite request în background
    try
    {
        var result = await SavePacientAsync();
        if (!result.IsSuccess)
        {
            // Rollback
            FormModel = backupModel;
            await IsVisibleChanged.InvokeAsync(true);
            await NotificationService.ShowError("Salvare eșuată: " + result.FirstError);
        }
    }
    catch (Exception ex)
    {
        // Rollback
        FormModel = backupModel;
        await IsVisibleChanged.InvokeAsync(true);
        await NotificationService.ShowError("Eroare: " + ex.Message);
    }
}
```

**Efort estimat:** 2-3 ore

---

### 6. 🧪 Unit Tests pentru FormModel

**Descriere:** Adaugă teste unitare pentru validarea și logica FormModel.

**Teste sugerate:**
```csharp
// PacientFormModelTests.cs
public class PacientFormModelTests
{
    [Theory]
    [InlineData("1900101123456", true)]  // Valid male CNP
    [InlineData("2900101123456", true)]  // Valid female CNP
    [InlineData("123", false)]            // Too short
    [InlineData("1234567890123", false)]  // Invalid checksum
    public void CNP_Validation_ShouldWork(string cnp, bool expectedValid)
    {
        var model = new PacientFormModel { CNP = cnp };
        var isValid = CNPValidator.IsValid(cnp);
        Assert.Equal(expectedValid, isValid);
    }
    
    [Fact]
    public void ParseCNP_ShouldExtractBirthDateAndSex()
    {
        var model = new PacientFormModel { CNP = "1900101123456" };
        model.ParseCNP();
        
        Assert.Equal(new DateTime(1990, 1, 1), model.Data_Nasterii);
        Assert.Equal("M", model.Sex);
    }
    
    [Fact]
    public void Clone_ShouldCreateDeepCopy()
    {
        var original = new PacientFormModel { Nume = "Test", Telefon = "0721123456" };
        var clone = original.Clone();
        
        clone.Nume = "Modified";
        
        Assert.Equal("Test", original.Nume);
        Assert.Equal("Modified", clone.Nume);
    }
}
```

**Locație:** `ValyanClinic.Tests/Components/Pacienti/`

**Efort estimat:** 2-3 ore

---

## 📊 Sumar Efort Total

| Îmbunătățire | Efort Estimat |
|--------------|---------------|
| Inline Styles → CSS | 2-3 ore |
| Tooltip-uri | 1-2 ore |
| Avatar Preview | 1 oră |
| Audit Trail | 3-4 ore |
| Optimistic UI | 2-3 ore |
| Unit Tests | 2-3 ore |
| **TOTAL** | **11-16 ore** |

---

## 🔗 Referințe

- [PacientAddEditModal.razor](../../ValyanClinic/Components/Pages/Pacienti/Modals/PacientAddEditModal.razor)
- [PacientAddEditModal.razor.cs](../../ValyanClinic/Components/Pages/Pacienti/Modals/PacientAddEditModal.razor.cs)
- [PacientAddEditModal.razor.css](../../ValyanClinic/Components/Pages/Pacienti/Modals/PacientAddEditModal.razor.css)
- [Testing Guide](../Testing/TESTING_GUIDE.md)
